using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	public class CsParameterInfo : GenParameterInfo {

		//** Constructors
		public CsParameterInfo(ParameterInfo parameter) : base(parameter) {}
		public CsParameterInfo(GenParameterInfo pi) : base(pi) {}

		//** Methods
		public new static string ToString(ParameterInfo pi) { return (new CsParameterInfo(pi)).ToString(); }
		public override string ToString() {
			string temp = ToString(_modifiers);
			string result = (temp != String.Empty) ? temp + " " : String.Empty;								// Modifiers
			result += SigHelper.CsParse(_type.Name, true);													// Type
			result += (_name != String.Empty) ? " " + _name : "";											// Name
//			result += (_defaultvalue != null && (temp = _defaultvalue.ToString()) != String.Empty) ? " = " + temp : "";	// Default value TODO: fix.
			return result;
		}
		public new static string ToHtml(ParameterInfo pi) { return (new CsParameterInfo(pi)).ToHtml(); }
		public override string ToHtml() {
			string temp = ToHtml(_modifiers);
			string result = (temp != String.Empty) ? temp + " " : String.Empty;											// Modifiers
			result += "<span title=\"" + _type.Namespace + "\">" + SigHelper.CsParse(_type.Name, true) + "</span>";		// Type
			result += (_name != String.Empty) ? " <i>" + _name + "</i>" : "";											// Name
//			result += (_defaultvalue != null && (temp = _defaultvalue.ToString()) != String.Empty) ? " = " + temp : "";	// Default value TODO: fix.
			return result;
		}

		private static string ToString(ParameterModifiers [] modifiers) {
			string result = String.Empty;

			for(int i = 0; i < modifiers.Length; i++) {

//MODIFIED, mod0075
//#if BETA1
				result += ((i > 0) ? " " : "") + SigHelper.CsTranslate(Enum.GetName(typeof(ParameterModifiers), modifiers[i]));
//#else
//				result += ((i > 0) ? " " : "") + SigHelper.CsTranslate(((Enum)modifiers[i]).ToString());
//#endif
			}


			return result;
		}
		// for now, no difference between modifiers for HTML and for simple text.
		private static string ToHtml(ParameterModifiers [] modifiers) { return ToString(modifiers); }
	}
}