using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using WikiHistory.HelpFunctions;
using System.Xml;
using System.ComponentModel;
using System.Web;
using System.Linq;


namespace WikiHistory
{

    public class ArticleText
    {
        public const int minLength = 1;

        public class Section
        {
            public int Start;
            public int Length;
            public Revision rev;
            public bool revKnown;
            public String substring;
            public Section(int start, int length) : this(start, length, null, false, "") { }
            public Section(int start, int length, String s) : this(start, length, null, false, s) { }
            public Section(int start, int length, Revision rev) : this(start, length, rev, true, "") { }
            public Section(int start, int length, Revision rev, String s) : this(start, length, rev, true, s) { }

            private Section(int start, int length, Revision rev, bool authorKnown, String basetext)
            {
                this.Start = start;
                this.Length = length;
                this.rev = rev;
                this.revKnown = authorKnown;
                this.substring = basetext.Substring(start, length);
            }
        }

        public class CacheSection
        {
            public List<Section> sections = new List<Section>();
            public const int capacity = 100;
            //public void Ad
        }

        private static Regex _sentenceSplitRegex = new Regex("(?<text>.*?)($|(\r?\n\r?\n)|(\\. ))", RegexOptions.Compiled | RegexOptions.Singleline);

        public List<Section> sections = new List<Section>();
        public bool ready = false;
        public string text;
        public string normalizedText;
        public ArticleText(string text)
        {
            this.text = text;
            this.normalizedText = normalizeString(text, false);

            sections.AddRange(GetText(" " + this.text + " ", _sentenceSplitRegex, (pos, len) => new Section(pos, len, normalizedText)));
        }

        private static IEnumerable<T> GetText<T>(string text, Regex regex, Func<int, int, T> func)
        {
            return from match in regex.Matches(text).OfType<Match>()
                   let grp = match.Groups["text"]
                   let len = grp.Index == 0
                               ? grp.Length + 1
                               : grp.Length + 2
                   let len2 = grp.Index + grp.Length == text.Length ? len - 1 : len
                   where grp.Length > 0
                   select func(Math.Max(0, grp.Index - 1), len2);//new Section(Math.Max(0, grp.Index - 1), len, text);
        }

        private Regex _spaceRegex = new Regex("  +", RegexOptions.Compiled);

        private string normalizeString(string s, bool collapse = false)
        {
            char[] specialChars = { '.', ',', '!', '?', '[', ']', '(', ')', '{', '}', '|', 
                              '-', '\'', '"', ':', '=', ';', '#', '/', '\\', '*',
                              '<', '>'/*, '»', '«', '»' */};

            s = " " + s + " ";
            s = s.Replace("\n", " ");
            s = s.Replace("\r", " ");
            s = s.Replace("\t", " ");
            // a lot of special chars
            foreach (char c in specialChars)
                s = s.Replace(c, ' ');

            if (collapse)
                return _spaceRegex.Replace(s, " ");
            return s;
        }

        public int SIndexOf(String where, String what)
        {
            return where.IndexOf(what, StringComparison.Ordinal);
        }

        private IEnumerable<Section> FindSections(string str)
        {
            return sections.Where(s => s.substring.Contains(str));
        }

        struct Substr
        {
            public Section Section;
            public string Text;
            public LCS Lcs;

            public Substr(Section section, string text, LCS lcs)
            {
                Section = section;
                Text = text;
                Lcs = lcs;
            }
        }

