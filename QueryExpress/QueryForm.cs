using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.Collections.Specialized;

namespace QueryExpress 
{
	/// <summary>MDI Child Query Form</summary>
	public class QueryForm : System.Windows.Forms.Form 
	{
		#region Private Fields
		DbClient dbClient;									// DbClient object used to talk to database server
		IBrowser browser;									// Browser object for displaying object browser (may be null)
		DateTime queryStartTime;					// For  the timer showing running query time
		string fileName;										// Filename for when query is saved
		static int untitledCount = 1;					// For default new filenames (Untited-1, Untitled-2, etc)
		bool realFileName = false;					// true if default name of "untitled-x" - forces Save As... when Save is requested
		bool resultsInText = false;					// text based results rather than grid based
		RichTextBox txtResultsBox;				// handle to the rich textbox used to display text results
		bool hideBrowser = false;						// hide the treeview, if available
		bool initializing = true;							// to prevent multiple updates during startup
		bool error = false;									// true if an error was encountered
		string lastDatabase;								// ...so we can tell when the database has changed
		#endregion

		#region Designer Fields
		private System.Windows.Forms.StatusBarPanel panRunStatus;
		private System.Windows.Forms.StatusBarPanel panExecTime;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TextBox txtQuery;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Timer tmrExecTime;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.StatusBarPanel panRows;
		private System.Windows.Forms.SaveFileDialog saveResultsDialog;
		private System.Windows.Forms.Splitter splQuery;
		private System.Windows.Forms.Panel panBrowser;
		private System.Windows.Forms.TreeView treeView;
		private System.Windows.Forms.Splitter splBrowser;
		private System.Windows.Forms.Panel panDatabase;
		private System.Windows.Forms.ComboBox cboDatabase;
		private System.Windows.Forms.Label label1;
		private XButton btnCloseBrowser;
		private System.Windows.Forms.ContextMenu cmRefresh;
		private System.Windows.Forms.MenuItem miRefresh;
		private System.Windows.Forms.ImageList imageList;
		#endregion

		#region Constructor
		/// <summary> Creates a MDI child form for running queries </summary>
		/// <param name="dbClient">A configured and connected dbClient object</param>
		/// <param name="browser">A database Browser object (or null if n/a)</param>
		/// <param name="hideBrowser">True if the browser should be hidden right away (low bandwidth)</param>
		public QueryForm (DbClient dbClient, IBrowser browser, bool hideBrowser) 
		{
			InitializeComponent();
			this.dbClient = dbClient;
			this.browser = browser;
			lastDatabase = dbClient.Database;				// this is so we know when the current database changes
			HideResults = true;
			HideBrowser = hideBrowser || (browser == null);
			FileName = "untitled" + untitledCount++.ToString() + ".sql";
			initializing = false;
			// For some reason, the designer has trouble with 8.0 point fonts - it wants to make it
			// 7.8 or 8.25, and this looks rather naff in the statusbar.
			statusBar.Font = new Font (statusBar.Font.Name, 8, FontStyle.Bold);
		}
		#endregion

		#region Properties

		/// <summary>Returns the database client object provided in construction</summary>
		public DbClient DbClient 
		{
			get {return dbClient;}
		}

		/// <summary>Returns the database browser object provided in construction</summary>
		public IBrowser Browser 
		{
			get {return browser;}
		}

		/// <summary>The current state of query execution</summary>
		public RunState RunState 
		{
			get {return dbClient.RunState;}
		}

		/// <summary>The filename given to the SQL query</summary>
		public string FileName 
		{
			get {return fileName;}
			set 
			{
				fileName = value;
				UpdateFormText();
				FirePropertyChanged();
			}
		}

		/// <summary>True if results should be displayed in textbox rather than in a grid</summary>
		public bool ResultsInText 
		{
			get {return resultsInText;}
			set 
			{
				resultsInText = value;
				FirePropertyChanged();
			}
		}

		/// <summary>True if the "hide results" option has been selected (manually or automatically)</summary>
		public bool HideResults 
		{
			get {return !tabControl.Visible;}
			set 
			{
				tabControl.Visible = !value;
				txtQuery.Dock = value ? DockStyle.Fill : DockStyle.Top;
				splQuery.Visible = !value;
				FirePropertyChanged();

				//HACK: Work around bug in splitter control, where it loses its SplitPosition the first time it's made invisible
				if (splQuery.SplitPosition < 0 || splQuery.SplitPosition > ClientSize.Height - statusBar.Height - 10)
					splQuery.SplitPosition = ClientSize.Height / 2;
				else
					// If you take the following line out, the results window will be invisible when running
					// the compiled executable on another differently setup PC.
					//  This is a beta 2 bug - not sure if still present in RTM
					splQuery.SplitPosition +=0;
			}
		}

