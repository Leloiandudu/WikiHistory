using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Drawing;

namespace WikiHistory.HelpFunctions
{
  public class Revision : IComparable
  {
    public static string currentTitle = "";

    public long id = -1;
    public bool minor = false;
    public string user;
    public bool anon = false;
    public DateTime timestamp;
    public string comment = string.Empty;
    public int size = -1;

    public Color displayColor = SystemColors.Window;
    public bool selected = false;

    public bool fullTextLoaded = false;
    public string fullText = string.Empty;

    int IComparable.CompareTo(object obj)
    {
      Revision otherRevision = (Revision)obj;
      if (this.timestamp > otherRevision.timestamp)
        return 1;
      if (this.timestamp < otherRevision.timestamp)
        return -1;
      else
        return 0;
    }

    public void SetFullText(String text)
    {
        this.fullText = text;
      /*  if (Properties.Settings.Default.CacheRevisions)
        {
            String tmp = this.fullText;
            this.fullText = Cache.Compress(this.fullText);
            this.SaveToDisk();
            this.fullText = tmp;
        }*/
        this.fullTextLoaded = true;
    }

    public void LoadFullText()
    {
        if (this.fullTextLoaded) { return;  }
       
      // check, if text is available offline
      string cacheFile = Cache.getFilePath(this.id.ToString());
      if ((cacheFile != "") && (File.Exists(cacheFile)))
      {
          StreamReader sr = new StreamReader(Cache.getFilePath(this.id.ToString()));
          this.fullText = Cache.Decompress(sr.ReadToEnd());
        this.fullTextLoaded = true;
        
        return;
      }
       
      string url = Projects.currentProjectBaseUrl + "index.php?title=" + WikiHelpFunctions.TitleToUrlTitle(currentTitle) + "&oldid=" + this.id.ToString() + "&action=raw";
      WebClient client1 = new WebClient();
      client1.Encoding = Encoding.UTF8;
      
      client1.Headers.Add("User-Agent", "WikiHistory (http://de.wikipedia.org/wiki/Benutzer:APPER/WikiHistory) [he]");
      this.fullTextLoaded = true;
      try
      {
        this.fullText = client1.DownloadString(url);
        if (Properties.Settings.Default.CacheRevisions)
        {
            String tmp = this.fullText;
            this.fullText = Cache.Compress(this.fullText);
            this.SaveToDisk();
            this.fullText = tmp;
        }
      }
      catch
      {
        this.fullText = "";
        this.fullTextLoaded = false;
      }
    }

    public void SaveToDisk()
    {
      if (!this.fullTextLoaded) return;
      StreamWriter sw = new StreamWriter(Cache.getFilePath(this.id.ToString()), false, Encoding.UTF8);
      sw.Write(this.fullText);
      sw.Close();
      Cache.deleteCacheFiles();
    }
  }
}
