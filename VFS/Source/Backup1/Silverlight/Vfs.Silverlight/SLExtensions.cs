using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Vfs
{
  public static class SLExtensions
  {
    public static string ToLowerInvariant(this string value)
    {
      return value.ToLower(CultureInfo.InvariantCulture);
    }



    /// <summary>
    /// Removes all instances in a given list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static int RemoveAll<T>(this List<T> list, Predicate<T> match)
    {
      Ensure.ArgumentNotNull(list, "list");
      Ensure.ArgumentNotNull(match, "match");

      int counter = 0;
      for (int i = list.Count - 1; i >= 0; i++)
      {
        if (match(list[i]))
        {
          list.RemoveAt(i);
          counter++;
        }
      }

      return counter;
    }


    /// <summary>
    /// Asynchronously executes a given function and invokes the callback action
    /// on the worker thread.
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="func"></param>
    /// <param name="callback"></param>
    public static void RunAsync<T>(this Dispatcher dispatcher, Func<T> func, Action<T> callback)
    {
      ThreadPool.QueueUserWorkItem(s =>
      {
        T result = func();
        dispatcher.BeginInvoke(() => callback(result));
      });
    }
  }
}
