using System;
using System.Collections;
using System.Reflection;

namespace SigHelper {

	public enum AccessModifiers : byte {
		Unassigned = 0,
		Unknown = 0x80,
		na = 0x40,
		Public = 1,
//MOD 5042
		ProtectedInternal,
		Protected,
		Internal,
		Private,
	}

	// Possible results from comparing old and new members
	// If two Type members share signatures, other changes might still be present
	public enum CompareResults : byte {
		Different,						// different TypeMembers
			Breaking,					// breaking changes exist
			NonBreaking,				// nonbreaking changes exist
			Same,						// no relevent differences found
	}

	public class GenMemberInfo {

// ** Constants
//MODIFIED, mod5006
		private const BindingFlags allBindingsLookup = BindingFlags.Public | 
				BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

		// ** Fields
		protected MemberTypes _membertype;
		protected string _name = String.Empty;
		public MemberInfo _memberinfo = null;
		protected Type _reflectedtype = null;
		//protected Type _declaringtype = null;
		public Type _declaringtype = null;
		protected Type _declaringbasetype = null;
		protected bool _inherited = false;
		protected bool _neworoverriden = false;
		protected bool _abstract = false; // this will be meaningful for methods, properties and events.
		protected bool _dbug = false;
		protected bool _obsolete = false;
		protected string _obsMessage = "";

		// ** Constructors
		public GenMemberInfo (MemberInfo member) : this(member, false) {}
		public GenMemberInfo (MemberInfo member, bool flag) {

			_dbug = flag;
			if (member == null)
				throw new ArgumentNullException("member");
			if (_dbug) Console.Write(" GenMemberInfo: Constructing member " + member.Name);
			_membertype = member.MemberType;
			_name = member.Name;
			_memberinfo = member;
			_reflectedtype = member.ReflectedType;
			_declaringtype = member.DeclaringType;
			_declaringbasetype = (_declaringtype != null) ? _declaringtype.BaseType : null;
			_inherited = (_declaringtype != null && _reflectedtype != null && _declaringtype != _reflectedtype);

			//robvi
			try
			{
				object []o = member.GetCustomAttributes(false);
			
				if (o != null) 
				{
					foreach(Object s in o) 
					{
						if (s.ToString().ToLower().Trim().IndexOf("obsoleteattribute") >= 0) 
						{
							_obsolete = true;
							_obsMessage = ((ObsoleteAttribute)s).Message;
						}
					}
				}

			}
			catch(Exception){}
		
				//MODIFIED, mod0001
				//			_neworoverriden = (!_inherited && _declaringbasetype != null && //_declaringbasetype.GetMember(_name,_membertype,BindingFlags.LookupAll).Length > 0);
				_neworoverriden = (!_inherited && _declaringbasetype != null && 
					_declaringbasetype.GetMember(_name,_membertype, allBindingsLookup).Length > 0);



				_abstract = false;	// derived classes should override this except for GenFieldInfo.
				if (_dbug) Console.WriteLine(" ... Done.");
			}

		public GenMemberInfo (GenMemberInfo mi) {
			if (mi == null)
				throw new ArgumentNullException();
			_membertype = mi._membertype;
			_name = mi._name;
			_memberinfo = mi._memberinfo;
			_reflectedtype = mi._reflectedtype;
			_declaringtype = mi._declaringtype;
			_declaringbasetype = mi._declaringbasetype;
			_inherited = mi._inherited;
			_neworoverriden = mi._neworoverriden;
			_abstract = mi._abstract;
			_dbug = mi._dbug;
		}

		// ** Properties
		public bool IsObsolete { get { return _obsolete; } }
		public string ObsoleteMessage { get { return _obsMessage; } }
		public bool IsInherited { get { return _inherited; } }
		public bool IsAbstract { get { return _abstract; } }
		public MemberTypes MemberType { get { return _membertype; } }
		public virtual string Name { get { return _name; } }
		public virtual string Key { get { return ToKey(); } }
		public virtual string ShortKey { get { return ToShortKey(); } }		// overriden only by type, method, constructor and property
		public virtual string Sig { get { return ToString(); } }

		// ** Methods
		public static string ToString(MemberInfo mi) { return (new GenMemberInfo(mi)).ToString(); }
		public new virtual string ToString() {
			string result = String.Empty;
			if (_inherited)
				result += InheritedString();

//MODIFIED, mod0013
			result += Enum.GetName(typeof(MemberTypes), _membertype) + ": ";

			result += (_declaringtype != null) ? _declaringtype.FullName + "." + _name : _name;
			return result;
		}

		// Default key for determining breaking changes (5 base elements)
		public virtual string ToKey() {

//MODIFIED, mod0014
			return (
				"MemberType=" + Enum.GetName(typeof(MemberTypes), _membertype) + ":" +
				"MemberName=" + _name + ":" +
				"Abstract=" + ((_abstract) ? "T" : "F") + ":" +
				"Inherited=" + ((_inherited) ? "T" : "F") + ":" +
				"NewOrOverriden=" + ((_neworoverriden) ? "T" : "F")
				);
		}

		// Default short key for identifying members
		public virtual string ToShortKey() {

			return (
				"MemberType=" + Enum.GetName(typeof(MemberTypes), _membertype) + ":" +
				"MemberName=" + _name
				);
		}

