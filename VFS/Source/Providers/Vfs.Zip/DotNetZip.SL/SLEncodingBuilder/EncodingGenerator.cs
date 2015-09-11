using System.Linq;
using System.Text;
using SLEncodingBuilder.Properties;
using SLEncodingBuilder.ViewModel;

namespace SLEncodingBuilder
{
  /// <summary>
  /// Generates the encoding class by adding data taken from the
  /// <see cref="MainViewModel"/> to the encoding template file.
  /// </summary>
  public static class EncodingGenerator
  {
    public static void GenerateCode(this MainViewModel vm)
    {
      StringBuilder template = new StringBuilder(Resources.EncodingGeneratorTemplate);
      Encoding encoding = vm.SourceEncoding;

      if(vm.SourceEncoding == null)
      {
        vm.Output = "";
        return;
      }

      StringBuilder charArrayBuilder = new StringBuilder();
      StringBuilder byteDictionaryBuilder = new StringBuilder();

      for (int i = 0; i < vm.ByteRange; i++)
      {
        //get characters for the byte - should be only one
        char[] charsForByte = encoding.GetChars(new[] { (byte)i });
        
        //this will blow if we got more than one character
        int character = charsForByte.Single();

        //add array and dictionary entry
        charArrayBuilder.Append("      ")
                        .Append("(char)").Append(character).Append(" /* byte ").Append(i).Append(" */  ");
        byteDictionaryBuilder.Append("      ")
                             .Append("{ (char)").Append(character).Append(", ").Append(i).Append(" }");

        if (i < vm.ByteRange - 1)
        {
          //add comma if it's not the last entry
          charArrayBuilder.AppendLine(",");
          byteDictionaryBuilder.AppendLine(",");
        }
      }

      template.Replace("Target.Namespace", vm.GeneratedNamespace);
      template.Replace("EncodingGeneratorTemplate", vm.GeneratedClassName);
      template.Replace("//Array-Contents", charArrayBuilder.ToString());
      template.Replace("//Dictionary-Contents", byteDictionaryBuilder.ToString());

      //set the web name property
      template.Replace("[EncodingName]", encoding.EncodingName);
      template.Replace("base.WebName", "\"" + encoding.WebName + "\"");

      //set fallback character value
      string charFallback = "";
      if(vm.FallbackCharacter.HasValue)
      {
        charFallback = "FallbackCharacter = '" + vm.FallbackCharacter + "';";
      }
      template.Replace("//FallbackCharacter", charFallback);
      
      
      vm.Output = template.ToString();
    }

  }
}
