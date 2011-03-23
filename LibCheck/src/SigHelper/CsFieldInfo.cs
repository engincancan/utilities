using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	public class CsFieldInfo : GenFieldInfo {

		//** Fields
		//** Ctors
		public CsFieldInfo (FieldInfo field) : base (field) {}
		public CsFieldInfo (CsFieldInfo fi) : base ((GenFieldInfo)fi) {}

		//** Properties
		public override string Sig { get { return ToHtml(); } }

		//** Methods
		public new static string ToString(FieldInfo fi) { return (new CsFieldInfo(fi)).ToString(); }
		public override string ToString() {
			string temp = null;
			string result = "Field: ";																// MemberType
			result += ((temp = ToString(_access)) != String.Empty) ? temp + " " : String.Empty;		// Scope
			result += ((temp = ToString(_modifiers)) != String.Empty) ? temp + " " : String.Empty;	// Modifiers
			result += (_fieldkind == FieldKinds.Constant) ? "const " : "";							// const
			result += SigHelper.CsParse(_fieldtype.Name, true) + " ";								// Type
			result += _name;																		// Name
//			if (_value != null)																		// ? Value
//				result += " = " + _value.ToString();
			return result;
		}
		public new static string ToHtml(FieldInfo fi) { return (new CsFieldInfo(fi)).ToHtml(); }
		public override string ToHtml() {
			string temp = null;
			string result = "Field: ";																// MemberType
			result += ((temp = ToHtml(_access)) != String.Empty) ? temp + " " : String.Empty;		// Scope
			result += ((temp = ToHtml(_modifiers)) != String.Empty) ? temp + " " : String.Empty;	// Modifiers
			result += (_fieldkind == FieldKinds.Constant) ? "const " : "";							// const
			result += "<span title=\"" + _fieldtype.Namespace + "\">";								// Type
			result += SigHelper.CsParse(_fieldtype.Name, true) + "</span> ";
			result += "<b>" + _name + "</b>";														// Name
			return result;
		}

		protected static string ToString(AccessModifiers access) {
			switch(access) {
			case AccessModifiers.na:
				return String.Empty;
			case AccessModifiers.Unknown:
			case AccessModifiers.Unassigned:
				return "UnknownAccess";
			default:
//MODIFIED, mod0069
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
				return "<i>UnknownAccess</i>";
			default:
//MODIFIED, mod0070
//#if BETA1
				return SigHelper.CsTranslate(Enum.GetName(typeof(AccessModifiers), access));
//#else
//				return SigHelper.CsTranslate(((Enum)access).ToString());
//#endif
			}
		}

		protected new static string ToString(FieldModifiers [] modifiers) {
			string result = String.Empty;
			for(int i = 0; i < modifiers.Length; i++) {

//MODIFIED, mod0071
//#if BETA1
				result += ((i > 0) ? " " : "") + SigHelper.CsTranslate(Enum.GetName(typeof(FieldModifiers), modifiers[i]));
//#else
//				result += ((i > 0) ? " " : "") + SigHelper.CsTranslate(((Enum)modifiers[i]).ToString());
//#endif
			}

			return result;
		}
		protected static string ToHtml(FieldModifiers [] modifiers) { return ToString(modifiers); }
	}
}