        public void MarkNewSections(Revision rev, BackgroundWorkerWithPause callingBw)
        {
            string searchtext = rev.fullText;

            var texts =
                GetText(rev.fullText, _sentenceSplitRegex, (pos, len) => rev.fullText.Substring(pos, len))
                .Select(t => normalizeString(t, false))
                .ToList();

            // previously found subsections should be removed
            foreach (Section s in sections)
            {

                if (s.revKnown)
                {
                    string sectionText = " " + s.substring + " ";
                    //int pos;
                    foreach (string t in texts)
                    {
                        int pos = 0;
                        if ((pos = SIndexOf(t, sectionText)) > -1)
                        {
                            string firstText = t.Substring(0, pos).Trim();
                            string lastText = t.Substring(pos + s.Length).Trim();
                            texts.Remove(t);

                            if (firstText != "") texts.Add(" " + firstText + " ");
                            if (lastText != "") texts.Add(" " + lastText + " ");
                            break;
                        }
                    }
                }
            }

            var textsTotal = texts.Sum(t => t.Length);
            var substrings = new List<Substr>();

            var newSections = sections.Where(s => !s.revKnown).ToList();
            var newTexts = new List<string>();

            for (; ; )
            {
                // finding another longest common substring

                System.Diagnostics.Debug.WriteLine(string.Format("[{0:T}] Next run: {1}/{2} sections, {3}/{4} ({5})", DateTime.Now,
                    sections.Where(s => s.revKnown).Count(), sections.Count,
                    textsTotal - texts.Sum(t => t.Length), searchtext.Length, texts.Count));

                const int minLcsLength = 3;

                foreach (var t in texts)
                    foreach (var s in newSections)
                    {
                        var lcs = LCSubstr(t, s.substring);
                        if (lcs.Length > minLcsLength)
                            substrings.Add(new Substr(s, t, lcs));
                    }
                foreach (var t in newTexts)
                    foreach (var s in sections.Where(s => !s.revKnown).Except(newSections))
                    {
                        var lcs = LCSubstr(t, s.substring);
                        if (lcs.Length > minLcsLength)
                            substrings.Add(new Substr(s, t, lcs));
                    }

                newTexts.Clear();
                newSections.Clear();

                if (substrings.Count == 0)
                    return;

                var max = substrings.Aggregate((x, y) => x.Lcs.Length > y.Lcs.Length ? x : y);

                var text = max.Text;
                var section = max.Section;
                var index = max.Lcs.Index1;
                var length = max.Lcs.Length;
                var pos = max.Lcs.Index2;

                string firstText = " " + text.Substring(0, index) + " ";
                string lastText = " " + text.Substring(index + length) + " ";

                texts.Remove(text);

                Section firstSection = new Section(section.Start, pos + 1, normalizedText);
                Section lastSection = new Section(section.Start + pos + length - 1, section.Length - pos - length + 1, normalizedText);
                Section middleSection = new Section(section.Start + pos + 1, length - 2, rev, normalizedText);

                sections.Remove(section);

                var newS = new[] { firstSection, lastSection }.Where(s => !IsEmpty(s.substring)).ToArray();
                var newT = new[] { firstText, lastText }.Where(t => !IsEmpty(t)).ToArray();
                
                texts.AddRange(newT);
                newTexts.AddRange(newT);
                sections.AddRange(newS);
                newSections.AddRange(newS);

                substrings.RemoveAll(x => x.Text == text || x.Section == section);
                
                if (middleSection.substring.Trim() != "")
                    sections.Add(middleSection);
            }
        }

        public struct LCS
        {
            public int Index1;
            public int Index2;
            public int Length;
        }

        public static LCS LCSubstr(string s1, string s2)
        {
            int[,] L = new int[2, s2.Length];
            int z = 0;
            int foundIndex = int.MaxValue;
            int foundIndex2 = 0;
            for (int i = 0; i < s1.Length; i++)
            {
                var iCur = i % 2;
                for (int j = 0; j < s2.Length; j++)
                {
                    bool first = i == 0 || j == 0 || L[1 - iCur, j - 1] == 0;

                    if (s1[i] == s2[j] && (s1[i] == ' ' || !first))
                    {
                        if (i == 0 || j == 0)
                            L[iCur, j] = 1;
                        else
                            L[iCur, j] = L[1 - iCur, j - 1] + 1;

                        if (s1[i] == ' ' && L[iCur, j] > z)
                        {
                            z = L[iCur, j];
                            foundIndex = i;
                            foundIndex2 = j;
                        }
                    }
                    else
                        L[iCur, j] = 0;
                }
            }

            var lcs = new LCS { Index1 = foundIndex - z + 1, Index2 = foundIndex2 - z + 1, Length = z };
            if (z == 0)
            {
                lcs.Index1 = -1;
                lcs.Index2 = -1;
            }

            return lcs;
        }

        private static bool IsEmpty(string str)
        {
            return str.Length == 0 || str.Trim().Length == 0;
        }
    }

