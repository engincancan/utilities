using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	public class CsConstructorInfo : GenConstructorInfo {

		//** Constructors
		//** Ctors
		public CsConstructorInfo (ConstructorInfo ctor) : base (ctor) {}
		public CsConstructorInfo (CsConstructorInfo ci) : base ((GenConstructorInfo)ci) {}

		//** Properties
		public override string Sig { get { return ToHtml(); } }

		//** Methods
		public new static string ToString(ConstructorInfo ci) { return (new CsConstructorInfo(ci)).ToString(); }
		public override string ToString() {
			string temp;
			string result = ToString(_methodkind) + ": ";											// MemberType
			result += ((temp = ToString(_access)) != String.Empty) ? temp + " " : String.Empty;		// Scope
			result += (_methodkind == MethodKinds.Ctor) ? _declaringtype.Name : _name;				// Name
			result += " " + ToString(_parameters, _varargs);										// Parameters
			return result;
		}
		public new static string ToHtml(ConstructorInfo ci) { return (new CsConstructorInfo(ci)).ToHtml(); }
		public override string ToHtml() {
			string temp;
			string result = ToHtml(_methodkind) + ": ";													// MemberType
			result += ((temp = ToHtml(_access)) != String.Empty) ? temp + " " : String.Empty;			// Scope
			result += "<b>" + ((_methodkind == MethodKinds.Ctor) ? _declaringtype.Name : _name) + "</b>";	// Name
			result += " " + ToHtml(_parameters, _varargs);												// Parameters
			return result;
		}

		// ** Helper Methods
		protected new static string ToString(MethodKinds kind) {
			MethodKinds temp = kind & MethodKinds.CtorMask;
			if (temp != 0 && temp != MethodKinds.UnknownCtor)
				return "Constructor";
			else
				return "UnknownConstructorType";
		}
		protected new static string ToHtml(MethodKinds kind) {
			MethodKinds temp = kind & MethodKinds.CtorMask;
			if (temp != 0 && temp != MethodKinds.UnknownCtor)
				return "Constructor";
			else
				return "<i>UnknownConstructorType</i>";
		}

		protected new static string ToString(AccessModifiers access) {
			switch(access) {
			case AccessModifiers.na:
				return String.Empty;
			case AccessModifiers.Unknown:
			case AccessModifiers.Unassigned:
				return "UnknownAccess";
			default:

//MODIFIED, mod0064
//#if BETA1
				return SigHelper.CsTranslate(Enum.GetName(typeof(AccessModifiers), access));
//#else
//				return SigHelper.CsTranslate(((Enum)access).ToString());
//#endif


			}
		}
		protected new static string ToHtml(AccessModifiers access) {
			switch(access) {
			case AccessModifiers.na:
				return String.Empty;
			case AccessModifiers.Unknown:
			case AccessModifiers.Unassigned:
				return "<i>UnknownAccess</i>";
			default:
//MODIFIED, mod0065
//#if BETA1
				return SigHelper.CsTranslate(Enum.GetName(typeof(AccessModifiers), access));
//#else
//				return SigHelper.CsTranslate(((Enum)access).ToString());
//#endif
			}
		}

		protected new static string ToString(GenParameterInfo [] parameters, bool varargs) {
			string result = "(";
			for(int i = 0; i < parameters.Length; i++)
				result += ((i > 0) ? ", " : "") + (new CsParameterInfo(parameters[i])).ToString();
			result += (varargs) ? ", __arglist" : "";
			result += ")";
			return result;
		}
		protected new static string ToHtml(GenParameterInfo [] parameters, bool varargs) {
			string result = "(";
			for(int i = 0; i < parameters.Length; i++)
				result += ((i > 0) ? ", " : "") + (new CsParameterInfo(parameters[i])).ToHtml();
			result += (varargs) ? ", <i>__arglist</i>" : "";
			result += ")";
			return result;
		}
	}
}