		/// <summary>True if the "hide browser" option has been selected</summary>
		public bool HideBrowser 
		{
			get {return hideBrowser;}
			set 
			{
				if (Browser == null && !value) return;		// Can't show browser if not available!
				hideBrowser = value;
				panBrowser.Visible = !value;						// show/hide the browser panel containing the treeview
				splBrowser.Visible = !value;						// show/hide the splitter
				if (!value) PopulateBrowser();
				FirePropertyChanged();
			}
		}

		// Private properties

		DataSet DSResults 
		{
			get {return DbClient.DataSet;}
		}

		bool ClientBusy 
		{
			get {return RunState != RunState.Idle;}
		}

		#endregion

		#region Events

		/// <summary>
		/// Fires when a public property has changed.  This is used for enabled/disabling buttons
		/// on a toolbar.
		/// </summary>
		public event EventHandler PropertyChanged;

		#endregion

		#region Query Execution Methods

		/// <summary>Starts execution of  a query</summary>
		public void Execute() 
		{
			if (RunState != RunState.Idle)
				return;

			if (HideResults) HideResults = false;
			error = false;

			// Delete any previously defined tab pages and their child controls
			tabControl.TabPages.Clear();

			TabPage tabPage = new TabPage (ResultsInText ? "Results" : "Messages");
			// We'll need a rich textbox because an ordinary textbox has limited capacity
			txtResultsBox = new RichTextBox();
			txtResultsBox.AutoSize = false;
			txtResultsBox.Dock = DockStyle.Fill;
			txtResultsBox.Multiline = true;
			txtResultsBox.WordWrap = false;
			txtResultsBox.Font = new Font ("Courier New", 8);
			txtResultsBox.ScrollBars = RichTextBoxScrollBars.Both;
			txtResultsBox.MaxLength = 0;
			txtResultsBox.Text = "";
			tabControl.TabPages.Add (tabPage);
			tabPage.Controls.Add (txtResultsBox);

			// If the user has selected text within the query window, just execute the
			// selected text.  Otherwise, execute the contents of the whole textbox.
			string query = txtQuery.SelectedText.Length == 0 ? txtQuery.Text : txtQuery.SelectedText;
			if (query.Trim() == "") return;

			// Use the database client class to execute the query.  Create delegates which will be invoked
			// when the query completes or cancels with an error.

			MethodInvoker results, done, failed;

			if (ResultsInText)
				results = new MethodInvoker (AddTextResults);
			else
				results = new MethodInvoker (AddGridResults);

			done = new MethodInvoker (QueryDone);
			failed = new MethodInvoker (QueryFailed);

			// dbClient.Execute runs asynchronously, so control will return immediately to the calling method.

			Cursor oldCursor = Cursor;
			Cursor = Cursors.WaitCursor;
			panRunStatus.Text = "Executing Query Batch...";
			dbClient.Execute (this, results, done, failed, query, ResultsInText);		// this does the work
			SetRunning (true);
			Cursor = oldCursor;
		}

		/// <summary> Cancel a running query asynchronously </summary>
		public void Cancel() 
		{
			panRunStatus.Text = "Cancelling...";
			dbClient.Cancel (new MethodInvoker (CancelDone));
			// Control will return immediately, and CancelDone will be invoked when the cancel is complete.
			FirePropertyChanged();
		}

		#endregion

		#region Methods to Populate Controls

		/// <summary>
		/// Create a new grid for each DataTable present in the results dataset
		/// </summary>
		void AddGridResults() 
		{
			// Note: we give this method via a delegate to our DbClient object.  The DbClient object then
			// invokes this delegate from its worker thread, when results have become available.
			const int MaxResultSets = 20;

			// Create a new tab page and grid for each new result set.  In case this has already been called,
			// (as will be the case with multiple queries, separated with the 'GO' construct) only add tab
			// pages for new result sets.
			for (int page = tabControl.TabCount - 1; page < Math.Min (MaxResultSets, DSResults.Tables.Count); page++) 
				AddGrid (DSResults.Tables [page] );
		}

