#region Using Directives

using System;
using System.Security.Cryptography;

#endregion

namespace JadEngine.VFS.Storage.Filters
{
	/// <summary>
	/// This filter applies the Rijndael encryption algorithm.
	/// </summary>
	/// <remarks>It uses 256 bits key length, CBC cypher mode and zeros for padding</remarks>
	//[JFilterDesignerAttribute(typeof(JEncryptionConfigurationUI))]
    [JFilterDesigner(typeof(IJFilterConfigurationUI))]
	public class JRijndaelEncryptionFilter : JEncryptionFilter
	{
		#region Properties

		/// <summary>
		/// Long name of the filter
		/// </summary>
		/// <remarks>Long name is "Rijndael Encryption"</remarks>
		public override string LongName
		{
			get { return "Rijndael Encryption"; }
		}

		/// <summary>
		/// Short name of the filter
		/// </summary>
		/// <remarks>Short name is "Rijn"</remarks>
		public override string ShortName
		{
			get { return "Rijn"; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public JRijndaelEncryptionFilter()
		{
			_keySet = false;
			_IVSet = false;

			_algorithm = new RijndaelManaged();
			_algorithm.BlockSize = 256;
			_algorithm.KeySize = 256;

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
			_algorithm.Key = generator.GetBytes(32);

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
			_algorithm.IV = generator.GetBytes(32);

			_IVSet = true;
		}

		#endregion
	}
}
