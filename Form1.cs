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
        }

        private void Form1_Load(object sender, EventArgs e) { }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }

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
                    else
                    {
                        File.Copy(srcFile.FullName, destFile, true);
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

            FileInfo[] leftFiles = leftDir.GetFiles();
            FileInfo[] rightFiles = rightDir.GetFiles();

            var allFileNames = leftFiles.Select(f => f.Name)
                .Union(rightFiles.Select(f => f.Name))
                .OrderBy(n => n);

            foreach (var fileName in allFileNames)
            {
                FileInfo leftFile = leftFiles.FirstOrDefault(f => f.Name == fileName);
                FileInfo rightFile = rightFiles.FirstOrDefault(f => f.Name == fileName);

                ListViewItem leftItem = CreateListViewItem(leftFile);
                ListViewItem rightItem = CreateListViewItem(rightFile);

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
                        else if (leftFile.LastWriteTime < rightFile.LastWriteTime)
                        {
                            leftItem.ForeColor = Color.Gray;
                            rightItem.ForeColor = Color.Red;
                        }
                        else
                        {
                            leftItem.ForeColor = Color.Red;
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

        private ListViewItem CreateListViewItem(FileInfo file)
        {
            if (file == null) return new ListViewItem(new string[] { "", "", "" });

            return new ListViewItem(new string[]
            {
                file.Name,
                file.Length.ToString("N0") + " bytes",
                file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
}