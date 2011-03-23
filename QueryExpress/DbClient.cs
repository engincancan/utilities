namespace QueryExpress
{
	using System;
	using System.Text;
	using System.Windows.Forms;
	using System.Data;
	using System.Data.OleDb;
	using System.Data.SqlClient;
	using System.Threading;
	using System.Text.RegularExpressions;
	using System.Collections;
	using System.Globalization;

	#region Client Factory Classes

	/// <summary> Abstract factory for obtaining provider-appropriate ADO.NET data objects. </summary>
	public interface IClientFactory
	{
		IDbConnection GetConnection (string connectString);
		IDbDataAdapter GetDataAdapter (string query, IDbConnection connection);
	}

	// Concrete Client Factory implementations.  Each implementation  creates ADO.NET objects
	// appropriate to a particular provider, such as SQL Server.  To enable native  support for another
	// provider's ADO.NET classes, define another client factory class below, and instatiate it in
	// ConnectForm.  DbClient itself can also be subclassed if any special behaviour is required.

	/// <summary> SQL Server Client </summary>
	public class SqlFactory: IClientFactory
	{
		public IDbConnection GetConnection (string connectString)
		{
			return new SqlConnection (connectString);
		}
		public IDbDataAdapter GetDataAdapter (string query, IDbConnection connection)
		{
			return new SqlDataAdapter (query, (SqlConnection) connection);
		}
	}

	/// <summary> OLE-DB Client </summary>
	public class OleDbFactory: IClientFactory
	{
		public IDbConnection GetConnection (string connectString)
		{
			return new OleDbConnection (connectString);
		}
		public IDbDataAdapter GetDataAdapter (string query, IDbConnection connection)
		{
			return new OleDbDataAdapter (query, (OleDbConnection) connection);
		}
	}

	#endregion

	public enum RunState {Idle, Running, Cancelling};

	/// <summary>
	/// Class for managing database connectivity.  This is instantiated and configured by ConnectForm,
	/// and then handed to QueryForm.
	/// </summary>
	public class DbClient
	{
		#region Fields
		public const string DateTimeFormatString = "yyyy'-'MM'-'dd HH':'mm':'ss";
		public const int DateTimeFormatStringLength = 19;
		protected int maxTextWidth = 60;					// maximum column width for text-based results
		protected IClientFactory clientFactory;			// the factory object that will create our connection & adapters
		protected string connectString = "";	
		protected string connectDescription = "";
		protected IDbConnection connection;				// a connection object (created by the client factory)
		protected IDbDataAdapter dataAdapter;		// the data adapter for executing queries into datasets
		protected IDataReader dataReader;					// data reader object for use with text results
		protected bool connected = false;
		protected bool buildingResults = false;
		protected RunState runState = RunState.Idle;
		protected string error;
		protected Thread workerThread;							// the background thread for running queries
		protected MethodInvoker task = null;					// next task for the worker thread
		protected DataSet dataSet = new DataSet();		// the DataSet we're going to fill
		protected StringBuilder textResults;					// text repository for results (alternative to DataSet)
		protected Control host;												// the host (form) with whom we'll communicate as the query progresses
		protected MethodInvoker queryResults;				// a delegate to inform when the query has some results
		protected MethodInvoker queryDone;					// a delegate to inform when the query completes successfully
		protected MethodInvoker queryFailed;				// a delegate to inform when the query fails
		protected MethodInvoker cancelDone;				// a delegate to inform when cancel action has completed
		protected IList queries;												// separated subqueries (separator = 'GO')
		protected DataSet syncDataSet;							// a dataset for running synchornous queries
		protected string syncQuery;
		protected int timeout;
		#endregion

		#region Constructor
		/// <summary> Creates a new database client object </summary>
		/// <param name="clientFactory">A provider-specific ClientFactory class</param>
		/// <param name="connectString">The connection string</param>
		/// <param name="connectDescription">Human-readable connection description</param>
		public DbClient (IClientFactory clientFactory, string connectString, string connectDescription)
		{
			this.clientFactory = clientFactory;
			this.connectString = connectString;
			this.connectDescription = connectDescription;
			// Given that the OleDb classes appear to be apartment threaded, we can't just spawn
			// worker threads to execute background queries as required, since the connection will
			// have been created on a different thread.  The easiest way around this is to start a worker
			// thread now, keeping it alive for the duration of the DbClient object, and have that 
			// thread process all database commands - connections, disconnections, queries, etc.
			this.workerThread = new Thread (new ThreadStart (StartWorker));
			workerThread.Name = "DbClient Worker Thread";
			workerThread.Start();
		}
		#endregion

		#region Public Properties

		/// <summary> An object for creating database connections & adapters </summary>
		public virtual IClientFactory ClientFactory
		{
			get {return clientFactory;}
		}

		/// <summary> The database connection string </summary>
		public virtual string ConnectString
		{
			get {return connectString;}
		}

		/// <summary> A human-readable version of the connection string </summary>
		public virtual string ConnectDescription
		{
			get {return connectDescription;}
		}

		public IDbConnection Connection
		{
			get { return connection; }
		}

		/// <summary> The dataset to which results are assigned (unless in Text mode) </summary>
		public virtual DataSet DataSet 
		{
			get {return dataSet;}
		}

		/// <summary> The repository for text results </summary>
		public virtual StringBuilder TextResults 
		{
			get {return textResults;}
		}

		/// <summary> The current state of query execution </summary>
		public virtual RunState RunState
		{
			get {return runState;}
		}

		public virtual string Database
		{
			get {return connection.Database;}
			set 
			{
				if (connection.Database != value)
					try {connection.ChangeDatabase (value);}
					catch (Exception e) {MessageBox.Show ("Cannot change database: " + e.Message);}
			}
		}

		/// <summary>A descriptive error message indicating why the last query failed</summary>
		public virtual string Error
		{
			get {return error;}
		}

		#endregion

		#region Public Methods

		/// <summary> This must be called before calling Execute</summary>
		public virtual bool Connect()
		{
			if (connected) return true;
			// Even though we're connecting synchronously, we have to marshal the
			// call onto the worker thread, otherwise if the connection object will be locked
			// into the main thread's apartment.
			RunOnWorker (new MethodInvoker (DoConnect), true);
			return connected;
		}

		/// <summary> Execute a query asynchronously.  Connect() must be called first. </summary>
		public virtual void Execute (
			Control host,										// the host on which we'll invoke the methods below
			MethodInvoker queryResults,		// method to invoke when results are present
			MethodInvoker queryDone,			// method to invoke when query has completed
			MethodInvoker queryFailed,			// method to invoke when query has failed
			string query,										// the actual query to run
			bool writeTextResults)						// results should be displayed in textbox rather than grid
		{
			// Save parameters to fields of this object, so they'll be visible to the worker thread
			this.host = host;
			this.queryResults = queryResults;
			this.queryDone = queryDone;
			this.queryFailed = queryFailed;

			// If 'GO' keyword is present, separate each subquery, so they can be run separately.
			// Use Regex class, as we need a case insensitive match.

			Regex r = new Regex (@"\bGO\b", RegexOptions.IgnoreCase);
			MatchCollection mc = r.Matches (query);
			queries = new ArrayList();
			int pos = 0;
			foreach (Match m in mc) {
				string sub = query.Substring (pos, m.Index - pos).Trim();
				if (sub.Length > 0) queries.Add (sub);
				pos = m.Index + m.Length + 1;
			}

			if (pos < query.Length) {
				string finalQuery = query.Substring (pos).Trim();
				if (finalQuery.Length > 0) queries.Add (finalQuery);
			}

			// Get a data adapter appropriate to the type of connection we have (SQL, Oracle, etc)
			dataAdapter = this.ClientFactory.GetDataAdapter ("", connection);

			runState = RunState.Running;
			buildingResults = false;

			if (writeTextResults)
				RunOnWorker (new MethodInvoker (FillTextResults));
			else
				RunOnWorker (new MethodInvoker (FillDataSet));
			return;
		}

		/// <summary> Execute simple query synchronously (ie wait for it to complete)</summary>
		public virtual DataSet Execute (string query, int timeout)
		{
			// Even though we just want to run a simple synchronous query, we have to marshal
			// to the worker thread, since this is the thread that created the connection.
			syncDataSet = new DataSet();
			this.syncQuery = query;
			this.timeout = timeout;
			RunOnWorker (new MethodInvoker (DoSyncExecute), true);			// true = synchronous
			if (syncDataSet == null)
				return null;
			else
				return syncDataSet;
		}

		/// <summary> Cancel a running query; inform us when it has done cancelling</summary>
		public virtual void Cancel (MethodInvoker cancelDone)
		{
			if (runState == RunState.Running)
			{
				DoCancel();
				this.cancelDone = cancelDone;
				// Start the thread that will inform us when the cancel has completed.  This could
				// take some time if a rollback is required.
				Thread informCancelDone = new Thread (new ThreadStart (InformCancelDone));
				informCancelDone.Name = "DbClient Inform Cancel";
				informCancelDone.Start();
			}
			else
				cancelDone();
		}

		/// <summary> Cancel a running query synchronously (ie wait for it to cancel)
		/// This method is called when closing an executing query.
		/// </summary>
		public virtual void Cancel()
		{
			if (runState == RunState.Running)
			{
				DoCancel();
				WaitForWorker();
				runState = RunState.Idle;
			}
		}

		/// <summary> Release SQL connection.  This is called automatically from Dispose() </summary>
		public virtual void Disconnect()
		{
			if (runState == RunState.Running) Cancel();
			if (connected)
				RunOnWorker (new MethodInvoker (connection.Close), true);
		}

		/// <summary> Cancels any running queries and disconnects </summary>
		public virtual void Dispose()
		{
			if (connected) Disconnect();
			StopWorker();
		}

		/// <summary> Returns a cloned DbClient object </summary>
		public virtual DbClient Clone()
		{
			return new DbClient (ClientFactory, ConnectString, ConnectDescription);
		}

		#endregion

		#region Private / Protected Code

		/// <summary>
		/// Start the background thread event loop
		/// </summary>
		protected void StartWorker()
		{
			do 
			{
				// Wait for the host thread to wake us up.  We have to use Sleep() rather than
				// Suspend() because Suspend sometimes hogs the CPU on NT4 (bug in beta 2?)
				//Thread.CurrentThread.Suspend();
				try {Thread.Sleep (Timeout.Infinite);} 
				catch (Exception) {}					// the wakeup call, ie Interrupt() will throw an exception
				// If we've been given nothing to do, it's time to exit (the form's being closed)
				if (task == null) break;
				// Otherwise, execute the given task
				task();
				task = null;
			} while (true);
		}

		protected void StopWorker()
		{
			WaitForWorker();
			// End the thread cleanly
			workerThread.Interrupt();			// Interrupt the thread without a task - this will end it.
			workerThread.Join();					// wait for it to end
		}

		protected void RunOnWorker (MethodInvoker method)
		{
			RunOnWorker (method, false);
		}

		protected void RunOnWorker (MethodInvoker method, bool synchronous)
		{
			if (task != null) 								// already doing something?
			{
				Thread.Sleep (100);					// give it 100ms to finish...
				if (task != null) return;				// still not finished - cannot run new task
			}
			WaitForWorker();
			task = method;
			workerThread.Interrupt();
			if (synchronous) WaitForWorker();
		}

		/// <summary>
		/// Wait for worker thread to become available
		/// </summary>
		protected void WaitForWorker() 
		{
			while (workerThread.ThreadState != ThreadState.WaitSleepJoin || task != null)
			{
				Thread.Sleep (20);
			}
		}

		protected void DoConnect()
		{
			if (connected) return;
			try
			{
				connection = ClientFactory.GetConnection (connectString);
				connection.Open();
				connected = true;
			}
			catch (Exception e)
			{
				error = e.Message;
			}
		}

		/// <summary> Executes the query into a DataSet for grid-based results </summary>
		protected virtual void FillDataSet() {
			dataSet.Dispose();
			dataSet = new DataSet();

			// Prevent command timing out because we want it to run forever (until it finishes or the user cancels it)
			dataAdapter.SelectCommand.CommandTimeout = 0;

			// We have to call the queryX delegates on the host's thread (using the Invoke method)
			// because new tabPages and Grids may be created, and this can only
			// done on the same thread which created the parent control (ie the form).

			// Run each subquery separately (subqueries are separated with 'GO' keyword) 
			int tableNum = 0;
			foreach (string s in queries) 
			{
				error = "";
				dataAdapter.SelectCommand.CommandText = s;
				try 
				{
					dataAdapter.Fill (dataSet);
				}
				catch (Exception e) 
				{
					error = e.Message;
					host.Invoke (queryFailed);
					if (runState == RunState.Cancelling) 
						return;
				}
				// Assign unique name to each table, so subsequent queries don't override new tables
				if (error == "") 
				{
					host.Invoke (queryResults);
					while (tableNum < dataSet.Tables.Count)
						dataSet.Tables [tableNum++].TableName = "Query " + tableNum.ToString();
				}
			}
			// Use (asynchronous) BeginInvoke so that we won't block the target method should
			// it want to start another task on the worker thread.
			runState = RunState.Idle;
			host.BeginInvoke (queryDone);
		}

		/// <summary> Executes the query into a StringBuilder object for text-based results </summary>
		protected virtual void FillTextResults() 
		{
			error = "";
			bool first = true;
			try 
			{
				foreach (string s in queries) {
					dataAdapter.SelectCommand.CommandText = s;
					dataReader = dataAdapter.SelectCommand.ExecuteReader();
					buildingResults = true;
					BuildTextResults (!first);
					buildingResults = false;
					dataReader.Close();
					first = false;
				}
			}
			catch (Exception e) 
			{
				error = e.Message;
			}
			finally 
			{
				// A valid and open DataReader may or may not be present. If the query was cancelled before
				// any results were returned, the DataReader object will be null.
				if (!(dataReader == null) && !dataReader.IsClosed)
					try {dataReader.Close();} 
					catch (Exception) {}
				buildingResults = false;
			}
			if (runState == RunState.Cancelling) return;
			runState = RunState.Idle;
			if (error != "") 
				host.BeginInvoke (queryFailed);
			else
				host.BeginInvoke (queryDone);
		}

		protected virtual void BuildTextResults (bool addSeparator)
		{
			textResults = new StringBuilder();
			DateTime lastResults = DateTime.Now;
			do
			{
				// Write column headers, and at the same time determine how much width to allocate each column.
				if (textResults.Length > 0 || addSeparator) textResults.Append ("\r\n\r\n");
				addSeparator = false;
				DataTable schema = dataReader.GetSchemaTable();					// for list of column names & sizes
				if (schema == null) textResults.Append ("The command(s) completed successfully.");
				else
				{
					int[] colWidths = new int [schema.Rows.Count];
					string separator = "";
					for (int col=0; col < schema.Rows.Count; col++) 
					{
						string colName = schema.Rows [col][0].ToString();
						int colSize = (int) schema.Rows [col]["ColumnSize"];
						Type dataType = (Type) schema.Rows [col]["DataType"];
						if (dataType == typeof (DateTime)) colSize = DateTimeFormatStringLength;
						// The column should be big enough also to accommodate the size of the column header
						colWidths [col] = Math.Min (maxTextWidth, Math.Max (colName.Length, colSize));
						if (colName.Length > maxTextWidth) colName = colName.Substring (0, maxTextWidth);
						textResults.Append (colName.PadRight (colWidths [col]) + " ");
						separator = separator + "".PadRight (colWidths [col], '-') + " ";
					}
					textResults.Append ("\r\n" + separator + "\r\n");

					string cell = "";
					if (runState == RunState.Cancelling) return;
					int rowCount = 0;
					while (dataReader.Read()) {														// Read through rows in result set
						rowCount++;
						for (int col = 0; col < dataReader.FieldCount; col++) 
						{
							object data = dataReader.GetValue (col);
							// Use a fixed format for dates, so we can predict its length.
							cell = data is DateTime ?
								((DateTime) data).ToString (DateTimeFormatString)
								: data.ToString();
							if (col == dataReader.FieldCount - 1)							// if on last field, don't truncate or pad
								textResults.Append (cell);
							else
							{
								textResults.Append (cell.PadRight (colWidths [col]), 0, colWidths [col]);
								textResults.Append (' ');
							}
						}
						if (!cell.EndsWith ("\r\n")) textResults.Append ("\r\n");

						// feed results to host every 5 seconds
						if (lastResults.AddSeconds (5) < DateTime.Now) 
						{
							if (runState == RunState.Cancelling) return;
							host.Invoke (queryResults);
							lastResults = DateTime.Now;
							textResults = new StringBuilder();
						}
						if (runState == RunState.Cancelling) return;
					}
					if (rowCount > 0) textResults.Append ("\r\n" + rowCount.ToString() + " row(s)\r\n");
				}
			} while (dataReader.NextResult());						// Get next result set (in case query has returned > 1)
			host.Invoke (queryResults);										// feed outstanding results to host
		}

		protected virtual void DoCancel()
		{
			if (runState == RunState.Running)
			{
				runState = RunState.Cancelling;
				// We have to cancel on a new thread - separate to the main worker thread (because the
				// worker thread will be busy) and separate to the main thread (as this is locked into the
				// main UI apartment, and its use could cause subsequent corruption to an OleDb connection).
				Thread cancelThread = new Thread (new ThreadStart (dataAdapter.SelectCommand.Cancel));
				cancelThread.Name = "DbClient Cancel Thread";
				cancelThread.Start();
				// wait for the command to finish: this won't take long (however the main worker thread that is
				// executing the actual command make take a while to register the cancel request & tidy up)
				cancelThread.Join();	
			}
		}

		protected virtual void InformCancelDone()
		{
			WaitForWorker();
			dataSet.Dispose();									// clear any partial results
			dataSet = new DataSet();
			runState = RunState.Idle;
			host.Invoke (cancelDone);
		}

		protected virtual void DoSyncExecute ()
		{
			// Called indirectly by Execute()
			IDbDataAdapter da = ClientFactory.GetDataAdapter (syncQuery, connection);
			da.SelectCommand.CommandTimeout = timeout;
			try 
			{
				da.Fill (syncDataSet);
			}
			catch (Exception e)
			{
				error = e.Message;
				syncDataSet = null;
			}
		}

		#endregion

	}
}