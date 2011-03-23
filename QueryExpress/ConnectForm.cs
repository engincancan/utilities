using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace QueryExpress
{
	/// <summary>
	/// Connection Form - this requests connection details from the user and then creates
	///  and configures DbClient and Browser objects, which can be obtained through
	///  properties of the form.
	/// </summary>
	public class ConnectForm : System.Windows.Forms.Form
	{
		#region User Fields
		// Save the config file location in a static field so we can always refer to the same file
		static string configFile = "";
		DbClient dbClient = null;
		IBrowser browser = null;
		#endregion

		#region Designer Fields
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtSqlServer;
		private System.Windows.Forms.TextBox txtSqlLoginName;
		private System.Windows.Forms.Label lblSqlPassword;
		private System.Windows.Forms.Label lblSqlLoginName;
		private System.Windows.Forms.TextBox txtSqlPassword;
		private System.Windows.Forms.RadioButton rbSqlUntrusted;
		private System.Windows.Forms.RadioButton rbSqlTrusted;
		private System.ComponentModel.Container components = null;
		private QueryExpress.DSSettings dsSettings;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtOleConnectString;
		private System.Windows.Forms.TabPage pagSQL;
		private System.Windows.Forms.TabPage pagOracle;
		private System.Windows.Forms.TabPage pagOleDb;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox txtOraPassword;
		private System.Windows.Forms.RadioButton rbOraNative;
		private System.Windows.Forms.RadioButton rbOraMSDriver;
		private System.Windows.Forms.TextBox txtOraDataSource;
		private System.Windows.Forms.TextBox txtOraLoginName;
		private System.Windows.Forms.Button btnLoadOleDb;
		private System.Windows.Forms.Button btnSaveOleDb;
		private System.Windows.Forms.SaveFileDialog saveOleDbFileDialog;
		private System.Windows.Forms.OpenFileDialog openOleDbFileDialog;
		private System.Windows.Forms.CheckBox chkLowBandwidth;
		#endregion

		#region Public Properties

		/// <summary>
		/// The database client object which is used to talk to the database server.
		/// This should be queried after the form is closed (following a DialogResult.OK)
		/// </summary>
		public DbClient DbClient
		{
			get {return dbClient;}
		}

		/// <summary>
		/// The database browser object which is used in producing a TreeView of objects.
		/// This should be queried after the form is closed (following a DialogResult.OK)
		/// This property will be null if no browser is available for the database provider.
		/// </summary>
		public IBrowser Browser
		{
			get {return browser;}
		}

		public bool LowBandwidth
		{
			get {return chkLowBandwidth.Checked;}
		}

		#endregion

		#region Constructor
		public ConnectForm()
		{
			InitializeComponent();
			Icon = null;

			if (configFile == "")
				configFile = Path.Combine (Directory.GetCurrentDirectory(), "QESettings.xml");

			// Read in connection settings, ignore any errors.
			try { dsSettings.ReadXml (configFile); } 
			catch (Exception) {}
			if (dsSettings.settings.Rows.Count < 1)
				dsSettings.settings.AddsettingsRow (0, "(local)", "master", true, "", "", "", "", false, false, false);
			try 
			{
				tabControl.SelectedIndex = dsSettings.settings [0].ConnectionPage;
				rbSqlTrusted.Checked = dsSettings.settings [0].SqlTrusted;
				rbSqlUntrusted.Checked = !rbSqlTrusted.Checked;
				rbOraMSDriver.Checked = dsSettings.settings [0].OraMSDriver;
				rbOraNative.Checked = !rbOraMSDriver.Checked;
			}
			catch (Exception) {}
			EnableControls();
		}
		#endregion

		#region Misc Methods
		// Enable / disable controls appropriate to UI selections
		private void EnableControls()
		{
			txtSqlLoginName.Enabled = txtSqlPassword.Enabled = rbSqlUntrusted.Checked;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
			this.pagOracle = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.rbOraMSDriver = new System.Windows.Forms.RadioButton();
			this.rbOraNative = new System.Windows.Forms.RadioButton();
			this.txtOraLoginName = new System.Windows.Forms.TextBox();
			this.dsSettings = new QueryExpress.DSSettings();
			this.txtOraPassword = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.txtOraDataSource = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.chkLowBandwidth = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.pagSQL = new System.Windows.Forms.TabPage();
			this.txtSqlLoginName = new System.Windows.Forms.TextBox();
			this.lblSqlPassword = new System.Windows.Forms.Label();
			this.lblSqlLoginName = new System.Windows.Forms.Label();
			this.txtSqlPassword = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rbSqlUntrusted = new System.Windows.Forms.RadioButton();
			this.rbSqlTrusted = new System.Windows.Forms.RadioButton();
			this.txtSqlServer = new System.Windows.Forms.TextBox();
			this.pagOleDb = new System.Windows.Forms.TabPage();
			this.btnLoadOleDb = new System.Windows.Forms.Button();
			this.txtOleConnectString = new System.Windows.Forms.TextBox();
			this.btnSaveOleDb = new System.Windows.Forms.Button();
			this.btnConnect = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.saveOleDbFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.openOleDbFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.pagOracle.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dsSettings)).BeginInit();
			this.pagSQL.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.pagOleDb.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// pagOracle
			// 
			this.pagOracle.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.groupBox2,
																					this.txtOraLoginName,
																					this.txtOraPassword,
																					this.label6,
																					this.label7,
																					this.txtOraDataSource,
																					this.label4});
			this.pagOracle.Location = new System.Drawing.Point(4, 22);
			this.pagOracle.Name = "pagOracle";
			this.pagOracle.Size = new System.Drawing.Size(311, 196);
			this.pagOracle.TabIndex = 1;
			this.pagOracle.Text = "Oracle";
			this.pagOracle.Enter += new System.EventHandler(this.pagOracle_Enter);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.rbOraMSDriver,
																					this.rbOraNative});
			this.groupBox2.Location = new System.Drawing.Point(14, 104);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(233, 67);
			this.groupBox2.TabIndex = 8;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Driver:";
			// 
			// rbOraMSDriver
			// 
			this.rbOraMSDriver.Location = new System.Drawing.Point(20, 39);
			this.rbOraMSDriver.Name = "rbOraMSDriver";
			this.rbOraMSDriver.Size = new System.Drawing.Size(87, 20);
			this.rbOraMSDriver.TabIndex = 1;
			this.rbOraMSDriver.Text = "&Microsoft";
			// 
			// rbOraNative
			// 
			this.rbOraNative.Checked = true;
			this.rbOraNative.Location = new System.Drawing.Point(20, 19);
			this.rbOraNative.Name = "rbOraNative";
			this.rbOraNative.Size = new System.Drawing.Size(170, 17);
			this.rbOraNative.TabIndex = 0;
			this.rbOraNative.TabStop = true;
			this.rbOraNative.Text = "&Oracle (requires 8i client)";
			// 
			// txtOraLoginName
			// 
			this.txtOraLoginName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dsSettings, "settings.OraLoginName"));
			this.txtOraLoginName.Location = new System.Drawing.Point(93, 43);
			this.txtOraLoginName.Name = "txtOraLoginName";
			this.txtOraLoginName.Size = new System.Drawing.Size(154, 20);
			this.txtOraLoginName.TabIndex = 5;
			this.txtOraLoginName.Text = "";
			// 
			// dsSettings
			// 
			this.dsSettings.DataSetName = "DSSettings";
			this.dsSettings.Locale = new System.Globalization.CultureInfo("en-US");
			this.dsSettings.Namespace = "http://tempuri.org/DSSettings.xsd";
			// 
			// txtOraPassword
			// 
			this.txtOraPassword.Location = new System.Drawing.Point(93, 71);
			this.txtOraPassword.Name = "txtOraPassword";
			this.txtOraPassword.PasswordChar = '*';
			this.txtOraPassword.Size = new System.Drawing.Size(154, 20);
			this.txtOraPassword.TabIndex = 7;
			this.txtOraPassword.Text = "";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(14, 44);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(68, 13);
			this.label6.TabIndex = 4;
			this.label6.Text = "&Login Name:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(14, 71);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(57, 13);
			this.label7.TabIndex = 6;
			this.label7.Text = "&Password:";
			// 
			// txtOraDataSource
			// 
			this.txtOraDataSource.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dsSettings, "settings.OraDataSource"));
			this.txtOraDataSource.Location = new System.Drawing.Point(93, 17);
			this.txtOraDataSource.Name = "txtOraDataSource";
			this.txtOraDataSource.Size = new System.Drawing.Size(154, 20);
			this.txtOraDataSource.TabIndex = 1;
			this.txtOraDataSource.Text = "";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(14, 19);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(67, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "&Data Source";
			// 
			// chkLowBandwidth
			// 
			this.chkLowBandwidth.Location = new System.Drawing.Point(15, 244);
			this.chkLowBandwidth.Name = "chkLowBandwidth";
			this.chkLowBandwidth.Size = new System.Drawing.Size(125, 18);
			this.chkLowBandwidth.TabIndex = 1;
			this.chkLowBandwidth.Text = "L&ow bandwidth";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(14, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(41, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Server:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(10, 10);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(127, 15);
			this.label3.TabIndex = 0;
			this.label3.Text = "Connection String";
			// 
			// pagSQL
			// 
			this.pagSQL.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.txtSqlLoginName,
																				 this.lblSqlPassword,
																				 this.lblSqlLoginName,
																				 this.txtSqlPassword,
																				 this.groupBox1,
																				 this.txtSqlServer,
																				 this.label1});
			this.pagSQL.Location = new System.Drawing.Point(4, 22);
			this.pagSQL.Name = "pagSQL";
			this.pagSQL.Size = new System.Drawing.Size(311, 196);
			this.pagSQL.TabIndex = 0;
			this.pagSQL.Text = "SQL Server";
			this.pagSQL.Enter += new System.EventHandler(this.pagSQL_Enter);
			// 
			// txtSqlLoginName
			// 
			this.txtSqlLoginName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dsSettings, "settings.SqlLoginName"));
			this.txtSqlLoginName.Location = new System.Drawing.Point(125, 129);
			this.txtSqlLoginName.Name = "txtSqlLoginName";
			this.txtSqlLoginName.Size = new System.Drawing.Size(160, 20);
			this.txtSqlLoginName.TabIndex = 6;
			this.txtSqlLoginName.Text = "";
			// 
			// lblSqlPassword
			// 
			this.lblSqlPassword.AutoSize = true;
			this.lblSqlPassword.Location = new System.Drawing.Point(49, 155);
			this.lblSqlPassword.Name = "lblSqlPassword";
			this.lblSqlPassword.Size = new System.Drawing.Size(57, 13);
			this.lblSqlPassword.TabIndex = 7;
			this.lblSqlPassword.Text = "&Password:";
			// 
			// lblSqlLoginName
			// 
			this.lblSqlLoginName.AutoSize = true;
			this.lblSqlLoginName.Location = new System.Drawing.Point(49, 129);
			this.lblSqlLoginName.Name = "lblSqlLoginName";
			this.lblSqlLoginName.Size = new System.Drawing.Size(66, 13);
			this.lblSqlLoginName.TabIndex = 5;
			this.lblSqlLoginName.Text = "&Login name:";
			// 
			// txtSqlPassword
			// 
			this.txtSqlPassword.Location = new System.Drawing.Point(125, 155);
			this.txtSqlPassword.Name = "txtSqlPassword";
			this.txtSqlPassword.PasswordChar = '*';
			this.txtSqlPassword.Size = new System.Drawing.Size(160, 20);
			this.txtSqlPassword.TabIndex = 8;
			this.txtSqlPassword.Text = "";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.rbSqlUntrusted,
																					this.rbSqlTrusted});
			this.groupBox1.Location = new System.Drawing.Point(14, 47);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(286, 140);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Connect Using:";
			// 
			// rbSqlUntrusted
			// 
			this.rbSqlUntrusted.Location = new System.Drawing.Point(18, 46);
			this.rbSqlUntrusted.Name = "rbSqlUntrusted";
			this.rbSqlUntrusted.Size = new System.Drawing.Size(194, 18);
			this.rbSqlUntrusted.TabIndex = 1;
			this.rbSqlUntrusted.Text = "S&QL Server Authentication";
			this.rbSqlUntrusted.Click += new System.EventHandler(this.rbSqlUntrusted_Click);
			this.rbSqlUntrusted.CheckedChanged += new System.EventHandler(this.rbSql_Changed);
			// 
			// rbSqlTrusted
			// 
			this.rbSqlTrusted.Checked = true;
			this.rbSqlTrusted.Location = new System.Drawing.Point(18, 23);
			this.rbSqlTrusted.Name = "rbSqlTrusted";
			this.rbSqlTrusted.Size = new System.Drawing.Size(190, 17);
			this.rbSqlTrusted.TabIndex = 0;
			this.rbSqlTrusted.TabStop = true;
			this.rbSqlTrusted.Text = "&Windows Authentication";
			this.rbSqlTrusted.CheckedChanged += new System.EventHandler(this.rbSql_Changed);
			// 
			// txtSqlServer
			// 
			this.txtSqlServer.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dsSettings, "settings.SqlServer"));
			this.txtSqlServer.Location = new System.Drawing.Point(73, 14);
			this.txtSqlServer.Name = "txtSqlServer";
			this.txtSqlServer.Size = new System.Drawing.Size(161, 20);
			this.txtSqlServer.TabIndex = 1;
			this.txtSqlServer.Text = "";
			// 
			// pagOleDb
			// 
			this.pagOleDb.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.btnLoadOleDb,
																				   this.txtOleConnectString,
																				   this.label3,
																				   this.btnSaveOleDb});
			this.pagOleDb.Location = new System.Drawing.Point(4, 22);
			this.pagOleDb.Name = "pagOleDb";
			this.pagOleDb.Size = new System.Drawing.Size(311, 196);
			this.pagOleDb.TabIndex = 2;
			this.pagOleDb.Text = "OLE-DB";
			// 
			// btnLoadOleDb
			// 
			this.btnLoadOleDb.Location = new System.Drawing.Point(12, 165);
			this.btnLoadOleDb.Name = "btnLoadOleDb";
			this.btnLoadOleDb.TabIndex = 2;
			this.btnLoadOleDb.Text = "&Load...";
			this.btnLoadOleDb.Click += new System.EventHandler(this.btnLoadOleDb_Click);
			// 
			// txtOleConnectString
			// 
			this.txtOleConnectString.AutoSize = false;
			this.txtOleConnectString.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dsSettings, "settings.OleDbConnect"));
			this.txtOleConnectString.Location = new System.Drawing.Point(12, 27);
			this.txtOleConnectString.Multiline = true;
			this.txtOleConnectString.Name = "txtOleConnectString";
			this.txtOleConnectString.Size = new System.Drawing.Size(288, 128);
			this.txtOleConnectString.TabIndex = 1;
			this.txtOleConnectString.Text = "";
			// 
			// btnSaveOleDb
			// 
			this.btnSaveOleDb.Location = new System.Drawing.Point(97, 165);
			this.btnSaveOleDb.Name = "btnSaveOleDb";
			this.btnSaveOleDb.TabIndex = 2;
			this.btnSaveOleDb.Text = "&Save...";
			this.btnSaveOleDb.Click += new System.EventHandler(this.btnSaveOleDb_Click);
			// 
			// btnConnect
			// 
			this.btnConnect.Location = new System.Drawing.Point(159, 240);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(79, 23);
			this.btnConnect.TabIndex = 2;
			this.btnConnect.Text = "Connect";
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(246, 240);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(79, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			// 
			// tabControl
			// 
			this.tabControl.Controls.AddRange(new System.Windows.Forms.Control[] {
																					 this.pagSQL,
																					 this.pagOracle,
																					 this.pagOleDb});
			this.tabControl.Location = new System.Drawing.Point(6, 7);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(319, 222);
			this.tabControl.TabIndex = 0;
			this.tabControl.TabStop = false;
			// 
			// saveOleDbFileDialog
			// 
			this.saveOleDbFileDialog.DefaultExt = "connectString";
			this.saveOleDbFileDialog.FileName = "OleDb";
			this.saveOleDbFileDialog.Filter = "Connection String|*.connectString|Text File|*.txt|All Files|*.*";
			// 
			// openOleDbFileDialog
			// 
			this.openOleDbFileDialog.DefaultExt = "connectString";
			this.openOleDbFileDialog.FileName = "OleDb";
			this.openOleDbFileDialog.Filter = "Connection String|*.connectString|Text File|*.txt|All Files|*.*";
			// 
			// ConnectForm
			// 
			this.AcceptButton = this.btnConnect;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(334, 272);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.chkLowBandwidth,
																		  this.btnCancel,
																		  this.btnConnect,
																		  this.tabControl});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConnectForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Connect...";
			this.Closed += new System.EventHandler(this.ConnectForm_Closed);
			this.pagOracle.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dsSettings)).EndInit();
			this.pagSQL.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.pagOleDb.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

		private void btnConnect_Click(object sender, System.EventArgs e)
		{
			// Save contents of controls to XML file.
			// Data binding to radio buttons doesn't seem to work, sadly, so we have to do this bit manually.
			dsSettings.settings [0].SqlTrusted = rbSqlTrusted.Checked;
			dsSettings.settings [0].OraMSDriver = rbOraMSDriver.Checked;
			dsSettings.settings [0].ConnectionPage = tabControl.SelectedIndex;
			try { dsSettings.WriteXml (configFile); } 
			catch (Exception) {}

			// Create DbClient (database client) object

			IClientFactory clientFactory;
			string connectString, connectDescription;

			switch (tabControl.SelectedIndex)
			{
				case 0:								// SQL Server
					clientFactory = new SqlFactory();
					connectString = "Data Source=" + txtSqlServer.Text.Trim() + ";app=Query Express";
					if (rbSqlTrusted.Checked)
						connectString +=  ";Integrated Security=SSPI";
					else
						connectString +=
							";User ID=" + txtSqlLoginName.Text.Trim() +
							";Password=" + txtSqlPassword.Text.Trim();
					connectDescription = txtSqlServer.Text + " (" + 
						(rbSqlTrusted.Checked ? "Trusted" : txtSqlLoginName.Text.Trim()) + ")";
					break;

				case 1:								// Oracle
					// As we don't yet have Oracle .NET classes, use the OleDb family of classes instead.
					clientFactory = new OleDbFactory();
					connectString = "Provider="
						+ ((rbOraMSDriver.Checked) ? "MSDAORA" : "OraOLEDB.Oracle")
						+ ";Data Source=" + txtOraDataSource.Text.Trim()
						+ ";User ID=" + txtOraLoginName.Text.Trim() + ";Password=" + txtOraPassword.Text.Trim();
					connectDescription = txtOraDataSource.Text.Trim();
					break;

				case 2:								// OLE-DB
					clientFactory = new OleDbFactory();
					connectString = txtOleConnectString.Text.Trim();
					connectDescription = "[" + connectString.Substring (0, Math.Min (25, connectString.Length)) + "...]";
					break;

				default:	return;
			}

			dbClient = new DbClient (clientFactory, connectString, connectDescription);
			Cursor oldCursor = Cursor;
			Cursor = Cursors.WaitCursor;
			
			ConnectingForm c = new ConnectingForm();
			c.Show();
			c.Refresh();
			bool success = dbClient.Connect();
			c.Close();
			Cursor = oldCursor;

			if (!success)
			{
				MessageBox.Show ("Unable to connect: " + dbClient.Error, "Query Express", MessageBoxButtons.OK, MessageBoxIcon.Error);
				dbClient.Dispose();
				return;
			}

			// Create a browser object (if available for the provider)

			if (tabControl.SelectedIndex == 0)					// SQL Server
				browser = new SqlBrowser (dbClient);

			if (tabControl.SelectedIndex == 1)					// Oracle
				browser = new OracleBrowser (dbClient);

			if (tabControl.SelectedIndex == 2)					// OleDb
				browser = new dl3bak.OleDbBrowser (dbClient);

			DialogResult = DialogResult.OK;
		}

		private void rbSqlUntrusted_Click(object sender, System.EventArgs e)
		{
			if (rbSqlUntrusted.Checked) txtSqlLoginName.Focus();
		}

		private void ConnectForm_Closed(object sender, System.EventArgs e)
		{
			Dispose();
		}

		private void rbSql_Changed (object sender, System.EventArgs e)
		{
			EnableControls();
		}

		private void pagSQL_Enter(object sender, System.EventArgs e)
		{
			// If everything's there but the password, focus straight to the password textbox.
			if (rbSqlUntrusted.Checked && txtSqlServer.Text.Trim().Length > 0 && txtSqlLoginName.Text.Trim().Length > 0)
				txtSqlPassword.Focus();
		}

		private void pagOracle_Enter(object sender, System.EventArgs e)
		{
			// If everything's there but the password, focus straight to the password textbox.
			if (txtOraDataSource.Text.Trim().Length > 0 && txtOraLoginName.Text.Trim().Length > 0)
				txtOraPassword.Focus();
		}

		private void btnSaveOleDb_Click(object sender, System.EventArgs e)
		{
			if (saveOleDbFileDialog.ShowDialog() == DialogResult.OK)
				FileUtil.WriteToFile (saveOleDbFileDialog.FileName, dsSettings.settings[0].OleDbConnect);
			btnConnect.Focus();
		}

		private void btnLoadOleDb_Click(object sender, System.EventArgs e)
		{
			if (openOleDbFileDialog.ShowDialog() == DialogResult.OK)
			{
				string data;
				FileUtil.ReadFromFile (openOleDbFileDialog.FileName, out data);
				dsSettings.settings[0].OleDbConnect = data;
				DataRow row = dsSettings.settings[0];
				row = dsSettings.settings[0];
			}
			btnConnect.Focus();
		}

		#endregion

	}
}
