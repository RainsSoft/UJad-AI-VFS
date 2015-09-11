using OpenRasta.Web;

namespace Vfs.Restful.Server.Handlers
{
  /// <summary>
  /// A simple operation result that provides a strongly typed
  /// <see cref="Item"/> property. This class is mainly used in order
  /// to be able to declare a more expressive return type in our
  /// header classes, as the <see cref="Item"/> property is only used to set the
  /// <see cref="OperationResult.ResponseResource"/>, but usually
  /// not accessed anymore.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class OperationResult<T> : OperationResult where T:class
  {
    public T Item
    {
      get { return ResponseResource as T; }
      set { ResponseResource = value; }
    }

    public OperationResult(int httpStatus)
      : base(httpStatus)
    {
    }

    public OperationResult() : base(200)
    {
    }
  }
}