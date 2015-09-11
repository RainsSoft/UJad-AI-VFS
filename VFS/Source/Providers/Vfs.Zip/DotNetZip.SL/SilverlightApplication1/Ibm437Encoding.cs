using System.Collections.Generic;
using System.Text;

namespace Hardcodet.Silverlight.Util
{
  public class CustomEncoding : Encoding
  {
    /// <summary>
    /// Gets the name registered with the
    /// Internet Assigned Numbers Authority (IANA) for the current encoding.
    /// </summary>
    /// <returns>
    /// The IANA name for the current <see cref="System.Text.Encoding"/>.
    /// </returns>
    public override string WebName
    {
      get
      {
        return "IBM437";
      }
    }


    /// <summary>
    /// Calculates the number of bytes produced by encoding a set of characters
    /// from the specified character array.
    /// </summary>
    /// <returns>
    /// The number of bytes produced by encoding the specified characters. This class
    /// alwas returns the value of <paramref name="count"/>.
    /// </returns>
    public override int GetByteCount(char[] chars, int index, int count)
    {
      return count;
    }

    /// <summary>
    /// Encodes a set of characters from the specified character array into the specified byte array.
    /// </summary>
    /// <returns>
    /// The actual number of bytes written into <paramref name="bytes"/>.
    /// </returns>
    /// <param name="chars">The character array containing the set of characters to encode. 
    /// </param><param name="charIndex">The index of the first character to encode. 
    /// </param><param name="charCount">The number of characters to encode. 
    /// </param><param name="bytes">The byte array to contain the resulting sequence of bytes.
    /// </param><param name="byteIndex">The index at which to start writing the resulting sequence of bytes. 
    /// </param>
    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
      for (int i = 0; i < charCount; i++)
      {
        var character = chars[i];
        bytes[byteIndex + i] = charToByte[character];
      }

      return charCount;
    }

    /// <summary>
    /// Calculates the number of characters produced by decoding a sequence
    /// of bytes from the specified byte array.
    /// </summary>
    /// <returns>
    /// The number of characters produced by decoding the specified sequence of bytes. This class
    /// alwas returns the value of <paramref name="count"/>. 
    /// </returns>
    public override int GetCharCount(byte[] bytes, int index, int count)
    {
      return count;
    }


    /// <summary>
    /// Decodes a sequence of bytes from the specified byte array into the specified character array.
    /// </summary>
    /// <returns>
    /// The actual number of characters written into <paramref name="chars"/>.
    /// </returns>
    /// <param name="bytes">The byte array containing the sequence of bytes to decode. 
    /// </param><param name="byteIndex">The index of the first byte to decode. 
    /// </param><param name="byteCount">The number of bytes to decode. 
    /// </param><param name="chars">The character array to contain the resulting set of characters. 
    /// </param><param name="charIndex">The index at which to start writing the resulting set of characters. 
    /// </param>
    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
      for (int i = 0; i < byteCount; i++)
      {
        chars[charIndex + i] = byteToChar[bytes[i + byteIndex]];
      }

