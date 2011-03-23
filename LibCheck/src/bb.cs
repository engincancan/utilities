using System;
using System.Xml;
using System.IO;
using System.Data;
using System.Collections;

public class bb {
	public static Change change = new Change();
	public static string OldBuild;
	public static string NewBuild;
 

    public static void DoFile (string name) {
        string tname = null;
		string assname = null;
		string version = null;
		Report report = new Report();
        try {
	        StreamReader sr = new StreamReader (name);
	        XmlTextReader xReader = new XmlTextReader (sr);
            while (xReader.Read ()) {
                if (xReader.NodeType == XmlNodeType.Element) {
                    if ( xReader.Name == "Assembly") {
						assname = xReader.GetAttribute ("Name");
						//AssemblyName = assname;
                        //Console.WriteLine (assname);
						version = xReader.GetAttribute ("NewVersion");
						NewBuild = version;
						OldBuild = xReader.GetAttribute ("OldVersion");
//Console.WriteLine("p1");
                    //    Console.WriteLine (version);
					}
					if ( xReader.Name == "Member") {
                        tname = xReader.GetAttribute ("TypeName");
						//Console.WriteLine (tname);
					//	tname = xReader.GetAttribute ("Namespace")+"."+tname;
                     
						//Console.WriteLine (tname);
						DataSet d = change.GetChanges(tname);
						if (d == null || d.Tables["Changes"].Rows.Count == 0) {
							//Console.WriteLine ("<br>Breaking change in {0} not documented. </br>", xReader.GetAttribute ("MemberSignature"));
							
							int raidNum = GetRaidNumber (tname);
							string text = String.Format ("Member {0}: {1}", xReader.GetAttribute ("ChangeType"), xReader.GetAttribute ("MemberSignature"));
							report.Add(assname, tname,  raidNum.ToString(), "0",text);
						} 
						else {
							//Console.WriteLine ("---------{0}--{1},{2}-----------",tname,d.Tables["Changes"].Rows.Count ,d.Tables["Changes"].Columns.Count );
							 report.Add(assname, tname, "0","0","");
							foreach(DataRow myRow in d.Tables["Changes"].Rows){
								if (myRow["date_entered"] is DateTime) {
									DateTime date = (DateTime)myRow["date_entered"];
									
									if ((DateTime.Now-date).TotalDays >= 10) { //you have 10 days to get it checked in
										//Console.WriteLine ("Skipping old change for {0}", (DateTime.Now-date).TotalDays);

										continue;
									}
								} else Console.WriteLine ("No date for {0}", tname);
								
									
								
								
								if (xReader.GetAttribute ("MemberSignature").IndexOf("(i)") != -1) {
								   continue;
							    }
								//if (myRow["number"].ToString().CompareTo(OldBuild) >=0 &&
									//myRow["number"].ToString().CompareTo(NewBuild) <=0 ) {
									int raidNum = GetRaidNumber (tname);
									string text = String.Format ("Member {0}: {1}", xReader.GetAttribute ("ChangeType"), xReader.GetAttribute ("MemberSignature"));
								   report.Add(assname, tname, raidNum.ToString(), myRow["change_id"].ToString(),text);
								/*} else {
									Console.WriteLine("{0} not between {1} and {2}", myRow["change_id"], OldBuild, NewBuild);
									int raidNum = GetRaidNumber (tname);
									string text = String.Format ("Member {0}: {1}", xReader.GetAttribute ("ChangeType"), xReader.GetAttribute ("MemberSignature"));
									report.Add(assname, tname, raidNum.ToString(), "0",text);
								}
							*/
																		  
														  
									
							}
								//}
							
   
							//DataColumn dc = d.Tables["Changes"].Columns["Title"].ToString();
							//Console.WriteLine (dc.);
							//foreach (IDataParameter p in d.GetFillParameters()) {
							//	Console.WriteLine("{0}, {1}, {2}", p.Value,p.ParameterName,p.SourceColumn );
							//}
							
						}
						
                    }
				}
			}
			report.Write(Console.Out);
        }
        catch (Exception e) {
                Console.WriteLine ("Problem with {0}.  Reason: {1}", name, e);
        }
    }
    public static void Main (string [] args) {
		if (args.Length == 0) {
			DoFile ("results.xml");
		}
		foreach (string s in args) {
		  DoFile (s);
		}
		

 
    }
    static int GetRaidNumber (string typeName){
        return 0;
    }
	static int GetRaidNumber1 (string typeName){
		try {
		
			BugListService b = new BugListService();
			DataSet bugs = new DataSet();
			//string sql = string.Format(@"Select * from Bugs Where Title Like 'Breaking Change' And IssueType = 'Change Request' And WarStatus = 'Approve' And Status = 'Active' And (Title Like '{0}' Or Description Like '{0}' )", typeName);
			string sql = string.Format(@"Select * from Bugs");


			b.GetBugList ("URT",sql,out bugs);
			if (bugs == null || bugs.Tables["Bugs"] == null) {
				Console.WriteLine ("no bugs for {0}", typeName);
				return 0;
			}
			if (bugs.Tables["Bugs"].Rows.Count == 0) {
				Console.WriteLine ("empty bug list for {0}", typeName);
			}
			int bugNum = 0;
			foreach(DataRow myRow in bugs.Tables["Bugs"].Rows){
				Console.WriteLine ("{0}:{1}", typeName, myRow["BugID"]);
				if (Convert.ToInt32 (myRow["BugID"]) > bugNum) {
					bugNum = Convert.ToInt32 (myRow["BugID"]);
				}			
			}
			return bugNum;	
		} 
		catch (Exception e) {
			Console.WriteLine (e);
			return 0;
		}
    
	}
}


