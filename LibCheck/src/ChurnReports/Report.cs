using System;
using System.IO;

namespace ChurnReports {

	public class GenericReport {

		// ** Constants
		protected const string _GenericHeader = @"
<html>
<head>
<title>{0}</title>
{1}
</head>

<body>
<a name=""top"" id=""top""></a>
<h2>{2}</h2>
";

		protected const string _GenericStyle = @"
<style type=""text/css"">
dt      { font-weight: Bold; }
.big    { font-size: 150%; font-weight: Bold; }
.small  { font-size:  75%; font-weight: Normal; }
.endNav { font-size:  75%; font-weight: Normal; margin: 20; }
</style>
";

		protected const string _GenericEndNav = @"
<p class=""endNav""><font size=2><a href=""#top"">Top</a></font></p>
";


		protected const string _GenericFooter = @"
</body>
</html>
";

		protected const string _MailTo = @"<a HREF=""mailto:{0}?CC={1};{2};{3}&Subject=Changes%20in%20{4}%20from%20{5}%20to%20{6}"">Contact Owners ({0},{1},{2},{3})</a>";

		// ** Fields
		protected string _filename = null;
		protected StreamWriter _writer = null;
		protected string _oldbuild = null;
		protected string _newbuild = null;
		protected bool _open = false;

		// ** Ctor
		public GenericReport(string name): this(name, "", "") {}
		public GenericReport(string name, string oldbuild, string newbuild) {
			_filename = name;
			_oldbuild = oldbuild;
			_newbuild = newbuild;
		}

		// ** Methods
		public void Open() { Open(false); }		// generally want to overwrite any preexisting files.
		public void Open(bool append) {
//Console.WriteLine("p1: " + _filename);
//_filename = _filename.Replace("/","\\");
//Console.WriteLine("p1: " + _filename);
			_writer = new StreamWriter(
						new BufferedStream(
							File.Open(_filename,((append) ? FileMode.Append : 
FileMode.Create), FileAccess.Write, FileShare.Read)));
//Console.WriteLine("p2");
			_open = true;
		}
		public void WriteLine(string stuff) {

			if (_open) _writer.WriteLine(stuff);
			else {
				Console.WriteLine("{0} is not currently open.", _filename);
			}
		}

		public void Flush() {
			if (_open) _writer.Flush();
		}

		public void Close() {
			if (!_open)
				return;

			_open = false;
			_writer.Flush();
			_writer.Close();
		}

		// ** Methods
		public string MailTo(string typestr, string[] owners, bool showOwners) {
			// expected items in owners: PM, Dev, Test, UE.

			//if we're not showing the owners, then don't!
			if (showOwners == false)
				return "";

			if (owners.Length != 4) throw new 
					ArgumentException("The owners' array must contain exactly 4 elements.", "owners");
//			return String.Format(_MailTo, new object[] { owners[0], owners[1], owners[2], owners[3], 
//					typestr, _oldbuild, _newbuild } );

//_MailTo
//@"<a //HREF=""mailto:{0}?CC={1};{2};{3}&Subject=Changes%20in%20{4}%20from%20{5}%20to%20{6}"">Contact Owners //({0},{1},{2},{3})</a>"

		String sPM = owners[0];
		String sDev = owners[1];
		String sTest = owners[2];
		String sUE = owners[3];
		Boolean entryMade = false;
		Boolean ccMade = false;
		string mailto = "";

		string subject = String.Format("Changes%20in%20{0}%20from%20{1}%20to%20{2}", 
				typestr, _oldbuild, _newbuild);

		sPM = (sPM == null ? "" : sPM);
		sDev = (sDev == null ? "" : sDev);
		sTest = (sTest == null ? "" : sTest);
		sUE = (sUE == null ? "" : sUE);

		if ((sPM.Trim() == "" || sPM == "unknown") && 
				(sDev.Trim() == "" || sDev.Trim() == "unknown") &&
				(sTest.Trim() == "" || sTest.Trim() == "unknown") &&
				(sUE.Trim() == "" || sUE.Trim() == "unknown")) {

			mailto = "<font size=2 color=blue><i>No Contacts Available</i></font>";
		}
		else {

			mailto = String.Format("{0}", "<a //HREF=\"mailto:");
//			mailto = String.Format("<a HREF=""mailto:");

			if (sPM.Trim() != "" && sPM.Trim() != "unknown") {
				mailto += String.Format("{0}", sPM);
				entryMade = true;

				sPM += " (PM)";
			}

			if (sDev.Trim() != "" && sDev.Trim() != "unknown") {
				if (entryMade) {
					mailto += String.Format("?CC={0}", sDev);
					ccMade = true;
				}
				else {
					mailto += String.Format("{0}", sDev);
					entryMade = true;
				}

				sDev += " (Dev)";
			}

			if (sTest.Trim() != "" && sTest.Trim() != "unknown") {
				if (ccMade) {
					mailto += String.Format(";{0}", sTest);
				}
				else if (entryMade) {
					mailto += String.Format("?CC={0}", sTest);
					ccMade = true;
				}
				else {
					mailto += String.Format("{0}", sTest);
					entryMade = true;
				}

				sTest += " (Test)";
			}

			if (sUE.Trim() != "" && sUE.Trim() != "unknown") {
				if (ccMade) {
					mailto += String.Format(";{0}", sUE);
				}
				else if (entryMade) {
					mailto += String.Format("?CC={0}", sUE);
					ccMade = true;
				}
				else {
					mailto += String.Format("{0}", sUE);
					entryMade = true;
				}

				sUE += " (UE)";
			}

			mailto += String.Format("&Subject={0}\">Contact Owners (", subject);

			entryMade = false;

			if (sPM.Trim() != "" && sPM.Trim() != "unknown") {
				mailto += sPM.Trim();
				entryMade = true;
			}

			if (sDev.Trim() != "" && sDev.Trim() != "unknown") {
				if (entryMade)
					mailto += ", ";

				mailto += sDev.Trim();
				entryMade = true;			
			}

			if (sTest.Trim() != "" && sTest.Trim() != "unknown") {
				if (entryMade)
					mailto += ", ";

				mailto += sTest.Trim();
				entryMade = true;			
			}

			if (sUE.Trim() != "" && sUE.Trim() != "unknown") {
				if (entryMade)
					mailto += ", ";

				mailto += sUE.Trim();
				entryMade = true;			
			}

			mailto += ")</a>";

		}

//		return _typestring + "(" + mailto + ")";
//		mailto = @ + mailto;
		return @mailto;

		}
	} // End GenericReport

	public class DetailReport : GenericReport {

		// ** Constants
		protected const string _DetailEndNav = @"
<p class=""endNav""><font size=2><a href=""#top"">Top</a> &bull; <a href=""{0}.{1}"" target=_top>Summary</a></font></p>";

		// ** Fields
		protected string _assembly = null;

		// ** Ctor
		public DetailReport(): this("", "", "", "") {}
		public DetailReport(string name): this("", "", "", name) {}
		public DetailReport(string name, string assembly): this(name, "", "", assembly) {}
		public DetailReport(string oldbuild, string newbuild, string assembly): this("", oldbuild, newbuild, assembly) {}
		public DetailReport(string name, string oldbuild, string newbuild, string assembly): base(name, oldbuild, newbuild) {
			_assembly = assembly;
		}
	} // end DetailReport
} // namespace