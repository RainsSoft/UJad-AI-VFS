#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using JadEngine.VFS.Storage.Filters;

#endregion

namespace JadEngine.VFS.Storage
{
	/// <summary>
	/// Represents a file inside a <see cref="JStorageSource"/>.
	/// </summary>
	public class JFile
	{
		#region Fields

		/// <summary>
		/// Name of the file
		/// </summary>
		private String _name;

		/// <summary>
		/// The directory where the file is located
		/// </summary>
		private JDirectory _parent;

		/// <summary>
		/// List of filters that are applied to read or write this file
		/// </summary>
		private List<IJFilter> _filters;

		/// <summary>
		/// Offset of the file inside the storage
		/// </summary>
		private long _offset;

		/// <summary>
		/// Length of the file
		/// </summary>
		private long _length;

		/// <summary>
		/// Extra information
		/// </summary>
		private JFileInfo _fileInfo;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the file
		/// </summary>
		public String Name
		{
			get { return _name; }
		}

		/// <summary>
		/// Gets or sets directory where the file is located
		/// </summary>
		public JDirectory Parent
		{
			get { return _parent; }
			internal set { _parent = value; }
		}

		/// <summary>
		/// Gets the list of filters of the file
		/// </summary>
		public List<IJFilter> Filters
		{
			get { return _filters; }
			set
			{
				if (value != null)
					_filters = value;
			}
		}

		/// <summary>
		/// Gets or sets the offsest of the file in the storage
		/// </summary>
		public long Offset
		{
			get { return _offset; }
			internal set { _offset = value; }
		}

		/// <summary>
		/// Gets or sets the length of the file
		/// </summary>
		public long Length
		{
			get { return _length; }
			internal set { _length = value; }
		}

		/// <summary>
		/// Gets or sets the extra information of the file
		/// </summary>
		public JFileInfo FileInfo
		{
			get { return _fileInfo; }
			set { _fileInfo = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor that assigns the name
		/// </summary>
		/// <param name="name">Name of the file</param>
		public JFile(String name) : this(name, -1, -1)
		{ }

		/// <summary>
		/// Constructor that assigns the name, offset and length
		/// </summary>
		/// <param name="name">Name of the file</param>
		/// <param name="offset">Offset of the file inside the storage</param>
		/// <param name="length">Length of the file</param>
		public JFile(String name, long offset, long length)
		{
			_name = name;
			_offset = offset;
			_length = length;

			_filters = new List<IJFilter>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the qualified name of to this file to be used in VFS searches.
		/// </summary>
		/// <returns>The qualified name of the file.</returns>
		public String GetQualifiedName()
		{
			StringBuilder builder;
			JDirectory current;

			builder = new StringBuilder();
			current = _parent;

			// Add all the directories except the root one
			while ((current != null) && !(current.Name.Equals("root", StringComparison.InvariantCultureIgnoreCase) && current.Parent == null))
			{
				builder.Insert(0, current.Name);
				builder.Insert(0, Path.DirectorySeparatorChar);
				
				current = current.Parent;
			}
            if (builder.Length > 1) {
                // Remove the first slash
                builder.Remove(0, 1);
            }
			// Add the file name
			builder.Append(Path.DirectorySeparatorChar);
			builder.Append(_name);

			return builder.ToString();
		}

		/// <summary>
		/// Gets the path of the file to be used in <see cref="JStorageSource"/> construction.
		/// </summary>
		/// <returns>The path of the file.</returns>
		public String GetPath()
		{
			StringBuilder builder;
			JDirectory current;

			builder = new StringBuilder();
			current = _parent;

			// Add all the directories
			while (current != null)
			{
				builder.Insert(0, current.Name);
				builder.Insert(0, Path.DirectorySeparatorChar);

				current = current.Parent;
			}

			// Remove the first slash
			builder.Remove(0, 1);

			return builder.ToString();
		}

		/// <summary>
		/// Applies the list of filters of the file to the stream for reading.
		/// </summary>
		/// <param name="baseStream">Base stream to wrap.</param>
		/// <remarks>
		/// Filters are applied in reverse order.
		/// </remarks>
		public Stream ApplyFiltersForRead(Stream baseStream)
		{
			Stream aux;

			aux = baseStream;
			for (int i = _filters.Count - 1; i >= 0; i--)
				aux = _filters[i].ApplyForRead(aux);

			return aux;
		}

		/// <summary>
		/// Applies the list of filters of the file to the stream for writing.
		/// </summary>
		/// <param name="baseStream">Base stream to wrap.</param>
		public Stream ApplyFiltersForWrite(Stream baseStream)
		{
			Stream aux;

			aux = baseStream;
			foreach (IJFilter filter in _filters)
				aux = filter.ApplyForWrite(aux);

			return aux;
		}

		#endregion
	}
}
