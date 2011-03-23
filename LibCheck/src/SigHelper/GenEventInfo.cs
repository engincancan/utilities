using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	public enum EventKinds : byte {
		Unassigned = 0x00,
			Unknown		= 0x80,
			Field		= 0x01,
			Property	= 0x02,
	}

	public enum EventModifiers : byte {
		Unknown = 0x80,
			New = 1,
			Static,
	}
	
	public class GenEventInfo : GenMemberInfo {

		//** Fields
		protected Type				_handlertype	= null;
		protected MethodInfo		_addmethod		= null;
		protected MethodInfo		_removemethod	= null;
		protected MethodInfo		_raisemethod	= null;
		protected EventKinds		_eventkind		= EventKinds.Unassigned;
		protected AccessModifiers	_access			= AccessModifiers.Unassigned;
		protected EventModifiers []	_modifiers		= null;

		//** Ctors
		public GenEventInfo (EventInfo ev) : base ((MemberInfo)ev) {
			_handlertype	= ev.EventHandlerType;
			_addmethod		= ev.GetAddMethod(true);
			_removemethod	= ev.GetRemoveMethod(true);
			_raisemethod	= ev.GetRaiseMethod(true);
			_eventkind		= (_addmethod.IsSpecialName) ? EventKinds.Field : EventKinds.Property;
			_access			= GetAccess(_addmethod);
			_modifiers		= GetModifiers(ev);

			_abstract = (_addmethod != null && _addmethod.IsAbstract);
		}

		public GenEventInfo (GenEventInfo ev) : base ((GenMemberInfo)ev) {}

		//** Properties
		public override string Key { get { return ToKey(); } }
		public override string Sig { get { return ToString(); } }

		//** Methods
		public static AccessModifiers GetAccess(MethodInfo addmethod) {
			MethodAttributes access = (addmethod.Attributes & MethodAttributes.MemberAccessMask);

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

		public static EventModifiers [] GetModifiers(EventInfo ev) {
			return new EventModifiers [] {};
		}

		public static string ToString(EventInfo ev) { return (new GenEventInfo(ev)).ToString(); }
		public override string ToString() {
			string temp = null;
			string result = "Event: ";

//MODIFIED, mod0035
//#if BETA1
			result += Enum.GetName(typeof(AccessModifiers), _access);
//#else
//			result += ((Enum)_access).ToString();
//#endif


			result += ((temp = ToString(_modifiers)) != "") ? temp + " " : "";
			result += "event ";
			result += _handlertype.FullName + " ";
			result += _name;
			if (_eventkind == EventKinds.Property)
				result += " { get; set; }";
			return result;
		}

		public static string ToHtml(EventInfo ev) { return (new GenEventInfo(ev)).ToHtml(); }
		public override string ToHtml() {
			return ToString();
		}

		public override string ToKey() {

			// consistent MemberInfo information
//MODIFIED, mod0036
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

//MODIFIED, mod0037
//#if BETA1
				result += ((i>0) ? "," : "") + Enum.GetName(typeof(EventModifiers), _modifiers[i]);
//#else
//				result += ((i>0) ? "," : "") + ((Enum)_modifiers[i]).ToString();
//#endif

			}

//MODIFIED, mod0038
//#if BETA1
			result += ":" +
				"HandlerType=" +_handlertype.FullName + ":" +
				"EventKind=" + Enum.GetName(typeof(EventKinds), _eventkind);
//#else
//			result += ":" +
//				"HandlerType=" +_handlertype.FullName + ":" +
//				"EventKind=" + ((Enum)_eventkind).ToString();
//#endif

			return result;
		}

		// check changes for Events
		public static CompareResults EvalChange(Hashtable okey, Hashtable nkey, bool oldAbst, bool newAbst, bool typeAdded) {
			if (okey["MemberType"] == null && nkey["MemberType"] == null)
				throw new ArgumentException("Error in one or more of the keys, no MemberType found.");
			if ((okey["MemberType"] != null && (string)okey["MemberType"] != "Event") || (nkey["MemberType"] != null && (string)nkey["MemberType"] != "Event"))
				throw new ArgumentException("Error in one or more of the keys, incorrect member type.");

			bool changed = false;									// bookkeeping flags
			CompareResults temp = CompareResults.Same;

			if (okey.Count > 0 && nkey.Count > 0) {					// both present
				if ((string)okey["MemberName"] != (string)nkey["MemberName"])		// compare name (assumed members of same type)
					return CompareResults.Different;
				if ((string)okey["Scope"] != (string)nkey["Scope"]) {				// change in scope
					if (IsBreakingScopeChange((string)okey["Scope"], (string)nkey["Scope"])) return CompareResults.Breaking;
					else changed = true;
				}
				if ((string)okey["HandlerType"] != (string)nkey["HandlerType"])		// change in event handler type. TODO: figure out if new type is assignable from old type.
					return CompareResults.Breaking;
				if ((string)okey["Abstract"] != (string)nkey["Abstract"]) {			// change in abstract'ness
					if ((string)nkey["Abstract"] == "T" && newAbst)
						return CompareResults.Breaking;
					else
						return CompareResults.NonBreaking;
				}
				if ((string)okey["EventKind"] != (string)nkey["EventKind"])
					changed = true;
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
				//if ((string)nkey["Abstract"] == "T" && newAbst)		// only breaking if abstractmember of abstract type
				//ROBVI: we want to ensure that the type wasn't just added
				if((string)nkey["Abstract"] == "T" && newAbst && !typeAdded)
					return CompareResults.Breaking;
				else
					return CompareResults.NonBreaking;
			}
			else													// represents a removed member, always breaking
				return CompareResults.Breaking;
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
			// change in static'ness implies breaking
			if (obits.Contains("Static") || nbits.Contains("Static"))
				return CompareResults.Breaking;
			// problems in metadata for new member implies breaking
			if (nbits.Contains("Unknown"))
				return CompareResults.Breaking;
			// any other changes are non-breaking
			if (obits.Count > 0 || nbits.Count > 0)
				return CompareResults.NonBreaking;
			else
				return CompareResults.Same;;
		}

		public static void Dump(EventInfo ev) { (new GenEventInfo(ev)).Dump(String.Empty); }
		public static void Dump(EventInfo ev, string pre) { (new GenEventInfo(ev)).Dump(pre); }
		public override void Dump() { Dump(String.Empty); }
		public override void Dump(string pre) {
			base.Dump(pre);

//MODIFIED, mod0039
//#if BETA1
			Console.WriteLine(pre + "_eventkind =   " + Enum.GetName(typeof(EventKinds), _eventkind));
//#else
//			Console.WriteLine(pre + "_eventkind =   " + ((Enum)_eventkind).ToString());
//#endif

//MODIFIED, mod0040
//#if BETA1
			Console.WriteLine(pre + "_access =      " + Enum.GetName(typeof(AccessModifiers), _access));
//#else
//			Console.WriteLine(pre + "_access =      " + ((Enum)_access).ToString());
//#endif

			Dump(_modifiers, pre);
			Console.WriteLine(pre + "_handlertype = " + _handlertype);
		}

		public static string ToString(EventModifiers [] modifiers) {
			string result = String.Empty;
			for (int i = 0; i < modifiers.Length; i++) {

//MODIFIED, mod0041
//#if BETA1
				result += ((i > 0) ? " " : "") + SigHelper.CsTranslate( Enum.GetName(typeof(EventModifiers), modifiers[i]) );
//#else
//				result += ((i > 0) ? " " : "") + SigHelper.CsTranslate(((Enum)modifiers[i]).ToString());
//#endif
			}
			return result;
		}

		public static string ToHtml(EventModifiers [] modifiers) {
			return ToString(modifiers);
		}

		protected static void Dump(EventModifiers [] modifiers, string pre) {
			string result = String.Empty;
			for (int i = 0; i < modifiers.Length; i++) {

//MODIFIED, mod0042
//#if BETA1
				result += ((i > 0) ? " " : "") + Enum.GetName(typeof(EventModifiers), modifiers[i]);
//#else
//				result += ((i > 0) ? " " : "") + ((Enum)modifiers[i]).ToString();
//#endif
			}

			Console.WriteLine(pre + "_modifiers =   " + result);
		}
	}
}