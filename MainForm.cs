using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Collections;

using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Specialized;
using WikiHistory.HelpFunctions;
using System.Collections.Generic;
using System.Reflection;

namespace WikiHistory
{
  public partial class MainForm : Form
  {
    SuggestionFetcher suggestionFetcher;
    BackgroundWorker bwSuggestions = new BackgroundWorker();
    RevisionsFetcher revisionsFetcher;
    BackgroundWorker bwLoadHistory = new BackgroundWorker();

    List<Revision> revisions;
    List<User> users;
    struct Statistics
    {
      public int nrOfMinorEdits;
      public int nrOfAnonymousEdits;
      public int nrOfAnonymousUsers;
      public DateTime firstEdit;
      public DateTime lastEdit;
    }

    Statistics statistics;

    ListViewColumnSorter lvwColumnSorterEdits = new ListViewColumnSorter();
    ListViewColumnSorter lvwColumnSorterUsers = new ListViewColumnSorter();
    public MainForm()
    {
      InitializeComponent();

      if (Properties.Settings.Default.NeedUpgrade)
      {
        Properties.Settings.Default.Upgrade();
        Properties.Settings.Default.NeedUpgrade = false;
        Properties.Settings.Default.AutoUpdateThisVersion = true;
        Properties.Settings.Default.Save();
      }

      if (Program.Mono)
      {
        comboBoxUserColor1.BackColor = Color.DarkGreen;
        comboBoxUserColor1.ForeColor = Color.White;
        comboBoxUserColor2.BackColor = Color.DarkBlue;
        comboBoxUserColor2.ForeColor = Color.White;
        comboBoxUserColor3.BackColor = Color.DarkRed;
        comboBoxUserColor3.ForeColor = Color.White;
        comboBoxUserColor4.BackColor = Color.DarkViolet;
        comboBoxUserColor4.ForeColor = Color.White;
        comboBoxUserColor5.BackColor = Color.DarkOrange;
        comboBoxUserColor5.ForeColor = Color.White;
        richTextBox1.Font = new Font(richTextBox1.Font, FontStyle.Bold);
      }

      createProjectMenu();

      #region loadSettings
      loadSelectedProject();
      #endregion

      #region initialization of background workers
      bwSuggestions.DoWork += new DoWorkEventHandler(bwSuggestions_DoWork);
      bwSuggestions.WorkerSupportsCancellation = true;

      bwLoadHistory.DoWork += new DoWorkEventHandler(bwLoadHistory_DoWork);
      bwLoadHistory.WorkerSupportsCancellation = true;

      bwFindRevision.DoWork += new DoWorkEventHandler(bwFindRevision_DoWork);
      bwFindRevision.WorkerSupportsCancellation = true;

      bwAuthors.DoWork += new DoWorkEventHandler(bwAuthors_DoWork);
      bwAuthors.WorkerSupportsCancellation = true;
      #endregion

      articleSuggestions.Left = textBoxArticleTitle.Left + groupBox1.Left;
      articleSuggestions.Width = textBoxArticleTitle.Width;
      articleSuggestions.Top = textBoxArticleTitle.Top + groupBox1.Top + textBoxArticleTitle.Height;

      listViewEdits.ListViewItemSorter = lvwColumnSorterEdits;
      listViewUsers.ListViewItemSorter = lvwColumnSorterUsers;

      suggestionFetcher = new SuggestionFetcher(Projects.currentProjectBaseUrl, 30);
      revisionsFetcher = new RevisionsFetcher(Projects.currentProjectBaseUrl);

      comboBox1.SelectedIndex = 0;

      prepareListViewStatistics();

      if ((Properties.Settings.Default.AutoUpdate) && (Properties.Settings.Default.AutoUpdateThisVersion))
        AutoUpdate.CheckForUpdate();
    }

    public void Reset()
    {
      lvwColumnSorterEdits.SortColumn = 3;
      lvwColumnSorterEdits.Order = SortOrder.Descending;
      users = null;
      revisions = null;
      statistics.nrOfMinorEdits = 0;
      statistics.nrOfAnonymousEdits = 0;
      statistics.nrOfAnonymousUsers = 0;

      Revision.currentTitle = "";

      listViewUsers.Items.Clear();
      listViewEdits.Items.Clear();
      prepareListViewStatistics();

      comboBoxUserColor1.Items.Clear();
      comboBoxUserColor2.Items.Clear();
      comboBoxUserColor3.Items.Clear();
      comboBoxUserColor4.Items.Clear();
      comboBoxUserColor5.Items.Clear();
      comboBoxUserColor1.Text = "";
      comboBoxUserColor2.Text = "";
      comboBoxUserColor3.Text = "";
      comboBoxUserColor4.Text = "";
      comboBoxUserColor5.Text = "";
      richTextBox1.Clear();
    }

