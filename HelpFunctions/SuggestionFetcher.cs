namespace WikiHistory.HelpFunctions
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Web;
  using System.Xml;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Runtime.Serialization.Json;
  using System.Xml.Linq;
  
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

      var reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(downloadedString), new XmlDictionaryReaderQuotas());
      var root = XElement.Load(reader);
      return (
          from item in root.Elements("item").ElementAt(1).Elements("item")
          let type = item.Attribute("type")
          where type != null && type.Value == "string"
          select new Suggestion { title = item.Value }
      ).ToList();
    }
  }
}
