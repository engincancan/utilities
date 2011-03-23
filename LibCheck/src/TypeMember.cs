using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using SigHelper;

namespace LibCheck {
[Serializable]
public class TypeMember
{
	string		_typename;		// short type name
	string		_typekey;		// unique identifier string
	string		_typeshortkey;		// signature identifier string
	string		_typestring;		// friendly, C# style description
	string		_typeVBstring;		// friendly, VB style description
	//robvi
	public string		_typebasetype;		//basetype for this TypeMember
	TypeKinds	_typekind;		// TypeKind from GenTypeInfo
	bool		_isabstracttype;	// abstract flag

	string		_membername;		// short member name (nested types are without enclosing type and "$")
	string		_memberkey;		// unique identifier without reflected or declaring type information (helps 
						// when comparing inherited members)

	string		_membershortkey;	// member signature identifier without type information
	string		_memberstring;		// friendly, C# style description
	string		_memberVBstring;	// friendly, VB style description
	//robvi
	public string _memberbasetype;//basetype for this member
	MemberTypes	_memberkind;		// from reflection
	bool		_isabstractmember;	// abstract flag
	bool		_isinheritedmember;	// inherited flag
//	bool		_isinterface;		// interface flag

	Version		_version;		// runtime version when store file was created
	string		_namespace;		// namespace reflected type is from
	string		_module;		// module reflected type is from
	string		_assembly;		// assebly reflected type is from

	string		_misc;			// encoding of other information of note.

//P12 Added
	bool		_isenum;

//P11 ADDED
//	bool		_isenum;
//	private static string previousEnum = "";


//robvi Added
	string	_layoutkind;
	string _offset="";
	public static Type marshalType = typeof(System.Runtime.InteropServices.Marshal);
	static Hashtable ht = new Hashtable();
	static Hashtable httrack = new Hashtable();
	static Hashtable ObsoleteHT = new Hashtable();
	




//MODIFIED, mod5021
	//default constructor
	public TypeMember () {}