		void AddGrid (DataTable dt)		// called by the method above
		{
			DataGrid dataGrid = new DataGrid();

			// Due to a bug in the grid control, we must add the grid to the tabpage before assigning a datasource.
			// This bug was introduced in Beta 1, was fixed for Beta 2, then reared its ugly head again in RTM.
			TabPage tabPage = new TabPage ("Result Set " + (tabControl.TabCount).ToString());
			tabPage.Controls.Add (dataGrid);
			tabControl.TabPages.Add (tabPage);
			
			dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			dataGrid.CaptionVisible = false;
			dataGrid.ReadOnly = true;

			DataGridTableStyle ts = new DataGridTableStyle();
			ts.MappingName = dt.TableName;
			dataGrid.TableStyles.Clear();
			dataGrid.TableStyles.Add (ts);
			dataGrid.SetDataBinding (dt, "");
			
			// The auto sizing feature below is no longer supported in RTM.
			// dataGrid.PreferredColumnWidth = -1;
			// Instead we'll have to size each column manually.
			// A graphics object is required to measure text so we can size the grid columns correctly
			System.Drawing.Graphics g = CreateGraphics();

			// For each column, determine the largest visible text string, and use that to size the column
			// We'll be measuring text for each row that's visible in the grid
			int maxRows = Math.Min (dataGrid.VisibleRowCount, dt.Rows.Count);
			GridColumnStylesCollection cols = ts.GridColumnStyles;
			const int margin = 6;		// allow 6 pixels per column, for grid lines and some white space
			int colNum = 0;
			if (cols.Count == 1)
				cols[0].Width = dataGrid.Width;
			else
				foreach (DataGridColumnStyle col in cols)
				{ 
					int maxWidth = (int) g.MeasureString (col.HeaderText,  dataGrid.Font).Width+ margin;
					for (int row = 0; row < maxRows; row++)
					{
						string s = dt.Rows [row] [colNum, DataRowVersion.Current].ToString();
						int length = (int) g.MeasureString (s, dataGrid.Font).Width+ margin;
						maxWidth = Math.Max (maxWidth, length);
					}
					// Assign length of longest string to the column width, but don't exceed width of actual grid.
					col.Width =Math.Min (dataGrid.Width, maxWidth);
					colNum++;
				}
			g.Dispose();

			// Set datetime columns to show the time as well as the date
			colNum = 0;
			foreach (DataGridColumnStyle col in cols)
			{
				DataGridTextBoxColumn textCol = col as DataGridTextBoxColumn;
				if (textCol != null && dt.Columns [colNum].DataType == typeof (DateTime))
					// Display the date in short format (ie using numbers), and the time in long
					// format (ie including seconds).  This is done using the 'G' format string.
					textCol.Format = "G";		
				colNum++;
			}
		}

		/// <summary>
		/// Create / append text results to the results window
		/// </summary>
		void AddTextResults () 
		{
			// Note: we give this method via a delegate to our DbClient object.  The DbClient object then
			// invokes this delegate from its worker thread, as results become available.
			if (RunState == RunState.Cancelling) return;
			txtResultsBox.AppendText (dbClient.TextResults.ToString());
		}

		/// <summary>
		/// Called when a query has successfully finished executing.
		/// </summary>
		void QueryDone() 
		{
			panRunStatus.Text = "Query batch completed" + (error ? " with errors" : ".");
			// If there were no results from query, display message to provide feedback to user
			if (!ResultsInText && !error) txtResultsBox.AppendText ("The command(s) completed successfully.");
			if (!ResultsInText)
				tabControl.SelectedIndex = error ? 0 : 1;
			ShowRowCount();
			SetRunning (false);
			txtQuery.Focus();
		}

		/// <summary>
		/// Called when a query has returned errors.
		/// </summary>
		void QueryFailed() 
		{
			error = true;
			txtResultsBox.AppendText (dbClient.Error + "\r\n\r\n");
		}

		/// <summary> Display the number of rows retrieved in status bar </summary>
		void ShowRowCount()
		{
			if (ResultsInText || tabControl.SelectedIndex < 1)
				panRows.Text = "";
			else
			{
				int rows;
				if (DSResults.Tables.Count == 0 || tabControl.SelectedIndex < 0)
					rows = 0;
				else
					rows = DSResults.Tables [tabControl.SelectedIndex - 1].Rows.Count;
				panRows.Text = rows == 0 ? "" :  rows.ToString() + " row" + (rows == 1 ? "" : "s");
			}
		}

		/// <summary> Show the elapsed time on the status bar </summary>
		void UpdateExecTime() 
		{
			TimeSpan t = DateTime.Now.Subtract (queryStartTime);
			panExecTime.Text = String.Format ("Exec Time: {0}:{1}:{2}"
				, t.Hours.ToString ("00"), t.Minutes.ToString ("00"), t.Seconds.ToString ("00"));
		}

		/// <summary> If a Browser object is available, populate the treeview control on the left </summary>
		void PopulateBrowser() 
		{
			if (Browser != null && !HideBrowser && !ClientBusy) 
				try
				{
					treeView.Nodes.Clear();
					TreeNode[] tn = Browser.GetObjectHierarchy();
					if (tn == null) HideBrowser = true;
					else {
						treeView.Nodes.AddRange (tn);
						treeView.Nodes[0].Expand();				// Expand the top level of hierarchy
						cboDatabase.Items.Clear();
						cboDatabase.Items.Add ("<refresh list...>");
						cboDatabase.Items.AddRange (Browser.GetDatabases());
						try {cboDatabase.Text = DbClient.Database;} 
						catch {}
					}
				}
				catch {}
		}

		/// <summary> This is called once a cancel request has been completed </summary>
		void CancelDone()
		{
			SetRunning (false);
			panRunStatus.Text = "Query batch was cancelled.";
		}

		#endregion

		#region Query Open/Save/Close

