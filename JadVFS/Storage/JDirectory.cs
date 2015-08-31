#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

#endregion

namespace JadEngine.VFS.Storage
{
	/// <summary>
	/// Represents a directory inside a <see cref="JStorageSource"/>.
	/// </summary>
	public class JDirectory
	{
		#region Fields

		/// <summary>
		/// Name of the directory
		/// </summary>
		private String _name;

		/// <summary>
		/// Parent directory
		/// </summary>
		private JDirectory _parent;

		/// <summary>
		/// Directories inside this directory
		/// </summary>
		private Dictionary<JFileName, JDirectory> _directories;

		/// <summary>
		/// Files inside this directory
		/// </summary>
		private Dictionary<JFileName, JFile> _files;

		/// <summary>
		/// Array of valid wildcards
		/// </summary>
		private char[] Wildcards = new char[] { '*', '?' };

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the directory
		/// </summary>
		public String Name
		{
			get { return _name; }
		}

		/// <summary>
		/// Gets or sets the parent directory
		/// </summary>
		public JDirectory Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor that assigns the directory name
		/// </summary>
		/// <param name="name">Name of the directory</param>
		public JDirectory(String name)
		{
			_name = name;

			_directories = new Dictionary<JFileName, JDirectory>();
			_files = new Dictionary<JFileName, JFile>();
		}

