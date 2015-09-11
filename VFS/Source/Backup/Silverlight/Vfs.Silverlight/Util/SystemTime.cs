using System;

namespace Vfs.Util
{
  /// <summary>
  /// Simple helper class that provides an façade to
  /// <see cref="DateTimeOffset.Now"/> which can be substituted
  /// for simple testing of time-related code.
  /// </summary>
  public static class SystemTime
  {
    /// <summary>
    /// Gets the system's current data and time. Only change for
    /// testing scenarios. Use <see cref="Reset"/> to
    /// reset the function to its default implementation.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
    public static Func<DateTimeOffset> Now;

    /// <summary>
    /// Reverts the <see cref="Now"/> function to its default
    /// implementation which just returns <see cref="DateTimeOffset.Now"/>.
    /// </summary>
    public static void Reset()
    {
      Now = () => DateTimeOffset.Now;
    }

    /// <summary>
    /// Inits the <see cref="Now"/> delegate.
    /// </summary>
    static SystemTime()
    {
      Reset();
    }


    /// <summary>
    /// Creates a disposable scope that automatically
    /// resets the <see cref="SystemTime"/> to its default
    /// value.
    /// </summary>
    /// <returns></returns>
    public static IDisposable CreateResetGuard()
    {
      return new Guard(Reset);
    }


    /// <summary>
    /// Sets the current system time and returns a guard
    /// that automatically resets the <see cref="SystemTime"/>
    /// to its default value once it is being disposed.
    /// </summary>
    /// <returns></returns>
    public static IDisposable CreateResetGuard(DateTimeOffset now)
    {
      Now = () => now;
      return new Guard(Reset);
    }

  }
}