using System;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.Collections.Specialized;

namespace QueryExpress
{
	#region Browser Interface
	/// <summary>
	/// An interface defining Browser classes (an Explorer-like tree view of a database).
	/// </summary>
	public interface IBrowser
	{
		/// <summary>
		/// Returns the active Database Client object (this should be set in construction)
		/// </summary>
		DbClient DbClient {get;}

		/// <summary>
		/// Returns an array of TreeNodes representing the object hierarchy for the "Explorer" view.
		/// This can return either the entire hierarchy, or for efficiency, just the higher level(s).
		/// </summary>
		TreeNode[] GetObjectHierarchy();

		/// <summary>
		/// Returns an array of TreeNodes representing the object hierarchy below a given node.
		/// This should return null if there is no hierarchy below the given node, or if the hierarchy
		/// is already present.  This method is called whenever the user expands a node.
		/// </summary>
		TreeNode[] GetSubObjectHierarchy (TreeNode node);

		/// <summary>
		/// Returns text suitable for dropping into a query window, for a given node.
		/// </summary>
		string GetDragText (TreeNode node);

		/// <summary>
		/// Returns a list of actions applicable to a node (suitable for a context menu).
		/// Returns null if no actions are applicable.
		/// </summary>
		StringCollection GetActionList (TreeNode node);

		/// <summary>
		/// Returns text suitable for pasting into a query window, given a particular node and action.
		/// GetActionList() should be called first to obtain a list of applicable actions.
		/// </summary>
		/// <param name="actionIndex">One of the action text strings returned by GetActionList()</param>
		string GetActionText (TreeNode node, string action);

		/// <summary>
		/// Returns a list of available databases
		/// </summary>
		string[] GetDatabases();

		/// <summary>
		/// Creates and returns a new browser object, using the supplied database client object.
		/// </summary>
		IBrowser Clone (DbClient newDbClient);
	}
	#endregion

	#region SQL Server Browser
	/// <summary>
	/// An implementation of IBrowser for MS SQL Server.
	/// </summary>
	public class SqlBrowser : IBrowser
	{
		class SqlNode : TreeNode
		{
			internal string type = "";
			internal string name, owner, safeName, dragText;
			public SqlNode (string text) : base (text) {}
		}

		const int timeout = 5;
		DbClient dbClient;

		public SqlBrowser (DbClient dbClient)
		{
			this.dbClient = dbClient;
		}

		public DbClient DbClient
		{
			get {return dbClient;}
		}

		public TreeNode[] GetObjectHierarchy()
		{
			TreeNode[] top = new TreeNode[]
			{
				new TreeNode ("User Tables"),
				new TreeNode ("System Tables"),
				new TreeNode ("Views"),
				new TreeNode ("User Stored Procs"),
				new TreeNode ("MS Stored Procs"),
				new TreeNode ("Functions")
			};

			DataSet ds = dbClient.Execute ("select type, ObjectProperty (id, N'IsMSShipped') shipped, object_name(id) object, user_name(uid) owner from sysobjects where type in (N'U', N'S', N'V', N'P', N'FN') order by object, owner", timeout);
			if (ds == null || ds.Tables.Count == 0) return null;

			foreach (DataRow row in ds.Tables[0].Rows)
			{
				string type = row ["type"].ToString().Substring (0, 2).Trim();
				
				int position;
				if (type == "U") position = 0;										// user table
				else if (type == "S") position = 1;								// system table
				else if (type == "V") position = 2;								// view
				else if (type == "FN") position = 5;								// function
				else if ((int) row ["shipped"] == 0) position = 3;				// user stored proc
				else position = 4;														// MS stored proc

				string prefix = row ["owner"].ToString() == "dbo" ? "" : row ["owner"].ToString() + ".";
				SqlNode node = new SqlNode (prefix + row ["object"].ToString());
				node.type = type;
				node.name = row ["object"].ToString();
				node.owner = row ["owner"].ToString();

				// If the object name contains a space, wrap the "safe name" in square brackets.
				if (node.owner.IndexOf (' ') >= 0 || node.name.IndexOf (' ') >= 0)
				{
					node.safeName = "[" + node.name + "]";
					node.dragText = "[" + node.owner + "].[" + node.name + "]";
				}
				else
				{
					node.safeName = node.name;
					node.dragText = node.owner + "." + node.name;
				}
				top [position].Nodes.Add (node);

				// Add a dummy sub-node to user tables and views so they'll have a clickable expand sign
				// allowing us to have GetSubObjectHierarchy called so the user can view the columns
				if (type == "U" || type == "V") node.Nodes.Add (new TreeNode());
			}
			return top;
		}

