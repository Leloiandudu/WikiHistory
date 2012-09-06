using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

namespace WikiHistory
{
  class AutoUpdate
  {
    public static void IsNewVersion()
    {
      IsNewVersion(false);
    }
    public static void IsNewVersion(bool ShowMessageWhenNoNewVersionAvailable)
    {
        return;
      string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

      WebClient client = new WebClient();
      client.Headers.Add("User-Agent", "WikiHistory (http://de.wikipedia.org/wiki/Benutzer:APPER/WikiHistory)");
      string answer="";
        try
      {
         answer = client.DownloadString("http://toolserver.org/~apper/WikiHistory/update.php?currentversion=" + version);
      }
      catch (Exception e) { }
      if (answer.Trim().Length > 0)
      {
        XmlDocument doc = new XmlDocument();
        try
        {
          doc.LoadXml(answer);
          string newVersion = doc.SelectSingleNode("/descendant-or-self::newversion/child::number").InnerText;
          string newVersionUrl = doc.SelectSingleNode("/descendant-or-self::newversion/child::url").InnerText;

          string updateMessage = "A new version of WikiHistory is available." + Environment.NewLine + Environment.NewLine;
          updateMessage += "The new version is " + newVersion + " (current version is " + version + "). Dou you want to update?" + Environment.NewLine + Environment.NewLine;
          updateMessage += "If you chose \"No\" you won't be asked again, in this case select \"Check for Updates...\" from the Help menu to manually check for updates in the future.";

          DialogResult dr = MessageBox.Show(updateMessage, Program.ProgramName, MessageBoxButtons.YesNoCancel);
          if (dr == DialogResult.Yes)
          {
            Process.Start(newVersionUrl);
          }
          else if (dr == DialogResult.No)
          {
            // do not ask for this update again
            Properties.Settings.Default.AutoUpdateThisVersion = false;
            Properties.Settings.Default.Save();
          }
          else // Cancel
          {
            // do nothing
          }
        }
        catch 
        {
          if (ShowMessageWhenNoNewVersionAvailable)
            MessageBox.Show("Error. Could not check for newer version, please try again later.", Program.ProgramName);
        }
      }
      else if (ShowMessageWhenNoNewVersionAvailable)
      {
        MessageBox.Show("You already have the most current version.", Program.ProgramName);
      }
    }

    public static void CheckForUpdateBlocking()
    {
      Cursor.Current = Cursors.WaitCursor;
      IsNewVersion(true);
      Cursor.Current = Cursors.Default;
    }

    public static void CheckForUpdate()
    {
      Thread t = new Thread(new ThreadStart(IsNewVersion));
      t.Start();
    }

  }
}