    #region suggestion!
    List<SuggestionFetcher.Suggestion> suggestions;
    private void bwSuggestions_DoWork(object sender, DoWorkEventArgs e)
    {
      if (textBoxArticleTitle.Text != string.Empty)
      {
        string lastSearch;
        do
        {
          lastSearch = textBoxArticleTitle.Text;
          suggestions = suggestionFetcher.getSuggestions(lastSearch);
        }
        while (lastSearch != textBoxArticleTitle.Text);

        if (suggestions.Count > 0)
          this.Invoke(new UpdateSuggestionBoxDelegate(updateSuggestionBox));
      }
    }

    private delegate void UpdateSuggestionBoxDelegate();
    private void updateSuggestionBox()
    {
      if (suggestions == null) return;
      if (Loading) return;
      if ((suggestions.Count < 1) || ((suggestions.Count == 1) && (suggestions[0].title == textBoxArticleTitle.Text)))
      {
        articleSuggestions.Visible = false;
      }
      else
      {
        if (suggestions.Count <= 8)
          articleSuggestions.Height = articleSuggestions.ItemHeight * suggestions.Count + 4;
        else
          articleSuggestions.Height = articleSuggestions.ItemHeight * 8 + 4;
        articleSuggestions.Items.Clear();
        foreach (SuggestionFetcher.Suggestion s in suggestions)
        {
          articleSuggestions.Items.Add(s.title);
        }

        articleSuggestions.BringToFront();
        articleSuggestions.Width = textBoxArticleTitle.Width; // only needed for MONO
        articleSuggestions.Visible = true;
      }
    }

    private void textBoxArticleTitle_TextChanged(object sender, EventArgs e)
    {
      suggestionFetcher.cancelLoading = true;
      articleSuggestions.Visible = false;
      if (!bwSuggestions.IsBusy) bwSuggestions.RunWorkerAsync(); 
    }

    private void textBoxArticleTitle_KeyDown(object sender, KeyEventArgs e)
    {
      if ((e.KeyCode == Keys.Down) && (articleSuggestions.Visible))
      {
        articleSuggestions.SelectedItem = articleSuggestions.Items[0];
        articleSuggestions.Focus();
      }
      else if (e.KeyCode == Keys.Enter)
      {
        suggestions = null;
        articleSuggestions.Visible = false;
        loadHistory();
      }
    }

    private void textBoxArticleTitle_Leave(object sender, EventArgs e)
    {
      if (!articleSuggestions.Focused)
        articleSuggestions.Visible = false;
    }

    private void articleSuggestions_KeyDown(object sender, KeyEventArgs e)
    {
      if ((e.KeyCode == Keys.Enter) && (articleSuggestions.SelectedIndex >= 0))
      {
        textBoxArticleTitle.Text = articleSuggestions.Items[articleSuggestions.SelectedIndex].ToString();
        textBoxArticleTitle.Select(textBoxArticleTitle.Text.Length, 0);
        textBoxArticleTitle.Focus();
        articleSuggestions.Visible = false;
      }
    }

    private void articleSuggestions_Click(object sender, EventArgs e)
    {
      if (articleSuggestions.SelectedIndex >= 0)
      {
        textBoxArticleTitle.Text = articleSuggestions.Items[articleSuggestions.SelectedIndex].ToString();
        textBoxArticleTitle.Select(textBoxArticleTitle.Text.Length, 0);
        textBoxArticleTitle.Focus();
        articleSuggestions.Visible = false;
      }
    }
    #endregion

    #region load history
    private bool loading = false;
    private bool Loading
    {
      get { return loading; }
      set 
      {
        loading = value;
        if (loading)
        {
          buttonSearch.Text = "Отменить загрузку";
          textBoxArticleTitle.Enabled = false;
        }
        else
        {
          buttonSearch.Text = "Загрузить историю";
          textBoxArticleTitle.Enabled = true;
        }
      }
    }

    private void buttonSearch_Click(object sender, EventArgs e)
    {
      if (!loading)
        loadHistory();
      else
        cancelLoading();
    }

    private void loadHistory()
    {
      if (textBoxArticleTitle.Text == string.Empty) return;
      Reset();
      Loading = true;
      Revision.currentTitle = textBoxArticleTitle.Text;
      bwLoadHistory.RunWorkerAsync();
    }