	//** Constructor
	public TypeMember (MemberInfo member, Type type, bool perfSer, bool isEnum, bool addStruct, bool addStructMethod, StreamWriter obsoletewriter)
	{
		if (member == null) throw new ArgumentNullException("member");

		if (member.MemberType == MemberTypes.TypeInfo) {
			#if DBUGTM
				Console.WriteLine("% TypeMember .ctor: Cannot construct a TypeMember from a MemberInfo of type TypeInfo.");
			#endif
			throw new ArgumentException("Cannot construct a TypeMember from a MemberInfo of type TypeInfo.", "member");
		}

		string temp = null;
//		if (member.ReflectedType == null) Console.WriteLine("\r\nmember.ReflectedType == null for " + member.Name); // reflection error?
//		CsTypeInfo ti = new CsTypeInfo(member.ReflectedType);	// Lose some of the information for inherited members, but have consistent type info
		CsTypeInfo ti = new CsTypeInfo(type);	// Get info on enclosing type.
		GenMemberInfo mi = null;
		
		switch(member.MemberType) {
		case MemberTypes.NestedType :
			#if DBUGTM
				Console.WriteLine(" % TypeMember .ctor: NestedType  - " + member.Name);
			#endif
			mi = new CsTypeInfo((Type)member);
			break;
		case MemberTypes.Constructor :
			#if DBUGTM
				Console.WriteLine(" % TypeMember .ctor: Constructor - " + member.Name);
			#endif
			mi = new CsConstructorInfo((ConstructorInfo)member);
			break;
		case MemberTypes.Method :
			#if DBUGTM
				Console.WriteLine(" % TypeMember .ctor: Method      - " + member.Name);
			#endif
			mi = new CsMethodInfo((MethodInfo)member);
			break;
		case MemberTypes.Event :
			#if DBUGTM
				Console.WriteLine(" % TypeMember .ctor: Event       - " + member.Name);
			#endif
			mi = new CsEventInfo((EventInfo)member);
			break;
		case MemberTypes.Field :

			#if DBUGTM
				Console.WriteLine(" % TypeMember .ctor: Field       - " + member.Name);
			#endif
			mi = new CsFieldInfo((FieldInfo)member);

			break;
		case MemberTypes.Property :
			#if DBUGTM
				Console.WriteLine(" % TypeMember .ctor: Property    - " + member.Name);
			#endif
			mi = new CsPropertyInfo((PropertyInfo)member, false);
			break;
		default :
			#if DBUGTM
				Console.WriteLine(" % TypeMember .ctor: Unknown Member Type.");
			#endif
			mi = new GenMemberInfo(member);

//MODIFIED, mod5002
			break;
		}

		_typename		= ti.Name;
		_typekey		= ti.Key;
		_typeshortkey		= ti.ShortKey;
		_typestring		= ti.Sig;
		_typeVBstring		= String.Empty;
		_typekind		= ti.TypeKind;
		_isabstracttype		= ti.IsAbstract;
		//robvi
		_typebasetype		= (ti._basetype == null? "" : ti._basetype.FullName);

//robvi Added for layout
		if(type.IsExplicitLayout) {_layoutkind = "Explicit";}
		if(type.IsLayoutSequential) {_layoutkind = "Sequential";}
		if(type.IsAutoLayout) {_layoutkind = "Auto";}



	//P12 Added
	_isenum			= isEnum;
		

//P12 Added
if (_isenum) {
//build memberstirng info here also...
//Console.WriteLine("p1");
				string tempMisc = "EnumWithValues=";
				string tempString = "\r\n<br>";

			//	Type thisType = member.DeclaringType;
			//	Console.WriteLine("Type: {0}" +  member.DeclaringType.FullName);
//P13 Added Change required due to ReflectingType being different in Whidbey
				Type thisType=member.ReflectedType;
	
			//	if (thisType.IsEnum){
			//		Console.WriteLine("Type: {0}" +  member.DeclaringType.FullName);
			//	}
				string[] names = Enum.GetNames(thisType);
				 

				for (int i = 0;i<names.Length;i++) {
					if (i>0)
						tempMisc+=",";

					tempMisc += names[i] + "-" + 
						((Enum)Enum.Parse(thisType,names[i])).ToString("d");

					for (int j = 0; j < 20; j++)
						tempString+="&nbsp";

					tempString += names[i] + " = " + 
						((Enum)Enum.Parse(thisType,names[i])).ToString("d");

					if (i < names.Length)
						tempString+=",\r\n<br>";
				}

				//add this to the type string...
				tempString += "</b>";
				if (_typestring.IndexOf("</b>") >=0) {
					_typestring = _typestring.Substring(0,_typestring.IndexOf("</b>")) +
							tempString;
				} else  {
					Console.WriteLine("Something went wrong");
				}

				//add the entries into the misc field...
				_typekey += ":" + tempMisc;
//				_misc = tempMisc;
}
//else {
		_membername		= mi.Name;
		_memberkey		= mi.Key;
		_membershortkey		= mi.ShortKey;
		_memberstring		= mi.Sig;
		_memberVBstring		= String.Empty;
		_memberkind		= mi.MemberType;
		_memberbasetype = mi._declaringtype.FullName;
		_isabstractmember	= mi.IsAbstract;
		_isinheritedmember	= mi.IsInherited;
		
//}
		_version	= Environment.Version;
		_namespace	= ((temp = member.ReflectedType.Namespace) != null) ? temp : String.Empty;
		_module		= member.DeclaringType.Module.Name;

		//OPTION: simply save all this info to the db.
		//THEN, repop on the way out...???
		try {
			_assembly = member.ReflectedType.Module.Assembly.GetName().Name;
		} catch(Exception) {
			_assembly = _module;
		}

// P12 Added this if
		if (_isenum == false ) {
			// add information here as needed until next breaking field change comes along
			_misc	= null;	

// Base Type: System.Enum

			//saving the values of enums...
			if (member.MemberType == MemberTypes.NestedType) {
				if (mi.Key.IndexOf("BaseType=System.Enum") >=0) {
					string tempMisc = "enumvalues=";

					Type thisType = member.ReflectedType.GetNestedType(_membername);

					string[] names = Enum.GetNames(thisType);

					for (int i = 0;i<names.Length;i++) {
						if (i>0)
							tempMisc+=",";

						tempMisc += names[i] + "=" + 
							((Enum)Enum.Parse(thisType,names[i])).ToString("d");
					}

					//add the entries into the misc field...

					if (tempMisc != "enumvalues=") {
						if (_misc != null && _misc != "")
							_misc += ";";
						else
							_misc = "";

							_misc += tempMisc;
					}
				}
			}

//P11 ADDED
// NEW STUFF
/*
		else if (member.MemberType == MemberTypes.Field) {
			if (member.DeclaringType.IsEnum && 
					previousEnum != member.DeclaringType.ToString()) {
				string tempMisc = "EnumWithValues=";

				Type thisType = member.DeclaringType;

				string[] names = Enum.GetNames(thisType);

				for (int i = 0;i<names.Length;i++) {
					if (i>0)
						tempMisc+=",";

					tempMisc += names[i] + "=" + 
						((Enum)Enum.Parse(thisType,names[i])).ToString("d");
				}

				//add the entries into the misc field...

				if (tempMisc != "EnumWithValues=") {
					if (_misc != null && _misc != "")
						_misc += ";";
					else
						_misc = "";

						_misc += tempMisc;
				}

				previousEnum = member.DeclaringType.ToString();
				_isenum = true;
			}
			else if (member.DeclaringType.IsEnum && 
					previousEnum == member.DeclaringType.ToString()) {
				if (_misc != null && _misc != "")
					_misc += ";";
				else
					_misc = "";

				_misc += "ignorethiselement";
			}
		}
*/

			//SAVING THE PARAMETER NAMES!
			if (member.MemberType == MemberTypes.Method) {

				GenParameterInfo[] pi = 
					GenMethodBase.GetParameters((MethodBase)(mi._memberinfo));

				string tempName = ((_misc == null || _misc == "") ? "" : ";") + "params=";

				for (int i=0;i < pi.Length;i++) {
//Console.WriteLine((pi[i]).ToString());
//Console.ReadLine();
//OK, simply don't do the split! This will get me halfway there for params...

//THIS IS HOW IT WAS...
					string[] items = (pi[i]).ToString().Split(' ');
//					string items = (pi[i]).ToString();

					if (i>0)
						tempName+=",";

//					tempName+=items;
					tempName+=items[1];
	
				}

				if (tempName.EndsWith("params=") == false)
					_misc += tempName;
/*
				//add the parameter sigs!
				for (int i=0;i < pi.Length;i++) {

					string[] items = (pi[i]).ToString().Split(' ');

					if (i>0)
						tempName+=",";

					tempName+=items[1];
	
				}
*/
			}
//P12 Added
		}

		//saving custom attributes...
		object []attribs = 
			member.GetCustomAttributes(false); //false says NOT get inherited attribs
/*
foreach (object o in attribs) {
	Type t = o.GetType();
	PropertyInfo []pi = t.GetProperties();

	foreach(PropertyInfo p in pi) {
//		try {
		    Console.WriteLine(p.Name + "=" + p.GetValue(p.Name,null));
		
//		}
//		catch  {}
//			Console.WriteLine(e.ToString());
//			Console.WriteLine(p.Name + "=" + p.GetValue(p.Name,new object[] {0}));
//		}
	}
Console.ReadLine();
}
*/
		if (attribs != null && attribs.Length > 0) {
			string stringAttribs = "";

			foreach(object obj in attribs) {
				if (stringAttribs != "")
					stringAttribs += ",";
				stringAttribs += obj.ToString();
			}

			_misc += ";attribs=" + stringAttribs;
		}



// Storing it here, so that we are backward compatible.
// If we add a new field, all existing stores will be invalid
		_misc		+= mi.IsObsolete ? ";obsoleteattribute-" + mi.ObsoleteMessage : "";
		if(mi.IsObsolete&& (!ObsoleteHT.Contains(mi._declaringtype+"."+mi.Name)))
		{
			ObsoleteHT.Add(mi._declaringtype+"."+mi.Name,null);
			//Console.WriteLine(mi._declaringtype +"."+mi.Name);
			obsoletewriter.WriteLine(mi._declaringtype +"."+mi.Name);

		}
		




		if (perfSer) {
			//figure out the serialization fields string...
			string serFields = MakeSerFieldsString(type);

			if (serFields != "") {
				if (_misc != null && _misc != "")
					_misc += ";";
				else
					_misc = "";
				_misc += serFields+";";
			}
		}
		if (addStruct) 
		{
			//figure out the serialization fields string...
			string structFields = MakeStructFieldsString(type);

			if (structFields != "") 
			{
				if (_misc != null && _misc != "")
					_misc += ";";
				else
					_misc = "";
				_misc += structFields;
			}
		}

		if (addStructMethod)
		{
			//figure out the serialization methods string...
			string structMethods = MakeStructMethodsString(type);

			if (structMethods != "")
			{
				if (_misc != null && _misc != "")
					_misc += ";";
				else
					_misc = "";
				_misc += structMethods;
			}
		}


	}

