using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Configuration
{
  /// <summary>
  /// This is quite a hack, which allows us to use the generated settings classes
  /// of the shared library in SL.
  /// </summary>
  public class ApplicationSettingsBase
  {
    private readonly Dictionary<string, string> settingsCache = new Dictionary<string, string>();

    
    public static ApplicationSettingsBase Synchronized(ApplicationSettingsBase wrapped)
    {
      return wrapped;
    }


    public ApplicationSettingsBase()
    {
      ReadProperties();
    }


    /// <summary>
    /// Reads properties via reflection and gets the default values.
    /// </summary>
    private void ReadProperties()
    {
      var settings = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(
          pi =>
          new
            {
              Property = pi.Name,
              DefaultValue = (DefaultSettingValueAttribute)pi.GetCustomAttributes(typeof (DefaultSettingValueAttribute), false).SingleOrDefault()
            })
        .Where(p => p.DefaultValue != null);

      foreach (var propertySetting  in settings)
      {
        if (!Contains(propertySetting.Property))
        {
          this[propertySetting.Property] = propertySetting.DefaultValue.Value;
        }
      }
    }


    public bool Contains(string propertyName)
    {
      return settingsCache.ContainsKey(propertyName);
    }

    public string this[string propertyName]
    {
      get
      {
        return settingsCache[propertyName];
      }
      private set
      {
        settingsCache[propertyName] = value;
      }
    }
  }
}