    public partial class MainForm : Form
    {
        BackgroundWorkerWithPause bwAuthors = new BackgroundWorkerWithPause();
        ArticleText thisArticleText;
        int revisionNumber;

        #region button handling
        private void buttonAnalyzeAuthors_Click(object sender, EventArgs e)
        {
            if (revisions == null)
            {
                MessageBox.Show("Please load an article first!", Program.ProgramName);
                return;
            }
            MainForm mf = this;
            if (!bwAuthors.IsBusy)
            {
                revisionNumber = revisions.Count - 1 - comboBoxAnalyzeWhichRevision.SelectedIndex;
                bwAuthors.RunWorkerAsync();
                buttonAnalyzeAuthors.Text = "Отмена";
                buttonAuthorsPause.Visible = true;
                AdjustAuthorsButtons();
            }
            else
            {
                bwAuthors.CancelAsync();
                analyzingEnded();
                stopped = 1;
            }
        }
        private void buttonAuthorsPause_Click(object sender, EventArgs e)
        {
            if (buttonAuthorsPause.Text == "Пауза")
            {
                bwAuthors.Paused = true;
                buttonAuthorsPause.Text = "Продолжить";
                showProgress("Анализ приостановлен.", -1);
            }
            else // Resume
            {
                bwAuthors.Paused = false;
                buttonAuthorsPause.Text = "Пауза";
                showProgress("", -1);
            }
        }
        #endregion

        private delegate void analyzingEndedDelegate();
        private void analyzingEnded()
        {
            buttonAnalyzeAuthors.Text = "Анализировать авторов";
            buttonAuthorsPause.Visible = false;
            AdjustAuthorsButtons();
        }
        public int stopped = 0;
        DateTime we;
        MainForm mf;

        private delegate void SetNameV(string name);
        void SetAutorsData(String a)
        {
            buttonAnalyzeAuthors.Text = "Отмена (" + a + ")";
            // Text = a; 
        }


        public void TimeThread()
        {
            stopped = 0;
            while (1 == 1)
            {
                while (bwAuthors.Paused) { Thread.Sleep(100); }
                if (bwAuthors.CancellationPending)
                {
                    return;
                }
                if (stopped == 1) { return; }
                Thread.Sleep(1000);
                if (DateTime.Now >= we) { return; }
                TimeSpan ts = we - DateTime.Now;

                SetNameV snv = new SetNameV(SetAutorsData);
                this.Invoke(snv, new object[] { ts.Hours + ":" + ts.Minutes + ":" + ts.Seconds });
                //mf.buttonAnalyzeAuthors.Text=ts.Hours + ":" + ts.Minutes + ":" + ts.Seconds;
                //Application.OpenForms[0].Text = "EAD";

            }
        }
        void SendProgressMessage(String msg)
        {
            this.Invoke(new ShowProgressDelegate(showProgress), new object[] { msg, -1 });
        }
        void SendProgressMessage2(String msg, double p)
        {
            this.Invoke(new ShowProgressDelegate(showProgress), new object[] { msg, p });
        }

        private static byte[] DecompressGzip(Stream streamInput)
        {

            Stream streamOutput = new MemoryStream();

            int iOutputLength = 0;

            try
            {

                byte[] readBuffer = new byte[4096 * 256];



                /// read from input stream and write to gzip stream



                using (GZipStream streamGZip = new GZipStream(streamInput, CompressionMode.Decompress))
                {



                    int i;

                    while ((i = streamGZip.Read(readBuffer, 0, readBuffer.Length)) != 0)
                    {

                        streamOutput.Write(readBuffer, 0, i);

                        iOutputLength = iOutputLength + i;

                    }

                }

            }

            catch (Exception ex)
            {

                // todo: handle exception

            }



            /// read uncompressed data from output stream into a byte array



            byte[] buffer = new byte[iOutputLength];

            streamOutput.Position = 0;

            streamOutput.Read(buffer, 0, buffer.Length);



            return buffer;

        }
        private static byte[] DecompressDeflate(Stream streamInput)
        {

            Stream streamOutput = new MemoryStream();

            int iOutputLength = 0;

            try
            {

                byte[] readBuffer = new byte[4096 * 256];



                /// read from input stream and write to gzip stream



                using (DeflateStream streamGZip = new DeflateStream(streamInput, CompressionMode.Decompress))
                {



                    int i;

                    while ((i = streamGZip.Read(readBuffer, 0, readBuffer.Length)) != 0)
                    {

                        streamOutput.Write(readBuffer, 0, i);

                        iOutputLength = iOutputLength + i;

                    }

                }

            }

            catch (Exception ex)
            {

                // todo: handle exception

            }



            /// read uncompressed data from output stream into a byte array



            byte[] buffer = new byte[iOutputLength];

            streamOutput.Position = 0;

            streamOutput.Read(buffer, 0, buffer.Length);



            return buffer;

        }