	//** Properties
	public string Name		{ get { return _typename + "." + _membername; } }
	public string FullName	{
		get {
			switch(_memberkind) {
			case MemberTypes.NestedType :
				return TypeFullName + "$" + _membername;
			default :
				return TypeFullName + "." + _membername;
			}
		}
	}

//MODIFIED, mod5022
	public bool IsInherited			{ get { return _isinheritedmember; }
						set { _isinheritedmember = value; } }

//P12 Added
	public bool IsEnum			{ get { return _isenum; }
						set { _isenum = value; } }

//P11 Added
//	public bool IsEnum			{ get { return _isenum; }
//						set { _isenum = value; } }

	public string TypeName			{ get { return _typename; }
						set { _typename = value; } }

//NOTE: no need to set this item, since it is done by setting the namespace, and the typename
	public string TypeFullName		{ get { return ((_namespace != String.Empty) ? _namespace + 
								" " : "") + _typename; } }

	public string TypeKey			{ get { return _typekey; }
						set { _typekey = value; } }

	public string TypeString		{ get { return _typestring; }
						set { _typestring = value; } }

	public string TypeShortKey		{ get { return _typeshortkey; }
						set { _typeshortkey = value; } }

	public string TypeShortSig		{ get { return _typeshortkey; }
						set { _typeshortkey = value; } }

	public TypeKinds TypeKind		{ get { return _typekind; }
						set { _typekind = value; } }

	public bool IsAbstractType		{ get { return _isabstracttype; }
						set { _isabstracttype = value; } }

	public string MemberName		{ get { return _membername; }
						set { _membername = value; } }

//NOTE: no need to set this item, since it is done by setting the TypeFullName, and the _membername
	public string MemberFullName		{ get { return FullName; } }

	public string MemberKey			{ get { return _memberkey; }
						set { _memberkey = value; } }

	public string MemberShortSig		{ get { return _membershortkey; }
						set { _membershortkey = value; } }

	public string MemberShortKey		{ get { return _membershortkey; }
						set { _membershortkey = value; } }

	public string MemberString		{ get { return _memberstring; }
						set { _memberstring = value; } }

	public MemberTypes MemberKind		{ get { return _memberkind; }
						set { _memberkind = value; } }

//NOTE: why is this the same as MemberKind????
	public MemberTypes MemberType		{ get { return _memberkind; }
						set { _memberkind = value; } }

	public bool IsAbstractMember		{ get { return _isabstractmember; }
						set { _isabstractmember = value; } }

	public bool IsInheritedMember		{ get { return _isinheritedmember; }
						set { _isinheritedmember = value; } }

	public Version Version			{ get { return _version; }
						set { _version = value; } }

//NOTE:  no need to set this item, since it is done by setting the Revision, and the Build...
	public int Revision			{ get { return _version.Revision; } }

//NOTE: no need to assign, since it is done by setting version...
	public int Build			{ get { return _version.Build; } }

//NOTE:  no need to set this item, since it is done by setting the Revision, and the Build...
	public string BuildName			{ get { return _version.Revision.ToString() + 
						((_version.Build > 0) ? "." + 
						_version.Build.ToString() : ""); } }

	public string Namespace 		{ get { return _namespace; }
						set { _namespace = value; } }

	public string Module			{ get { return _module; }
						set { _module = value; } }

	public string Assembly			{ get { return _assembly; }
						set { _assembly = value; } }

	public string Misc			{ get { return _misc; }
						set { _misc = value; } }

	public string TypeTitle			{ get { return TypeString; }
						set { TypeString = value; } }


	//Robvi Added
	public string LayoutKind	{ get { return _layoutkind; }
		set { LayoutKind = value; } 	
	}

	public string OffSet	{ get { return _offset; }
		set { LayoutKind = value; } 	
	}


	public string[] Owners 
	{
		get {
			if ( Owners2.dbNotAvailable)
				return new string[4] {"unknown","unknown","unknown","unknown"};
			else
				return new string[4] { GetPMOwner(TypeFullName), GetDevOwner(TypeFullName),
						GetTestOwner(TypeFullName), GetUEOwner(TypeFullName) };			
		
		}
	}


	//** Methods

