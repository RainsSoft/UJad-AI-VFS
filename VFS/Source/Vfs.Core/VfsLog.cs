using System;
using Slf;

namespace Vfs
{
  /// <summary>
  /// An internal façade to SLF's <see cref="LoggerService"/>,
  /// which writes to a named logger called <c>VFS</c>.
  /// </summary>
  public static class VfsLog
  {
    public const string VfsLoggerName = "VFS";

    public static ILogger Logger
    {
      get { return LoggerService.GetLogger(VfsLoggerName); }
    }

    public static void Info(string msg)
    {
      Logger.Info(msg);
    }

    public static void Info(Exception exception, string msg)
    {
      Logger.Info(exception, msg);
    }

    public static void Info(Exception exception, string format, params object[] args)
    {
      Logger.Info(exception, format, args);
    }

    public static void Info(string format, params object[] args)
    {
      Logger.Info(format, args);
    }

    public static void Warn(string msg)
    {
      Logger.Warn(msg);
    }

    public static void Warn(Exception exception, string msg)
    {
      Logger.Warn(exception, msg);
    }

    public static void Warn(Exception exception, string format, params object[] args)
    {
      Logger.Warn(exception, format, args);
    }

    public static void Warn(string format, params object[] args)
    {
      Logger.Warn(format, args);
    }

    public static void Error(string msg)
    {
      Logger.Error(msg);
    }

    public static void Error(Exception exception, string msg)
    {
      Logger.Error(exception, msg);
    }

    public static void Error(Exception exception, string format, params object[] args)
    {
      Logger.Error(exception, format, args);
    }

    public static void Error(string format, params object[] args)
    {
      Logger.Error(format, args);
    }

    public static void Fatal(string msg)
    {
      Logger.Fatal(msg);
    }

    public static void Fatal(Exception exception, string msg)
    {
      Logger.Fatal(exception, msg);
    }

    public static void Fatal(Exception exception, string format, params object[] args)
    {
      Logger.Fatal(exception, format, args);
    }

    public static void Fatal(string format, params object[] args)
    {
      Logger.Fatal(format, args);
    }

    public static void Debug(string msg)
    {
      Logger.Debug(msg);
    }

    public static void Debug(Exception exception, string msg)
    {
      Logger.Debug(exception, msg);
    }

    public static void Debug(Exception exception, string format, params object[] args)
    {
      Logger.Debug(exception, format, args);
    }

    public static void Debug(string format, params object[] args)
    {
      Logger.Debug(format, args);
    }
  }
}
