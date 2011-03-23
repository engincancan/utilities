using System;
using System.IO;

namespace ChurnReports {

	/*	Split report structure:
		Report contains two frames: Removed and Added members (changed members should show up in both frmaes.)
		The removed and added frames are organized by type followed by member list.
		Any color coding should be part of the typesig or membersig parameter.
		For changed types (not simple added or removed) use a short sig for the type and add a separate member row for the full typesigs
	*/

	public class SplitReport : DetailReport {

		// ** Constants
		private const string _SplitFrameset = @"
<html>
<head>
<title>{0}</title>
</head>

<frameset id=""Main"" cols=""*,*"" frameborder=""1"" framespacing=""1"">
 <frame name=""Removed"" src=""{1}.Removed.html"" marginheight=""3"" scrolling=""yes"">
 <frame name=""Added""   src=""{1}.Added.html""   marginheight=""3"" scrolling=""yes"">
 <noframes>
  <p><b>Sorry for the inconvenience, but...</b></p>
  <p>This web site depends on frames and it appears your browser does not support 
   them.  You can download the latest version of Internet Explorer at Microsoft's 
   <a href=""http://www.microsoft.com/ie"">Internet Explorer web site</a>.</p>
 </noframes>
</frameset>

</html>";

		private const string _SplitType = @"
<dl><dt>{0}</dt>";

		private const string _SplitMember = @"
 <dd>{0}</dd>";

		private const string _SplitTypeEnd = @"
</dl>";

		// ** Fields
		protected GenericReport _added = null;
		protected GenericReport _removed = null;

		// ** Ctor
		public SplitReport(string oldbuild, string newbuild, string assembly, String location):
			base(location + assembly + ".Split.html", oldbuild, newbuild, assembly) {
				Open();
				string title = "Split Report for " + _assembly + ": " + _oldbuild + " to " + _newbuild;
				WriteLine(String.Format(_SplitFrameset, title, _assembly));
				base.Close();

				_added = new GenericReport(location + _assembly + ".Added.html");
				_added.Open();
				title = _assembly + "<br /><small>Members Added to " + _newbuild + "</small>";
				_added.WriteLine(String.Format(_GenericHeader, title, _GenericStyle,
						"<center>" + title + "</center>"));
				_added.Flush();

				_removed = new GenericReport(location + _assembly + ".Removed.html");
				_removed.Open();
				title = _assembly + "<br /><small>Members Removed from " + _oldbuild + "</small>";
				_removed.WriteLine(String.Format(_GenericHeader, title, _GenericStyle));
				_removed.Flush();
			}

		// ** Methods
		public void AddedType(string typesig, string typestr, string[] owners, bool showOwners) {
			_added.WriteLine(String.Format(_SplitType, typesig, MailTo(typestr, owners, showOwners)));
		}
		public void RemovedType(string typesig, string typestr, string[] owners, bool showOwners) {
			_removed.WriteLine(String.Format(_SplitType, typesig, MailTo(typestr, owners, showOwners)));
		}

		public void AddedMember(string membersig, bool ignoreInherit) {

			if (membersig.IndexOf("(i)") < 0 || ignoreInherit == false) {

				_added.WriteLine(String.Format(_SplitMember, membersig));
			}
		}
		public void RemovedMember(string membersig, bool ignoreInherit) {

			if (membersig.IndexOf("(i)") < 0 || ignoreInherit == false) {

				_removed.WriteLine(String.Format(_SplitMember, membersig));
			}
		}

		public void AddedTypeEnd() {
			_added.WriteLine(_SplitTypeEnd);
			_added.Flush();
		}
		public void RemovedTypeEnd() {
			_removed.WriteLine(_SplitTypeEnd);
			_removed.Flush();
		}

		public new void Close() {
			_added.WriteLine(_DetailEndNav);
			_added.WriteLine(_GenericFooter);
			_added.Close();

			_removed.WriteLine(_DetailEndNav);
			_removed.WriteLine(_GenericFooter);
			_removed.Close();
		}
	} // end SplitReport
} // namespace