using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.Configuration
{
  /// <summary>
  /// Stores application settings 
  /// </summary>
  public class ApplicationSettingsBase
  {
    public IsolatedStorageSettings AppSettings { get; private set; }

    
    public static ApplicationSettingsBase Synchronized(ApplicationSettingsBase wrapped)
    {
      return wrapped;
    }


    public ApplicationSettingsBase()
    {
      AppSettings = IsolatedStorageSettings.ApplicationSettings;
      ReadProperties();
    }


    private void ReadProperties()
    {
      var settings = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
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
      return AppSettings.Contains(propertyName);
    }

    public virtual object this[string propertyName]
    {
      get
      {
        return AppSettings[propertyName];
      }
      set
      {
        AppSettings.Remove(propertyName);
        AppSettings.Add(propertyName, value);
      }
    }


    public void Save()
    {
      AppSettings.Save();
    }

  }
}
