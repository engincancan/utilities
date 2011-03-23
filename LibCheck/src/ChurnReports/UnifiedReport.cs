using System;
using System.IO;
using System.Collections;

namespace ChurnReports {

	/*	Unified report structure:
		Report contains two columns: Removed and Added members (changed members should show up in both columns.)
		The removed and added columns are organized by type (colspan=2) followed by member lists (in either column).
		Any color coding should be part of the typesig or membersig parameter.

		For changed types (not simple added or removed) use a short sig for the type and add a separate member row for the full typesigs
	*/
	public class UnifiedReport : DetailReport {

		// ** Constants
/*
		private const string _UnifiedHeader = @"
<table width=""95%"" align=""center"" border=""1"" cellspacing=""2"" cellpadding=""2"">
<thead>
 <tr class=""big"">
  <th width=""50%"" align=""left"">Removed</td>
  <th width=""50%"" align=""left"">Added</td>
 </tr>
</thead>
<tbody>";
*/

		private const string _UnifiedHeader = @"
<table width=""90%"" align=""center"" border=""1"" cellspacing=""0"" bordercolor=""A0000"" style=background-color: #F0F0F0; border-style: solid;>
<thead>
 <tr>
  <th width=""50%"" align=""left"">Removed</td>
  <th width=""50%"" align=""left"">Added</td>
 </tr>
</thead>
<tbody>";

		private const string _UnifiedTypeRow = @"
 <tr>
  <a name=""{2}""><td colspan=""2"" align=""center"" style=""font:100%;""><b>{0} ({1}) {3}</b></td>
 </tr>";

		private const string _UnifiedMembersRow = @"
 <tr bgcolor=""#F0F0F0"">
  <td width=""50%"" align=""left"">{0}
  </td>
  <td width=""50%"" align=""left"">{1}
  </td>
 </tr>";

		private const string _UnifiedMemberList = @"
   <ul>{0}
   </ul>";

		private const string _UnifiedMemberListItem = @"
    <li>{0}</li>";

		private const string _UnifiedFooter = @"
</tbody>
</table>";

		// ** Fields
		protected bool _stuffAdded = false;
		protected bool _stuffRemoved = false;
		protected string _newBuffer = "";
		protected string _oldBuffer = "";
	//robvi	
		protected string _newSerialBuffer = "";
		protected string _oldSerialBuffer = "";

		private string oldEntry = "";
		private string newEntry = "";

		// ** Ctor
		public UnifiedReport(string oldbuild, string newbuild, string assembly, String location,
				string oldVers, string newVers, bool allDetails, bool useHTM):
					base((allDetails == true) ? location + "details.Unified." +
					(useHTM ? "htm" : "html") : 
					location + assembly + ".Unified." +
					(useHTM ? "htm" : "html"), oldbuild, newbuild, assembly) {
//Console.WriteLine("punif");
				oldEntry = oldbuild;
				newEntry = newbuild;
				Open();
				string title = "";
//Console.WriteLine("punif2");
				if (allDetails)
					title = "Changes from " + _oldbuild + 
					    " (" + (oldVers == "" ? "Version Unknown" : "Version " + oldVers) + 
					    ")<br> to " + _newbuild + " (" + 
					    (newVers == "" ? "Version Unknown" : "Version " + newVers) + ")";
				else
					title = "Changes in " + _assembly + " from " + _oldbuild + 
					    " (" + (oldVers == "" ? "Version Unknown" : "Version " + oldVers) + 
					    ")<br> to " + _newbuild + " (" + 
					    (newVers == "" ? "Version Unknown" : "Version " + newVers) + ")";
				WriteLine(@"<font face=""Arial"" size=""1"">");
				WriteLine(String.Format(_GenericHeader, title, _GenericStyle,
						"<center>" + title + "</center>"));
				WriteLine(_UnifiedHeader);

				Flush();
			}

		// ** Methods
		public void WriteTypeRow(string typesig, string typestr, 
					string extraStr, string[] owners, bool showOwners) {

			WriteLine(String.Format(_UnifiedTypeRow, typesig, MailTo(typestr, owners, showOwners), typestr,
					extraStr));
			Flush();
			_stuffAdded = _stuffRemoved = false;
			_newBuffer = _oldBuffer = "";
		}

		public void WriteTypeSubRow(string oldsig, string newsig, ArrayList breakingItems, bool noColor) {

//ADDITIONS!
			String oldVal = FindSig(oldsig);
			String newVal = FindSig(newsig);

			String tempVal = _UnifiedMembersRow;

			//compare the values...
//THIS CODE SPITS OUT THE SIGNATURE INFO TO THE FILE...
			if (oldVal != newVal && oldVal.Trim() != "n/a" && 
						newVal.Trim() != "n/a" && 
						oldVal.Trim() != "" && newVal.Trim() != "") {

				//if different, change the _UnifiedMembersRow
				//first, find the end '</tr>'
				int locEnd = tempVal.ToLower().IndexOf("</td>");

				if ( locEnd > 0 )
					tempVal = tempVal.Substring(1, locEnd - 1) + 
					"<br><font color=blue size=2>(Please hover your mouse over the type " + 
					"to see the inheritance " + 
					"signatures)</font>"+ tempVal.Substring(locEnd);

				locEnd = tempVal.ToLower().LastIndexOf("</td>");

				if ( locEnd > 0 ) {
/*
					string colChange = "blue";

					foreach(string s in breakingItems) {

						if (oldsig.IndexOf(s) < 0 && newsig.IndexOf(s) >= 0) {
							if (noColor)
								colChange = "black";
							else
								colChange = "B0000";
							break;
						}
					}
*/
					tempVal = tempVal.Substring(1, locEnd - 1) + 
					"<br><font color=blue" + 
					" size=2>(Please hover your mouse over the type " + 
					"to see the inheritance " + 
					"signatures)</font>"+ tempVal.Substring(locEnd);
				}
			}

			WriteLine(String.Format(tempVal, oldsig, newsig));
//			WriteLine(String.Format(_UnifiedMembersRow, oldsig, newsig));
			Flush();
		}

