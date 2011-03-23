using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	public enum FieldModifiers : byte {
		Unknown = 0x80,
			na = 0x40,
			New = 1,
			Static,
			Readonly,
	}
	
	public enum FieldKinds : byte {
		Unassigned = 0,
			Unknown = 0x80,
			Constant = 1,
			Field,
	}
	
	public class GenFieldInfo : GenMemberInfo {

		//** Fields
		protected Type				_fieldtype	= null;
		protected AccessModifiers	_access		= AccessModifiers.Unassigned;
		protected FieldModifiers []	_modifiers	= null;
		protected FieldKinds		_fieldkind	= FieldKinds.Unassigned;
		protected object		_value		= null;

		//** Ctors
		public GenFieldInfo (FieldInfo field) : base ((MemberInfo)field) {

			_fieldtype = field.FieldType;
			_access = GetAccess(field);
			_modifiers = GetModifiers(field, _neworoverriden);
			_fieldkind = (field.IsLiteral) ? FieldKinds.Constant : FieldKinds.Field;

			//only store the value if it is public AND if it is literal, that is, set at compile time
			if (field.IsPublic && field.IsLiteral) {
//if (field.Name != (string)(field.GetValue(field).ToString()))
//Console.WriteLine("Name = {0}, Value = {1}", field.Name, field.GetValue(field));
				_value = field.GetValue(field);
			}

		}

		public GenFieldInfo (GenFieldInfo fi) : base ((GenMemberInfo)fi) {
			_fieldtype = fi._fieldtype;
			_access = fi._access;
			_modifiers = fi._modifiers;
			_fieldkind = fi._fieldkind;
			_value = fi._value;
		}

		// ** Properties
		public override string Key { get { return ToKey(); } }
		public override string Sig { get { return ToString(); } }

		// ** Methods
	
		public static AccessModifiers GetAccess(FieldInfo field) {
			FieldAttributes access = field.Attributes & FieldAttributes.FieldAccessMask;
			switch (access) {
			case FieldAttributes.Private :
			case FieldAttributes.PrivateScope :
				return AccessModifiers.Private;
			case FieldAttributes.Public :
				return AccessModifiers.Public;
			case FieldAttributes.FamANDAssem :
			case FieldAttributes.Family :
				return AccessModifiers.Protected;
			case FieldAttributes.Assembly :
				return AccessModifiers.Internal;
			case FieldAttributes.FamORAssem :
				return AccessModifiers.ProtectedInternal;
			default :
				return AccessModifiers.Unknown;
			}
		}
		
		public static FieldModifiers [] GetModifiers(FieldInfo field, bool neworoverriden) {
			ArrayList result = new ArrayList(2);
			
			if (neworoverriden)
				result.Add(FieldModifiers.New);
			
			if (field.IsStatic)
				result.Add(FieldModifiers.Static);
			
			if (field.IsInitOnly)
				result.Add(FieldModifiers.Readonly);

			return (FieldModifiers [])result.ToArray(Type.GetType("SigHelper.FieldModifiers"));
		}

		public static string ToString(FieldInfo fi) { return (new GenFieldInfo(fi)).ToString(); }
		public override string ToString() {
			string temp = null;
			string result = "Field: ";

//MODIFIED, mod0043
//#if BETA1
			result += Enum.GetName(typeof(AccessModifiers), _access);
//#else
//			result += ((Enum)_access).ToString();
//#endif
//Console.WriteLine("const section!");
//Console.ReadLine();
			result += ((temp = ToString(_modifiers)) != "") ? temp + " " : "";
			if (_fieldkind == FieldKinds.Constant)
				result += "const ";
			result += _fieldtype.FullName + " ";
			result += _name;
			if (_value != null)
				result += " = " + _value.ToString();

			return result;
		}

		public static string ToString(FieldModifiers [] modifiers) {
			string result = String.Empty;
			for(int i = 0; i < modifiers.Length; i++) {

//MODIFIED, mod0044
//#if BETA1
				result += ((i > 0) ? " " : "") + Enum.GetName(typeof(FieldModifiers), modifiers[i]);
//#else
//				result += ((i > 0) ? " " : "") + ((Enum)modifiers[i]).ToString();
//#endif

			}

			return result;
		}

		public static string ToHtml(FieldInfo ev) { 
			return (new GenFieldInfo(ev)).ToHtml();
		}

		public override string ToHtml() {
			return ToString();
		}

		public override string ToKey() {

			// consistent MemberInfo information
//MODIFIED, mod0045
//#if BETA1
			string result =
				base.ToKey() + ":" +			
				"Scope=" + Enum.GetName(typeof(AccessModifiers), _access) + ":" +
				"Modifiers=";
//#else
//			string result =
//				base.ToKey() + ":" +			
//				"Scope=" + ((Enum)_access).ToString() + ":" +
//				"Modifiers=";
//#endif


			for (int i = 0; i < _modifiers.Length; i++) {

//MODIFIED, mod0046
//#if BETA1
				result += ((i>0) ? "," : "") + Enum.GetName(typeof(FieldModifiers), _modifiers[i]);
//#else
//				result += ((i>0) ? "," : "") + ((Enum)_modifiers[i]).ToString();
//#endif
			}

//MODIFIED, mod0047
//#if BETA1
//Console.WriteLine("const section!");
//Console.ReadLine();
			result += ":" +
				"FieldType=" + _fieldtype.FullName + ":" +
				"FieldKind=" + Enum.GetName(typeof(FieldKinds), _fieldkind);

//NEW ADDITION: Value
			//this is done to avoid a low char surrogate not followed 
			//by high char surrogate exception
//			try {
//				result += ":" + "Value=" + _value;
				if (_value != null && _value.ToString().Length <= 60) {
					result += ":" + "Value=" + _value;
				}
//				else {

//				}
//			} catch (Exception e) {

//				Console.ReadLine();
//			}
//#else
//			result += ":" +
//				"FieldType=" +_fieldtype.FullName + ":" +
//				"FieldKind=" + ((Enum)_fieldkind).ToString();
//#endif

			return result;
		}

//CHECK IN HERE!!!
		// check changes for Fields
		public static CompareResults EvalChange(Hashtable okey, Hashtable nkey, bool oldAbst, bool newAbst, bool typeAdded) {
			if (okey["MemberType"] == null && nkey["MemberType"] == null)
				throw new ArgumentException("Error in one or more of the keys, no MemberType found.");
			if ((okey["MemberType"] != null && (string)okey["MemberType"] != "Field") || (nkey["MemberType"] != null && (string)nkey["MemberType"] != "Field"))
				throw new ArgumentException("Error in one or more of the keys, incorrect member type.");

			bool changed = false;									// bookkeeping flags
			CompareResults temp = CompareResults.Same;

			if (okey.Count > 0 && nkey.Count > 0) {					// both present
				if ((string)okey["MemberName"] != (string)nkey["MemberName"])		// compare name (assumed members of same type)
					return CompareResults.Different;
				if ((string)okey["Scope"] != (string)nkey["Scope"]) {				// change in scope
					if (IsBreakingScopeChange((string)okey["Scope"], (string)nkey["Scope"])) return CompareResults.Breaking;
					else changed = true;
				}
				if ((string)okey["FieldType"] != (string)nkey["FieldType"])			// change in field type. TODO: figure out if new type is assignable from old type.
					return CompareResults.Breaking;
				//Robvi Added, if the offset is part of the key then it's breaking if it's changed
				if ((string)okey["Offset"] != (string)nkey["Offset"])			
					return CompareResults.Breaking;
				



				if ((string)okey["FieldKind"] != (string)nkey["FieldKind"]) 
				{		// change to const is breaking, otherwise not.
					if ((string)nkey["FieldKind"] == "Constant") return CompareResults.Breaking;
					else changed = true;
				}
				if ((string)okey["Modifiers"] != (string)nkey["Modifiers"])			// find change in modifiers
					temp = CompareModifiers((string)okey["Modifiers"], (string)nkey["Modifiers"]);

//NEW: report if the values have changed...
				if ((string)nkey["Value"] != (string)okey["Value"]) {
					return CompareResults.Breaking;
				}

				if (temp == CompareResults.Breaking)				// evaluate the fallout of all this.
					return CompareResults.Breaking;
				else if (temp == CompareResults.Same && !changed)
					return CompareResults.Same;
				else
					return CompareResults.NonBreaking;
			}
			else if (nkey.Count > 0) {								// represents an added member
				if ((string)nkey["Abstract"] == "T" && newAbst)	
				{	// only breaking if abstractmember of abstract type
					return CompareResults.Breaking;
				}
					// Robvi breaking if a field is added when the offset is present, meaning the layout was either sequential or explicit
				else if((string)nkey["Offset"]!=null && typeAdded==false)
				{
					return CompareResults.Breaking;
				}
				else													// represents a removed member, always breaking
					return CompareResults.NonBreaking;
			}
			else													// represents a removed member, always breaking
				return CompareResults.Breaking;
		}	// EvalChange

		// TODO: verify this logic is correct
		public static CompareResults CompareModifiers(string oldones, string newones) {
			ArrayList obits = new ArrayList(oldones.Split(new Char[] {','}));	// extract modifiers
			ArrayList nbits = new ArrayList(newones.Split(new Char[] {','}));

			// remove modifiers that are in each
			int max = obits.Count;
			for (int i = 0; i < max;) {
				bool found = false;
				for (int j = 0; j < nbits.Count; j++) {
					if (obits[i] == nbits[j]) {
						found = true;
						nbits.RemoveAt(j);
						obits.RemoveAt(i);
						max--;
						break;
					}
				}
				if (!found) i++;
			}
			// change in static'ness or restricting writeability implies breaking
			if (obits.Contains("Static") || nbits.Contains("Static") || nbits.Contains("Readonly"))
				return CompareResults.Breaking;
			// problems in metadata for new member implies breaking
			if (nbits.Contains("Unknown") || nbits.Contains("na"))
				return CompareResults.Breaking;
			// any other changes are non-breaking
			if (obits.Count > 0 || nbits.Count > 0)
				return CompareResults.NonBreaking;
			else
				return CompareResults.Same;;
		}

		public static void Dump(FieldInfo ev) { (new GenFieldInfo(ev)).Dump(String.Empty); }
		public static void Dump(FieldInfo ev, string pre) { (new GenFieldInfo(ev)).Dump(pre); }
		public override void Dump() { Dump(String.Empty); }

		public override void Dump(string pre) {
			base.Dump(pre);
			Console.WriteLine(pre + "_fieldtype =   " + _fieldtype.FullName);

//MODIFIED, mod0048
//#if BETA1
			Console.WriteLine(pre + "_access =      " + Enum.GetName(typeof(AccessModifiers), _access));
//#else
//			Console.WriteLine(pre + "_access =      " + ((Enum)_access).ToString());
//#endif

			Console.Write(pre + "_modifiers =   ");
			for (int i = 0; i < _modifiers.Length; i++) {

//MODIFIED, mod0049
//#if BETA1
				Console.Write(((i > 0) ? " " : "") + Enum.GetName(typeof(FieldModifiers), _modifiers[i]));
//#else
//				Console.Write(((i > 0) ? " " : "") + ((Enum)_modifiers[i]).ToString());
//#endif
			}

			Console.WriteLine();

//MODIFIED, mod0050
//#if BETA1
			Console.WriteLine(pre + "_fieldkind =   " + Enum.GetName(typeof(FieldKinds), _fieldkind));
//#else
//			Console.WriteLine(pre + "_fieldkind =   " + ((Enum)_fieldkind).ToString());
//#endif

//			Console.WriteLine(pre + "_value =       " + _value.ToString());
		}
	}
}