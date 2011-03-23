using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	public enum PropertyModifiers : byte {
		Unknown = 0x80,
			na = 0x40,
			New = 1,
			Override,
			Abstract,
			Virtual,
			Static,
			Protected,
			//robvi
			ProtectedInternal
	
	}
	
	public enum PropertyKinds : byte {
		Unassigned = 0,
			Unknown = 0x80,
			Accessor = 1,
			Indexer,
	}
	
	public class GenPropertyInfo : GenMemberInfo {

		//** Fields
		protected MethodInfo		_getter			= null;
		protected AccessModifiers	_getaccess		= AccessModifiers.Unassigned;
		protected MethodInfo		_setter			= null;
		protected AccessModifiers	_setaccess		= AccessModifiers.Unassigned;
		protected ParameterInfo []	_indexparams	= null;		// this needs to be fron the getter or setter to have parameter names.
		protected Type				_propertytype	= null;
		protected PropertyModifiers []	_modifiers	= null;
		protected PropertyKinds		_propertykind	= PropertyKinds.Unassigned;

		//** Ctors
		public GenPropertyInfo (PropertyInfo property) : this (property, false) {}
		public GenPropertyInfo (PropertyInfo property, bool flag) : base ((MemberInfo)property, flag) {
			_getter			= property.GetGetMethod();
			_setter			= property.GetSetMethod();
			_indexparams	= property.GetIndexParameters();
			if (_indexparams == null)
				_indexparams = new ParameterInfo [0];
			_propertytype	= property.PropertyType;
			_getaccess		= GetGetAccess(property);
			#if DBUGGenPI
//MODIFIED, mod0053
//#if BETA1
			Console.WriteLine(" {0}.{1} get access is: '{2}'", _reflectedtype.FullName, property.Name,
					Enum.GetName(typeof(AccessModifiers), _getaccess));
//#else
//			Console.WriteLine(" {0}.{1} get access is: '{2}'", _reflectedtype.FullName, property.Name,
//				((Enum)_getaccess).ToString());
//#endif

			#endif
			_setaccess		= GetSetAccess(property);
			#if DBUGGenPI
//MODIFIED, mod0054
//#if BETA1
			Console.WriteLine(" {0}.{1} set access is: '{2}'", _reflectedtype.FullName, property.Name, 
					Enum.GetName(typeof(AccessModifiers), _setaccess)););
//#else
//			Console.WriteLine(" {0}.{1} set access is: '{2}'", _reflectedtype.FullName, property.Name,
//					((Enum)_setaccess).ToString());
//#endif

			#endif
			_modifiers		= GetModifiers(property, _neworoverriden);
			_propertykind	= (_indexparams.Length > 0) ? PropertyKinds.Indexer : PropertyKinds.Accessor;

			_abstract = ((_getter != null && _getter.IsAbstract) || (_setter != null && _setter.IsAbstract));
		}

		public GenPropertyInfo (GenPropertyInfo pi) : base ((GenMemberInfo)pi) {
			_getter			= pi._getter;
			_setter			= pi._setter;
			_indexparams	= pi._indexparams;
			_propertytype	= pi._propertytype;
			_getaccess		= pi._getaccess;
			_setaccess		= pi._setaccess;
			_modifiers		= pi._modifiers;
			_propertykind	= pi._propertykind;
		}

		//** Properties
		public override string Key { get { return ToKey(); } }
		public override string ShortKey { get { return ToShortKey(); } }
		public override string Sig { get { return ToString(); } }

		//** Methods
		public static AccessModifiers GetGetAccess(PropertyInfo property) {
//			if (property.ReflectedType.IsInterface)
//				return AccessModifiers.na;
			return GetAccess(property.GetGetMethod());
		}
		public static AccessModifiers GetSetAccess(PropertyInfo property) {
//			if (property.ReflectedType.IsInterface)
//				return AccessModifiers.na;
			return GetAccess(property.GetSetMethod());
		}
		public static AccessModifiers GetAccess(MethodInfo method) {
			if (method == null)
				return AccessModifiers.na;

			MethodAttributes access = (method.Attributes & MethodAttributes.MemberAccessMask);
			#if DBUGGenPI
//MODIFIED, mod0055
//#if BETA1
			Console.WriteLine(" {0,-24} - Scope from MethodAttribute: '{1}'", "",
					Enum.GetName(typeof(MethodAttributes), access));
//#else
//			Console.WriteLine(" {0,-24} - Scope from MethodAttribute: '{1}'", "", ((Enum)access).ToString());
//#endif
			#endif
			switch (access) {
			case MethodAttributes.Private :
			case MethodAttributes.PrivateScope :
				return AccessModifiers.Private;
			case MethodAttributes.Public :
				return AccessModifiers.Public;
			case MethodAttributes.FamANDAssem :
			case MethodAttributes.Family :
				return AccessModifiers.Protected;
			case MethodAttributes.Assembly :
				return AccessModifiers.Internal;
			case MethodAttributes.FamORAssem :
				return AccessModifiers.ProtectedInternal;
			default :
				return AccessModifiers.Unknown;
			}
		}
		
		public static AccessModifiers GetAccess(PropertyInfo property) {
			MethodInfo method = (property.GetGetMethod() != null) ? property.GetGetMethod() : property.GetSetMethod();
			if (method == null)
				return AccessModifiers.Unknown;

			MethodAttributes access = (method.Attributes & MethodAttributes.MemberAccessMask);
			if (property.ReflectedType.IsInterface)
				return AccessModifiers.na;
	
			switch (access) {
			case MethodAttributes.Private :
			case MethodAttributes.PrivateScope :
				return AccessModifiers.Private;
			case MethodAttributes.Public :
				return AccessModifiers.Public;
			case MethodAttributes.FamANDAssem :
			case MethodAttributes.Family :
				return AccessModifiers.Protected;
			case MethodAttributes.Assembly :
				return AccessModifiers.Internal;
			case MethodAttributes.FamORAssem :
				return AccessModifiers.ProtectedInternal;
			default :
				return AccessModifiers.Unknown;
			}
		}
		
		public static PropertyModifiers [] GetModifiers(PropertyInfo property, bool neworoverriden) {

			ArrayList result = new ArrayList(2);

			MethodInfo method = (property.GetGetMethod() != null) ? property.GetGetMethod() : property.GetSetMethod();
			if (method == null) {
				method = (property.GetGetMethod(true) != null) ? property.GetGetMethod(true) : 						property.GetSetMethod(true);
	
				if (method.IsPublic) 
				{ //shouldn't be possible...
				} 
				else if (method.IsFamily) 
				{
					result.Add( PropertyModifiers.Protected );
				} 
				else if (method.IsFamilyOrAssembly )
				{
					result.Add ( PropertyModifiers.ProtectedInternal );
				}
				else
				{ //this is the unknown result...
					return new PropertyModifiers [] { PropertyModifiers.Unknown };
				}
			}

			if (neworoverriden) {
				if ((method.Attributes & MethodAttributes.NewSlot) != 0)
					result.Add(PropertyModifiers.New);
				else if ((method.Attributes & MethodAttributes.Virtual) != 0)
					result.Add(PropertyModifiers.Override);
				else
					result.Add(PropertyModifiers.New);
			}
			
			if (method.IsStatic)
				result.Add(PropertyModifiers.Static);
			
			if (method.IsAbstract 
					&& !property.ReflectedType.IsInterface)
				result.Add(PropertyModifiers.Abstract);

			if (method.IsVirtual
					&& !property.ReflectedType.IsInterface
					&& !result.Contains(PropertyModifiers.Abstract)
					&& !result.Contains(PropertyModifiers.Override))
				result.Add(PropertyModifiers.Virtual);

			return (PropertyModifiers [])result.ToArray(Type.GetType("SigHelper.PropertyModifiers"));
		}

		public override string ToKey() {
			string result =
				base.ToKey() + ":" +									// consistent MemberInfo information
				"Modifiers=";
			for (int i = 0; i < _modifiers.Length; i++) {

//MODIFIED, mod0056
//#if BETA1
				result += ((i>0) ? "," : "") + Enum.GetName(typeof(PropertyModifiers), _modifiers[i]);
//#else
//				result += ((i>0) ? "," : "") + ((Enum)_modifiers[i]).ToString();
//#endif
			}

//MODIFIED, mod0057
//#if BETA1
			result += ":" +
				"PropertyType=" +_propertytype.FullName + ":" +
				"PropertyKind=" + Enum.GetName(typeof(PropertyKinds), _propertykind) + ":" +
				"Getter=" + ((_getter != null) ? "T" : "F") + ":" +
				"GetScope=" + Enum.GetName(typeof(AccessModifiers), _getaccess) + ":" +
				"Setter=" + ((_setter != null) ? "T" : "F") + ":" +
				"SetScope=" + Enum.GetName(typeof(AccessModifiers), _setaccess) + ":" +
				"Parameters=" + GenParameterInfo.PSig(_indexparams);
//#else
//			result += ":" +
//				"PropertyType=" +_propertytype.FullName + ":" +
//				"PropertyKind=" + ((Enum)_propertykind).ToString() + ":" +
//				"Getter=" + ((_getter != null) ? "T" : "F") + ":" +
//				"GetScope=" + ((Enum)_getaccess).ToString() + ":" +
//				"Setter=" + ((_setter != null) ? "T" : "F") + ":" +
//				"SetScope=" + ((Enum)_setaccess).ToString() + ":" +
//				"Parameters=" + GenParameterInfo.PSig(_indexparams);
//#endif

			return result;
		}

		public override string ToShortKey() {
			string result =
				base.ToShortKey() + ":" +								// consistent MemberInfo information
				"Parameters=" + GenParameterInfo.PSig(_indexparams);
			return result;
		}

		// check changes for Properties
		public static CompareResults EvalChange(Hashtable okey, Hashtable nkey, bool oldAbst, bool newAbst, bool typeAdded) {
			if (okey["MemberType"] == null && nkey["MemberType"] == null)
				throw new ArgumentException("Error in one or more of the keys, no MemberType found.");
			if ((okey["MemberType"] != null && (string)okey["MemberType"] != "Property") || (nkey["MemberType"] != null && (string)nkey["MemberType"] != "Property"))
				throw new ArgumentException("Error in one or more of the keys, incorrect member type.");

			bool changed = false;									// bookkeeping flags
			CompareResults temp = CompareResults.Same;

			if (okey.Count > 0 && nkey.Count > 0) {					// both present
				if ((string)okey["MemberName"] != (string)nkey["MemberName"]
					|| (string)okey["Parameters"] != (string)nkey["Parameters"]) {	// compare name and parameter list (assumed members of same type)
					return CompareResults.Different;
				}
				if ((string)okey["PropertyType"] != (string)nkey["PropertyType"]) {	// change in property type. TODO: figure out if new type is assignable from old type.
					return CompareResults.Breaking;
				}
				if ((string)okey["Abstract"] != (string)nkey["Abstract"]) {			// change in abstract'ness
					if ((string)nkey["Abstract"] == "T" && newAbst)
						return CompareResults.Breaking;
					else
						return CompareResults.NonBreaking;
				}
				if ((string)okey["PropertyKind"] != (string)nkey["PropertyKind"]) {	// change in property kind, i.e., Indexer, &c.
					changed = true;
				}
				if ((string)okey["Getter"] == "T") {								// change in get method
					if ((string)nkey["Getter"] == "F")									// removal is breaking
						return CompareResults.Breaking;
					else if ((string)okey["GetScope"] != (string)nkey["GetScope"]) {	// check scope change
						if (IsBreakingScopeChange((string)okey["GetScope"], (string)nkey["GetScope"]))
							return CompareResults.Breaking;
						else
							changed = true;
					}
				}
				if ((string)okey["Setter"] == "T") {								// change in set method
					if ((string)nkey["Setter"] == "F")									// removal is breaking
						return CompareResults.Breaking;
					else if ((string)okey["SetScope"] != (string)nkey["SetScope"]) {	// check scope change
						if (IsBreakingScopeChange((string)okey["SetScope"], (string)nkey["SetScope"]))
							return CompareResults.Breaking;
						else
							changed = true;
					}
				}
				if ((string)okey["Getter"] != (string)nkey["Getter"] ||				// addition breaking only for abstract accessor of abstract type
					(string)okey["Getter"] != (string)nkey["Getter"]) {				// TODO: verify that both accessors share abstractness.
					if ((string)nkey["Abstract"] == "T" && newAbst)
						return CompareResults.Breaking;
					else
						return CompareResults.NonBreaking;
				}
				if ((string)okey["Modifiers"] != (string)nkey["Modifiers"]) {		// change in modifiers
					temp = CompareModifiers((string)okey["Modifiers"], (string)nkey["Modifiers"]);
				}

				if (temp == CompareResults.Breaking) {				// evaluate the fallout of all this.
					return CompareResults.Breaking;
				}
				else if (temp == CompareResults.Same && !changed) {
					return CompareResults.Same;
				}
				else {
					return CompareResults.NonBreaking;
				}
			}
			else if (nkey.Count > 0) {								// represents an added member
				if ((string)nkey["Abstract"] == "T" && newAbst && typeAdded == false) {
// only breaking if abstractmember of abstract type
					return CompareResults.Breaking;
				}
				else {
					return CompareResults.NonBreaking;
				}
			}
			else {													// represents a removed member, always breaking
				return CompareResults.Breaking;
			}
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
			// change to static'ness implies breaking, change to virtual'ness does not.
			if (obits.Contains("Static") || nbits.Contains("Static"))
				return CompareResults.Breaking;
			// problems with new metadata implies breaking(???)
			if (nbits.Contains("Unknown") || nbits.Contains("na")) {
				return CompareResults.Breaking;
			}

			// any other changes are non-breaking
			if (obits.Count > 0 || nbits.Count > 0)
				return CompareResults.NonBreaking;
			else
				return CompareResults.Same;;
		}

		protected static string PSig(ParameterInfo[] pa) {
			string result = "[";
			for(int i = 0; i < pa.Length; i++)
				result += ((i>0)?", ":"") + (int)pa[i].Attributes + " " + pa[i].ParameterType.Name + " " + pa[i].Name;
			return result + "]";
		}

		public static string ToString(PropertyInfo pi) { return (new GenPropertyInfo(pi)).ToString(); }
		public override string ToString() {
			string temp = null;
			string result = "Property: ";
			result += ((temp = ToString(_modifiers)) != "") ? temp + " " : "";
			result += _propertytype.FullName;
			result += (_propertykind == PropertyKinds.Indexer) ? "this" + ToString(_indexparams) : _name;
			result += " { ";

//MODIFIED, mod0058
//#if BETA1
			result += ((_getter != null) ? Enum.GetName(typeof(AccessModifiers), _getaccess) + " get; " : "");
//#else
//			result += ((_getter != null) ? ((Enum)_getaccess).ToString() + " get; " : "");
//#endif

//MODIFIED, mod0059
//#if BETA1
			result += ((_setter != null) ? Enum.GetName(typeof(AccessModifiers), _setaccess) + " set; " : "");
//#else
//			result += ((_setter != null) ? ((Enum)_setaccess).ToString() + " set; " : "");
//#endif

			result += "}";
			return result;
		}

		public static string ToHtml(PropertyInfo pi) { return (new GenPropertyInfo(pi)).ToHtml(); }
		public override string ToHtml() {
			return ToString();
		}

		public static string ToString(PropertyModifiers [] modifiers) {
			string result = String.Empty;
			for(int i = 0; i < modifiers.Length; i++) {

//MODIFIED, mod0060
//#if BETA1
				result += ((i > 0) ? " " : "") + Enum.GetName(typeof(PropertyModifiers), modifiers[i]);
//#else
//				result += ((i > 0) ? " " : "") + ((Enum)modifiers[i]).ToString();
//#endif
			}

			return result;
		}

		public static string ToString(ParameterInfo [] pa) {
			string result = "[";
			for(int i = 0; i < pa.Length; i++)
				result += ((i > 0) ? ", " : "") + GenParameterInfo.ToString(pa[i]);
			return result + "]";
		}

		public static void Dump(PropertyInfo pi) { (new GenPropertyInfo(pi)).Dump(String.Empty); }
		public static void Dump(PropertyInfo pi, string pre) { (new GenPropertyInfo(pi)).Dump(pre); }
		public override void Dump() { Dump(String.Empty); }
		public override void Dump(string pre) {
			base.Dump(pre);
			Console.Write(pre + "_modifiers =   ");
			for (int i = 0; i < _modifiers.Length; i++) {

//MODIFIED, mod0061
//#if BETA1
				Console.Write(((i > 0) ? " " : "") + Enum.GetName(typeof(PropertyModifiers), _modifiers[i]));
//#else
//				Console.Write(((i > 0) ? " " : "") + ((Enum)_modifiers[i]).ToString());
//#endif
			}

			Console.WriteLine();
			Console.WriteLine(pre + "_propertytype =   " + _propertytype.FullName);
			Console.WriteLine(pre + "_getter =         " + ((_getter != null) ? _getter.Name : "<null>"));

//MODIFIED, mod0062
//#if BETA1
			Console.WriteLine(pre + "_getaccess =      " + Enum.GetName(typeof(AccessModifiers), _getaccess));
//#else
//			Console.WriteLine(pre + "_getaccess =      " + ((Enum)_getaccess).ToString());
//#endif

			Console.WriteLine(pre + "_setter =         " + ((_setter != null) ? _setter.Name : "<null>"));

//MODIFIED, mod0063
//#if BETA1
			Console.WriteLine(pre + "_setaccess =      " + Enum.GetName(typeof(AccessModifiers), _setaccess));
//#else
//			Console.WriteLine(pre + "_setaccess =      " + ((Enum)_setaccess).ToString());
//#endif

			Console.Write(pre + "_indexparams = ");
			for (int i = 0; i < _indexparams.Length; i++)
				Console.Write(((i > 0) ? ", " : "") + _indexparams[i].ToString());
			Console.WriteLine();
		}
	}
}
