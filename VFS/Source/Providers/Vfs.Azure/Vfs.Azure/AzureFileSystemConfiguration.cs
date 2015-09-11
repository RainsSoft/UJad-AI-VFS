using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Vfs.Azure
{
  public class AzureFileSystemConfiguration
  {
    /// <summary>
    /// The client, which is used to access cloud storage.
    /// </summary>
    public CloudBlobClient BlobClient { get; set; }

    /// <summary>
    /// The root container. If this property is null, the scope
    /// of the provider will be account root container.
    /// </summary>
    public CloudBlobContainer RootContainer { get; set; }

    public AzureFileSystemConfiguration(CloudBlobClient blobClient)
    {
      BlobClient = blobClient;
    }


    public AzureFileSystemConfiguration(CloudBlobClient blobClient, string containerName)
    {
      BlobClient = blobClient;
    }


    public static AzureFileSystemConfiguration CreateForDevelopmentStorage()
    {
      var account = CloudStorageAccount.DevelopmentStorageAccount;
      return new AzureFileSystemConfiguration(account.CreateCloudBlobClient());
    }
  }
}
