using System.IO;
using Hardcodet.Commons.IO;
using NUnit.Framework;
using Vfs.LocalFileSystem.Test.Transfers.Downloading;
using Vfs.Locking;
using Vfs.Transfer;
using Vfs.Util;


namespace TransferServices_Test.Download_Service
{
  [TestFixture]
  public class Given_DownloadService_When_Streaming_Whole_File : DownloadServiceTestBase
  {
    [Test]
    public void Read_Data_Should_Match_Source_File()
    {
      string targetPath = FileUtil.CreateTempFilePath(ParentDirectory.FullName, "copy", "bin");
      
      //get stream and write to file
      using (Stream stream = DownloadHandler.ReadFile(SourceFileInfo.FullName))
      {
        stream.WriteTo(targetPath);
      }

      //calculate hashes of source and target and compare
      string sourceHash = SourceFile.CalculateMd5Hash();
      string targetHash = new FileInfo(targetPath).CalculateMd5Hash();
      Assert.AreEqual(sourceHash, targetHash);
    }



    [Test]
    [ExpectedException(typeof(TransferStatusException))]
    public void Aborted_Transfer_Should_Hit_While_Reading_Even_If_We_Try_To_Read_In_One_Buffer()
    {
      //behind the scenes, many blocks are read, independent of the buffer size. Accordingly,
      //the transfer will abort as soon as we want it
      string resourceId = SourceFileInfo.FullName;
     
      using (Stream stream = DownloadHandler.ReadFile(resourceId))
      {
        //we need to get the token in order to get the transfer that was created internally
        var token = GetToken(resourceId);

        byte[] buffer = new byte[1234];
        stream.Read(buffer, 0, buffer.Length);
        int r = stream.Read(buffer, 1000, 234);
        Assert.AreEqual(234, r);

        //abort the transfer
        DownloadHandler.CancelTransfer(token.TransferId, AbortReason.ClientAbort);

        //try to read again
        stream.Read(buffer, 0, buffer.Length);
      }
    }



    [Test]
    public void Aborted_Transfer_Should_Unlock_Immediately()
    {
      //behind the scenes, many blocks are read, independent of the buffer size. Accordingly,
      //the transfer will abort as soon as we want it
      string resourceId = SourceFileInfo.FullName;

      using (Stream stream = DownloadHandler.ReadFile(resourceId))
      {
        //we need to get the token in order to get the transfer that was created internally
        var token = GetToken(resourceId);

        byte[] buffer = new byte[1234];
        stream.Read(buffer, 0, buffer.Length);
        int r = stream.Read(buffer, 1000, 234);
        Assert.AreEqual(234, r);

        //abort the transfer
        DownloadHandler.CancelTransfer(token.TransferId, AbortReason.ClientAbort);

        Assert.AreEqual(ResourceLockState.Unlocked, provider.LockRepository.GetLockState(resourceId));
      }
    }



    [Test]
    public void Getting_Stream_Should_Create_Token_For_Resource()
    {
      string resourceId = SourceFileInfo.FullName;
      using(Stream stream = DownloadHandler.ReadFile(resourceId))
      {
        Assert.IsNotNull(GetToken(resourceId));
      }
    }


    [Test]
    public void Getting_The_Stream_Should_Lock_File()
    {
      string resourceId = SourceFile.FullName.ToLowerInvariant();

      var state = LockRepository.GetLockState(resourceId);
      Assert.AreEqual(ResourceLockState.Unlocked, state);

      using (Stream stream = DownloadHandler.ReadFile(resourceId))
      {
        state = LockRepository.GetLockState(resourceId);
        Assert.AreEqual(ResourceLockState.ReadOnly, state);
      }
    }


    [Test]
    public void Closing_Stream_Should_Unlock_File()
    {
      string resourceId = SourceFile.FullName.ToLowerInvariant();
      using (Stream stream = DownloadHandler.ReadFile(resourceId))
      {

        byte[] buffer = new byte[6000];
        int read = stream.Read(buffer, 0, buffer.Length);
        Assert.AreEqual(buffer.Length, read);

        Assert.AreEqual(ResourceLockState.ReadOnly, provider.LockRepository.GetLockState(resourceId));

        stream.Close();
      }

      Assert.AreEqual(ResourceLockState.Unlocked, provider.LockRepository.GetLockState(resourceId));
    }


  }

}
