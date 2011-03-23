using System;
using System.Xml;
using System.IO;

namespace LibCheck {

//MODIFIED, mod5007
public class XmlReport : IDisposable {	
    XmlTextWriter xWriter;
	bool NeedsInit;
	bool NeedsClosing;
	bool readyToWrite;
	string storeName = "";
	string interimOldV = "";
	string interimNewV = "";

//MODIFIED, mod5030
   public XmlReport (String outputLoc) {
	xWriter = new XmlTextWriter (new StreamWriter (outputLoc + "results.xml"));
	xWriter.Formatting = Formatting.Indented ;
        xWriter.WriteStartElement("TestResults");

//MODIFIED, mod5016

        xWriter.WriteAttributeString("TestName", "BinaryCompatibility");

	NeedsInit =true;
        NeedsClosing =false;
   }

   public void WriteStartAssembly (string name, string oldv, string newv) {
//Console.WriteLine("p2");
	readyToWrite = true;
	storeName = name;
	interimOldV = oldv;
	interimNewV = newv;
   }


// added for change requests by documentation team
// robvi
	public void WriteAssemblySummary(string added, string removed, string Asm1Total, string Asm2Total)
	{
		xWriter.WriteStartElement("AssemblySummary");
		xWriter.WriteAttributeString("AddedTypes", added);
		xWriter.WriteAttributeString("RemovedTypes", removed);
		xWriter.WriteAttributeString("Asm1TotalTypes", Asm1Total);
		xWriter.WriteAttributeString("Asm2TotalTypes", Asm2Total);
		xWriter.WriteEndElement();
	}

	public void WriteTypeSummary(string added, string removed, string asm1Members, string asm2Members)
	{
		xWriter.WriteStartElement("TypeSummary");
		xWriter.WriteAttributeString("AddedMembers", added);
		xWriter.WriteAttributeString("RemovedMembers", removed);
		xWriter.WriteAttributeString("Asm1TotalMembers", asm1Members);
		xWriter.WriteAttributeString("Asm2TotalMembers", asm2Members);
		xWriter.WriteEndElement();
	}

   private void WriteStartAss (string name, string oldv, string newv) {
        xWriter.WriteStartElement("Assembly");

	string temp = name;
	string tempDot = name.Substring(name.Length - 3,1);
	string tempExt = name.Substring(name.Length - 2,2);

	if (tempDot == ".") {
		try {
			int i = Convert.ToInt32(tempExt);
			temp = temp.Substring(0,name.Length - 3);
		} catch (Exception) {}
	}

        xWriter.WriteAttributeString("Name", temp);
        xWriter.WriteAttributeString("OldVersion", oldv);
	xWriter.WriteAttributeString("NewVersion", newv);
   }

//      <BinaryCompatible  WithBuild="2903">False</BinaryCompatible>
   public void AssemblyCompat (bool compat, string buildNum) {
        xWriter.WriteStartElement("BinaryCompatible");

        xWriter.WriteAttributeString("WithBuild", buildNum);
	xWriter.WriteString (XmlConvert.ToString(compat));
        xWriter.WriteEndElement(); //BinaryCompatible
   }

	public void StartTypeSection(TypeMember tm, string oldVersion, string newVersion) {
		if (readyToWrite) {
			if (oldVersion != null  && oldVersion.Trim() != "")
				interimOldV = oldVersion;

			if (newVersion != null  && newVersion.Trim() != "")
				interimNewV = newVersion;

			WriteStartAss(storeName, interimOldV, interimNewV);
			readyToWrite = false;
		}

		if (NeedsInit) {
			xWriter.WriteStartElement("Type");

			xWriter.WriteAttributeString("Name", tm.TypeName);
			xWriter.WriteAttributeString("Namespace", tm.Namespace);

			NeedsInit = false;
			NeedsClosing = true;
		}
	}

	public void EndTypeSection(string oldVersion, string newVersion) {
		if (readyToWrite) {
			if (oldVersion != null  && oldVersion.Trim() != "")
				interimOldV = oldVersion;

			if (newVersion != null  && newVersion.Trim() != "")
				interimNewV = newVersion;

			WriteStartAss(storeName, interimOldV, interimNewV);
			readyToWrite = false;
		}

		if (NeedsClosing) {
			xWriter.WriteEndElement();
			NeedsInit = true;
			NeedsClosing = false;
		}	
	}

	public void MemberRemoved (TypeMember tm) {
		MemberChange ("Removed", tm.Namespace, tm.TypeName, tm.Name, tm.MemberString, tm);
	}
	public void MemberAdded (TypeMember tm) {
		MemberChange ("Added", tm.Namespace, tm.TypeName, tm.Name, tm.MemberString, tm);
	}

