// Query Express 3.7 for .NET
// Written by Joseph Albahari July 2001, last updated 12 August 2005
// OleDb Browser written by Klaus Evers
// Copyright (c) 2005 Joseph Albahari, Klaus Evers
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this work to deal in this work without restriction (including the rights to
// use, modify, distribute, sublicense, and use within a commercial product).

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace QueryExpress
{
	/// <summary>Main MDI Form for Query Express</summary>
	public class MainForm : System.Windows.Forms.Form
	{
		#region Designer Fields
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem11;
		private System.Windows.Forms.MenuItem miFile;
		private System.Windows.Forms.MenuItem miQuery;
		private System.Windows.Forms.MenuItem miEdit;
		private System.Windows.Forms.MenuItem miWindow;
		private System.Windows.Forms.MenuItem miConnect;
		private System.Windows.Forms.MenuItem miDisconnect;
		private System.Windows.Forms.MenuItem miOpen;
		private System.Windows.Forms.MenuItem miSave;
		private System.Windows.Forms.MenuItem miSaveAs;
		private System.Windows.Forms.MenuItem miExit;
		private System.Windows.Forms.MenuItem miExecute;
		private System.Windows.Forms.MenuItem miCancel;
		private System.Windows.Forms.MenuItem miUndo;
		private System.Windows.Forms.MenuItem menuItem16;
		private System.Windows.Forms.MenuItem miCut;
		private System.Windows.Forms.MenuItem miCopy;
		private System.Windows.Forms.MenuItem miPaste;
		private System.Windows.Forms.MenuItem miHelp;
		private System.Windows.Forms.MenuItem miAbout;
		private System.Windows.Forms.MdiClient mdiClient1;
		private System.Windows.Forms.MenuItem miNew;
		private QueryExpress.EditManager editManager;
		private System.Windows.Forms.MenuItem miSaveResults;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem miResultsText;
		private System.Windows.Forms.MenuItem miResultsGrid;
		private System.Windows.Forms.MenuItem miHideResults;
		private System.Windows.Forms.MenuItem miHideBrowser;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.ToolBarButton tbConnect;
		private System.Windows.Forms.ToolBarButton tbDisconnect;
		private System.Windows.Forms.ToolBarButton tbNew;
		private System.Windows.Forms.ToolBarButton tbOpen;
		private System.Windows.Forms.ToolBarButton tbSave;
		private System.Windows.Forms.ToolBarButton tbExecute;
		private System.Windows.Forms.ToolBarButton tbCancel;
		private System.Windows.Forms.ToolBarButton sep1;
		private System.Windows.Forms.ToolBarButton sep2;
		private System.Windows.Forms.ToolBarButton sep3;
		private System.Windows.Forms.ToolBarButton sep4;
		private System.Windows.Forms.ToolBarButton sep5;
		private System.Windows.Forms.ToolBar toolBar;
		private System.Windows.Forms.ToolBarButton tbResultsText;
		private System.Windows.Forms.ToolBarButton tbResultsGrid;
		private System.Windows.Forms.ToolBarButton tbHideResults;
		private System.Windows.Forms.ToolBarButton tbHideBrowser;
		private System.Windows.Forms.MenuItem miNextPane;
		private System.Windows.Forms.MenuItem miPrevPane;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem miSelectAll;
		private System.ComponentModel.IContainer components;
		#endregion

		#region Constructor
		public MainForm()
		{
			System.Threading.Thread.CurrentThread.Name = "Main Thread";		// to ease debugging
			InitializeComponent();
			// Maximize main window if it was maximized last time...
			try
			{ 
				DSSettings dsSettings = new DSSettings();
				dsSettings.ReadXml ("QESettings.xml");
				// Don't worry if no file / record is present for now - ConnectForm will create one for next time.
				if (dsSettings.settings [0].MdiParentMaximized)
					WindowState = FormWindowState.Maximized;
			}
			catch (Exception) {}

			// Wire the toolbar buttons to the event handlers already defined for the menu options.
			// Assign the event handlers to the tag property, which can query when toolbar's click event is handled.

			tbConnect.Tag = new EventHandler (miConnect_Click);
			tbDisconnect.Tag = new EventHandler (miDisconnect_Click);

			tbNew.Tag = new EventHandler (miNew_Click);
			tbOpen.Tag = new EventHandler (miOpen_Click);
			tbSave.Tag = new EventHandler (miSave_Click);

			tbExecute.Tag = new EventHandler (miExecute_Click);
			tbCancel.Tag = new EventHandler (miCancel_Click);

			tbHideBrowser.Tag = new EventHandler (miHideBrowser_Click);
			tbHideResults.Tag = new EventHandler (miHideResults_Click);

			tbResultsGrid.Tag = new EventHandler (miResultsGrid_Click);
			tbResultsText.Tag = new EventHandler (miResultsText_Click);

			// Start by displaying the Connection dialog
			miConnect.PerformClick();
		}
		#endregion 

		#region Misc Private Methods

		/// <summary>
		/// We'll attach the PropertyChanged event of the QueryForms to this handler, so we can
		/// update the toolbar and menu items when the state of a QueryForm changes.
		/// </summary>
		void ChildPropertyChanged (object sender, EventArgs e)
		{
			EnableControls();
		}

		/// <summary>
		/// Enable / Disable toolbar and menu items
		/// </summary>
		void EnableControls()
		{
			QueryForm q;
			bool active = IsChildActive();
			if (active) q = GetQueryChild(); else q = null;

			miOpen.Enabled = tbOpen.Enabled =
				miSaveResults.Enabled = (active && q.RunState == RunState.Idle);

			miNew.Enabled = tbNew.Enabled =
				miSave.Enabled = tbSave.Enabled =
				miSaveAs.Enabled = active;

			miDisconnect.Enabled = tbDisconnect.Enabled = (active && q.RunState != RunState.Cancelling);

			miExecute.Enabled = tbExecute.Enabled = (active && q.RunState == RunState.Idle);

			miCancel.Enabled = tbCancel.Enabled = (active && q.RunState == RunState.Running);

			miResultsText.Enabled = tbResultsText.Enabled =
				miResultsGrid.Enabled = tbResultsGrid.Enabled = active;

			miResultsText.Checked = tbResultsText.Pushed = (active && q.ResultsInText);
			miResultsGrid.Checked = tbResultsGrid.Pushed = (active && !q.ResultsInText);

			miNextPane.Enabled = miPrevPane.Enabled = active;

			miHideResults.Enabled = tbHideResults.Enabled = active;
			miHideBrowser.Enabled = tbHideBrowser.Enabled =
				(active && q.Browser != null && q.RunState == RunState.Idle);

			miHideResults.Checked = tbHideResults.Pushed = (active && q.HideResults);
			miHideBrowser.Checked = tbHideBrowser.Pushed = (active && q.HideBrowser);
		}

		bool IsChildActive()
		{
			return ActiveMdiChild != null;
		}

		QueryForm GetQueryChild()
		{
			return (QueryForm) ActiveMdiChild;
		}

		protected override void Dispose( bool disposing )
		{
			try		// Save the Window state to the settings XML file (if the XML file exists and is valid).
			{
				DSSettings dsSettings = new DSSettings();
				dsSettings.ReadXml ("QESettings.xml");
				dsSettings.settings [0].MdiParentMaximized = (WindowState == FormWindowState.Maximized);
				dsSettings.WriteXml ("QESettings.xml");
			} 
			catch (Exception) {}
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.miFile = new System.Windows.Forms.MenuItem();
			this.miConnect = new System.Windows.Forms.MenuItem();
			this.miDisconnect = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.miNew = new System.Windows.Forms.MenuItem();
			this.miOpen = new System.Windows.Forms.MenuItem();
			this.miSave = new System.Windows.Forms.MenuItem();
			this.miSaveAs = new System.Windows.Forms.MenuItem();
			this.menuItem11 = new System.Windows.Forms.MenuItem();
			this.miSaveResults = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.miExit = new System.Windows.Forms.MenuItem();
			this.miEdit = new System.Windows.Forms.MenuItem();
			this.miUndo = new System.Windows.Forms.MenuItem();
			this.menuItem16 = new System.Windows.Forms.MenuItem();
			this.miCut = new System.Windows.Forms.MenuItem();
			this.miCopy = new System.Windows.Forms.MenuItem();
			this.miPaste = new System.Windows.Forms.MenuItem();
			this.miQuery = new System.Windows.Forms.MenuItem();
			this.miExecute = new System.Windows.Forms.MenuItem();
			this.miCancel = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.miResultsText = new System.Windows.Forms.MenuItem();
			this.miResultsGrid = new System.Windows.Forms.MenuItem();
			this.miWindow = new System.Windows.Forms.MenuItem();
			this.miNextPane = new System.Windows.Forms.MenuItem();
			this.miPrevPane = new System.Windows.Forms.MenuItem();
			this.miHideResults = new System.Windows.Forms.MenuItem();
			this.miHideBrowser = new System.Windows.Forms.MenuItem();
			this.miHelp = new System.Windows.Forms.MenuItem();
			this.miAbout = new System.Windows.Forms.MenuItem();
			this.sep5 = new System.Windows.Forms.ToolBarButton();
			this.tbOpen = new System.Windows.Forms.ToolBarButton();
			this.sep3 = new System.Windows.Forms.ToolBarButton();
			this.tbHideBrowser = new System.Windows.Forms.ToolBarButton();
			this.tbSave = new System.Windows.Forms.ToolBarButton();
			this.sep1 = new System.Windows.Forms.ToolBarButton();
			this.tbConnect = new System.Windows.Forms.ToolBarButton();
			this.mdiClient1 = new System.Windows.Forms.MdiClient();
			this.tbExecute = new System.Windows.Forms.ToolBarButton();
			this.tbResultsGrid = new System.Windows.Forms.ToolBarButton();
			this.tbNew = new System.Windows.Forms.ToolBarButton();
			this.tbResultsText = new System.Windows.Forms.ToolBarButton();
			this.sep4 = new System.Windows.Forms.ToolBarButton();
			this.sep2 = new System.Windows.Forms.ToolBarButton();
			this.tbDisconnect = new System.Windows.Forms.ToolBarButton();
			this.toolBar = new System.Windows.Forms.ToolBar();
			this.tbCancel = new System.Windows.Forms.ToolBarButton();
			this.tbHideResults = new System.Windows.Forms.ToolBarButton();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.editManager = new QueryExpress.EditManager(this.components);
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.miSelectAll = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.miFile,
																					 this.miEdit,
																					 this.miQuery,
																					 this.miWindow,
																					 this.miHelp});
			// 
			// miFile
			// 
			this.miFile.Index = 0;
			this.miFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				   this.miConnect,
																				   this.miDisconnect,
																				   this.menuItem7,
																				   this.miNew,
																				   this.miOpen,
																				   this.miSave,
																				   this.miSaveAs,
																				   this.menuItem11,
																				   this.miSaveResults,
																				   this.menuItem2,
																				   this.miExit});
			this.miFile.Text = "&File";
			// 
			// miConnect
			// 
			this.miConnect.Index = 0;
			this.miConnect.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
			this.miConnect.Text = "&Connect...";
			this.miConnect.Click += new System.EventHandler(this.miConnect_Click);
			// 
			// miDisconnect
			// 
			this.miDisconnect.Index = 1;
			this.miDisconnect.Text = "&Disconnect";
			this.miDisconnect.Click += new System.EventHandler(this.miDisconnect_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 2;
			this.menuItem7.Text = "-";
			// 
			// miNew
			// 
			this.miNew.Index = 3;
			this.miNew.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
			this.miNew.Text = "&New";
			this.miNew.Click += new System.EventHandler(this.miNew_Click);
			// 
			// miOpen
			// 
			this.miOpen.Index = 4;
			this.miOpen.Text = "&Open...";
			this.miOpen.Click += new System.EventHandler(this.miOpen_Click);
			// 
			// miSave
			// 
			this.miSave.Index = 5;
			this.miSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
			this.miSave.Text = "&Save";
			this.miSave.Click += new System.EventHandler(this.miSave_Click);
			// 
			// miSaveAs
			// 
			this.miSaveAs.Index = 6;
			this.miSaveAs.Text = "Save &As...";
			this.miSaveAs.Click += new System.EventHandler(this.miSaveAs_Click);
			// 
			// menuItem11
			// 
			this.menuItem11.Index = 7;
			this.menuItem11.Text = "-";
			// 
			// miSaveResults
			// 
			this.miSaveResults.Index = 8;
			this.miSaveResults.Text = "Save &Query Results...";
			this.miSaveResults.Click += new System.EventHandler(this.miSaveResults_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 9;
			this.menuItem2.Text = "-";
			// 
			// miExit
			// 
			this.miExit.Index = 10;
			this.miExit.Text = "E&xit";
			this.miExit.Click += new System.EventHandler(this.miExit_Click);
			// 
			// miEdit
			// 
			this.miEdit.Index = 1;
			this.miEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				   this.miUndo,
																				   this.menuItem16,
																				   this.miCut,
																				   this.miCopy,
																				   this.miPaste,
																				   this.menuItem3,
																				   this.miSelectAll});
			this.miEdit.Text = "&Edit";
			// 
			// miUndo
			// 
			this.miUndo.Index = 0;
			this.miUndo.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
			this.miUndo.Text = "&Undo";
			// 
			// menuItem16
			// 
			this.menuItem16.Index = 1;
			this.menuItem16.Text = "-";
			// 
			// miCut
			// 
			this.miCut.Index = 2;
			this.miCut.Shortcut = System.Windows.Forms.Shortcut.CtrlX;
			this.miCut.Text = "Cu&t";
			// 
			// miCopy
			// 
			this.miCopy.Index = 3;
			this.miCopy.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
			this.miCopy.Text = "&Copy";
			// 
			// miPaste
			// 
			this.miPaste.Index = 4;
			this.miPaste.Shortcut = System.Windows.Forms.Shortcut.CtrlV;
			this.miPaste.Text = "&Paste";
			// 
			// miQuery
			// 
			this.miQuery.Index = 2;
			this.miQuery.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					this.miExecute,
																					this.miCancel,
																					this.menuItem1,
																					this.miResultsText,
																					this.miResultsGrid});
			this.miQuery.Text = "&Query";
			// 
			// miExecute
			// 
			this.miExecute.Index = 0;
			this.miExecute.Shortcut = System.Windows.Forms.Shortcut.F5;
			this.miExecute.Text = "&Execute";
			this.miExecute.Click += new System.EventHandler(this.miExecute_Click);
			// 
			// miCancel
			// 
			this.miCancel.Index = 1;
			this.miCancel.Shortcut = System.Windows.Forms.Shortcut.ShiftF5;
			this.miCancel.Text = "&Cancel Executing Query     Alt+Break  or";
			this.miCancel.Click += new System.EventHandler(this.miCancel_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 2;
			this.menuItem1.Text = "-";
			// 
			// miResultsText
			// 
			this.miResultsText.Index = 3;
			this.miResultsText.Shortcut = System.Windows.Forms.Shortcut.CtrlT;
			this.miResultsText.Text = "Results in &Text";
			this.miResultsText.Click += new System.EventHandler(this.miResultsText_Click);
			// 
			// miResultsGrid
			// 
			this.miResultsGrid.Checked = true;
			this.miResultsGrid.Index = 4;
			this.miResultsGrid.Shortcut = System.Windows.Forms.Shortcut.CtrlD;
			this.miResultsGrid.Text = "Results in &Grid";
			this.miResultsGrid.Click += new System.EventHandler(this.miResultsGrid_Click);
			// 
			// miWindow
			// 
			this.miWindow.Index = 3;
			this.miWindow.MdiList = true;
			this.miWindow.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.miNextPane,
																					 this.miPrevPane,
																					 this.miHideResults,
																					 this.miHideBrowser});
			this.miWindow.Text = "&Window";
			// 
			// miNextPane
			// 
			this.miNextPane.Index = 0;
			this.miNextPane.Shortcut = System.Windows.Forms.Shortcut.F6;
			this.miNextPane.Text = "Switch to &Next Pane";
			this.miNextPane.Click += new System.EventHandler(this.miNextPane_Click);
			// 
			// miPrevPane
			// 
			this.miPrevPane.Index = 1;
			this.miPrevPane.Shortcut = System.Windows.Forms.Shortcut.ShiftF6;
			this.miPrevPane.Text = "Switch to &Previous Pane";
			this.miPrevPane.Click += new System.EventHandler(this.miPrevPane_Click);
			// 
			// miHideResults
			// 
			this.miHideResults.Checked = true;
			this.miHideResults.Index = 2;
			this.miHideResults.Shortcut = System.Windows.Forms.Shortcut.CtrlR;
			this.miHideResults.Text = "Hide &Results Pane";
			this.miHideResults.Click += new System.EventHandler(this.miHideResults_Click);
			// 
			// miHideBrowser
			// 
			this.miHideBrowser.Index = 3;
			this.miHideBrowser.Shortcut = System.Windows.Forms.Shortcut.F8;
			this.miHideBrowser.Text = "Hide &Object Browser";
			this.miHideBrowser.Click += new System.EventHandler(this.miHideBrowser_Click);
			// 
			// miHelp
			// 
			this.miHelp.Index = 4;
			this.miHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				   this.miAbout});
			this.miHelp.Text = "&Help";
			// 
			// miAbout
			// 
			this.miAbout.Index = 0;
			this.miAbout.Text = "&About...";
			this.miAbout.Click += new System.EventHandler(this.miAbout_Click);
			// 
			// sep5
			// 
			this.sep5.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbOpen
			// 
			this.tbOpen.ImageIndex = 3;
			this.tbOpen.ToolTipText = "Open Existing Query";
			// 
			// sep3
			// 
			this.sep3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbHideBrowser
			// 
			this.tbHideBrowser.ImageIndex = 10;
			this.tbHideBrowser.ToolTipText = "Show/Hide Browser";
			// 
			// tbSave
			// 
			this.tbSave.ImageIndex = 4;
			this.tbSave.ToolTipText = "Save Query";
			// 
			// sep1
			// 
			this.sep1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbConnect
			// 
			this.tbConnect.ImageIndex = 0;
			this.tbConnect.ToolTipText = "Connect";
			// 
			// mdiClient1
			// 
			this.mdiClient1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mdiClient1.Location = new System.Drawing.Point(0, 30);
			this.mdiClient1.Name = "mdiClient1";
			this.mdiClient1.TabIndex = 0;
			// 
			// tbExecute
			// 
			this.tbExecute.ImageIndex = 5;
			this.tbExecute.ToolTipText = "Execute Query";
			// 
			// tbResultsGrid
			// 
			this.tbResultsGrid.ImageIndex = 8;
			this.tbResultsGrid.ToolTipText = "Results in Grid";
			// 
			// tbNew
			// 
			this.tbNew.ImageIndex = 2;
			this.tbNew.ToolTipText = "New Query Window";
			// 
			// tbResultsText
			// 
			this.tbResultsText.ImageIndex = 7;
			this.tbResultsText.ToolTipText = "Results in Text";
			// 
			// sep4
			// 
			this.sep4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// sep2
			// 
			this.sep2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbDisconnect
			// 
			this.tbDisconnect.ImageIndex = 1;
			this.tbDisconnect.ToolTipText = "Disconnect and Close Query";
			// 
			// toolBar
			// 
			this.toolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																					   this.tbConnect,
																					   this.tbDisconnect,
																					   this.sep1,
																					   this.tbNew,
																					   this.tbOpen,
																					   this.tbSave,
																					   this.sep2,
																					   this.tbExecute,
																					   this.tbCancel,
																					   this.sep3,
																					   this.tbResultsText,
																					   this.tbResultsGrid,
																					   this.sep4,
																					   this.tbHideResults,
																					   this.tbHideBrowser,
																					   this.sep5});
			this.toolBar.DropDownArrows = true;
			this.toolBar.ImageList = this.imageList;
			this.toolBar.Name = "toolBar";
			this.toolBar.ShowToolTips = true;
			this.toolBar.Size = new System.Drawing.Size(777, 30);
			this.toolBar.TabIndex = 0;
			this.toolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar_ButtonClick);
			// 
			// tbCancel
			// 
			this.tbCancel.ImageIndex = 6;
			this.tbCancel.ToolTipText = "Cancel Executing Query";
			// 
			// tbHideResults
			// 
			this.tbHideResults.ImageIndex = 9;
			this.tbHideResults.ToolTipText = "Show/Hide Results Window";
			// 
			// imageList
			// 
			this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList.ImageSize = new System.Drawing.Size(23, 21);
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// editManager
			// 
			this.editManager.MenuItemCopy = this.miCopy;
			this.editManager.MenuItemCut = this.miCut;
			this.editManager.MenuItemEdit = this.miEdit;
			this.editManager.MenuItemPaste = this.miPaste;
			this.editManager.MenuItemSelectAll = this.miSelectAll;
			this.editManager.MenuItemUndo = this.miUndo;
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 5;
			this.menuItem3.Text = "-";
			// 
			// miSelectAll
			// 
			this.miSelectAll.Index = 6;
			this.miSelectAll.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
			this.miSelectAll.Text = "Select &All";
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(777, 531);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.toolBar,
																		  this.mdiClient1});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.IsMdiContainer = true;
			this.Menu = this.mainMenu;
			this.Name = "MainForm";
			this.Text = "Query Express";
			this.MdiChildActivate += new System.EventHandler(this.MainForm_MdiChildActivate);
			this.ResumeLayout(false);

		}
		#endregion

		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		#region Event Handlers
        
		private void MainForm_MdiChildActivate(object sender, System.EventArgs e)
		{
			// The user has just clicked a new query form - update the menu / toolbar accordingly
			EnableControls();
		}

		private void toolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			// We've assigned events to the Tag property of all our toolbar buttons in the form's constructor.
			// Now, we can just invoke the event handler.
			if (e.Button.Tag is EventHandler)
			{
				EventHandler eh = (EventHandler) e.Button.Tag;
				eh (e.Button, EventArgs.Empty);
			}
		}

		private void miConnect_Click (object sender, System.EventArgs e)
		{
			// Create & show a connection dialog form, then hand this to a new Query form.
			ConnectForm cf = new ConnectForm();
			if (cf.ShowDialog() == DialogResult.OK)
			{
				QueryForm qf = new QueryForm (cf.DbClient, cf.Browser, cf.LowBandwidth);
				qf.MdiParent = this;
				// This is so that we can update the toolbar and menu as the state of the QueryForm changes.
				qf.PropertyChanged += new EventHandler (ChildPropertyChanged);
				qf.Show();
			}
		}

		private void miDisconnect_Click (object sender, System.EventArgs e)
		{
			if (IsChildActive()) GetQueryChild().Close();
		}

		private void miNew_Click (object sender, System.EventArgs e)
		{
			if (IsChildActive())
			{
				// Change the cursor to an hourglass while we're doing this, in case establishing the
				// new connection takes some time.
				Cursor oldCursor = Cursor;
				Cursor = Cursors.WaitCursor;
				QueryForm newQF = GetQueryChild().Clone();
				if (newQF != null)																// could be null if new connection failed
				{
					newQF.MdiParent = this;
					newQF.PropertyChanged += new EventHandler (ChildPropertyChanged);
					newQF.Show();
				}
				Cursor = oldCursor;
			}
		}

		private void miOpen_Click (object sender, System.EventArgs e)
		{
			if (IsChildActive())	GetQueryChild().Open();
		}

		private void miSave_Click (object sender, System.EventArgs e)
		{
			if (IsChildActive())	GetQueryChild().Save();
		}

		private void miSaveAs_Click (object sender, System.EventArgs e)
		{
			if (IsChildActive()) GetQueryChild().SaveAs();
		}

		private void miSaveResults_Click (object sender, System.EventArgs e)
		{
			if (IsChildActive()) GetQueryChild().SaveResults();
		}

		private void miExit_Click (object sender, System.EventArgs e)
		{
			Close();
		}

		private void miExecute_Click(object sender, System.EventArgs e)
		{
			if (IsChildActive()) GetQueryChild().Execute();
		}

		private void miCancel_Click (object sender, System.EventArgs e)
		{
			if (IsChildActive()) GetQueryChild().Cancel();
		}

		private void miResultsText_Click(object sender, System.EventArgs e)
		{
			// Changing the value of this property will automatically invoke the QueryForm's
			// PropertyChanged event, which we've wired to EnableControls().
			if (IsChildActive()) GetQueryChild().ResultsInText = true;
		}

		private void miResultsGrid_Click(object sender, System.EventArgs e)
		{
			if (IsChildActive()) GetQueryChild().ResultsInText = false;
		}

		private void miNextPane_Click(object sender, System.EventArgs e)
		{
			if (IsChildActive()) GetQueryChild().SwitchPane (true);
		}

		private void miPrevPane_Click(object sender, System.EventArgs e)
		{
			if (IsChildActive()) GetQueryChild().SwitchPane (false);
		}

		private void miHideResults_Click(object sender, System.EventArgs e)
		{
			if (IsChildActive()) GetQueryChild().HideResults = !GetQueryChild().HideResults;
		}

		private void miHideBrowser_Click(object sender, System.EventArgs e)
		{
			if (IsChildActive()) GetQueryChild().HideBrowser = !GetQueryChild().HideBrowser;
		}

		private void miAbout_Click (object sender, System.EventArgs e)
		{
			new About().ShowDialog();
		}

		#endregion

	}

}