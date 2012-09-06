using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;
using System.Xml;
using System.Windows.Forms;
namespace WikiHistory.HelpFunctions
{
 
  class RevisionsFetcher
  {
    private string baseUrl = "http://en.wikipedia.org/w/";
    /// <summary>
    /// The base URL for the suggestions (location of api.php, e.g. "http://en.wikipedia.org/w/")
    /// </summary>
    public string BaseURL
    {
      get { return baseUrl; }
      set { baseUrl = value; }
    }

    public RevisionsFetcher(string baseUrl)
    {
      this.baseUrl = baseUrl;
      Reset();
    }

    public void Reset()
    {
      rvstartid = 0;
      ready = false;
    }

    public bool ready;
    private Int64 rvstartid;
    public List<Revision> getRevisions(string title)
    {
      // prepare
      string url = this.baseUrl + "api.php?action=query&prop=revisions&titles=" + HttpUtility.UrlEncode(title) + "&rvlimit=500&format=xml&rvprop=ids|timestamp|flags|comment|user|size";
      if (rvstartid > 0) url += "&rvstartid=" + this.rvstartid.ToString();

      // load
      WebClient client1 = new WebClient();
      XmlDocument xmlDoc = new XmlDocument();
      List<Revision> revisions = new List<Revision>();
      client1.Encoding = Encoding.UTF8;
      client1.Headers.Add("User-Agent", "WikiHistory (http://de.wikipedia.org/wiki/Benutzer:APPER/WikiHistory) [he]");

      try { xmlDoc.LoadXml( client1.DownloadString(url)); }
      catch  { return revisions; }
      XmlNode xmlRevisions = xmlDoc.DocumentElement;
      XmlNode currentNode;

      

      this.ready = true;

      // query
      currentNode = xmlRevisions.SelectSingleNode("query");
      if (currentNode == null) return revisions;
      currentNode = currentNode.SelectSingleNode("pages");
      if (currentNode == null) return revisions;
      currentNode = currentNode.SelectSingleNode("page");
      if (currentNode == null) return revisions;
      if (currentNode.Attributes.GetNamedItem("missing") != null) return revisions;
      currentNode = currentNode.SelectSingleNode("revisions");
      if (currentNode == null) return revisions;

      foreach (XmlNode rev in currentNode.SelectNodes("rev"))
      {
        Revision newRevision = new Revision();
        XmlNode attribute;
        
        attribute = rev.Attributes.GetNamedItem("revid");
        if (attribute != null) { try { newRevision.id = Convert.ToInt64(attribute.Value); } catch { newRevision.id = -1; } }
        attribute = rev.Attributes.GetNamedItem("minor");
        if (attribute != null) { newRevision.minor = true; }
        attribute = rev.Attributes.GetNamedItem("user");
        if (attribute != null) { newRevision.user = attribute.Value; }
        attribute = rev.Attributes.GetNamedItem("anon");
        if (attribute != null) { newRevision.anon = true; }
        attribute = rev.Attributes.GetNamedItem("timestamp");
        if (attribute != null) { newRevision.timestamp = ConvertTimestampToDateTime(attribute.Value); }
        attribute = rev.Attributes.GetNamedItem("comment");
        if (attribute != null) { newRevision.comment = attribute.Value; }
        attribute = rev.Attributes.GetNamedItem("size");
        if (attribute != null) { newRevision.size = Convert.ToInt32(attribute.Value); }

        revisions.Add(newRevision);
      }

      // query-continue
      currentNode = xmlRevisions.SelectSingleNode("query-continue");
      if (currentNode != null)
      {
        this.ready = false;
        currentNode = currentNode.SelectSingleNode("revisions");
        if (currentNode != null)
        {
          this.rvstartid = Convert.ToInt64(currentNode.Attributes.GetNamedItem("rvcontinue").Value);
        }
      }

      return revisions;
    }

    private static DateTime ConvertTimestampToDateTime(string timestamp)
    {
      Regex r = new Regex(@"^([0-9]{4})\-([0-9]{2})\-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})Z");
      Match m = r.Match(timestamp);

      if (!m.Success)
      {
        return new DateTime(0, 0, 0, 0, 0, 0);
      }

      int year, month, day, hour, minute, second;
      try { year = Convert.ToInt32(m.Groups[1].Value); }
      catch { year = 0; }
      try { month = Convert.ToInt32(m.Groups[2].Value); }
      catch { month = 0; }
      try { day = Convert.ToInt32(m.Groups[3].Value); }
      catch { day = 0; }
      try { hour = Convert.ToInt32(m.Groups[4].Value); }
      catch { hour = 0; }
      try { minute = Convert.ToInt32(m.Groups[5].Value); }
      catch { minute = 0; }
      try { second = Convert.ToInt32(m.Groups[6].Value); }
      catch { second = 0; }
      return new DateTime(year, month, day, hour, minute, second);
    }
  }
}
