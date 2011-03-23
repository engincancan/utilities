#region Using directives

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;


#endregion

namespace Willeke.Scott.LessMSIerables
{
	class Program
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern int FreeConsole();


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			try
			{
				RegistryTool.RegisterShortcutmenu();
				// Handle args:
				for (int i = 0; i < args.Length; i++)
				{
					if (args.Length < 1)
						continue;

					if (args[i][0] != '/' && args[i][0] != '-')
						continue;
					switch (args[i][1])
					{
						case 'x':
						{
							if (++i >= args.Length)
								return LaunchForm("");
			
							string sMsi = args[i];

							EnsureFileRooted(ref sMsi);
							if (++i >= args.Length)
								return LaunchForm(sMsi);

							string sOutDir = args[i];
							EnsureFileRooted(ref sOutDir);

							FileInfo msiFile = new FileInfo(sMsi);
							DirectoryInfo outDir = new DirectoryInfo(sOutDir);

							Trace.WriteLine("x: Extracting \'" + msiFile + "\' to \'" + outDir + "\'.");
							Wixtracts.ExtractFiles(msiFile, outDir);
							return 0;
						}
					}
				}
			}
			catch (Exception eCatchAll)
			{
				Trace.WriteLine("Error: " + eCatchAll.ToString());
				return -1;
			}
			return LaunchForm("");
		}

		private static void EnsureFileRooted(ref string sFileName)
		{
			if (!Path.IsPathRooted(sFileName))
				sFileName = Path.Combine(Directory.GetCurrentDirectory(), sFileName);
		}

		static int LaunchForm(string inputFile)
		{
			MainForm form = new MainForm(inputFile);
			FreeConsole();
			Application.EnableVisualStyles();
			Application.Run(form);
			return 0;
		}
	}
}