using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	public class GenConstructorInfo : GenMethodBase {

		//** Fields
		protected MethodModifiers [] _modifiers = null;

		//** Ctors
		public GenConstructorInfo (ConstructorInfo ctor) : base ((MethodBase)ctor) {
			_modifiers = GetModifiers(ctor);
		}

		public GenConstructorInfo (GenConstructorInfo ci) : base ((GenMethodBase)ci) {
			_modifiers = ci._modifiers;
		}

		//** Properties
		public override string Key { get { return ToKey(); } }
		public override string ShortKey { get { return ToShortKey(); } }
		public override string Sig { get { return ToString(); } }

		//** Methods
		private static MethodModifiers [] GetModifiers(MethodBase ctor) {
			ArrayList result = new ArrayList(2);

			if (ctor.IsStatic)
				result.Add(MethodModifiers.Static);
			
			if (ctor.IsAbstract)
				result.Add(MethodModifiers.Abstract);

			if (ctor.IsVirtual)
				result.Add(MethodModifiers.Virtual);

			// How can we tell when this is external? result.Add(MethodModifiers.External);
			
			return (MethodModifiers [])result.ToArray(Type.GetType("SigHelper.MethodModifiers"));
		}

		public static string ToString(ConstructorInfo ci) {
			return (new GenConstructorInfo(ci)).ToString();
		}
		public override string ToString() {
			string temp;
			string result = ToString(_methodkind) + ": ";
			result += ((temp = ToString(_access)) != String.Empty) ? temp + " " : String.Empty;
			result += ((temp = ToString(_modifiers)) != String.Empty) ? temp + " " : String.Empty;
			result += (_methodkind == MethodKinds.CCtor) ? _name : _declaringtype.Name;
			result += " " + ToString(_parameters, _varargs);
			return result;
		}

		public static string ToHtml(ConstructorInfo ci) { return (new GenConstructorInfo(ci)).ToHtml(); }
		public override string ToHtml() { return ToString(); }

		public override string ToKey() {

			// consistent MemberInfo information
//MODIFIED, mod0033
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

//MODIFIED, mod0034
//#if BETA1
				result += ((i>0) ? "," : "") + Enum.GetName(typeof(MethodModifiers), _modifiers[i]);
//#else
//				result += ((i>0) ? "," : "") + ((Enum)_modifiers[i]).ToString();
//#endif
			}

			result += ":" +
				"Parameters=" + PSig((ConstructorInfo)_memberinfo);
			return result;
		}

		public override string ToShortKey() {
			string result =
				base.ToShortKey() +	":" +							// consistent MemberInfo information
				"Parameters=" + PSig((ConstructorInfo)_memberinfo);
			return result;
		}

		// check changes for Constructors
		public static CompareResults EvalChange(Hashtable okey, Hashtable nkey, bool oldAbst, bool newAbst) {
			if (okey["MemberType"] == null && nkey["MemberType"] == null)
				throw new ArgumentException("Error in one or more of the keys, no MemberType found.");
			if ((okey["MemberType"] != null && (string)okey["MemberType"] != "Constructor") || (nkey["MemberType"] != null && (string)nkey["MemberType"] != "Constructor"))
				throw new ArgumentException("Keys are not for the correct member type.");

			bool changed = false;									// bookkeeping flags
			CompareResults temp = CompareResults.Same;

			if (okey.Count > 0 && nkey.Count > 0) {					// both present
				if ((string)okey["MemberName"] != (string)nkey["MemberName"]
					|| (string)okey["Parameters"] != (string)nkey["Parameters"])	// compare name and parameter list (assumed members of same type)
					return CompareResults.Different;
				if ((string)okey["Scope"] != (string)nkey["Scope"]) {				// change in Scope
					if (IsBreakingScopeChange((string)okey["Scope"], (string)nkey["Scope"])) return CompareResults.Breaking;
					else changed = true;
				}
				if ((string)okey["Abstract"] != (string)nkey["Abstract"]) {			// change in abstract'ness
					if ((string)nkey["Abstract"] == "T" && newAbst)
						return CompareResults.Breaking;
					else
						return CompareResults.NonBreaking;
				}
				if ((string)okey["Modifiers"] != (string)nkey["Modifiers"])			// change in modifiers
					temp = CompareModifiers((string)okey["Modifiers"], (string)nkey["Modifiers"]);

				if (temp == CompareResults.Breaking)				// evaluate the fallout of all this.
					return CompareResults.Breaking;
				else if (temp == CompareResults.Same && !changed)
					return CompareResults.Same;
				else
					return CompareResults.NonBreaking;
			}
			else if (nkey.Count > 0) {								// represents an added member
				if ((string)nkey["Abstract"] == "T" && newAbst)				// only breaking if abstractmember of abstract type
					return CompareResults.Breaking;
				else
					return CompareResults.NonBreaking;
			}
			else													// represents a removed member, always breaking
				return CompareResults.Breaking;
		}
	}
}