	// determine if (and how) two types are similar
	public static CompareResults EvalTypeChange (TypeMember oldtm, 
			TypeMember newtm, ArrayList intfcAdds, bool perfSer, out bool typeAdded, bool addStruct, bool addStructMethod) {
		typeAdded = false;

		if (oldtm == null && newtm == null)
			throw new ArgumentException("Cannot compare two null values.");



		if (perfSer) {
			int istart;
			int iend;
			if (oldtm.Misc.ToLower().IndexOf("serfields") >= 0 && 
					newtm.Misc.ToLower().IndexOf("serfields") >= 0) {

				istart = oldtm.Misc.ToLower().IndexOf("serfields");
				iend = oldtm.Misc.ToLower().IndexOf(";", istart + 1);

				if (iend < 0)
					iend = oldtm.Misc.Length - 1;

				string otmser = oldtm.Misc.Substring(istart, iend - istart);
	
				istart = newtm.Misc.ToLower().IndexOf("serfields");
				iend = newtm.Misc.ToLower().IndexOf(";", istart + 1);
			
				if (iend < 0)
					iend = newtm.Misc.Length - 1;

				string ntmser = newtm.Misc.Substring(istart, iend - istart);

//				ntmser = ntmser.Replace("=F,", ",");
//				ntmser = ntmser.Replace("=T,", ",");
//				otmser = otmser.Replace("=F,", ",");
//				otmser = otmser.Replace("=T,", ",");

				if (ntmser != otmser)
				{
					//Calculate what's been removed or added:
					Hashtable oldSerFields = new Hashtable();
					Hashtable newSerFields = new Hashtable();
					int start = otmser.IndexOf("=");
					int sep = 0;
					string sub;
					
					while(start < otmser.Length)
					{
						string optionalfield = "";
						sep = otmser.IndexOf(',',start+1);
						if(sep==-1)//end case
						{
							sub = otmser.Substring(start+1);
							optionalfield = sub.Substring(sub.LastIndexOf("=")+1);
							sub=sub.Replace("=F", "");
							sub=sub.Replace("=T", "");

							oldSerFields.Add(sub,optionalfield);

							break;
						}
						sub = otmser.Substring(start+1,sep-(start+1));
						optionalfield = sub.Substring(sub.LastIndexOf("=") + 1);
						sub=sub.Replace("=F", "");
						sub=sub.Replace("=T", "");
						oldSerFields.Add(sub, optionalfield);

						start=sep;
					}

					sep = 0;
					start = ntmser.IndexOf("=");
					while(start < ntmser.Length)
					{
						string optionalfield = "";

						sep = ntmser.IndexOf(',',start+1);
						if(sep==-1)//end case
						{
							sub = ntmser.Substring(start+1);
							optionalfield = sub.Substring(sub.LastIndexOf("=") + 1);
							sub=sub.Replace("=F", "");
							sub=sub.Replace("=T", "");
							newSerFields.Add(sub,optionalfield);

							break;
						}
						sub = ntmser.Substring(start+1,sep-(start+1));
						optionalfield = sub.Substring(sub.LastIndexOf("=") + 1);
						sub=sub.Replace("=F", "");
						sub=sub.Replace("=T", "");
						newSerFields.Add(sub,optionalfield);

						start=sep;
					}
					
					bool breakingadd = false;
					bool breakingremove = false;

					//compare the two hashtables to find out what's been added or removed
					foreach(String fName in oldSerFields.Keys)
					{

						//if the new hashtables doesn't have this type, it's been removed
						if(!newSerFields.ContainsKey(fName)&&fName!="")
						{
							string type, name;
							int eqindex = fName.IndexOf("=");
							type = fName.Substring(0,eqindex);
							name = fName.Substring(eqindex+1);
												
							//add this to the unified report
							breakingremove = true;
							
						}
					}
					foreach(String fName in newSerFields.Keys)
					{
						//if the old hashtables doesn't have this type, it's been added
						if(!oldSerFields.ContainsKey(fName)&&fName!=""&&newSerFields[fName].ToString()!="T")
						{
							int eqindex=0;
							string type, name;
							eqindex = fName.IndexOf("=");
							type = fName.Substring(0,eqindex);
							name = fName.Substring(eqindex+1);
							
							//add this to the unified report
							breakingadd = true;
						}
					}

					//return this as being a break
					if (breakingadd || breakingremove)
					{
						return CompareResults.Breaking;
					}
					

				}
		
		

			}

			//robvi Here we check if unsealed serializable types now implement iserializeable
			//more string parsing first...
	
			string oserializeable, osealed, ocontrolledSer;
			string nserializeable, nsealed, ncontrolledSer;
			istart = oldtm.Misc.ToLower().IndexOf("serializeable=");
			iend = oldtm.Misc.ToLower().IndexOf(";", istart + 1);

			if (iend < 0)
				iend = oldtm.Misc.Length - 1;

			string otypeser = oldtm.Misc.Substring(istart, iend - istart);

			istart = otypeser.IndexOf("serializeable");
			iend = otypeser.IndexOf(",", istart+1);
			oserializeable = otypeser.Substring(istart, iend - istart);
			istart = otypeser.IndexOf("sealed");
			iend = otypeser.IndexOf(",", istart+1);
			osealed = otypeser.Substring(istart, iend - istart);
			istart = otypeser.IndexOf("controlledSer");
			ocontrolledSer = otypeser.Substring(istart);

			istart = newtm.Misc.ToLower().IndexOf("serializeable=");
			iend = newtm.Misc.ToLower().IndexOf(";", istart + 1);

			if (iend < 0)
				iend = newtm.Misc.Length - 1;

			string ntypeser = newtm.Misc.Substring(istart, iend - istart);
				

			istart = ntypeser.IndexOf("serializeable");
			iend = ntypeser.IndexOf(",", istart+1);
			nserializeable = ntypeser.Substring(istart, iend - istart);
			istart = ntypeser.IndexOf("sealed");
			iend = ntypeser.IndexOf(",", istart+1);
			nsealed = ntypeser.Substring(istart, iend - istart);
			istart = ntypeser.IndexOf("controlledSer");
			ncontrolledSer = ntypeser.Substring(istart);

			//This is where the comparison is done

			//[Serializable] and not ISerializable and not Sealed Must stay [Serializable]
 			if((oserializeable.CompareTo("serializeable=True")==0)&&(osealed.CompareTo("sealed=False")==0)&&(ocontrolledSer.CompareTo("controlledSer=False")==0))
			{
				if(nserializeable.CompareTo("serializeable=False")==0)
				{
					return CompareResults.Breaking;
				}

			}
			//[Serializable] and ISerializable Must stay ISerializable
			if((oserializeable.CompareTo("serializeable=True")==0)&&(ocontrolledSer.CompareTo("controlledSer=True")==0))
			{
				if(ncontrolledSer.CompareTo("controlledSer=False")==0)
				{
					return CompareResults.Breaking;
				}
			}
					
			//[Serializable] must stay [Serializeable]
			if((oserializeable.CompareTo("serializeable=True")==0)&&(nserializeable.CompareTo("serializeable=False")==0))
			{
				return CompareResults.Breaking;
			}

		
			
			return CompareResults.Same;
		}

		if (addStructMethod) 
		{
			if (oldtm.Misc.ToLower().IndexOf("structmethods") >= 0 || 
				newtm.Misc.ToLower().IndexOf("structmethods") >= 0) 
			{
				
				int istart = oldtm.Misc.ToLower().IndexOf("structmethods");
				int iend = oldtm.Misc.ToLower().IndexOf(";", istart + 1);

				if (iend < 0)
					iend = oldtm.Misc.Length - 1;

				string otmser;
				if(istart==-1)
					otmser="";
				else
					otmser = oldtm.Misc.Substring(istart, iend - istart);
	
				istart = newtm.Misc.ToLower().IndexOf("structmethods");
				iend = newtm.Misc.ToLower().IndexOf(";", istart + 1);
			
				if (iend < 0)
					iend = newtm.Misc.Length - 1;

				string ntmser;
				if(istart==-1)
					ntmser="";
				else
				    ntmser = newtm.Misc.Substring(istart, iend - istart);
				

				bool ocomvistype, ocomvisasm;
				bool ncomvistype, ncomvisasm;
					
				string oldcom = oldtm.Misc.Substring(oldtm.Misc.IndexOf(";")+1);
				string newcom = newtm.Misc.Substring(newtm.Misc.IndexOf(";")+1);
						
				ocomvistype = oldcom.Substring(0,oldcom.IndexOf(","))=="comvistype=True" ?  true : false;
				ocomvisasm = oldcom.Substring(oldcom.IndexOf(",")+1)=="comvisasm=True" ? true : false;

				ncomvistype = newcom.Substring(0,newcom.IndexOf(","))=="comvistype=True" ? true : false;
				ncomvisasm = newcom.Substring(newcom.IndexOf(",")+1)=="comvisasm=True" ? true : false;
						
				if (ntmser != otmser)
				{
					if(!httrack.ContainsKey(oldtm.FullName))
					{
						httrack.Add(oldtm.FullName,null);
						Console.WriteLine("Broken: " + oldtm.FullName);
					}
					if(!ocomvisasm)
					{
						if(ocomvistype)
							return CompareResults.Breaking;
						else 
							return CompareResults.Same;
					}
					else
					{
						if(ocomvistype)
							return CompareResults.Breaking;
						else
							return CompareResults.Same;
					}

				}
			}
			return CompareResults.Same;
		}



		if (addStruct) 
		{
			if (oldtm.Misc.ToLower().IndexOf("structfields") >= 0 || 
				newtm.Misc.ToLower().IndexOf("structfields") >= 0) 
			{

				int istart = oldtm.Misc.ToLower().IndexOf("structfields");
				int iend = oldtm.Misc.ToLower().IndexOf(";", istart + 1);

				if (iend < 0)
					iend = oldtm.Misc.Length - 1;

				string otmser;
				if(istart<0)
					otmser="";
				else
					otmser = oldtm.Misc.Substring(istart, iend - istart);
				
	
				istart = newtm.Misc.ToLower().IndexOf("structfields");
				iend = newtm.Misc.ToLower().IndexOf(";", istart + 1);
			
				if (iend < 0)
					iend = newtm.Misc.Length - 1;


				string ntmser;
				if(istart<0)
						ntmser="";
				else
					ntmser = newtm.Misc.Substring(istart, iend - istart);

				if (ntmser != otmser)
				{
					if(!httrack.ContainsKey(oldtm.FullName))
					{
						httrack.Add(oldtm.FullName,null);
						Console.WriteLine("Broken: " + oldtm.FullName);
					}
					return CompareResults.Breaking;

				}
		
		

			}
			return CompareResults.Same;
		}

		if (oldtm != null && newtm != null && oldtm._typeshortkey != newtm._typeshortkey)
			return CompareResults.Different;

		string oldkey = (oldtm != null) ? oldtm._typekey : "";
		string newkey = (newtm != null) ? newtm._typekey : "";

		try {
			CompareResults result = GenTypeInfo.EvalTypeChange(oldkey, newkey, intfcAdds,
					out typeAdded);

			return result;
		}
		catch (Exception e) {
			throw new ApplicationException("Could not compare type keys.", e);
		}
	}


