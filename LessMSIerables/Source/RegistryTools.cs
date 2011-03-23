using System;
using System.Collections;
//using System.Security.AccessControl;
using System.Text;
using Microsoft.Win32;

namespace Willeke.Scott.LessMSIerables
{
	internal class RegistryTool
	{

		public static void RegisterShortcutmenu()
		{
			RegistryKey extractKey = Registry.ClassesRoot.CreateSubKey(@"Msi.Package\shell\Extract");
			extractKey.SetValue("", "&Extract Files");
			RegistryKey command = extractKey.CreateSubKey("command");
			
			string cmd = command.GetValue("") as string;
			if (cmd != null && cmd.StartsWith(GetExePath()))
				return;
			
			command.SetValue("", '\"' + GetExePath() + "\" /x \"%1\" \"%1_extracted\"");
		}

		static string GetExePath()
		{
			return typeof(RegistryTool).Module.FullyQualifiedName;
		}
	}
}
