namespace WikiHistory.HelpFunctions
{
  using System;
  using System.Collections.Generic;
  using System.Net;
  using System.Web;
  using System.Xml;
  using System.Text;
  using System.Text.RegularExpressions;
  
  /// <summary>
  /// This class implements simple fetching of suggestions for AJAX style search.
  /// 
  /// It has the following limitations:
  /// * currently only working for namespace 0 (articles)
  /// </summary>
  class SuggestionFetcher
  {
    public class Suggestion
    {
      public string title;
    }

    public bool cancelLoading = false;

    private string baseUrl = "http://en.wikipedia.org/w/";

    /// <summary>
    /// The base URL for the suggestions (location of api.php, e.g. "http://en.wikipedia.org/w/")
    /// </summary>
    public string BaseURL
    {
      get { return baseUrl; }
      set { baseUrl = value; }
    }

    private int maxResults = 15;

    /// <summary>
    /// Maximum results for each query (between 1 and 100)
    /// </summary>
    public int MaxResults
    {
      get { return maxResults; }
      set 
      {
        if ((maxResults > 0) && (maxResults <= 100))
          maxResults = value;
        else
          throw new ArgumentOutOfRangeException("MaxResults", "MaxResults is limited to values from 1 to 100");
      }
    }

    public SuggestionFetcher(string baseUrl, int maxResults)
    {
      this.baseUrl = baseUrl;
      this.MaxResults = maxResults;
    }

    bool downloading = false;
    string downloadedString = "";
    private void HandleDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
      if ((e.Error == null) && (!e.Cancelled))
      {
        downloadedString = (string)e.Result;
        downloading = false;
      }
    }

    public List<Suggestion> getSuggestions(string search)
    {
      cancelLoading = false;

      // load
      WebClient client1 = new WebClient();
      XmlDocument xmlDoc = new XmlDocument();
      client1.Encoding = Encoding.UTF8;
      client1.Headers.Add("User-Agent", "WikiHistory (http://de.wikipedia.org/wiki/Benutzer:APPER/WikiHistory)");
      try
      {
        DateTime start = DateTime.Now;
        downloading = true;
        downloadedString = "";
        client1.DownloadStringCompleted += HandleDownloadStringCompleted;
        client1.DownloadStringAsync(new Uri(baseUrl + "api.php?action=opensearch&search=" + HttpUtility.UrlEncode(search) + "&namespace=0&limit=" + maxResults.ToString()));
        while (downloading)
        {
          TimeSpan execTime = DateTime.Now.Subtract(start);
          if ((cancelLoading) || (execTime.TotalSeconds > 2))
          {
            client1.CancelAsync();
            return new List<Suggestion>();
          }
        }
        if (downloadedString == "")
          return new List<Suggestion>();
        //xmlDoc.LoadXml(downloadedString);
      }
      catch
      {
        return new List<Suggestion>();
      }

      Regex r = new Regex("\"([^\"]+)\"", RegexOptions.Compiled);
      MatchCollection mc = r.Matches(downloadedString);

      Regex unicodeReg = new Regex(@"\\u([0-9ABCDEF]{2})([0-9ABCDEF]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
      Decoder unicodeDecoder = Encoding.Unicode.GetDecoder();

      List<Suggestion> suggestions = new List<Suggestion>();
      Match m;
      for (int i = 1; i < mc.Count; i++)
      {
        m = mc[i];
        Suggestion newSuggestion = new Suggestion();
        string title = m.Groups[1].Value;
        MatchCollection uc = unicodeReg.Matches(title);
        foreach (Match u in uc)
        {
          byte[] unicodeBytes = new byte[2];
          char[] unicodeChars = new char[1];
          unicodeBytes[0] = Byte.Parse(u.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
          unicodeBytes[1] = Byte.Parse(u.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
          int i1, i2;
          bool b1;
          unicodeDecoder.Convert(unicodeBytes, 0, 2, unicodeChars, 0, 1, true, out i1, out i2, out b1);
          title = title.Replace(u.Groups[0].Value, unicodeChars[0].ToString());
        }

        newSuggestion.title = title;
        suggestions.Add(newSuggestion);
      }


      //XmlNode xmlSuggestions = xmlDoc.DocumentElement.ChildNodes[1];
      //if (xmlSuggestions == null)
      //  return new List<Suggestion>();

      //// prepare collection
      //List<Suggestion> suggestions = new List<Suggestion>();
      //foreach (XmlNode article in xmlSuggestions.ChildNodes)
      //{
      //  Suggestion newSuggestion = new Suggestion();
      //  foreach (XmlNode element in article)
      //  {
      //    if (element.Name == "Text")
      //      newSuggestion.title = element.InnerText;
      //    else if (element.Name == "Description")
      //      newSuggestion.description = element.InnerText;
      //    else if (element.Name == "Url")
      //      newSuggestion.url = element.InnerText;
      //    else if (element.Name == "Image")
      //    {
      //      newSuggestion.image = element.Attributes.GetNamedItem("source").Value;
      //      try
      //      {
      //        newSuggestion.imageWidth = Convert.ToInt16(element.Attributes.GetNamedItem("width").Value);
      //        newSuggestion.imageHeight = Convert.ToInt16(element.Attributes.GetNamedItem("height").Value);
      //      }
      //      catch
      //      {
      //        newSuggestion.image = string.Empty;
      //      }
      //    }
      //  }
      //  suggestions.Add(newSuggestion);
      //}
      return suggestions;
    }
  }
}
