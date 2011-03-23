using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {
	
	public enum ParameterModifiers : byte {
		ByRef,
		Out,
		OutAttribute
	}
	
	public class GenParameterInfo {
		
		//** Fields
		protected ParameterModifiers [] _modifiers = null;
		protected Type _type = null;
		protected string _name = String.Empty;
		protected object _defaultvalue;

		//** Ctors
		public GenParameterInfo(ParameterInfo parameter) {
			if (parameter == null)
				throw new ArgumentNullException();
			_modifiers = GetModifiers(parameter);
			_type = parameter.ParameterType;
			string temp = parameter.Name;
			_name = (temp == null) ? String.Empty : temp.Trim();
			_defaultvalue = parameter.DefaultValue;
		}

		public GenParameterInfo(GenParameterInfo pi) {
			if (pi == null)
				throw new ArgumentNullException();
			_modifiers = pi._modifiers;
			_type = pi._type;
			_name = pi._name;
			_defaultvalue = pi._defaultvalue;
		}

		//** Properties
		public virtual string Name { get { return _name; } }
		public virtual string Sig { get { return ToString(); } }
		public virtual string Key { get { return ToKey(); } }

		//** Methods
		// Modifier retrieval for constructor
		protected static ParameterModifiers [] GetModifiers(ParameterInfo parameter) {
			ArrayList result = new ArrayList(1);




			if (parameter.IsOut && parameter.ParameterType.IsByRef) {
				result.Add(ParameterModifiers.Out);
			}
			else if (parameter.IsOut) {
				result.Add(ParameterModifiers.OutAttribute);
			}

/*
			if (parameter.IsOut)
				result.Add(ParameterModifiers.Out);
			else if (parameter.ParameterType.ToString().EndsWith("&"))
				result.Add(ParameterModifiers.ByRef);
*/

			return (ParameterModifiers [])result.ToArray(Type.GetType("SigHelper.ParameterModifiers"));
		}

		// Human readable text for single parameter
		// [modifier][ modifier]... type name[ = default value]
		public static string ToString(ParameterInfo pi) { return (new GenParameterInfo(pi)).ToString(); }
		public override string ToString() {
			string temp = ToString(_modifiers);																			// Modifiers
			string result = (temp != String.Empty) ? temp + " " : String.Empty;
//THIS IS WHERE WE WOULD GET THE ASSEMBLY NAME...
//			temp = _type.Assembly.ToString() + "." + _type.ToString();
			temp = _type.ToString();																					// Type
			int count = temp.Length - 1;
			result += (temp[count] == '&') ? temp.Substring(0,count) : temp;
			result += (_name != String.Empty) ? " " + _name : "";														// Name
//			result += (_defaultvalue != null && (temp = _defaultvalue.ToString()) != String.Empty) ? " = " + temp : "";	// Default value TODO: fix.

			return result;
		}

		public string ToString(bool getFullVal) {
			string temp = ToString(_modifiers);																			// Modifiers
			string result = (temp != String.Empty) ? temp + " " : String.Empty;
//THIS IS WHERE WE WOULD GET THE ASSEMBLY NAME...
//			temp = _type.Assembly.ToString() + "." + _type.ToString();
			temp = _type.ToString();		
				// Type
			int count = temp.Length - 1;
			result += (temp[count] == '&') ? temp.Substring(0,count) : temp;
			result += (_name != String.Empty) ? " " + _name : "";														// Name
//			result += (_defaultvalue != null && (temp = _defaultvalue.ToString()) != String.Empty) ? " = " + temp : "";	// Default value TODO: fix.
//Console.WriteLine(result);
//Console.ReadLine();
			return result;
		}


		// Human readable HTML for single parameter
		public static string ToHtml(ParameterInfo pi) { return (new GenParameterInfo(pi)).ToHtml(); }
		public virtual string ToHtml() { return ToString(); }
		// Machine readable key for single parameter
		// name[ modifier]...
		public static string ToKey(ParameterInfo pi) { return (new GenParameterInfo(pi)).ToKey(); }
		public string ToKey() {
//THIS IS WHERE WE WOULD GET THE ASSEMBLY NAME... ALREADY DONE?
			string result = _type.FullName;
			for (int i = 0; i < _modifiers.Length; i++) {

//MODIFIED, mod0051
//#if BETA1
				result += " " + Enum.GetName(typeof(ParameterModifiers), _modifiers[i]);
//#else
//				result += " " + ((Enum)_modifiers[i]).ToString();
//#endif
			}

			return result;
		}

		// Human readable text for parameter list
		// [parameterText[, parameterText]...[, __arglist]]
		public static string ToString(MethodBase method) { return ToString(method.GetParameters(), method.CallingConvention); }
		public static string ToString(ParameterInfo[] pa) { return ToString(pa, CallingConventions.Standard); }
		public static string ToString(ParameterInfo[] pa, CallingConventions cc) {
			string result = String.Empty;
			for(int i = 0; i < pa.Length; i++)
				result += ((i > 0) ? ", " : "") + ToString(pa[i]);
			result += ((cc & CallingConventions.VarArgs) != 0) ? ", __arglist" : "";
			return result;
		}
		public static string ToString(GenParameterInfo[] pa, bool varargs) {
			string result = String.Empty;
			for(int i = 0; i < pa.Length; i++)
				result += ((i > 0) ? ", " : "") + pa[i].ToString();
			result += (varargs) ? ", __arglist" : "";
			return result;
		}
		// Human readable HTML for parameter list
		// [parameterHtml[, parameterHtml]...[, __arglist]]
		public static string ToHtml(MethodBase method) { return ToHtml(method.GetParameters(), method.CallingConvention); }
		public static string ToHtml(ParameterInfo[] pa) { return ToHtml(pa, CallingConventions.Standard); }
		public static string ToHtml(ParameterInfo[] pa, CallingConventions cc) {
			string result = String.Empty;
			for(int i = 0; i < pa.Length; i++)
				result += ((i > 0) ? ", " : "") + ToHtml(pa[i]);
			result += ((cc & CallingConventions.VarArgs) != 0) ? ", __arglist" : "";
			return result;
		}
		public static string ToHtml(GenParameterInfo[] pa, bool varargs) {
			string result = String.Empty;
			for(int i = 0; i < pa.Length; i++)
				result += ((i > 0) ? ", " : "") + pa[i].ToHtml();
			result += (varargs) ? ", __arglist" : "";
			return result;
		}
		// Human readable HTML for parameter list
		// Machine readable key for parameter list
		// [parameterKey[,parameterKey]...[,__arglist]]
		public static string PSig(MethodBase method) { return PSig(method.GetParameters(), method.CallingConvention); }
		public static string PSig(ParameterInfo[] pa) { return PSig(pa, CallingConventions.Standard); }
		public static string PSig(ParameterInfo[] pa, CallingConventions cc) {
			string result = String.Empty;
			for(int i = 0; i < pa.Length; i++) {
				string aName = (pa[i]).ParameterType.Assembly.ToString();
				int found = (pa[i]).ParameterType.Assembly.ToString().IndexOf(",");

				result += ((i > 0) ? "," : "") + aName.Substring(0, ((found >= 0) ? found : aName.Length)) + "." + ToKey(pa[i]);
			}
//CHANGED
//				result += ((i > 0) ? "," : "") + ToKey(pa[i]);
			result += ((cc & CallingConventions.VarArgs) != 0) ? ",__arglist" : "";
			return result;
		}
		public static string PSig(GenParameterInfo[] pa, bool varargs) {
			string result = String.Empty;
			for(int i = 0; i < pa.Length; i++)
				result += ((i > 0) ? "," : "") + pa[i].ToKey();
			result += (varargs) ? ",__arglist" : "";
			return result;
		}

		// Helper functions for human readable output methods
		private static string ToString(ParameterModifiers [] modifiers) {
			string result = String.Empty;
			for(int i = 0; i < modifiers.Length; i++) {

//MODIFIED, mod0052
//#if BETA1
				result += ((i > 0) ? " " : "") + Enum.GetName(typeof(ParameterModifiers), modifiers[i]);
//#else
//				result += ((i > 0) ? " " : "") + ((Enum)modifiers[i]).ToString();
//#endif
			}

			return result;
		}
		private static string ToHtml(ParameterModifiers [] modifiers) { return ToString(modifiers); }
	}
}