using System;
using System.Collections;
using System.Text;
using Microsoft.Tools.WindowsInstallerXml.Msi;
using System.Diagnostics;

namespace Willeke.Scott.LessMSIerables
{
	/// <summary>
	/// Represents a file in the msi file table/view.
	/// </summary>
	public class MsiFile
	{
		public string File;// a unique id for the file
		public string LongFileName;
		public string ShortFileName;
		public int FileSize;
		public string Version;

		private MsiFile()
		{
		}

		/// <summary>
		/// Creates a <see cref="MsiFile"/> from the specified list of values. The specified columns must be in the same order as the values.
		/// </summary>
		public static MsiFile[] CreateMsiFilesFromMSI(Database msidb)
		{
			const string tableName = "File";
			if (!msidb.TableExists(tableName))
			{
				Trace.WriteLine("No Files Found.");
				return new MsiFile[0];
			}

			string query = string.Concat("SELECT * FROM `", tableName, "`");

			using (ViewWrapper view = new ViewWrapper(msidb.OpenExecuteView(query)))
			{
				ArrayList/*<MsiFile>*/ files = new ArrayList(view.Records.Count);

				ColumnInfo[] columns = view.Columns;
				foreach (object[] values in view.Records)
				{
					Hashtable/*<string, int>*/ colIndexes = new Hashtable(columns.Length, System.Collections.CaseInsensitiveHashCodeProvider.DefaultInvariant, System.Collections.CaseInsensitiveComparer.DefaultInvariant);
					for (int cIndex = 0; cIndex < columns.Length; cIndex++)
						colIndexes[((ColumnInfo)columns[cIndex]).Name] = cIndex;

					MsiFile file = new MsiFile();

					string fileName = Convert.ToString(values[(int)colIndexes["FileName"]]);
					string[] split = fileName.Split('|');
					
					file.ShortFileName = split[0];
					if (split.Length > 1)
						file.LongFileName = split[1];
					else
						file.LongFileName = split[0];

					file.File = Convert.ToString(values[(int)colIndexes["File"]]);
					file.FileSize = Convert.ToInt32(values[(int)colIndexes["FileSize"]]);
					file.Version = Convert.ToString(values[(int)colIndexes["Version"]]);
					files.Add(file);
				}
				return (MsiFile[])files.ToArray(typeof(MsiFile));
			}
		}
	}
}
