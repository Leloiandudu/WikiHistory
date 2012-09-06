using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace WikiHistory.HelpFunctions
{
  /// <summary>
  /// This class helps to export the colored text to HTML. It is created with the text
  /// and then you're able to set parts to be surrounded by a given tag
  /// </summary>
  class HTMLHelper
  {
    string text = "";

    public HTMLHelper(string text)
    {
      this.text = text;
    }

    private class Tag : IComparable
    {
      public int pos = 0;
      public string tag = "";
      public Tag(int pos, string tag)
      {
        this.pos = pos;
        this.tag = tag;
      }
      public int CompareTo(object other)
      {
        return this.pos.CompareTo(((Tag)other).pos);
      }
    }
    private List<Tag> tags = new List<Tag>();

    public void SetTag(int start, int length, string starttag, string endtag)
    {
      tags.Add(new Tag(start, starttag));
      tags.Add(new Tag(start + length, endtag));
    }

    public string GetHTML()
    {
      tags.Sort(delegate(Tag t1, Tag t2) { return -1 * t1.CompareTo(t2); });
      string html = text;
      for (int i = 0; i < tags.Count; i++)
      {
        html = html.Substring(0, tags[i].pos) + tags[i].tag + html.Substring(tags[i].pos);
      }
      return html;
    }

    public static string ColorToRGB(Color c)
    {
      return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }
  }
}
