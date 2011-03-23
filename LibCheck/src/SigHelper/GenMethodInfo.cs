using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {
	
	public class GenMethodInfo : GenMethodBase {

// ** Constants
//MODIFIED, mod5005
		private const BindingFlags allBindingsLookup = BindingFlags.Public | 
				BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

		//** Fields
		protected MethodModifiers [] _modifiers = null;
		protected Type _returntype = null;
		
		//** Ctors
		public GenMethodInfo (MethodInfo method) : base ((MethodBase)method) {
			// for some reason I'm losing new'd virtual methods of abstract types.
			// also, I'm not even checking multiple inheritance.
			_neworoverriden = false;
			if (_declaringbasetype != null && _declaringtype == _reflectedtype) {

//MODIFIED, mod0002
//				MemberInfo [] ma = //method.ReflectedType.BaseType.GetMember(_name,_membertype,BindingFlags.LookupAll);
				MemberInfo [] ma = 
						method.ReflectedType.BaseType.GetMember(_name,_membertype,allBindingsLookup);

				foreach (MethodInfo mi in ma) {
					// check if method sigs match on both access and parameters.
					if (GetAccess(mi) == GetAccess(method) && PSig(mi) == PSig(method)) {
						_neworoverriden = true;
						break;
					}
				}
			}
			if (_neworoverriden) 
				_inherited = false;

			_modifiers = GetModifiers(method, _neworoverriden);
			_returntype = method.ReturnType;
		}

		public GenMethodInfo (GenMethodInfo mi) : base((GenMethodBase)mi) {
			_modifiers = mi._modifiers;
			_returntype = mi._returntype;
		}

		//** Properties
		public Type ReturnType { get { return _returntype; } }
		public override string Key { get { return ToKey(); } }
		public override string ShortKey { get { return ToShortKey(); } }
		public override string Sig { get { return ToString(); } }

		//** Methods
		public override string ToKey() {

			// consistent MemberInfo information
//MODIFIED, 0025
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
//MODIFIED, 0026
//#if BETA1
				result += ((i>0) ? "," : "") + Enum.GetName(typeof(MethodModifiers), _modifiers[i]);
//#else
//				result += ((i>0) ? "," : "") + ((Enum)_modifiers[i]).ToString();
//#endif
			}

			string aName = _returntype.Assembly.ToString();
			int found = aName.IndexOf(",");

			aName = aName.Substring(0, ((found >= 0) ? found : aName.Length)) + ".";

			result += ":" +
				"ReturnType=" + aName + _returntype.FullName + ":";
//HERE IS WHERE I COULD ADD EXTRA INFO FOR RETURN_TYPE ASSEMBLY
//robvi
			result += "ReturnBaseType=" + GetBases(_returntype); 

//			while(_returntype.BaseType!=null)
//			{
//					+ _returntype.BaseType.FullName + ":" +
//			}
//
//			




			result += ":" + "Parameters=" + PSig((MethodInfo)_memberinfo);
//HERE IS WHERE I COULD ADD EXTRA INFO FOR PARAM_ASSEMBLIES
			return result;
		}

		public override string ToShortKey() {
			string result =
				base.ToShortKey() +	":" +		// consistent MemberInfo information
				"Parameters=" + PSig((MethodInfo)_memberinfo);
			return result;
		}

		// check changes for Methods
		public static CompareResults EvalChange(Hashtable okey, Hashtable nkey, bool oldAbst, bool newAbst, bool typeAdded) {

			if (okey["MemberType"] == null && nkey["MemberType"] == null)
				throw new ArgumentException("Error in one or more of the keys, no MemberType found.");

			if ((okey["MemberType"] != null && (string)okey["MemberType"] != "Method") || 
					(nkey["MemberType"] != null && (string)nkey["MemberType"] != "Method"))
				throw new ArgumentException("Keys are not for the correct member type.");

			bool changed = false;	// bookkeeping flags

			CompareResults temp = CompareResults.Same;

			if (okey.Count > 0 && nkey.Count > 0) {	// both present

//Console.WriteLine((string)okey["Parameters"]);
//Console.ReadLine();
//P1: WE ARE NOT CAPTURING A SIMPLE RENAMING OF A PARAMETER!!!
/*
if ((string)okey["MemberName"] == "IndexOf") {
	Console.WriteLine((string)okey["MemberName"]);
//	for (int i = 0;i<okey.Count;i++)
	foreach (object o in okey.Values)
		Console.WriteLine(o);
	Console.WriteLine((string)okey["Parameters"]);
	Console.ReadLine();
}
*/

				// compare name and parameter list (assumed members of same type)
				if ((string)okey["MemberName"] != (string)nkey["MemberName"]
						|| (string)okey["Parameters"] != (string)nkey["Parameters"]) {
					//figure if it's breaking!
//Console.WriteLine("p1");
					return CompareResults.Breaking;
//				} else {
//Console.WriteLine("p2: {0}", okey["Parameters"]);
				}

				if ((string)okey["Scope"] != (string)nkey["Scope"]) {	// change in scope

				    if (IsBreakingScopeChange((string)okey["Scope"], (string)nkey["Scope"]))
					return CompareResults.Breaking;
				    else
					changed = true;
				}

				// change in return type. TODO: figure out if new type is assignable from old type.
				//robvi
				//we want to check of the return base type has changed but only flag it as breaking
				//if it doesn't inherit from the old type.
				if ((string)okey["ReturnType"] != (string)nkey["ReturnType"])
				{
					if(((string)nkey["ReturnBaseType"]).IndexOf((string)okey["ReturnBaseType"])==-1)																		  
						return CompareResults.Breaking;
					else
						return CompareResults.NonBreaking;
				}

				// change in abstract'ness
				if ((string)okey["Abstract"] != (string)nkey["Abstract"]) {

					if ((string)nkey["Abstract"] == "T" && newAbst)
						return CompareResults.Breaking;
					else
						return CompareResults.NonBreaking;
				}

				// change in modifiers
				if ((string)okey["Modifiers"] != (string)nkey["Modifiers"])
					temp = CompareModifiers((string)okey["Modifiers"], (string)nkey["Modifiers"]);

				// evaluate the fallout of all this.
				if (temp == CompareResults.Breaking)
					return CompareResults.Breaking;
				else if (temp == CompareResults.Same && !changed)
					return CompareResults.Same;
				else
					return CompareResults.NonBreaking;

			} else if (nkey.Count > 0) {	// represents an added member
//Console.WriteLine("p1");
				// only breaking if abstractmember of abstract type
//NO. Breaking also if this is an INTERFACE, and we are adding a method...



				if ((string)nkey["Abstract"] == "T" && newAbst && typeAdded == false)	
					return CompareResults.Breaking;
				else
					return CompareResults.NonBreaking;
			}
			else // represents a removed member, always breaking
				return CompareResults.Breaking;
		}


		private static MethodModifiers [] GetModifiers(MethodBase method, bool neworoverriden) {
			ArrayList result = new ArrayList(2);

			if (neworoverriden) {
				if ((method.Attributes & MethodAttributes.NewSlot) != 0)
					result.Add(MethodModifiers.New);
				else if ((method.Attributes & MethodAttributes.Virtual) != 0)
					result.Add(MethodModifiers.Override);
				else
					result.Add(MethodModifiers.New);
			}
			
			if (method.IsStatic)
				result.Add(MethodModifiers.Static);
			
			if (method.IsAbstract							// TODO: logic that doesn't display redundant modifiers needs to move to CsMethodInfo (breaks key logic)
					&& !method.ReflectedType.IsInterface)
				result.Add(MethodModifiers.Abstract);

			if (method.IsVirtual
					&& !method.ReflectedType.IsInterface
					&& !result.Contains(MethodModifiers.Abstract)
					&& !result.Contains(MethodModifiers.Override))
				result.Add(MethodModifiers.Virtual);

			// How can we tell when this is external? result.Add(MethodModifiers.External);
			
			return (MethodModifiers [])result.ToArray(Type.GetType("SigHelper.MethodModifiers"));
		}

		public static string ToString(MethodInfo mi) { return (new GenMethodInfo(mi)).ToString(); }
		public override string ToString() {
			string temp;
			string result = ToString(_methodkind);
			result += (_inherited) ? " " + this.InheritedString() + ": " : ": ";
			result += ((temp = ToString(_access)) != String.Empty) ? temp + " " : String.Empty;
			result += ((temp = ToString(_modifiers)) != String.Empty) ? temp + " " : String.Empty;
			result += _name + " ";
			result += ToString(_parameters, _varargs);
			return result;
		}
	
		//robvi
		//returns a comma separated list of the types this type inherits from

		private string GetBases(Type type)
		{	
			if(type.BaseType==null)	//this is only true if the type is System.Object
				return type.FullName;
			else
				return(type.FullName + "," + GetBases(type.BaseType));
		}
	}
}