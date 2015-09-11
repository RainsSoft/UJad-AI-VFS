using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Hardcodet.Silverlight.Util;

namespace ConsoleApplication1
{

  class Program
  {
    private static void TEst()
    {
      var encoding = new CustomEncoding(); // Encoding.GetEncoding(1252);
      //get characters for the byte - should be only one
      char[] charsForByte = encoding.GetChars(new[] { (byte)128 });
      string s = encoding.GetString(new[] {(byte) 128});
      
      //this will blow if we got more than one character
      char character = charsForByte.Single();
      Console.Out.WriteLine("character = {0}", character);
      Console.Out.WriteLine("character = {0}", (char)character);
      Console.ReadLine();
    }

    static void Main(string[] args)
    {
      TEst();

      var encoding = Encoding.GetEncoding("iso-8859-1");
      var copy = new CustomEncoding();
      Verification.Compare(copy, encoding, CustomEncoding.CharacterCount);

     
      var bytes = copy.GetBytes(new[] {'a', 'x', 'b'});



      char[] chars = new char[256];
      char[] chars2 = new char[256];

      for (int i = 0; i < 256; i++)
      {
        var singleChar = encoding.GetChars(new[] {(byte)i});
        if(singleChar.Length != 1) throw new InvalidOperationException();
        chars[i] = singleChar[0];

        chars2[i] = copy.GetChars(new[] {(byte)i})[0];
      }

      Console.Out.WriteLine("encoding = {0}", encoding.GetMaxByteCount(256));
      Console.Out.WriteLine("encoding = {0}", encoding.GetMaxCharCount(256));


      StringBuilder builder = new StringBuilder();
      for (int i  = 0; i < chars.Length; i++)
      {
        var c = chars[i];
        builder.Append(c);
        Console.Out.Write(c);
        Console.Out.Write(chars2[i]);
        Console.Out.Write("\n");
      }


      string s = builder.ToString();
      Console.Out.WriteLine("s.Length = {0}", s.Length);
      byte[] a = encoding.GetBytes(s);
      byte[] b = copy.GetBytes(s);

      Stopwatch w = new Stopwatch();
      w.Start();
      for (int i = 0; i < 10000; i++)
      {
        encoding.GetBytes(s);
      }
      w.Stop();
      Console.Out.WriteLine("TOTAL: " + w.ElapsedMilliseconds);

      w = new Stopwatch();
      w.Start();
      for (int i = 0; i < 10000; i++)
      {
        encoding.GetString(a);
      }
      w.Stop();
      Console.Out.WriteLine("TOTAL: " + w.ElapsedMilliseconds);


      w = new Stopwatch();
      w.Start();
      for (int i = 0; i < 10000; i++)
      {
        copy.GetBytes(s);
      }
      w.Stop();
      Console.Out.WriteLine("TOTAL: " + w.ElapsedMilliseconds);

      w = new Stopwatch();
      w.Start();
      for (int i = 0; i < 10000; i++)
      {
        copy.GetString(a);
      }
      w.Stop();
      Console.Out.WriteLine("TOTAL: " + w.ElapsedMilliseconds);

      return;


      Debug.Assert(a.Length == b.Length, "Not the same length: " + a.Length + "," + b.Length);
      for (int i = 0; i < a.Length; i++)
      {
        byte ba = a[i];
        byte bb = b[i];
        Console.Out.WriteLine("ba, bb = {0}, {1}", ba, bb);
        Debug.Assert(ba == bb, "Difference at index " + i + " " + ba + "," + bb);
      }


      Console.ReadLine();
    }


    public class IBM437Encoding : Encoding
    {
      private static Dictionary<char, byte> reverseTable;
      private static char[] translationTable;


      public static string Generate(int codePage)
      {
        Encoding encoding = Encoding.GetEncoding(codePage);
        translationTable = new char[256];
        reverseTable = new Dictionary<char, byte>(256);

        StringBuilder charArrayBuilder = new StringBuilder();
        charArrayBuilder.AppendLine("private static char[] byteToChar = new [] {");


        StringBuilder byteLookupBuilder = new StringBuilder();
        byteLookupBuilder.Append("private static Dictionary<char, byte> charToByte")
                         .AppendLine(" = new Dictionary<char, byte>(256) {");
        
        for (int i = 0; i < 256; i++)
        {
          var singleChar = encoding.GetChars(new[] { (byte)i });
          if (singleChar.Length != 1) throw new InvalidOperationException();

          int character = singleChar[0];
          charArrayBuilder.Append("\t").Append("(char)").Append(character);
          byteLookupBuilder.Append("{(char)").Append(character).Append(", ").Append(i).Append("}");

          if (i < 256 - 1)
          {
            charArrayBuilder.AppendLine(",");
            byteLookupBuilder.AppendLine(",");
          }

          //reverseTable.Add(singleChar[0], (byte)i);
        }

        charArrayBuilder.AppendLine();
        charArrayBuilder.AppendLine("};");


        byteLookupBuilder.AppendLine();
        byteLookupBuilder.AppendLine("};");

        return byteLookupBuilder.ToString();
      }


      static IBM437Encoding()
      {
        Encoding encoding = Encoding.GetEncoding(437);
        translationTable = new char[256];
        reverseTable = new Dictionary<char, byte>(256);

        for (int i = 0; i < 256; i++)
        {
          var singleChar = encoding.GetChars(new[] { (byte)i });
          if (singleChar.Length != 1) throw new InvalidOperationException();
          translationTable[i] = singleChar[0];

          reverseTable.Add(singleChar[0], (byte)i);
        }
      }


      public override int GetByteCount(char[] chars, int index, int count)
      {
        return count;
      }

      public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
      {
        for (int i = 0; i < charCount; i++)
        {
          var character = chars[i];
          bytes[byteIndex + i] = reverseTable[character];
        }

        return charCount;
      }

      public override int GetCharCount(byte[] bytes, int index, int count)
      {
        return count;
      }

      public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
      {
        for (int i = 0; i < byteCount; i++)
        {
          chars[charIndex + i] = translationTable[bytes[i + byteIndex]];
        }

        return byteCount;
      }

      public override int GetMaxByteCount(int charCount)
      {
        return charCount + 1;
      }

      public override int GetMaxCharCount(int byteCount)
      {
        return byteCount;
      }
    }
  }

}
