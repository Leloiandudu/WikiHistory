using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WikiHistory.HelpFunctions;
using System.Drawing;

namespace WikiHistory
{
  public partial class MainForm : Form
  {
    #region filling
    private void prepareListViewUsers()
    {
      if (users == null) return;
      List<ListViewItem> items = new List<ListViewItem>();
      foreach (User user in users)
      {
        ListViewItem lvi = new ListViewItem(user.name);
        lvi.UseItemStyleForSubItems = false;
        if (user.anon) lvi.SubItems[0].ForeColor = Color.Blue;

        ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem(lvi, user.nrOfEdits.ToString());
        lvi.SubItems.Add(lvsi);

        lvsi = new ListViewItem.ListViewSubItem(lvi, user.nrOfMinorEdits.ToString());
        lvi.SubItems.Add(lvsi);

        lvsi = new ListViewItem.ListViewSubItem(lvi, ((int)Math.Floor(((double)user.nrOfMinorEdits / user.nrOfEdits) * 100)).ToString() + " %");
        lvi.SubItems.Add(lvsi);

        lvsi = new ListViewItem.ListViewSubItem(lvi, DateTimeHelpFunctions.DateTimeToString(user.firstEdit));
        lvi.SubItems.Add(lvsi);

        lvsi = new ListViewItem.ListViewSubItem(lvi, DateTimeHelpFunctions.DateTimeToString(user.lastEdit));
        if (user.nrOfEdits == 1) lvsi.ForeColor = Color.DarkGray;
        lvi.SubItems.Add(lvsi);

        if (user.percentageOfContentAdded < 0)
        {
          lvsi = new ListViewItem.ListViewSubItem(lvi, "?");
          lvsi.ForeColor = Color.DarkGray;
        }
        else
        {
          lvsi = new ListViewItem.ListViewSubItem(lvi, ((int)(Math.Round(100 * user.percentageOfContentAdded))).ToString() + " %");
        }
        lvi.SubItems.Add(lvsi);

        if (bwLoadHistory.CancellationPending) return;
        items.Add(lvi);
      }
      this.Invoke(new FillListViewUsersDelegate(fillListViewUsers), new object[] { items });
    }

    private delegate void FillListViewUsersDelegate(List<ListViewItem> items);
    private void fillListViewUsers(List<ListViewItem> items)
    {
      listViewUsers.BeginUpdate();
      listViewUsers.Items.Clear();
      listViewUsers.Items.AddRange(items.ToArray());
      lvwColumnSorterUsers.Order = SortOrder.Descending;
      lvwColumnSorterUsers.SortColumn = 1;
      listViewUsers.Sort();
      listViewUsers.EndUpdate();
    }
    #endregion

    #region ordering
    private void listViewUsers_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorterUsers.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorterUsers.Order == SortOrder.Ascending)
        {
          lvwColumnSorterUsers.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorterUsers.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorterUsers.SortColumn = e.Column;
        lvwColumnSorterUsers.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      listViewUsers.Sort();
    }
    #endregion
  }
}
