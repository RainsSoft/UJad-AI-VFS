#region License
/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */
#endregion

using System;
using System.IO;
using OpenRasta.IO;
using RESTful_Filesystem.Resources;

namespace RESTful_Filesystem.Handlers
{
  public class HomeHandler
  {



    public object Get(string filePath)
    {
      return new MemoryStream(new byte[2048]); // HomeResource() {Message = "hello world"};
    }
  }
}
