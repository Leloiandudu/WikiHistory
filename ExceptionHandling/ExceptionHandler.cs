using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using System.Net;
using System.IO;
using System.Web;
using System.Security.Cryptography;

namespace ExceptionHandling
{
  public partial class ExceptionHandler : Form
  {
    private string errorReport = "";
    private string errorBaseUrl = "";
    private string programName = "";

    public ExceptionHandler()
    {
      InitializeComponent();
    }

    private void ExceptionHandler_Load(object sender, EventArgs e)
    {
      this.Icon = System.Drawing.SystemIcons.Error;
      pictureBox1.BackgroundImage = System.Drawing.SystemIcons.Error.ToBitmap();
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void buttonSend_Click(object sender, EventArgs e)
    {
      string url = errorBaseUrl + "?program=" + HttpUtility.UrlEncode(programName);
      // add md5
      byte[] tmpHash = new MD5CryptoServiceProvider().ComputeHash(ASCIIEncoding.ASCII.GetBytes(errorReport));
      url += "&h=" + ByteArrayToString(tmpHash);

      HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
      request.KeepAlive = false;
      request.ProtocolVersion = HttpVersion.Version10;
      request.Method = "POST";
      request.ContentType = "application/x-www-form-urlencoded";
      byte[] postBytes = ASCIIEncoding.ASCII.GetBytes("error=" + HttpUtility.UrlEncode(errorReport));
      request.ContentLength = postBytes.Length;
      Stream stream = request.GetRequestStream();
      stream.Write(postBytes, 0, postBytes.Length);
      stream.Close();
      HttpWebResponse response = (HttpWebResponse)request.GetResponse();

      Application.Exit();
    }

    static string ByteArrayToString(byte[] arrInput)
    {
      int i;
      StringBuilder sOutput = new StringBuilder(arrInput.Length * 2);
      for (i = 0; i < arrInput.Length; i++)
      {
        sOutput.Append(arrInput[i].ToString("X2"));
      }
      return sOutput.ToString();
    }

    public void InitializeWindow(string programName, Exception e, string baseUrl)
    {
      errorBaseUrl = baseUrl;
      this.programName = programName;
      label2.Text = label2.Text.Replace("$APPNAME$", programName);
      label3.Text = label3.Text.Replace("$APPNAME$", programName);

      StringBuilder sb = new StringBuilder();
      sb.Append("Date and Time:         ");
      sb.AppendLine(DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo));
      sb.Append("Exception Source:      ");
      sb.AppendLine(e.Source);
      sb.Append("Application Domain:    ");
      try
      { sb.AppendLine(System.AppDomain.CurrentDomain.FriendlyName); }
      catch (Exception ex)
      { sb.AppendLine(ex.ToString()); }
      sb.Append("Assembly Full Name:    ");
      sb.AppendLine(System.Reflection.Assembly.GetEntryAssembly().FullName);
      sb.Append("Assembly Version:      ");
      sb.AppendLine(System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
      sb.Append("Assembly Build Date:   ");
      sb.AppendLine(System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetEntryAssembly().Location).ToString(DateTimeFormatInfo.InvariantInfo));
      sb.AppendLine();
      sb.Append("Message:               ");
      sb.AppendLine(e.Message);
      sb.AppendLine();
      sb.AppendLine();
      sb.AppendLine("----- Stack Trace -----");
      sb.AppendLine(e.StackTrace);

      errorReport = sb.ToString();
      textBox1.Text = errorReport;
    }

    private string GetIP()
    {
      try
      {
        string ip = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[0].ToString();
        return ip;
      }
      catch
      {
        return "127.0.0.1";
      }
    }
    private string GetUser()
    {
      string ret = "";
      try
      {
        ret = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
      }
      catch
      {
        try
        {
          ret = System.Environment.UserDomainName + @"\" + System.Environment.UserName;
        }
        catch { }
      }
      return ret;
    }
  }
}
