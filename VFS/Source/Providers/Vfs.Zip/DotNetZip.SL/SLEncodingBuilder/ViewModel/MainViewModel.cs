using System;
using System.Text;
using System.Windows;

namespace SLEncodingBuilder.ViewModel
{
  public class MainViewModel : SimpleObject
  {
    #region SourceEncodingName

    private string sourceEncodingName = "";

    /// <summary>
    /// The name or code page of the source encoding.
    /// </summary>
    public string SourceEncodingName
    {
      get { return sourceEncodingName; }
      set
      {
        sourceEncodingName = value;
        OnPropertyChanged(() => SourceEncodingName);

        if (String.IsNullOrEmpty(value))
        {
          SourceEncoding = null;
          return;
        }


        try
        {
          int codePage;
          if(int.TryParse(value, out codePage))
          {
            SourceEncoding = Encoding.GetEncoding(codePage);
          }
          else
          {
            SourceEncoding = Encoding.GetEncoding(value);
          }
        }
        catch
        {
          SourceEncoding = null;
          throw;
        }
      }
    }

    #endregion

    #region SourceEncoding

    private Encoding sourceEncoding;

    public Encoding SourceEncoding
    {
      get { return sourceEncoding; }
      private set
      {
        if(value != null && !value.IsSingleByte)
        {
          string msg = "[{0}] is not a single byte encoding. This generator only supports encodings that use only 1 byte per character.";
          msg = String.Format(msg, value.EncodingName);
          MessageBox.Show(msg, "Invalid Encoding", MessageBoxButton.OK, MessageBoxImage.Error);
          throw new InvalidOperationException(msg);
        }

        sourceEncoding = value;
        OnPropertyChanged(() => SourceEncoding);
        OnPropertyChanged(() => HasEncoding);
      }
    }

    #endregion

    #region ByteRange

    private int byteRange = 256;

    /// <summary>
    /// The number of supported characters. Cannot be more than 256,
    /// because a single byte supports at max 256 individual characters.
    /// </summary>
    public int ByteRange
    {
      get { return byteRange; }
      set
      {
        if(value < 1 || value > 256)
        {
          throw new InvalidOperationException("Byte range must be between 1 and 256 bytes.");
        }

        byteRange = value;
        OnPropertyChanged(() => ByteRange);
      }
    }

    #endregion

    #region GeneratedClassName

    private string generatedClassName = "CustomEncoding";

    /// <summary>
    /// The name of the class that is being generated.
    /// </summary>
    public string GeneratedClassName
    {
      get { return generatedClassName; }
      set
      {
        if (String.IsNullOrEmpty(value)) value = "CustomEncoding";
        generatedClassName = value;
        OnPropertyChanged(() => GeneratedClassName);
      }
    }

    #endregion

    #region GeneratedNamespace

    private string generatedNamespace = "Hardcodet.Silverlight.Util";

    /// <summary>
    /// The namespace to be used.
    /// </summary>
    public string GeneratedNamespace
    {
      get { return generatedNamespace; }
      set
      {
        if (String.IsNullOrEmpty(value)) value = "Hardcodet.Silverlight.Util";
        generatedNamespace = value;
        OnPropertyChanged("GeneratedNamespace");
      }
    }

    #endregion

    #region FallbackCharacter

    private char? fallbackCharacter = '?';

    public char? FallbackCharacter
    {
      get { return fallbackCharacter; }
      set
      {
        fallbackCharacter = value;
        OnPropertyChanged(() => FallbackCharacter);
      }
    }

    #endregion

    #region Output

    private string output = "";

    /// <summary>
    /// The generated code output.
    /// </summary>
    public string Output
    {
      get { return output; }
      set
      {
        output = value;
        OnPropertyChanged(() => Output);
      }
    }

    #endregion

    #region HasEncoding

    public bool HasEncoding
    {
      get { return SourceEncoding != null; }
    }

    #endregion

    public MainViewModel()
    {
      PropertyChanged += (s, p) => { if (p.PropertyName != "Output") this.GenerateCode(); };
    }



    public static MainViewModel Get
    {
      get { return new MainViewModel(); }
    }

  }
}