public class Report {
	Hashtable table;
	public Report () {
		table = new Hashtable();
	}
	public void Add (string AssemblyName, string Type, string BugNumber, string ChangeNumber, string Text) {
       if (table.Contains (Type)) {
			Entry e = new Entry (BugNumber, ChangeNumber,Text, AssemblyName);
		    ArrayList l = (ArrayList)table[Type];
		    if (!l.Contains (e)) {
				l.Add( e);
			}

	   } 
	   else {
		   ArrayList l = new ArrayList ();
		   l.Add (new Entry (BugNumber, ChangeNumber,Text,AssemblyName));
		   table.Add (Type, l);
	   }
	}

	public void Write (TextWriter w) {
		foreach (DictionaryEntry de in table) {

			w.WriteLine (@"<h2><p>{2}: <a target=_blank href= http://urtcop/Churn/{0}to{1}/{2}.Unified.html#{3} >{3}</a></p></h2>", bb.OldBuild, bb.NewBuild, ((Entry)((IList)de.Value)[0]).AssemblyName,de.Key);
			w.WriteLine("<blockquote>");
			Entry e = null; 

			foreach (Entry e1 in (ICollection)de.Value) {
				w.WriteLine("<p>{0}</p>", e1.Text);
				e = e1;
			}
			if (e.ChangeNumber == string.Empty || e.ChangeNumber == "0" ) {
				w.WriteLine("<p><font color=red>No Change Entry Found </font></p>");
			} 
			else {
				w.WriteLine(@"<p><a target=_blank href=http://net/change_details.aspx?change%5Fid={0} > Change ID {0}</a></p>", e.ChangeNumber);
			}
			if (e.BugNumber== string.Empty || e.BugNumber== "0" ) {
				w.WriteLine("<p><font color=red>No Raid Entry Found </font></p>");
			} 
			else {
				w.WriteLine(@"<p><a target=_blank href=http://webbugs/Bugs/BugDetails.aspx?BugID={0}&ConnID=44  > Bug Number {0}</a></p>", e.BugNumber);
			}				
			w.WriteLine("</blockquote>");
		}

		
	}
}
public class Entry {
		public string BugNumber;
		public string ChangeNumber;
		public string Text;
		public string AssemblyName;
		public Entry (string bugNumber, string changeNumber, string text, string assemblyName){
			BugNumber = bugNumber;
			ChangeNumber = changeNumber;
			Text = text;
			AssemblyName = assemblyName;
		}
	//Only checks Text and AssemblyName
		override public bool Equals (object o) {
			Entry other = o as Entry;
			if (o == null) {
				return false;
			}
			
			return other.Text == Text && other.AssemblyName==AssemblyName;
		}
		override public int GetHashCode () {
			return base.GetHashCode();
		}
}