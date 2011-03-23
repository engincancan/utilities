using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	public class CsPropertyInfo : GenPropertyInfo {

		//** Fields
		//** Ctors
		public CsPropertyInfo (PropertyInfo property, bool flag) : base (property, flag) {}
		public CsPropertyInfo (PropertyInfo property) : base (property, false) {}
		public CsPropertyInfo (CsPropertyInfo pi) : base((GenPropertyInfo)pi) {}

		//** Properties
		public override string Sig { get { return ToHtml(); } }

		//** Methods
		public new static string ToString(PropertyInfo pi) { return (new CsPropertyInfo(pi)).ToString(); }
		public override string ToString() {
			string temp = null;
			string result = "Property: ";																		// MemberType
			result += ((temp = ToString(_modifiers)) != String.Empty) ? temp + " " : String.Empty;				// Modifiers
			result += SigHelper.CsTranslate(_propertytype.Name) + " ";											// Type
			temp = (_name == null || _name == String.Empty) ? "UnknownName" : _name;							// Name
			result += (_propertykind == PropertyKinds.Indexer) ? "this " + ToString(_indexparams) : temp;
			result += " { ";																					// Accessors
			if (_getter != null)
				result += (((temp = ToString(_getaccess)) != String.Empty) ? temp + " " : "") + "get; ";		// Getter
			if (_setter != null)
				result += (((temp = ToString(_setaccess)) != String.Empty) ? temp + " " : "") + "set; ";		// Setter
			result += "}";
			return result;
		}
		public new static string ToHtml(PropertyInfo pi) { return (new CsPropertyInfo(pi)).ToHtml(); }
		public override string ToHtml() {
			string temp = null;
			string result = "Property: ";																						// MemberType
			result += ((temp = ToHtml(_modifiers)) != String.Empty) ? temp + " " : String.Empty;								// Modifiers
			result += "<span title=\"" + _propertytype.Namespace + "\">" + SigHelper.CsTranslate(_propertytype.Name) + "</span> ";	// Type
			temp = (_name == null || _name == String.Empty) ? "<i>UnknownName</i>" : _name;										// Name
			result += (_propertykind == PropertyKinds.Indexer) ? "<b>this</b> " + ToHtml(_indexparams) : "<b>" + temp + "</b>";
			result += " { ";																									// Accessors
			if (_getter != null)
				result += (((temp = ToString(_getaccess)) != String.Empty) ? temp + " " : "") + "get; ";						// Getter
			if (_setter != null)
				result += (((temp = ToString(_setaccess)) != String.Empty) ? temp + " " : "") + "set; ";						// Setter
			result += "}";
			return result;
		}

		public new static string ToString(ParameterInfo [] pa) {
			string result = "[";
			for(int i = 0; i < pa.Length; i++)
				result += ((i > 0) ? ", " : "") + CsParameterInfo.ToString(pa[i]);
			return result + "]";
		}
		public static string ToHtml(ParameterInfo [] pa) {
			string result = "[";
			for(int i = 0; i < pa.Length; i++)
				result += ((i > 0) ? ", " : "") + (new CsParameterInfo(pa[i])).ToHtml();
			return result + "]";
		}

		protected static string ToString(AccessModifiers access) {
			switch(access) {
			case AccessModifiers.na:
				return String.Empty;
			case AccessModifiers.Unknown:
			case AccessModifiers.Unassigned:
				return "UnknownScope";
			default:
//MODIFIED, mod0076
//#if BETA1
				return SigHelper.CsTranslate(Enum.GetName(typeof(AccessModifiers), access));
//#else
//				return SigHelper.CsTranslate(((Enum)access).ToString());
//#endif
			}
		}
		protected static string ToHtml(AccessModifiers access) {
			switch(access) {
			case AccessModifiers.na:
				return String.Empty;
			case AccessModifiers.Unknown:
			case AccessModifiers.Unassigned:
				return "<i>UnknownScope</i>";
			default:
//MODIFIED, mod0077
//#if BETA1
				return SigHelper.CsTranslate(Enum.GetName(typeof(AccessModifiers), access));
//#else
//				return SigHelper.CsTranslate(((Enum)access).ToString());
//#endif
			}
		}

		protected new static string ToString(PropertyModifiers [] modifiers) {
			string result = String.Empty;
			for(int i = 0; i < modifiers.Length; i++) {
				result += ((i > 0) ? " " : "");

//MODIFIED, mod0078
//#if BETA1
				result += ((modifiers[i] & (PropertyModifiers.na | PropertyModifiers.Unknown)) > 0)
					? "UnknownModifier2" : SigHelper.CsTranslate(Enum.GetName(typeof(PropertyModifiers), modifiers[i])) ;

//#else
//				result += ((modifiers[i] & (PropertyModifiers.na | PropertyModifiers.Unknown)) > 0)
//					? "UnknownModifier" : SigHelper.CsTranslate(((Enum)modifiers[i]).ToString()) ;
//#endif
			}
			return result;
		}
		protected static string ToHtml(PropertyModifiers [] modifiers) {
			string result = String.Empty;
			for(int i = 0; i < modifiers.Length; i++) {
				result += ((i > 0) ? " " : "");

//MODIFIED, mod0079
//#if BETA1
//if (((modifiers[i] & (PropertyModifiers.na | PropertyModifiers.Unknown)) > 0)) {
//	Console.WriteLine("p1: " + modifiers[i]);
//	Console.ReadLine();
//}

				result += ((modifiers[i] & (PropertyModifiers.na | PropertyModifiers.Unknown)) > 0)
					? "<i>UnknownModifier3</i>" : SigHelper.CsTranslate(Enum.GetName(typeof(PropertyModifiers), modifiers[i])) ;


//#else
//				result += ((modifiers[i] & (PropertyModifiers.na | PropertyModifiers.Unknown)) > 0)
//					? "<i>	Modifier</i>" : //SigHelper.CsTranslate(((Enum)modifiers[i]).ToString()) ;
//#endif
			}
			return result;
		}
	}
}