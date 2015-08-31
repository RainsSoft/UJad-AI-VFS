#region Using Directives

using System;
using System.IO;

#endregion

namespace JadEngine.VFS.Storage
{
	/// <summary>
	/// Type of file
	/// </summary>
	public enum JFileType : int
	{
		/// <summary>
		/// The file is on the disk
		/// </summary>
		Disk = 0,

		/// <summary>
		/// The file is on a storage
		/// </summary>
		Storage = 1
	}

	/// <summary>
	/// Represents extra information about a <see cref="JFile"/>
	/// </summary>
	public class JFileInfo
	{
		#region Fields

		/// <summary>
		/// The type of file
		/// </summary>
		private JFileType _type;

		/// <summary>
		/// File info of the file (if it's a hard disk file)
		/// </summary>
		private FileInfo _info;

		/// <summary>
		/// Storage that contains the file (if it's a storage file)
		/// </summary>
		private JStorageSource _storage;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the type of the file
		/// </summary>
		public JFileType Type
		{
			get { return _type; }
		}

		/// <summary>
		/// Gets the information of a hard disk file
		/// </summary>
		public FileInfo Info
		{
			get { return _info; }
		}

		/// <summary>
		/// Gets the storage of a storage file
		/// </summary>
		public JStorageSource Storage
		{
			get { return _storage; }
		}

		#endregion

		#region Constructors
		
		/// <summary>
		/// Creates a <see cref="JFileInfo"/> for a disk file
		/// </summary>
		/// <param name="info">Information of the hard disk file</param>
		public JFileInfo(FileInfo info)
		{
			_info = info;
			_type = JFileType.Disk;
		}

		/// <summary>
		/// Creates a <see cref="JFileInfo"/> for a storage file
		/// </summary>
		/// <param name="storage">Storage where the file is located</param>
		public JFileInfo(JStorageSource storage)
		{
			_storage = storage;
			_type = JFileType.Storage;
		}

		#endregion
	}
}
