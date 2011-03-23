using System;
using System.Collections;
using System.IO;

namespace SigHelper {

	// string lookup table, extends Hashtable
	public class TTable: Hashtable {
		// ** Fields
		static int _defaultSize = 128;
		static bool _translate = true;
		static char _fieldDelim = ',';
		
		// ** Constructors
		public TTable(): base(_defaultSize) {}
		public TTable(int size): base(size) {}
		public TTable(String file): base(_defaultSize) { Load(file); }
		public TTable(String file, int size): base(size) { Load(file); }

		// ** Methods
		// read in delimited columns from a file, ingores extras
		public bool Load(String fileName) {
			StreamReader fileReader = new StreamReader(fileName);
			string temp = null;
			bool success = true;
			
			// skip blank lines and comment style entries
			while (fileReader.Peek() != -1) {
				try {
					temp = fileReader.ReadLine();
					if (temp==null || temp==String.Empty || temp=="")
						continue;
					if(temp.StartsWith("//"))
						continue;
					
					string [] arr = temp.Split (new char [] {_fieldDelim});
					if (arr.Length < 2) {
						Console.WriteLine("Damaged entry, '{0}', in file {1}.", temp, fileName);
						continue;
					}
					
					string key = arr[0].Trim();
					if (base.ContainsKey(key)) {
						Console.WriteLine("\r\nDuplicate key, {0}, found in file {1}", key, fileName);
						Console.WriteLine("Existing translation for {0} is {1}.", key, base[key]);
					}
					else
						base.Add (key, arr[1].Trim());
				}
				catch(Exception e) {
					Console.WriteLine("\r\nException in file, {0}, see line: {1}", fileName, temp);
					Console.WriteLine(e.ToString());
					success = false;
				}
			}
			fileReader.Close();
			return success;
		}

		// Look in the table for matches. If none match, strip off the last bit and try again.
		public string Translate(String word) {
			if (_translate && base.ContainsKey(word))
				return (String)base[word];
			else
				return word;
		}

		public string Translate(Object o) {
			if (o is String)
				return Translate((String)o);
			else
				throw new ApplicationException("Expected a string type for lookup.");
		}
	} // end public class TTable
}