	// determine if (and how) two members are similar
	// assumes types are at least similar (even if breaking change exists)
	//assumption is ok, because calling procedure checks that the types ARE similar
	public static CompareResults EvalMemberChange (TypeMember oldtm, TypeMember newtm, ArrayList intfcAdds, bool typeAdded) {

		if (oldtm == null && newtm == null)
			throw new ArgumentException("Cannot compare two null values.");

//this check I believe to be superfluous:
//this is done in the calling procedure...
		if (oldtm != null && newtm != null && oldtm._membershortkey != newtm._membershortkey)
			return CompareResults.Different;

		string oldkey = (oldtm != null) ? oldtm._memberkey : "";
		string newkey = (newtm != null) ? newtm._memberkey : "";
		bool oldAbst = (oldtm != null && oldtm._isabstracttype);
		bool newAbst = (newtm != null && newtm._isabstracttype);
//Console.WriteLine("oldkey = " + oldkey);
//if (oldtm == null)
//	Console.WriteLine("oldtm = null!");

		bool oldObs = false;
		bool newObs = false;

		//okay, check ALSO to see if the member is obsolete...
		if (oldtm != null && oldtm.Misc != null && oldtm.Misc.IndexOf("obsoleteattribute-") >= 0) {
			oldObs = true;
		}

		if (newtm != null && newtm.Misc != null && newtm.Misc.IndexOf("obsoleteattribute-") >= 0) {
			newObs = true;
		}
		
		try {

//			bool isEnum = false;
			CompareResults result = CompareResults.Same;

			result = GenMemberInfo.EvalChange(oldkey, newkey, oldAbst, 
				newAbst, intfcAdds, typeAdded);

//P12
//need to change return value...
//MAY NOW BE UNNECESSARY...
//if (isEnum) {
//return CompareResults.Breaking;
//}


			//check for an enum's VALUES
			if ( ((oldtm != null && 
				oldtm.MemberKey.ToLower().IndexOf("basetype=system.enum") >=0) ||
				(newtm != null && 
				newtm.MemberKey.ToLower().IndexOf("basetype=system.enum") >=0)) &&
				(result == CompareResults.Same || result == CompareResults.NonBreaking)) {

				if (oldtm != null && oldtm._misc != null && 
						newtm != null && newtm._misc != null) {

				int boldEnum = (oldtm != null ? oldtm._misc.IndexOf("enumvalues=") : -1);
				int bnewEnum = (newtm != null ? newtm._misc.IndexOf("enumvalues=") : -1);

				//retrieve the enumvals!

				if (boldEnum >= 0 && bnewEnum >= 0) {

					int eoldEnum = oldtm._misc.LastIndexOf(";") > boldEnum ? 
							oldtm._misc.LastIndexOf(";"): oldtm._misc.Length;
					int enewEnum = newtm._misc.LastIndexOf(";") > bnewEnum ? 
							newtm._misc.LastIndexOf(";"): newtm._misc.Length;

					boldEnum += "enumvalues=".Length;
					bnewEnum += "enumvalues=".Length;

					string oldmisc = oldtm._misc.Substring(boldEnum, eoldEnum-boldEnum);
					string newmisc = newtm._misc.Substring(bnewEnum, enewEnum-bnewEnum);

					string[]oldEntries = oldmisc.Split(',');
					string[]newEntries = newmisc.Split(',');

					Array.Sort(oldEntries);
					Array.Sort(newEntries);

					//ok, if the lengths are different, its a breaking change!
					if (oldEntries.Length != newEntries.Length)
						if (oldEntries.Length > newEntries.Length) {
							return CompareResults.Breaking;
						} else {
							return CompareResults.NonBreaking;
						}
					else {
						//if any of the entries are not the same, it's ALSO breaking
						for (int i=0;i<oldEntries.Length;i++) {
						
							if (oldEntries[i] != newEntries[i])
								return CompareResults.Breaking;
						}
					}
				}
				}
			}
//Console.WriteLine("p4");
			if ( ((oldtm != null && 
				oldtm.MemberKey.ToLower().IndexOf("membertype=method") >=0) ||
				(newtm != null && 
				newtm.MemberKey.ToLower().IndexOf("membertype=method") >=0)) &&
				(result == CompareResults.Same)) {

				if (oldtm._misc != null && newtm._misc != null) {
				int boldMeth = (oldtm != null ? oldtm._misc.IndexOf("params=") : -1);
				int bnewMeth = (newtm != null ? newtm._misc.IndexOf("params=") : -1);

				//retrieve the parameter names!
				if (boldMeth >= 0 && bnewMeth >= 0) {

//					int eoldMeth = oldtm._misc.LastIndexOf(";") > boldMeth ? 
//							oldtm._misc.LastIndexOf(";"): oldtm._misc.Length;
//					
//					int enewMeth = newtm._misc.LastIndexOf(";") > bnewMeth ? 
//							newtm._misc.LastIndexOf(";"): newtm._misc.Length;

					//this should be the first occurence of ";" after "params="
					int eoldMeth = oldtm._misc.LastIndexOf(";") > boldMeth ? 
						oldtm._misc.IndexOf(";"): oldtm._misc.Length;
						
					int enewMeth = newtm._misc.LastIndexOf(";") > bnewMeth ? 
						newtm._misc.IndexOf(";"): newtm._misc.Length;

					string oldmisc = oldtm._misc.Substring(boldMeth, eoldMeth-boldMeth);
					string newmisc = newtm._misc.Substring(bnewMeth, enewMeth-bnewMeth);


					//if changed, then the NAMES changed, so indicate that!
					if (oldmisc != newmisc)
						return CompareResults.Breaking;
				}
				}
			}

//COMMENTED, TEMPORARILY
/*
			//check for attributes...
			if ((oldtm != null || newtm != null) &&
					(result == CompareResults.Same || 
					result == CompareResults.NonBreaking)) {
				int boldAttr = (oldtm != null ? oldtm._misc.IndexOf("attribs=") : -1);
				int bnewAttr = (newtm != null ? oldtm._misc.IndexOf("attribs=") : -1);

				if (boldAttr >= 0 && bnewAttr >= 0) {
					int eoldAttr = oldtm._misc.LastIndexOf(";") > boldAttr ? 
							oldtm._misc.LastIndexOf(";"): oldtm._misc.Length;
					int enewAttr = newtm._misc.LastIndexOf(";") > bnewAttr ? 
							newtm._misc.LastIndexOf(";"): newtm._misc.Length;

					string oldmisc = oldtm._misc.Substring(boldAttr, eoldAttr-boldAttr);
					string newmisc = newtm._misc.Substring(bnewAttr, enewAttr-bnewAttr);

					if (oldmisc != newmisc) {
						//determine if it's breaking...
						

					}
				}				
			}
*/
			if (oldObs == false && newObs == true) {
//				return CompareResults.Breaking;
				return CompareResults.NonBreaking;
			}
			else
				return result;
		}
		catch (Exception e) {

			throw new ApplicationException("Could not compare member keys.", e);
		}
	}

//MODIFIED, mod5033
	// the xxToHtml methods generate text for the Unified, Split and All reports
	public string TypeToHtml() {
		String sPM = "";
		String sDev ="";
		String sTest = "";
		String sUE = "";
		Boolean entryMade = false;
		Boolean ccMade = false;
		string mailto = "";

		string subject = String.Format("Changes%20in%20{0}%20from%20{1}%20to%20{2}", 
				TypeFullName, LibChk.OldVer, LibChk.NewVer);

		if (Owners2.dbNotAvailable == false) {
			sPM = GetPMOwner(TypeFullName);
			sDev = GetDevOwner(TypeFullName);
			sTest = GetTestOwner(TypeFullName);
			sUE = GetUEOwner(TypeFullName);
		}

		sPM = (sPM == null ? "" : sPM);
		sDev = (sDev == null ? "" : sDev);
		sTest = (sTest == null ? "" : sTest);
		sUE = (sUE == null ? "" : sUE);

		if (sPM.Trim() != "" && sDev.Trim() != "" && sTest.Trim() != "" && sUE.Trim() != "") {

//			mailto = String.Format("<a HREF=mailto:");
			mailto = "<a HREF=mailto:";

			if (sPM.Trim() != "") {
				mailto += String.Format("{0}", sPM);
				entryMade = true;

				sPM += " (PM)";
			}

			if (sDev.Trim() != "") {
				if (entryMade) {
					mailto += String.Format("?CC={0}", sDev);
					ccMade = true;
				}
				else {
					mailto += String.Format("{0}", sDev);
					entryMade = true;
				}

				sDev += " (Dev)";
			}

			if (sTest.Trim() != "") {
				if (ccMade) {
					mailto += String.Format(";{0}", sTest);
				}
				else if (entryMade) {
					mailto += String.Format("?CC={0}", sTest);
					ccMade = true;
				}
				else {
					mailto += String.Format("{0}", sTest);
					entryMade = true;
				}

				sTest += " (Test)";
			}

			if (sUE.Trim() != "") {
				if (ccMade) {
					mailto += String.Format(";{0}", sUE);
				}
				else if (entryMade) {
					mailto += String.Format("?CC={0}", sUE);
					ccMade = true;
				}
				else {
					mailto += String.Format("{0}", sUE);
					entryMade = true;
				}

				sUE += " (UE)";
			}

			mailto += String.Format("&Subject={0}>Contact Owners (", subject);

			entryMade = false;

			if (sPM.Trim() != "") {
				mailto += sPM.Trim();
				entryMade = true;
			}

			if (sDev.Trim() != "") {
				if (entryMade)
					mailto += ", ";

				mailto += sDev.Trim();
				entryMade = true;			
			}

			if (sTest.Trim() != "") {
				if (entryMade)
					mailto += ", ";

				mailto += sTest.Trim();
				entryMade = true;			
			}

			if (sUE.Trim() != "") {
				if (entryMade)
					mailto += ", ";

				mailto += sUE.Trim();
				entryMade = true;			
			}

			mailto += ")</a>";

		}
		else {
			mailto = "";

		}

//		 String.Format("<a HREF=mailto:{0}?CC={1};{2};{3}&Subject={4}>Contact Owners (");


//{0}, {1}, {2}, {3})</a>", new object [] {GetPMOwner(TypeFullName), GetDevOwner(TypeFullName), GetTestOwner(TypeFullName), //GetUEOwner(TypeFullName), subject});



//		string mailto = String.Format("<a HREF=mailto:{0}?CC={1};{2};{3}&Subject={4}>Contact Owners ({0}, {1}, {2}, //{3})</a>", new object [] {GetPMOwner(TypeFullName), GetDevOwner(TypeFullName), GetTestOwner(TypeFullName), //GetUEOwner(TypeFullName), subject});

		return _typestring + "(" + mailto + ")";

	}

