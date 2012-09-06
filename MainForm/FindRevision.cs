using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WikiHistory.HelpFunctions;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace WikiHistory
{
  public partial class MainForm : Form
  {
    #region form handling
    private void buttonSearchRevision_Click(object sender, EventArgs e)
    {
      FindRevision(textBoxSearchRevision.Text);
    }

    private void textBoxSearchRevision_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        buttonSearchRevision.Focus();
        FindRevision(textBoxSearchRevision.Text);
      }
    }
    #endregion

    BackgroundWorker bwFindRevision = new BackgroundWorker();

    private void FindRevision(string text)
    {
      if (!bwFindRevision.IsBusy)
      {
        if ((revisions == null) || (Revision.currentTitle == ""))
        {
          MessageBox.Show("Пожалуйста, загрузите статью сначала!", Program.ProgramName);
          return;
        }
        #region check regular expression
        if (checkBoxFindRevisionRegEx.Checked)
        {
          try
          {
            Regex r = new Regex(text);
          }
          catch
          {
            MessageBox.Show("The entered string isn't a valid regular expression", Program.ProgramName);
            return;
          }
        }
        #endregion
        #region selected revisions
        foreach (Revision rev in revisions)
          rev.selected = true;
        if (comboBox1.SelectedIndex == 1) // selected
        {
          foreach (ListViewItem lvi in listViewEdits.Items)
            ((Revision)lvi.Tag).selected = lvi.Selected;
        }
        else if (comboBox1.SelectedIndex == 2) // given timespan
        {
          foreach (ListViewItem lvi in listViewEdits.Items)
          {
            Revision rev = (Revision)lvi.Tag;
            if ((rev.timestamp < dateTimePicker1.Value) || (rev.timestamp > dateTimePicker2.Value))
              rev.selected = false;
          }
        }
        #endregion
        foreach (Revision rev in revisions)
          rev.displayColor = SystemColors.Window;
        prepareListViewEdits();
        buttonSearchRevision.Text = "Отменить";
        textBoxSearchRevision.Enabled = false;
        bwFindRevision.RunWorkerAsync(text);
      }
      else
      {
        bwFindRevision.CancelAsync();
        textBoxSearchRevision.Enabled = true;
        buttonSearchRevision.Text = "Искать!";
      }
    }

    private void bwFindRevision_DoWork(object sender, DoWorkEventArgs e)
    {
      string searchText = (string)e.Argument;
      bool caseInsensitive = checkBoxFindRevisionCaseInsensitive.Checked;
      bool found = false;
      int nrOfRevisionsFound = 0;

      if (caseInsensitive)
        searchText = searchText.ToLowerInvariant();

      Regex r;
      RegexOptions rOptions = RegexOptions.None;
      if (caseInsensitive)
        rOptions |= RegexOptions.IgnoreCase;
      if (checkBoxFindRevisionRegEx.Checked)
        r = new Regex(searchText, rOptions);
      else
        r = new Regex(Regex.Escape(searchText), rOptions);

      this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Поиск...", -1 });

      for (int i = 0; i < revisions.Count; i++)
      {
        if (!revisions[i].selected) continue;

        if (bwFindRevision.CancellationPending)
        {
          this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Поиск отменен", -1 });
          return;
        }

        if (!revisions[i].fullTextLoaded)
        {
          this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Loading text for revision " + revisions[i].id.ToString() + " (" + DateTimeHelpFunctions.DateTimeToString(revisions[i].timestamp) + ")", (double)i / revisions.Count });
          revisions[i].LoadFullText();
        }

        found = r.IsMatch(revisions[i].fullText);        
        if (found) nrOfRevisionsFound++;
        this.Invoke(new ShowRevisionStateDelegate(showRevisionState), new object[] { revisions[i], found });

        if ((found) && (checkBoxFindRevisionOnlyOldest.Checked))
        {
          // found!
          this.Invoke(new FindRevisionSuccessDelegate(findRevisionSuccess), new object[] { revisions[i] });
          this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Текст найден", -1 });
          MessageBox.Show("Первое совпадение в версии " + revisions[i].id.ToString() + ".");
          this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "", -1 });
          return;
        }
      }

      if (nrOfRevisionsFound > 0)
      {
        this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Text found", -1 });
        MessageBox.Show("The searched text was found in " + nrOfRevisionsFound.ToString() + " revisions.");
        this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "", -1 });
        this.Invoke(new FindRevisionEndedDelegate(findRevisionEnded));
      }
      else
      {
        this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Text not found", -1 });
        MessageBox.Show("The searched text wasn't found in any revision.");
        this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "", -1 });
        this.Invoke(new FindRevisionEndedDelegate(findRevisionEnded));
      }
    }

    private delegate void FindRevisionEndedDelegate();
    private void findRevisionEnded()
    {
      textBoxSearchRevision.Enabled = true;
      buttonSearchRevision.Text = "Search!";
    }

    private delegate void FindRevisionSuccessDelegate(Revision rev);
    private void findRevisionSuccess(Revision rev)
    {
      findRevisionEnded();
      tabControl1.SelectedIndex = 1;

      comboBoxUser1.Text = "";
      showProgress("", -1);

      foreach (ListViewItem lvi in listViewEdits.Items)
      {
        if ((Revision)lvi.Tag == rev)
        {
          lvi.Selected = true;
          lvi.EnsureVisible();
          break;
        }
      }
    }

    private delegate void ShowRevisionStateDelegate(Revision rev, bool found);
    private void showRevisionState(Revision rev, bool found)
    {
      if (found)
        rev.displayColor = Color.LightGreen;
      else
        rev.displayColor = Color.LightPink;
      foreach (ListViewItem lvi in listViewEdits.Items)
      {
        if ((Revision)lvi.Tag == rev)
        {
          lvi.BackColor = rev.displayColor;
          break;
        }
      }
    }

    #region handling of GUI
    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      dateTimePicker1.Enabled = (comboBox1.SelectedIndex == 2);
      dateTimePicker2.Enabled = dateTimePicker1.Enabled;      
    }
    #endregion
  }
}