      return byteCount;
    }

    /// <summary>
    /// Calculates the maximum number of bytes produced by encoding the specified number of characters.
    /// </summary>
    /// <returns>
    /// The maximum number of bytes produced by encoding the specified number of characters. This
    /// class alwas returns the value of <paramref name="charCount"/>.
    /// </returns>
    /// <param name="charCount">The number of characters to encode. 
    /// </param>
    public override int GetMaxByteCount(int charCount)
    {
      return charCount;
    }

    /// <summary>
    /// Calculates the maximum number of characters produced by decoding the specified number of bytes.
    /// </summary>
    /// <returns>
    /// The maximum number of characters produced by decoding the specified number of bytes. This class
    /// alwas returns the value of <paramref name="byteCount"/>.
    /// </returns>
    /// <param name="byteCount">The number of bytes to decode.</param> 
    public override int GetMaxCharCount(int byteCount)
    {
      return byteCount;
    }


    #region Character Table

    /// <summary>
    /// This table contains characters in an array. The index within the
    /// array corresponds to the encoding's mapping of bytes to characters
    /// (e.g. if a byte value of 5 is used to encode the character 'x', this
    /// character will be stored at the array index 5.
    /// </summary>
    private static char[] byteToChar = new char[]
    {
      (char)0,
      (char)1,
      (char)2,
      (char)3,
      (char)4,
      (char)5,
      (char)6,
      (char)7,
      (char)8,
      (char)9,
      (char)10,
      (char)11,
      (char)12,
      (char)13,
      (char)14,
      (char)15,
      (char)16,
      (char)17,
      (char)18,
      (char)19,
      (char)20,
      (char)21,
      (char)22,
      (char)23,
      (char)24,
      (char)25,
      (char)26,
      (char)27,
      (char)28,
      (char)29,
      (char)30,
      (char)31,
      (char)32,
      (char)33,
      (char)34,
      (char)35,
      (char)36,
      (char)37,
      (char)38,
      (char)39,
      (char)40,
      (char)41,
      (char)42,
      (char)43,
      (char)44,
      (char)45,
      (char)46,
      (char)47,
      (char)48,
      (char)49,
      (char)50,
      (char)51,
      (char)52,
      (char)53,
      (char)54,
      (char)55,
      (char)56,
      (char)57,
      (char)58,
      (char)59,
      (char)60,
      (char)61,
      (char)62,
      (char)63,
      (char)64,
      (char)65,
      (char)66,
      (char)67,
      (char)68,
      (char)69,
      (char)70,
      (char)71,
      (char)72,
      (char)73,
      (char)74,
      (char)75,
      (char)76,
      (char)77,
      (char)78,
      (char)79,
      (char)80,
      (char)81,
      (char)82,
      (char)83,
      (char)84,
      (char)85,
      (char)86,
      (char)87,
      (char)88,
      (char)89,
      (char)90,
      (char)91,
      (char)92,
      (char)93,
      (char)94,
      (char)95,
      (char)96,
      (char)97,
      (char)98,
      (char)99,
      (char)100,
      (char)101,
      (char)102,
      (char)103,
      (char)104,
      (char)105,
      (char)106,
      (char)107,
      (char)108,
      (char)109,
      (char)110,
      (char)111,
      (char)112,
      (char)113,
      (char)114,
      (char)115,
      (char)116,
      (char)117,
      (char)118,
      (char)119,
      (char)120,
      (char)121,
      (char)122,
      (char)123,
      (char)124,
      (char)125,
      (char)126,
      (char)127,
      (char)199,
      (char)252,
      (char)233,
      (char)226,
      (char)228,
      (char)224,
      (char)229,
      (char)231,
      (char)234,
      (char)235,
      (char)232,
      (char)239,
      (char)238,
      (char)236,
      (char)196,
      (char)197,
      (char)201,
      (char)230,
      (char)198,
      (char)244,
      (char)246,
      (char)242,
      (char)251,
      (char)249,
      (char)255,
      (char)214,
      (char)220,
      (char)162,
      (char)163,
      (char)165,
      (char)8359,
      (char)402,
      (char)225,
      (char)237,
      (char)243,
      (char)250,
      (char)241,
      (char)209,
      (char)170,
      (char)186,
      (char)191,
      (char)8976,
      (char)172,
      (char)189,
      (char)188,
      (char)161,
      (char)171,
      (char)187,
      (char)9617,
      (char)9618,
      (char)9619,
      (char)9474,
      (char)9508,
      (char)9569,
      (char)9570,
      (char)9558,
      (char)9557,
      (char)9571,
      (char)9553,
      (char)9559,
      (char)9565,
      (char)9564,
      (char)9563,
      (char)9488,
      (char)9492,
      (char)9524,
      (char)9516,
      (char)9500,
      (char)9472,
      (char)9532,
      (char)9566,
      (char)9567,
      (char)9562,
      (char)9556,
      (char)9577,
      (char)9574,
      (char)9568,
      (char)9552,
      (char)9580,
      (char)9575,
      (char)9576,
      (char)9572,
      (char)9573,
      (char)9561,
      (char)9560,
      (char)9554,
      (char)9555,
      (char)9579,
      (char)9578,
      (char)9496,
      (char)9484,
      (char)9608,
      (char)9604,
      (char)9612,
      (char)9616,
      (char)9600,
      (char)945,
      (char)223,
      (char)915,
      (char)960,
      (char)931,
      (char)963,
      (char)181,
      (char)964,
      (char)934,
      (char)920,
      (char)937,
      (char)948,
      (char)8734,
      (char)966,
      (char)949,
      (char)8745,
      (char)8801,
      (char)177,
      (char)8805,
      (char)8804,
      (char)8992,
      (char)8993,
      (char)247,
      (char)8776,
      (char)176,
      (char)8729,
      (char)183,
      (char)8730,
      (char)8319,
      (char)178,
      (char)9632,
      (char)160
    };

    #endregion


    #region Byte Lookup Dictionary

    /// <summary>
    /// This dictionary is used to resolve byte values for a given character.
    /// </summary>
    private static Dictionary<char, byte> charToByte = new Dictionary<char, byte>
    {
      { (char)0, 0 },
      { (char)1, 1 },
      { (char)2, 2 },
      { (char)3, 3 },
      { (char)4, 4 },
      { (char)5, 5 },
      { (char)6, 6 },
      { (char)7, 7 },
      { (char)8, 8 },
      { (char)9, 9 },
      { (char)10, 10 },
      { (char)11, 11 },
      { (char)12, 12 },
      { (char)13, 13 },
      { (char)14, 14 },
      { (char)15, 15 },
      { (char)16, 16 },
      { (char)17, 17 },
      { (char)18, 18 },
      { (char)19, 19 },
      { (char)20, 20 },
      { (char)21, 21 },
      { (char)22, 22 },
      { (char)23, 23 },
      { (char)24, 24 },
      { (char)25, 25 },
      { (char)26, 26 },
      { (char)27, 27 },
      { (char)28, 28 },
      { (char)29, 29 },
      { (char)30, 30 },
      { (char)31, 31 },
      { (char)32, 32 },
      { (char)33, 33 },
      { (char)34, 34 },
      { (char)35, 35 },
      { (char)36, 36 },
      { (char)37, 37 },
      { (char)38, 38 },
      { (char)39, 39 },
      { (char)40, 40 },
      { (char)41, 41 },
      { (char)42, 42 },
      { (char)43, 43 },
      { (char)44, 44 },
      { (char)45, 45 },
      { (char)46, 46 },
      { (char)47, 47 },
      { (char)48, 48 },
      { (char)49, 49 },
      { (char)50, 50 },
      { (char)51, 51 },
      { (char)52, 52 },
      { (char)53, 53 },
      { (char)54, 54 },
      { (char)55, 55 },
      { (char)56, 56 },
      { (char)57, 57 },
      { (char)58, 58 },
      { (char)59, 59 },
      { (char)60, 60 },
      { (char)61, 61 },
      { (char)62, 62 },
      { (char)63, 63 },
      { (char)64, 64 },
      { (char)65, 65 },
      { (char)66, 66 },
      { (char)67, 67 },
      { (char)68, 68 },
      { (char)69, 69 },
      { (char)70, 70 },
      { (char)71, 71 },
      { (char)72, 72 },
      { (char)73, 73 },
      { (char)74, 74 },
      { (char)75, 75 },
      { (char)76, 76 },
      { (char)77, 77 },
      { (char)78, 78 },
      { (char)79, 79 },
      { (char)80, 80 },
      { (char)81, 81 },
      { (char)82, 82 },
      { (char)83, 83 },
      { (char)84, 84 },
      { (char)85, 85 },
      { (char)86, 86 },
      { (char)87, 87 },
      { (char)88, 88 },
      { (char)89, 89 },
      { (char)90, 90 },
      { (char)91, 91 },
      { (char)92, 92 },
      { (char)93, 93 },
      { (char)94, 94 },
      { (char)95, 95 },
      { (char)96, 96 },
      { (char)97, 97 },
      { (char)98, 98 },
      { (char)99, 99 },
      { (char)100, 100 },
      { (char)101, 101 },
      { (char)102, 102 },
      { (char)103, 103 },
      { (char)104, 104 },
      { (char)105, 105 },
      { (char)106, 106 },
      { (char)107, 107 },
      { (char)108, 108 },
      { (char)109, 109 },
      { (char)110, 110 },
      { (char)111, 111 },
      { (char)112, 112 },
      { (char)113, 113 },
      { (char)114, 114 },
      { (char)115, 115 },
      { (char)116, 116 },
      { (char)117, 117 },
      { (char)118, 118 },
      { (char)119, 119 },
      { (char)120, 120 },
      { (char)121, 121 },
      { (char)122, 122 },
      { (char)123, 123 },
      { (char)124, 124 },
      { (char)125, 125 },
      { (char)126, 126 },
      { (char)127, 127 },
      { (char)199, 128 },
      { (char)252, 129 },
      { (char)233, 130 },
      { (char)226, 131 },
      { (char)228, 132 },
      { (char)224, 133 },
      { (char)229, 134 },
      { (char)231, 135 },
      { (char)234, 136 },
      { (char)235, 137 },
      { (char)232, 138 },
      { (char)239, 139 },
      { (char)238, 140 },
      { (char)236, 141 },
      { (char)196, 142 },
      { (char)197, 143 },
      { (char)201, 144 },
      { (char)230, 145 },
      { (char)198, 146 },
      { (char)244, 147 },
      { (char)246, 148 },
      { (char)242, 149 },
      { (char)251, 150 },
      { (char)249, 151 },
      { (char)255, 152 },
      { (char)214, 153 },
      { (char)220, 154 },
      { (char)162, 155 },
      { (char)163, 156 },
      { (char)165, 157 },
      { (char)8359, 158 },
      { (char)402, 159 },
      { (char)225, 160 },
      { (char)237, 161 },
      { (char)243, 162 },
      { (char)250, 163 },
      { (char)241, 164 },
      { (char)209, 165 },
      { (char)170, 166 },
      { (char)186, 167 },
      { (char)191, 168 },
      { (char)8976, 169 },
      { (char)172, 170 },
      { (char)189, 171 },
      { (char)188, 172 },
      { (char)161, 173 },
      { (char)171, 174 },
      { (char)187, 175 },
      { (char)9617, 176 },
      { (char)9618, 177 },
      { (char)9619, 178 },
      { (char)9474, 179 },
      { (char)9508, 180 },
      { (char)9569, 181 },
      { (char)9570, 182 },
      { (char)9558, 183 },
      { (char)9557, 184 },
      { (char)9571, 185 },
      { (char)9553, 186 },
      { (char)9559, 187 },
      { (char)9565, 188 },
      { (char)9564, 189 },
      { (char)9563, 190 },
      { (char)9488, 191 },
      { (char)9492, 192 },
      { (char)9524, 193 },
      { (char)9516, 194 },
      { (char)9500, 195 },
      { (char)9472, 196 },
      { (char)9532, 197 },
      { (char)9566, 198 },
      { (char)9567, 199 },
      { (char)9562, 200 },
      { (char)9556, 201 },
      { (char)9577, 202 },
      { (char)9574, 203 },
      { (char)9568, 204 },
      { (char)9552, 205 },
      { (char)9580, 206 },
      { (char)9575, 207 },
      { (char)9576, 208 },
      { (char)9572, 209 },
      { (char)9573, 210 },
      { (char)9561, 211 },
      { (char)9560, 212 },
      { (char)9554, 213 },
      { (char)9555, 214 },
      { (char)9579, 215 },
      { (char)9578, 216 },
      { (char)9496, 217 },
      { (char)9484, 218 },
      { (char)9608, 219 },
      { (char)9604, 220 },
      { (char)9612, 221 },
      { (char)9616, 222 },
      { (char)9600, 223 },
      { (char)945, 224 },
      { (char)223, 225 },
      { (char)915, 226 },
      { (char)960, 227 },
      { (char)931, 228 },
      { (char)963, 229 },
      { (char)181, 230 },
      { (char)964, 231 },
      { (char)934, 232 },
      { (char)920, 233 },
      { (char)937, 234 },
      { (char)948, 235 },
      { (char)8734, 236 },
      { (char)966, 237 },
      { (char)949, 238 },
      { (char)8745, 239 },
      { (char)8801, 240 },
      { (char)177, 241 },
      { (char)8805, 242 },
      { (char)8804, 243 },
      { (char)8992, 244 },
      { (char)8993, 245 },
      { (char)247, 246 },
      { (char)8776, 247 },
      { (char)176, 248 },
      { (char)8729, 249 },
      { (char)183, 250 },
      { (char)8730, 251 },
      { (char)8319, 252 },
      { (char)178, 253 },
      { (char)9632, 254 },
      { (char)160, 255 }
    };

    #endregion
  }
}
