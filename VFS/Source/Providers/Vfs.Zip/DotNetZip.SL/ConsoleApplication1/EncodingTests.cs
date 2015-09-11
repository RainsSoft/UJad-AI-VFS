using System.Linq;
using System.Text;
using Hardcodet.Silverlight.Util;
using NUnit.Framework;


namespace ConsoleApplication1
{
  [TestFixture]
  public class EncodingTests
  {
    private CustomEncoding myEncoding;
    private Encoding referenceEncoding;

    [SetUp]
    public void Init()
    {
      myEncoding = new CustomEncoding();
      referenceEncoding = Encoding.GetEncoding(myEncoding.WebName);
    }

    private static byte[] GetByteRange(int range)
    {
      byte[] data = new byte[range];
      for (int i = 0; i < range; i++)
      {
        data[i] = (byte) i;
      }

      return data;
    }

    private char[] GetCharacters(int range)
    {
      var bytes = GetByteRange(range);
      return referenceEncoding.GetChars(bytes);
    }

    


    [Test]
    public void Decoding_Bytes_Should_Return_Same_Characters()
    {
      var customChars = myEncoding.GetChars(GetByteRange(CustomEncoding.CharacterCount));
      var winChars = referenceEncoding.GetChars(GetByteRange(CustomEncoding.CharacterCount));
      Assert.AreEqual(customChars, winChars);
    }

    [Test]
    public void Decoding_Bytes_Should_Return_Same_Strings()
    {
      var customString = myEncoding.GetString(GetByteRange(CustomEncoding.CharacterCount));
      var winString = referenceEncoding.GetString(GetByteRange(CustomEncoding.CharacterCount));
      Assert.AreEqual(customString, winString);
    }

    [Test]
    public void Encoding_Characters_Should_Return_Same_Bytes()
    {
      var customBytes = myEncoding.GetBytes(GetCharacters(CustomEncoding.CharacterCount));
      var winBytes = referenceEncoding.GetBytes(GetCharacters(CustomEncoding.CharacterCount));
      Assert.AreEqual(winBytes, customBytes);
    }


    [Test]
    public void Encoding_Strings_Should_Return_Same_Bytes()
    {
      var s = new string(GetCharacters(CustomEncoding.CharacterCount));

      var customBytes = myEncoding.GetBytes(s);
      var winBytes = referenceEncoding.GetBytes(s);
      Assert.AreEqual(winBytes, customBytes);
    }


    [Test]
    public void Roundtrip_Should_Work()
    {
      string original = new string(GetCharacters(CustomEncoding.CharacterCount));

      var bytes = myEncoding.GetBytes(original);
      var chars = myEncoding.GetChars(bytes);
      bytes = myEncoding.GetBytes(chars);
      var s = myEncoding.GetString(bytes);

      Assert.AreEqual(original, s);

      bytes = referenceEncoding.GetBytes(s);
      chars = referenceEncoding.GetChars(bytes);
      bytes = referenceEncoding.GetBytes(chars);
      s = referenceEncoding.GetString(bytes);

      Assert.AreEqual(original, s);
    }


    [Test]
    [ExpectedException(typeof(EncoderFallbackException))]
    public void Setting_Fallback_Character_That_Is_Not_Supported_Itself_Should_Fail()
    {
      //if the test fails, you might have to change this character, because it's probably supported
      //by the encoding
      char c = (char)34355;
      myEncoding.FallbackCharacter = c;
    }


    [Test]
    public void Setting_The_Fallback_Character_Should_Update_Fallback_Byte_For_Decoding()
    {
      char c = 'x';
      byte b = myEncoding.GetBytes("x").Single();

      Assert.AreNotEqual(b, myEncoding.FallbackByte);
      myEncoding.FallbackCharacter = c;
      Assert.AreEqual(b, myEncoding.FallbackByte);
    }


    [Test]
    public void Submitting_Unsupported_Char_Should_Use_Fallback_Character_If_Configured()
    {
      //if the test fails, you might have to change this character, because it's probably supported
      //by the encoding
      char c = (char) 34355;

      myEncoding.FallbackCharacter = 'x';
      Assert.AreEqual("x", myEncoding.GetString(myEncoding.GetBytes(c.ToString())));
      
      myEncoding.FallbackCharacter = 'y';
      Assert.AreEqual("y", myEncoding.GetString(myEncoding.GetBytes(c.ToString())));
    }


    [Test]
    [ExpectedException(typeof(EncoderFallbackException))]
    public void Submitting_Unsupported_Char_Should_Throw_Exception_If_No_Fallback_Char_Was_Configured()
    {
      //if the test fails, you might have to change this character, because it's probably supported
      //by the encoding
      char c = (char)34355;

      myEncoding.FallbackCharacter = null;
      myEncoding.GetBytes(c.ToString());
    }


    [Test]
    public void Submitting_Unsupported_Byte_Should_Use_Fallback_Character_If_Configured()
    {
      if(CustomEncoding.CharacterCount == 256)
      {
        Assert.Inconclusive("The encoding under test supports 256 characters, which means that every possible byte value is covered for.");
      }

      //getting the number of bytes will be outside the array scope (which is 0 based)
      byte byteValue = (byte) CustomEncoding.CharacterCount;

      myEncoding.FallbackCharacter = 'x';
      Assert.AreEqual("x", myEncoding.GetString(new[] {byteValue}));

      myEncoding.FallbackCharacter = 'y';
      Assert.AreEqual("y", myEncoding.GetString(new[] { byteValue }));
    }


    [Test]
    [ExpectedException(typeof(EncoderFallbackException))]
    public void Submitting_Unsupported_Byte_Should_Throw_Exception_If_No_Fallback_Char_Was_Configured()
    {
      if (CustomEncoding.CharacterCount == 256)
      {
        Assert.Inconclusive("The encoding under test supports 256 characters, which means that every possible byte value is covered for.");
      }

      //getting the number of bytes will be outside the array scope (which is 0 based)
      byte byteValue = (byte)CustomEncoding.CharacterCount;

      myEncoding.FallbackCharacter = null;
      myEncoding.GetString(new[] { byteValue });
    }

  }
}