        private void bwAuthors_DoWork(object sender, DoWorkEventArgs e)
        {


            Revision rev = revisions[revisionNumber];
            if (!rev.fullTextLoaded)
            {
                this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Загрузка текущего текста статьи (" + DateTimeHelpFunctions.DateTimeToString(rev.timestamp) + ")", -1 });
                rev.LoadFullText();
            }
            thisArticleText = new ArticleText(rev.fullText);
            int revtow = 0, totalsize = 0, ls = 0, f2size = 0;

            if (revisionNumber >= 10)
            {

                // now check every revision
                totalsize = 0; ls = 0;
                for (int i = 0; i < revisionNumber; i++)
                {
                    totalsize += Math.Abs((revisions[i].size) - ls);
                    ls = revisions[i].size;
                }
                revtow = revisionNumber / 10;
                if (revtow > 10) { revtow = 10; }
                if (revtow < 4) { revtow = 4; }
                //MessageBox.Show(revtow+"");
                for (int i = 0; i < revtow; i++)
                {
                    f2size += Math.Abs((revisions[i].size) - ls);
                    ls = revisions[i].size;
                }
                //f2size = revisions[0].size + Math.Abs(revisions[1].size - revisions[0].size) + Math.Abs(revisions[2].size - revisions[1].size) + Math.Abs(revisions[3].size - revisions[2].size);

            }
            else { revtow = -1; }
            int wss = 0;

            // Selecting ids

            //Loading them
            String rvstartid = "";
            int rvl = 75;

            int already = 0;
        againss:

            WebClient client1 = new WebClient();
            XmlDocument xmlDoc = new XmlDocument();

            SendProgressMessage2("Загрузка версий .. " + already + "/" + revisionNumber, already / revisionNumber);

            if (already + rvl > revisionNumber) { rvl = revisionNumber - already + 1; }
            String rvlimit = "" + rvl;
            already += rvl;
            String url = Projects.currentProjectBaseUrl + "api.php?format=xml&action=query&prop=revisions&rvlimit=" + rvlimit + "&rvdir=newer&titles=" + HttpUtility.UrlEncode(Revision.currentTitle) + "&rvprop=content|ids&" + rvstartid;

            if (bwAuthors.CancellationPending)
            {
                this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Analyzing authors canceled", -1 });
                return;
            }
            while (bwAuthors.Paused) { Thread.Sleep(100); }

            // String url = "http://toolserver.org/~haffman/wikihistory/grv.php?rvlist="+ldrv;
            client1.Encoding = Encoding.UTF8;
            client1.Headers.Add("User-Agent", "WikiHistory (http://de.wikipedia.org/wiki/Benutzer:APPER/WikiHistory) [he]");
            client1.Headers["Accept-Encoding"] = "gzip";

            //MessageBox.Show(client1.DownloadString(url));
        tryg:
            try
            {
                System.IO.StreamReader webReader;
                webReader = new System.IO.StreamReader(client1.OpenRead(url));
                string sResponseHeader = client1.ResponseHeaders["Content-Encoding"];
                String data = "";
                if (!string.IsNullOrEmpty(sResponseHeader))
                {
                    if (sResponseHeader.ToLower().Contains("gzip"))
                    {

                        byte[] b = DecompressGzip(webReader.BaseStream);

                        data = System.Text.Encoding.GetEncoding(client1.Encoding.CodePage).GetString(b);

                    }

                    else if (sResponseHeader.ToLower().Contains("deflate"))
                    {

                        byte[] b = DecompressDeflate(webReader.BaseStream);

                        data = System.Text.Encoding.GetEncoding(client1.Encoding.CodePage).GetString(b);

                    }
                    else { data = webReader.ReadToEnd(); }
                }
                else { data = webReader.ReadToEnd(); }
                //MessageBox.Show(data);

                //MessageBox.Show(sResponseHeader);
                // byte[] ba = client1.DownloadData(url);
                //MessageBox.Show(ba.Length + "/" + Cache.Decompress2(ba).Length);
                xmlDoc.LoadXml(data/*Cache.Decompress(*//*client1.DownloadString(url)*//*)*/);
            }
            catch (Exception es) { MessageBox.Show(es.Message); SendProgressMessage("Сбой при загрузке версий.. Очередная попытка ..."); Thread.Sleep(1000); goto tryg; }
            //MessageBox.Show("A");
            XmlNode xmlRevisions = xmlDoc.DocumentElement;
            XmlNode currentNode;

            rvstartid = "";
            XmlNode currentNode2 = xmlRevisions.SelectSingleNode("query-continue");
            if (currentNode2 != null)
            {
                currentNode2 = currentNode2.SelectSingleNode("revisions");
                long cntid = 0;
                try { cntid = Convert.ToInt64(currentNode2.Attributes.GetNamedItem("rvstartid").Value); }
                catch { }
                if (cntid > 0) { rvstartid = "" + cntid; }
            }


            currentNode = xmlRevisions.SelectSingleNode("query");
            if (currentNode == null) return;
            currentNode = currentNode.SelectSingleNode("pages");
            if (currentNode == null) return;
            currentNode = currentNode.SelectSingleNode("page");
            if (currentNode == null) return;
            if (currentNode.Attributes.GetNamedItem("missing") != null) return;
            currentNode = currentNode.SelectSingleNode("revisions");
            if (currentNode == null) return;

            foreach (XmlNode xrev in currentNode.SelectNodes("rev"))
            {
                XmlNode attribute;
                long nowid = 0;
                attribute = xrev.Attributes.GetNamedItem("revid");
                if (attribute == null) { continue; }
                try { nowid = Convert.ToInt64(attribute.Value); }
                catch { continue; }
                // MessageBox.Show(xrev.InnerText);
                for (int i = 0; i <= revisionNumber; i++)
                {
                    if (revisions[i].id == nowid)
                    {
                        revisions[i].SetFullText(xrev.InnerText);
                        // MessageBox.Show("EBF");
                        break;
                    }
                }
            }
            if (already > revisionNumber) { }
            else if (rvstartid != "") { rvstartid = "rvstartid=" + rvstartid; goto againss; }

            DateTime tf = DateTime.Now;
            DateTime t1 = DateTime.Now;
            //Loaded

            for (int i = 0; i <= revisionNumber; i++) // < revisions.Count
            {

                if (bwAuthors.CancellationPending)
                {
                    this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Analyzing authors canceled", -1 });
                    return;
                }
                while (bwAuthors.Paused) { Thread.Sleep(100); }


                // if the next version is from the same author, this version could be skipped
                if ((i < revisionNumber) && (revisions[i].user == revisions[i + 1].user)) continue;


                // get revision full text
                rev = revisions[i];
                int sleepms = 3000;
                int trying = 1;
            again:
                if (!rev.fullTextLoaded)
                {
                    this.Invoke(new ShowProgressDelegate(showProgress), new object[] { (trying > 1 ? "(попытка " + trying : "") + "Загрузка текста для версии " + rev.id.ToString() + " (" + DateTimeHelpFunctions.DateTimeToString(rev.timestamp) + ") .... " + (100 * i / revisionNumber) + "%", (double)i / revisionNumber });
                    rev.LoadFullText();
                }
                if (!rev.fullTextLoaded)
                {
                    Thread.Sleep(sleepms);
                    sleepms *= 2;
                    trying++;
                    goto again;
                }
                if (rev.fullText.Length == 0) { continue; }//vandal 99.999999%
                //DateTime t3 = DateTime.Now;
                this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Анализ версии " + rev.id.ToString() + " (" + DateTimeHelpFunctions.DateTimeToString(rev.timestamp) + "; " + rev.size + " байт; " + (i > 0 ? "" + (rev.size - revisions[i - 1].size) + " байт" : "") + ")  .... " + (100 * i / revisionNumber) + "%", (double)i / revisionNumber });
                thisArticleText.MarkNewSections(rev, bwAuthors);
                //DateTime t4 = DateTime.Now;
                //TimeSpan t7 = t4 - t3;
                //MessageBox.Show(i + ": " + (t7.Hours * 3600000 + t7.Minutes * 60000 + t7.Seconds * 1000.0 + t7.Milliseconds));

                TimeSpan ts = DateTime.Now - tf;
                double ws = ts.Hours * 3600000 + ts.Minutes * 60000 + ts.Seconds * 1000 + ts.Milliseconds;

                f2size = 0; ls = 0;
                for (int iq = 0; iq < i; iq++)
                {
                    f2size += Math.Abs((revisions[iq].size) - ls);
                    ls = revisions[iq].size;
                }
                double ps = f2size / ws;//байт в миллисекунду
                we = DateTime.Now.AddSeconds((totalsize - f2size) * ps / 1000);
                if (i == 0) { tf = DateTime.Now.AddSeconds(5); continue; }
                //MessageBox.Show(we.Year + "." + we.Month + "." + we.Day + " " + we.Hour + ":" + we.Minute + ":" + we.Second+" (ws="+ws+" ps="+ps+" f2size="+f2size+" totalsize="+totalsize);
                if (i > 2 && wss == 0 && ((totalsize - f2size) * ps / 1000) > 15) { new Thread(TimeThread).Start(); wss = 1; }

            }
            stopped = 1;
            thisArticleText.ready = true;

            this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Статья полностью проанализирована...", 1 });

            // prepare user data
            int totalLength = 0;
            foreach (ArticleText.Section s in thisArticleText.sections)
                if (s.revKnown) totalLength += s.Length;
            foreach (User u in users)
            {
                u.lengthOfContentAdded = 0;
                foreach (ArticleText.Section s in thisArticleText.sections)
                {
                    if ((s.revKnown) && (s.rev.user == u.name))
                        u.lengthOfContentAdded += s.Length;
                }
                if (thisArticleText.text.Length > 0)
                    u.percentageOfContentAdded = (double)u.lengthOfContentAdded / totalLength;
            }
            prepareListViewUsers();

            List<User> topUsers = new List<User>();
            User maxUser;
            for (int i = 0; i < 5; i++)
            {
                maxUser = null;
                foreach (User u in users) { if ((!topUsers.Contains(u)) && (u.lengthOfContentAdded > 0) && ((maxUser == null) || (u.lengthOfContentAdded > maxUser.lengthOfContentAdded))) maxUser = u; }
                topUsers.Add(maxUser);
            }

            //this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "", -1 });


            DateTime t2 = DateTime.Now;
            TimeSpan t = t2 - t1;//(t.Hours*3600000 + t.Minutes*60000 + t.Seconds * 1000.0 + t.Milliseconds)
            this.Invoke(new ShowProgressDelegate(showProgress), new object[] { "Обработка заняла " + t.Hours + " ч " + (t.Minutes % 60) + " мин " + (t.Seconds % 60) + " с " + (t.Milliseconds % 1000) + " мс", -1 });
            this.Invoke(new analyzingEndedDelegate(analyzingEnded));
            this.Invoke(new AuthorsPrepareColoredComboBoxesDelegate(authorsPrepareColoredComboBoxes), new object[] { topUsers });
        }

