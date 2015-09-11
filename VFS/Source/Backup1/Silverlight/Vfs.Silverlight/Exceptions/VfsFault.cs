using System;
using System.Text;
using Vfs.Auditing;

namespace Vfs
{
  /// <summary>
  /// A Data Transfer Object (DTO) that can be used
  /// to transfer fault information.
  /// </summary>
  public class VfsFault
  {
    public const string FaultContentType = "application/vnd.vfs-fault+xml";

    /// <summary>
    /// Categorizes the fault. Use this flag to translate the
    /// fault back into a matching exception.
    /// </summary>
    public VfsFaultType FaultType { get; set; }

    /// <summary>
    /// The executed file system task that was running when
    /// the fault occurred.
    /// </summary>
    public FileSystemTask Context { get; set; }

    /// <summary>
    /// The ID of the fault or exception, if available.
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// The fault / exception message.
    /// </summary>
    public string Message { get; set; }


    /// <summary>
    /// Creates an empty fault.
    /// </summary>
    public VfsFault()
    {
      Context = FileSystemTask.Undefined;
      FaultType = VfsFaultType.Undefined;
      Message = String.Empty;
    }


    /// <summary>
    /// Creates a given 
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static VfsFault CreateFromException(VfsException exception)
    {
      VfsFaultException fe = exception as VfsFaultException;
      if (fe != null) return fe.Fault;

      return new VfsFault
                    {
                      FaultType = exception.FaultType,
                      Message = exception.Message,
                      EventId = exception.EventId
                    };
    }



    /// <summary>
    /// Creates a string representation of the fault.
    /// </summary>
    public string CreateFaultMessage()
    {
      StringBuilder builder = new StringBuilder();
      if(Context != FileSystemTask.Undefined)
      {
        builder.Append("Context: ").Append(Context.ToString());
      }

      if(EventId != 0)
      {
        builder.AppendLine("Event: ").Append(EventId);
      }

      if(!String.IsNullOrEmpty(Message))
      {
        builder.AppendLine(Message);
      }

      return builder.ToString();
    }
  }
}