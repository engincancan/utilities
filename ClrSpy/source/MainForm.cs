// MainForm.cs
// Author: Adam Nathan
// Date: 5/11/2003
// For more information, see http://blogs.gotdotnet.com/anathan
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace ClrSpy
{
	/// <summary>
	/// The main application form.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		#region Fields for the UI
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Button aboutButton;
		private System.Windows.Forms.Button optionsButton;
		private System.Windows.Forms.Button probeButton;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.Button removeButton;
		private System.Windows.Forms.Button editFilterButton;
		private System.Windows.Forms.CheckBox checkMarshaling;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.GroupBox errorGroup;
		private System.Windows.Forms.GroupBox failureGroup;
		private System.Windows.Forms.GroupBox warningGroup;
		private System.Windows.Forms.GroupBox infoGroup;
		private System.Windows.Forms.GroupBox filterGroup;
		private System.Windows.Forms.ImageList fileIconList;
		private System.Windows.Forms.ImageList clrSpyIconList;
		private System.Windows.Forms.Label filterText;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView programList;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.Panel sidePanel;
		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.PictureBox logo;
		private System.Windows.Forms.PictureBox errorMessagesPicture;
		private System.Windows.Forms.PictureBox failurePicture;
		private System.Windows.Forms.PictureBox warningPicture;
		private System.Windows.Forms.PictureBox infoPicture;
		private System.Windows.Forms.RadioButton radioEverything;
		private System.Windows.Forms.RadioButton radioFilter;
		private System.Windows.Forms.TextBox filterTextBox;
		private ClrSpy.MyMenuItem menu_ShowOrHide;
		private ClrSpy.MyMenuItem menu_ProbeState;
		private ClrSpy.MyMenuItem menu_Exit;
		private ClrSpy.MyContextMenu contextMenu;
		private ClrSpy.MyNotifyIcon notifyIcon;
		#endregion

		private static DebugMonitor dm = null;
		private string summaryText;
		private bool probesAllowed = true;
		private bool closeApplication = false;
		private bool sessionEnding = false;

		private const int padding = 8;
		private const int topPadding = 86;

		private const string infoPrefix = "CDP> Logged information from ";
		private const string errorPrefix = "CDP> Reported error from ";
		private const string probePrefix = "CDP> ";

		// The main entry point for the application.
		[STAThread]
		static void Main() 
		{
			// Balloon tooltips are only supported on Windows XP or greater
			if (Environment.OSVersion.Platform != PlatformID.Win32NT ||
				Environment.OSVersion.Version < new Version(5, 1))
			{
				MessageBox.Show("CLR SPY is only fully functional on Windows XP or greater.  Balloon tooltips will not be shown, but you can still log messages to a file.  See the Options dialog for more details.", "Pre-Windows XP Operating System", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Options.ShowBalloons = false;
			}

			try
			{
				dm = new DebugMonitor();
				dm.Start();
			}
			catch (AlreadyRunningException)
			{
				MessageBox.Show("CLR SPY (or another debug monitor) is already running.  You can only have one instance open at a time.", "CLR Spy Already Running", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			Application.EnableVisualStyles();
			Application.Run(new MainForm());
		}

		// Constructor
		public MainForm()
		{
			// Required for Windows Form Designer support
			InitializeComponent();

			// Fill in the list of probes, grouped by category
			int settingProbes = 0;
			int errorProbes = 0;
			int failureProbes = 0;
			int warningProbes = 0;
			int infoProbes = 0;

			int fontHeight = Convert.ToInt32(Math.Max(16, this.Font.GetHeight()), CultureInfo.InvariantCulture); 

			// Process the list of global settings
			for (int i = 0; i < Probe.GlobalSettingsList.Length; i++)
			{
				if (Probe.GlobalSettingsList[i].ConfigName == "CDP.AllowDebugProbes")
				{
					// Treat this setting specially, with the probeButton
				}
				else
				{
					CheckBox cb = new CheckBox();
					Probe.GlobalSettingsList[i].CheckBox = cb;
					cb.FlatStyle = FlatStyle.System;
					cb.Text = Probe.GlobalSettingsList[i].DisplayName;
					cb.Tag = Probe.GlobalSettingsList[i].ConfigName;
					cb.Left = padding;
					cb.Height = fontHeight;
					cb.Width = errorGroup.Width - cb.Left - padding;
					cb.CheckedChanged += new System.EventHandler(this.GlobalDataChanged);

					switch (Probe.GlobalSettingsList[i].Type)
					{
						case ProbeType.Setting:
							settingProbes++;
							this.Controls.Add(cb);
							cb.Left = padding * 2;
							cb.Top = topPadding + (settingProbes-1) * fontHeight;
							break;
						default:
							throw new ApplicationException("Entries in GlobalSettingsList must be of type Setting, not " + Probe.GlobalSettingsList[i].Type);
					}
				}
			}

			// Process the list of probes
			for (int i = 0; i < Probe.ProbeList.Length; i++)
			{
				if (Probe.ProbeList[i].ConfigName == "CDP.Marshaling.Filter")
				{
					// Treat this setting specially, with the filter controls
				}
				else
				{
					CheckBox cb = new CheckBox();
					Probe.ProbeList[i].CheckBox = cb;
					cb.FlatStyle = FlatStyle.System;
					cb.Text = Probe.ProbeList[i].DisplayName;
					cb.Tag = Probe.ProbeList[i].ConfigName;
					cb.Left = padding;
					cb.Height = fontHeight;
					cb.Width = errorGroup.Width - cb.Left - padding;
					cb.CheckedChanged += new System.EventHandler(this.ProbeDataChanged);

					switch (Probe.ProbeList[i].Type)
					{
						case ProbeType.Error:
							errorProbes++;
							errorGroup.Controls.Add(cb);
							cb.Top = errorProbes * fontHeight;
							break;
						case ProbeType.Failure:
							failureProbes++;
							failureGroup.Controls.Add(cb);
							cb.Top = failureProbes * fontHeight;
							break;
						case ProbeType.Warning:
							warningProbes++;
							warningGroup.Controls.Add(cb);
							cb.Top = warningProbes * fontHeight;
							break;
						case ProbeType.Info:
							if (cb.Text == "Marshaling")
							{
								Probe.ProbeList[i].CheckBox = checkMarshaling;
							}
							else
							{
								Debug.Assert(false, "Ignoring info probe \"" + cb.Text + "\" because this is hardcoded for marshaling.");
							}
							infoProbes++;
							break;
						default:
							throw new ApplicationException("Unexpected probe type in ProbeList: " + Probe.ProbeList[i].Type);
					}
				}
			}

			// Size the groupboxes appropriately
			errorGroup.Height = padding + (errorProbes + 1) * fontHeight;
			errorGroup.TabIndex = 1;
			failureGroup.Height = padding + (failureProbes + 1) * fontHeight;
			failureGroup.TabIndex = 2;
			warningGroup.Height = padding + (warningProbes + 1) * fontHeight;
			warningGroup.TabIndex = 3;
			infoGroup.Height = padding + radioFilter.Bottom;
			infoGroup.TabIndex = 4;

			errorGroup.Top = (padding/2) + topPadding + (settingProbes) * fontHeight;
			failureGroup.Top = errorGroup.Bottom + padding;
			warningGroup.Top = failureGroup.Bottom + padding;
			infoGroup.Top = warningGroup.Bottom + padding;

			filterGroup.Top = infoGroup.Top;
			filterGroup.Height = infoGroup.Height;
			sidePanel.Height = filterGroup.Top - padding - sidePanel.Top;

			// Set the boundaries of the form
			this.ClientSize = new Size(this.ClientSize.Width, infoGroup.Bottom + padding * 2);
			this.MinimumSize = new Size(this.Width, this.Height);
			this.MaximumSize = new Size(this.Width * 2, this.Height);

			// Initialize this form's icon to the first image in the list
			Bitmap bmp = new Bitmap(clrSpyIconList.Images[0]);
			this.Icon = Icon.FromHandle(bmp.GetHicon());

			// Set the transparent color on the form's pictures
			((Bitmap)errorMessagesPicture.Image).MakeTransparent(Color.Lime);
			((Bitmap)failurePicture.Image).MakeTransparent(Color.Lime);
			((Bitmap)warningPicture.Image).MakeTransparent(Color.Lime);
			((Bitmap)infoPicture.Image).MakeTransparent(Color.Lime);

			notifyIcon.Icon = this.Icon;
			notifyIcon.ContextMenu = this.contextMenu;
			notifyIcon.Visible = true;

			try
			{
				// Clear all CDP settings in machine.config
				DebugProbeConfigFile.EraseAllProbeSettings(RuntimeEnvironment.SystemConfigurationFile, false);
			}
			catch (Exception ex)
			{
				MessageBox.Show("There was an error writing to \"" + RuntimeEnvironment.SystemConfigurationFile + 
					"\": " + ex.Message + "  CLR SPY will not function properly unless you manually set AllowDebugProbes=true in this file.",
					"Error Writing to Configuration File", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			// Initialize the settings, using this app's config file if present
			foreach (Probe p in Probe.GlobalSettingsList)
			{
				if (p.CheckBox != null) 
				{
					if (ConfigurationSettings.AppSettings[p.ConfigName] == "false")
						p.CheckBox.Checked = false;
					else
						p.CheckBox.Checked = true;
				}
			}
			foreach (Probe p in Probe.ProbeList)
			{
				if (p.CheckBox != null) 
				{
					if (ConfigurationSettings.AppSettings[p.ConfigName] == "false")
						p.CheckBox.Checked = false;
					else
						p.CheckBox.Checked = true;
				}
				else if (p.ConfigName == "CDP.Marshaling.Filter")
				{
					filterText.Text = ConfigurationSettings.AppSettings[p.ConfigName];
				}
			}

			// Apply other options saved in CLR SPY's config file

			if (ConfigurationSettings.AppSettings["CLRSPY.NoFilter"] == "false")
				radioFilter.Checked = true;
			else
				radioEverything.Checked = true;

			// Don't set ShowBalloons to true at this point because the default is
			// already correct (true unless the OS doesn't support balloon tooltips).
			if (ConfigurationSettings.AppSettings["CLRSPY.ShowBalloons"] == "false")
				Options.ShowBalloons = false;

			if (ConfigurationSettings.AppSettings["CLRSPY.LogToFile"] == "true")
				Options.LogToFile = true;
			else
				Options.LogToFile = false;

			if (ConfigurationSettings.AppSettings["CLRSPY.LogFile"] != null)
				Options.LogFile = ConfigurationSettings.AppSettings["CLRSPY.LogFile"];

			// Now that the probe settings are in place, fill the program list.
			// Any programs that get added will have their config file updated to
			// match the current settings.
			string programs = ConfigurationSettings.AppSettings["CLRSPY.ProgramList"];
			if (programs != null)
			{
				foreach (string s in programs.Split('|'))
				{
					if (s.Length > 0)
					{
						AddFileToProgramList(s);
					}
				}
			}

			// Update the summary to show the right number of applications
			UpdateSummary();

			// Set up callback for probe notifications
			dm.Message += new DebugMessageEventHandler(ProcessDebugMessage);
		}

		// Clean up any resources being used, and stop monitoring.
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
				dm.Stop();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.contextMenu = new ClrSpy.MyContextMenu();
			this.menu_ShowOrHide = new ClrSpy.MyMenuItem();
			this.menu_ProbeState = new ClrSpy.MyMenuItem();
			this.menu_Exit = new ClrSpy.MyMenuItem();
			this.notifyIcon = new ClrSpy.MyNotifyIcon();
			this.infoGroup = new System.Windows.Forms.GroupBox();
			this.checkMarshaling = new System.Windows.Forms.CheckBox();
			this.infoPicture = new System.Windows.Forms.PictureBox();
			this.radioFilter = new System.Windows.Forms.RadioButton();
			this.radioEverything = new System.Windows.Forms.RadioButton();
			this.editFilterButton = new System.Windows.Forms.Button();
			this.errorGroup = new System.Windows.Forms.GroupBox();
			this.errorMessagesPicture = new System.Windows.Forms.PictureBox();
			this.warningGroup = new System.Windows.Forms.GroupBox();
			this.warningPicture = new System.Windows.Forms.PictureBox();
			this.sidePanel = new System.Windows.Forms.Panel();
			this.programList = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.fileIconList = new System.Windows.Forms.ImageList(this.components);
			this.removeButton = new System.Windows.Forms.Button();
			this.addButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.topPanel = new System.Windows.Forms.Panel();
			this.aboutButton = new System.Windows.Forms.Button();
			this.probeButton = new System.Windows.Forms.Button();
			this.logo = new System.Windows.Forms.PictureBox();
			this.clrSpyIconList = new System.Windows.Forms.ImageList(this.components);
			this.failureGroup = new System.Windows.Forms.GroupBox();
			this.failurePicture = new System.Windows.Forms.PictureBox();
			this.filterText = new System.Windows.Forms.Label();
			this.filterGroup = new System.Windows.Forms.GroupBox();
			this.filterTextBox = new System.Windows.Forms.TextBox();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.optionsButton = new System.Windows.Forms.Button();
			this.infoGroup.SuspendLayout();
			this.errorGroup.SuspendLayout();
			this.warningGroup.SuspendLayout();
			this.sidePanel.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.failureGroup.SuspendLayout();
			this.filterGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenu
			// 
			this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.menu_ShowOrHide,
																						this.menu_ProbeState,
																						this.menu_Exit});
			// 
			// menu_ShowOrHide
			// 
			this.menu_ShowOrHide.DefaultItem = true;
			this.menu_ShowOrHide.Index = 0;
			this.menu_ShowOrHide.Text = "Hide";
			this.menu_ShowOrHide.Click += new System.EventHandler(this.menu_ShowOrHide_Click);
			// 
			// menu_ProbeState
			// 
			this.menu_ProbeState.Index = 1;
			this.menu_ProbeState.Text = "Stop Probing";
			this.menu_ProbeState.Click += new System.EventHandler(this.menu_ProbeState_Click);
			// 
			// menu_Exit
			// 
			this.menu_Exit.Index = 2;
			this.menu_Exit.Text = "Exit";
			this.menu_Exit.Click += new System.EventHandler(this.menu_Exit_Click);
			// 
			// notifyIcon
			// 
			this.notifyIcon.ContextMenu = this.contextMenu;
			this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
			this.notifyIcon.Text = null;
			this.notifyIcon.Visible = false;
			this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
			// 
			// infoGroup
			// 
			this.infoGroup.Controls.Add(this.checkMarshaling);
			this.infoGroup.Controls.Add(this.infoPicture);
			this.infoGroup.Controls.Add(this.radioFilter);
			this.infoGroup.Controls.Add(this.radioEverything);
			this.infoGroup.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.infoGroup.Location = new System.Drawing.Point(8, 296);
			this.infoGroup.Name = "infoGroup";
			this.infoGroup.Size = new System.Drawing.Size(216, 78);
			this.infoGroup.TabIndex = 0;
			this.infoGroup.TabStop = false;
			this.infoGroup.Text = "Show Extra Information";
			// 
			// checkMarshaling
			// 
			this.checkMarshaling.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkMarshaling.Location = new System.Drawing.Point(8, 16);
			this.checkMarshaling.Name = "checkMarshaling";
			this.checkMarshaling.Size = new System.Drawing.Size(104, 16);
			this.checkMarshaling.TabIndex = 23;
			this.checkMarshaling.Tag = "CDP.Marshaling";
			this.checkMarshaling.Text = "Marshaling";
			this.checkMarshaling.CheckedChanged += new System.EventHandler(this.ProbeDataChanged);
			// 
			// infoPicture
			// 
			this.infoPicture.Image = ((System.Drawing.Image)(resources.GetObject("infoPicture.Image")));
			this.infoPicture.Location = new System.Drawing.Point(176, 15);
			this.infoPicture.Name = "infoPicture";
			this.infoPicture.Size = new System.Drawing.Size(32, 32);
			this.infoPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.infoPicture.TabIndex = 22;
			this.infoPicture.TabStop = false;
			// 
			// radioFilter
			// 
			this.radioFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioFilter.Location = new System.Drawing.Point(24, 50);
			this.radioFilter.Name = "radioFilter";
			this.radioFilter.Size = new System.Drawing.Size(104, 16);
			this.radioFilter.TabIndex = 13;
			this.radioFilter.Text = "Filter";
			this.radioFilter.CheckedChanged += new System.EventHandler(this.ProbeDataChanged);
			// 
			// radioEverything
			// 
			this.radioEverything.Checked = true;
			this.radioEverything.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioEverything.Location = new System.Drawing.Point(24, 33);
			this.radioEverything.Name = "radioEverything";
			this.radioEverything.Size = new System.Drawing.Size(104, 16);
			this.radioEverything.TabIndex = 12;
			this.radioEverything.TabStop = true;
			this.radioEverything.Text = "Everything";
			this.radioEverything.CheckedChanged += new System.EventHandler(this.ProbeDataChanged);
			// 
			// editFilterButton
			// 
			this.editFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.editFilterButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.editFilterButton.Location = new System.Drawing.Point(144, 48);
			this.editFilterButton.Name = "editFilterButton";
			this.editFilterButton.Size = new System.Drawing.Size(48, 23);
			this.editFilterButton.TabIndex = 21;
			this.editFilterButton.Text = "Edit";
			this.editFilterButton.Click += new System.EventHandler(this.editFilterButton_Click);
			// 
			// errorGroup
			// 
			this.errorGroup.BackColor = System.Drawing.SystemColors.Control;
			this.errorGroup.Controls.Add(this.errorMessagesPicture);
			this.errorGroup.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.errorGroup.Location = new System.Drawing.Point(8, 106);
			this.errorGroup.Name = "errorGroup";
			this.errorGroup.Size = new System.Drawing.Size(216, 56);
			this.errorGroup.TabIndex = 1;
			this.errorGroup.TabStop = false;
			this.errorGroup.Text = "Show Error Messages";
			// 
			// errorMessagesPicture
			// 
			this.errorMessagesPicture.Image = ((System.Drawing.Image)(resources.GetObject("errorMessagesPicture.Image")));
			this.errorMessagesPicture.Location = new System.Drawing.Point(176, 15);
			this.errorMessagesPicture.Name = "errorMessagesPicture";
			this.errorMessagesPicture.Size = new System.Drawing.Size(32, 32);
			this.errorMessagesPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.errorMessagesPicture.TabIndex = 11;
			this.errorMessagesPicture.TabStop = false;
			// 
			// warningGroup
			// 
			this.warningGroup.Controls.Add(this.warningPicture);
			this.warningGroup.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.warningGroup.Location = new System.Drawing.Point(8, 232);
			this.warningGroup.Name = "warningGroup";
			this.warningGroup.Size = new System.Drawing.Size(216, 56);
			this.warningGroup.TabIndex = 2;
			this.warningGroup.TabStop = false;
			this.warningGroup.Text = "Show Warning Messages";
			// 
			// warningPicture
			// 
			this.warningPicture.Image = ((System.Drawing.Image)(resources.GetObject("warningPicture.Image")));
			this.warningPicture.Location = new System.Drawing.Point(176, 15);
			this.warningPicture.Name = "warningPicture";
			this.warningPicture.Size = new System.Drawing.Size(31, 32);
			this.warningPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.warningPicture.TabIndex = 13;
			this.warningPicture.TabStop = false;
			// 
			// sidePanel
			// 
			this.sidePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.sidePanel.Controls.Add(this.programList);
			this.sidePanel.Controls.Add(this.removeButton);
			this.sidePanel.Controls.Add(this.addButton);
			this.sidePanel.Controls.Add(this.label1);
			this.sidePanel.Location = new System.Drawing.Point(232, 88);
			this.sidePanel.Name = "sidePanel";
			this.sidePanel.Size = new System.Drawing.Size(200, 200);
			this.sidePanel.TabIndex = 12;
			this.sidePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.sidePanel_Paint);
			// 
			// programList
			// 
			this.programList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.programList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.columnHeader1,
																						  this.columnHeader2});
			this.programList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.programList.Location = new System.Drawing.Point(8, 56);
			this.programList.Name = "programList";
			this.programList.Size = new System.Drawing.Size(184, 136);
			this.programList.SmallImageList = this.fileIconList;
			this.programList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.programList.TabIndex = 21;
			this.programList.View = System.Windows.Forms.View.Details;
			this.programList.SelectedIndexChanged += new System.EventHandler(this.programList_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Application";
			this.columnHeader1.Width = 140;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Path";
			this.columnHeader2.Width = 200;
			// 
			// fileIconList
			// 
			this.fileIconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.fileIconList.ImageSize = new System.Drawing.Size(16, 16);
			this.fileIconList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// removeButton
			// 
			this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.removeButton.Enabled = false;
			this.removeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.removeButton.Location = new System.Drawing.Point(64, 25);
			this.removeButton.Name = "removeButton";
			this.removeButton.Size = new System.Drawing.Size(128, 23);
			this.removeButton.TabIndex = 20;
			this.removeButton.Text = "Remove Selected Items";
			this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// addButton
			// 
			this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.addButton.Location = new System.Drawing.Point(8, 25);
			this.addButton.Name = "addButton";
			this.addButton.Size = new System.Drawing.Size(50, 23);
			this.addButton.TabIndex = 18;
			this.addButton.Text = "Add...";
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.label1.Location = new System.Drawing.Point(8, 7);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(119, 16);
			this.label1.TabIndex = 13;
			this.label1.Text = "Monitored Applications";
			// 
			// topPanel
			// 
			this.topPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.topPanel.BackColor = System.Drawing.SystemColors.Control;
			this.topPanel.Controls.Add(this.optionsButton);
			this.topPanel.Controls.Add(this.aboutButton);
			this.topPanel.Controls.Add(this.probeButton);
			this.topPanel.Controls.Add(this.logo);
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = new System.Drawing.Size(440, 79);
			this.topPanel.TabIndex = 15;
			this.topPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.topPanel_Paint);
			// 
			// aboutButton
			// 
			this.aboutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.aboutButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.aboutButton.Location = new System.Drawing.Point(328, 4);
			this.aboutButton.Name = "aboutButton";
			this.aboutButton.Size = new System.Drawing.Size(104, 23);
			this.aboutButton.TabIndex = 15;
			this.aboutButton.Text = "About...";
			this.aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
			// 
			// probeButton
			// 
			this.probeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.probeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.probeButton.Location = new System.Drawing.Point(328, 52);
			this.probeButton.Name = "probeButton";
			this.probeButton.Size = new System.Drawing.Size(104, 23);
			this.probeButton.TabIndex = 16;
			this.probeButton.Text = "Stop Probing";
			this.probeButton.Click += new System.EventHandler(this.probeButton_Click);
			// 
			// logo
			// 
			this.logo.BackColor = System.Drawing.Color.White;
			this.logo.Image = ((System.Drawing.Image)(resources.GetObject("logo.Image")));
			this.logo.Location = new System.Drawing.Point(0, 0);
			this.logo.Name = "logo";
			this.logo.Size = new System.Drawing.Size(97, 79);
			this.logo.TabIndex = 15;
			this.logo.TabStop = false;
			// 
			// clrSpyIconList
			// 
			this.clrSpyIconList.ImageSize = new System.Drawing.Size(16, 16);
			this.clrSpyIconList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("clrSpyIconList.ImageStream")));
			this.clrSpyIconList.TransparentColor = System.Drawing.Color.Lime;
			// 
			// failureGroup
			// 
			this.failureGroup.Controls.Add(this.failurePicture);
			this.failureGroup.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.failureGroup.Location = new System.Drawing.Point(8, 168);
			this.failureGroup.Name = "failureGroup";
			this.failureGroup.Size = new System.Drawing.Size(216, 54);
			this.failureGroup.TabIndex = 16;
			this.failureGroup.TabStop = false;
			this.failureGroup.Text = "Force Non-Deterministic Failures";
			// 
			// failurePicture
			// 
			this.failurePicture.Image = ((System.Drawing.Image)(resources.GetObject("failurePicture.Image")));
			this.failurePicture.Location = new System.Drawing.Point(176, 15);
			this.failurePicture.Name = "failurePicture";
			this.failurePicture.Size = new System.Drawing.Size(30, 30);
			this.failurePicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.failurePicture.TabIndex = 12;
			this.failurePicture.TabStop = false;
			// 
			// filterText
			// 
			this.filterText.AutoSize = true;
			this.filterText.BackColor = System.Drawing.SystemColors.Control;
			this.filterText.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.filterText.Location = new System.Drawing.Point(11, 19);
			this.filterText.Name = "filterText";
			this.filterText.Size = new System.Drawing.Size(0, 16);
			this.filterText.TabIndex = 18;
			// 
			// filterGroup
			// 
			this.filterGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.filterGroup.Controls.Add(this.filterTextBox);
			this.filterGroup.Controls.Add(this.editFilterButton);
			this.filterGroup.Controls.Add(this.filterText);
			this.filterGroup.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.filterGroup.Location = new System.Drawing.Point(232, 296);
			this.filterGroup.Name = "filterGroup";
			this.filterGroup.Size = new System.Drawing.Size(200, 78);
			this.filterGroup.TabIndex = 22;
			this.filterGroup.TabStop = false;
			this.filterGroup.Text = "Marshaling Filter";
			// 
			// filterTextBox
			// 
			this.filterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.filterTextBox.Location = new System.Drawing.Point(8, 16);
			this.filterTextBox.Name = "filterTextBox";
			this.filterTextBox.Size = new System.Drawing.Size(184, 20);
			this.filterTextBox.TabIndex = 23;
			this.filterTextBox.Text = "";
			this.filterTextBox.Visible = false;
			// 
			// optionsButton
			// 
			this.optionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.optionsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.optionsButton.Location = new System.Drawing.Point(328, 28);
			this.optionsButton.Name = "optionsButton";
			this.optionsButton.Size = new System.Drawing.Size(104, 23);
			this.optionsButton.TabIndex = 18;
			this.optionsButton.Text = "Options...";
			this.optionsButton.Click += new System.EventHandler(this.optionsButton_Click);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(440, 390);
			this.Controls.Add(this.filterGroup);
			this.Controls.Add(this.failureGroup);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.sidePanel);
			this.Controls.Add(this.warningGroup);
			this.Controls.Add(this.errorGroup);
			this.Controls.Add(this.infoGroup);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(400, 350);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "CLR SPY";
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
			this.Closed += new System.EventHandler(this.MainForm_Closed);
			this.infoGroup.ResumeLayout(false);
			this.errorGroup.ResumeLayout(false);
			this.warningGroup.ResumeLayout(false);
			this.sidePanel.ResumeLayout(false);
			this.topPanel.ResumeLayout(false);
			this.failureGroup.ResumeLayout(false);
			this.filterGroup.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		// Keep the controls related to marshaling and filtering in-sync
		private void UpdateMarshalingState()
		{
			if (checkMarshaling.Checked)
			{
				radioEverything.Enabled = true;
				radioFilter.Enabled = true;
				filterText.Enabled = radioFilter.Checked;
			}
			else
			{
				radioEverything.Enabled = false;
				radioFilter.Enabled = false;
				filterText.Enabled = false;
			}
		}

		// Update the summary sentence, which reports how many probes
		// are enabled and how many applications are being monitored
		private void UpdateSummary()
		{
			if (!probesAllowed)
			{
				summaryText = "All probes are disabled.";
			}
			else if (programList.Items.Count == 0)
			{
				summaryText = "No applications are being monitored.";
			}
			else
			{
				// Count how many probes are enabled
				int count = 0;
				foreach (Control c in errorGroup.Controls)
				{
					if (c is CheckBox && ((CheckBox)c).Checked) count++;
				}
				foreach (Control c in failureGroup.Controls)
				{
					if (c is CheckBox && ((CheckBox)c).Checked) count++;
				}
				foreach (Control c in warningGroup.Controls)
				{
					if (c is CheckBox && ((CheckBox)c).Checked) count++;
				}
				foreach (Control c in infoGroup.Controls)
				{
					if (c is CheckBox && ((CheckBox)c).Checked) count++;
				}

				if (count == 0)
					summaryText = "No probes are enabled for ";
				else if (count == 1)
					summaryText = "1 probe is enabled for ";
				else
					summaryText = count + " probes are enabled for ";

				if (programList.Items.Count == 1)
					summaryText += "1 application.";
				else
					summaryText += programList.Items.Count + " applications.";
			}

			notifyIcon.Text = "CLR SPY: " + summaryText;
			topPanel.Invalidate();
		}

		// Add a unique application to the list to be monitored, which
		// involves extracting an icon for the application and configuring it
		// with a .config file.
		private void AddFileToProgramList(string filename)
		{
			// Don't add to the list if it's already there
			foreach (ListViewItem lvi in programList.Items)
			{
				if (String.Compare(lvi.Tag.ToString(), filename, true, CultureInfo.CurrentCulture) == 0) return;
			}

			try
			{
				// Extract the appropriate icon and wrap it in a System.Drawing.Icon
				SHFILEINFO shfi = new SHFILEINFO();

				SHGetFileInfo(filename, FILE_ATTRIBUTE_NORMAL, ref shfi, Marshal.SizeOf(shfi),
					SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);

				fileIconList.Images.Add(Icon.FromHandle(shfi.hIcon));
			}
			catch
			{
				// If this fails, just use the CLR SPY icon
				fileIconList.Images.Add(this.Icon);
			}

			ListViewItem newItem = new ListViewItem(new string[]{Path.GetFileName(filename), Path.GetDirectoryName(filename)}, fileIconList.Images.Count - 1);
			newItem.Tag = filename;
			programList.Items.Add(newItem);

			UpdateApplicationProbeSettings(filename + ".config");
		}

		// Write the current probe settings into the passed-in configuration file
		private void UpdateApplicationProbeSettings(string configFilename)
		{
			try
			{
				using (DebugProbeConfigFile config = new DebugProbeConfigFile(configFilename, true))
				{
					foreach (Probe p in Probe.ProbeList)
					{
						if (p.ConfigName == "CDP.Marshaling.Filter")
							config.SetAttribute(p.ConfigName, radioEverything.Checked ? "everything" : filterText.Text);
						else
							config.SetAttribute(p.ConfigName, p.CheckBox.Checked.ToString().ToLower(CultureInfo.InvariantCulture));
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("There was an error writing to \"" + configFilename + 
					"\": " + ex.Message, "Error Writing to Configuration File", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// Animates the icon from an open eye to a closed eye, or vice versa
		private void ChangeIcon(bool enabledToDisabled)
		{
			int finalIcon;

			if (enabledToDisabled)
				finalIcon = 2;
			else
				finalIcon = 0;

			// This is the "eye half-open" picture
			Bitmap bmp = new Bitmap(clrSpyIconList.Images[1]);
			this.Icon = Icon.FromHandle(bmp.GetHicon());
			notifyIcon.Icon = this.Icon;

			// Briefly sleep so the animation is noticeable
			System.Threading.Thread.Sleep(100);

			// This is the destination picture
			bmp = new Bitmap(clrSpyIconList.Images[finalIcon]);
			this.Icon = Icon.FromHandle(bmp.GetHicon());
			notifyIcon.Icon = this.Icon;
		}

		// Ensure that system shutdown or user logout properly exits
		// this application.  This is done "manually" since the
		// SessionEnding/SessionEnded events don't help due to the way
		// this app is structured.
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_ENDSESSION || m.Msg == WM_QUERYENDSESSION) sessionEnding = true;
			base.WndProc (ref m);
		}

		#region Event handlers
		// Called when user activity triggers a change in global settings
		// stored in machine.config
		private void GlobalDataChanged(object sender, System.EventArgs e)
		{
			try
			{
				// Update the settings in machine.config
				using (DebugProbeConfigFile config = new DebugProbeConfigFile(RuntimeEnvironment.SystemConfigurationFile, false))
				{
					foreach (Probe p in Probe.GlobalSettingsList)
					{
						if (p.ConfigName == "CDP.AllowDebugProbes")
							config.SetAttribute(p.ConfigName, probesAllowed.ToString().ToLower(CultureInfo.InvariantCulture));
						else
							config.SetAttribute(p.ConfigName, p.CheckBox.Checked.ToString().ToLower(CultureInfo.InvariantCulture));
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("There was an error writing to \"" + RuntimeEnvironment.SystemConfigurationFile + 
					"\": " + ex.Message, "Error Writing to Configuration File", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// Called when user activity triggers a change in probe settings
		// stored in each application's config file
		private void ProbeDataChanged(object sender, System.EventArgs e)
		{
			UpdateMarshalingState();

			string filename = null;

			// Update the .config file for each application in the list
			foreach (ListViewItem item in programList.Items)
			{
				filename = item.Tag + ".config";
				UpdateApplicationProbeSettings(filename);
			}

			UpdateSummary();
		}

		// Shows balloon tooltip and/or writes to file if the incoming debug message is relevent
		private void ProcessDebugMessage(object sender, DebugMessageEventArgs e)
		{
			string rawMessage = e.Message;
			int pid = e.ProcessID;

			// Only process messages from the debug probes
			if (!rawMessage.StartsWith(probePrefix))
				return;

			// Get the process name from its ID
			string processName = null;
			try
			{
				processName = Process.GetProcessById(pid).ProcessName + ".exe";
			}
			catch
			{
				// Sometimes the process has already exited by the time we're
				// processing the message.  We could have kept track of which
				// process name was associated with this PID before, but this is simpler.
				processName = "<Exited Process>";
			}

			// Look for a specific message that appears whenever a process first loads the CLR and
			// when probes are enabled.
			if (rawMessage.StartsWith(probePrefix + "CDP.AllowDebugProbes = true"))
			{
				// Show an initial message for each new process being monitored,
				// or each new process that could have been monitored (since we get these
				// messages for all managed apps when probes are allowed in machine.config).
				if (Options.ShowBalloons && processName != "<Exited Process>")
				{
					bool foundProgram = false;

					// See if the process name appears in the list.
					// This doesn't distinguish between multiple programs with the
					// same name running from different directories.
					foreach (ListViewItem item in programList.Items)
					{
						if (String.Compare(processName, item.Text, true, CultureInfo.CurrentCulture) == 0)
						{
							foundProgram = true;
							break;
						}
					}
					
					if (foundProgram)
						notifyIcon.ShowBalloon(summaryText.Substring(0, summaryText.IndexOf("abled") + 5) + ".  This application will run with these settings until it is closed.", "CLR SPY is monitoring " + processName + " (PID " + pid + ")", BalloonIcon.Info);
					else
						notifyIcon.ShowBalloon("This application is not in CLR SPY's list.  If you want to monitor it, add it to the list then restart the application.", "CLR SPY is not monitoring " + processName + " (PID " + pid + ")", BalloonIcon.Info);
				}
				return;
			}
			else if (rawMessage.StartsWith(probePrefix + "CDP.") || rawMessage.IndexOf(": Probe enabled.") > 0)
			{
				// This is one of the initial probe messages that we don't want to report.
				return;
			}

			// At this point, we've got a probe message that we'd like to report

			string message = rawMessage;
			string title = "Unexpected message from " + processName + " (PID " + pid + ")";
			BalloonIcon icon = BalloonIcon.None;

			// Strip the prefix off of the message
			if (rawMessage.IndexOf(":") >= 0 && rawMessage.Length > rawMessage.IndexOf(":") + 2)
			{
				message = rawMessage.Substring(rawMessage.IndexOf(":") + 2);
			}

			// Strip off the extra \n that probe message currently have
			if (message.EndsWith("\n")) message = message.Substring(0, message.Length - 1);

			// Figure out the appropriate icon and display name for the
			// probe reporting this message
			if (rawMessage.StartsWith(infoPrefix))
			{
				// Messages from both the warning and info probes start with the same prefix
				foreach (Probe p in Probe.ProbeList)
				{
					if (p.Type == ProbeType.Warning && rawMessage.StartsWith(infoPrefix + p.ConfigName + ":"))
					{
						// WARNING
						title = p.DisplayName + " in " + processName + " (PID " + pid + ")";
						icon = BalloonIcon.Warning;
						break;
					}
					else if (p.Type == ProbeType.Info && rawMessage.StartsWith(infoPrefix + p.ConfigName + ":"))
					{
						// INFO
						title = p.DisplayName + " in " + processName + " (PID " + pid + ")";
						icon = BalloonIcon.Info;
						break;
					}
				}
			}
			else if (rawMessage.StartsWith(errorPrefix))
			{
				// Messages from the error probes have a unique prefix
				foreach (Probe p in Probe.ProbeList)
				{
					if ((p.Type == ProbeType.Error || p.Type == ProbeType.Failure) && rawMessage.StartsWith(errorPrefix + p.ConfigName + ":"))
					{
						// ERROR
						title = p.DisplayName + " in " + processName + " (PID " + pid + ")";
						icon = BalloonIcon.Error;
						break;
					}
				}
			}
			else
			{
				// This was an unexpected message
				icon = BalloonIcon.Error;
			}

			// Output the message
			if (Options.ShowBalloons) notifyIcon.ShowBalloon(message, title, icon);
			if (Options.LogToFile)
			{
				string logFileMessage = "[" + DateTime.Now + "] " + title + ": " + message;
				try
				{
					using (StreamWriter sw = File.AppendText(Options.LogFile))
					{
						sw.WriteLine(logFileMessage);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error writing message to log file \"" + Options.LogFile + "\": " + ex.Message + Environment.NewLine + Environment.NewLine + "The message is: " + logFileMessage);
				}
			}
		}

		// Work that needs to be done right before exiting the application
		private void MainForm_Closed(object sender, System.EventArgs e)
		{
			this.notifyIcon.Visible = false;

			try
			{
				// Disable probes in machine.config, to be extra safe
				using (DebugProbeConfigFile config = new DebugProbeConfigFile(RuntimeEnvironment.SystemConfigurationFile, false))
				{
					config.SetAttribute("CDP.AllowDebugProbes", "false");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("There was an error writing to \"" + RuntimeEnvironment.SystemConfigurationFile + 
					"\": " + ex.Message + "  Debug probes may still be enabled for some applications after exiting CLR SPY.",
					"Error Writing to Configuration File", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			// Clear probe settings for all applications in the list
			foreach (ListViewItem lvi in programList.Items)
			{
				try
				{
					DebugProbeConfigFile.EraseAllProbeSettings(lvi.Tag.ToString() + ".config", true);
				}
				catch (Exception ex)
				{
					MessageBox.Show("There was an error writing to \"" + lvi.Tag + 
						".config\": " + ex.Message + "  If the file exists, probes may still be enabled for " + lvi.Text + ".",
						"Error Writing to Configuration File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			try
			{
				// Save settings to the CLR SPY configuration file
				using (ConfigFile config = new ConfigFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, typeof(MainForm).Assembly.GetName().Name + ".exe.config"), true))
				{
					// Save the probe settings
					foreach (Probe p in Probe.GlobalSettingsList)
					{
						if (p.CheckBox != null) 
							config.SetAppSetting(p.ConfigName, p.CheckBox.Checked.ToString().ToLower(CultureInfo.InvariantCulture));
					}
					foreach (Probe p in Probe.ProbeList)
					{
						if (p.CheckBox != null) 
							config.SetAppSetting(p.ConfigName, p.CheckBox.Checked.ToString().ToLower(CultureInfo.InvariantCulture));
						else if (p.ConfigName == "CDP.Marshaling.Filter")
							config.SetAppSetting(p.ConfigName, filterText.Text);
					}

					// Also remember which marshaling radio button is selected
					config.SetAppSetting("CLRSPY.NoFilter", radioEverything.Checked.ToString().ToLower(CultureInfo.InvariantCulture));

					// Save the current options
					config.SetAppSetting("CLRSPY.ShowBalloons", Options.ShowBalloons.ToString().ToLower(CultureInfo.InvariantCulture));
					config.SetAppSetting("CLRSPY.LogToFile", Options.LogToFile.ToString().ToLower(CultureInfo.InvariantCulture));
					config.SetAppSetting("CLRSPY.LogFile", Options.LogFile);

					// Save the program list
					StringBuilder sb = new StringBuilder();
					for (int i = 0; i < programList.Items.Count; i++)
					{
						sb.Append(programList.Items[i].Tag);
						if (i != programList.Items.Count-1) sb.Append("|");
					}
					config.SetAppSetting("CLRSPY.ProgramList", sb.ToString());
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("There was an error writing to \"" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, typeof(MainForm).Assembly.GetName().Name + ".exe.config") + 
					"\": " + ex.Message + "  Therefore, your current settings will not be saved for the next time you run CLR SPY.",
					"Error Writing to Configuration File", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// Intercept closing the application and just "hide" it in the notification
		// area, unless the user has explicitly chosen to close it or the user's session is ending
		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!closeApplication && !sessionEnding)
			{
				if (this.Visible) menu_ShowOrHide_Click(this, null);

				if (Options.ShowBalloons)
				{
					if (probesAllowed)
						notifyIcon.ShowBalloon("CLR SPY will continue to run so you can receive notifications.  Right-click the icon and select \"Exit\" to close the program.", "CLR SPY is still running!", BalloonIcon.Info);
					else
						notifyIcon.ShowBalloon("CLR SPY will continue to run, even though all probes are currently disabled.  Right-click the icon and select \"Exit\" to close the program.", "CLR SPY is still running!", BalloonIcon.Info);
				}

				e.Cancel = true;
			}
		}

		// Force redrawing when form resizes
		private void MainForm_Resize(object sender, System.EventArgs e)
		{
			topPanel.Invalidate();
			sidePanel.Invalidate();
		}

		// Show the About dialog
		private void aboutButton_Click(object sender, System.EventArgs e)
		{
			AboutForm af = new AboutForm();
			af.ShowDialog();
		}

		// Show the Options dialog
		private void optionsButton_Click(object sender, System.EventArgs e)
		{
			Options o = new Options();
			o.ShowDialog();
		}

		// Toggle the probing mode
		private void probeButton_Click(object sender, System.EventArgs e)
		{
			if (probesAllowed)
			{
				foreach (Control c in this.Controls)
				{
					if (c as Panel == null) c.Enabled = false;
				}
				probeButton.Text = "Resume Probing";
			}
			else
			{
				foreach (Control c in this.Controls)
				{
					c.Enabled = true;
				}
				probeButton.Text = "Stop Probing";

				UpdateMarshalingState();
			}

			menu_ProbeState.Text = probeButton.Text;
			probesAllowed = !probesAllowed;

			UpdateSummary();
			ChangeIcon(!probesAllowed);

			GlobalDataChanged(this, null);
		}

		// Show the file dialog, and add the application(s) to the list, if appropriate.
		// This also configures the application by writing to its .config file.
		private void addButton_Click(object sender, System.EventArgs e)
		{
			openFileDialog.Title = "Choose an Application";
			openFileDialog.Filter = "Executable Files (*.exe)|*.exe";
			openFileDialog.Multiselect = true;
			
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				foreach (string filename in openFileDialog.FileNames)
				{
					AddFileToProgramList(filename);
				}
			}

			UpdateSummary();
		}

		// Remove the selected application(s) from the list.
		// This also configures the application by writing to its .config file.
		private void removeButton_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem lvi in programList.SelectedItems)
			{
				// Remove the app from the list
				programList.Items.Remove(lvi);

				// Clear the removed application's probe settings
				try
				{
					DebugProbeConfigFile.EraseAllProbeSettings(lvi.Tag.ToString() + ".config", true);
				}
				catch (Exception ex)
				{
					MessageBox.Show("There was an error writing to \"" + lvi.Tag + 
						".config\": " + ex.Message + "  If the file exists, probes may still be enabled for " + lvi.Text + ".",
						"Error Writing to Configuration File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			UpdateSummary();
		}

		// Toggle "edit mode" for the Marshaling filter
		private void editFilterButton_Click(object sender, System.EventArgs e)
		{
			if (editFilterButton.Text == "Edit")
			{
				filterText.Visible = false;
				filterTextBox.Text = filterText.Text;
				filterTextBox.Visible = true;
				editFilterButton.Text = "Save";
				filterTextBox.Focus();
			}
			else
			{
				filterText.Text = filterTextBox.Text;
				ProbeDataChanged(this, null);

				filterTextBox.Visible = false;
				filterText.Visible = true;
				editFilterButton.Text = "Edit";
			}
		}

		// The notify icon context menu handler for exiting the application
		private void menu_Exit_Click(object sender, System.EventArgs e)
		{
			closeApplication = true;
			this.Close();
		}

		// The notify icon context menu handler for enabling/disabling probing
		private void menu_ProbeState_Click(object sender, System.EventArgs e)
		{
			probeButton_Click(sender, e);
		}

		// The notify icon context menu handler for showing/hiding the main form
		private void menu_ShowOrHide_Click(object sender, System.EventArgs e)
		{
			if (this.Visible)
			{
				this.Visible = false;
				menu_ShowOrHide.Text = "Show";
			}
			else
			{
				this.Visible = true;
				menu_ShowOrHide.Text = "Hide";
			}
		}

		// Double-clicking the notify icon acts like selecting Show/Hide
		private void notifyIcon1_DoubleClick(object sender, System.EventArgs e)
		{
			menu_ShowOrHide_Click(sender, e);
		}

		// Determine whether removeButton should be enabled or disabled
		private void programList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (programList.SelectedIndices.Count > 0)
				removeButton.Enabled = true;
			else
				removeButton.Enabled = false;
		}

		// Paint the side panel with a vertical gradient
		private void sidePanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Rectangle r = new Rectangle(0, 0, sidePanel.Width, sidePanel.Height);
			using (Brush b = new LinearGradientBrush(r, SystemColors.ActiveCaption, SystemColors.Control, LinearGradientMode.Vertical))
			{
				e.Graphics.FillRectangle(b, r);
			}
		}

		// Paint the top panel with a horizontal gradient
		private void topPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Rectangle r = new Rectangle(logo.Width - 1, 0, topPanel.Width - logo.Width, topPanel.Height);
			using (Brush b = new LinearGradientBrush(r, Color.White, SystemColors.Control, LinearGradientMode.Horizontal))
			{
				e.Graphics.FillRectangle(b, r);
			}
			e.Graphics.DrawString(summaryText, this.Font, Brushes.Black, logo.Right + 5, probeButton.Top + 5);
		}
		#endregion

		#region Interop definitions
		private const int FILE_ATTRIBUTE_NORMAL = 0x80;
		private const int SHGFI_ICON = 0x100;
		private const int SHGFI_SMALLICON = 0x1;
		private const int SHGFI_USEFILEATTRIBUTES = 0x10;
		private const int WM_ENDSESSION = 0x11;
		private const int WM_QUERYENDSESSION = 0x16;

		[DllImport("shell32.dll", CharSet=CharSet.Auto)]
		private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, int uFlags);

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public int dwAttributes;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=256)]
			public char [] szDisplayName;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=80)]
			public char [] szTypeName;
		}
		#endregion
	}
}