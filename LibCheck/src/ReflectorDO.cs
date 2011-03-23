using System.Collections;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using SigHelper;

namespace LibCheck
{

	/// <summary>
	///		Summary description for ReflectorDO.
	/// </summary>
	public class ReflectorDO : BaseDataObject
	{
		//class type
		public const int CLASS_VALUE = 0;
		public const int ENUM_VALUE = 1;
		public const int INTERFACE_VALUE = 2;
		public const int STRUCT_VALUE = 3;
		public const int VALUETYPE_VALUE = 4;

		//Var Direction
		public const int VAR_IN = 0;
		public const int VAR_OUT = 1;
		public const int VAR_REF = 2;

		

		public ReflectorDO() {

		}

		public ReflectorDO(string userID, string password) : base(userID, password) {
		}


		/// <summary>
		/// Insert a namespace object into the namespace table.
		/// </summary>
		/// <param name="build">CLR Build</param>
		/// <param name="ns">Namespace</param>
		public Guid InsertNamespace(string build, string ns) {

			SqlCommand cmd = new SqlCommand("Insert into namespace (oid, namespace) values (@oid, @ns)", this.Connection);			
			Guid g = Guid.NewGuid();

			cmd.Parameters.Add("@oid", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@ns", SqlDbType.VarChar, 150);
		
			cmd.Parameters["@oid"].Value = g;
			cmd.Parameters["@ns"].Value = ns;

			try {
				this.Connection.Open();
				cmd.ExecuteNonQuery();
			} catch(Exception e) {
				Trace.WriteLine(e.ToString());
			} finally {
				if(this.Connection.State == ConnectionState.Open) {
					this.Connection.Close();
				}
			}
			
			return g;		
		}


		public void InsertAssemblyNamespace(Guid assemblyID, Guid namespaceID) {

			SqlCommand cmd = new SqlCommand("Insert into AssemblyNamespaces (namespaceid, assemblyID) values (@namespaceid, @assemblyID)", this.Connection);
			
			cmd.Parameters.Add("@namespaceid", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@assemblyID", SqlDbType.UniqueIdentifier);
		
			cmd.Parameters["@namespaceid"].Value = namespaceID;
			cmd.Parameters["@assemblyID"].Value = assemblyID;
			
			SqlTransaction sqlt = null;

			try {
				this.Connection.Open();
				sqlt = this.Connection.BeginTransaction();
				cmd.Transaction = sqlt;
				cmd.ExecuteNonQuery();
				sqlt.Commit();

			} catch(Exception e) {
				sqlt.Rollback();
				Trace.WriteLine(e.ToString());
			} finally {
				if(this.Connection.State == ConnectionState.Open) {
					this.Connection.Close();
				}
			}
			
		}

		
		
		public Guid InsertAssembly(string name, string version, string culture, string publicKey) {

			SqlCommand cmd = new SqlCommand("Insert into assembly (oid, assemblyname, version, culture, publickeytoken) values (@oid, @name, @version, @culture, @publickeytoken)", this.Connection);

			cmd.Parameters.Add("@oid", SqlDbType.UniqueIdentifier);	
			cmd.Parameters.Add("@name", SqlDbType.VarChar, 200);
			cmd.Parameters.Add("@version", SqlDbType.VarChar, 150);
			cmd.Parameters.Add("@culture", SqlDbType.VarChar, 150);
			cmd.Parameters.Add("@publickeytoken", SqlDbType.VarChar, 150);
		
			Guid g = Guid.NewGuid();
			cmd.Parameters["@oid"].Value = g;
			cmd.Parameters["@name"].Value = name;
			cmd.Parameters["@version"].Value = version;
			cmd.Parameters["@culture"].Value = culture;
			cmd.Parameters["@publickeytoken"].Value = publicKey;
			

			SqlTransaction sqlt = null;
			try {
				this.Connection.Open();
				sqlt = this.Connection.BeginTransaction();
				cmd.Transaction = sqlt;
				cmd.ExecuteNonQuery();
				sqlt.Commit();
			} catch(Exception e) {
				sqlt.Rollback();
				Trace.WriteLine(e.ToString());
			} finally {
				if(this.Connection.State == ConnectionState.Open) {
					this.Connection.Close();
				}
			}

			return g;

		}			

		public void ClearAll() {
			SqlCommand cmd = new SqlCommand("clearall", this.Connection);
			cmd.CommandType = CommandType.StoredProcedure;

			try {
				this.Connection.Open();
				cmd.ExecuteNonQuery();
			} catch(Exception e) {
				Trace.WriteLine(e.ToString());
			} finally {
				if(this.Connection.State == ConnectionState.Open) {
					this.Connection.Close();
				}
			}
		}

	
		public Guid InsertType(Guid assemblyID, Guid namespaceID, Type t) {

			int wType = -1;

			Guid g = Guid.NewGuid();

			string name = t.Name;

			if(t.IsClass) {
				wType = CLASS_VALUE;	
			} else if(t.IsEnum) {
				wType = ENUM_VALUE;
			} else if(t.IsInterface) {
				wType = INTERFACE_VALUE;
			} else if(t.IsValueType) {
				wType = VALUETYPE_VALUE;
			}
			
			string baseType = null; 
			if(null != t.BaseType) {
				baseType = t.BaseType.FullName;
			} else {
				baseType = "";
			}

			int isAbstract = Convert.ToInt32(t.IsAbstract);
			int isAnsiClass = Convert.ToInt32(t.IsAnsiClass);
			int isArray = Convert.ToInt32(t.IsArray);
			int isAutoClass = Convert.ToInt32(t.IsAutoClass);
			int isAutoLayout = Convert.ToInt32(t.IsAutoLayout);
			int isComObject = Convert.ToInt32(t.IsCOMObject);
			int isExplicitLayout = Convert.ToInt32(t.IsExplicitLayout);
			int isLayoutSequential = Convert.ToInt32(t.IsLayoutSequential);
			int isMarshalByRef = Convert.ToInt32(t.IsMarshalByRef);
			int isPrimitive = Convert.ToInt32(t.IsPrimitive);
			int isSealed = Convert.ToInt32(t.IsSealed);
			int isSerializable = Convert.ToInt32(t.IsSerializable);


			//build the sql statement
			string colList = null;
			colList += "oid, AssemblyID, namespaceID, name, Type, abstract, ansiclass, basetype";
			colList += ", array, autoclass, autolayout, comobject, explicitlayout, layoutsequential";
			colList += ", marshalbyref, primitive, sealed, isserializable";

			string valList = "@oid, @AssemblyID, @namespaceID, @name, @Type, @abstract, @ansiclass, @basetype";
			valList += ", @array, @autoclass, @autolayout, @comobject, @explicitlayout, @layoutsequential";
			valList += ", @marshalbyref, @primitive, @sealed, @isserializable";

			SqlCommand cmd = new SqlCommand("Insert into type (" + colList +  ") values (" + valList + ")", this.Connection);

			// add the parameters
			cmd.Parameters.Add("@oid", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@AssemblyID", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@namespaceID", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@name", SqlDbType.VarChar, 200);
			cmd.Parameters.Add("@Type", SqlDbType.Int);
			cmd.Parameters.Add("@abstract", SqlDbType.Int);
			cmd.Parameters.Add("@ansiclass", SqlDbType.Int);
			cmd.Parameters.Add("@basetype", SqlDbType.VarChar, 200);
			cmd.Parameters.Add("@array", SqlDbType.Int);
			cmd.Parameters.Add("@autoclass", SqlDbType.Int);
			cmd.Parameters.Add("@autolayout", SqlDbType.Int);
			cmd.Parameters.Add("@comobject", SqlDbType.Int);
			cmd.Parameters.Add("@explicitlayout", SqlDbType.Int);
			cmd.Parameters.Add("@layoutsequential", SqlDbType.Int);
			cmd.Parameters.Add("@marshalbyref", SqlDbType.Int);
			cmd.Parameters.Add("@primitive", SqlDbType.Int);
			cmd.Parameters.Add("@sealed", SqlDbType.Int);
			cmd.Parameters.Add("@isserializable", SqlDbType.Int);

			//set the value
			cmd.Parameters["@oid"].Value = g;
			cmd.Parameters["@AssemblyID"].Value = assemblyID;
			cmd.Parameters["@namespaceID"].Value = namespaceID;
			cmd.Parameters["@name"].Value = name;
			cmd.Parameters["@Type"].Value = wType;
			cmd.Parameters["@abstract"].Value = isAbstract;
			cmd.Parameters["@ansiclass"].Value = isAbstract;
			cmd.Parameters["@basetype"].Value = baseType;
			cmd.Parameters["@array"].Value = isArray;
			cmd.Parameters["@autoclass"].Value = isAutoClass;
			cmd.Parameters["@autolayout"].Value = isAutoLayout;
			cmd.Parameters["@comobject"].Value = isComObject;
			cmd.Parameters["@explicitlayout"].Value = isExplicitLayout;
			cmd.Parameters["@layoutsequential"].Value = isLayoutSequential;
			cmd.Parameters["@marshalbyref"].Value = isMarshalByRef;
			cmd.Parameters["@primitive"].Value = isPrimitive;
			cmd.Parameters["@sealed"].Value = isSealed;
			cmd.Parameters["@isserializable"].Value = isSerializable;
			
			
			
			
			try {
				this.Connection.Open();
				cmd.ExecuteNonQuery();
			} catch(Exception e) {
				Trace.WriteLine(e.ToString());
			} finally {
				if(this.Connection.State == ConnectionState.Open) {
					this.Connection.Close();
				}
			}

			return g;
		}			
	
		public void InsertParameter(ParameterInfo p, Guid ownerID) {

			Guid g = Guid.NewGuid();

			int varDir = -1;

			if(p.IsIn) {
				varDir = VAR_IN;
			} else if (p.IsOut) {
				varDir = VAR_OUT;
			}

		
			string colList = "oid, ownerid, name, type, vardirection";
			string valList = "@oid, @ownerid, @name, @type, @vardirection";

			SqlCommand cmd = new SqlCommand("Insert into Parameters (" + colList +  ") values (" + valList + ")", this.Connection);

			cmd.Parameters.Add("@oid", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@ownerid", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@name", SqlDbType.VarChar, 200);
			cmd.Parameters.Add("@type", SqlDbType.VarChar, 200);
			cmd.Parameters.Add("@vardirection", SqlDbType.Int);

			cmd.Parameters["@oid"].Value = g;
			cmd.Parameters["@ownerid"].Value = ownerID;
			cmd.Parameters["@name"].Value = p.Name;
			cmd.Parameters["@type"].Value = p.ParameterType.Name;
			cmd.Parameters["@vardirection"].Value = varDir;

			SqlTransaction sqlt = null;
			
			try {
				this.Connection.Open();
				sqlt = this.Connection.BeginTransaction();
				cmd.Transaction = sqlt;
				cmd.ExecuteNonQuery();
				sqlt.Commit();
			} catch(Exception e) {
				Trace.WriteLine(e.ToString());
				sqlt.Rollback();
			} finally {
				if(this.Connection.State == ConnectionState.Open) {
					this.Connection.Close();
				}
			}	
			
		
	
		}

		public Guid InsertProperty(PropertyInfo p, Guid typeID) {
			string rw = null;

			Guid g = Guid.NewGuid();

			//deterime if the propertey is r/w
			MethodInfo[] myi = p.GetAccessors(false);
			if(p.CanRead) {
				rw += "r";
			}
			if(p.CanWrite) {
				rw += "w";
			}
				
			//Determine if property is an indexed propertey
			int isIndexer = 0;
			ParameterInfo[] ip = p.GetIndexParameters();
			if((null != ip) && (ip.Length > 0)) {
				isIndexer = 1;
			}


			string colList = "oid, TypeID, propertyname, accessor, indexer";
			string valList = "@oid, @TypeID, @propertyname, @accessor, @indexer";

			SqlCommand cmd = new SqlCommand("Insert into Properties (" + colList +  ") values (" + valList + ")", this.Connection);

			cmd.Parameters.Add("@oid", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@TypeID", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@propertyname", SqlDbType.VarChar, 200);
			cmd.Parameters.Add("@accessor", SqlDbType.VarChar, 2);
			cmd.Parameters.Add("@indexer", SqlDbType.Int);

			cmd.Parameters["@oid"].Value = g;
			cmd.Parameters["@TypeID"].Value = typeID;
			cmd.Parameters["@propertyname"].Value = p.Name;
			cmd.Parameters["@accessor"].Value = rw;
			cmd.Parameters["@indexer"].Value = isIndexer;


			SqlTransaction sqlt = null;
			
			try {
				this.Connection.Open();
				sqlt = this.Connection.BeginTransaction();
				cmd.Transaction = sqlt;
				cmd.ExecuteNonQuery();
				sqlt.Commit();
			} catch(Exception e) {
				Trace.WriteLine(e.ToString());
				sqlt.Rollback();
			} finally {
				if(this.Connection.State == ConnectionState.Open) {
					this.Connection.Close();
				}
			}
			return g;
		}			


		public Guid InsertMethod(MethodInfo p, Guid typeID, int count) {
//			string rw = null;

			Guid g = Guid.NewGuid();

			string colList = "oid, TypeID, methodname, MethodGroup";
			string valList = "@oid, @TypeID, @methodname, @MethodGroup";

			SqlCommand cmd = new SqlCommand("Insert into Methods (" + colList +  ") values (" + valList + ")", this.Connection);

			cmd.Parameters.Add("@oid", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@TypeID", SqlDbType.UniqueIdentifier);
			cmd.Parameters.Add("@methodname", SqlDbType.VarChar, 200);
			cmd.Parameters.Add("@MethodGroup", SqlDbType.Int);

			cmd.Parameters["@oid"].Value = g;
			cmd.Parameters["@TypeID"].Value = typeID;
			cmd.Parameters["@methodname"].Value = p.Name ;
			cmd.Parameters["@MethodGroup"].Value = count;


			SqlTransaction sqlt = null;
			
			try {
				this.Connection.Open();
				sqlt = this.Connection.BeginTransaction();
				cmd.Transaction = sqlt;
				cmd.ExecuteNonQuery();
				sqlt.Commit();
			} catch(Exception e) {
				Trace.WriteLine(e.ToString());
				sqlt.Rollback();
			} finally {
				if(this.Connection.State == ConnectionState.Open) {
					this.Connection.Close();
				}
			}
			return g;		
		}
		

		public Guid InsertInterface(Type p, Guid typeID) {

			return Guid.NewGuid();
		}
		
		
		
		public void LoadTypeTable() {


			SqlCommand cmd = new SqlCommand("Insert into typedescription (oid, Description) values (@oid, @desc)", this.Connection);

			cmd.Parameters.Add("@oid", SqlDbType.Int);
			cmd.Parameters.Add("@desc", SqlDbType.VarChar, 50);
		
			
			SqlTransaction sqlt = null;

			try {
				this.Connection.Open();
				sqlt = this.Connection.BeginTransaction();
				cmd.Transaction = sqlt;
				
//				int i = 0;

				///////////////////////////////////////
				/// do for each item in type table
				cmd.Parameters["@oid"].Value = -1;
				cmd.Parameters["@desc"].Value = "Unknown";
				cmd.ExecuteNonQuery();

				cmd.Parameters["@oid"].Value = CLASS_VALUE;
				cmd.Parameters["@desc"].Value = "Class";
				cmd.ExecuteNonQuery();

				cmd.Parameters["@oid"].Value = ENUM_VALUE;
				cmd.Parameters["@desc"].Value = "Enum";
				cmd.ExecuteNonQuery();

				cmd.Parameters["@oid"].Value = INTERFACE_VALUE;
				cmd.Parameters["@desc"].Value = "Interface";
				cmd.ExecuteNonQuery();

				cmd.Parameters["@oid"].Value = STRUCT_VALUE;
				cmd.Parameters["@desc"].Value = "Struct";
				cmd.ExecuteNonQuery();

				cmd.Parameters["@oid"].Value = VALUETYPE_VALUE;
				cmd.Parameters["@desc"].Value = "Value Type";
				cmd.ExecuteNonQuery();

				
				///////////////////////////////////////

				
				sqlt.Commit();

			} catch(Exception e) {
				sqlt.Rollback();
				Trace.WriteLine(e.ToString());
			} finally {
				if(this.Connection.State == ConnectionState.Open) {
					this.Connection.Close();
				}
			}
		}			

		public ArrayList GetDistinctFiles(string buildnum) {
			SqlCommand cmd = new SqlCommand("LibCheck_GetDistinctFiles", this.Connection);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("@BuildNum", SqlDbType.VarChar, 500);
			cmd.Parameters["@BuildNum"].Value = buildnum;

			ArrayList al = new ArrayList();

			try {
				if (this.Connection.State == ConnectionState.Closed)
					this.Connection.Open();

				SqlDataReader myReader = cmd.ExecuteReader();

				while (myReader.Read()) {				    
					al.Add(myReader["FileLoc"].ToString());
				}
				
				myReader.Close();
			} catch(Exception e) {
//				Console.WriteLine("noo!");
				Trace.WriteLine(e.ToString());
			}

			return al;
		}

		public Hashtable GetKitType(string fileloc) {
			SqlCommand cmd = new SqlCommand("LibCheck_GetTypes", this.Connection);
			cmd.CommandType = CommandType.StoredProcedure;

			//Should REALLY be retrieving by section...
			cmd.Parameters.Add("@FileLoc", SqlDbType.VarChar, 500);
			cmd.Parameters["@FileLoc"].Value = fileloc;
			
			ArrayList al = new ArrayList();
			Hashtable ht = new Hashtable();

			string tmID = "";
			string section = "";

			try {
				if (this.Connection.State == ConnectionState.Closed)
					this.Connection.Open();

				SqlDataReader myReader = cmd.ExecuteReader();

				while (myReader.Read()) {

				    if (tmID == "") {
					tmID = myReader["TypeMemberID"].ToString();

					//SHOULD MAP ONE-TO-ONE TO FileLoc, only copy once
					section = myReader["NameSection"].ToString();
//					htTemp = new Hashtable();
				    }
				    else if (tmID != myReader["TypeMemberID"].ToString()) {
					ht.Add(tmID,al);
					tmID = myReader["TypeMemberID"].ToString();
					al = new ArrayList();
				    }
//Console.WriteLine(tmID);
//Console.ReadLine();
				    //reconstruct the TypeMember...
				    TypeMember tm = new TypeMember();

				    tm.IsInherited = 
					(Convert.ToInt32(myReader["IsInherited"]) == 0) ? false:true;
				    tm.TypeName = myReader["TypeName"].ToString();
				    tm.TypeKey = myReader["TypeKey"].ToString();
				    tm.TypeString = myReader["TypeString"].ToString();
				    tm.TypeShortKey = myReader["TypeShortKey"].ToString();
				    tm.TypeKind = (TypeKinds)Convert.ToInt32(myReader["TypeKind"]);
				    tm.IsAbstractType = 
					(Convert.ToInt32(myReader["IsAbstractType"]) == 0) ? false:true;
				    tm.MemberName = myReader["MemberName"].ToString();
				    tm.MemberKey = myReader["MemberKey"].ToString();
				    tm.MemberShortSig = myReader["MemberShortSig"].ToString();
				    tm.MemberString = myReader["MemberString"].ToString();
				    tm.MemberKind = (MemberTypes)Convert.ToInt32(myReader["MemberKind"]);
				    tm.IsAbstractMember = 
					(Convert.ToInt32(myReader["IsAbstract"]) == 0) ? false:true;
				    tm.Namespace = myReader["Namespace"].ToString();
//Console.WriteLine(tmID + ", " + tm.MemberName);
				    al.Add(tm);
				}

				myReader.Close();
			} catch(Exception e) {
				Console.WriteLine(e.ToString());
				Trace.WriteLine(e.ToString());
			}
//Console.ReadLine();
			//need this last one
			ht.Add(tmID,al);

			return ht;
		}

		public void InsertKitType(Hashtable ht, string file, string section, string buildnum) {
//Console.WriteLine(section);
//Console.WriteLine(file);
//Console.ReadLine();
//return;
			SqlTransaction sqlt = null;

			try {
				if (this.Connection.State == ConnectionState.Closed)
					this.Connection.Open();
				sqlt = this.Connection.BeginTransaction();

				SqlCommand cmd = new SqlCommand("LibCheck_AddTypeMember", this.Connection);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Transaction = sqlt;

				foreach(string typememberID in ht.Keys) {
				    foreach (TypeMember tm in (ArrayList)ht[typememberID]) {

				    Guid g = Guid.NewGuid();

				    cmd.Parameters.Add("@TypeID", SqlDbType.UniqueIdentifier);
				    cmd.Parameters.Add("@NameSection", SqlDbType.VarChar, 500);
				    cmd.Parameters.Add("@TypeMemberID", SqlDbType.VarChar, 100);
				    cmd.Parameters.Add("@BuildNum", SqlDbType.VarChar, 500);
				    cmd.Parameters.Add("@FileLoc", SqlDbType.VarChar, 500);
				    cmd.Parameters.Add("@IsInherited", SqlDbType.Int);
				    cmd.Parameters.Add("@TypeName", SqlDbType.VarChar, 1000);
				    cmd.Parameters.Add("@TypeKey", SqlDbType.VarChar, 1000);
				    cmd.Parameters.Add("@TypeString", SqlDbType.VarChar, 1500);
				    cmd.Parameters.Add("@TypeShortKey", SqlDbType.VarChar, 1500);
				    cmd.Parameters.Add("@TypeKind", SqlDbType.Int);
				    cmd.Parameters.Add("@IsAbstractType", SqlDbType.Int);
				    cmd.Parameters.Add("@MemberName", SqlDbType.VarChar, 1000);
				    cmd.Parameters.Add("@MemberKey", SqlDbType.VarChar, 1000);
				    cmd.Parameters.Add("@MemberShortSig", SqlDbType.VarChar, 1500);
				    cmd.Parameters.Add("@MemberString", SqlDbType.VarChar, 1500);
				    cmd.Parameters.Add("@MemberKind", SqlDbType.Int);
				    cmd.Parameters.Add("@IsAbstract", SqlDbType.Int);
				    cmd.Parameters.Add("@Namespace", SqlDbType.VarChar, 500);

				    cmd.Parameters["@TypeID"].Value = g;
				    cmd.Parameters["@NameSection"].Value = section;
				    cmd.Parameters["@TypeMemberID"].Value = typememberID;
				    cmd.Parameters["@BuildNum"].Value = buildnum;
				    cmd.Parameters["@FileLoc"].Value = file;
				    cmd.Parameters["@IsInherited"].Value = tm.IsInherited ;
				    cmd.Parameters["@TypeName"].Value = tm.TypeName;
				    cmd.Parameters["@TypeKey"].Value = tm.TypeKey;
				    cmd.Parameters["@TypeString"].Value = tm.TypeString ;
				    cmd.Parameters["@TypeShortKey"].Value = tm.TypeShortKey;
				    cmd.Parameters["@TypeKind"].Value = tm.TypeKind;
				    cmd.Parameters["@IsAbstractType"].Value = tm.IsAbstractType ;
				    cmd.Parameters["@MemberName"].Value = tm.MemberName;
				    cmd.Parameters["@MemberKey"].Value = tm.MemberKey;
				    cmd.Parameters["@MemberShortSig"].Value = tm.MemberShortSig ;
				    cmd.Parameters["@MemberString"].Value = tm.MemberString;
				    cmd.Parameters["@MemberKind"].Value = tm.MemberKind;
				    cmd.Parameters["@IsAbstract"].Value = tm.IsAbstractMember ;
				    cmd.Parameters["@Namespace"].Value = tm.Namespace;

				    cmd.ExecuteNonQuery();
				    cmd.Parameters.Clear();
				}
				}

				sqlt.Commit();
			} catch(Exception e) {
				Console.WriteLine("NOOOOOOO! " + e.ToString());
				Trace.WriteLine(e.ToString());
				sqlt.Rollback();
			}	
		}	
	}
}
