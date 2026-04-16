using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace FileCompare
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            lvwLeftDir.MouseDoubleClick += LvwLeftDir_MouseDoubleClick;
            lvwrightDir.MouseDoubleClick += LvwRightDir_MouseDoubleClick;
        }

        private void Form1_Load(object sender, EventArgs e) { }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }

        private void LvwLeftDir_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            HandleFolderNavigation(lvwLeftDir, true);
        }

        private void LvwRightDir_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            HandleFolderNavigation(lvwrightDir, false);
        }

        private void HandleFolderNavigation(ListView listView, bool isLeft)
        {
            if (listView.SelectedItems.Count > 0)
            {
                var item = listView.SelectedItems[0];
                if (item.Tag is DirectoryInfo dirInfo)
                {
                    string targetName = dirInfo.Name;
                    string newLeftPath = Path.Combine(txtLeftDir.Text, targetName);
                    string newRightPath = Path.Combine(txtRightDir.Text, targetName);

                    if (Directory.Exists(newLeftPath) && Directory.Exists(newRightPath))
                    {
                        txtLeftDir.Text = newLeftPath;
                        txtRightDir.Text = newRightPath;
                        CompareAndDisplayFolders();
                    }
                    else if (isLeft && Directory.Exists(newLeftPath))
                    {
                        txtLeftDir.Text = newLeftPath;
                        CompareAndDisplayFolders();
                    }
                    else if (!isLeft && Directory.Exists(newRightPath))
                    {
                        txtRightDir.Text = newRightPath;
                        CompareAndDisplayFolders();
                    }
                }
                else if (item.Text == "[..]")
                {
                    txtLeftDir.Text = Directory.GetParent(txtLeftDir.Text)?.FullName ?? txtLeftDir.Text;
                    txtRightDir.Text = Directory.GetParent(txtRightDir.Text)?.FullName ?? txtRightDir.Text;
                    CompareAndDisplayFolders();
                }
            }
        }

        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {
            ExecuteCopy(txtLeftDir.Text, txtRightDir.Text);
        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {
            ExecuteCopy(txtRightDir.Text, txtLeftDir.Text);
        }

        private void btnLeftDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                if (!string.IsNullOrWhiteSpace(txtLeftDir.Text) && Directory.Exists(txtLeftDir.Text))
                {
                    dlg.SelectedPath = txtLeftDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtLeftDir.Text = dlg.SelectedPath;
                    CompareAndDisplayFolders();
                }
            }
        }

        private void btnRightDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                if (!string.IsNullOrWhiteSpace(txtRightDir.Text) && Directory.Exists(txtRightDir.Text))
                {
                    dlg.SelectedPath = txtRightDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtRightDir.Text = dlg.SelectedPath;
                    CompareAndDisplayFolders();
                }
            }
        }

        private void ExecuteCopy(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(sourcePath) || !Directory.Exists(targetPath)) return;

            DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);
            FileInfo[] sourceFiles = sourceDir.GetFiles();

            foreach (var srcFile in sourceFiles)
            {
                string destFile = Path.Combine(targetPath, srcFile.Name);

                if (File.Exists(destFile))
                {
                    FileInfo targetFile = new FileInfo(destFile);

                    if (srcFile.Length == targetFile.Length && srcFile.LastWriteTime.Equals(targetFile.LastWriteTime))
                    {
                        continue;
                    }

                    if (srcFile.LastWriteTime > targetFile.LastWriteTime)
                    {
                        File.Copy(srcFile.FullName, destFile, true);
                    }
                    else if (srcFile.LastWriteTime < targetFile.LastWriteTime)
                    {
                        string msg = $"{srcFile.Name}은 대상 폴더의 파일보다 오래된 버전입니다. 정말 덮어씌우시겠습니까?";
                        if (MessageBox.Show(msg, "복사 확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            File.Copy(srcFile.FullName, destFile, true);
                        }
                    }
                }
                else
                {
                    File.Copy(srcFile.FullName, destFile, true);
                }
            }
            CompareAndDisplayFolders();
        }

        private void CompareAndDisplayFolders()
        {
            string leftPath = txtLeftDir.Text;
            string rightPath = txtRightDir.Text;

            if (!Directory.Exists(leftPath) || !Directory.Exists(rightPath)) return;

            lvwLeftDir.Items.Clear();
            lvwrightDir.Items.Clear();

            DirectoryInfo leftDir = new DirectoryInfo(leftPath);
            DirectoryInfo rightDir = new DirectoryInfo(rightPath);

            if (leftDir.Parent != null || rightDir.Parent != null)
            {
                lvwLeftDir.Items.Add(new ListViewItem("[..]"));
                lvwrightDir.Items.Add(new ListViewItem("[..]"));
            }

            var leftSubDirs = leftDir.GetDirectories();
            var rightSubDirs = rightDir.GetDirectories();
            var allDirNames = leftSubDirs.Select(d => d.Name).Union(rightSubDirs.Select(d => d.Name)).OrderBy(n => n);

            foreach (var dirName in allDirNames)
            {
                DirectoryInfo lDir = leftSubDirs.FirstOrDefault(d => d.Name == dirName);
                DirectoryInfo rDir = rightSubDirs.FirstOrDefault(d => d.Name == dirName);

                ListViewItem lItem = CreateDirItem(lDir);
                ListViewItem rItem = CreateDirItem(rDir);

                if (lDir != null && rDir != null)
                {
                    lItem.ForeColor = Color.Black;
                    rItem.ForeColor = Color.Black;
                }
                else
                {
                    if (lDir != null) lItem.ForeColor = Color.Purple;
                    if (rDir != null) rItem.ForeColor = Color.Purple;
                }

                lvwLeftDir.Items.Add(lItem);
                lvwrightDir.Items.Add(rItem);
            }

            FileInfo[] leftFiles = leftDir.GetFiles();
            FileInfo[] rightFiles = rightDir.GetFiles();
            var allFileNames = leftFiles.Select(f => f.Name).Union(rightFiles.Select(f => f.Name)).OrderBy(n => n);

            foreach (var fileName in allFileNames)
            {
                FileInfo leftFile = leftFiles.FirstOrDefault(f => f.Name == fileName);
                FileInfo rightFile = rightFiles.FirstOrDefault(f => f.Name == fileName);

                ListViewItem leftItem = CreateFileItem(leftFile);
                ListViewItem rightItem = CreateFileItem(rightFile);

                if (leftFile != null && rightFile != null)
                {
                    if (leftFile.Length == rightFile.Length && leftFile.LastWriteTime.Equals(rightFile.LastWriteTime))
                    {
                        leftItem.ForeColor = Color.Black;
                        rightItem.ForeColor = Color.Black;
                    }
                    else
                    {
                        if (leftFile.LastWriteTime > rightFile.LastWriteTime)
                        {
                            leftItem.ForeColor = Color.Red;
                            rightItem.ForeColor = Color.Gray;
                        }
                        else
                        {
                            leftItem.ForeColor = Color.Gray;
                            rightItem.ForeColor = Color.Red;
                        }
                    }
                }
                else
                {
                    if (leftFile != null) leftItem.ForeColor = Color.Purple;
                    if (rightFile != null) rightItem.ForeColor = Color.Purple;
                }

                lvwLeftDir.Items.Add(leftItem);
                lvwrightDir.Items.Add(rightItem);
            }
        }

        private ListViewItem CreateDirItem(DirectoryInfo dir)
        {
            if (dir == null) return new ListViewItem(new string[] { "", "", "" });
            var item = new ListViewItem(new string[] { $"[{dir.Name}]", "<DIR>", dir.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") });
            item.Tag = dir;
            return item;
        }

        private ListViewItem CreateFileItem(FileInfo file)
        {
            if (file == null) return new ListViewItem(new string[] { "", "", "" });
            return new ListViewItem(new string[] { file.Name, file.Length.ToString("N0") + " bytes", file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") });
        }
    }
}