        private delegate void AuthorsPrepareColoredComboBoxesDelegate(List<User> topUsers);
        private void authorsPrepareColoredComboBoxes(List<User> topUsers)
        {
            if (topUsers[0] != null) comboBoxUserColor1.SelectedItem = topUsers[0].name; else comboBoxUserColor1.SelectedIndex = -1;
            if (topUsers[1] != null) comboBoxUserColor2.SelectedItem = topUsers[1].name; else comboBoxUserColor2.SelectedIndex = -1;
            if (topUsers[2] != null) comboBoxUserColor3.SelectedItem = topUsers[2].name; else comboBoxUserColor3.SelectedIndex = -1;
            if (topUsers[3] != null) comboBoxUserColor4.SelectedItem = topUsers[3].name; else comboBoxUserColor4.SelectedIndex = -1;
            if (topUsers[4] != null) comboBoxUserColor5.SelectedItem = topUsers[4].name; else comboBoxUserColor5.SelectedIndex = -1;

            showAuthorsColoredRichTextBox(richTextBox1);
        }

        private void showAuthorsColoredRichTextBox(RichTextBox rtb)
        {
            rtb.Clear();
            if ((thisArticleText == null) || (!thisArticleText.ready)) return;
            rtb.Text = thisArticleText.text;
            Color c;
            foreach (ArticleText.Section s in thisArticleText.sections)
            {
                rtb.Select(s.Start - 1, s.Length);
                c = Color.White;
                if (s.revKnown)
                {
                    if (s.rev.user == comboBoxUserColor1.Text)
                        c = comboBoxUserColor1.BackColor;
                    else if (s.rev.user == comboBoxUserColor2.Text)
                        c = comboBoxUserColor2.BackColor;
                    else if (s.rev.user == comboBoxUserColor3.Text)
                        c = comboBoxUserColor3.BackColor;
                    else if (s.rev.user == comboBoxUserColor4.Text)
                        c = comboBoxUserColor4.BackColor;
                    else if (s.rev.user == comboBoxUserColor5.Text)
                        c = comboBoxUserColor5.BackColor;
                }
                else
                    c = Color.LightPink; // TODO: don't needed normally...
                if (c != Color.White)
                {
                    if (!Program.Mono)
                        rtb.SelectionBackColor = c;
                    else
                        rtb.SelectionColor = c;
                }
            }
        }