    private void cancelLoading()
    {
      if (bwLoadHistory.IsBusy)
      {
        bwLoadHistory.CancelAsync();
        while (bwLoadHistory.IsBusy) { Application.DoEvents(); }
        showProgress("Loading cancelled!", -1);
      }

      Loading = false;
    }

    private void bwLoadHistory_DoWork(object sender, DoWorkEventArgs e)
    {
      List<Revision> tempList;
      revisions = new List<Revision>();
      revisionsFetcher.Reset();
      while (!revisionsFetcher.ready)
      {
        if (bwLoadHistory.CancellationPending) return;
        this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Загрузка истории статьи (загружены " + revisions.Count.ToString() + " версий)", -1 } );

        tempList = revisionsFetcher.getRevisions(Revision.currentTitle);
        revisions.AddRange(tempList);
      }
      this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Загружена история (" + revisions.Count.ToString() + " версий); Заполнение списка...", 0 });

      if (revisions.Count == 0)
      {
        this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "", -1 });
        MessageBox.Show("Ни одной версии не загружено!");
        this.Invoke(new LoadingReadyDelegate(LoadingReady));
        return;
      }

      // sort list!
      this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Сортировка загруженных версий", -1 });
      revisions.Sort();

      // fill list
      prepareListViewEdits();

      // get users!
      this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Подготовка данных пользователей...", 0 });
      users = new List<User>();
      statistics.nrOfAnonymousUsers = 0;
      foreach (Revision rev in revisions)
      {
        User u1 = users.Find(delegate(User u) { return string.Compare(u.name, rev.user) == 0; });
        if (u1 == null)
        {
          u1 = new User();
          if (rev.user == null) { continue; }
           u1.name = rev.user;
             
          //MessageBox.Show(rev.user + "L" + rev.user.Length);
          if (rev.anon)
          {
            u1.anon = true;
            statistics.nrOfAnonymousUsers++;
          }
          u1.firstEdit = rev.timestamp;
          u1.lastEdit = rev.timestamp;
          u1.nrOfEdits = 1;
          if (rev.minor) u1.nrOfMinorEdits = 1;
          users.Add(u1);
        }
        else
        {
          u1.nrOfEdits++;
          if (rev.minor) u1.nrOfMinorEdits++;
          if (rev.timestamp < u1.firstEdit) u1.firstEdit = rev.timestamp;
          if (rev.timestamp > u1.lastEdit) u1.lastEdit = rev.timestamp;
        }
      }

      // fill user combo box (at edits overview)
      this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Заполнение списка пользователей...", -1 });
      prepareUserComboBox();

      // fill user list box
      prepareListViewUsers();

      // statistics
      this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Подготовка статистики...", -1 });
      this.Invoke(new FillListViewStatisticsDelegate(fillListViewStatistics));