		private String FindSig( String valToSearch ) {

			//rip the searchstring, for the <span title section
			int fp = valToSearch.IndexOf("<span title=\"");
			int sp = 0;
			string s = "";

			if (fp > 0)
				sp = valToSearch.IndexOf("\">", fp + 1);

			if (sp > 0)
				s = valToSearch.Substring(fp + 13, sp - (fp + 13));

			return s;
		}

		public bool AddedMember(string membersig, bool ignoreInherit, ref int added, bool verify) {

			if (membersig.IndexOf("(i)") < 0 || true ) {
//			if (membersig.IndexOf("(i)") < 0 ) { //|| ignoreInherit == false) {
				if (verify == false) {
					_stuffAdded = true;
					_newBuffer += String.Format(_UnifiedMemberListItem, membersig);
				}

				return true;
//			} else if (membersig.IndexOf("(i)") > 0) {
//				added--;
			}
			
			return false;
		}

		public bool RemovedMember(string membersig, bool ignoreInherit, ref int removed, bool verify) {

			if (membersig.IndexOf("(i)") < 0 || true ) {
//			if (membersig.IndexOf("(i)") < 0 ) { //|| ignoreInherit == false) {
				if (verify == false) {
					_stuffRemoved = true;
					_oldBuffer += String.Format(_UnifiedMemberListItem, membersig);
				}

				return true;
//			} else if (membersig.IndexOf("(i)") > 0) {
//				removed--;
			}
			
			return false;
		}

		public void WriteMemberRow() {
			_newBuffer   = (_stuffAdded)   ? String.Format(_UnifiedMemberList, _newBuffer)   : "--> NONE <--";
			_oldBuffer = (_stuffRemoved) ? String.Format(_UnifiedMemberList, _oldBuffer) : "--> NONE <--";
			WriteLine(String.Format(_UnifiedMembersRow, _oldBuffer, _newBuffer));
			Flush();
		}
	

		//robvi
		public bool RemovedSerialMember(string membersig) 
		{
			_oldSerialBuffer += String.Format(_UnifiedMemberListItem, membersig);
			return true;
		}

		public bool AddedSerialMember(string membersig) 
		{
			_newSerialBuffer += String.Format(_UnifiedMemberListItem, membersig);
			return false;
		}

		public void WriteSerialMemberRow() 
		{
			_newSerialBuffer   = (_newSerialBuffer!="") ? String.Format(_UnifiedMemberList, _newSerialBuffer) : "-->NONE<--";
			_oldSerialBuffer = (_oldSerialBuffer!="") ? String.Format(_UnifiedMemberList, _oldSerialBuffer) : "-->NONE<--";
			WriteLine(String.Format(_UnifiedMembersRow, _oldSerialBuffer, _newSerialBuffer));
			_newSerialBuffer="";
			_oldSerialBuffer="";
			Flush();
		}

		public string GetAddedMember() {
			//rip out all the html!

			string temp = _newBuffer;

			 while (true) {
				if (temp.IndexOf("<") >= 0) {
					int start = temp.IndexOf("<");
					int end = temp.IndexOf(">") + 1;
					if (end > 0) {
					    try {
						temp = temp.Remove(start, end - start);
					    } catch (Exception e) {
						Console.WriteLine("A formatting error occurred: " + e.ToString());
					    }

					} else {
						temp = temp.Trim();
						temp = SplitLine(temp, ":");
						return temp;
					}
				} else {
					temp = temp.Trim();
					temp = SplitLine(temp,":");
					return temp;
				}
			}
		}

		public string SplitLine(string temp, string s) {

			int start = -1;
			int spaceFound = -1;
			string output = "";

			while (true) {
				start = temp.IndexOf(s,start + 1);

				if (start >= 0) {
					spaceFound = temp.LastIndexOf(" ",start);

					output += "<br>" + temp.Substring(spaceFound + 1,
						start - spaceFound);

					//copy out the NEXT section
					int nextStart = temp.IndexOf(s,start + 1);
				    if (nextStart > 0) {
					int nextSpace = temp.LastIndexOf(" ",nextStart);

					output += temp.Substring(start + 1, nextSpace - start);
				    } else {
					output += temp.Substring(start + 1);
					return output;
				    }
				} else {
					output += temp.Substring(start + 1);
					return output;
				}
			}
		}

		public void Close(bool useHTM) {
			WriteLine(_UnifiedFooter);
			WriteLine(String.Format(_DetailEndNav, "..\\APIChanges" + oldEntry + "to" + newEntry, (useHTM ? "htm" : "html")));
			WriteLine(_GenericFooter);
			base.Close();
		}
	} // end UnifiedReport
} // namespace
