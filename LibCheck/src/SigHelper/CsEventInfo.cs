using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	public class CsEventInfo : GenEventInfo {

		//** Fields
		//** Ctors
		public CsEventInfo (EventInfo ev) : base (ev) {}
		public CsEventInfo (CsEventInfo ev) : base ((GenEventInfo)ev) {}

		//** Properties
		public override string Sig { get { return ToHtml(); } }

		//** Methods
		public new static string ToString(EventInfo ev) { return (new CsEventInfo(ev)).ToString(); }
		public override string ToString() {
			string temp = null;
			string result = ToString(_eventkind) + ": ";												// MemberType
			result += ((temp = ToString(_access)) != String.Empty) ? temp + " " : String.Empty;			// Scope
			result += ((temp = ToString(_modifiers)) != String.Empty) ? temp + " " : String.Empty;		// Modifiers
			result += "event ";																			// keyword
			result += SigHelper.CsParse(_handlertype.Name) + " ";										// Type
			result += _name;																			// Name
			if (_eventkind == EventKinds.Property)
				result += " { get; set; }";																// Accessors
			return result;
		}
		public new static string ToHtml(EventInfo ev) { return (new CsEventInfo(ev)).ToHtml(); }
		public override string ToHtml() {
			string temp = null;
			string result = ToHtml(_eventkind) + ": ";													// MemberType
			result += ((temp = ToHtml(_access)) != String.Empty) ? temp + " " : String.Empty;			// Scope
			result += ((temp = ToHtml(_modifiers)) != String.Empty) ? temp + " " : String.Empty;		// Modifiers
			result += "event ";																			// keyword
			result += "<span title=\"" + _handlertype.Namespace + "\">";								// Type
			result += SigHelper.CsParse(_handlertype.Name) + "</span> ";
			result += "<b>" + _name + "</b>";															// Name
			if (_eventkind == EventKinds.Property)
				result += " { get; set; }";																// Accessors
			return result;
		}

		//** Helper Methods
		protected static string ToString(EventKinds kind) {
			if (kind != EventKinds.Unassigned && kind != EventKinds.Unknown)
				return "Event";
			else
				return "UnknownEventType";
		}
		protected static string ToHtml(EventKinds kind) {
			if (kind != EventKinds.Unassigned && kind != EventKinds.Unknown)
				return "Event";
			else
				return "<i>UnknownEventType</i>";
		}

		protected static string ToString(AccessModifiers access) {
			switch(access) {
			case AccessModifiers.na:
				return String.Empty;
			case AccessModifiers.Unknown:
			case AccessModifiers.Unassigned:
				return "UnknownAccess";
			default:
//MODIFIED, mod0066
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
//MODIFIED, mod0067
//#if BETA1
				return SigHelper.CsTranslate(Enum.GetName(typeof(AccessModifiers), access));
//#else
//				return SigHelper.CsTranslate(((Enum)access).ToString());
//#endif
			}
		}

		protected new static string ToString(EventModifiers [] modifiers) {
			string result = String.Empty;

			for(int i = 0; i < modifiers.Length; i++) {

//MODIFIED, mod0068
//#if BETA1
				result += ((i > 0) ? " " : "") + SigHelper.CsTranslate(Enum.GetName(typeof(EventModifiers), modifiers[i]));
//#else
//				result += ((i > 0) ? " " : "") + SigHelper.CsTranslate(((Enum)modifiers[i]).ToString());
//#endif
			}

			return result;
		}
		protected new static string ToHtml(EventModifiers [] modifiers) { return ToString(modifiers); }
	}
}