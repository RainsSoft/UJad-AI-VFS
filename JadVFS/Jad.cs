using System;
using System.Collections.Generic;
//using System.Linq;
using System.IO;
#if !UNITY_EDITOR
    using UnityEngine;
#endif

namespace JadEngine.VFS {
    /// <summary>
    /// 用于不再修改的打包文件读取
    /// </summary>
    public sealed class Jad {
        /// <summary>
        /// The Virtual File System
        /// </summary>
        private static JVFS _vfs;
        /// <summary>
        /// Gets access to the Virtual File System
        /// </summary>
        public static JVFS VFS {
            get { return _vfs; }
        }
        /// <summary>
        /// the virtual root dirpath
        /// </summary>
        public static string startupDir = "";
        static Jad() {
            string DatabaseName = "JadVFS";
#if UNITY_EDITOR
            var dbPath = string.Format(@"Assets/StreamingAssets/{0}", DatabaseName);
#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID 
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#elif UNITY_WINRT
			var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
			// then save to Application.persistentDataPath
			File.Copy(loadDb, filepath);
#endif

            Debug.Log("Database written");
        }

        var dbPath = filepath;
#endif
            startupDir = dbPath;
            _vfs = new JVFS("MainVFS", startupDir);
        }
    }
}