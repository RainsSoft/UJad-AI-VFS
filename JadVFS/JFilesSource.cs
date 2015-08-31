#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

#endregion

namespace JadEngine.VFS
{

    /// <summary>
    /// Represents a place where files are located. The <see cref="JVFS"/> 
    /// uses this objects to locate files.
    /// </summary>
    /// <example>
    /// Examples of file sources are:
    /// - hard disk paths (all files and directories in c:\mytextures).
    /// - storage files (files and directories inside the storage "c:\resources\mystorage.jsf").
    /// - ...
    /// </example>
    /// <remarks>
    /// Note to implementers: all methods from this class should allow only read access to the streams they return.
    /// To allow write access inherit from <see cref="JWritableSource"/> instead.
    /// </remarks>
    public abstract class JFilesSource
    {
        #region Fields

        /// <summary>
        /// Name of the files source
        /// </summary>
        protected String _name;

        /// <summary>
        /// Path of the source
        /// </summary>
        protected String _path;

        /// <summary>
        /// Collection of defined directories of the source
        /// </summary>
        protected Dictionary<String, String> _definedPaths;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the files source
        /// </summary>
        public virtual String Name {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets the path of the source
        /// </summary>
        public virtual String Path {
            get { return _path; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        protected JFilesSource() {
            _definedPaths = new Dictionary<string, string>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new defined path to the <see cref="JFilesSource"/>.
        /// </summary>
        /// <param name="name">Name of the defined path.</param>
        /// <param name="path">Real path of the defined path.</param>
        public virtual void AddDefinedPath(string name, string path) {
            if (path.Contains(".."))
                throw new IOException("The path can't contain the \"..\" modifier.");

            _definedPaths.Add(name, path);
        }

        /// <summary>
        /// Gets a stream to a file.
        /// </summary>
        /// <param name="path">Relative path of the file.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="recurse">If the search should be recursive or not.</param>
        /// <returns>A stream to the file.</returns>
        public abstract Stream GetFile(string path, string fileName, bool recurse);

        /// <summary>
        /// Gets a stream to a file.
        /// </summary>
        /// <param name="qualifiedName">Relative path and name of the file.</param>
        /// <returns>A stream to the file.</returns>
        /// <remarks>This search is never recursive.</remarks>
        public abstract Stream GetFile(string qualifiedName);

        /// <summary>
        /// Gets a stream to a file.
        /// </summary>
        /// <param name="definedPath">A defined path where to search the file.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="recurse">If the search should be recursive or not.</param>
        /// <returns>A stream to the file.</returns>
        public abstract Stream GetFileFromDefinedPath(string definedPath, string fileName, bool recurse);

        /// <summary>
        /// Finds a file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>A stream to the file.</returns>
        /// <remarks>This search is always recursive.</remarks>
        public abstract Stream FindFile(string fileName);

        /// <summary>
        /// Gets the collection of files on a directory.
        /// </summary>
        /// <param name="path">Path of the directory</param>
        /// <param name="recurse">If the search should be recursive (include subdirectories) or not.</param>
        /// <param name="searchPattern">Mask to filter the files.</param>
        /// <returns>The collection of files of the directory.</returns>
        /// <remarks>
        /// Note to implementers: the file names should be correct names for the method "Stream GetFile(string qualifiedName)".
        /// </remarks>
        public abstract Collection<string> GetFiles(string path, bool recurse, string searchPattern);

        /// <summary>
        /// Gets the collection of files on a defined path.
        /// </summary>
        /// <param name="definedPath">A defined path where to search the file.</param>
        /// <param name="recurse">If the search should be recursive (include subdirectories) or not.</param>
        /// <param name="searchPattern">Mask to filter the files.</param>
        /// <returns>The collection of files of the directory.</returns>
        /// <remarks>
        /// Note to implementers: the file names should be correct names for the method "Stream GetFile(string qualifiedName)".
        /// </remarks>
        public abstract Collection<string> GetFilesFromDefinedPath(string definedPath, bool recurse, string searchPattern);

        #endregion
    }
    class SearchPattern
    {

        public SearchPattern(string pattern) : this(pattern, false) { }
        public SearchPattern(string pattern, bool ignore) {
            this.ignore = ignore;
            Compile(pattern);
        }
        public bool IsMatch(string text) {
            return Match(ops, text, 0);
        }
        // private
        private Op ops;		// the compiled pattern
        private bool ignore;	// ignore case
        private void Compile(string pattern) {
            if (pattern == null || pattern.IndexOfAny(InvalidChars) >= 0)
                throw new ArgumentException("Invalid search pattern.");
            if (pattern == "*") {	// common case
                ops = new Op(OpCode.True);
                return;
            }
            ops = null;
            int ptr = 0;
            Op last_op = null;
            while (ptr < pattern.Length) {
                Op op;

                switch (pattern[ptr]) {
                    case '?':
                        op = new Op(OpCode.AnyChar);
                        ++ptr;
                        break;
                    case '*':
                        op = new Op(OpCode.AnyString);
                        ++ptr;
                        break;

                    default:
                        op = new Op(OpCode.ExactString);
                        int end = pattern.IndexOfAny(WildcardChars, ptr);
                        if (end < 0)
                            end = pattern.Length;
                        op.Argument = pattern.Substring(ptr, end - ptr);
                        if (ignore)
                            op.Argument = op.Argument.ToLowerInvariant();
                        ptr = end;
                        break;
                }
                if (last_op == null)
                    ops = op;
                else
                    last_op.Next = op;
                last_op = op;
            }
            if (last_op == null)
                ops = new Op(OpCode.End);
            else
                last_op.Next = new Op(OpCode.End);
        }
        private bool Match(Op op, string text, int ptr) {
            while (op != null) {
                switch (op.Code) {
                    case OpCode.True:
                        return true;
                    case OpCode.End:
                        if (ptr == text.Length)
                            return true;
                        return false;

                    case OpCode.ExactString:
                        int length = op.Argument.Length;
                        if (ptr + length > text.Length)
                            return false;
                        string str = text.Substring(ptr, length);
                        if (ignore)
                            str = str.ToLowerInvariant();
                        if (str != op.Argument)
                            return false;
                        ptr += length;
                        break;
                    case OpCode.AnyChar:
                        if (++ptr > text.Length)
                            return false;
                        break;
                    case OpCode.AnyString:
                        while (ptr <= text.Length) {
                            if (Match(op.Next, text, ptr))
                                return true;
                            ++ptr;
                        }
                        return false;
                }
                op = op.Next;
            }
            return true;
        }
        // private static

        internal static readonly char[] WildcardChars = { '*', '?' };

        internal static readonly char[] InvalidChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        private class Op
        {
            public Op(OpCode code) {
                this.Code = code;
                this.Argument = null;
                this.Next = null;
            }

            public OpCode Code;
            public string Argument;
            public Op Next;
        }
        private enum OpCode
        {
            ExactString,		// literal
            AnyChar,		// ?
            AnyString,		// *
            End,			// end of pattern
            True			// always succeeds
        };

    }
}
