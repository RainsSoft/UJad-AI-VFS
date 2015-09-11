using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Vfs.Samples.LocalClient.ViewModel
{
  public abstract class ViewModelBase : INotifyPropertyChanged
  {

    #region INotifyPropertyChanged event

    ///<summary>
    ///Occurs when a property value changes.
    ///</summary>
    public event PropertyChangedEventHandler PropertyChanged;


    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for
    /// a given property.
    /// </summary>
    /// <param name="propertyName">The name of the changed property.</param>
    protected void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    #endregion
  }
}