	public string MemberToHtml() { return MemberToHtml("black"); }
	public string MemberToHtml(string color) {

/*
		if (_memberstring.IndexOf("(i)") < 0 && IsInherited) {
			int found = _memberstring.IndexOf(":");
				
			string output = _memberstring.Substring(0, found + 1) + 
					" <i><font color=blue>(i)</font></i>" + 
					_memberstring.Substring(found + 1);

			return String.Format("<font color=\"{1}\">{0}</font>", output, color);

		}
*/
		//ASSUMPTION: Value will always be the LAST entry
		if (_memberkey.IndexOf("Value=") >= 0 ) {
//Console.WriteLine(_memberkey);
			string s = _memberkey.Substring(_memberkey.IndexOf("Value=") + "Value=".Length);

			//check the memberstring to ensure it doesn't ALREADY contain this...
			if (_memberstring.IndexOf(", <b><i>Value = " + s) < 0)
				_memberstring += ", <b><i>Value = " + s + "</i></b>";
//			else if (_memberstring.IndexOf(", <b><i>Value = " + s) > 
//					_memberstring.LastIndexOf("<td"))
		}

		return String.Format("<font color=\"{1}\">{0}</font>", _memberstring, color);
	}

	// ** static Methods
	public static bool IsSpecialName(MemberInfo member) {
		switch(member.MemberType) {
		case MemberTypes.Field :
			return ((FieldInfo)member).IsSpecialName;
		case MemberTypes.Constructor :
		case MemberTypes.Method :
			return ((MethodBase)member).IsSpecialName;
		case MemberTypes.Property :
			return ((PropertyInfo)member).IsSpecialName;
		case MemberTypes.Event :
			return ((EventInfo)member).IsSpecialName;
		case MemberTypes.NestedType :
		case MemberTypes.TypeInfo :
			return ((Type)member).IsSpecialName;
		default :
			return false;
		}
	}

