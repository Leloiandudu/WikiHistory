using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WikiHistory.HelpFunctions;
using System.Globalization;
using System.Threading;
using WikiHistory.AdditionalForms;

namespace WikiHistory
{
  public partial class MainForm
  {
    #region "Projects" menu
    private void loadSelectedProject()
    {
      #region search Project/Language
      Projects.Project selectedProject = null; ;
      Projects.ProjectLanguage selectedLanguage = null;
      foreach (Projects.Project p in Projects.projects)
      {
        if (p.shortname == Properties.Settings.Default.Project)
        {
          selectedProject = p;
          if (p.languages.Count > 0)
          {
            foreach (Projects.ProjectLanguage lang in p.languages)
            {
              if (lang.shortname == Properties.Settings.Default.ProjectLanguage)
              {
                selectedLanguage = lang;
                break;
              }
            }
          }
          break;
        }
      }
      if ((selectedProject != null) && (selectedProject.languages.Count == 0)) selectedLanguage = null;
      if (selectedProject == null) selectedProject = Projects.projects[0]; // Wikipedia
      if ((selectedProject.languages.Count > 0) && (selectedLanguage == null))
      {
        selectedLanguage = selectedProject.languages[0]; // some (random) standard!
        // search for standard language
        foreach (Projects.ProjectLanguage lang in selectedProject.languages)
        {
          if (lang.shortname == selectedProject.standardLanguage)
          {
            selectedLanguage = lang;
            break;
          }
        }
        // maybe the system is in another language
        CultureInfo ci = Thread.CurrentThread.CurrentCulture;
        string localLanguage = ci.TwoLetterISOLanguageName;
        foreach (Projects.ProjectLanguage lang in selectedProject.languages)
        {
          if (lang.shortname == localLanguage)
          {
            selectedLanguage = lang;
            break;
          }
        }
      }
      // save
      Properties.Settings.Default.Project = selectedProject.shortname;
      if (selectedLanguage != null)
        Properties.Settings.Default.ProjectLanguage = selectedLanguage.shortname;
      else
        Properties.Settings.Default.ProjectLanguage = "";
      Properties.Settings.Default.Save();
      #endregion

      // baseUrl
      if (selectedProject.languages.Count == 0)
        Projects.currentProjectBaseUrl = selectedProject.baseUrl;
      else
        Projects.currentProjectBaseUrl = selectedLanguage.baseUrl;
      Projects.currentProjectBaseUrl += selectedProject.adding;//"/w/";

      suggestionFetcher = new SuggestionFetcher(Projects.currentProjectBaseUrl, 30);
      revisionsFetcher = new RevisionsFetcher(Projects.currentProjectBaseUrl);

      // show name
      Projects.currentProjectSaveName = "";
      if (selectedLanguage != null) Projects.currentProjectSaveName = selectedLanguage.shortname + ".";
      Projects.currentProjectSaveName += selectedProject.shortname;
      labelProject.Text = "Project: " + Projects.currentProjectSaveName;

      #region show in menu
      // remove all checked
      foreach (ToolStripMenuItem m in menuProject.DropDownItems)
      {
        m.Checked = false;
        foreach (ToolStripMenuItem m2 in m.DropDownItems)
          m2.Checked = false;
      }

      // set checked
      foreach (ToolStripMenuItem m in menuProject.DropDownItems)
      {
        Projects.Project project = ((Projects.Project)m.Tag);
        if (project == selectedProject)
        {
          if (project.languages.Count == 0)
            m.Checked = true;
          else
          {
            foreach (ToolStripMenuItem m2 in m.DropDownItems)
            {
              object[] properties = (object[])m2.Tag;
              Projects.ProjectLanguage lang = (Projects.ProjectLanguage)(properties[1]);
              if (lang == selectedLanguage)
              {
                m2.Checked = true;
                break;
              }
            }
          }
          break;
        }
      }

      #endregion
    }


    private void createProjectMenu()
    {
      foreach (Projects.Project p in Projects.projects)
      {
        ToolStripMenuItem m = new ToolStripMenuItem();
        m.Text = p.longname;
        m.Name = "menuProject_" + p.shortname;
        m.Tag = p;
        if (p.languages.Count > 0)
        {
          foreach(Projects.ProjectLanguage lang in p.languages)
          {
            ToolStripMenuItem m2 = new ToolStripMenuItem();
            m2.Text = lang.originalname;
            m2.Name = "menuProject_" + p.shortname + "_" + lang.shortname;
            m2.Tag = new object[] { p, lang };
            m2.Click += new System.EventHandler(menuProjectLanguage_Click);
            m.DropDownItems.AddRange(new ToolStripItem[] { m2 });
          }
        }
        else
        {
          m.Click += new System.EventHandler(menuProject_Click);
        }
        menuProject.DropDownItems.AddRange(new ToolStripItem[] { m });
      }
    }

    #region event handler
    private void menuProject_Click(object sender, EventArgs e)
    {
      string project = ((Projects.Project)(((ToolStripMenuItem)sender).Tag)).shortname;
      Properties.Settings.Default.ProjectLanguage = "";
      Properties.Settings.Default.Project = project;
      Properties.Settings.Default.Save();
      loadSelectedProject();
    }
    private void menuProjectLanguage_Click(object sender, EventArgs e)
    {
      object[] properties = (object[])((ToolStripMenuItem)sender).Tag;
      Projects.Project project = (Projects.Project)(properties[0]);
      Projects.ProjectLanguage lang = (Projects.ProjectLanguage)(properties[1]);

      Properties.Settings.Default.ProjectLanguage = lang.shortname;
      Properties.Settings.Default.Project = project.shortname;
      Properties.Settings.Default.Save();
      loadSelectedProject();
    }
    #endregion
    #endregion

    // ------------------------------------------------------------------------
    // normal menus

    #region "File" menu
    private void MenuFileExit_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }
    #endregion

    #region "Settings" menu
    private void menuSettingsCache_Click(object sender, EventArgs e)
    {
      CacheSettings cs = new CacheSettings();
      cs.ShowDialog();
    }
    #endregion

    #region "?" menu
    private void menuHelpCheckUpdates_Click(object sender, EventArgs e)
    {
      AutoUpdate.CheckForUpdateBlocking();
    }

    private void MenuHelpAbout_Click(object sender, EventArgs e)
    {
      MessageBox.Show("WikiHistory" + Environment.NewLine + "© 2008–2010 Christian Thiele ([[de:User:APPER]])"+Environment.NewLine+"(c) 2011 Haffman edition ([[ru:User:Haffman]]) ", "About WikiHistory");
    }
    #endregion

    // ------------------------------------------------------------------------
    // context menus
    #region context menu for author textbox
    private void CopyAuthorsToClipboard(bool wiki)
    {
      string html = getAuthorsColoredHTML(wiki);
      if (html != "")
      {
        try
        {
          Clipboard.SetText(html);
        }
        catch
        {
          // second try!
          Thread.Sleep(100);
          try
          {
            Clipboard.SetText(html);
          }
          catch
          {
            MessageBox.Show("Copying Text to clipboard failed. Eventually another application blocks the clipboard.", Program.ProgramName);
          }
        }
      }
      else
        MessageBox.Show("Please load article text first.", Program.ProgramName);
    }
    private void cMenuCopyAsHtml_Click(object sender, EventArgs e)
    {
      CopyAuthorsToClipboard(false);
    }
    private void cMenuCopyAsWikiText_Click(object sender, EventArgs e)
    {
      CopyAuthorsToClipboard(true);
    }
    #endregion
  }
}
