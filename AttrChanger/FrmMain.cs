using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AttrChanger
{
    public partial class FrmMain : Form
    {
        private FileUtils Utils = new FileUtils();
        private BindingSource BindingSource = new BindingSource();
        private List<string> FilePathList = new List<string>();
        private List<FileInfoExtended> FilesInfoExtended = new List<FileInfoExtended>();
        private FileInfoExtended CurrentFile;
        private string InfoMessage = "Attr Changer V1.1";

        public FrmMain()
        {
            InitializeComponent();

            //Add drag and drop
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(MainForm_DragEnter);
            this.DragDrop += new DragEventHandler(MainForm_DragDrop);
            this.openToolStripButton.Click += ToolStrip_OpenFileClicked;
            this.saveToolStripButton.Click += BtnSave_Click;
            this.infoToolStripButton.Click += BtnShowInfo_Click;

            //Setup dataGrids
            dgvMetaData.ReadOnly = true;
            dgvMetaData.AllowUserToResizeColumns = true;
            dgvMetaData.AllowUserToResizeRows = true;

            dgvFiles.ReadOnly = true;
            dgvFiles.AllowUserToResizeColumns = true;
            dgvFiles.AllowUserToResizeRows = true;

            //Resize columns in DataGridView
            dgvMetaData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void ToolStrip_OpenFileClicked(object sender, EventArgs e)
        {
            string path = OpenFileDialog();
            if (!string.IsNullOrEmpty(path))
            {
                FilePathList = Utils.SearchFiles(path, ref toolStripLabel1, ref toolStripPrgsBar).Select(o => o.Value).ToList();
                LoadFilesToDataGrid();
            }
        }

        private void LoadFilesToDataGrid()
        {
            try
            {
                //Initialise FileObj
                FilesInfoExtended = FilePathList.Select(o => new FileInfoExtended(o)).ToList();

                //Refresh grids
                LoadDataBinding_Files();
            }
            catch (DirectoryNotFoundException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDataBinding_Files()
        {
            BindingSource.DataSource = FilesInfoExtended;
            dgvFiles.DataSource = BindingSource;
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            //Add files
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            FilePathList = files.ToList();
            LoadFilesToDataGrid();
        }

        private void DgvFiles_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvFiles.SelectedRows.Count > 0)
            {
                FileInfoExtended selectedFile = dgvFiles.SelectedRows[0].DataBoundItem as FileInfoExtended;
                UpdatePanelsInfo(selectedFile);
            }
        }

        private void UpdatePanelsInfo(FileInfoExtended selectedFile)
        {
            //Locations label
            lblLocation.Text = selectedFile.Location;

            //Load Metadatas
            dgvMetaData.DataSource = selectedFile.Metadatas;

            //Update calendars info
            dtpCreated.Value = DateTime.Parse(selectedFile.Created);
            dtpModified.Value = DateTime.Parse(selectedFile.Modified);
            dtpAccessed.Value = DateTime.Parse(selectedFile.LastAccessed);

            //Update attributes info
            chkHidden.Checked = selectedFile.ExtendedInfo.Attributes.HasFlag(FileAttributes.Hidden);
            chkReadOnly.Checked = selectedFile.ExtendedInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
            chkCompressed.Checked = selectedFile.ExtendedInfo.Attributes.HasFlag(FileAttributes.Compressed);

            //Refrsh selected file
            CurrentFile = selectedFile;
        }

        private string OpenFileDialog()
        {
            var filePath = string.Empty;
            string folderSel = "Folder Selection";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.ValidateNames = false;
                openFileDialog.CheckFileExists = false;
                openFileDialog.CheckPathExists = true;
                openFileDialog.FileName = folderSel;

                openFileDialog.InitialDirectory = "C:\\";
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName.Replace(folderSel, "");
                }
            }

            return filePath;
        }

        private void BtnShowInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(InfoMessage, "About...", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (CurrentFile != null)
            {
                //Update DateTimes
                File.SetCreationTime(CurrentFile.Location, dtpCreated.Value);
                File.SetLastWriteTime(CurrentFile.Location, dtpModified.Value);
                File.SetLastAccessTime(CurrentFile.Location, dtpAccessed.Value);

                //Update file attributes
                if (chkHidden.Checked) { File.SetAttributes(CurrentFile.Location, FileAttributes.Hidden); }
                if (chkReadOnly.Checked) { File.SetAttributes(CurrentFile.Location, FileAttributes.ReadOnly); }
                if (chkCompressed.Checked) { File.SetAttributes(CurrentFile.Location, FileAttributes.Compressed); }
            }
        }
    }
}