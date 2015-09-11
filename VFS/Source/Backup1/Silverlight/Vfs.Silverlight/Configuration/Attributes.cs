using System;
using System.Net;
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

  [AttributeUsage(AttributeTargets.Property)]
  public sealed class DefaultSettingValueAttribute : Attribute
  {
    public string Value { get; private set; }


    public DefaultSettingValueAttribute(string value)
    {
      Value = value;
    }
  }


  [AttributeUsage(AttributeTargets.Property)]
  public class SettingAttribute : Attribute
  {
  }

 
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class ApplicationScopedSettingAttribute : SettingAttribute
  {
  }

}
