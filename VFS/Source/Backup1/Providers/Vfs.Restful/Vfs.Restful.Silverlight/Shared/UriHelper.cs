using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Vfs.Restful
{
  public static class UriHelper
  {
    /// <summary>
    /// Constructs an URI by replacing a given placeholder with a
    /// specified value. This method ensures the placeholder is actually
    /// available in debug builds.
    /// </summary>
    /// <param name="uri">The uri to be processed.</param>
    /// <param name="placeholder">The placeholder to be replaced.</param>
    /// <param name="value">The value that is being injected for the
    /// placeholder.</param>
    /// <returns>Constructed string.</returns>
    public static string ConstructUri(this string uri, string placeholder, string value)
    {
#if DEBUG
      //only check URIs while debugging - once they are ok, we should be fine
      if(!uri.Contains(placeholder))
      {
        string msg = "Uri {0} doesn't contain the placeholder {1} that could take the value {2}.";
        msg = String.Format(msg, uri, placeholder, value);
#if !SILVERLIGHT
        Debug.Fail(msg);
#else
        Debug.WriteLine(msg);
#endif
      }
#endif

      return uri.Replace(placeholder, value);
    }
  }

}
