using System;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

using ComplexIT.COM;

namespace EvidenceBrowser
{
	/// <summary>
	/// Hybrid of code by jason whittington (http://www.neocranium.com) and henkk de koning (http://www.complexit.com)
	/// The heavy lifting (loading an in-memory xml document into IE) was done by henkk
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
	{
		/// <summary>
		/// Default stylesheet as used in IE. Loaded using the res: protocol from
		/// res://msxml.dll/defaultss.xsl Two reasons to interop to msxml:
		/// 1) the fucker uses unsupported xsl elements like node-name and entity-ref
		/// 2) the res: protocol is easily the easiest way to extract content from
		///    unmanaged resources, but only supported on msxml.
		/// </summary>
		private MSXML2.IXSLTemplate _defaultStylesheet;

		/// <summary>
		/// This guy is used to load a document into IE without navigating
		/// </summary>
		private IPersistStreamInit _documentObject;

		/// <summary>
		/// Command Line supplied assembly Name
		/// </summary>
		public string AssemblyName="";

		private AxSHDocVw.AxWebBrowser _webBrowser;
		private System.Windows.Forms.MainMenu _menuBar;
		private System.Windows.Forms.MenuItem _miFile;
		private System.Windows.Forms.MenuItem _miOpen;
		private System.Windows.Forms.MenuItem _miExit;
		private System.Windows.Forms.OpenFileDialog _ofd;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmMain()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
			this._webBrowser = new AxSHDocVw.AxWebBrowser();
			this._menuBar = new System.Windows.Forms.MainMenu();
			this._miFile = new System.Windows.Forms.MenuItem();
			this._miOpen = new System.Windows.Forms.MenuItem();
			this._miExit = new System.Windows.Forms.MenuItem();
			this._ofd = new System.Windows.Forms.OpenFileDialog();
			((System.ComponentModel.ISupportInitialize)(this._webBrowser)).BeginInit();
			this.SuspendLayout();
			// 
			// _webBrowser
			// 
			this._webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this._webBrowser.Enabled = true;
			this._webBrowser.Location = new System.Drawing.Point(0, 0);
			this._webBrowser.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("_webBrowser.OcxState")));
			this._webBrowser.Size = new System.Drawing.Size(672, 296);
			this._webBrowser.TabIndex = 0;
			// 
			// _menuBar
			// 
			this._menuBar.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this._miFile});
			// 
			// _miFile
			// 
			this._miFile.Index = 0;
			this._miFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					this._miOpen,
																					this._miExit});
			this._miFile.Text = "&File";
			// 
			// _miOpen
			// 
			this._miOpen.Index = 0;
			this._miOpen.Text = "&Open";
			this._miOpen.Click += new System.EventHandler(this._miOpen_Click);
			// 
			// _miExit
			// 
			this._miExit.Index = 1;
			this._miExit.Text = "E&xit";
			this._miExit.Click += new System.EventHandler(this._miExit_Click);
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(672, 294);
			this.Controls.Add(this._webBrowser);
			this.Menu = this._menuBar;
			this.Name = "frmMain";
			this.Text = "Evidence Browser";
			this.Load += new System.EventHandler(this.frmMain_Load);
			((System.ComponentModel.ISupportInitialize)(this._webBrowser)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			// Dominick's addition : Include commandline parameter handling 
			frmMain frm = new frmMain();

			if (args.Length == 1)
				frm.AssemblyName = args[0];	
				

			Application.Run(frm);
		}

		private void frmMain_Load(object sender, System.EventArgs e)
		{
			this._ofd.Filter = "Assemblies |*.dll; *.exe";

			//////////
			// Load the stylesheet IE uses for displaying XML
			//////////
			MSXML2.FreeThreadedDOMDocument _doc = new MSXML2.FreeThreadedDOMDocumentClass();
			_doc.load("res://msxml.dll/defaultss.xsl");
			this._defaultStylesheet = (MSXML2.XSLTemplate)new MSXML2.XSLTemplateClass();
			this._defaultStylesheet.stylesheet = _doc;

			//////////
			// Register a handler to pick up a ref to the doc model
			//////////
			this._webBrowser.NavigateComplete2 
				+= new AxSHDocVw.DWebBrowserEvents2_NavigateComplete2EventHandler(
					StoreIEDocObject);

			//////////
			// Surf to about:blank, to get an instance of the doc model
			//////////
			object _url = "about:blank";
			object _flags = new object();
			object _targetFrameName = new object();
			object _postData = new object();
			object _headers = new object();
			this._webBrowser.Navigate2(
				ref _url, ref _flags, ref _targetFrameName, ref _postData, ref _headers);

			this._documentObject = (IPersistStreamInit)this._webBrowser.Document;

			if (AssemblyName != string.Empty)
				_showEvidence(AssemblyName);
		}
		
		private void StoreIEDocObject(object sender, AxSHDocVw.DWebBrowserEvents2_NavigateComplete2Event e)
		{
			//////////
			// Keep a ref to the doc object
			//////////
			this._documentObject = (IPersistStreamInit)this._webBrowser.Document;

			//////////
			// Tell the user to load xml
			//////////
			//((mshtml.IHTMLDocument2)this._documentObject).body.innerHTML
			//	= "<Font face='arial' color='red' size=4>Please open an xml document</Font>";
		}

		private void _miOpen_Click(object sender, System.EventArgs e)
		{
			//////////
			// Ask for a file name
			//////////
			if (this._ofd.ShowDialog() != DialogResult.OK)
			{
				return;
			}


			_showEvidence(_ofd.FileName);
		}

		private void _showEvidence(string assemblyName)
		{

			try
			{
				//////////
				// Load the request in a dom
				//////////
				MSXML2.FreeThreadedDOMDocument _xmlDoc
					= new MSXML2.FreeThreadedDOMDocumentClass();
				
				// Dominick's Additions (Jason's idea)
				Assembly a = Assembly.LoadFrom(assemblyName);
								
				string evidence = string.Format("<evidence assembly='{0}'>", a.GetName().Name);

				foreach (object o in a.Evidence)
					evidence += o.ToString();

				evidence += "</evidence>";

				_xmlDoc.loadXML(evidence);

				//_xmlDoc.load(this._ofd.FileName);

				//////////
				// Create an xslt processor based on the default xml display stylesheet
				//////////
				MSXML2.IXSLProcessor _xslProcessor = this._defaultStylesheet.createProcessor();
				_xslProcessor.input = _xmlDoc;
 
				//////////
				// Run the transform
				//////////
				_xslProcessor.transform();
 
				//////////
				// Write the result into an IStream implementation
				//////////
				AxMemoryStream _axStream = new AxMemoryStream();
				TextWriter _writer = new StreamWriter(_axStream);
				_writer.Write(_xslProcessor.output.ToString());
				_writer.Flush();
				_axStream.Seek(0, SeekOrigin.Begin);
 
				//////////
				// Use IPersistStreamInit to load the html into the browser
				//////////
				this._documentObject.InitNew();
				this._documentObject.Load(_axStream);
			}
			catch (Exception _ex)
			{
				MessageBox.Show(_ex.ToString());
			}
		}

		private void _miExit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