	//     <Member ChangeType="Added" Namespace="System.CodeDom" TypeName="IMethodAttribute" MemberName="Bar" MemberSig="void Bar(int)"/>
	private  void MemberChange (string changeType, string ns, string typeName, string memberName, string sig, TypeMember tm) {
		int i = memberName.IndexOf(".");
		if (i != -1) {
			memberName = memberName.Remove (0,i+1);
		}

		//figure out the assemblies for each param in a method!
		string[] shortKeys = tm.MemberKey.Split(new Char[] {':'});
		string[] paramVals = null;
		string retVal = "";
//Console.WriteLine(
		foreach (string s in shortKeys) {
			if (s.ToLower().IndexOf("parameters=") >= 0) {
				paramVals = s.Replace("Parameters=","").Split(new Char[] {','});

//				for (int j=0;j<paramVals.Length;j++) {
//					paramVals[j] = paramVals[j].Substring(0,
//						paramVals[j].LastIndexOf(".") >= 0 ? 
//						paramVals[j].LastIndexOf(".") : paramVals[j].Length);
//				}
//				break;
			} else if (s.ToLower().IndexOf("returntype=") >= 0) {
				retVal = s.Replace("ReturnType=","");
//Console.WriteLine(retVal);
			}
		}


		xWriter.WriteStartElement("Member");

		xWriter.WriteAttributeString("ChangeType", changeType);
		xWriter.WriteAttributeString("MemberName", memberName);
		xWriter.WriteAttributeString("MemberSignature", RipHTML(sig, paramVals, retVal, true));

		xWriter.WriteEndElement(); //Member
	}


   public void WriteEndAssembly (string oldVersion, string newVersion) {
	if (readyToWrite) {
		if (oldVersion != null  && oldVersion.Trim() != "")
			interimOldV = oldVersion;

		if (newVersion != null  && newVersion.Trim() != "")
			interimNewV = newVersion;

		WriteStartAss(storeName, interimOldV, interimNewV);
		readyToWrite = false;
	}

        xWriter.WriteEndElement(); //Assembly
   }

	//rips out all the html!
	private string RipHTML(string stringToRip, string[] paramVals, string retVal, bool keepString) {
		string temp = stringToRip;

		//first thing is to replace line breaks with a space!
		while (true) {
			int start = temp.ToLower().IndexOf("<br>");

			if (start >= 0) {
				temp = temp.Substring(0,start) + " " + temp.Substring(start + 4);
			} else
				break;
		}
//Console.WriteLine(temp);
//Console.ReadLine();

		int paramCount = 0;

		while (true) {

			if (temp.IndexOf("<") >= 0) {
				int start = temp.IndexOf("<");
				int end = temp.IndexOf(">") + 1;
				string val = "";

				int bracketStart = temp.IndexOf("(");
				int bracketEnd = temp.IndexOf(")");

//must ensure it is a PARAM
//must put it in the right place too...
				if (keepString) {
					//this basically means keep the 'string'part of the section
					//this is only done if it is a span
					int found = temp.ToLower().IndexOf("span title=\"");

					if (found >= 0 && found < end) {
//						found += 12; //this is the length of the above string!
//						int innerFound = temp.ToLower().IndexOf("\"", found + 1);
//						val = temp.Substring(found, innerFound - found) + ".";
						if (found > bracketStart && found < bracketEnd) {

							val = paramVals[paramCount];
							paramCount++;
							if (end >= 0) {
								end = temp.IndexOf(" ", end + 1);
							}
						} else if (found < bracketStart && found < bracketEnd) {
							//a return value!
							end = temp.IndexOf("<", start + 1);
							val = retVal;
//Console.WriteLine(val);
						}
					}
				}

				if (end > 0) {
				    try {
					if (!keepString) {
						temp = temp.Remove(start, end - start);
					} else {
						temp = temp.Remove(start, end - start).Insert(start, val);
//Console.WriteLine(temp);
//Console.ReadLine();
					}

					temp = temp.Replace("mscorlib.System.Void","void");
			    	    } catch (Exception) {}

//				    temp = temp.Substring(0,;
//Console.WriteLine(temp);
//Console.ReadLine();

				} else
					break;
			} else
				break;
		}
//Console.WriteLine("\r\n" + temp);
		//replace non-breaking spaces with a space also...
		while (true) {
			int start = temp.IndexOf("&nbsp");

			if (start >= 0) {
				temp = temp.Substring(0,start) + " " + temp.Substring(start + 5);
			} else {
				temp = temp.Trim();
				return temp;
			}
		}
	}

/*
	//not used yet
	public void WriteVersion(string oldVers, string newVers) {
		xWriter.WriteStartElement("Versions ");

		xWriter.WriteAttributeString("OldVersion", oldVers);
		xWriter.WriteAttributeString("NewVersion", newVers);

		xWriter.WriteEndElement(); //Member
	}
*/

   public void Dispose () {

      xWriter.WriteEndElement (); // end TestResults
      xWriter.Close();
   } 
 
}
}