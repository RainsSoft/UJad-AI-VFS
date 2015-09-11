using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vfs.Metadata
{
  /// <summary>
  /// A metadata property.
  /// </summary>
  public class MetaProperty
  {
    /// <summary>
    /// The meta data key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The property value.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Whether the property can be edited.
    /// </summary>
    public bool IsReadOnly { get; set; }
  }
}
