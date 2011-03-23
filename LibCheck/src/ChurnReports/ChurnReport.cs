using System;
using System.IO;

namespace ChurnReports {

	/*	Churn report structure:
		Table with summary row for each assembly.
	*/
	public class ChurnReport : GenericReport {

		// ** Constants
		private bool sumclr = false;

		private const string _ChurnHeader = @"
<table width=""90%"" align=""center"" border=""1"" cellspacing=""0"" bordercolor=""A0000"" style=background-color: #F0F0F0; border-style: solid;>
<thead>
 <tr>
  <th width=""56%"" align=""left"">Assembly</td>
  {0}
  <th width=""11%"" align=""left"">Members Added</td>
  <th width=""11%"" align=""left"">Members Removed</td>
  <th width=""11%"" align=""left"">Breaking Changes</td>";




		private const string _SerialHeader = @"
<table width=""90%"" align=""center"" border=""1"" cellspacing=""0"" bordercolor=""A0000"" style=background-color: #F0F0F0; border-style: solid;>
<thead>
 <tr>
  <th width=""56%"" align=""left"">Assembly</td>
  {0}
  <th width=""11%"" align=""left"">Breaking Types</td>";
		
  	private const string _ChurnRow = @"
 <tr bgcolor={0}>
  <td><b>{1} ({4})</b></td>
   {7}
  <td>{2} added</td>
  <td>{3} removed</td>
  <td>{5} breaking</td>";

		private const string _ChurnNonRow = @"
 <tr bgcolor={0}>
  <td><b>{1} ({4})</b></td>{7}
  <td>{2} added</td>
  <td>{3} removed</td>
  <td>{5} breaking</td>";

		private const string _ChurnRowSpecial = @"
 <tr bgcolor={0}>
  <td><b>{1} ({4})</b></td>{7}
  <td>{2} added</td>
  <td>{3} removed</td>
  <td>{5} breaking</td>";

  	private const string _SerialRow = @"
 <tr bgcolor={0}>
  <td><b>{1} ({4})</b></td>
   {7}
  <td>{5} breaking</td>";

		private const string _SerialNonRow = @"
 <tr bgcolor={0}>
  <td><b>{1} ({4})</b></td>{7}
  <td>{5} breaking</td>";

		private const string _SerialRowSpecial = @"
 <tr bgcolor={0}>
  <td><b>{1} ({4})</b></td>{7}
  <td>{5} breaking</td>";

		private string _allDetRow = @"<p><center>
	<tr><td colspan = {0}><center>For a detailed breakdown of the changes, 
	<b><a href=""api_change\details.Unified.{1}"">Click Here</b></a>.</center></td></tr>";

		private const string _ChurnFooter = @"</tbody></table>";

		private string churnHeader = "";
		private const string _chChurn = @"<th width=""11%"" align=""left"">Percent Churn</td>
 </tr>
</thead>
<tbody>";
		private const string _chNoChurn = @"</tr></thead><tbody>";

		private string churnRow = "";
		private const string _crChurn = @"<td>{6:##0.0}% churn</td></tr>";
		private const string _noChurn = @"</tr>";

		private string churnNonRow = "";
		private const string _cnrChurn = @"<td>{6:##0.0}% churn</td></tr>";

		private string churnRowSpecial = "";
		private const string _crsChurn = @"<td>{6}</td></tr>";


		private const string _EchoFormat =
				"SUMMRY:: Assembly {0,-32} - Added Members: {1,3}, Removed: {2,3}, Breaking: {3,3}, Total: {4,5}, Churn: {5:##0.0}%";

		private const string _EchoFormatSpecial =
				"SUMMRY:: Assembly {0,-32} - Added Members: {1,3}, Removed: {2,3}, Breaking: {3,3}, Total: {4,5}, Churn: {5}";

