using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using WikiHistory.HelpFunctions;

namespace WikiHistory.AdditionalForms
{
  public partial class CacheSettings : Form
  {
    public CacheSettings()
    {
      InitializeComponent();
    }

    private void CacheSettings_Load(object sender, EventArgs e)
    {
      checkBox1.Checked = Properties.Settings.Default.CacheRevisions;

      string cachePath = Cache.getPath();
      label1.Text += cachePath;
      if (cachePath == "")
      {
        if (checkBox1.Checked)
        {
          MessageBox.Show("Error: could not create cache folder, disabling cache!");
          checkBox1.Checked = false;
        }
        checkBox1.Enabled = false;        
      }

      int size = Properties.Settings.Default.CacheSize;
      if (size > numericUpDown1.Maximum) size = (int)numericUpDown1.Maximum;
      if (size > 0)
      {
        checkBox2.Checked = true;
        numericUpDown1.Value = size;
      }
    }

    private void Save()
    {
      Properties.Settings.Default.CacheRevisions = checkBox1.Checked;

      if (!checkBox2.Checked)
        Properties.Settings.Default.CacheSize = 0;
      else
        Properties.Settings.Default.CacheSize = (int)numericUpDown1.Value;

      Properties.Settings.Default.Save();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      Save();
      this.Close();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBox1.Checked)
      {
        checkBox2.Enabled = true;
        if (checkBox2.Checked) numericUpDown1.Enabled = true;
        button3.Enabled = true;
      }
      else
      {
        checkBox2.Enabled = false;
        numericUpDown1.Enabled = false;
        button3.Enabled = false;
      }
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBox2.Checked)
        numericUpDown1.Enabled = true;
      else
        numericUpDown1.Enabled = false;
    }

    private void button3_Click(object sender, EventArgs e)
    {
      string path = Cache.getPath();
      string[] files = Directory.GetFiles(path);
      foreach (string file in files)
        File.Delete(file);
    }
  }
}