	public static string GetPMOwner (string typeName) {
		#if DBUGTM
		Console.WriteLine("GetPMOwner({0}):", typeName);
		#endif

		// convert space between namespace and type to a dot.
		string name = (new StringBuilder(typeName)).Replace(" ", ".").ToString();

		Owners2 o = new Owners2();

		string s = "pmowners";
		try {
			#if DBUGTM
			Console.Write("o.GetPMOwner('{0}').Trim() = ", name);
			#endif

			s = o.GetPMOwner(name).Trim();

			#if DBUGTM
			Console.WriteLine("'{0}'", s);
			#endif

			return (s == "pmowners") ? "" : s;
		}
		#if DBUGTM
		catch (Exception e) {
			Console.WriteLine("** Exception Caught **\r\n" + e.ToString());
			return s;
		}
		#else
		catch { return s; }
		#endif
	}

	public static string GetDevOwner (string typeName) {
		string name = (new StringBuilder(typeName)).Replace(" ", ".").ToString();		// convert space between namespace and type to a dot.
		Owners2 o = new Owners2();
		string s = "devowner";
		try { return (((s = o.GetDevOwner(name).Trim()) == "unknown") ? "" : s); }
		catch { return s; }
	}

	public static string GetTestOwner (string typeName) {
		string name = (new StringBuilder(typeName)).Replace(" ", ".").ToString();		// convert space between namespace and type to a dot.
		Owners2 o = new Owners2();
		string s = "testowner";
		try { return (((s = o.GetTestOwner(name).Trim()) == "unknown") ? "" : s); }
		catch { return s; }
	}

	public static string GetUEOwner (string typeName) {
		Owners2 o = new Owners2();
		string name = (new StringBuilder(typeName)).Replace(" ", ".").ToString();		// convert space between namespace and type to a dot.
		string s = "ueowner";
		try { return (((s = o.GetUEOwner(name).Trim()) == "unknown") ? "" : s); }
		catch { return s; }
	}

//this routine sets all the variables for the TypeMember, being all the variables necessary to use a TypeMember...
//you can pass null for many of these...
	public void SetAllVars() {

/*
	string		_typename;		// short type name
	string		_typekey;		// unique identifier string
	string		_typeshortkey;		// signature identifier string
	string		_typestring;		// friendly, C# style description
	string		_typeVBstring;		// friendly, VB style description
	TypeKinds	_typekind;		// TypeKind from GenTypeInfo
	bool		_isabstracttype;	// abstract flag

	string		_membername;		// short member name (nested types are without enclosing type and "$")
	string		_memberkey;		// unique identifier without reflected or declaring type information (helps 
						// when comparing inherited members)

	string		_membershortkey;	// member signature identifier without type information
	string		_memberstring;		// friendly, C# style description
	string		_memberVBstring;	// friendly, VB style description
	MemberTypes	_memberkind;		// from reflection
	bool		_isabstractmember;	// abstract flag
	bool		_isinheritedmember;	// inherited flag

	Version		_version;		// runtime version when store file was created
	string		_namespace;		// namespace reflected type is from
	string		_module;		// module reflected type is from
	string		_assembly;		// assebly reflected type is from

	string		_misc;			// encoding of other information of note.
*/


	}


	//robvi this is the helper class to get the offset of fields
	public static string OffsetOf(Type t, String fieldName)
	{
		if (t == null)
			throw new ArgumentNullException("t"); 
		
		
		FieldInfo f = t.GetField(fieldName, BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Static);
		if(f==null)
		{
			Console.Write("Null: " + fieldName);
		}
		IntPtr offsetHandle;
		object[] args = new object[1];
		args[0] = f;

		try
		{
			offsetHandle = (IntPtr)marshalType.InvokeMember("OffsetOfHelper",BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.InvokeMethod, null,null, args);
			return offsetHandle.ToString();
		}
		catch(Exception)
		{
			return "Unmarshallable";
		}
		
	}


