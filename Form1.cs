using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace logviewer
{
    public partial class Form1 : Form
    {
        private LogFile _selectedLogFile = null;

        public class AppSettings
        {
            public string LastOpenFolderPath;
        }

        public static AppSettings appSettings;

        public Form1()
        {
            InitializeComponent();

            try
            {
                var str = ReadAllText("defaults.json");
                var js = Newtonsoft.Json.JsonSerializer.CreateDefault();
                appSettings = js.Deserialize<AppSettings>(new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(str)));
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }

            if (appSettings == null)
            {
                appSettings = new AppSettings();
            }
            else
            {
                if (!string.IsNullOrEmpty(appSettings.LastOpenFolderPath))
                {
                    LogManager.GetInstance().AddFolder(appSettings.LastOpenFolderPath);
                    SyncTreeView();
                }
            }
        }

        public static string ReadAllText(string filename)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var filepath = System.IO.Path.Combine(path, filename);
            if (System.IO.File.Exists(filepath))
            {
                return System.IO.File.ReadAllText(filepath);
            }
            return string.Empty;
        }

        public static void WriteAllText(string filename, string text)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var filepath = System.IO.Path.Combine(path, filename);
            System.IO.File.WriteAllText(filepath, text);
        }

        public static void SaveAppSettings()
        {
            try
            {
                var js = Newtonsoft.Json.JsonSerializer.CreateDefault();
                var w = new System.IO.StringWriter();
                js.Serialize(w, appSettings);
                WriteAllText("defaults.json", w.ToString());
            }
            catch (Exception)
            {
            }
        }

        private void SyncFolder(TreeNode folderNode, LogFolder folder)
        {
            for (var i = 0; i < folder.Count; i++)
            {
                var folderfile = folder.GetFile(i);
                TreeNode fileNode = null;
                foreach (TreeNode iNode in folderNode.Nodes)
                {
                    if (iNode.Tag == folderfile)
                    {
                        fileNode = iNode;
                        break;
                    }
                }
                if (fileNode == null)
                {
                    fileNode = new TreeNode();
                    fileNode.Text = folderfile.Name;
                    fileNode.ToolTipText = folderfile.FullName;
                    fileNode.Tag = folderfile;
                    folderNode.Nodes.Add(fileNode);
                }
            }
        }

        // 同步目录列表
        private void SyncTreeView()
        {
            foreach (var folderKV in LogManager.GetInstance().folders)
            {
                TreeNode node = null;
                foreach (TreeNode iNode in treeView1.Nodes)
                {
                    if (iNode.Tag == folderKV.Value)
                    {
                        node = iNode;
                        break;
                    }
                }
                if (node == null)
                {
                    node = new TreeNode();
                    node.Text = folderKV.Value.Name;
                    node.ToolTipText = folderKV.Value.FullName;
                    node.Tag = folderKV.Value;
                    node.Expand();
                    treeView1.Nodes.Add(node);
                }
                SyncFolder(node, folderKV.Value);
            }
        }

        private void openOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = appSettings.LastOpenFolderPath ?? "";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                appSettings.LastOpenFolderPath = dialog.SelectedPath;
                SaveAppSettings();
                LogManager.GetInstance().AddFolder(dialog.SelectedPath);
                SyncTreeView();
            }
        }

        private void RefreshLogFile()
        {
            if (_selectedLogFile == null)
            {
                listView1.VirtualListSize = 0;
                return;
            }
            listView1.VirtualListSize = _selectedLogFile.Count;
            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.Columns[1].Width = 200;
            listView1.Columns[2].Width = 120;
            listView1.Columns[3].Width = 500;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var logFile = e.Node.Tag as LogFile;
            if (logFile != null && _selectedLogFile != logFile)
            {
                _selectedLogFile = logFile;
                RefreshLogFile();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count != 1)
            {
                return;
            }
            var index = listView1.SelectedIndices[0];
            var entry = _selectedLogFile.GetEntry(index);
            if (entry != null)
            {
                listView2.Items.Clear();
                if (entry.objects != null)
                {
                    foreach (var kv in entry.objects)
                    {
                        var item = new ListViewItem();
                        item.Text = kv.Key;
                        var value = kv.Value.ToString();
                        value = value.Replace("\n", "\r\n");
                        var valueSubItem = new ListViewItem.ListViewSubItem();
                        valueSubItem.Text = value;
                        item.ToolTipText = value;
                        item.SubItems.Add(valueSubItem);
                        listView2.Items.Add(item);
                    }
                }
                //textBox1.Text = entry.fulltext;
            }
        }

        private void listView1_RetrieveVirtualItem_1(object sender, RetrieveVirtualItemEventArgs e)
        {
            //Console.WriteLine("retrieve {0}", e.ItemIndex);
            var entry = _selectedLogFile.GetEntry(e.ItemIndex);
            e.Item = new ListViewItem();
            e.Item.Tag = entry;
            e.Item.Text = entry.level.ToString();
            e.Item.SubItems.Add(entry.time.ToString());
            e.Item.SubItems.Add(_selectedLogFile.Name);
            e.Item.SubItems.Add(entry.message);
            switch (entry.level)
            {
                case LogLevel.DEBUG: e.Item.BackColor = Color.LightGray; break;
                case LogLevel.WARN: e.Item.BackColor = Color.Yellow; break;
                case LogLevel.ERROR: e.Item.BackColor = Color.Red; break;
                case LogLevel.DPANIC: e.Item.BackColor = Color.Red; break;
                case LogLevel.PANIC: e.Item.BackColor = Color.Red; break;
                case LogLevel.FATAL: e.Item.BackColor = Color.Red; break;
                default: break;
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count != 1)
            {
                return;
            }
            var item = listView2.SelectedItems[0];
            textBox1.Text = item.SubItems[1].Text;
        }
    }
}
