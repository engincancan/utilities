namespace LibCheck
{

	using System;
	using System.ComponentModel;
	using System.Data;
	using System.Data.SqlClient;
	using System.Diagnostics;


	/// <summary>
	///		Summary description for DataObject.
	/// </summary>
	public class BaseDataObject
	{
		string connectionString = null;
		SqlConnection workConn = null;

		[Description("Base Constructor")]
		public BaseDataObject() {
			connectionString = "Server=delphi;Database=LibCheck;UID=LibCheckAdmin;pwd=LibCheck";
			Connect(connectionString);
		}

		public BaseDataObject(string connString) {
			if(null == connString) {
				throw new ArgumentNullException("connString", "connString may not be null");
			}
			Connect(connString);
		}

		public BaseDataObject(string userID, string password) {
			connectionString = "Server=delphi;Database=LibCheck;";
			connectionString += "UID=" + userID + ";PWD=" + password + ";";

			Trace.WriteLine(string.Format("Connecting as \"{0}\"", userID));

			Connect(connectionString);
		}

		protected SqlConnection Connection {
			get {
				return this.workConn;
			}
		}


		[Description("Create a SqlConnection")]
		protected void Connect(string connString) {

			Trace.WriteLine("Connecting...");
			try {
				workConn = new SqlConnection(connectionString);
				workConn.Open();
				Trace.WriteLine("Connected to: " + workConn.Database);
			} catch(Exception e){
				Trace.WriteLine("Connection failed...");
				Trace.WriteLine("Error: " + e.ToString());
			}


		}

		public void Close() {
			workConn.Close();
		}
	}
}