		public static Hashtable LoadKey(string key) {
			if (key == null || key == "")
				return new Hashtable(0);

			Hashtable result = new Hashtable(12);

			//ok, the value field can oddly, be an http:, so we have to allow for this!
			string[] sa = key.Split(new Char[] {':'});

			//check the last entry and ensure it isn't simply http: ...
			if ((sa[sa.Length - 2]).ToLower().IndexOf("http") >= 0) {
				string []sa2 = new string[sa.Length - 1];

				for (int i=0;i< sa.Length;i++) {
					if (i < sa.Length - 2)
						sa2[i] = sa[i];
					else {
						sa2[i] = sa[i] + ":" + sa[i + 1];

						sa = sa2;
						break;
					}
				}
			}

			foreach (string s in sa) {
				string[] temp = s.Split(new Char[] {'='});

				if (temp != null && temp.Length >= 2)
					result.Add(temp[0].Trim(), temp[1].Trim());
			}

			return result;
		}

		public static string ToHtml(MemberInfo mi) { return (new GenMemberInfo(mi)).ToHtml(); }
		public virtual string ToHtml() {
			string result = String.Empty;
			if (_inherited)
				result += InheritedHtml();

			result += Enum.GetName(typeof(MemberTypes), _membertype) + ": ";

			result += _declaringtype.FullName + "." + _name;

			return result;

		}

		public static CompareResults EvalChange(string oldkey, string newkey, 
				bool oldAbst, bool newAbst,ArrayList intfcAdds,
				bool typeAdded) {

			Hashtable okey = LoadKey(oldkey);
			Hashtable nkey = LoadKey(newkey);

			string membertype = (okey.Count > 0) ? (string)okey["MemberType"] : 
					(string)nkey["MemberType"];
//P12 changes in here...
//			if (membertype == null) {
//				
//				isEnum = true;
//MUST CHANGE THIS LINE, possibly
//				return CompareResults.Same;
//			}// else {
//P12
// check to see if this is a enum...
//if (
//				
//throw new ArgumentException("Error in one or more of the keys, no MemberType  found.");

//			} else {

			if (membertype == "Constructor") return GenConstructorInfo.EvalChange(okey, nkey, oldAbst, newAbst);
			if (membertype == "Event")       return GenEventInfo.EvalChange(okey, nkey, oldAbst, newAbst, typeAdded);
            //if (membertype == "Field")       return GenFieldInfo.EvalChange(okey, nkey, oldAbst, newAbst);
			//Robvi we need to pass over whether the type was added to know if we flag the changes
			if (membertype == "Field")       return GenFieldInfo.EvalChange(okey, nkey, oldAbst, newAbst, typeAdded);
			if (membertype == "Method")
				return GenMethodInfo.EvalChange(okey, nkey, oldAbst, newAbst,
					typeAdded);
			if (membertype == "Property")    return GenPropertyInfo.EvalChange(okey, 
					nkey, oldAbst, newAbst, typeAdded);
			if (membertype == "NestedType")  return GenTypeInfo.EvalChange(okey, 
					nkey, oldAbst, newAbst, intfcAdds);

			throw new ArgumentException("Unexpected MemberType " + membertype + " in memberkey");
//			}
		}

		public static void Dump(MemberInfo mi) { (new GenMemberInfo(mi)).Dump(String.Empty); }
		public static void Dump(MemberInfo mi, string pre) { (new GenMemberInfo(mi)).Dump(pre); }
		public virtual void Dump() { Dump(String.Empty); }
		public virtual void Dump(string pre) {

//MODIFIED, mod0017
//			Console.WriteLine("\r\n" + pre + "_membertype =    " + ((Enum)_membertype).ToString());
			Console.WriteLine("\r\n" + pre + "_membertype =    " + 
					Enum.GetName(typeof(MemberTypes), _membertype));

			Console.WriteLine(pre + "_name =              " + _name);
			Console.WriteLine(pre + "_reflectedtype =     " + _reflectedtype);
			Console.WriteLine(pre + "_declaringtype =     " + _declaringtype);
			Console.WriteLine(pre + "_declaringbasetype = " + _declaringbasetype);
			Console.WriteLine(pre + "_inherited =         " + _inherited);
			Console.WriteLine(pre + "_neworoverriden =    " + _neworoverriden);
			Console.WriteLine(pre + "_abstract =          " + _abstract);
		}
		
		protected virtual string InheritedString() { return (_inherited) ? "(i)" : String.Empty; }
		protected virtual string InheritedHtml() {
			return (_inherited) ? "<font color=\"Blue\"><i title=\"" + _declaringtype.FullName + "\">(i)</i></font>" : String.Empty;
		}

		// Breaking if new access is more restrictive than old access. progression is public, (protected, internal), protected internal, and private.
		// only exception is changing between protected and internal is breaking either way.
		public static bool IsBreakingScopeChange(string oldscope, string newscope) {
			int osc = 0;
			int nsc = 6;
			for (int i = 1; i <=5; i++) {

//MODIFIED: mod0018
//				string temp = ((Enum)(AccessModifiers)i).ToString();
				string temp = Enum.GetName(typeof(AccessModifiers), i);

				if (oldscope != null && oldscope == temp) osc = i;
				if (newscope != null && newscope == temp) nsc = i;
			}

			//robvi: protectedInternal to protected is NOT breaking
			if (oldscope != null && newscope != null && newscope == "Protected" && oldscope == "ProtectedInternal")
				return false;
			if (oldscope != null && newscope != null && newscope == "Protected" && oldscope == "Internal")
				return true;
			else
				return (nsc > osc);
		}
	}
}
