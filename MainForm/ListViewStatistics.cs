using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WikiHistory.HelpFunctions;

namespace WikiHistory
{
  public partial class MainForm : Form
  {
    private void prepareListViewStatistics()
    {
      listViewStatistics.Items.Clear();
      listViewStatistics.Items.Add(new ListViewItem("Правок"));
      listViewStatistics.Items.Add(new ListViewItem("Малых правок"));
      listViewStatistics.Items.Add(new ListViewItem("Анонимных правок"));
      listViewStatistics.Items.Add(new ListViewItem(string.Empty));
      listViewStatistics.Items.Add(new ListViewItem("Различных участников"));
      listViewStatistics.Items.Add(new ListViewItem("Анонимных участников"));
      listViewStatistics.Items.Add(new ListViewItem("Правок на участника"));
      listViewStatistics.Items.Add(new ListViewItem(string.Empty));
      listViewStatistics.Items.Add(new ListViewItem("Первая правка"));
      listViewStatistics.Items.Add(new ListViewItem("Последняя правка"));
    }

    private delegate void FillListViewStatisticsDelegate();
    private void fillListViewStatistics()
    {
      tabControl1.SelectedIndex = 0;

      // Edits
      listViewStatistics.Items[0].SubItems.Add(revisions.Count.ToString());

      statistics.nrOfMinorEdits = 0;
      statistics.nrOfAnonymousEdits = 0;
      foreach (Revision rev in revisions)
      {
        if (rev.minor) statistics.nrOfMinorEdits++;
        if (rev.anon) statistics.nrOfAnonymousEdits++;
      }
      // Minor Edits
      listViewStatistics.Items[1].SubItems.Add(statistics.nrOfMinorEdits.ToString());
      listViewStatistics.Items[1].SubItems.Add(((int)Math.Round(100 * (double)statistics.nrOfMinorEdits / revisions.Count)).ToString() + " %");

      // Anonymous Edits
      listViewStatistics.Items[2].SubItems.Add(statistics.nrOfAnonymousEdits.ToString());
      listViewStatistics.Items[2].SubItems.Add(((int)Math.Round(100 * (double)statistics.nrOfAnonymousEdits / revisions.Count)).ToString() + " %");

      // Different Users
      listViewStatistics.Items[4].SubItems.Add(users.Count.ToString());

      // Anonymous Users
      listViewStatistics.Items[5].SubItems.Add(statistics.nrOfAnonymousUsers.ToString());
      listViewStatistics.Items[5].SubItems.Add(((int)Math.Round(100 * (double)statistics.nrOfAnonymousUsers / users.Count)).ToString() + " %");

      // Edits per User
      listViewStatistics.Items[6].SubItems.Add(((double)revisions.Count / users.Count).ToString("0.00"));

      statistics.firstEdit = revisions[0].timestamp;
      statistics.lastEdit = statistics.firstEdit;
      foreach (Revision rev in revisions)
      {
        if (statistics.firstEdit > rev.timestamp) statistics.firstEdit = rev.timestamp;
        if (statistics.lastEdit < rev.timestamp) statistics.lastEdit = rev.timestamp;
      }

      // First Edit
      listViewStatistics.Items[8].SubItems.Add(DateTimeHelpFunctions.DateTimeToString(statistics.firstEdit));
      listViewStatistics.Items[8].SubItems.Add(DateTimeHelpFunctions.DaysAgo(statistics.firstEdit));

      // Last Edit
      listViewStatistics.Items[9].SubItems.Add(DateTimeHelpFunctions.DateTimeToString(statistics.lastEdit));
      listViewStatistics.Items[9].SubItems.Add(DateTimeHelpFunctions.DaysAgo(statistics.lastEdit));
    }
  }
}