	//Added for serialization field breaking change detection
	//Added by Robvi 09/15/2003
	//this routine makes the string of serializable fields for this type
	//Params: Type t, the type from which to get fields from
	//Rturns: a comma separated list of serializeable fields ended with a semicolon
	public string MakeSerFieldsString(Type t) 
	{

		//THIS SECTION details storing all of the FIELD information for serializable objects,
		// so that we can determine if they have changed from one version to the next...
		TypeAttributes ta = t.Attributes & TypeAttributes.Serializable;
		TypeAttributes sealedType = t.Attributes & TypeAttributes.Sealed;
		bool controlledSer = false;
		bool isSealed = false;
		bool isSerializable = false;
		string serFields = "";

		if(sealedType == TypeAttributes.Sealed)
		{
			isSealed = true;
		}
		
		isSerializable = t.IsSerializable;
		if(isSerializable)
		{
			controlledSer = typeof(System.Runtime.Serialization.ISerializable).IsAssignableFrom(t);
		
			if (controlledSer == false) 
			{
				FieldInfo[] fiAll = t.GetFields(BindingFlags.Public | 
					BindingFlags.NonPublic | BindingFlags.Instance );

				ArrayList al = new ArrayList();
				

				foreach(FieldInfo fi in fiAll)
				{
					bool optionalfield = false;
					object[] attributelist = fi.GetCustomAttributes(false);

					//Following Code is Whidbey only:  We don't want to store fields
					//that have the [OptionalField] attribute
//					foreach(object attr in attributelist)
//					{
//						if(attr is System.Runtime.Serialization.OptionalFieldAttribute)
//						{
//							optionalfield = true;
//						}
//					}
					if(!optionalfield)
						al.Add(fi.FieldType.FullName.ToString() + "=" + fi.Name + "=F");
					else
						al.Add(fi.FieldType.FullName.ToString() + "=" + fi.Name + "=T");

				}

				

				if (al.Count >= 0) 
				{
					al.Sort();
					
					serFields = "SerFields=";
					bool entryMade = false;
					foreach (string s in al) 
					{

						if (!entryMade)
							entryMade = true;
						else
							
							serFields += ",";
						serFields += s;
					}
				}
			

				serFields += ";" + "serializeable=" + isSerializable + ",sealed=" + isSealed + ",controlledSer=" + controlledSer;
				return serFields;
			}
		}
		return "serializeable=" + isSerializable + ",sealed=" + isSealed + ",controlledSer=" + controlledSer;
	}



	//this routine makes the string of structlayout fields for this type...
	public string MakeStructFieldsString(Type t) 
	{
		if (t.Name.Equals("System.DateTime"))
		{
			Console.WriteLine("DateTime");
		}

		//THIS SECTION details storing all of the FIELD information for marshallable objects,
		// so that we can determine if they have changed from one version to the next...
			
		if (t.IsExplicitLayout||t.IsLayoutSequential)
		{
			

			FieldInfo[] fiAll = t.GetFields(BindingFlags.Public | 
				BindingFlags.NonPublic | BindingFlags.Instance );

			ArrayList al = new ArrayList();

			object[] args = new object[1];

			if(!ht.ContainsKey(t.FullName))
			{
				ht.Add(t.FullName,null);
				Console.Write("\nsl:t " + t.FullName);
				if(t.IsExplicitLayout) 
					Console.Write(" sl:l " + "ExplicitLayout")  ;
				else
					Console.Write(" sl:l " + "SequentialLayout");


				foreach(FieldInfo fi in fiAll)
				{
					args[0] = fi;
					//implement offsets at a later time
					string offset = "";
			
				
					Console.Write(" sl:f " + fi.Name);
				


					try
					{
						//offset = Marshal.OffsetOf(fi.DeclaringType, fi.Name).ToString();
						offset = marshalType.InvokeMember("OffsetOfHelper",BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.InvokeMethod, null,null, args).ToString();
					//offset = marshalType.InvokeMember("OffsetOfHelper",BindingFlags.InvokeMethod, null,null, args).ToString();
						//al.Add(fi.FieldType.ToString() + "=" + fi.Name + "=" + marshalType.InvokeMember("OffsetOfHelper",BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.InvokeMethod, null,null, args).ToString());
					}
					catch(Exception e)
					{
							Console.WriteLine(e.Message);
							offset="unmarshallable";
					}
					al.Add(fi.FieldType.ToString() + "=" + fi.Name + "@" + offset);
					//al.Add(fi.FieldType.ToString() + "=" + fi.Name);
				
				}
			}//end if(!ht.ContainsKey(t.FullName))

			if (al.Count > 0) 
			{
				al.Sort();

				string structFields = "StructFields=";
				bool entryMade = false;

				foreach (string s in al) 
				{

					if (!entryMade)
						entryMade = true;
					else
						structFields += ",";
						
					structFields += s;
				}
				//added this in for other check
				structFields += ";";
				return structFields;
			}
		}
		return "";
	}//MakeStructFieldString
		
	//This function returns the string of instance methods for this type...
	
	public string MakeStructMethodsString(Type t) 
	{

		bool comvisibletype = true;
		bool comvisibleasm = true;
		object[] typeattributes = t.GetCustomAttributes(false); 
		object[] asmattributes = t.Assembly.GetCustomAttributes(false);

		foreach (object ta in typeattributes)
		{
			if (ta is System.Runtime.InteropServices.ComVisibleAttribute)
				comvisibletype=((System.Runtime.InteropServices.ComVisibleAttribute)ta).Value;
		}
		
		foreach (object ta in asmattributes)
		{
			if (ta is System.Runtime.InteropServices.ComVisibleAttribute)
				comvisibleasm = ((System.Runtime.InteropServices.ComVisibleAttribute)ta).Value;
		}



		//we're only concerned with instance methods that are public and declared at this type hierarchy level (not inherited)
			MethodInfo[] miAll = t.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);

		
		

			ArrayList al = new ArrayList();

			object[] args = new object[1];

			
			if(!ht.ContainsKey(t.FullName))
			{
				ht.Add(t.FullName,null);
				Console.Write("\nt: " + t.FullName);
			
				foreach(MethodInfo mi in miAll)
				{
					//another change: we are only concerned with methods not explicity marked with ComVisible(false)
					//ignore all methods that are not ComVisible
					object[] methodattributes = mi.GetCustomAttributes(false);
					bool comvisiblemethod = true;
					foreach (object ma in methodattributes)
					{
						if (ma is System.Runtime.InteropServices.ComVisibleAttribute)
						{
							comvisiblemethod = ((System.Runtime.InteropServices.ComVisibleAttribute)ma).Value;
						}
					}

					if (comvisiblemethod)
					{


						ParameterInfo[] pi = mi.GetParameters();
						string parameters = "";
						foreach (ParameterInfo par in pi)
						{
							parameters = parameters + par.ParameterType + " " + par.Name + "*";
						}


						al.Add(mi.Name + "(" + parameters + ")");
						Console.Write("m: " + mi.Name + "\n");
					}
				}
			}

			if (al.Count > 0) 
			{
				al.Sort();

				string structMethods = "StructMethods=";
				bool entryMade = false;

				foreach (string s in al) 
				{

					if (!entryMade)
						entryMade = true;
					else
						structMethods += ",";
						
					structMethods += s;
				}
				
				structMethods += ";" + "comvistype=" + comvisibletype + ",comvisasm=" + comvisibleasm;
				return structMethods;
			}
		
		return "comvisibletype=" + comvisibletype + ",comvisibleasm=" + comvisibleasm;
	}
		



}	// public class TypeMember
}
