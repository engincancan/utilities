using System;

using System.Web.Services;
using System.IO;
using System.Collections;
using System.Data;

using System.Data.SqlClient;

namespace LibCheck {

public class Owners2 {

	static DataSet ds;
	public static Boolean dbNotAvailable = false;

	public Owners2 ()
	{
		try {

			Reload ();
		} catch {

			dbNotAvailable = true;
		}
         }
    

	[WebMethod]
	public void Reload () {

		SqlConnection myConnection;
	        myConnection = new SqlConnection("server=chriskg1;uid=sam;pwd=peanut~01;database=pubs");

        	myConnection.Open();
	        ds = new DataSet();

		SqlDataAdapter tAdapt = new SqlDataAdapter();

		tAdapt.SelectCommand = new SqlCommand("SELECT * FROM Owners",myConnection);
		tAdapt.Fill(ds, "Owners");

        	myConnection.Close();
	}

	[WebMethod]
	public string GetPMOwner(string namespaceName) {

		string name = null;

		while (name == null)
		{

			foreach(DataRow dr in ds.Tables["Owners"].Rows) {

				if (Convert.ToString(dr["NamespaceElement"]).ToUpper() == namespaceName.ToUpper() )
					return Convert.ToString(dr["PMOwner"]);
			}
		     
			int index = namespaceName.LastIndexOf (".");

			if (index == -1) {
				name = "unknown";
				break;
			}

			namespaceName = namespaceName.Remove (index, namespaceName.Length - index);
		}

		return name;
	}


	[WebMethod]
	public string GetDevOwner(string namespaceName) {        

		string name = null;

		while (name == null)
		{
			foreach(DataRow dr in ds.Tables["Owners"].Rows) {
		     
				if (Convert.ToString(dr["NamespaceElement"]).ToUpper() == namespaceName.ToUpper() )
					return Convert.ToString(dr["DevOwner"]);

			}		     
			
			int index = namespaceName.LastIndexOf (".");

			if (index == -1)
			{
				name = "unknown";
				break;
			}

			namespaceName = namespaceName.Remove (index, namespaceName.Length - index);	
		}

		return name;
	}


	[WebMethod]
	public string GetUEOwner(string namespaceName) {

		string name = null;

		while (name == null) {

			foreach(DataRow dr in ds.Tables["Owners"].Rows) {

				if (Convert.ToString(dr["NamespaceElement"]).ToUpper() == namespaceName.ToUpper() )
					return Convert.ToString(dr["UEOwner"]);

			}
		     
			int index = namespaceName.LastIndexOf (".");

			if (index == -1) {
				name = "unknown";
				break;
			}

			namespaceName = namespaceName.Remove (index, namespaceName.Length - index);		
		}

		return name;	
	}

	[WebMethod]
	public string GetTestOwner(string namespaceName) {    

		string name = null;

		while (name == null) {

			foreach(DataRow dr in ds.Tables["Owners"].Rows) {
				if (Convert.ToString(dr["NamespaceElement"]).ToUpper() == namespaceName.ToUpper() )
					return Convert.ToString(dr["TestOwner"]);

			}		     
			
			int index = namespaceName.LastIndexOf (".");

			if (index == -1) {
				name = "unknown";
				break;
			}

			namespaceName = namespaceName.Remove (index, namespaceName.Length - index);			
		}

		return name;	
	}

}
}