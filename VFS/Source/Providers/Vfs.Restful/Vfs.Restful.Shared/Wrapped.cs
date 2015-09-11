using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vfs.Restful
{
  /// <summary>
  /// A wrapper class, which can be used to
  /// wrap value types in order to ensure proper
  /// data contract serialization of value types.
  /// </summary>
  /// <typeparam name="T">An arbitrary wrapped value.</typeparam>
  public class Wrapped<T>
  {
    /// <summary>
    /// The wrapped value.
    /// </summary>
    public T Value { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public Wrapped()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public Wrapped(T value)
    {
      Value = value;
    }
  }
}