		// ** Ctor
		public ChurnReport(string oldbuild, string newbuild, String location, 
					bool showChurn, bool incHeader, 
					bool sumColor, bool allDetails, bool noLink, bool useHTM,
					bool makeComReport): 
					base(location + "APIChanges" + oldbuild + "to" + newbuild + (useHTM ? ".htm" : ".html"), oldbuild, newbuild) {
			sumclr = sumColor;

			Open();

			string totHeader = "";

			if (showChurn) {
				churnRow = _ChurnRow + _crChurn;
				churnHeader = _ChurnHeader + _chChurn;
				churnNonRow = _ChurnNonRow + _cnrChurn;
				churnRowSpecial = _ChurnRowSpecial + _crsChurn;
			} else {
				churnRow = _ChurnRow + _noChurn;
				churnHeader = _ChurnHeader + _chNoChurn;
				churnNonRow = _ChurnNonRow + _noChurn;
				churnRowSpecial = _ChurnRowSpecial + _noChurn;
			}

			string title = "";
			WriteLine(@"<font face=""Arial"" size=""1"">");
			title += "API Changes from " + _oldbuild + " to " + _newbuild;
			WriteLine(String.Format(_GenericHeader, title, _GenericStyle, 
					"<center>" + title + "</center>"));
			WriteLine("</font>");
			WriteLine(@"<font face=""Arial"" size=""2"">");
			if (incHeader) {
				StreamReader sr = File.OpenText("reffiles\\header.txt");

				while (sr.Peek() > -1) {
					totHeader += sr.ReadLine();
				}
			}

			WriteLine(String.Format(churnHeader, (allDetails ? "" : @"<th> &nbsp;</td>")));
			if (allDetails && noLink == false)
				WriteLine(String.Format(_allDetRow, (showChurn ? "5":"4"), 
						(useHTM ? "htm" : "html")));
			WriteLine(totHeader);

			if (makeComReport) {
				WriteLine("For details of the COM Compatibility results, <a href=ComCompat.htm" + (useHTM ? "" : "l") + "><b>Click Here</b></a>");
			}
		}


		//ctor overloaded for serial compat
		public ChurnReport(string oldbuild, string newbuild, String location, 
					bool showChurn, bool incHeader, 
					bool sumColor, bool allDetails, bool noLink, bool useHTM,
					bool makeComReport, bool addSer):
					base(location + "Serialization Breaking Changes" + oldbuild + "to" + newbuild + (useHTM ? ".htm" : ".html"), oldbuild, newbuild) {
			sumclr = sumColor;

			Open();

			string totHeader = "";

			if (showChurn) {
				churnRow = _SerialRow + _crChurn;
				churnHeader = _SerialHeader + _chChurn;
				churnNonRow = _SerialNonRow + _cnrChurn;
				churnRowSpecial = _SerialRowSpecial + _crsChurn;
			} else {
				churnRow = _SerialRow + _noChurn;
				churnHeader = _SerialHeader + _chNoChurn;
				churnNonRow = _SerialNonRow + _noChurn;
				churnRowSpecial = _SerialRowSpecial + _noChurn;
			}

			string title = "";
			WriteLine(@"<font face=""Arial"" size=""1"">");
			title += "Serialization Field Changes from " + _oldbuild + " to " + _newbuild;
			WriteLine(String.Format(_GenericHeader, title, _GenericStyle, 
					"<center>" + title + "</center>"));
			WriteLine("</font>");
			WriteLine(@"<font face=""Arial"" size=""2"">");
			if (incHeader) {
				StreamReader sr = File.OpenText("reffiles\\header.txt");

				while (sr.Peek() > -1) {
					totHeader += sr.ReadLine();
				}
			}

			WriteLine(String.Format(churnHeader, (allDetails ? "" : @"<th> &nbsp;</td>")));
			if (allDetails && noLink == false)
				WriteLine(String.Format(_allDetRow, (showChurn ? "5":"4"), 
						(useHTM ? "htm" : "html")));
			WriteLine(totHeader);

		}


		public ChurnReport(string oldbuild, string newbuild, String location, 
					bool showChurn, bool incHeader, 
					bool sumColor, bool allDetails, bool noLink, bool useHTM,
					bool makeComReport, bool addSer, bool addStruct):
					base(location + "Layout Breaking Changes" + oldbuild + "to" + newbuild + (useHTM ? ".htm" : ".html"), oldbuild, newbuild) {
			sumclr = sumColor;

			Open();

			string totHeader = "";

			if (showChurn) {
				churnRow = _SerialRow + _crChurn;
				churnHeader = _SerialHeader + _chChurn;
				churnNonRow = _SerialNonRow + _cnrChurn;
				churnRowSpecial = _SerialRowSpecial + _crsChurn;
			} else {
				churnRow = _SerialRow + _noChurn;
				churnHeader = _SerialHeader + _chNoChurn;
				churnNonRow = _SerialNonRow + _noChurn;
				churnRowSpecial = _SerialRowSpecial + _noChurn;
			}

			string title = "";
			WriteLine(@"<font face=""Arial"" size=""1"">");
			title += "Field Changes from " + _oldbuild + " to " + _newbuild;
			WriteLine(String.Format(_GenericHeader, title, _GenericStyle, 
					"<center>" + title + "</center>"));
			WriteLine("</font>");
			WriteLine(@"<font face=""Arial"" size=""2"">");
			if (incHeader) {
				StreamReader sr = File.OpenText("reffiles\\header.txt");

				while (sr.Peek() > -1) {
					totHeader += sr.ReadLine();
				}
			}

			WriteLine(String.Format(churnHeader, (allDetails ? "" : @"<th> &nbsp;</td>")));
			if (allDetails && noLink == false)
				WriteLine(String.Format(_allDetRow, (showChurn ? "5":"4"), 
						(useHTM ? "htm" : "html")));
			WriteLine(totHeader);

		}


