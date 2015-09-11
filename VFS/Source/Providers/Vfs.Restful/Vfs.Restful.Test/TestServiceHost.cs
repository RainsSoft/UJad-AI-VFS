using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Hosting.HttpListener;

namespace Vfs.Restful.Test
{
  public class TestServiceHost : HttpListenerHost
  {
    public IConfigurationSource Configuration { get; set; }

    public override bool ConfigureRootDependencies(IDependencyResolver resolver)
    {
      var result = base.ConfigureRootDependencies(resolver);
      if (result && Configuration != null) resolver.AddDependencyInstance(typeof (IConfigurationSource), Configuration, DependencyLifetime.PerRequest);

      return result; 
    }
  }
}