        private string getAuthorsColoredHTML(bool wiki)
        {
            if ((thisArticleText == null) || (!thisArticleText.ready)) return "";
            HTMLHelper h = new HTMLHelper(thisArticleText.text);
            string starttag, endtag;
            foreach (ArticleText.Section s in thisArticleText.sections)
            {
                string color = "";
                if (s.revKnown)
                {
                    if (s.rev.user == comboBoxUserColor1.Text)
                        color = HTMLHelper.ColorToRGB(comboBoxUserColor1.BackColor);
                    else if (s.rev.user == comboBoxUserColor2.Text)
                        color = HTMLHelper.ColorToRGB(comboBoxUserColor2.BackColor);
                    else if (s.rev.user == comboBoxUserColor3.Text)
                        color = HTMLHelper.ColorToRGB(comboBoxUserColor3.BackColor);
                    else if (s.rev.user == comboBoxUserColor4.Text)
                        color = HTMLHelper.ColorToRGB(comboBoxUserColor4.BackColor);
                    else if (s.rev.user == comboBoxUserColor5.Text)
                        color = HTMLHelper.ColorToRGB(comboBoxUserColor5.BackColor);
                }
                else
                    color = HTMLHelper.ColorToRGB(Color.LightPink);  // TODO: don't needed normally...

                if (s.rev != null)
                {
                    if (color != "")
                        starttag = "<span style=\"background-color:#" + color + "\" title=\"User: " + s.rev.user + "\">";
                    else
                        starttag = "<span title=\"User: " + s.rev.user + "\">";
                }
                else
                    starttag = "<span title=\"Unknown User\">";
                endtag = "</span>";
                if (wiki)
                {
                    starttag = "</nowiki>" + starttag + "<nowiki>";
                    endtag = "</nowiki>" + endtag + "<nowiki>";
                }
                h.SetTag(s.Start - 1, s.Length, starttag, endtag);
            }
            string html = h.GetHTML();
            if (wiki)
            {
                html = "<nowiki>" + html + "</nowiki>";
                html = html.Replace("\n", "</nowiki><br />\n<nowiki>");
            }
            return html;
        }

        #region handling of user color combo boxes
        private void comboBoxUserColor_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void comboBoxUserColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            showAuthorsColoredRichTextBox(richTextBox1);
        }

        private void comboBoxUserColor_TextUpdate(object sender, EventArgs e)
        {
            showAuthorsColoredRichTextBox(richTextBox1);
        }
        #endregion
    }
}
