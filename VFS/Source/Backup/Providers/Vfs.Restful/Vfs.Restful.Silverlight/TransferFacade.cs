using System;
using System.Collections.Generic;
using Microsoft.Http;
using Vfs.Auditing;
using Vfs.Transfer;

namespace Vfs.Restful.Client
{
  /// <summary>
  /// Implements functionality to access both upload and download transfer services.
  /// </summary>
  public abstract class TransferFacade<TToken> : ITransferHandler<TToken>
  {
    #region construction

    /// <summary>
    /// Initializes a new instance of the façade.
    /// </summary>
    protected TransferFacade(string serviceBaseUri)
    {
      ServiceBaseUri = serviceBaseUri;
    }

    #endregion

    public VfsUris Uris
    {
      get { return VfsUris.Default; }
    }


    private IAuditor auditor = new NullAuditor();

    /// <summary>
    /// Adds an auditor to file system provider, which receives
    /// auditing messages for file system requests and incidents.<br/>
    /// Assigning a null reference does not set the property to null,
    /// but falls back to a <see cref="NullAuditor"/> instead so
    /// this property never returns null but a
    /// valid <see cref="IAuditor"/> instance.
    /// </summary>
    public virtual IAuditor Auditor
    {
      get { return auditor; }
      set { auditor = value ?? new NullAuditor(); }
    }


    /// <summary>
    /// The base URI of the service that is being accessed to submit
    /// file system requests.
    /// </summary>
    public string ServiceBaseUri { get; private set; }


    /// <summary>
    /// Indicates how restrictively data blocks may be transmitted.
    /// </summary>
    public TransmissionCapabilities TransmissionCapabilities
    {
      get 
      {
        string actionUri = GetTransmissionCapabilitiesRequestUri();

        var wrapped = SecureGet<Wrapped<TransmissionCapabilities>>(FileSystemTask.ProviderMetaDataRequest,
                          actionUri,
                          () => "Could not get transmission capabilitites.");
        return wrapped.Value;
      }
    }


    protected abstract string GetTransmissionCapabilitiesRequestUri();



    /// <summary>
    /// Gets the maximum block size that can be transferred, if such
    /// a limit exists. Returns null in case of arbitrary block sizes.
    /// </summary>
    public int? MaxBlockSize
    {
      get
      {
        Wrapped<int?> wrapped = SecureGet<Wrapped<int?>>(FileSystemTask.ProviderMetaDataRequest,
                                                         GetMaxBlockSizeRequestUri(),
                                                         () => "Could not get maximum block size.");
        return wrapped.Value;
      }
    }



    protected abstract string GetMaxBlockSizeRequestUri();



    /// <summary>
    /// Requeries a previously issued token. Can be used if the client only stores
    /// <see cref="TransferToken.TransferId"/> values rather than the tokens and
    /// needs to get ahold of them again.
    /// </summary>
    /// <param name="transferId"></param>
    /// <returns></returns>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    public TToken ReloadToken(string transferId)
    {
      string actionUri = GetReloadTokenRequestUri();
      actionUri = actionUri.ConstructUri(Uris.PatternTransferId, transferId);


      return SecureGet<TToken>(GetReloadTokenRequestContext(),
                          actionUri,
                          () => String.Format("Could not reload token for transfer [{0}].", transferId));
    }

    protected abstract string GetReloadTokenRequestUri();
    protected abstract FileSystemTask GetReloadTokenRequestContext();


    /// <summary>
    /// Gets the server-side status of the transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The status of the requested transfer. If the transfer is unknown,
    /// this does not cause an exception, but merely results in a return value
    /// of <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    public TransferStatus GetTransferStatus(string transferId)
    {
      string actionUri = GetTransferStatusRequestUri();
      actionUri = actionUri.ConstructUri(Uris.PatternTransferId, transferId);

      var wrapped = SecureGet<Wrapped<TransferStatus>>(GetTransferStatusRequestContext(),
                        actionUri,
                        () => String.Format("Could not get transfer status of transfer [{0}].", transferId));
      return wrapped.Value;
    }

    protected abstract string GetTransferStatusRequestUri();
    protected abstract FileSystemTask GetTransferStatusRequestContext();




