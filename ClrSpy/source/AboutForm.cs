// AboutForm.cs
// Author: Adam Nathan
// Date: 5/11/2003
// For more information, see http://blogs.gotdotnet.com/anathan
using System.Runtime.InteropServices;

namespace ClrSpy
{
	/// <summary>
	/// The About box form.
	/// </summary>
	public class AboutForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox logo;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label clrVersionLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AboutForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			versionLabel.Text = "Version " + typeof(AboutForm).Assembly.GetName().Version;
			clrVersionLabel.Text = "Currently running on .NET Framework " + RuntimeEnvironment.GetSystemVersion() + ".";
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AboutForm));
			this.logo = new System.Windows.Forms.PictureBox();
			this.versionLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.clrVersionLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// logo
			// 
			this.logo.BackColor = System.Drawing.Color.White;
			this.logo.Image = ((System.Drawing.Image)(resources.GetObject("logo.Image")));
			this.logo.Location = new System.Drawing.Point(8, 8);
			this.logo.Name = "logo";
			this.logo.Size = new System.Drawing.Size(97, 79);
			this.logo.TabIndex = 16;
			this.logo.TabStop = false;
			// 
			// versionLabel
			// 
			this.versionLabel.AutoSize = true;
			this.versionLabel.BackColor = System.Drawing.Color.Transparent;
			this.versionLabel.Location = new System.Drawing.Point(120, 24);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(43, 16);
			this.versionLabel.TabIndex = 17;
			this.versionLabel.Text = "Version";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Location = new System.Drawing.Point(120, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(88, 16);
			this.label2.TabIndex = 18;
			this.label2.Text = "by Adam Nathan";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Location = new System.Drawing.Point(8, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(269, 16);
			this.label1.TabIndex = 19;
			this.label1.Text = "Requires version 1.1 or later of the .NET Framework.";
			// 
			// clrVersionLabel
			// 
			this.clrVersionLabel.AutoSize = true;
			this.clrVersionLabel.BackColor = System.Drawing.Color.Transparent;
			this.clrVersionLabel.Location = new System.Drawing.Point(8, 120);
			this.clrVersionLabel.Name = "clrVersionLabel";
			this.clrVersionLabel.Size = new System.Drawing.Size(195, 16);
			this.clrVersionLabel.TabIndex = 20;
			this.clrVersionLabel.Text = "Currently running on .NET Framework";
			// 
			// AboutForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(274, 144);
			this.Controls.Add(this.clrVersionLabel);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.versionLabel);
			this.Controls.Add(this.logo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About CLR SPY";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
