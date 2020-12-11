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
    public static string currentProjectBaseUrl = "https://en.wikipedia.org/w/";
    public static string currentProjectSaveName = "en.wikipedia";

    static Projects()
    {
      Project newProject;

      // TODO: fill this automatically from https://en.wikipedia.org/w/api.php?action=sitematrix

      // Wikipedia
      newProject = new Project();
      newProject.shortname = "wikipedia"; // not wiki
      newProject.longname = "Wikipedia";
      #region "Wikipedia" languages
      newProject.languages.Add(new ProjectLanguage("de", "Deutsch", "https://de.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("en", "English", "https://en.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("es", "Español", "https://es.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("fr", "Français", "https://fr.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("it", "Italiano", "https://it.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("ja", "日本語", "https://ja.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("pl", "Polski", "https://pl.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("pt", "Português", "https://pt.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("ru", "Русский", "https://ru.wikipedia.org"));
      newProject.languages.Add(new ProjectLanguage("zh", "中文", "https://zh.wikipedia.org"));
      #endregion
      newProject.standardLanguage = "en"; // English
      projects.Add(newProject);

      // Commons
      newProject = new Project();
      newProject.shortname = "commons";
      newProject.longname = "Wikimedia Commons";
      newProject.baseUrl = "https://commons.wikimedia.org";
      projects.Add(newProject);
        

    }
  }
}