    /// <summary>
    /// Tells the transfer service that transmission is being
    /// paused for an unknown period of time. This should keep
    /// the transfer enabled (including lock to protect the resource),
    /// but gives the service time to free or unlock resources.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    public void PauseTransfer(string transferId)
    {
      string actionUri = GetPauseTransferRequestUri();
      actionUri = actionUri.ConstructUri(Uris.PatternTransferId, transferId);


      SecureRequest(GetPauseTransferRequestContext(),
                    c =>
                      {
                        c.DefaultHeaders.ContentLength = 0;
                        return c.Post(actionUri, HttpContent.CreateEmpty());
                      },
                    () => String.Format("Could not pause transfer [{0}].", transferId));
    }

    protected abstract string GetPauseTransferRequestUri();
    protected abstract FileSystemTask GetPauseTransferRequestContext();



    /// <summary>
    /// Completes a given file transfer - invoking this operation
    /// closes and removes the transfer from the service. It is highly
    /// recommended to invoke this method after finishing a transfer
    /// in order to free used/locked resources as soon as possible.<br/>
    /// In case of an upload, setting the <see cref="DataBlockInfo.IsLastBlock"/>
    /// property of the last submitted data block implicitly calls this method.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <returns>The new status of the transfer, which is <see cref="TransferStatus.Completed"/>
    /// in case of a known transfer. If the transfer is not known (maybe because it was aborted
    /// by the system), this method returns <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    public TransferStatus CompleteTransfer(string transferId)
    {
      string actionUri = GetCompleteTransferRequestUri();
      actionUri = actionUri.ConstructUri(Uris.PatternTransferId, transferId);

      var wrapped = SecurePost<Wrapped<TransferStatus>>(GetCompleteTransferRequestContext(),
                        actionUri,
                        HttpContent.CreateEmpty(),
                        () => String.Format("Could not complete transfer [{0}].", transferId));
      return wrapped.Value;

    }

    protected abstract string GetCompleteTransferRequestUri();
    protected abstract FileSystemTask GetCompleteTransferRequestContext();


    /// <summary>
    /// Aborts a currently managed resource transfer.
    /// </summary>
    /// <param name="transferId">Identifies the transfer and resource.</param>
    /// <param name="reason">The reason to cancel the transfer.</param>
    /// <returns>The new status of the transfer, which is <see cref="TransferStatus.Aborted"/>
    /// in case of a known transfer. If the transfer is not known (maybe because it was aborted
    /// by the system), this method returns <see cref="TransferStatus.UnknownTransfer"/>.</returns>
    /// <exception cref="UnknownTransferException">In case the <paramref name="transferId"/>
    /// does not refer to an active transfer.</exception>
    public TransferStatus CancelTransfer(string transferId, AbortReason reason)
    {
      string actionUri = GetCancelTransferRequestUri();
      actionUri = actionUri.ConstructUri(Uris.PatternTransferId, transferId);
      actionUri = actionUri.ConstructUri(Uris.PatternAbortReason, reason.ToString());

      var wrapped = SecurePost<Wrapped<TransferStatus>>(GetCancelTransferRequestContext(),
                        actionUri,
                        HttpContent.CreateEmpty(),
                        () => String.Format("Could not cancel transfer [{0}].", transferId));
      return wrapped.Value;
    }

    protected abstract string GetCancelTransferRequestUri();
    protected abstract FileSystemTask GetCancelTransferRequestContext();



    //TODO remove once obsolete
    public IEnumerable<DataBlockInfo> GetTransferredBlocks(string transferId)
    {
      throw new NotSupportedException("Not supported, should be removed from interface.");
    }




    protected virtual T SecureFunc<T>(FileSystemTask task, Func<T> func, Func<string> errorMessage)
    {
      return Util.SecureFunc(Auditor, task, func, errorMessage);
    }


    protected virtual void SecureAction(FileSystemTask task, Action action, Func<string> errorMessage)
    {
      Util.SecureAction(Auditor, task, action, errorMessage);
    }


    protected T SecureGet<T>(FileSystemTask context, string actionUri, Func<string> errorMessage)
    {
      return Util.SecureFunc(Auditor, context,
                             () => Util.Get<T>(ServiceBaseUri, actionUri),
                             errorMessage);
    }

    protected T SecurePost<T>(FileSystemTask context, string actionUri, HttpContent content, Func<string> errorMessage)
    {
      return Util.SecureFunc(Auditor, context,
                             () => Util.Post<T>(ServiceBaseUri, actionUri, content),
                             errorMessage);
    }


    protected void SecureRequest(FileSystemTask context, Func<HttpClient, HttpResponseMessage> requestFunc, Func<string> errorMessage)
    {
      Util.SecureAction(Auditor, context,
                             () => Util.RunRequest(ServiceBaseUri, requestFunc),
                             errorMessage);
    }

  }
}