		/// <summary>Returns false if the Open was cancelled or if the file I/O failed </summary>
		public bool Open() 
		{
			openFileDialog.FileName = "*.sql";
			if (openFileDialog.ShowDialog() == DialogResult.OK) 
			{
				string f = openFileDialog.FileName;
				if (System.IO.Path.GetExtension (f) == "") f += ".sql";
				return OpenFile (f);
			}
			else
				return false;
		}

		/// <summary>Returns false if user cancelled or open failed </summary>
		bool OpenFile (string fileName) 
		{
			string s;
			if (FileUtil.ReadFromFile (fileName, out s) && CloseQuery()) 
			{
				txtQuery.Text = s;
				txtQuery.Modified = false;
				this.FileName = fileName;
				realFileName = true;
				return true;
			}
			else 
			{
				MessageBox.Show (FileUtil.Error, "Error opening file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
		}

		/// <summary>Returns false if user cancelled or save failed</summary>
		public bool Save() 
		{
			if (!realFileName)
				return SaveAs();
			else
				return SaveFile (FileName);
		}

		/// <summary>Returns false if user cancelled or save failed</summary>
		public bool SaveAs() 
		{
			saveFileDialog.FileName = FileName;
			if (saveFileDialog.ShowDialog() == DialogResult.OK) 
			{
				FileName = saveFileDialog.FileName;
				realFileName = true;
				return SaveFile (FileName);
			}
			else return false;
		}

		bool SaveFile (string fileName) 
		{
			if (FileUtil.WriteToFile (fileName, txtQuery.Text)) 
			{
				txtQuery.Modified = false;
				return true;
			} 
			else 
			{
				MessageBox.Show (FileUtil.Error, "Error saving file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
		}

		bool CloseQuery() 
		{
			// Check to see if a query is running, and warn user that the query will be cancelled.
			if (RunState != RunState.Idle) 
				if (MessageBox.Show (FileName + " is currently executing.\nWould you like to cancel the query?",
					"Query Express", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
					// The Dispose method in DbClient will actually do the Cancel
					return false;

			// If the query text has been modified, give option of saving changes.
			// Don't nag the user in the case of simple queries of less than 30 characters.
			if (txtQuery.Modified && txtQuery.Text.Length > 30) 
			{
				DialogResult dr = MessageBox.Show ("Save changes to " + FileName + "?", Text,
					MessageBoxButtons.YesNoCancel);
				if (dr == DialogResult.Yes) 
				{
					if (!Save()) return false;
				}
				else if (dr == DialogResult.Cancel)
					return false;
			}
			return true;
		}

		#endregion

		#region Methods for Saving Results 

		/// <summary>
		/// Present a Save... dialog for query results and save to CSV or XML format
		/// </summary>
		public void SaveResults() 
		{
			if (!ResultsInText && (DSResults.Tables.Count == 0) || ClientBusy)
				return;
			saveResultsDialog.Filter = ResultsInText ? "Text Format|*.txt" : "CSV Format|*.csv|XML|*.xml"
				+ "|All files|*.*";
			if (saveResultsDialog.ShowDialog() != DialogResult.OK) return;
			if (ResultsInText)
				SaveResultsText (saveResultsDialog.FileName);
			else if (System.IO.Path.GetExtension (saveResultsDialog.FileName).ToUpper() == ".XML")
				SaveResultsXml (saveResultsDialog.FileName);
			else
				SaveResultsCsv (saveResultsDialog.FileName);
		}

		void SaveResultsText (string fileName) 
		{
			FileUtil.WriteToFile (fileName, DbClient.TextResults.ToString());
		}

		void SaveResultsXml (string fileName) 
		{
			DSResults.WriteXml (fileName);
		}

		void SaveResultsCsv (string fileName) 
		{
			// Save the currently selected table only
			DataTable table = DSResults.Tables [tabControl.SelectedIndex - 1];
			System.IO.StreamWriter w;
			try {w = System.IO.File.CreateText (fileName);}
			catch (Exception e) 
			{
				MessageBox.Show ("Could not create file: " + fileName + "\n" + e.Message
					, "Query Express", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			using (w) 
			{
				// Write a header consisting of a list of columns
				string colList = "";
				foreach (DataColumn column in table.Columns) 
				{
					if (colList.Length > 0) colList += ",";
					colList += column.ColumnName;
				}
				w.WriteLine (colList);
				foreach (DataRow row in table.Rows) 
				{
					string line = "";
					foreach (object cell in row.ItemArray) 
					{
						if (line.Length > 0) line += ",";
						// String types may contain embedded commas, so wrap in quotes.
						if (cell is string)
							line += '"'.ToString() + cell.ToString() + '"'.ToString();
						else
							line += cell.ToString();
					}
					w.WriteLine (line);
				}
			}
		}

		#endregion

		#region Misc Methods

		/// <summary>
		/// Move the cursor into the next or previous window
		/// </summary>
		public void SwitchPane (bool forward) 
		{
			if (ResultsInText) 
			{
				if (txtQuery.Focused)
					txtResultsBox.Focus();
				else
					txtQuery.Focus();
				return;
			}
			if (forward) 
			{
				if (txtQuery.Focused) 
				{
					tabControl.Focus();
					tabControl.SelectedIndex = 0;
				}
				else 
				{
					if (tabControl.SelectedIndex < tabControl.TabCount - 1)
						tabControl.SelectedIndex++;
					else
						txtQuery.Focus();
				}
			}
			else 
			{
				if (txtQuery.Focused) 
				{
					tabControl.Focus();
					tabControl.SelectedIndex = tabControl.TabCount - 1;
				}
				else 
				{
					if (tabControl.SelectedIndex > 0)
						tabControl.SelectedIndex--;
					else
						txtQuery.Focus();
				}
			}
			if (!txtQuery.Focused)
				tabControl.SelectedTab.Controls [0].Focus();
		}

		/// <summary>
		/// Return a copy of the QueryForm object, with separate connection and browser objects
		/// </summary>
		public QueryForm Clone() 
		{
			// Make a copy of the QueryForm's DbClient object.  We can't use the same object
			// object because this would mean sharing the same connection, preventing concurrent queries.
			DbClient d = DbClient.Clone();
			if (d.Connect()) 
			{
				d.Database = DbClient.Database;
				// We have to duplicate the Browser too, since it has a reference to the DbClient object.
				IBrowser b = null;
				if (Browser != null) try { b = Browser.Clone (d); } catch {}
				QueryForm newQF = new QueryForm (d, b, HideBrowser);
				newQF.ResultsInText = ResultsInText;
				return newQF;
			}
			else 
			{
				MessageBox.Show ("Unable to connect: " + d.Error, "Query Express", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
		}

		/// <summary>
		/// Check the current database - if it has changed, update controls accordingly
		/// </summary>
		void CheckDatabase() 
		{
			if (lastDatabase != dbClient.Database) 
			{
				lastDatabase = dbClient.Database;
				UpdateFormText();
				PopulateBrowser();
			}
		}
		
		/// <summary>
		/// Update the form's caption to show the connection & selected database 
		/// </summary>
		void UpdateFormText() 
		{
			Text = dbClient.ConnectDescription + " - " + dbClient.Database + " - " + fileName;
		}

		/// <summary>
		/// This should be called whenever a query is started or stopped
		/// </summary>
		void SetRunning (bool running) 
		{
			// Start the timer in the status bar
			if (running) 
			{
				queryStartTime = DateTime.Now;
				UpdateExecTime();
			}
			tmrExecTime.Enabled = running;
			if (!running) CheckDatabase();
			FirePropertyChanged();
		}

		void FirePropertyChanged() 
		{
			if (!initializing && PropertyChanged != null)
				PropertyChanged (this, EventArgs.Empty);		// fire event
		}

		protected override void Dispose (bool disposing) 
		{
			if (disposing) 
			{
				if (components != null)
					components.Dispose();
			}
			dbClient.Dispose();					// Close connections, etc.
			base.Dispose (disposing);
		}

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() 
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(QueryForm));
			this.panRows = new System.Windows.Forms.StatusBarPanel();
			this.txtQuery = new System.Windows.Forms.TextBox();
			this.cboDatabase = new System.Windows.Forms.ComboBox();
			this.panBrowser = new System.Windows.Forms.Panel();
			this.treeView = new System.Windows.Forms.TreeView();
			this.panDatabase = new System.Windows.Forms.Panel();
			this.btnCloseBrowser = new QueryExpress.XButton();
			this.label1 = new System.Windows.Forms.Label();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.panRunStatus = new System.Windows.Forms.StatusBarPanel();
			this.panExecTime = new System.Windows.Forms.StatusBarPanel();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tmrExecTime = new System.Windows.Forms.Timer(this.components);
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.splQuery = new System.Windows.Forms.Splitter();
			this.saveResultsDialog = new System.Windows.Forms.SaveFileDialog();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.splBrowser = new System.Windows.Forms.Splitter();
			this.cmRefresh = new System.Windows.Forms.ContextMenu();
			this.miRefresh = new System.Windows.Forms.MenuItem();
			((System.ComponentModel.ISupportInitialize)(this.panRows)).BeginInit();
			this.panBrowser.SuspendLayout();
			this.panDatabase.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.panRunStatus)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.panExecTime)).BeginInit();
			this.SuspendLayout();
			// 
			// panRows
			// 
			this.panRows.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.panRows.MinWidth = 60;
			this.panRows.Width = 60;
			// 
			// txtQuery
			// 
			this.txtQuery.AcceptsReturn = true;
			this.txtQuery.AcceptsTab = true;
			this.txtQuery.AllowDrop = true;
			this.txtQuery.AutoSize = false;
			this.txtQuery.Dock = System.Windows.Forms.DockStyle.Top;
			this.txtQuery.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.txtQuery.Location = new System.Drawing.Point(205, 0);
			this.txtQuery.Multiline = true;
			this.txtQuery.Name = "txtQuery";
			this.txtQuery.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtQuery.Size = new System.Drawing.Size(609, 201);
			this.txtQuery.TabIndex = 0;
			this.txtQuery.Text = "";
			this.txtQuery.WordWrap = false;
			this.txtQuery.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtQuery_DragDrop);
			this.txtQuery.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtQuery_DragEnter);
			// 
			// cboDatabase
			// 
			this.cboDatabase.ContextMenu = this.cmRefresh;
			this.cboDatabase.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.cboDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDatabase.DropDownWidth = 128;
			this.cboDatabase.ItemHeight = 16;
			this.cboDatabase.Location = new System.Drawing.Point(0, 22);
			this.cboDatabase.Name = "cboDatabase";
			this.cboDatabase.Size = new System.Drawing.Size(202, 24);
			this.cboDatabase.TabIndex = 1;
			this.cboDatabase.SelectedIndexChanged += new System.EventHandler(this.cboDatabase_SelectedIndexChanged);
			this.cboDatabase.Enter += new System.EventHandler(this.cboDatabase_Enter);
			// 
			// panBrowser
			// 
			this.panBrowser.Controls.Add(this.treeView);
			this.panBrowser.Controls.Add(this.panDatabase);
			this.panBrowser.Dock = System.Windows.Forms.DockStyle.Left;
			this.panBrowser.Location = new System.Drawing.Point(0, 0);
			this.panBrowser.Name = "panBrowser";
			this.panBrowser.Size = new System.Drawing.Size(202, 499);
			this.panBrowser.TabIndex = 3;
			// 
			// treeView
			// 
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.ImageIndex = -1;
			this.treeView.Location = new System.Drawing.Point(0, 46);
			this.treeView.Name = "treeView";
			this.treeView.SelectedImageIndex = -1;
			this.treeView.Size = new System.Drawing.Size(202, 453);
			this.treeView.TabIndex = 0;
			this.treeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseDown);
			this.treeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseUp);
			this.treeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeExpand);
			this.treeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
			// 
			// panDatabase
			// 
			this.panDatabase.ContextMenu = this.cmRefresh;
			this.panDatabase.Controls.Add(this.btnCloseBrowser);
			this.panDatabase.Controls.Add(this.label1);
			this.panDatabase.Controls.Add(this.cboDatabase);
			this.panDatabase.Dock = System.Windows.Forms.DockStyle.Top;
			this.panDatabase.Location = new System.Drawing.Point(0, 0);
			this.panDatabase.Name = "panDatabase";
			this.panDatabase.Size = new System.Drawing.Size(202, 46);
			this.panDatabase.TabIndex = 1;
			// 
			// btnCloseBrowser
			// 
			this.btnCloseBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCloseBrowser.Location = new System.Drawing.Point(182, 1);
			this.btnCloseBrowser.Name = "btnCloseBrowser";
			this.btnCloseBrowser.Size = new System.Drawing.Size(20, 19);
			this.btnCloseBrowser.TabIndex = 2;
			this.btnCloseBrowser.TabStop = false;
			this.btnCloseBrowser.Click += new System.EventHandler(this.btnCloseBrowser_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(-1, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(66, 18);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Database:";
			// 
			// statusBar
			// 
			this.statusBar.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.statusBar.Location = new System.Drawing.Point(205, 477);
			this.statusBar.Name = "statusBar";
			this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						 this.panRunStatus,
																						 this.panExecTime,
																						 this.panRows});
			this.statusBar.ShowPanels = true;
			this.statusBar.Size = new System.Drawing.Size(609, 22);
			this.statusBar.SizingGrip = false;
			this.statusBar.TabIndex = 2;
			// 
			// panRunStatus
			// 
			this.panRunStatus.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.panRunStatus.Text = "Ready";
			this.panRunStatus.Width = 469;
			// 
			// panExecTime
			// 
			this.panExecTime.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.panExecTime.MinWidth = 80;
			this.panExecTime.Width = 80;
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "sql";
			this.saveFileDialog.Filter = "SQL files|*.sql|Text files|*.txt|All files|*.*";
			// 
			// tabControl
			// 
			this.tabControl.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(205, 208);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(609, 269);
			this.tabControl.TabIndex = 1;
			this.tabControl.TabStop = false;
			this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
			// 
			// tmrExecTime
			// 
			this.tmrExecTime.Interval = 1000;
			this.tmrExecTime.Tick += new System.EventHandler(this.tmrExecTime_Tick);
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "SQL";
			this.openFileDialog.Filter = "SQL files|*.sql|Text files|*.txt|All files|*.*";
			// 
			// splQuery
			// 
			this.splQuery.Dock = System.Windows.Forms.DockStyle.Top;
			this.splQuery.Location = new System.Drawing.Point(205, 201);
			this.splQuery.MinExtra = 0;
			this.splQuery.Name = "splQuery";
			this.splQuery.Size = new System.Drawing.Size(609, 7);
			this.splQuery.TabIndex = 1;
			this.splQuery.TabStop = false;
			this.splQuery.Resize += new System.EventHandler(this.splQuery_Resize);
			this.splQuery.Paint += new System.Windows.Forms.PaintEventHandler(this.splQuery_Paint);
			// 
			// saveResultsDialog
			// 
			this.saveResultsDialog.Filter = "CSV Format|*.csv|XML|*.xml|All files|*.*";
			this.saveResultsDialog.Title = "Save Query Results";
			// 
			// imageList
			// 
			this.imageList.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// splBrowser
			// 
			this.splBrowser.Location = new System.Drawing.Point(202, 0);
			this.splBrowser.Name = "splBrowser";
			this.splBrowser.Size = new System.Drawing.Size(3, 499);
			this.splBrowser.TabIndex = 4;
			this.splBrowser.TabStop = false;
			// 
			// cmRefresh
			// 
			this.cmRefresh.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.miRefresh});
			// 
			// miRefresh
			// 
			this.miRefresh.Index = 0;
			this.miRefresh.Text = "&Refresh Browser";
			this.miRefresh.Click += new System.EventHandler(this.miRefresh_Click);
			// 
			// QueryForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(814, 499);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.splQuery);
			this.Controls.Add(this.txtQuery);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.splBrowser);
			this.Controls.Add(this.panBrowser);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Name = "QueryForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "QueryForm";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.QueryForm_KeyDown);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.QueryForm_Closing);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.QueryForm_KeyPress);
			((System.ComponentModel.ISupportInitialize)(this.panRows)).EndInit();
			this.panBrowser.ResumeLayout(false);
			this.panDatabase.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.panRunStatus)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.panExecTime)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

		private void QueryForm_Activated(object sender, System.EventArgs e) 
		{
			FirePropertyChanged();
		}

		private void QueryForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) 
		{
			// Check for Alt+Break combination (alternative shortcut for cancelling a query)
			if (e.Alt && e.KeyCode == Keys.Pause && RunState == RunState.Running) 
			{
				Cancel();
				e.Handled = true;
			}
		}

		private void QueryForm_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e) 
		{
			// Check for Control+E keypress (alternative to F5 for executing a  query)
			// Because this keystroke does get received in the KeyPress event, we are obliged to trap
			// it here (rather than in KeyDown) so we can set Handled to true to prevent the
			// default behaviour (ie a beep).
			if (e.KeyChar == '\x005' && RunState == RunState.Idle) 
			{
				Execute();
				e.Handled = true;
			}
		}

		private void QueryForm_Closing(object sender, System.ComponentModel.CancelEventArgs e) 
		{
			if (!CloseQuery()) e.Cancel = true;
		}

		private void tmrExecTime_Tick(object sender, System.EventArgs e) 
		{
			UpdateExecTime();
		}

		private void tabControl_SelectedIndexChanged(object sender, System.EventArgs e) 
		{
			// Workaround: there's a bug in the grid control, whereby the scrollbars in grids in tabpages
			// don't resize when the parent control is resized with another tabpage active.
			this.Height += 1;
			this.Height -= 1;
			// If there is more than one result set, show the row count in the currently selected table.
			//tabControl.SelectedTab.inRefresh();
			ShowRowCount();
		}

		private void treeView_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e) 
		{
			// Allow objects to be dragged from the browser to the query textbox.
			if (e.Button == MouseButtons.Left && e.Item is TreeNode) 
			{
				// Ask the browser object for a string applicable to dragging onto the query window.
				string dragText = Browser.GetDragText ((TreeNode) e.Item);
				// We'll use a simple string-type DataObject
				if (dragText != "")
					treeView.DoDragDrop (new DataObject (dragText), DragDropEffects.Copy);
			}
		}

		private void treeView_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e) 
		{
			// If a browser has been installed, see if it has a sub object hierarchy for us at the point of expansion
			if (Browser == null) return;
			TreeNode[] subtree = Browser.GetSubObjectHierarchy (e.Node);
			if (subtree != null) 
			{
				e.Node.Nodes.Clear();
				e.Node.Nodes.AddRange (subtree);
			}
		}

		private void treeView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) 
		{
			// When right-clicking, first select the node under the mouse.
			if (e.Button == MouseButtons.Right) 
			{
				TreeNode tn = treeView.GetNodeAt (e.X, e.Y);
				if (tn != null)
					treeView.SelectedNode = tn;
			}
		}

		private void treeView_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) 
		{
			if (Browser == null) return;
			// Display a context menu if the browser has an action list for the selected node
			if (e.Button == MouseButtons.Right && treeView.SelectedNode != null) 
			{
				StringCollection  actions = Browser.GetActionList (treeView.SelectedNode);
				if (actions != null) 
				{
					System.Windows.Forms.ContextMenu cm = new ContextMenu();
					foreach (string action in actions)
						cm.MenuItems.Add (action, new EventHandler (DoBrowserAction));
					cm.Show (treeView, new Point (e.X, e.Y));
				}
			}
		}

		private void DoBrowserAction (object sender, EventArgs e) 
		{
			// This is called from the context menu activated by the TreeView's right-click
			// event handler (treeView_MouseUp) and appends text to the query textbox
			// applicable to the selected menu item.
			MenuItem mi = (MenuItem) sender;
			// Ask the browser for the text to append, applicable to the selected node and menu item text.
			string s = Browser.GetActionText (treeView.SelectedNode, mi.Text);
			if (s == null) return;
			if (s.Length > 200) HideResults = true;
			if (txtQuery.Text != "") txtQuery.AppendText ("\r\n\r\n");
			int start = txtQuery.SelectionStart;
			txtQuery.AppendText (s);
			txtQuery.SelectionStart = start;
			txtQuery.SelectionLength = s.Length;
			txtQuery.Modified = true;
			txtQuery.Focus();
		}

		private void txtQuery_DragEnter(object sender, System.Windows.Forms.DragEventArgs e) 
		{
			if (e.Data.GetDataPresent (typeof (string))) 
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void txtQuery_DragDrop(object sender, System.Windows.Forms.DragEventArgs e) 
		{
			if (e.Data.GetDataPresent (typeof (string))) 
			{
				string s = (string) e.Data.GetData (typeof (string));
				// Have the newly inserted text highlighted
				int start = txtQuery.SelectionStart;
				txtQuery.SelectedText = s;
				txtQuery.SelectionStart = start;
				txtQuery.SelectionLength = s.Length;
				txtQuery.Modified = true;
				txtQuery.Focus();
			}
		}

		private void btnCloseBrowser_Click(object sender, System.EventArgs e) 
		{
			HideBrowser = true;
		}

		private void cboDatabase_Enter(object sender, System.EventArgs e) 
		{
			if (ClientBusy) txtQuery.Focus();
		}

		private void cboDatabase_SelectedIndexChanged(object sender, System.EventArgs e) 
		{
			if (cboDatabase.SelectedIndex == 0)
				PopulateBrowser();
			else
				DbClient.Database = cboDatabase.Text;
			CheckDatabase();
		}

		private void splQuery_Paint(object sender, System.Windows.Forms.PaintEventArgs e) 
		{
			// We need a 3D border effect on the bottom of this control to look right.
			e.Graphics.Clear (splQuery.BackColor);
			ControlPaint.DrawBorder3D (e.Graphics, e.ClipRectangle, Border3DStyle.Raised, Border3DSide.Bottom);
		}

		private void splQuery_Resize(object sender, System.EventArgs e) 
		{
			// Force a re-paint
			Invalidate();
		}

		#endregion

		private void miRefresh_Click(object sender, System.EventArgs e)
		{
			PopulateBrowser();
		}

	}		// end of QueryForm class

	#region Helper Classes

	/// <summary>
	///  A simple flat close button, with an X painted on it
	///  </summary>
	public class XButton : Control
	{
		bool mouseIn = false;
		bool mouseDown = false;

		public XButton() 
		{
			ResizeRedraw = true;
		}

		protected override void OnMouseEnter (EventArgs e) 
		{
			mouseIn = true;
			Invalidate();
			base.OnMouseEnter (e);
		}

		protected override void OnMouseLeave (EventArgs e) 
		{
			mouseIn = false;
			Invalidate();
			base.OnMouseLeave (e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				mouseDown = true;
				Invalidate();
			}
			base.OnMouseDown (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				mouseDown = false;
				Invalidate();
			}
			base.OnMouseUp (e);
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			// Imitate usual behaviour of button which is to show as unpressed when the mouse button
			// is pressed down then dragged away.
			if (mouseIn != ClientRectangle.Contains (e.X, e.Y))
			{
				mouseIn = ClientRectangle.Contains (e.X, e.Y);
				Invalidate();
			}
			base.OnMouseMove (e);
		}

		protected override void OnPaint (PaintEventArgs e) 
		{
			e.Graphics.Clear (BackColor);

			// Draw a sunken border if the mouse is in the control and pressed, draw a raised border
			// if the mouse is in the control but not pressed.
			if (mouseIn || mouseDown) 
				ControlPaint.DrawBorder3D (e.Graphics, ClientRectangle,
					mouseDown && mouseIn ? Border3DStyle.SunkenOuter : Border3DStyle.RaisedInner);

			// Deflate our client rectangle then draw the X inside it
			Rectangle r = ClientRectangle;
			r.Inflate (-4, -4);
			// A square shape with an odd # of pixels required is to render properly
			r.Width = r.Height = Math.Min (r.Width, r.Height) / 2 * 2 + 1;

			// Draw the 'X'
			using (Pen p = new Pen (Color.Black, 2))
			{
				e.Graphics.DrawLine (p, r.Left, r.Top, r.Right, r.Bottom);
				e.Graphics.DrawLine (p, r.Right, r.Top, r.Left, r.Bottom);
			}

			base.OnPaint (e);
		}
	}

	#endregion

}