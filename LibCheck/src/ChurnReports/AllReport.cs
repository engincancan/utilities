using System;
using System.IO;

namespace ChurnReports {

	/*	All report structure:
		AssemblyName
			Type list
				Added/Removed list
					Members list
				Added/Removed list terminator

		Any color coding should be part of the typesig or membersig parameter
	*/
	public class AllReport : GenericReport {

		// ** Constants
		private const string _AllAssembly = @"
<a name=""{0}"" id=""{0}""></a>
<h2>Member Changes in {0}<h2>
";

		private const string _AllType = @"
<h4>Type: {0} ({1})</h4>
";

		private const string _AllChangeStart = @"
<dl><dt>{0} Members<dt>
";

		private const string _AllMember = @"
 <dd>{0}</dd>
";

		private const string _AllChangeEnd = @"
</dl>
";

		// ** Ctor
		public AllReport(string oldbuild, string newbuild): base("All.html", oldbuild, newbuild) {
			Open();
			string title = "Changed Members Detail from " + _oldbuild + " to " + _newbuild;
			WriteLine(String.Format(_GenericHeader, title, _GenericStyle, 
					"<center>" + title + "</center>"));
		}

		// ** Methods
		public void WriteAssembly(string assembly) { WriteLine(String.Format(_AllAssembly, assembly)); }

		public void WriteType(string typesig, string[] owners, bool showOwners) {
			WriteLine(String.Format(_AllType, typesig, MailTo(typesig, owners, showOwners)));
		}

		public void WriteAddedLine()              { WriteLine(String.Format(_AllChangeStart, "Added")); }
		public void WriteRemovedLine()            { WriteLine(String.Format(_AllChangeStart, "Removed")); }
		public void WriteMember(string membersig) { WriteLine(String.Format(_AllMember, membersig)); }
		public void WriteChangeEnd()              { WriteLine(_AllChangeEnd); Flush(); }

		public new void Close() {
			WriteLine(_GenericEndNav);
			WriteLine(_GenericFooter);
			base.Close();
		}
	} // end AllReport

} // namespace