		public TreeNode[] GetSubObjectHierarchy (TreeNode node)
		{
			// Show the column breakdown for the selected table
			if (node is SqlNode)
			{
				SqlNode sn = (SqlNode) node;
				if (sn.type == "U" || sn.type == "V")					// break down columns for user tables and views
				{
					DataSet ds = dbClient.Execute ("select COLUMN_NAME name, DATA_TYPE type, CHARACTER_MAXIMUM_LENGTH clength, NUMERIC_PRECISION nprecision, NUMERIC_SCALE nscale, IS_NULLABLE nullable  from INFORMATION_SCHEMA.COLUMNS where TABLE_CATALOG = db_name() and TABLE_SCHEMA = '"
						+ sn.owner +  "' and TABLE_NAME = '" + sn.name + "' order by ORDINAL_POSITION", timeout);
					if (ds == null || ds.Tables.Count == 0) return null;

					TreeNode[] tn = new SqlNode [ds.Tables[0].Rows.Count];
					int count = 0;

					foreach (DataRow row in ds.Tables[0].Rows)
					{
						string length;
						if (row ["clength"].ToString() != "")
							length = "(" + row ["clength"].ToString() + ")";
						else if (row ["nprecision"].ToString() != "")
							length = "(" + row ["nprecision"].ToString() + ","	 + row ["nscale"].ToString() + ")";
						else length = "";

						string nullable = row ["nullable"].ToString().StartsWith ("Y") ? "null" : "not null";

						SqlNode column = new SqlNode (row ["name"].ToString() + " ("
							+ row ["type"].ToString() + length + ", " + nullable + ")");
						column.type = "CO";			// column
						column.dragText = row ["name"].ToString();
						if (column.dragText.IndexOf (' ') >= 0)
							column.dragText = "[" + column.dragText+ "]";
						column.safeName = column.dragText;
						tn [count++] = column;
					}
					return tn;
				}
			}
			return null;
		}

		public string GetDragText (TreeNode node)
		{
			if (node is SqlNode)
				return ((SqlNode) node).dragText;
			else
				return "";
		}

		public StringCollection GetActionList (TreeNode node)
		{
			if (!(node is SqlNode)) return null;

			SqlNode sn = (SqlNode) node;
			StringCollection output = new StringCollection();

			if (sn.type == "U" || sn.type == "S" || sn.type == "V")
			{
				output.Add ("select * from " + sn.safeName);
				output.Add ("sp_help " + sn.safeName);
				if (sn.type != "V")
				{
					output.Add ("sp_helpindex " + sn.safeName);
					output.Add ("sp_helpconstraint " + sn.safeName);
					output.Add ("sp_helptrigger " + sn.safeName);
				}
				output.Add ("(insert all fields)");
				output.Add ("(insert all fields, table prefixed)");
			}

			if (sn.type == "V" || sn.type == "P" || sn.type == "FN")
				output.Add ("View / Modify " + sn.name);

			if (sn.type == "CO" && ((SqlNode) sn.Parent).type == "U")
				output.Add ("Alter column...");

			return output.Count == 0 ? null : output;
		}

		public string GetActionText (TreeNode node, string action)
		{
			if (!(node is SqlNode)) return null;

			SqlNode sn = (SqlNode) node;

			if (action.StartsWith ("select * from ") || action.StartsWith ("sp_"))
				return action;

			if (action.StartsWith ("(insert all fields"))
			{
				StringBuilder sb = new StringBuilder();
				// If the table-prefixed option has been selected, add the table name to all the fields
				string prefix = action == "(insert all fields)" ? "" : sn.safeName + ".";
				int chars = 0;
				foreach (TreeNode subNode in GetSubObjectHierarchy (node))
				{
					if (chars > 50)
					{
						chars = 0;
						sb.Append ("\r\n");
					}
					string s = (sb.Length == 0 ? "" : ", ") + prefix + ((SqlNode) subNode).dragText;
					chars += s.Length;
					sb.Append (s);
				}
				return sb.Length == 0 ? null : sb.ToString();
			}

			if (action.StartsWith ("View / Modify "))
			{
				DataSet ds = dbClient.Execute ("sp_helptext " + sn.safeName, timeout);
				if (ds == null || ds.Tables.Count == 0) return null;

				StringBuilder sb = new StringBuilder();
				bool altered = false;
				foreach (DataRow row in ds.Tables[0].Rows)
				{
					string line = row[0].ToString();
					if (!altered && line.Trim().ToUpper().StartsWith ("CREATE"))
					{
						sb.Append ("ALTER" + line.Trim().Substring (6, line.Trim().Length - 6) + "\r\n");
						altered = true;
					}
					else
						sb.Append (line);
				}
				return sb.ToString().Trim();
			}

			if (action == "Alter column...")
				return "alter table " + ((SqlNode) sn.Parent).dragText + " alter column " + sn.safeName + " ";

			return null;
		}

		public string[] GetDatabases()
		{
			// cool, but only supported in SQL Server 2000+
			DataSet ds = dbClient.Execute ("dbo.sp_MShasdbaccess", timeout);
			// works in SQL Server 7...
			if (ds == null || ds.Tables.Count == 0)
				ds = dbClient.Execute ("select name from master.dbo.sysdatabases order by name", timeout);
			if (ds == null || ds.Tables.Count == 0) return null;
			string[] sa = new string [ds.Tables[0].Rows.Count];
			int count = 0;
			foreach (DataRow row in ds.Tables[0].Rows)
				sa [count++] = row[0].ToString().Trim();
			return sa;
		}

