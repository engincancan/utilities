using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

//MODIFIED, mod5029
	// For allowing different report styles in text or HTML
	[Serializable]
	public enum TypeFormats : byte {
		NamespaceSeparate = 0x01,
			ShortSig = 0x02,
			IncludeInheritFlag = 0x4,
			IncludeBaseClass = 0x8,
			Default = 0,
	}

//MODIFIED, mod5029
	// Modifiers of interest for breaking change logic and reporting
	[Serializable]
	public enum TypeModifiers : byte {
		Unknown = 0x80,
			na = 0x40,
			New = 1,
			Abstract,
			Sealed,
	}

//MODIFIED, mod5029
	// To track classifications that can be made in a report.
	[Serializable]
	public enum TypeKinds : byte {
		Unassigned = 0,
			Unknown = 0x80,
			Class = 1,
			Interface,
			Struct,
			Enum,
			Attribute,
			Delegate,
			//	EventHandler,
			//	EventArgs,
			//	Exception,
	}

//MODIFIED, mod5029
	[Serializable]
	public class GenTypeInfo: GenMemberInfo {

// ** Constants
//MODIFIED, mod5004
		private const BindingFlags allBindingsLookup = BindingFlags.Public | 
				BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		
		// ** Fields
		//protected Type _basetype = null;								// base type for this type
		//robvi
		public Type _basetype = null;								// base type for this type
		protected Type [] _primaryinterfaces = null;					// interfaces that would appear in the class declaration.
		protected Type [] _secondaryinterfaces = null;					// interfaces that are inherited from parent classes and interfaces.
		protected string _namespace = String.Empty;						// namespace for this type
		protected AccessModifiers _access = AccessModifiers.Unassigned;	// access modifiers for this type
		protected TypeModifiers [] _modifiers = null;					// other modifiers for this type
		protected TypeKinds _typekind = TypeKinds.Unassigned;			// classification of this type (see TypeKinds enum)
		protected bool _nested = false;									// is this a nested type
		protected TypeFormats _format = TypeFormats.Default;			// output formatting information
		
		// ** Constructors
		public GenTypeInfo(Type type): this(type, TypeFormats.Default, false) {}
		public GenTypeInfo(Type type, TypeFormats format): this(type, TypeFormats.Default, false) {}
		public GenTypeInfo(Type type, bool flag): this(type, TypeFormats.Default, flag) {}
		public GenTypeInfo(Type type, TypeFormats format, bool flag): base((MemberInfo)type, flag) {

			if (_dbug) Console.Write(" GenTypeInfo: Constructing " + type.FullName);
			_abstract = type.IsAbstract;
			_memberinfo = type;
			_basetype = type.BaseType;
			GetInterfaces(type, out _primaryinterfaces, out _secondaryinterfaces);
			_namespace = (type.Namespace != null) ? type.Namespace : String.Empty;
			_access = GetAccess(type);
//Console.WriteLine("sub002");
			_modifiers = GetModifiers(type);
//Console.WriteLine("sub02");
			_typekind = GetKind(type);
			_nested = (_membertype == MemberTypes.NestedType);
			_format = format;

			_neworoverriden = false;
			if (_nested) {
				_namespace = _name;
				_name = type.Name.Substring(type.Name.IndexOf('$')+1);
				string newname = _declaringbasetype.Name + "$" + _name;

//MODIFIED, mod0003
//				if (_declaringbasetype.GetNestedType(newname,BindingFlags.LookupAll) != null) {
				if (_declaringbasetype.GetNestedType(newname,allBindingsLookup) != null) {
					_neworoverriden = true;
				}
			}
			if (_dbug) Console.Write(" ... Done.");
		}
		
		public GenTypeInfo(GenTypeInfo ti): base((GenMemberInfo)ti) {
			_memberinfo = ti._memberinfo;
			_basetype = ti._basetype;
			_primaryinterfaces = ti._primaryinterfaces;
			_secondaryinterfaces = ti._secondaryinterfaces;
			_namespace = ti._namespace;
			_access = ti._access;
			_modifiers = ti._modifiers;
			_typekind = ti._typekind;
			_nested = ti._nested;
			_format = ti._format;
		}
		
		// ** Properties
		public TypeKinds TypeKind { get { return _typekind; } }
		public TypeFormats TypeFormat { get { return _format; } set { _format = value; } }
		public override string Name { get { return _name; } }
		public override string Key { get { return ToKey(); } }
		public override string ShortKey { get { return ToShortKey(); } }
		public override string Sig { get { return ToString(TypeFormats.Default); } }
		public bool IsSealed {
			get {
				foreach (TypeModifiers mod in _modifiers) {
					if (mod == TypeModifiers.Sealed)
						return true;
				}
				return false;
			}
		}

		protected virtual string ClassBaseString {
			get {
				string result = String.Empty;
				// add class-base declaration
				bool firstone = true;
				if (_basetype != null) {
					result += " : " + _basetype.FullName;
					firstone = false;
				}
				// including interface-list
				foreach (Type t in _primaryinterfaces) {
					if (firstone) {
						result += " : " + t.FullName;
						firstone = false;
					}
					else
						result += ", " + t.FullName;
				}
				// add inherited interfaces for debugging and comparision purposes
				firstone = true;
				foreach (Type t in _secondaryinterfaces) {
					if (firstone) {
						result += " (: " + t.FullName;
						firstone = false;
					}
					else
						result += ", " + t.FullName;
				}
				result += (!firstone) ? ")" : "";
				return result;
			}
		}

		protected virtual string ClassBaseHtml {
			get {
				string result = String.Empty;
				// add class-base declaration
				bool firstone = true;
				if (_basetype != null) {
					result += " : " + _basetype.FullName;
					firstone = false;
				}
				// including interface-list
				foreach (Type t in _primaryinterfaces) {
					if (firstone) {
						result += " : " + t.FullName;
						firstone = false;
					}
					else
						result += ", " + t.FullName;
				}
				// add inherited interfaces for debugging and comparision purposes
				firstone = true;
				foreach (Type t in _secondaryinterfaces) {
					if (firstone) {
						result += " (:<i> " + t.FullName;
						firstone = false;
					}
					else
						result += ", " + t.FullName;
				}
				result += (!firstone) ? "</i>)" : "";
				return result;
			}
		}

		// ** Methods

		// data acquisition methods
		public static AccessModifiers GetAccess(Type type) {
			TypeAttributes access = (TypeAttributes)((int)type.Attributes & (int)TypeAttributes.VisibilityMask);
			switch (access) {
			case TypeAttributes.NotPublic :
			case TypeAttributes.NestedPrivate :
				return AccessModifiers.Private;
			case TypeAttributes.Public :
			case TypeAttributes.NestedPublic :
				return AccessModifiers.Public;
			case TypeAttributes.NestedFamANDAssem :
			case TypeAttributes.NestedFamily :
				return AccessModifiers.Protected;
			case TypeAttributes.NestedAssembly :
				return AccessModifiers.Internal;
			case TypeAttributes.NestedFamORAssem :
				return AccessModifiers.ProtectedInternal;
			default :
				return AccessModifiers.Unknown;
			}
		}
		
		public static TypeModifiers [] GetModifiers(Type type) {
//Console.WriteLine("sub0001");
			ArrayList result = new ArrayList(2);
//Console.WriteLine("sub0002");			
			if (IsNew(type))
				result.Add(TypeModifiers.New);

//Console.WriteLine("sub0003");
			if (type.IsAbstract && !type.IsInterface)
				result.Add(TypeModifiers.Abstract);
//Console.WriteLine("sub0004");

			if (type.IsSealed)
				result.Add(TypeModifiers.Sealed);
//Console.WriteLine("sub0005");			
			return (TypeModifiers [])result.ToArray(Type.GetType("SigHelper.TypeModifiers"));
//Console.WriteLine("sub0006");
		}

		public static TypeKinds GetKind(Type type) {
			if (type.IsInterface)
				return TypeKinds.Interface;
			else if (type.IsEnum)
				return TypeKinds.Enum;
			else if (type.IsValueType)
				return TypeKinds.Struct;
			// should these non-keyword "types" be lower cased?
			else if (type.Equals(Type.GetType("System.Attribute")) || 
						type.IsSubclassOf(Type.GetType("System.Attribute")))
				return TypeKinds.Attribute;
			else if (type.Equals(Type.GetType("System.Delegate")) || 
						type.IsSubclassOf(Type.GetType("System.Delegate")))
				return TypeKinds.Delegate;
			else
				return TypeKinds.Class;
		}

		// Since Type.GetInterfaces returns all interfaces for all base types, 
		// figure out which are unique to this type.
		protected static void GetInterfaces(Type type, out Type [] primary, out Type [] secondary) {
			ArrayList first = new ArrayList(type.GetInterfaces());
			ArrayList second = new ArrayList();

			// sort through all posible routes for inheriting an interface
			ArrayList everything = new ArrayList(first);
			if (type.BaseType!=null && !everything.Contains(type.BaseType))
				everything.Add(type.BaseType);

			foreach(Type t in everything) {
				Type bt = t.BaseType;
				if ((bt != null) && bt.IsInterface && !second.Contains(bt)) {
					second.Add(bt);
					first.Remove(bt);
				}
				foreach(Type i in t.GetInterfaces()) {
					if (!second.Contains(i)) {
						second.Add(i);
						first.Remove(i);
					}
				}
			}

			primary = (Type [])first.ToArray(Type.GetType("System.Type"));
			secondary = (Type [])second.ToArray(Type.GetType("System.Type"));
		}

		// output methods
		public static string ToString(Type t) { return (new GenTypeInfo(t)).ToString(); }
		public static string ToString(Type t, TypeFormats f) { return (new GenTypeInfo(t, f)).ToString(); }
		public override string ToString() { return ToString(_format); }

		public virtual string ToString(TypeFormats format) {
			string result = (IncludeInheritFlag(format) && _inherited) ? InheritedString() : "";
			
			if (ShortSig(format)) 
				return result + ((_namespace != String.Empty) ? _namespace + "." + _name : _name);
			
			string temp = String.Empty;
			result += (NamespaceSeparate(format) && _namespace != String.Empty) ? _namespace + ": " : "";
			result += ((temp = ToString(_access)) != String.Empty) ? temp + " " : "";
			result += ((temp = ToString(_modifiers)) != String.Empty) ? temp + " " : "";
			result += ToString(_typekind) + " ";
			result += (!NamespaceSeparate(format) && _namespace != String.Empty) ? _namespace + "." : "";
			result += _name;
			result += (IncludeBaseClass(format)) ? ClassBaseString : "";

			return result;
		}
		// TODO: add more specific HTML formatting. 
		// Perhaps use spans and classes for a style sheet to interact with.

		public static string ToHtml(Type t) { return (new GenTypeInfo(t)).ToHtml(); }
		public static string ToHtml(Type t, TypeFormats f) { return (new GenTypeInfo(t, f)).ToHtml(); }
		public override string ToHtml() { return ToHtml(_format); }

		public virtual string ToHtml(TypeFormats format) {
			string result = (IncludeInheritFlag(format) && _inherited) ? InheritedHtml() : String.Empty;
			
			if (ShortSig(format))
				return "<b>" + ((_namespace != String.Empty) ? _namespace + 
						"." + _name : _name) + "</b>";
			
			string temp = String.Empty;
			if (NamespaceSeparate(format) && _namespace != String.Empty)
				result += "<b>" + _namespace + "</b>: ";
			result += ((temp = ToHtml(_access)) != String.Empty) ? temp + " " : "";
			result += ((temp = ToHtml(_modifiers)) != String.Empty) ? temp + " " : "";
			result += ToHtml(_typekind) + " ";
			result += "<b>" + ((!NamespaceSeparate(format) && _namespace != String.Empty) ? _namespace + "." : "");
			result += _name + "</b>";
			result += (IncludeBaseClass(format)) ? ClassBaseString : "";
			return result;
		}

		public static string ToKey(Type type) { return (new GenTypeInfo(type)).ToKey(); }
		public override string ToKey() {

//MODIFIED, mod0019
//#if BETA1
			string result = 
				base.ToKey() + ":" +
				"Nested=" + ((_nested) ? "T" : "F") + ":" +
				"Sealed=" + ((IsSealed) ? "T" : "F") + ":" +
				"Scope=" + Enum.GetName(typeof(AccessModifiers), _access) + ":" +
				"Namespace=" + ((_namespace != null) ? _namespace : "") + ":" +
				"BaseType=" + ((_basetype != null) ? _basetype.FullName : "") + ":" +
				"Interfaces=";

//#else
//			string result = 
//				base.ToKey() + ":" +
//				"Nested=" + ((_nested) ? "T" : "F") + ":" +
//				"Sealed=" + ((IsSealed) ? "T" : "F") + ":" +
//				"Scope=" + ((Enum)_access).ToString() + ":" +
//				"Namespace=" + ((_namespace != null) ? _namespace : "") + ":" +
//				"BaseType=" + ((_basetype != null) ? _basetype.FullName : "") + ":" +
//				"Interfaces=";
//#endif

			for (int i = 0; i < _primaryinterfaces.Length; i++)
				result += ((i > 0) ? "," : "") + _primaryinterfaces[i].FullName;
			if (_primaryinterfaces.Length > 0 && _secondaryinterfaces.Length > 0)
				result += ",";
			for (int i = 0; i < _secondaryinterfaces.Length; i++)
				result += ((i > 0) ? "," : "") + _secondaryinterfaces[i].FullName;

			return result;
		}

		public static string ToShortKey(Type type) { return (new GenTypeInfo(type)).ToShortKey(); }
		public override string ToShortKey() {
			string result = 
				base.ToShortKey() +										// consistent MemberInfo information
				((!_nested) ? ":Namespace=" + _namespace : "");			// Namespace if not nested.

			return result;
		}

		// check changes for TypeInfo's
		public static CompareResults EvalTypeChange(string oldkey, string newkey, ArrayList intfcAdds, out bool typeAdded) {
			typeAdded = false;
			Hashtable okey = LoadKey(oldkey);
			Hashtable nkey = LoadKey(newkey);

			if (okey["MemberType"] == null && nkey["MemberType"] == null)
				throw new ArgumentException("Error in one or more of the keys, no MemberType found.");

//			if ((okey["MemberType"] != null && (string)okey["MemberType"] != "TypeInfo") || 
//					(nkey["MemberType"] != null && (string)nkey["MemberType"] != "TypeInfo"))
			
			//robvi check for nested types
			if 
			(
				(okey["MemberType"] != null && ( ((string)okey["MemberType"] != "TypeInfo") && ((string)okey["MemberType"] != "NestedType") )  )
				|| 
				(nkey["MemberType"] != null && ( ((string)nkey["MemberType"] != "TypeInfo") && ((string)nkey["MemberType"] != "NestedType") )  )
			)
						throw new ArgumentException("Keys are not for the correct member type.");	
		
				bool changed = false;		// bookkeeping flags
			

			CompareResults temp = CompareResults.Same;

			if (okey.Count > 0 && nkey.Count > 0) {	// both present

				// compare Type name and Namespace (assumed from same assembly)
				if ((string)okey["MemberName"] != (string)nkey["MemberName"]
					|| (string)okey["Namespace"] != (string)nkey["Namespace"])	
					return CompareResults.Different;

				if ((string)okey["Abstract"] != (string)nkey["Abstract"]) { // change from/to Abstract

					//robvi: a change to abstract is only breaking if the class does not have exactly
					//one private constructor with no parameters and all the members are static
					if ((string)nkey["Abstract"] == "T" && (string)okey["SingleCtor"] == "F" && (string)nkey["AllStatic"]== "T")
						return CompareResults.Breaking;
					else
						changed = true;
				}

				if ((string)okey["Sealed"] != (string)nkey["Sealed"]) {	// change from/to Sealed

					if ((string)nkey["Sealed"] == "T")
						return CompareResults.Breaking;
					else
						changed = true;
				}

				if ((string)okey["Scope"] != (string)nkey["Scope"]) {	// change in Scope

					if (IsBreakingScopeChange((string)okey["Scope"], (string)nkey["Scope"]))
						return CompareResults.Breaking;
					else
						changed = true;
				}

				if ((string)okey["BaseType"] != (string)nkey["BaseType"]) {// change in Component type

					return CompareResults.Breaking;								
// TODO: how to tell if the new type is assignable from the old type.
				}

				if ((string)okey["Interfaces"] != (string)nkey["Interfaces"]) {	// change in Interface list. 

					temp = CompareInterfaces((string)okey["Interfaces"], (string)nkey["Interfaces"],
							intfcAdds);	
				}

//P12, last but not least, figure out if there is 
				if ((string)okey["EnumWithValues"] != 
						(string)nkey["EnumWithValues"]) {
					temp = DetEnumChange((string)okey["EnumWithValues"],
							(string)nkey["EnumWithValues"]);
//					return temp;
				}

				if (temp == CompareResults.Breaking)	// evaluate the fallout of all this.
					return CompareResults.Breaking;
				else if (temp == CompareResults.Same && !changed)
					return CompareResults.Same;
				else
					return CompareResults.NonBreaking;

			}
			else if (nkey.Count > 0) {		// represents an added type
				typeAdded = true;
				// actually, since this is adding a type, this should never be breaking
				// rule revoked, 02/25/02
//				if ((string)nkey["Abstract"] == "T")	
// only breaking if new abstract type
//					return CompareResults.Breaking;
//				else
//
//				if ((string)nkey["Abstract"] == "T")	
// only breaking if new abstract type
//					return CompareResults.Breaking;
//				else
					return CompareResults.NonBreaking;
			}
			else {
				// represents a removed type, always breaking
				return CompareResults.Breaking;
			}
		}

		// check changes for NestedType's
		public static CompareResults EvalChange(Hashtable okey, Hashtable nkey, bool oldAbst, bool newAbst,
				ArrayList intfcAdds) {
			if (okey["MemberType"] == null && nkey["MemberType"] == null)
				throw new ArgumentException("Error in one or more of the keys, no MemberType found.");
			if ((okey["MemberType"] != null && (string)okey["MemberType"] != "NestedType") || (nkey["MemberType"] != null && (string)nkey["MemberType"] != "NestedType"))
				throw new ArgumentException("Keys are not for the correct member type.");

			bool changed = false;									// bookkeeping flags
			CompareResults temp = CompareResults.Same;

			if (okey.Count > 0 && nkey.Count > 0) {					// both present
				if ((string)okey["MemberName"] != (string)nkey["MemberName"])	// compare Type name (assumed member of same type)
					return CompareResults.Different;
				if ((string)okey["Abstract"] != (string)nkey["Abstract"]) {	// change from/to Abstract
					if ((string)nkey["Abstract"] == "T" && newAbst) return CompareResults.Breaking;
					else changed = true;
				}
				if ((string)okey["Sealed"] != (string)nkey["Sealed"]) {			// change from/to Sealed: Does this make sense for a nested type?
					if ((string)nkey["Sealed"] == "T") return CompareResults.Breaking;
					else changed = true;
				}
				if ((string)okey["Scope"] != (string)nkey["Scope"]) {			// change in Scope
					if (IsBreakingScopeChange((string)okey["Scope"], (string)nkey["Scope"])) return CompareResults.Breaking;
					else changed = true;
				}
				if ((string)okey["BaseType"] != (string)nkey["BaseType"])		// change in Component type
					return CompareResults.Breaking;								// TODO: how to tell if the new type is assignable from the old type.
				if ((string)okey["Interfaces"] != (string)nkey["Interfaces"])	// change in Interface list. TODO: check if this is the desired logic.
					temp = CompareInterfaces((string)okey["Interfaces"], (string)nkey["Interfaces"],
							intfcAdds);

				if (temp == CompareResults.Breaking)							// evaluate the fallout of all this.
					return CompareResults.Breaking;
				else if (temp == CompareResults.Same && !changed)
					return CompareResults.Same;
				else
					return CompareResults.NonBreaking;
			}
			else if (nkey.Count > 0) {								// represents an added nestedtype
				if ((string)nkey["Abstract"] == "T" && newAbst)					// only breaking if new abstract member of an abstract type
					return CompareResults.Breaking;
				else
					return CompareResults.NonBreaking;
			}
			else													// represents a removed nestedtype, always breaking
				return CompareResults.Breaking;
		}

		public static CompareResults CompareInterfaces(string oldones, string newones, ArrayList intfcAdds) {

			// extract interfaces
			ArrayList obits = new ArrayList(oldones.Split(new Char[] {','}));	
			ArrayList nbits = new ArrayList(newones.Split(new Char[] {','}));

			// remove interfaces that are in each
			int max = obits.Count;

			for (int i = 0; i < max;) {
				bool found = false;
				for (int j = 0; j < nbits.Count; j++) {
					if (obits[i].Equals(nbits[j])) {
						found = true;
						nbits.RemoveAt(j);
						obits.RemoveAt(i);
						max--;
						break;
					}
				}
				if (!found) i++;
			}

//MODIFIED, mod5037
			//for each empty string in the oldlist, decrease the count of valid entries by one
			int oldCount = obits.Count;

//MODIFIED, mod5037
//this logic is added
			foreach (String s in obits) {
				if (s.Trim() == "") {
					oldCount--;
				}
			}

			//do the same for the new list...
			int newCount = nbits.Count;

			foreach (String s in nbits) {
				if (s.Trim() == "") {
					newCount--;
				}
			}	

			if (oldCount > 0) {
				return CompareResults.Breaking;	// breaking if any were removed
			}

			if (newCount > 0) {
				//determine if it was a breaking add...
				//if a match is found, then you know this is a breaking add, return
				foreach(string s in intfcAdds) {
					foreach (string y in nbits) {
						if (y.ToLower().Trim() == s.ToLower().Trim()) {
							return CompareResults.Breaking;
						}
					}
				}

				return CompareResults.NonBreaking;	// else nonbreaking if any were added
			}

			return CompareResults.Same;		// otherwise they're the same
		}

		public static void Dump(Type t) { (new GenTypeInfo(t)).Dump(String.Empty); }
		public static void Dump(Type t, string pre) { (new GenTypeInfo(t)).Dump(pre); }
		public override void Dump() { Dump(String.Empty); }
		public override void Dump(string pre) {
			base.Dump(pre);
			Console.WriteLine(pre + "_memberinfo =       " + _memberinfo);
			Console.WriteLine(pre + "_basetype =   " + _basetype);
			Console.Write(pre + "_primaryinterfaces =");
			foreach(Type t in _primaryinterfaces) { Console.Write(" " + t); }
			Console.Write(pre + "_secondaryinterfaces =");
			foreach(Type t in _secondaryinterfaces) { Console.Write(" " + t); }
			Console.WriteLine();
			Console.WriteLine(pre + "_namespace =  " + _namespace);

//MODIFIED, mod0020
//#if BETA1
			Console.WriteLine(pre + "_access =     " + Enum.GetName(typeof(AccessModifiers), _access));
//#else
//			Console.WriteLine(pre + "_access =     " + ((Enum)_access).ToString());
//#endif

			Console.Write(pre + "_modifiers = ");
			foreach (TypeModifiers m in _modifiers) {

//MODIFIED, mod0021
//#if BETA1
				Console.Write(" " + Enum.GetName(typeof(TypeModifiers), m));
//#else
//				Console.Write(" " + ((Enum)m).ToString());
//#endif
			}



			Console.WriteLine();

//MODIFIED, mod0022
//#if BETA1
			Console.WriteLine(pre + "_typekind =   " + Enum.GetName(typeof(TypeKinds), _typekind));
//#else
//			Console.WriteLine(pre + "_typekind =   " + ((Enum)_typekind).ToString());
//#endif

			Console.WriteLine(pre + "_nested =     " + _nested);

//MODIFIED, mod0023
//#if BETA1
			Console.WriteLine(pre + "_format =     " + Enum.GetName(typeof(TypeFormats), _format));
//#else
//			Console.WriteLine(pre + "_format =     " + ((Enum)_format).ToString());
//#endif

		}

		// ouput helper methods
		public static string ToString(AccessModifiers access) { return ((Enum)access).ToString(); }
		
		public static string ToString(TypeModifiers [] modifiers) {
			string result = String.Empty;
			foreach (TypeModifiers tm in modifiers)
				result += ((Enum)tm).ToString() + " ";
			return result.TrimEnd(new Char[] {' '});
		}
		
		public static string ToString(TypeKinds kind) { return ((Enum)kind).ToString(); }
		
		public static string ToHtml(AccessModifiers access) {
			string result = ((Enum)access).ToString();
			if (result == "Unassigned" || result == "NoModifier")
				result = "<i>" + result + "</i>";
			return result;
		}
		
		public static string ToHtml(TypeModifiers [] modifiers) {
			string result = String.Empty;
			foreach (TypeModifiers tm in modifiers) {
				result += (tm == TypeModifiers.Unknown) ? "<i>UnknownModifier1</i> " : 
						((Enum)tm).ToString() + " ";
			}

			return result.TrimEnd(new Char[] {' '});
		}
		
		public static string ToHtml(TypeKinds kind) {
			string result = ((Enum)kind).ToString();
			if (result == "Unassigned" || result == "Unknown")
				result = "<i>" + result + "</i>";
			return result;
		}

		// formatting helper methods
		protected static bool NamespaceSeparate(TypeFormats format)		{ return ((format & TypeFormats.NamespaceSeparate) != 0); }
		protected static bool ShortSig(TypeFormats format)				{ return ((format & TypeFormats.ShortSig) != 0); }
		protected static bool IncludeInheritFlag(TypeFormats format)	{ return ((format & TypeFormats.IncludeInheritFlag) != 0); }
		protected static bool IncludeBaseClass(TypeFormats format)		{ return ((format & TypeFormats.IncludeBaseClass) != 0); }

		// general helper methods
		public bool IsNew() { return _neworoverriden; }
		public static bool IsNew(Type type) {

			if (type.MemberType == MemberTypes.NestedType && type.DeclaringType.BaseType != null) {
#if BETA1
				string newname = "";

				if (type.Name.IndexOf('$') >= 0)
					newname = type.DeclaringType.BaseType.Name +
							type.Name.Substring(type.Name.IndexOf('$'));
				else
					newname = type.DeclaringType.BaseType.Name +
							"." + type.Name;
#else
				string newname = "";

				if (type.Name.IndexOf('+') >= 0)
					newname = type.DeclaringType.BaseType.Name +
							type.Name.Substring(type.Name.IndexOf('+'));
				else
					newname = type.DeclaringType.BaseType.Name +
							"." + type.Name;
#endif

				if (type.DeclaringType.BaseType.GetNestedType(newname,allBindingsLookup) != null)
					return true;
			}
			return false;
		}

//P12, added entire routine
		private static CompareResults DetEnumChange(string oldEnum, string newEnum) {

			oldEnum = oldEnum == null ? "" : oldEnum;
			newEnum = newEnum == null ? "" : newEnum;

			string[]oldEntries = oldEnum.Split(',');
			string[]newEntries = newEnum.Split(',');

			Array.Sort(oldEntries);
			Array.Sort(newEntries);

			//ok, if the lengths are different, its a breaking change!
			//note that this is only true if an entry was REMOVED
			if (oldEntries.Length != newEntries.Length) {

				if (oldEntries.Length > newEntries.Length) {
					return CompareResults.Breaking;
				} else {
					return CompareResults.NonBreaking;
				}
			} else {
				//if any of the entries are not the same, it's ALSO breaking
				for (int i=0;i<oldEntries.Length;i++) {
						
					if (oldEntries[i] != newEntries[i])
						return CompareResults.Breaking;
				}
			}

			return CompareResults.Same;
		}
	}
}