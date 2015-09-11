using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Vfs.Util;
using Vfs.Util.TemporaryStorage;

namespace Vfs.Zip
{

        class _Test_ZipVFS_
        {
            static void Main(string[] args) {
               
               string ZipFilePath = Path.Combine("", "TestArchive.zip");
               if (File.Exists(ZipFilePath)) File.Delete(ZipFilePath);
                var zip = new ZipFile(ZipFilePath);
                zip.AddDirectory("LocalVFS", "LocalVFS/");
                zip.Save();
                zip.Dispose();
                //
                ZipFileSystemConfiguration zc = ZipFileSystemConfiguration.CreateDefaultConfig(ZipFilePath,"_tmp_");
                ZipFileProvider lp = new ZipFileProvider(zc);
                bool exfd = lp.ExistFolder("LocalVFS/", true);
                if (exfd) {
                    lp.DeleteFolder("LocalVFS/");
                }
                byte[] dataTest = Encoding.UTF8.GetBytes("this is a test context!!!");
                File.WriteAllBytes("test.cs", dataTest);
                lp.CreateFolder("LocalVFS/");
                string filepath = lp.CreateFilePath("LocalVFS/", "test.txt");

                //lp.MoveFile("test.cs", filepath);
                byte[] dataTest2 = Encoding.UTF8.GetBytes("this is a test write data!!!");
                using (MemoryStream ms = new MemoryStream(dataTest2)) {
                    lp.WriteFile("LocalVFS/test.txt", ms, true, dataTest2.Length, ContentUtil.UnknownContentType);
                }
                //lp.DeleteFile("LocalVFS/test.txt");
                lp.CopyFolder("LocalVFS/", "localvfs_test/");
                lp.Dispose();
                int jj = 0;
            }


        }
    
}
