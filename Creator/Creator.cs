using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Creator
{
    public partial class Creator : Form
    {
        public static string FilePath;
        public static string FolderPath;

        public Creator()
        {
            InitializeComponent();
        }

        public void GetFilePath()
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.InitialDirectory = "C:\\";
                fileDialog.Filter = "txt files (*.txt) | *.txt | All files(*.*) | *.*";
                fileDialog.FilterIndex = 2;
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    FilePath = fileDialog.FileName;
                    txtFilePath.Text = FilePath;
                }
            }
        }

        private void GetFolderPath()
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select bin directory.";
                folderDialog.ShowNewFolderButton = false;
                folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    FolderPath = folderDialog.SelectedPath;
                    txtFolderPath.Text = FolderPath;
                }
            }
        }

        private void DisplayDownloadProgress(string info)
        {
            rtbMain.Text += info + Environment.NewLine;
        }

        private async Task GetDownloadInfoAsync()
        {
            string[] commands = Connector.MakeCommand();

            if (commands != null)
            {
                for (int i = 0; i < commands.Length; i++)
                {
                    string info = commands[i];
                    await Task.Run(() => Thread.Sleep(100));
                    DisplayDownloadProgress(info);
                }                
            }
            else
            {
                DisplayDownloadProgress("- - - Aborted - - -");             
            }
        }

        private void ActivateFinishBtn()
        {
            if (FilePath != null && FolderPath != null)
            {
                btnFinish.Enabled = true;
            }
        }

        private void ActivateExecuteBtn()
        {
            if (FilePath != null && FolderPath != null)
            {
                btnExecute.Enabled = true;
            }
        }

        #region Events        

        private async void btnExecute_Click(object sender, EventArgs e)
        {
            Connector.ExecuteCommand();
            await GetDownloadInfoAsync();            
            ActivateFinishBtn();
        }

        private void btnFilePath_Click(object sender, EventArgs e)
        {
            rtbMain.Text = "";
            GetFilePath();
            ActivateExecuteBtn();
        }

        private void btnFolderPath_Click(object sender, EventArgs e)
        {
            rtbMain.Text = "";
            GetFolderPath();
            ActivateExecuteBtn();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        private void btnFinish_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
