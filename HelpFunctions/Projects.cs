using System;
using System.Collections.Generic;
using System.Text;

namespace WikiHistory.HelpFunctions
{
  static class Projects
  {
    public class ProjectLanguage
    {
      public string shortname = "";
      public string originalname = "";
      public string baseUrl = "";
      

      public ProjectLanguage(string shortname, string originalname, string baseUrl)
      {
        this.shortname = shortname;
        this.originalname = originalname;
        this.baseUrl = baseUrl;
      }
    }

    public class Project
    {
      public string shortname = "";
      public string longname = "";
      public List<ProjectLanguage> languages = new List<ProjectLanguage>();
      public string baseUrl = "";
      public string standardLanguage;
      public string adding = "/w/";
    }

    public static List<Project> projects = new List<Project>();
    public static string currentProjectBaseUrl = "http://en.wikipedia.org/w/";
    public static string currentProjectSaveName = "en.wikipedia";

    static Projects()
    {
      Project newProject;

      // TODO: fill this automatically from http://en.wikipedia.org/w/api.php?action=sitematrix

      // Wikipedia
      newProject = new Project();
      newProject.shortname = "wikipedia"; // not wiki
      newProject.longname = "Wikipedia";
      #region "Wikipedia" languages
      newProject.languages.Add(new ProjectLanguage("de", "Deutsch", "http://de.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("en", "English", "http://en.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("es", "Español", "http://en.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("fr", "Français", "http://fr.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("it", "Italiano", "http://it.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("ja", "日本語", "http://ja.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("pl", "Polski", "http://pl.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("pt", "Português", "http://pt.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("ru", "Русский", "http://ru.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("zh", "中文", "http://zh.wikipedia.org"));
      #endregion
      newProject.standardLanguage = "en"; // English
      projects.Add(newProject);

      // Commons
      newProject = new Project();
      newProject.shortname = "commons";
      newProject.longname = "Wikimedia Commons";
      newProject.baseUrl = "http://commons.wikimedia.org";
      projects.Add(newProject);
        

    }
  }
}
