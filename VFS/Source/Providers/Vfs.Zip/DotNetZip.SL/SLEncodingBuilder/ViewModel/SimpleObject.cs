using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;


namespace SLEncodingBuilder.ViewModel
{
  /// <summary>
  /// Simple base class that provides a solid implementation
  /// of the <see cref="INotifyPropertyChanged"/> event including
  /// reflection-based property name verification in debug builds.
  /// </summary>
  public abstract class SimpleObject : INotifyPropertyChanged
  {
    ///<summary>
    ///Occurs when a property value changes.
    ///</summary>
    public event PropertyChangedEventHandler PropertyChanged;


    /// <summary>
    /// Allows triggering the <see cref="PropertyChanged"/> event using
    /// a lambda expression, thus avoiding strings. Keep in minde that
    /// using this method comes with a performance penalty, so don't use
    /// it for frequently updated properties that cause a lot of events
    /// to be fired.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="propertyExpression">Expression pointing to a given
    /// property.</param>
    protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
      //the cast will always succeed if properly used
      MemberExpression memberExpression = (MemberExpression)propertyExpression.Body;
      string propertyName = memberExpression.Member.Name;
      OnPropertyChanged(propertyName);
    }


    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for
    /// a given property.
    /// </summary>
    /// <param name="propertyName">The name of the changed property.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
      //validate the property name in debug builds
      VerifyProperty(propertyName);

      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }


    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for
    /// a set of properties.
    /// </summary>
    /// <param name="propertyNames">Provides the names of the changed properties.</param>
    protected void OnPropertyChanged(params string[] propertyNames)
    {
      foreach (string propertyName in propertyNames)
      {
        OnPropertyChanged(propertyName);
      }
    }
      

    /// <summary>
    /// Verifies whether the current class provides a property with a given
    /// name. This method is only invoked in debug builds, and results in
    /// a runtime exception if the <see cref="OnPropertyChanged(string)"/> method
    /// is being invoked with an invalid property name. This may happen if
    /// a property's name was changed but not the parameter of the property's
    /// invocation of <see cref="OnPropertyChanged(string)"/>.<br/>
    /// Credits for this one to Josh Smith: http://joshsmithonwpf.wordpress.com/
    /// </summary>
    /// <param name="propertyName">The name of the changed property.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)"), Conditional("DEBUG")]
    [DebuggerNonUserCode]
    private void VerifyProperty(string propertyName)
    {
      Type type = GetType();

      //look for a *public* property with the specified name
      PropertyInfo pi = type.GetProperty(propertyName);
      if (pi == null)
      {
        //there is no matching property - notify the developer
        string msg = "OnPropertyChanged was invoked with invalid property name {0}: ";
        msg += "{0} is not a public property of {1}.";
        msg = String.Format(msg, propertyName, type.FullName);
        Debug.Fail(msg);
      }
    }
  }
}