		/// <summary>
		/// Constructor that assigns the directory name and parent
		/// </summary>
		/// <param name="name">Name of the directory</param>
		/// <param name="parent">Parent of the directory</param>
		public JDirectory(String name, JDirectory parent)
			: this(name)
		{
			_parent = parent;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds a directory to this directory
		/// </summary>
		/// <param name="directory">Directory to add</param>
		public void AddDirectory(JDirectory directory)
		{
			_directories.Add(new JFileName(directory.Name), directory);
			directory.Parent = this;
		}

		/// <summary>
		/// Adds a directory to a directory inside a path that starts in this
		/// directory
		/// </summary>
		/// <param name="path">Path to add the directory</param>
		/// <param name="directory">Directory to add</param>
		public void AddDirectory(String path, JDirectory directory)
		{
			JDirectory targetDirectory;

			targetDirectory = GetDirectory(path);
			if (targetDirectory == null)
				throw new IOException("The path " + path + " is not correct in the JDirectory " + _name + ".");

			targetDirectory.AddDirectory(directory);
		}

		/// <summary>
		/// Adds a file to this directory
		/// </summary>
		/// <param name="file">File to add</param>
		public void AddFile(JFile file)
		{
			_files.Add(new JFileName(file.Name), file);
			file.Parent = this;
		}

		/// <summary>
		/// Adds a file to a directory inside a path that starts in this
		/// directory
		/// </summary>
		/// <param name="path">Path to add the file</param>
		/// <param name="file">File to add</param>
		public void AddFile(String path, JFile file)
		{

			JDirectory directory;

			directory = GetDirectory(path);
			if (directory == null)
				throw new IOException("The path " + path + " is not correct in the JDirectory " + _name + ".");

			directory.AddFile(file);
		}

		/// <summary>
		/// Gets a <see cref="JDirectory"/> using its path.
		/// </summary>
		/// <param name="path">Path to the directory.</param>
		/// <returns>The <see cref="JDirectory"/> searched.</returns>
		public JDirectory GetDirectory(string path)
		{
			String[] splitPath;
			JDirectory current;

			// Split the path
			splitPath = path.Split(new char[2] { Path.DirectorySeparatorChar, "/".ToCharArray()[0] }, StringSplitOptions.RemoveEmptyEntries);

			// Find the directory
			current = this;
			for (int i = 0; i < splitPath.Length; i++)
			{
				current = current.GetPrivateDirectory(splitPath[i]);
				if (current == null)
					return null;
			}

			return current;
		}

		/// <summary>
		/// Gets a <see cref="JFile"/> using its qualified name.
		/// </summary>
		/// <param name="qualifiedName">Qualified name of the file.</param>
		/// <param name="options">Search options.</param>
		/// <returns>The <see cref="JFile"/> searched.</returns>
		public JFile GetFile(string qualifiedName, SearchOption options)
		{
			JDirectory directory;

			directory = GetDirectory(System.IO.Path.GetDirectoryName(qualifiedName));
			if (directory == null)
				return null;

			return directory.GetPrivateFile(System.IO.Path.GetFileName(qualifiedName), options);
		}

		/// <summary>
		/// Gets a file using its path and name.
		/// </summary>
		/// <param name="path">Path of the file.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="options">Search options.</param>
		/// <returns></returns>
		public JFile GetFile(string path, string fileName, SearchOption options)
		{
			JDirectory directory;

			directory = GetDirectory(path);
			if (directory == null)
				return null;

			return directory.GetFile(fileName, options);
		}

		/// <summary>
		/// Gets the list of filenames from a <see cref="JDirectory"/>.
		/// </summary>
		/// <param name="path">Path to the JDirectory.</param>
		/// <param name="searchPattern">Search pattern to match in the filenames.</param>
		/// <param name="searchOption">Search options.</param>
		/// <returns>The collection of qualified file names in the JDirectory.</returns>
		public Collection<string> GetFileNames(string path, string searchPattern, SearchOption searchOption)
		{
			Collection<string> fileNames;
			JDirectory directory;

			fileNames = new Collection<string>();

			directory = GetDirectory(path);
			if (directory == null)
				return fileNames;

			if (searchOption == SearchOption.TopDirectoryOnly)
				foreach (JFile file in directory.GetFiles())
					if (Match(searchPattern, file.Name, false))
						fileNames.Add(file.GetQualifiedName());

			if (searchOption == SearchOption.AllDirectories)
				GetPrivateFileNames(directory, searchPattern, fileNames);

			return fileNames;
		}

		/// <summary>
		/// Gets a list of all the directories in the directory
		/// </summary>
		/// <returns>The list of directories</returns>
		public List<JDirectory> GetDirectories()
		{
			return new List<JDirectory>(_directories.Values);
		}

		/// <summary>
		/// Gets a list of all the files in the directory
		/// </summary>
		/// <returns>The list of files</returns>
		public List<JFile> GetFiles()
		{
			return new List<JFile>(_files.Values);
		}

		/// <summary>
		/// Gets the total number of directories in this directory and all subdirectories
		/// </summary>
		/// <returns>The total number of directories and subdirectories</returns>
		public int GetTotalNumberOfDirectories()
		{
			int value;

			value = 0;
			foreach (JDirectory directory in _directories.Values)
				value += directory.GetTotalNumberOfDirectories();

			value += _directories.Values.Count;

			return value;
		}

		/// <summary>
		/// Gets the total number of files in this directory and all subdirectories
		/// </summary>
		/// <returns>The total number of files</returns>
		public int GetTotalNumberOfFiles()
		{
			int value;

			value = 0;
			foreach (JDirectory directory in _directories.Values)
				value += directory.GetTotalNumberOfFiles();

			value += _files.Values.Count;

			return value;
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Private method to get a <see cref="JDirectory"/>.
		/// </summary>
		/// <param name="name">Directory name.</param>
		/// <returns>The searched directory.</returns>
		private JDirectory GetPrivateDirectory(string name)
		{
			JDirectory searched;

			_directories.TryGetValue(new JFileName(name), out searched);

			return searched;
		}

		/// <summary>
		/// Private method to get a <see cref="JFile"/>.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="options">If the search should be recursive or not.</param>
		/// <returns>The searched file.</returns>
		private JFile GetPrivateFile(string fileName, SearchOption options)
		{
			JFile searched;

			searched = null;
			if (_files.TryGetValue(new JFileName(fileName), out searched))
				return searched;

			if (options == SearchOption.AllDirectories)
			{
				foreach (JDirectory directory in _directories.Values)
				{
					searched = directory.GetPrivateFile(fileName, options);
					if (searched != null)
						break;
				}
			}

			return searched;
		}

		/// <summary>
		/// Gets the qualified names of all the files of the directory and its subdirectories.
		/// </summary>
		/// <param name="directory">Directory where to perfom the search.</param>
		/// <param name="searchPattern">Search pattern to match in the filenames.</param>
		/// <param name="fileNames">Collection where the qualified names will be added.</param>
		private void GetPrivateFileNames(JDirectory directory, string searchPattern, Collection<string> fileNames)
		{
			foreach (JFile file in directory.GetFiles())
				if (Match(searchPattern, file.Name, false))
					fileNames.Add(file.GetQualifiedName());

			foreach (JDirectory childDirectory in directory.GetDirectories())
				directory.GetPrivateFileNames(childDirectory, searchPattern, fileNames);
		}

		/// <summary>
		/// Returns true if the string matches the pattern which may contain * and ? wildcards.
		/// </summary>
		/// <param name="pattern">Pattern to match.</param>
		/// <param name="fileName">Filename to match.</param>
		/// <param name="caseSensitive">If the match is case sensitive or not.</param>
		/// <returns>True if the patterna and the fileName match, false if not.</returns>
		/// <remarks>
		/// Based on robagar C# port of Jack Handy Codeproject article:
		/// http://www.codeproject.com/string/wildcmp.asp#xx1000279xx
		/// </remarks>
		private bool Match(string pattern, string fileName, bool caseSensitive)
		{
			// if not concerned about case, convert both string and pattern
			// to lower case for comparison
			if (!caseSensitive)
			{
				pattern = pattern.ToLower();
				fileName = fileName.ToLower();
			}            		
            if(string.IsNullOrEmpty(pattern))return false;//如果空字符 返回没有
            if (pattern.CompareTo("*")==0 || pattern.CompareTo("*.*")==0) {
                return true;//快速匹配
            }           
            // if pattern doesn't actually contain any wildcards, use simple equality
            //if (pattern.IndexOfAny(Wildcards) == -1)//不存在匹配符号
            //	return (fileName == pattern);
            bool findP = false;
            foreach (var v1 in Wildcards) {
                foreach (var v2 in pattern) {
                    if (v1.CompareTo(v2) == 0) {
                        findP = true; break;
                    }
                }
            }
            if (!findP) return (fileName == pattern);
			// otherwise do pattern matching
            SearchPattern mSearchPattern =new SearchPattern(pattern);
            return mSearchPattern.IsMatch(fileName);
            //下面逻辑可能有点问题
			int i = 0;
			int j = 0;
			while (i < fileName.Length && j < pattern.Length && pattern[j] != '*')
			{
				if ((pattern[j] != fileName[i]) && (pattern[j] != '?'))
				{
					return false;
				}
				i++;
				j++;
			}

			// if we have reached the end of the pattern without finding a * wildcard,
			// the match must fail if the string is longer or shorter than the pattern
			if (j == pattern.Length)
				return fileName.Length == pattern.Length;

			int cp = 0;
			int mp = 0;
			while (i < fileName.Length)
			{
				if (j < pattern.Length && pattern[j] == '*')
				{
					if ((j++) >= pattern.Length)
					{
						return true;
					}
					mp = j;
					cp = i + 1;
				}
				else if (j < pattern.Length && (pattern[j] == fileName[i] || pattern[j] == '?'))
				{
					j++;
					i++;
				}
				else
				{
					j = mp;
					i = cp++;
				}
			}

			while (j < pattern.Length && pattern[j] == '*')
			{
				j++;
			}

			return j >= pattern.Length;
		}

		#endregion
	}
}
