using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Windows.Forms;

namespace WikiHistory.HelpFunctions
{
  public class Cache
  {
      public static string Compress(string text)
      {
          byte[] buffer = Encoding.UTF8.GetBytes(text);
          MemoryStream ms = new MemoryStream();
          using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
          {
              zip.Write(buffer, 0, buffer.Length);
          }

          ms.Position = 0;
          MemoryStream outStream = new MemoryStream();

          byte[] compressed = new byte[ms.Length];
          ms.Read(compressed, 0, compressed.Length);

          byte[] gzBuffer = new byte[compressed.Length + 4];
          System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
          System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
          return Convert.ToBase64String(gzBuffer);
      }
      public static string Compress2(byte[] buffer)
      {
          //byte[] buffer = Encoding.UTF8.GetBytes(text);
          MemoryStream ms = new MemoryStream();
          using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
          {
              zip.Write(buffer, 0, buffer.Length);
          }

          ms.Position = 0;
          MemoryStream outStream = new MemoryStream();

          byte[] compressed = new byte[ms.Length];
          ms.Read(compressed, 0, compressed.Length);

          byte[] gzBuffer = new byte[compressed.Length + 4];
          System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
          System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
          return Convert.ToBase64String(gzBuffer);
      }

      public static string Decompress(string compressedText)
      {
          byte[] gzBuffer = Convert.FromBase64String(compressedText);
          using (MemoryStream ms = new MemoryStream())
          {
              int msgLength = BitConverter.ToInt32(gzBuffer, 0);
              ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

              byte[] buffer = new byte[msgLength];

              ms.Position = 0;
              using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
              {
                  zip.Read(buffer, 0, buffer.Length);
              }

              return Encoding.UTF8.GetString(buffer);
          }
      }
      public static string Decompress2(byte[] gzBuffer)
      {
          //byte[] gzBuffer = Convert.FromBase64String(compressedText);
          using (MemoryStream ms = new MemoryStream())
          {
              int msgLength = BitConverter.ToInt32(gzBuffer, 0);
              ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

              byte[] buffer = new byte[msgLength];
              MessageBox.Show(msgLength + "");

              ms.Position = 0;
              using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
              {
                  zip.Read(buffer, 0, buffer.Length);
              }

              return Encoding.UTF8.GetString(buffer);
          }
      }
      public static string Zip(string value)
      {
          //Transform string into byte[]  
          byte[] byteArray = new byte[value.Length];
          int indexBA = 0;
          foreach (char item in value.ToCharArray())
          {
              byteArray[indexBA++] = (byte)item;
          }

          //Prepare for compress
          System.IO.MemoryStream ms = new System.IO.MemoryStream();
          System.IO.Compression.GZipStream sw = new System.IO.Compression.GZipStream(ms,
              System.IO.Compression.CompressionMode.Compress);

          //Compress
          sw.Write(byteArray, 0, byteArray.Length);
          //Close, DO NOT FLUSH cause bytes will go missing...
          sw.Close();

          //Transform byte[] zip data to string
          byteArray = ms.ToArray();
          System.Text.StringBuilder sB = new System.Text.StringBuilder(byteArray.Length);
          foreach (byte item in byteArray)
          {
              sB.Append((char)item);
          }
          ms.Close();
          sw.Dispose();
          ms.Dispose();
          return sB.ToString();
      }
      public static string UnZip(StreamReader ms)
      {
          //Transform string into byte[]
          //byte[] byteArray = new byte[ms.Length];
          //ms.Read(byteArray, 0, ms.Length);
          /*int indexBA = 0;
          foreach (char item in value.ToCharArray())
          {
              byteArray[indexBA++] = (byte)item;
          }*/

          //Prepare for decompress
         // System.IO.MemoryStream ms = new System.IO.MemoryStream(byteArray);
          System.IO.Compression.GZipStream sr = new System.IO.Compression.GZipStream(ms.BaseStream,
              System.IO.Compression.CompressionMode.Decompress);
          //sr.Position = 0;

          //Reset variable to collect uncompressed result
          String ret = "";

          //Decompress
          //int rByte = sr.Read(byteArray, 0, byteArray.Length);
          using (var srs = new StreamReader(sr))
          {
             
              ret = srs.ReadToEnd();
              srs.Close();
              srs.Dispose();
          }

          sr.Close();
          ms.Close();
          sr.Dispose();
          ms.Dispose();

          return ret;
      }
    public static string getPath()
    {
      string path = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
      path = Path.Combine(path, "WikiHistory");
      try
      {
        Directory.CreateDirectory(path);
        path = Path.Combine(path, "Revisions");
        Directory.CreateDirectory(path);
      }
      catch
      {
        Properties.Settings.Default.CacheRevisions = false;
        Properties.Settings.Default.Save();
        path = "";
      }
      return path;
    }

    public static string getFilePath(string id)
    {
      string path = Cache.getPath();
      if (path == "") return "";
      string filename = Projects.currentProjectSaveName + "." + id + ".rev_gz3";
      path = Path.Combine(path, filename);
      return path;
    }

    private class clsCompareFileInfo : IComparer
    {
      public int Compare(object x, object y)
      {
        return DateTime.Compare(((FileInfo)y).LastWriteTime, ((FileInfo)x).LastWriteTime);
      }
    }

    /// <summary>
    /// deletes all cache files over the cache limit
    /// </summary>
    public static void deleteCacheFiles()
    {
      long allowedSize = Properties.Settings.Default.CacheSize;
      if (allowedSize < 1) return;
      allowedSize = 1024 * 1024 * allowedSize; // bytes

      DirectoryInfo di = new DirectoryInfo(getPath());
      FileInfo[] files = di.GetFiles();
      Array.Sort(files, new clsCompareFileInfo());

      long fileSize = 0;
      foreach (FileInfo fi in files)
      {
        fileSize += fi.Length;
        if (fileSize > allowedSize)
        {
          File.Delete(fi.FullName);
        }
      }
    }
  }

  public class User
  {
    public string name;
    public bool anon = false;
    public DateTime firstEdit;
    public DateTime lastEdit;
    public int nrOfEdits = 0;
    public int nrOfMinorEdits = 0;
    public int lengthOfContentAdded = -1;
    public double percentageOfContentAdded = -1;
  }
}
