using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hardcodet.Commons;
using Slf;

namespace Vfs.Test
{
  public class SlfGuard : Guard
  {
    private IFactoryResolver resolver = LoggerService.FactoryResolver;

    public SlfGuard(ILogger logger)
    {
      LoggerService.SetLogger(logger);

      //cleanup action
      DisposalAction = () => LoggerService.FactoryResolver = resolver;
    }


    public SlfGuard(Action disposeAction) : base(disposeAction)
    {
      throw new InvalidOperationException("The guard creates its own action...");
    }
  }
}