		// ** Methods
		public string WriteRow(string assembly, bool aAdded, bool aRemoved, int added, 
				int removed, int total, int breaking, bool allDetails, 
				bool sumAll, bool useHTM) {

			string color;

			if (sumclr == false)
				color = "#F0F0F0";

			else if (added > 0 && (removed + breaking) <=0)
				color = "\"#99FF99\"";
			else if (breaking > 10)
				color = "red";
			else if (breaking > 0)
				color = "yellow";
			else if (removed > 0) //should never get here...
				color = "\"FFAA44\"";
			else
				color = "#F0F0F0";

			float churn = (float)(Math.Max(added,removed) * 100.0 / total);
			string special = (aAdded) ? "New Assembly" : ((aRemoved) ? "Removed Assembly" : "");
			if (aAdded || aRemoved) {
				if (allDetails) {
				    if (!((added + removed + breaking) <= 0 && sumAll == false))
					WriteLine(String.Format(churnRowSpecial, new object [] {color, 
							assembly, added, removed, total, breaking, special,""}));
				} else {
				    if (!((added + removed + breaking) <= 0 && sumAll == false))
					WriteLine(String.Format(churnRowSpecial, new object [] {color, 
							assembly, added, removed, total, breaking, special,
			String.Format(@"<td><a href=""{0}.Unified.{1}"">[Details]</a></td>",
					assembly, (useHTM ? "htm" : "html"))}));
				}
			} else {
				if ((added + removed + breaking) == 0) {
					if (allDetails) {
					    if (!((added + removed + breaking) <= 0 && sumAll == false))
						WriteLine(String.Format(churnNonRow, new object [] {color, assembly, 
							added, removed, total, breaking, churn,""}));
					} else {
					    if (!((added + removed + breaking) <= 0 && sumAll == false))
						WriteLine(String.Format(churnNonRow, new object [] {color, assembly, 
							added, removed, total, breaking, churn,
							@"<td>&nbsp;</td>"}));
					}
				} else {			
					if (allDetails) {
					    if (!((added + removed + breaking) <= 0 && sumAll == false))
						WriteLine(String.Format(churnRow, new object [] {color, assembly, 
							added, removed, total, breaking, churn,""}));
					} else {
					    if (!((added + removed + breaking) <= 0 && sumAll == false))
						WriteLine(String.Format(churnRow, new object [] {color, assembly, 
							added, removed, total, breaking, churn,
							String.Format(@"<td><center><b><a href=""api_change\{0}.Unified.{1}"">[Details]</a></b></center></td>",
assembly, (useHTM ? "htm" : "html"))}));
					}
				}
			}
			Flush();

			if (aAdded || aRemoved)
				return String.Format(_EchoFormatSpecial, new object [] {assembly, added, 
						removed, breaking, total, special});
			else
				return String.Format(_EchoFormat, new object [] {assembly, added, 
						removed, breaking, total, churn});
		}

		public void AddHeader() {}

		public new void Close() {
			Close(false,false,true,false);
		}

		public void Close(bool allDetails, bool noLink, bool showChurn, bool useHTM) {

			if (allDetails && noLink == false)
				WriteLine(String.Format(_allDetRow, (showChurn ? "5":"4"),
						(useHTM ? "htm" : "html")));
			WriteLine(_ChurnFooter);

			WriteLine(_GenericEndNav);
			WriteLine(_GenericFooter);
			base.Close();
		}
	} // end ChurnReport
} // namespace
