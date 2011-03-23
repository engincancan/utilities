using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	// Note: Event add_x() and remove_x() are not accounted for yet, but these are not necessary for LibCheck.

	public enum MethodKinds : int {
		Unassigned = 0x0000,
			Unknown		= 0x8000,
			Ctor		= 0x0001,
			CCtor		= 0x0002,
			UnknownCtor	= 0x0003,
			CtorMask	= 0x0003,
			Method		= 0x0004,
			Accessor	= 0x0008,
			Indexer		= 0x0010,
			AccessorMask = 0x0018,
			UnaryOp		= 0x0020,
			BinaryOp	= 0x0040,
			ConversionOp = 0x0060,
			OperatorMask = 0x0060,
			MethodMask	= 0x007d,
	}

	public enum MethodModifiers : byte {
		Unknown = 0x80,
			New = 1,
			Override,
			Static,
			Abstract,
			Virtual,
			Extern,
	}
	
	public class GenMethodBase : GenMemberInfo {

		//** Fields
		protected bool _specialname = false;
		protected MethodKinds _methodkind;
		protected AccessModifiers _access = AccessModifiers.Unassigned;
		protected GenParameterInfo [] _parameters = null;
		protected bool _varargs = false;
		protected int _attributes = 0;

		//** Constructors
		public GenMethodBase(MethodBase method) : base((MemberInfo)method) {
			_specialname = method.IsSpecialName;
			_methodkind = GetMethodKind(method);
			_access = GetAccess(method);
			_parameters = GetParameters(method);
			_varargs = ((method.CallingConvention & CallingConventions.VarArgs) != 0) ? true : false;
			_attributes = (int)method.Attributes;
			_abstract = method.IsAbstract;
		}
		
		public GenMethodBase(GenMethodBase mb) : base((GenMemberInfo) mb) {
			_specialname = mb._specialname;
			_methodkind = mb._methodkind;
			_access = mb._access;
			_parameters = mb._parameters;
			_varargs = mb._varargs;
			_attributes = mb._attributes;
		}

		//** Properties
		public bool IsSpecialName { get { return _specialname; } }
		public bool HasVarArgs { get { return _varargs; } }

		//** Methods
		// Method kind retrieval method for constructor
		protected static MethodKinds GetMethodKind(MethodBase method) {
			string name = method.Name;
			if ((method.MemberType & MemberTypes.Constructor) != 0) {
				if (name == ConstructorInfo.ConstructorName)
					return MethodKinds.Ctor;
				else if (name == ConstructorInfo.TypeConstructorName)
					return MethodKinds.CCtor;
				else
					return MethodKinds.UnknownCtor;
			}

			if (!method.IsSpecialName)
				return MethodKinds.Method;

			int args = method.GetParameters().Length;
			bool getter = name.StartsWith("get_");
			bool setter = name.StartsWith("set_");

			if ( getter || setter) {
				if ((getter && args > 0) || (setter && args > 1))
					return MethodKinds.Indexer;
				else
					return MethodKinds.Accessor;
			}
			else if (name.StartsWith("op_")) {
				if (name == "op_Implicit" || name =="op_Explicit")
					return MethodKinds.ConversionOp;
				else if (args == 1)
					return MethodKinds.UnaryOp;
				else
					return MethodKinds.BinaryOp;
			}
			else
				return MethodKinds.Unknown;
		}
		// Access modifier retrieval method for constructor
		protected static AccessModifiers GetAccess(MethodBase method) {
			MethodAttributes access = (method.Attributes & MethodAttributes.MemberAccessMask);

			if (method.ReflectedType.IsInterface)
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
		// Parameter list retrieval method for constructor
		public static GenParameterInfo [] GetParameters(MethodBase method) {

			ParameterInfo [] pa = method.GetParameters();
			int count = pa.Length;
			if (count == 0)
				return new GenParameterInfo[0];

			GenParameterInfo [] result = new GenParameterInfo[count];
			for(int i = 0; i < count; i++)
				result[i] = new GenParameterInfo(pa[i]);

			return result;
		}

		// Helper functions for human readable output methods for constructors and methods.
		protected static string ToString(MethodKinds kind) {

//MODIFIED, mod0027
//#if BETA1
			 return Enum.GetName(typeof(MethodKinds), kind);
//#else
//			 return ((Enum)kind).ToString();
//#endif
		}



		protected static string ToHtml(MethodKinds kind) {

//MODIFIED, mod0028
//#if BETA1
			 String result = Enum.GetName(typeof(MethodKinds), kind);
//#else
//			 String result = ((Enum)kind).ToString();
//#endif

			if (result == "Unknown" || result == "Unassigned")
				result = "<i>" + result + "</i>";
			return result;
		}
		protected static string ToString(MethodModifiers [] modifiers) {
			string result = String.Empty;
			int count = modifiers.Length;
			for(int i = 0; i < count; i++) {

//MODIFIED, mod0029
//#if BETA1
				result += Enum.GetName(typeof(MethodModifiers), modifiers[i]) + ((i < count-1) ? " " : "");
//#else
//				result += ((Enum)modifiers[i]).ToString() + ((i < count-1) ? " " : "");
//#endif
			}

			return result;
		}
		protected static string ToHtml(MethodModifiers [] modifiers) {
			string result= String.Empty;
			int count = modifiers.Length;
			for(int i = 0; i < count; i++) {

//MODIFIED, mod0030
//#if BETA1
				string temp = Enum.GetName(typeof(MethodModifiers), modifiers[i]);
//#else
//				string temp = ((Enum)modifiers[i]).ToString();
//#endif

				if (temp == "Unknown")
					temp = "<i>" + temp + "</i>";
				result += temp + ((i < count-1) ? " " : "");
			}
			return result;
		}
		protected static string ToString(AccessModifiers access) {

//MODIFIED, mod0031
//#if BETA1
			return Enum.GetName(typeof(AccessModifiers), access);
//#else
//			return ((Enum)access).ToString();
//#endif
		}


		protected static string ToHtml(AccessModifiers access) {

//MODIFIED, mod0032
//#if BETA1
			String result = Enum.GetName(typeof(AccessModifiers), access);
//#else
//			String result = ((Enum)access).ToString();
//#endif

			if (result == "Unassigned" || result == "NoModifier")
				result = "<i>" + result + "</i>";
			return result;
		}
		protected static string ToString(GenParameterInfo [] parameters, bool varargs) {
			string result = "(";
			int count = parameters.Length;
			for(int i = 0; i < count; i++)
				result += ((i > 0) ? ", " : "") + parameters[i].ToString();
			if (varargs)
				result += ", __arglist";
			result += ")";
			return result;
		}

		protected static string ToHtml(GenParameterInfo [] parameters, bool varargs) {
			string result = "(";
			int count = parameters.Length;
			for(int i = 0; i < count; i++)
				result += parameters[i].ToHtml() + ((i < count-1) ? ", " : "");
			result += ")";
			return result;
		}
		// generate unique parameter sigs for comparing method implementations
		protected static string PSig(MethodBase method) { return GenParameterInfo.PSig(method); }

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
			// change in static'ness implies breaking, change in virtual'ness does not.
			if (obits.Contains("Static") || nbits.Contains("Static"))
				return CompareResults.Breaking;
			// problems with new metadata implies breaking(???)
			if (nbits.Contains("Unknown"))
				return CompareResults.Breaking;
			// other changes imply non-breaking
			if (obits.Count > 0 || nbits.Count > 0)
				return CompareResults.NonBreaking;
			else
				return CompareResults.Same;;
		}
	}
}