/*
Scott Willeke - 2004
http://scott.willeke.com 
Consider this code licensed under Common Public License Version 1.0 (http://www.opensource.org/licenses/cpl1.0.txt).
*/
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Microsoft.Tools.WindowsInstallerXml;
using Microsoft.Tools.WindowsInstallerXml.Msi;
using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;
using System.Diagnostics;
using Microsoft.Tools.WindowsInstallerXml.Cab;

namespace Willeke.Scott.LessMSIerables
{
	class MainForm : Form
	{
		public MainForm(string defaultInputFile)
		{
			InitializeComponent();
			if (defaultInputFile != null && defaultInputFile.Length > 0) 
				this.txtMsiFileName.Text = defaultInputFile;
		}


		#region UI Event Handlers

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			if (DialogResult.OK != this.openMsiDialog.ShowDialog(this))
				return;
			this.txtMsiFileName.Text = this.openMsiDialog.FileName;
			LoadCurrentFile();
		}

		private void ReloadCurrentUIOnEnterKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
			{
				e.Handled = true;
				LoadCurrentFile();
			}
		}


		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			ViewTable();
		}

		private void btnExtract_Click(object sender, EventArgs e)
		{
			ArrayList/*<MsiFile>*/ selectedFiles = new ArrayList();

			if (this.folderBrowser.SelectedPath == null || folderBrowser.SelectedPath.Length <= 0)
				this.folderBrowser.SelectedPath = this.GetSelectedMsiFile().DirectoryName;

			if (DialogResult.OK != this.folderBrowser.ShowDialog(this))
				return;

			this.btnExtract.Enabled = false;
			ExtractionProgressDialog progressDialog = new ExtractionProgressDialog(this);
			progressDialog.Show();
			progressDialog.Update();
			try
			{
				DirectoryInfo outputDir = new DirectoryInfo(this.folderBrowser.SelectedPath);
				foreach (MsiFileListViewItem item in this.fileList.Items)
				{
					if (item.Checked)
						selectedFiles.Add(item._file);
				}
				
				FileInfo msiFile = GetSelectedMsiFile();
				if (msiFile == null)
					return;
				MsiFile[] filesToExtract = (MsiFile[])selectedFiles.ToArray(typeof(MsiFile));
				Wixtracts.ExtractFiles(msiFile, outputDir, filesToExtract, new AsyncCallback(progressDialog.UpdateProgress));
			}
			finally
			{
				progressDialog.Close();
				progressDialog.Dispose();				
				this.btnExtract.Enabled = true;
			}
		}

		private class ExtractionProgressDialog : Form
		{
			private ProgressBar _progressBar;
			private Label _label;

			public ExtractionProgressDialog(Form owner)
			{
				this.Owner = owner;
				
				this.ShowInTaskbar = false;
				this.TopMost = true;
				this.Size = new Size(320, 125);
				this.ControlBox = false;
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.FormBorderStyle = FormBorderStyle.FixedDialog;
				this.StartPosition = FormStartPosition.Manual;

				this.Top = owner.Top + ((owner.Height - this.Height)/2);
				this.Left = owner.Left + ((owner.Width - this.Width)/2);
                
				this.DockPadding.Left = this.DockPadding.Right = 10;
				this.DockPadding.Top = this.DockPadding.Bottom = 10;

				_label = new Label();
				_label.Text = "";
				_label.Dock = DockStyle.Top;
				this.Controls.Add(_label);

				_progressBar = new ProgressBar();
				_progressBar.Dock = DockStyle.Top;
				_progressBar.Value = 0;
				this.Controls.Add(_progressBar);
			}

			public void UpdateProgress(IAsyncResult result)
			{
				if (result is Wixtracts.ExtractionProgress)
					UpdateProgress((Wixtracts.ExtractionProgress)result);
			}

			delegate void UpdateProgressHandler(Wixtracts.ExtractionProgress progress);

			public void UpdateProgress(Wixtracts.ExtractionProgress progress)
			{
				if (this.InvokeRequired)
				{
					// This is ahack, but should be okay if needed to get around invoke
					this.Invoke(new UpdateProgressHandler(this.UpdateProgress), new object[]{progress});
					return;
				}

				_progressBar.Minimum = 0;
				_progressBar.Maximum = progress.TotalFileCount;
				_progressBar.Value = progress.FilesExtractedSoFar;
				string details;
				if (progress.Activity == Wixtracts.ExtractionActivity.ExtractingFile)
					details = "Extracting file '" + progress.CurrentFileName + "'";
				else
					details = Enum.GetName(typeof(Wixtracts.ExtractionActivity), progress.Activity);

				_label.Text = string.Format("Extracting ({0})...", details);
				this.Invalidate(true);
				this.Update();
			}
		}

	
		private void ChangeUiEnabled(bool doEnable)
		{
			this.btnExtract.Enabled = doEnable;
			this.cboTable.Enabled = doEnable;
		}
		#endregion

		

		private void LoadCurrentFile()
		{
			bool isBadFile = false;
			try
			{
				ViewFiles();
				ViewTable();
			}
			catch (Exception eCatchAll)
			{
				isBadFile = true;
				Error("Failed to open file.", eCatchAll);
			}
			ChangeUiEnabled(!isBadFile);
		}


		/// <summary>
		/// Updates the ui with the currently selected msi file.
		/// </summary>
		private void ViewFiles()
		{
			using (Database msidb = new Database(GetSelectedMsiFile().FullName, OpenDatabase.ReadOnly))
			{
				ViewFiles(msidb);
			}
		}


		/// <summary>
		/// Displays the list of files in the extract tab.
		/// </summary>
		/// <param name="msidb">The msi database.</param>
		private void ViewFiles(Database msidb)
		{
			if (msidb == null)
				return;

			using (new Willeke.Shared.Windows.Forms.DisposableCursor(this))
			{
				//used to filter and sort columns
				Hashtable/*<string, int>*/ columnMap = new Hashtable(CaseInsensitiveHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant);
				columnMap.Add("FileName", 0);
				columnMap.Add("FileSize", 1);
				columnMap.Add("Version", 2);
				columnMap.Add("Language", 3);
				columnMap.Add("Attributes", 4);

				try
				{
					this.statusPanelDefault.Text = "";
					fileList.Clear();
					MsiFileListViewItem.AddColumns(fileList);

					foreach (MsiFile msiFile in MsiFile.CreateMsiFilesFromMSI(msidb))
						fileList.Items.Add(new MsiFileListViewItem(msiFile));
					statusPanelFileCount.Text = string.Concat(fileList.Items.Count, " files found.");
				}
				catch (Exception eUnexpected)
				{
					Error(string.Concat("Cannot view files:", eUnexpected.Message), eUnexpected);
				}
			}
		}


		private void ViewTable()
		{
			using (Database msidb = new Database(GetSelectedMsiFile().FullName, OpenDatabase.ReadOnly))
			{
				string tableName = this.cboTable.Text;
				ViewTable(msidb, tableName);
			}
		}


		/// <summary>
		/// Shows the table in the list on the view table tab.
		/// </summary>
		/// <param name="msidb">The msi database.</param>
		/// <param name="tableName">The name of the table.</param>
		private void ViewTable(Database msidb, string tableName)
		{
			if (msidb == null || tableName == null && tableName.Length == 0)
				return;

			Status(string.Concat("Processing Table \'", tableName, "\'."));

			using (new Willeke.Shared.Windows.Forms.DisposableCursor(this))
			{
				try
				{
					tableList.Clear();
					if (!msidb.TableExists(tableName))
					{
						Error("Table \'" + tableName + "' does not exist.", null);
						return;
					}

					string query = string.Concat("SELECT * FROM `", tableName, "`");

					using (ViewWrapper view = new ViewWrapper(msidb.OpenExecuteView(query)))
					{

						int colWidth = this.tableList.ClientRectangle.Width / view.Columns.Length;
						foreach (ColumnInfo col in view.Columns)
						{
							ColumnHeader header = new ColumnHeader();
							header.Text = string.Concat(col.Name, " (", col.TypeID, ")");
							header.Width = colWidth;
							tableList.Columns.Add(header);
						}

						foreach (object[] values in view.Records)
						{
							ListViewItem item = new ListViewItem(Convert.ToString(values[0]));
							for (int colIndex = 1; colIndex < values.Length; colIndex++)
								item.SubItems.Add(Convert.ToString(values[colIndex]));
							tableList.Items.Add(item);
						}
					}
				}
				catch (Exception eUnexpected)
				{
					Error(string.Concat("Cannot view table:", eUnexpected.Message), eUnexpected);
				}
			}
		}


		private FileInfo GetSelectedMsiFile()
		{
			FileInfo file = new FileInfo(this.txtMsiFileName.Text);
			if (!file.Exists)
			{
				Error(string.Concat("File \'", file.FullName, "\' does not exist."), null);
				return null;
			}
			return file;
		}

		class MsiFileListViewItem : ListViewItem
		{
			public MsiFile _file;

			public static void AddColumns(ListView lv)
			{
				lv.Columns.Add("File Name", 200, HorizontalAlignment.Left);
				lv.Columns.Add("Size", 60, HorizontalAlignment.Left);
				lv.Columns.Add("Version", 60, HorizontalAlignment.Left);

			}
			public MsiFileListViewItem(MsiFile file)
			{
				this.Checked = true;
				_file = file;
				this.Text = file.LongFileName;
				this.SubItems.Add(Convert.ToString(file.FileSize));
				this.SubItems.Add(Convert.ToString(file.Version));
			}
  
			public override string ToString()
			{
				return _file.LongFileName;
			}
		}

		#region Status Stuff
		private void Status(string msg)
		{
			this.statusPanelDefault.Text = "Status:" + msg;
		}

		private void Error(string msg, Exception exception)
		{
			this.statusPanelDefault.Text = "ERROR:" + msg;
			if (exception != null)
				this.statusPanelDefault.ToolTipText = exception.ToString();
			else
				this.statusPanelDefault.ToolTipText = "";

		}
		#endregion	

		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.StatusBarPanel statusPanelDefault;
		private System.Windows.Forms.StatusBarPanel statusPanelFileCount;

		#region Designer Stuff
			/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.txtMsiFileName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.tabs = new System.Windows.Forms.TabControl();
			this.tabExtractFiles = new System.Windows.Forms.TabPage();
			this.btnExtract = new System.Windows.Forms.Button();
			this.fileList = new System.Windows.Forms.ListView();
			this.tabTableView = new System.Windows.Forms.TabPage();
			this.cboTable = new System.Windows.Forms.ComboBox();
			this.tableList = new System.Windows.Forms.ListView();
			this.label2 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
			this.openMsiDialog = new System.Windows.Forms.OpenFileDialog();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.statusPanelDefault = new System.Windows.Forms.StatusBarPanel();
			this.statusPanelFileCount = new System.Windows.Forms.StatusBarPanel();
			this.tabs.SuspendLayout();
			this.tabExtractFiles.SuspendLayout();
			this.tabTableView.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.statusPanelDefault)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusPanelFileCount)).BeginInit();
			this.SuspendLayout();
			// 
			// txtMsiFileName
			// 
			this.txtMsiFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtMsiFileName.Location = new System.Drawing.Point(46, 4);
			this.txtMsiFileName.Name = "txtMsiFileName";
			this.txtMsiFileName.Size = new System.Drawing.Size(258, 20);
			this.txtMsiFileName.TabIndex = 1;
			this.txtMsiFileName.Text = "";
			this.txtMsiFileName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ReloadCurrentUIOnEnterKeyPress);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 7);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(26, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "&File:";
			// 
			// btnBrowse
			// 
			this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowse.Location = new System.Drawing.Point(311, 7);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(20, 17);
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "...";
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// tabs
			// 
			this.tabs.Controls.Add(this.tabExtractFiles);
			this.tabs.Controls.Add(this.tabTableView);
			this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabs.Location = new System.Drawing.Point(0, 27);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(347, 346);
			this.tabs.TabIndex = 1;
			// 
			// tabExtractFiles
			// 
			this.tabExtractFiles.Controls.Add(this.btnExtract);
			this.tabExtractFiles.Controls.Add(this.fileList);
			this.tabExtractFiles.Location = new System.Drawing.Point(4, 22);
			this.tabExtractFiles.Name = "tabExtractFiles";
			this.tabExtractFiles.Size = new System.Drawing.Size(339, 320);
			this.tabExtractFiles.TabIndex = 0;
			this.tabExtractFiles.Text = "Extract Files";
			// 
			// btnExtract
			// 
			this.btnExtract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExtract.Enabled = false;
			this.btnExtract.Location = new System.Drawing.Point(258, 291);
			this.btnExtract.Name = "btnExtract";
			this.btnExtract.TabIndex = 0;
			this.btnExtract.Text = "E&xtract";
			this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
			// 
			// fileList
			// 
			this.fileList.AllowColumnReorder = true;
			this.fileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.fileList.CheckBoxes = true;
			this.fileList.FullRowSelect = true;
			this.fileList.GridLines = true;
			this.fileList.LabelWrap = false;
			this.fileList.Location = new System.Drawing.Point(9, 10);
			this.fileList.Name = "fileList";
			this.fileList.Size = new System.Drawing.Size(321, 274);
			this.fileList.TabIndex = 1;
			this.fileList.View = System.Windows.Forms.View.Details;
			// 
			// tabTableView
			// 
			this.tabTableView.Controls.Add(this.cboTable);
			this.tabTableView.Controls.Add(this.tableList);
			this.tabTableView.Controls.Add(this.label2);
			this.tabTableView.Location = new System.Drawing.Point(4, 22);
			this.tabTableView.Name = "tabTableView";
			this.tabTableView.Size = new System.Drawing.Size(339, 320);
			this.tabTableView.TabIndex = 1;
			this.tabTableView.Text = "Table View";
			// 
			// cboTable
			// 
			this.cboTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.cboTable.Enabled = false;
			this.cboTable.Items.AddRange(new object[] {
														  "File",
														  "Media",
														  "Shortcut",
														  "Property",
														  "AppSearch            ",
														  "RegLocator",
														  "CompLocator",
														  "DrLocator",
														  "Signature",
														  "LaunchCondition",
														  "Condition",
														  "FeatureComponents",
														  "Class",
														  "ProgId",
														  "Extension",
														  "Feature",
														  "Directory",
														  "Environment",
														  "MsiAssembly",
														  "RemoveFile",
														  "ReserveCost",
														  "Shortcut",
														  "Component",
														  "File",
														  "CreateFolder",
														  "Upgrade",
														  "ServiceControl",
														  "ServiceInstall",
														  "LockPermissions",
														  "Media",
														  "CustomAction",
														  "UIText",
														  "TextStyle",
														  "Dialog",
														  "Control",
														  "ActionText",
														  "Error",
														  "ModuleSignature",
														  "Component"});
			this.cboTable.Location = new System.Drawing.Point(48, 7);
			this.cboTable.Name = "cboTable";
			this.cboTable.Size = new System.Drawing.Size(282, 21);
			this.cboTable.TabIndex = 8;
			this.cboTable.Text = "File";
			this.cboTable.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ReloadCurrentUIOnEnterKeyPress);
			this.cboTable.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// tableList
			// 
			this.tableList.AllowColumnReorder = true;
			this.tableList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tableList.FullRowSelect = true;
			this.tableList.GridLines = true;
			this.tableList.LabelWrap = false;
			this.tableList.Location = new System.Drawing.Point(9, 35);
			this.tableList.Name = "tableList";
			this.tableList.Size = new System.Drawing.Size(321, 279);
			this.tableList.TabIndex = 7;
			this.tableList.View = System.Windows.Forms.View.Details;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 10);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(33, 16);
			this.label2.TabIndex = 9;
			this.label2.Text = "&Table";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.txtMsiFileName);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.btnBrowse);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(347, 27);
			this.panel1.TabIndex = 0;
			// 
			// openMsiDialog
			// 
			this.openMsiDialog.DefaultExt = "msi";
			this.openMsiDialog.Filter = "msierablefiles|*.msi|All Files|*.*";
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 373);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						  this.statusPanelDefault,
																						  this.statusPanelFileCount});
			this.statusBar1.ShowPanels = true;
			this.statusBar1.Size = new System.Drawing.Size(347, 16);
			this.statusBar1.TabIndex = 2;
			// 
			// statusPanelDefault
			// 
			this.statusPanelDefault.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.statusPanelDefault.Width = 321;
			// 
			// statusPanelFileCount
			// 
			this.statusPanelFileCount.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
			this.statusPanelFileCount.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.statusPanelFileCount.Width = 10;
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(347, 389);
			this.Controls.Add(this.tabs);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.panel1);
			this.Name = "MainForm";
			this.Text = "Less MSIérables";
			this.tabs.ResumeLayout(false);
			this.tabExtractFiles.ResumeLayout(false);
			this.tabTableView.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.statusPanelDefault)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusPanelFileCount)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox txtMsiFileName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage tabExtractFiles;
		private System.Windows.Forms.TabPage tabTableView;
		private System.Windows.Forms.ComboBox cboTable;
		private System.Windows.Forms.ListView tableList;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ListView fileList;
		private System.Windows.Forms.Button btnExtract;
		private System.Windows.Forms.FolderBrowserDialog folderBrowser;
		private System.Windows.Forms.OpenFileDialog openMsiDialog;
		#endregion

	}
}
