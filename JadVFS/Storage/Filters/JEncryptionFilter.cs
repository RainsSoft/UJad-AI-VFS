#region Using Directives

using System;
using System.Security.Cryptography;
using System.IO;

#endregion

namespace JadEngine.VFS.Storage.Filters
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class JEncryptionFilter : IJFilter
	{
		#region Fields

		/// <summary>
		/// Symmetric algorithm used for cryptography operations
		/// </summary>
		protected SymmetricAlgorithm _algorithm;

		/// <summary>
		/// Indicates if the key has being set or not
		/// </summary>
		protected bool _keySet = false;

		/// <summary>
		/// Indicates if the initialization vector has being set or not
		/// </summary>
		protected bool _IVSet = false;

		#endregion

		#region Properties

		/// <summary>
		/// Long name of the filter
		/// </summary>
		public virtual string LongName
		{
			get { return ""; }
		}

		/// <summary>
		/// Short name of the filter
		/// </summary>
		public virtual string ShortName
		{
			get { return ""; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Creates a key from a string and sets it for the algorithm.
		/// </summary>
		/// <param name="key">The key string</param>
		public abstract void SetKey(String key);

		/// <summary>
		/// Creates an initialization vector from a string and sets it for the algorithm.
		/// </summary>
		/// <param name="IV">The IV string</param>
		public abstract void SetIV(String IV);

		/// <summary>
		/// Applies the filter for reading. This method modifies 
		/// the input stream and creates a new one that is returned.
		/// </summary>
		/// <param name="input">The input stream</param>
		/// <returns>The input stream with the filter applied</returns>
		public Stream ApplyForRead(Stream input)
		{
			if (!_keySet)
				throw new InvalidOperationException("The key has not been set");

			if (!_IVSet)
				throw new InvalidOperationException("The initializing vector (IV) has not been set");

			return new CryptoStream(input, _algorithm.CreateDecryptor(), CryptoStreamMode.Read);
		}

		/// <summary>
		/// Applies the filter for writing. This method modifies 
		/// the input stream and creates a new one that is returned.
		/// </summary>
		/// <param name="input">The input stream</param>
		/// <returns>The input stream with the filter applied</returns>
		public Stream ApplyForWrite(Stream input)
		{
			if (!_keySet)
				throw new InvalidOperationException("The key has not been set");

			if (!_IVSet)
				throw new InvalidOperationException("The initializing vector (IV) has not been set");

			return new CryptoStream(input, _algorithm.CreateEncryptor(), CryptoStreamMode.Write);
		}

		#endregion
	}
}
