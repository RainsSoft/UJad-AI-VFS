#region Using Directives

using System;

#endregion

namespace JadEngine.VFS.Storage
{
	/// <summary>
	/// Represents a file name in a <see cref="JStorageSource"/> system.
	/// </summary>
	/// <remarks>
	/// This class allows to match two strings without taking 
	/// into the account their case (important when searching in
	/// dictionaries).
	/// </remarks>
	public class JFileName
	{
		#region Fields

		/// <summary>
		/// Name of the file.
		/// </summary>
		private string _name;

		/// <summary>
		/// Lower case name of the file.
		/// </summary>
		private string _lowerCaseName;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				_lowerCaseName = value.ToLower();
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="name">Name of the file.</param>
		public JFileName(string name)
		{
			_name = name;
			_lowerCaseName = name.ToLower();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Tells if this object instance and another object are the same.
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns>If the objects are the same or not.</returns>
		/// <remarks>The comparison is value based, not reference based.</remarks>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (obj is JFileName == false)
				return false;

			return (this == (JFileName) obj);
		}

		/// <summary>
		/// Gets the hash code of the object
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			return _lowerCaseName.GetHashCode();
		}

		/// <summary>
		/// Tells if two objects are the same or not.
		/// </summary>
		/// <param name="obj1">First object to compare.</param>
		/// <param name="obj2">Second object to compare.</param>
		/// <returns>True if the objects are the same, false if not.</returns>
		/// <remarks>The comparison is value based, not reference based.</remarks>
		public static bool operator ==(JFileName obj1, JFileName obj2)
		{
			return (obj1.Name.ToLower().Equals(obj2.Name.ToLower()));
		}

		/// <summary>
		/// Tells if two objects are different or not.
		/// </summary>
		/// <param name="obj1">First object to compare.</param>
		/// <param name="obj2">Second object to compare.</param>
		/// <returns>True if the objects are different, false if not.</returns>
		/// <remarks>The comparison is value based, not reference based.</remarks>
		public static bool operator !=(JFileName obj1, JFileName obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion
	}
}
