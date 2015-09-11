using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace Vfs.Azure
{
  public class AzureFileItem : VirtualFileItem
  {
    /// <summary>
    /// The underlying blob.
    /// </summary>
    public CloudBlob Blob { get; set; }

    /// <summary>
    /// Indicates whether the resource physically exists on the file system
    /// or not.
    /// </summary>
    public override bool Exists
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Gets a string that provides the fully qualified string of the resource (as opposite to the
    /// <see cref="VirtualResourceInfo.FullName"/>, which is publicly exposed to
    /// clients, e.g. in exception messages).<br/>
    /// It should be ensured that this identifier always looks exactly the same for different requests,
    /// as it is being used for internal processes such as resource locking or auditing.
    /// </summary>
    public override string QualifiedIdentifier
    {
      get { throw new NotImplementedException(); }
    }
  }
}