      // ready
      this.Invoke(new ShowProgressDelegate(showProgress), new object[] { string.Empty, -1 });
      this.Invoke(new LoadingReadyDelegate(LoadingReady));
    }

    private delegate void LoadingReadyDelegate();
    private void LoadingReady()
    {
      Loading = false;
    }
    #endregion

    #region show progress
    private delegate void ShowProgressDelegate(string message, double progress);
    private void showProgress(string message, double progress)
    {
      toolStripStatusLabel1.Text = message;
      if (progress == -1)
      {
        toolStripProgressBar1.Visible = false;
      }
      else
      {
        if (progress < 0) progress = 0;
        else if (progress > 1) progress = 1;
        toolStripProgressBar1.Value = (int)(progress * 100);
        toolStripProgressBar1.Visible = true;
      }
    }
    #endregion

    #region user combo boxes
    #region filling
    private void prepareUserComboBox()
    {
      List<string> items = new List<string>();
      foreach (User u in users)
      {
          if (u == null) { continue; }
         
          if (u.name == null || u.name == "") { continue; }
          //MessageBox.Show(u.name);
        items.Add(u.name);
      }
      this.Invoke(new FillUserComboBoxDelegate(fillUserComboBox), new object[] { items });
    }

    private delegate void FillUserComboBoxDelegate(List<string> items);
    private void fillUserComboBox(List<string> items)
    {
      comboBoxUser1.BeginUpdate();
      comboBoxUser1.Items.Clear();
      comboBoxUser1.Items.Add(" (all)");
      comboBoxUser1.Items.AddRange(items.ToArray());
      comboBoxUser1.EndUpdate();

      comboBoxUserColor1.BeginUpdate();
      comboBoxUserColor1.Items.Clear();
      comboBoxUserColor1.Items.AddRange(items.ToArray());
      comboBoxUserColor1.EndUpdate();
      comboBoxUserColor2.BeginUpdate();
      comboBoxUserColor2.Items.Clear();
      comboBoxUserColor2.Items.AddRange(items.ToArray());
      comboBoxUserColor2.EndUpdate();
      comboBoxUserColor3.BeginUpdate();
      comboBoxUserColor3.Items.Clear();
      comboBoxUserColor3.Items.AddRange(items.ToArray());
      comboBoxUserColor3.EndUpdate();
      comboBoxUserColor4.BeginUpdate();
      comboBoxUserColor4.Items.Clear();
      comboBoxUserColor4.Items.AddRange(items.ToArray());
      comboBoxUserColor4.EndUpdate();
      comboBoxUserColor5.BeginUpdate();
      comboBoxUserColor5.Items.Clear();
      comboBoxUserColor5.Items.AddRange(items.ToArray());
      comboBoxUserColor5.EndUpdate();
    }
    #endregion

    #region handling
    private void comboBoxUser1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (comboBoxUser1.SelectedIndex > 0)
      {
        string user = (string)comboBoxUser1.Items[comboBoxUser1.SelectedIndex];
        prepareListViewEdits(user);
      }
      else
        prepareListViewEdits();
      showProgress("", -1);
    }

    private void comboBoxUser1_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        prepareListViewEdits(comboBoxUser1.Text);
        showProgress("", -1);
      }
    }
    #endregion
    #endregion

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
       
      showProgress("Закрытие...", -1);
      //bwAuthors.stopped = 1;
      if (bwFindRevision.IsBusy) bwFindRevision.CancelAsync();
      if (bwSuggestions.IsBusy) bwSuggestions.CancelAsync();
      if (bwLoadHistory.IsBusy) bwLoadHistory.CancelAsync();
      if (bwAuthors.IsBusy) bwAuthors.CancelAsync();
      Thread.Sleep(500);
      Environment.Exit(-1);
      
      while (bwFindRevision.IsBusy) { Application.DoEvents(); }
      while (bwSuggestions.IsBusy) { Application.DoEvents(); }
      while (bwLoadHistory.IsBusy) { Application.DoEvents(); }
      while (bwAuthors.IsBusy) { Application.DoEvents(); }
    }

    private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
    {
      //e.Handled = true;
    }

    private void tabPage5_SizeChanged(object sender, EventArgs e)
    {
      AdjustAuthorsButtons();
    }
    private void AdjustAuthorsButtons()
    {
      if (buttonAuthorsPause.Visible)
      {
        buttonAnalyzeAuthors.Left = tabPage5.Width / 2;
        buttonAuthorsPause.Width = tabPage5.Width / 2;
        buttonAnalyzeAuthors.Width = buttonAuthorsPause.Width;
        buttonAuthorsPause.Left = 0;
      }
      else
      {
        buttonAnalyzeAuthors.Left = 0;
        buttonAnalyzeAuthors.Width = tabPage5.Width;
      }
    }
    
    int FastIndexOf(string source, string pattern)
    {
        if (pattern == null) throw new ArgumentNullException();
        if (pattern.Length == 0) return 0;
        if (pattern.Length == 1) return source.IndexOf(pattern[0]);
        bool found;
        int limit = source.Length - pattern.Length + 1;
        if (limit < 1) return -1;
        // Store the first 2 characters of "pattern"
        char c0 = pattern[0];
        char c1 = pattern[1];
        // Find the first occurrence of the first character
        int first = source.IndexOf(c0, 0, limit);
        while (first != -1)
        {
            // Check if the following character is the same like
            // the 2nd character of "pattern"
            if (source[first + 1] != c1)
            {
                first = source.IndexOf(c0, ++first, limit - first);
                continue;
            }
            // Check the rest of "pattern" (starting with the 3rd character)
            found = true;
            for (int j = 2; j < pattern.Length; j++)
                if (source[first + j] != pattern[j])
                {
                    found = false;
                    break;
                }
            // If the whole word was found, return its index, otherwise try again
            if (found) return first;
            first = source.IndexOf(c0, ++first, limit - first);
        }
        return -1;
    }

    public int RegexIndexOf(String where, String what)
    {
        Regex rg = new Regex(what, RegexOptions.None);
        Match m = rg.Match(where);
        return (m.Success ? m.Index : -1);
    }

    private void button1_Click(object sender, EventArgs e)
    {
       
        
    }
        
  }
}
