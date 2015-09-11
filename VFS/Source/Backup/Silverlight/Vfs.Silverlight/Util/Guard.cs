using System;

namespace Vfs.Util
{
  /// <summary>
  /// Simple helper class that executes an arbitrary
  /// action delegate during disposal. Use this class
  /// as a guard that ensures execution of certain code
  /// during cleanup by just wrapping a code block in a using
  /// statement that creates the guard.
  /// </summary>
  /// <example>
  /// The following example uses the guard class to reset a given counter:
  /// <code>
  ///   public void Test()
  ///   {
  ///     int counter = 0;
  ///     using (new Guard(() => counter = 0))
  ///     {
  ///       for(int i=0; i&lt;10; i++)
  ///       {
  ///         Console.WriteLine(counter++);
  ///       }
  ///     }
  ///
  ///     Console.Out.WriteLine("Counter after reset: " + counter);
  ///   }
  /// </code>
  /// </example>
  public class Guard : IDisposable 
  {
    /// <summary>
    /// The action that is being executed as soon
    /// as <see cref="Dispose"/> is being invoked.
    /// </summary>
    public Action DisposalAction { get; protected set; }


    /// <summary>
    /// Protected parameterless constructor that supports deferred
    /// setting of the disposal <see cref="DisposalAction"/>.
    /// </summary>
    protected Guard()
    {
    }


    /// <summary>
    /// Inits the guard with the action to be performed once
    /// disposal takes place.
    /// </summary>
    /// <param name="disposeAction">The action that is being executed as soon
    /// as <see cref="Dispose"/> is being invoked.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="disposeAction"/>
    /// is a null reference.</exception>
    public Guard(Action disposeAction)
    {
      if (disposeAction == null) throw new ArgumentNullException("disposeAction");
      DisposalAction = disposeAction;
    }


    /// <summary>
    /// Cancels the disposal action.
    /// </summary>
    public void CancelDisposeAction()
    {
      DisposalAction = null;
    }


    /// <summary>
    /// Disposes the scope and executes the
    /// underlying disposal <see cref="DisposalAction"/>.
    /// </summary>
    public virtual void Dispose()
    {
      if (DisposalAction != null) DisposalAction();
    }
  }
}