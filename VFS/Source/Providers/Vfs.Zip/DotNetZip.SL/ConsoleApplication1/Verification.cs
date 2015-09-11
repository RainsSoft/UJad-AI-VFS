using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
  public static class Verification
  {
    public static void Compare(Encoding encoding, Encoding winEncoding, int range)
    {
      StringBuilder encodingChars = new StringBuilder();
      StringBuilder winChars = new StringBuilder();

      char[] encodingCharArray = new char[range];
      char[] winCharArray = new char[range];

      
      //decode bytes to characters / string
      for (int i = 0; i < range; i++)
      {
        char encodingChar = encoding.GetChars(new[] {(byte) i}).Single();
        char winChar = winEncoding.GetChars(new[] {(byte) i}).Single();
        Debug.Assert(encodingChar == winChar, "Got different characters for byte " + i + ": " + encodingChar + " / " + winChar);

        encodingCharArray[i] = encodingChar;
        winCharArray[i] = winChar;

        encodingChars.Append(encodingChar);
        winChars.Append(winChar);
      }

      //encode characters
      byte[] encodingBytes = encoding.GetBytes(encodingCharArray);
      byte[] winBytes = encoding.GetBytes(winCharArray);
      Debug.Assert(encodingBytes.Length == winBytes.Length,
                   "Encoded char arrays return byte arrays of different sizes: " + encodingBytes.Length + " vs. " +
                   winBytes.Length);

      for (int i = 0; i < encodingBytes.Length; i++)
      {
        byte encodingByte = encodingBytes[i];
        byte winByte = winBytes[i];
        Debug.Assert(encodingByte == winByte, "Got different bytes at index " + i + ": " + encodingByte + " / " + winByte);
      }

      Console.Out.WriteLine("Compared encodings successfully for " + range + " bytes.");
    }
  }
}
