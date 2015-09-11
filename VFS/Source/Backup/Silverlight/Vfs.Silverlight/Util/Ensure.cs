// SLF.NET - Simple Logging Façade for .NET
// Contact and Information: http://slf.codeplex.com
//
// This library is free software; you can redistribute it and/or
// modify it in any way you see fit as long as this copyright
// notice is not being removed.
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE


using System;

namespace Vfs
{
  /// <summary>
  /// Provides common runtime validation functionality.
  /// </summary>
  public static class Ensure
  {
    /// <summary>
    /// Makes sure a given argument is not null.
    /// </summary>
    /// <typeparam name="T">Type of the argument.</typeparam>
    /// <param name="argument">The submitted parameter value.</param>
    /// <param name="argumentName">The name of the argument.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="argument"/>
    /// is a null reference.</exception>
    public static void ArgumentNotNull<T>(T argument, string argumentName) where T : class
    {
      if (argument == null) throw new ArgumentNullException(argumentName);
    }


    /// <summary>
    /// Makes sure the submitted <paramref name="timeout"/> is either
    /// null or a non-negative value.
    /// </summary>
    /// <param name="timeout">The timeout to be validated.</param>
    /// <param name="argumentName">The name of the argument.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the argument is
    /// negative.</exception>
    public static void ArgumentNotNegative(TimeSpan? timeout, string argumentName)
    {
      if(timeout.HasValue && timeout.Value < TimeSpan.Zero)
      {
        throw new ArgumentOutOfRangeException(argumentName, "Invalid negative timeout specified.");
      }
    }

  }
}