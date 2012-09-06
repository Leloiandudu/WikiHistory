using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using ExceptionHandling;

namespace WikiHistory
{
  static class Program
  {
    public static string ProgramName = "WikiHistory";
    private static string ErrorBaseUrl = "http://www.apper.de/programError.php";
    public static bool Mono = false;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      if (Type.GetType("Mono.Runtime") != null)
        Mono = true;
      GlobalExceptionHandling();
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      try
      {
        Application.Run(new MainForm());
      }
      catch (Exception e)
      {
        // this one catches the exceptions in MONO
        ExceptionHandler eh = new ExceptionHandler();
        eh.InitializeWindow(Program.ProgramName, e, ErrorBaseUrl);
        eh.ShowDialog();
      }
    }

    public static void GlobalExceptionHandling()
    {
      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      // this one catches the exceptions in .NET
      ExceptionHandler eh = new ExceptionHandler();
      eh.InitializeWindow(Program.ProgramName, e.Exception, ErrorBaseUrl);
      eh.ShowDialog();
    }
  }
}