		public IBrowser Clone (DbClient newDbClient)
		{
			SqlBrowser sb = new SqlBrowser (newDbClient);
			return sb;
		}
	}
	#endregion

	#region Simple Oracle Browser
	/// <summary>
	/// A simple implementation of IBrowser for Oracle.  No support for SPs, packages, etc
	/// </summary>
	public class OracleBrowser : IBrowser
	{
		class OracleNode : TreeNode
		{
			internal string type = "";
			internal string dragText = "";
			public OracleNode (string text) : base (text) {}
		}

		const int timeout = 5;
		DbClient dbClient;

		public OracleBrowser (DbClient dbClient)
		{
			this.dbClient = dbClient;
		}

		public DbClient DbClient
		{
			get {return dbClient;}
		}

		public TreeNode[] GetObjectHierarchy()
		{
			TreeNode[] top = new TreeNode[]
			{
				new TreeNode ("User Tables"),
				new TreeNode ("User Views"),
			};

			DataSet ds = dbClient.Execute ("select TABLE_NAME from USER_TABLES", timeout);
			if (ds == null || ds.Tables.Count == 0) return null;

			foreach (DataRow row in ds.Tables[0].Rows)
			{
				OracleNode node = new OracleNode (row [0].ToString());
				node.type = "T";
				node.dragText = node.Text;
				top [0].Nodes.Add (node);
				// Add a dummy sub-node to user tables and views so they'll have a clickable expand sign
				// allowing us to have GetSubObjectHierarchy called so the user can view the columns
				node.Nodes.Add (new TreeNode());
			}

			ds = dbClient.Execute ("select VIEW_NAME from USER_VIEWS", timeout);
			if (ds == null || ds.Tables.Count == 0) return top;

			foreach (DataRow row in ds.Tables[0].Rows)
			{
				OracleNode node = new OracleNode (row [0].ToString());
				node.type = "V";
				node.dragText = node.Text;
				top [1].Nodes.Add (node);
				// Add a dummy sub-node to user tables and views so they'll have a clickable expand sign
				// allowing us to have GetSubObjectHierarchy called so the user can view the columns
				node.Nodes.Add (new TreeNode());
			}

			return top;
		}

		public TreeNode[] GetSubObjectHierarchy (TreeNode node)
		{
			// Show the column breakdown for the selected table
			
			if (node is OracleNode)
			{
				OracleNode on = (OracleNode) node;
				if (on.type == "T" || on.type == "V")
				{
					DataSet ds = dbClient.Execute ("select COLUMN_NAME name, DATA_TYPE type, DATA_LENGTH clength, DATA_PRECISION nprecision, DATA_SCALE nscale, NULLABLE nullable from USER_TAB_COLUMNS where TABLE_NAME = '"
						+ on.Text + "'", timeout);
					if (ds == null || ds.Tables.Count == 0) return null;

					TreeNode[] tn = new OracleNode [ds.Tables[0].Rows.Count];
					int count = 0;

					foreach (DataRow row in ds.Tables[0].Rows)
					{
						string length;
						if (row ["clength"].ToString() != "")
							length = "(" + row ["clength"].ToString() + ")";
						else if (row ["nprecision"].ToString() != "")
							length = "(" + row ["nprecision"].ToString() + ","	 + row ["nscale"].ToString() + ")";
						else length = "";

						string nullable = row ["nullable"].ToString().StartsWith ("Y") ? "null" : "not null";

						OracleNode column = new OracleNode (row ["name"].ToString() + " ("
							+ row ["type"].ToString() + length + ", " + nullable + ")");

						column.dragText = row ["name"].ToString();

						tn [count++] = column;
					}
					return tn;
				}
			}
			return null;
		}

		public string GetDragText (TreeNode node)
		{
			if (node is OracleNode)
				return ((OracleNode) node).dragText;
			else
				return null;
		}

		public StringCollection GetActionList (TreeNode node)
		{
			if (!(node is OracleNode)) return null;

			OracleNode on = (OracleNode) node;
			StringCollection output = new StringCollection();

			if (on.type == "T" || on.type == "V")
			{
				output.Add ("select * from " + on.dragText);
			}

			return output.Count == 0 ? null : output;
		}

		public string GetActionText (TreeNode node, string action)
		{
			if (!(node is OracleNode)) return null;
			OracleNode on = (OracleNode) node;
			if (action.StartsWith ("select * from "))
				return action;
			else
				return null;
		}

		public string[] GetDatabases()
		{
			return new String[] {dbClient.Database};
		}

		public IBrowser Clone (DbClient newDbClient)
		{
			OracleBrowser ob = new OracleBrowser (newDbClient);
			return ob;
		}
	}
	#endregion

}
