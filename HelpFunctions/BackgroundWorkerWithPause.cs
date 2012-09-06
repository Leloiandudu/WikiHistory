using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace WikiHistory.HelpFunctions
{
  public class BackgroundWorkerWithPause : BackgroundWorker
  {
    private volatile bool paused = false;
    public bool Paused
    {
      get { return paused; }
      set { paused = value; }
    }
  }
}
