// Options.cs
// Author: Adam Nathan
// Date: 5/11/2003
// For more information, see http://blogs.gotdotnet.com/anathan
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace ClrSpy
{
	/// <summary>
	/// Summary description for Options.
	/// </summary>
	public class Options : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button OKButton;
		private System.Windows.Forms.CheckBox checkLogToFile;
		private System.Windows.Forms.CheckBox checkShowBalloons;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public static bool ShowBalloons = true;
		public static bool LogToFile = false;
		private System.Windows.Forms.TextBox textLogFile;
		public static string LogFile = Path.Combine(Directory.GetDirectoryRoot(Environment.SystemDirectory), "clrspy.log");

		public Options()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			checkShowBalloons.Checked = Options.ShowBalloons;
			checkLogToFile.Checked = Options.LogToFile;
			textLogFile.Text = Options.LogFile;

			// Balloon tooltips are only supported on Windows XP or greater
			if (Environment.OSVersion.Platform != PlatformID.Win32NT ||
				Environment.OSVersion.Version < new Version(5, 1))
			{
				checkShowBalloons.Enabled = false;
			}
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OKButton = new System.Windows.Forms.Button();
			this.checkLogToFile = new System.Windows.Forms.CheckBox();
			this.textLogFile = new System.Windows.Forms.TextBox();
			this.checkShowBalloons = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// OKButton
			// 
			this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OKButton.Location = new System.Drawing.Point(152, 88);
			this.OKButton.Name = "OKButton";
			this.OKButton.TabIndex = 0;
			this.OKButton.Text = "OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// checkLogToFile
			// 
			this.checkLogToFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkLogToFile.Location = new System.Drawing.Point(8, 32);
			this.checkLogToFile.Name = "checkLogToFile";
			this.checkLogToFile.Size = new System.Drawing.Size(160, 24);
			this.checkLogToFile.TabIndex = 1;
			this.checkLogToFile.Text = "Log messages to a file:";
			this.checkLogToFile.CheckedChanged += new System.EventHandler(this.checkLogToFile_CheckedChanged);
			// 
			// textLogFile
			// 
			this.textLogFile.Location = new System.Drawing.Point(24, 54);
			this.textLogFile.Name = "textLogFile";
			this.textLogFile.Size = new System.Drawing.Size(344, 20);
			this.textLogFile.TabIndex = 2;
			this.textLogFile.Text = "";
			this.textLogFile.TextChanged += new System.EventHandler(this.textLogFile_TextChanged);
			// 
			// checkShowBalloons
			// 
			this.checkShowBalloons.Checked = true;
			this.checkShowBalloons.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkShowBalloons.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkShowBalloons.Location = new System.Drawing.Point(8, 8);
			this.checkShowBalloons.Name = "checkShowBalloons";
			this.checkShowBalloons.Size = new System.Drawing.Size(288, 24);
			this.checkShowBalloons.TabIndex = 3;
			this.checkShowBalloons.Text = "Display messages as balloon tooltips";
			this.checkShowBalloons.CheckedChanged += new System.EventHandler(this.checkShowBalloons_CheckedChanged);
			// 
			// Options
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(378, 120);
			this.Controls.Add(this.textLogFile);
			this.Controls.Add(this.checkShowBalloons);
			this.Controls.Add(this.checkLogToFile);
			this.Controls.Add(this.OKButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Options";
			this.Text = "Options";
			this.ResumeLayout(false);

		}
		#endregion

		private void OKButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void checkShowBalloons_CheckedChanged(object sender, System.EventArgs e)
		{
			Options.ShowBalloons = checkShowBalloons.Checked;
		}

		private void checkLogToFile_CheckedChanged(object sender, System.EventArgs e)
		{
			Options.LogToFile = checkLogToFile.Checked;
		}

		private void textLogFile_TextChanged(object sender, System.EventArgs e)
		{
			Options.LogFile = textLogFile.Text;
		}
	}
}
