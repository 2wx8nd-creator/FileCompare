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
        private void btnCopyFromLeft_Click(object sender, EventArgs e) { }
        private void btnCopyFromRight_Click(object sender, EventArgs e) { }

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
                    // 1. 이름, 크기, 수정시간이 모두 같은 경우
                    if (leftFile.Length == rightFile.Length && leftFile.LastWriteTime.Equals(rightFile.LastWriteTime))
                    {
                        leftItem.ForeColor = Color.Black;
                        rightItem.ForeColor = Color.Black;
                    }
                    else
                    {
                        // 2. 이름은 같지만 데이터(크기/시간)가 다른 경우 [수정 부분]
                        if (leftFile.LastWriteTime > rightFile.LastWriteTime)
                        {
                            leftItem.ForeColor = Color.Red;     // 최신 파일
                            rightItem.ForeColor = Color.Gray;    // [수정됨] 오래된 파일
                        }
                        else if (leftFile.LastWriteTime < rightFile.LastWriteTime)
                        {
                            leftItem.ForeColor = Color.Gray;    // [수정됨] 오래된 파일
                            rightItem.ForeColor = Color.Red;     // 최신 파일
                        }
                        else
                        {
                            // 수정시간은 같은데 크기가 다른 경우 등 예외 처리
                            leftItem.ForeColor = Color.Red;
                            rightItem.ForeColor = Color.Red;
                        }
                    }
                }
                else
                {
                    // 3. 한 쪽에만 파일이 있는 경우
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