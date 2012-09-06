using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WikiHistory.HelpFunctions;
using System.Drawing;
using System.Diagnostics;

namespace WikiHistory
{
  public partial class MainForm : Form
  {
    #region filling
    private void prepareListViewEdits()
    {
      prepareListViewEdits("");
    }
    private void prepareListViewEdits(string user)
    {
      if (revisions == null) return;
      List<ListViewItem> items = new List<ListViewItem>();
      foreach (Revision rev in revisions)
      {
        ListViewItem lvi = new ListViewItem(rev.id.ToString());
        lvi.UseItemStyleForSubItems = false;
        lvi.BackColor = rev.displayColor;

        ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem(lvi, rev.user);
        if (rev.anon) lvsi.ForeColor = Color.Blue;
        lvi.SubItems.Add(lvsi);

        if (rev.minor)
          lvsi = new ListViewItem.ListViewSubItem(lvi, "X");
        else
          lvsi = new ListViewItem.ListViewSubItem(lvi, "");
        lvi.SubItems.Add(lvsi);

        lvsi = new ListViewItem.ListViewSubItem(lvi, DateTimeHelpFunctions.DateTimeToString(rev.timestamp));
        lvi.SubItems.Add(lvsi);

        if (rev.size >= 0)
        {
          lvsi = new ListViewItem.ListViewSubItem(lvi, rev.size.ToString());
          lvi.SubItems.Add(lvsi);
        }
        else
        {
          lvsi = new ListViewItem.ListViewSubItem(lvi, "0");
          lvsi.ForeColor = Color.DarkGray;
          lvi.SubItems.Add(lvsi);
        }

        lvsi = new ListViewItem.ListViewSubItem(lvi, rev.comment);
        lvi.SubItems.Add(lvsi);

        lvi.Tag = rev;

        if (bwLoadHistory.CancellationPending) return;
        if ((user == "") || (user == rev.user))
          items.Add(lvi);
        if ((items.Count % 100 == 0) || (items.Count == revisions.Count))
        {
          this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Filling List (" + items.Count.ToString() + " revisions)", (double)items.Count / revisions.Count });
        }
      }
      this.Invoke(new FillListViewEditsDelegate(fillListViewEdits), new object[] { items });
      this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Edit list filled", -1 });
    }

    private delegate void FillListViewEditsDelegate(List<ListViewItem> items);
    private void fillListViewEdits(List<ListViewItem> items)
    {
      listViewEdits.BeginUpdate();
      listViewEdits.Items.Clear();
      listViewEdits.Items.AddRange(items.ToArray());
      listViewEdits.Sort();
      listViewEdits.EndUpdate();
      foreach(ListViewItem lvi in items)
      {
        comboBoxAnalyzeWhichRevision.Items.Insert(0, "Version " + lvi.SubItems[0].Text + " (" + lvi.SubItems[3].Text + "; " + lvi.SubItems[1].Text + ")");
      }
      if (comboBoxAnalyzeWhichRevision.Items.Count > 0)
      {
        comboBoxAnalyzeWhichRevision.Items[0] = "Current version";
        comboBoxAnalyzeWhichRevision.SelectedIndex = 0;
      }
    }
    #endregion

    #region listview ordering
    private void listViewEdits_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorterEdits.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorterEdits.Order == SortOrder.Ascending)
        {
          lvwColumnSorterEdits.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorterEdits.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorterEdits.SortColumn = e.Column;
        lvwColumnSorterEdits.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      listViewEdits.Sort();
    }
    #endregion

    #region context menu
    private void listViewEdits_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        ListViewItem lvi = listViewEdits.GetItemAt(e.X, e.Y);
        if (lvi != null)
        {
          cMenuListViewEditsShowDiffTwoVersions.Visible = false;
          cMenuListViewEditsShowDiff.Visible = false;
          if (listViewEdits.SelectedItems.Count == 2)
            cMenuListViewEditsShowDiffTwoVersions.Visible = true;
          else if (listViewEdits.SelectedItems.Count == 1)
            cMenuListViewEditsShowDiff.Visible = true;

          contextMenuListViewEdits.Show(listViewEdits.PointToScreen(new Point(e.X, e.Y)));
        }
      }
    }

    #region menu handling
    private void cMenuListViewEditsShowDiff_Click(object sender, EventArgs e)
    {
      ShowDiffInBrowser();
    }

    private void cMenuListViewEditsShowDiffTwoVersions_Click(object sender, EventArgs e)
    {
      ShowDiffInBrowser();
    }

    private void cMenuListViewEditsCopy_Click(object sender, EventArgs e)
    {
      string copy = "";

      foreach (ListViewItem lvi in listViewEdits.SelectedItems)
      {
        Revision rev = (Revision)lvi.Tag;
        string newLine = " ";

        newLine += DateTimeHelpFunctions.DateTimeToString(rev.timestamp) + "  ";
        newLine += rev.user;
        for (int i = 0; i < 30 - rev.user.Length; i++) newLine += " ";
        newLine += rev.comment;

        copy += newLine + Environment.NewLine;
      }


      Clipboard.SetText(copy);
    }
    #endregion
    #endregion

    #region double click
    private void listViewEdits_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        ShowDiffInBrowser();
      }
    }
    #endregion

    #region key events
    private void listViewEdits_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Control && (e.KeyCode == Keys.A)) // select all
      {
        foreach (ListViewItem lvi in listViewEdits.Items)
          lvi.Selected = true;
      }

    }
    #endregion

    #region show in browser
    private void ShowDiffInBrowser()
    {
      if (listViewEdits.SelectedItems.Count != 2)
      {
        long id = ((Revision)listViewEdits.SelectedItems[0].Tag).id;
        string url = Projects.currentProjectBaseUrl + "index.php?title=" + WikiHelpFunctions.TitleToUrlTitle(Revision.currentTitle) + "&oldid=" + id.ToString() + "&diff=prev";
        Process.Start(url);
      }
      else
      {
        long id1 = ((Revision)listViewEdits.SelectedItems[0].Tag).id;
        long id2 = ((Revision)listViewEdits.SelectedItems[1].Tag).id;

        if (((Revision)listViewEdits.SelectedItems[0].Tag).timestamp < ((Revision)listViewEdits.SelectedItems[1].Tag).timestamp)
        {
          long tmp = id1;
          id1 = id2;
          id2 = tmp;
        }

        string url = Projects.currentProjectBaseUrl + "index.php?title=" + WikiHelpFunctions.TitleToUrlTitle(Revision.currentTitle) + "&diff=" + id1.ToString() + "&oldid=" + id2.ToString();
        Process.Start(url);
      }
    }
    #endregion
  }
}