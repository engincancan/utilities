/*
Scott Willeke - 2004
http://scott.willeke.com 
Consider this code licensed under Common Public License Version 1.0 (http://www.opensource.org/licenses/cpl1.0.txt).
*/
using System;
using System.Collections;
using System.Text;

using Microsoft.Tools.WindowsInstallerXml;
using Microsoft.Tools.WindowsInstallerXml.Msi;
using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

namespace Willeke.Scott.LessMSIerables
{
	/// <summary>
	/// FYI: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/msi/setup/column_definition_format.asp
	/// </summary>
	class ColumnInfo
	{
		public ColumnInfo(string name, string typeID)
		{
			this.Name = name;
			this.TypeID = typeID;
		}

		public string Name;

		/// <summary>
		/// s? 	String, variable length (?=1-255)
		/// s0 	String, variable length
		/// i2 	Short integer
		/// i4 	Long integer
		/// v0 	Binary Stream
		/// g? 	Temporary string (?=0-255)
		/// j? 	Temporary integer (?=0,1,2,4)
		/// An uppercase letter indicates that null values are allowed in the column.
		/// </summary>
		public string TypeID;


		public bool IsString
		{
			get
			{
				return
					TypeID[0] == 's' || TypeID[0] == 'S'
					|| TypeID[0] == 'g' || TypeID[0] == 'G'
					|| TypeID[0] == 'l' || TypeID[0] == 'L';
			}
		}

		public bool IsInteger
		{
			get
			{
				return
					TypeID[0] == 'i' || TypeID[0] == 'I'
					|| TypeID[0] == 'j' || TypeID[0] == 'J'
					;
			}
		}

		public bool IsStream
		{
			get
			{
				return
					 TypeID[0] == 'v' || TypeID[0] == 'V';
			}
		}

		public int Size
		{
			get
			{
				return int.Parse(TypeID.Substring(1));
			}
		}
	}
}
