using System;
using System.Collections.Generic;
using System.Text;

namespace WikiHistory.HelpFunctions
{
  static class WikiHelpFunctions
  {
    public static string TitleToUrlTitle(string title)
    {
      return title.Replace(" ", "_");
    }
  }
}
