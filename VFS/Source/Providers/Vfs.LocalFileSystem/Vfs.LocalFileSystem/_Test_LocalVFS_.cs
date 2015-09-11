using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vfs.Util;

namespace Vfs.LocalFileSystem
{
    class _Test_LocalVFS_
    {
        static void Main(string[] args) {
            LocalFileSystemProvider lp = new LocalFileSystemProvider(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory),true);
           bool exfd= lp.ExistFolder("LocalVFS",true);
           if (exfd) {
               lp.DeleteFolder("LocalVFS");
           }
           byte[] dataTest = Encoding.UTF8.GetBytes("this is a test context!!!");
            File.WriteAllBytes("test.cs",dataTest);
            lp.CreateFolder("LocalVFS");
           string filepath= lp.CreateFilePath("LocalVFS", "test.txt");
           
           //lp.MoveFile("test.cs", filepath);
           byte[] dataTest2 = Encoding.UTF8.GetBytes("this is a test write data!!!");
           using (MemoryStream ms = new MemoryStream(dataTest2)) {
               lp.WriteFile("LocalVFS/test.txt",ms, true, dataTest2.Length, ContentUtil.UnknownContentType);
           }
           //lp.DeleteFile("LocalVFS/test.txt");
           lp.CopyFolder("LocalVFS", "localvfs_test");
           lp.Dispose();
           int jj = 0;
        }
        
        
    }
}
