using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows.Forms;

namespace FileSearcher
{
    delegate void MyDelegate(string text);
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            GetDrives();
        }

        DriveInfo[] driveInfos = DriveInfo.GetDrives();

        private void GetDrives()
        {
            foreach (var drive in driveInfos)
            {
                checkedListBox1.Items.Add(string.Format(drive.Name));
            }
            checkedListBox1.Items.Add(string.Format("Browse..."));
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            checkedListBox1.Items.RemoveAt(checkedListBox1.Items.Count - 1);
            checkedListBox1.Items.Add(string.Format(folderBrowser.SelectedPath), true);
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            textBox1.SelectAll();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonSearch_Click(sender, e);
            }
        }

        ToolTip tp = new ToolTip();

        private void checkedListBox1_MouseMove(object sender, MouseEventArgs e) // tooltip for checkedListBox1
        {
            int index = checkedListBox1.IndexFromPoint(e.Location);

            if (index >= 0 && index < checkedListBox1.Items.Count - 1)
            {
                tp.SetToolTip(checkedListBox1, driveInfos[index].DriveType.ToString());
            }
            else if (index == checkedListBox1.Items.Count - 1)
            {
                tp.SetToolTip(checkedListBox1, checkedListBox1.Items[index].ToString());
            }
        }

        static object block = new object();

        int count = 0, founded = 0;

        private void FileSearch(object mySearchedFile)
        {
            count++;
            SearchAttributes searchedFile = (SearchAttributes)mySearchedFile;
            string searchPath = searchedFile.SearchPath;
            string searchPattern = searchedFile.SearchPattern;

            lock (block)
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(searchPath);

                    FileInfo[] fileInfo;

                    fileInfo = dir.GetFiles(searchPattern);

                    foreach (var item in fileInfo)
                    {
                        founded++;
                        listBox1.Invoke(new MyDelegate((s) => listBox1.Items.Add(s)), item.FullName);
                        textBox2.Invoke(new MyDelegate((s) => textBox2.Text = s), "Please wait, searching... Founded " + founded + " files:");
                    }

                    DirectoryInfo[] dirInfo = dir.GetDirectories();

                    foreach (var item in dirInfo)
                    {
                        if (item.Attributes.Equals(FileAttributes.System | FileAttributes.Hidden | FileAttributes.Directory))
                        {
                            listBox1.Invoke(new MyDelegate((s) => listBox1.Items.Add(s)), "The process failed: " + item.FullName + " " + item.Attributes);
                            continue;
                        }

                        SearchAttributes search = new SearchAttributes
                        {
                            SearchPath = item.FullName.ToString(),
                            SearchPattern = textBox1.Text
                        };

                        FileSearch(search);
                    }
                }
                catch (Exception e)
                {
                    textBox2.Invoke(new MyDelegate((s) => textBox2.Text = s), "The process failed: " + e.Message);

                    // Add all errors to the same list box where the list of found files.
                    listBox1.Invoke(new MyDelegate((s) => listBox1.Items.Add(s)), "The process failed: " + e.Message);

                }
            }

            count--;

            if (count == 0)
            {
                textBox2.Invoke(new MyDelegate((s) => textBox2.Text = s), "Founded " + founded + " files:");
                MessageBox.Show("Founded " + founded + " files", "Search finished!");
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            textBox2.Text = "Please wait, searching...";

            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    SearchAttributes search = new SearchAttributes
                    {
                        SearchPath = checkedListBox1.Items[i].ToString(),
                        SearchPattern = textBox1.Text
                    };

                    Thread th = new Thread(FileSearch);
                    // run the search in the new stream so that the search window does not hang
                    th.Start(search);
                }
            }
        }

        private void buttonView_Click(object sender, EventArgs e)
        {
            try
            {
                StreamReader reader = File.OpenText(listBox1.SelectedItem.ToString());

                MessageBox.Show(reader.ReadToEnd());

                reader.Close();
            }
            catch (Exception ex)
            {
                textBox2.Text = "The process failed: " + ex.Message;
            }
        }

        private void buttonZIP_Click(object sender, EventArgs e)
        {
            try
            {
                FileStream source = File.OpenRead(listBox1.SelectedItem.ToString());

                SaveFileDialog saveZipFile = new SaveFileDialog();
                saveZipFile.ShowDialog();

                FileStream destination = File.Create(saveZipFile.FileName + ".gz");

                GZipStream compressor = new GZipStream(destination, CompressionMode.Compress);

                source.CopyTo(compressor); 

                compressor.Close();
            }
            catch (Exception ex)
            {
                textBox2.Text = "The process failed: " + ex.Message;
            }
        }
    }
}
