#region Using Directives

using System;
using System.Security.Cryptography;

#endregion

namespace JadEngine.VFS.Storage.Filters
{
	/// <summary>
	/// This filter applies the DES encryption algorithm.
	/// </summary>
	/// <remarks>It uses CBC cypher mode and zeros for padding</remarks>
	//[JFilterDesigner(typeof(JEncryptionConfigurationUI))]
    [JFilterDesigner(typeof(IJFilterConfigurationUI))]
	public class JDESEncryptionFilter : JEncryptionFilter
	{
		#region Properties

		/// <summary>
		/// Long name of the filter
		/// </summary>
		/// <remarks>Long name is "DES Encryption"</remarks>
		public override string LongName
		{
			get { return "DES Encryption"; }
		}

		/// <summary>
		/// Short name of the filter
		/// </summary>
		/// <remarks>Short name is "DES"</remarks>
		public override string ShortName
		{
			get { return "DES"; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public JDESEncryptionFilter()
		{
			_keySet = false;
			_IVSet = false;

			_algorithm = new DESCryptoServiceProvider();
			_algorithm.BlockSize = 64;
			_algorithm.KeySize = 64;

			_algorithm.Mode = CipherMode.CBC;
			_algorithm.Padding = PaddingMode.Zeros;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Creates a key from a string and sets it for the algorithm.8λ
		/// </summary>
		/// <param name="key">The key string</param>
		/// <remarks>Uses the RFC 2898 algorithm to generate the key</remarks>
		public override void SetKey(String key)
		{
			Rfc2898DeriveBytes generator = new Rfc2898DeriveBytes(key, new byte[8] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 });
			_algorithm.Key = generator.GetBytes(8);

			_keySet = true;
		}

		/// <summary>
		/// Creates an initialization vector from a string and sets it for the algorithm.8λ
		/// </summary>
		/// <param name="IV">The IV string</param>
		/// <remarks>Uses the RFC 2898 algorithm to generate the key</remarks>
		public override void SetIV(String IV)
		{
			Rfc2898DeriveBytes generator = new Rfc2898DeriveBytes(IV, new byte[8] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 });
			_algorithm.IV = generator.GetBytes(8);

			_IVSet = true;
		}

		#endregion
	}
}
