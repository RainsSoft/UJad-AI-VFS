using System;
using System.Collections.Generic;
using System.Linq;
using Ionic.Zip;

namespace Vfs.Zip
{
  /// <summary>
  /// Represents a file within a ZIP file.
  /// </summary>
  public class ZipFileItem : VirtualFileItem
  {
    /// <summary>
    /// Provides a hiearchical view on the file entry within
    /// the ZIP file.
    /// </summary>
    public ZipNode Node { get; set; }


    /// <summary>
    /// Indicates whether the resource physically exists on the file system
    /// or not.
    /// </summary>
    public override bool Exists
    {
      get { return Node.FileEntry != null; }
    }

    /// <summary>
    /// Gets a string that provides the fully qualified string of the resource (as opposite to the
    /// <see cref="VirtualResourceInfo.FullName"/>, which is publicly exposed to
    /// clients, e.g. in exception messages).<br/>
    /// It should be ensured that this identifier always looks the same for different requests,
    /// as it is being used for internal processes such as resource locking or auditing.
    /// </summary>
    public override string QualifiedIdentifier
    {
      get { return Node.FullName; }
    }


       
    public ZipFileItem(ZipNode node, VirtualFileInfo virtualFile)
    {
      Node = node;
      ResourceInfo = virtualFile;
    }
  }
}
