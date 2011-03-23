using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.InteropServices;
using System.Text;
using SigHelper;
using ChurnReports;
using ComComparer;
using System.Data;
using System.Threading;
using System.Globalization;

//we need this because some namespaces did not exist in each version
//for example, StringCOllection moved from System.Collections to System.Collections.Specialized

using System.Data.SqlClient;
using System.Collections.Specialized;


namespace LibCheck 
{
	public class LibChk 
	{

		// ** Constants
		private const BindingFlags allBindingsLookup = BindingFlags.Public | 
			BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

		// ** Fields	
	
		public static string vers = String.Format("{0:00}.{1:00}.{2}.{3:00}", new Object[]
	{ Environment.Version.Major, Environment.Version.Minor, Environment.Version.Revision, Environment.Version.Build });

		static string en = Environment.NewLine; //included SOLELY to make the usage statement more readable

		static string usage = 
			en + "LibCheck <option>" + 
			en +
			en + "OPTIONS:" + 
			en + "     -store [<assembly> | All | Full] <buildNumber>" +
			en + "     -compare [<oldNumber> | current]  [<newNumber> | current ]" +
			en + "     [-split 4 | 10 | 15]   [-out <path>]" +
			en + "     [-full <path>]         <options>" +
			en + 
			en + "-----------------------------------------------------------------------" +
			en + "     -store    Indicates that a set of store files should be created." +
			en + "               These can later be used to compare to other store files." +
			en + "               For a full comparison of all assemblies in the .NET framework," +
			en + "               specify All. For a full comparison of all dlls in a specific" +
			en + "               directory, specify full. Note that this will only work if" +
			en + "               the -full switch is also specified." +
			en +
			en + "     -compare  Indicates that a comparison should be made between two" +
			en + "               sets of store files (representing different builds)." +
			en + "               The Store and Compare options cannot both be specified." +
			en + "               The inputs for the process are presumed to exist in" +
			en + "               directories with the same names as those specified." +
			en + "               If current is specified, the compare will be done between the " +
			en + "               current assemblies in the version directory. " +
			en +
			en + "     [-split]  Allows you to specify whether files being split should be" + 
			en + "               split into 4, 10, 15, or 20 files. The default is 10." +
			en +
			en + "     [-out]    Allows you to specify the location the reports are" +
			en + "               written to when comparing files. The default is" +
			en + "               <oldNumber>to<newNumber>" +
			en + "               E.g. -out 2505to2508          (Relative Path)" +
			en + "               E.g. -out c:\\reports          (Absolute Path)" +
			en +
			en + "     [-full]   Indicates that the path is a fully specified path to an " +
			en + "               assembly. If this switch is specified, please include " +
			en + "               the fully-specified path after the switch. Otherwise, the " +
			en + "               path is assumed to be in the same location as the SDK. " +
			en +
			en + "     [-owners] Indicates that the owners should be included in the output. " +
			en +
			en + "     [-bydll]  Indicates that the store should be done on a dll by dll basis" +
			en + "               rather than on a namespace by namespace basis. This is the default." +
			en +
			en + "     [-byspace]Indicates that the store should be done on a namespace by" +
			en + "               namespace basis, rather than on a dll by dll basis." +
			en +
			en + "     [-db]     Indicates that the information should be stored in a" +
			en + "               database, rather than serialized to file." +
			en + 
			en + "	   [-gacload]Indicates that the files contained in gacload.txt should be loaded from" + 
			en + "			     the GAC instead of from the version directory." +
			en +
			en + "     [-owners] Indicates that the owners should be included in the output. " +
			en +
			en + "     [-nochurn]Indicates that the estimated churn should not be shown. " +
			en +
			en + "     [-adds]   Indicates that the report should display only those things which. " +
			en + "               can be categorized as an added item, such as a new member, or" +
			en + "               type. " + 
			en + "               NOTE: removing something from an old store will meet this category." +
			en +
			en + "     [-ser]    Indicates the store file should be created with serialization" +
			en + "               information included, to determine serialization compatibility or" +
			en + "               the comparison should be performed with this information included." +
			en +
			en + "     [-soap]   Indicates that the serialized information should be stored" +
			en + "               using the soap format. You cannnot specify soap and the DB" +
			en + "               option together. If you don't specify soap or db," +
			en + "               the information is serialized using the binary formatter." +
			en +
			en + "     [-struct]  Indicates structure layout field data should be created and saved " +
			en +
			en + "     [-structmethod]  Indicates structure layout method data should be created and saved " +
			en + "     				    Note that only one of -struct -structmethod or -ser may be used at one time." +
			en + 
			en + "     [-file]   Indicates output should be stored as a controlled format, or" +
			en + "               retrieved from that format." +
			en +
			en + "     [-supp]   Suppresses output to the console." +
			en +
			en + "     [-header] Include information from the header.txt file in the summary report." +
			en +
			en + "     [-sumclr] Changed entries in the Summary report will appear in color." +
			en +
			en + "     [-noclr]  Changed entries in the Detail reports will not appear in color." +
			en +
			en + "     [-sumcho] Indicates that only namespaces that have changed should be listed" +
			en + "               in the summary page." +
			en +
			en + "     [-alldet] Indicates that the details for changes should all be placed in one," +
			en + "               big details page, rather than being split up." +
			en + "               When you select this option, the details links are placed at the" +
			en + "               top and bottom of the page." +
			en +
			en + "     [-nolink] Indicates no links to details should be on the summary page." +
			en + "               You can only specify this option with the -alldet switch." +
			en +
			en + "     [-htm]    Indicates that the summary and detail files should have the" +
			en + "               extension 'htm', as opposed to the default, 'html'." +
			en +
			en + "     [-com <x>]Generate COM compatibility reports for any assemblies found," +
			en + "               when doing a comparison. If <x> is replaced with a t (default)" +
			en + "               the report is generated: if replaced with an f, it is not." +
			en + "               Replacing <x> with the word 'only' generates the com report only." +
			en + "-----------------------------------------------------------------------" +
			en +
			en + "NOTES: Either -store, or -compare must be specified." +
			en + 
			en + "       If you are storing a file (-store), the build number specified" + 
			en + "       indicates the location the serialization file will be" + 
			en + "       written to. Also note that in order to be successfully stored," +
			en + "       classes MUST be in a namespace. Classes not within a namespace" +
			en + "       will not cause an exception, but will not be captured by the tool." +
			en +
			en + "-----------------------------------------------------------------------";

		static XmlReport xmlReport;
#if DOREPORTS
	public static StreamWriter errorWriter  = null;
	public static StreamWriter reportWriter = null;
	static XmlReport xmlReport;
#endif

		static ChurnReport   summary = null;	// Churn Report
		static UnifiedReport unified = null;	// Unified Reports - one per assembly.

		static bool   _runStore = false;
		static string _assembly = null;
		static string _buildNumber = null;
		static bool   _runCompare = false;
		static CurrentCompare _runCurrentCompare = CurrentCompare.Specific;
		static string _oldBuild = null;
		static string _newBuild = null;

		static string newVersion = "";
		static string oldVersion = "";

		enum CurrentCompare {Specific = 0, Old = 1, New = 2};

		static string _codebase = null;
		static int _dbug = 0;

		// THIS IS THE STORE FOR NAMESPACES!
		static Hashtable htNamespaces = new Hashtable();
		static Hashtable ObsoleteHT = new Hashtable();

		static int numSplits = 0;

		static String outputLoc = "";

		static Hashtable htGACdlls = null;
		static ArrayList alSplitF = null;
		static ArrayList alSplitNamespaces = null;
		static ArrayList alIntfcAdds = null;
		static ArrayList comDlls = null;
		static Hashtable htRanges = null;
		static StreamWriter AddsFile = null;

		static StringCollection splitRanges = new StringCollection();

		static string fileFound = "";
		static String fileDir = "";
		static int splitFound = 0;

		static bool useHTM = false;
		static bool sumAll = true; //DEFAULT
		static bool allDetails = false;
		static bool noLink = false;
		static bool addsOnly = false;
		static bool noColor = false;
		static bool sumColor = false;
		static bool suppress = false;
		static bool showOwners = false;
		static bool byDll = true; //DEFAULT
		static bool fullSpec = false;
		static bool showChurn = true; //DEFAULT
		static bool addSer = false;
		static bool storeDB = false;
		static bool storeSoap = false;
		static bool incHeader = false;
		static bool storeAsFile = true; //DEFAULT
		static bool makeComReport = true;
		static bool comOnly = false;
		static bool GACload = true;                

		//just for verification testing
		//robvi
		static Hashtable ht = new Hashtable();
		static bool addStruct = false;
		static bool addStructMethod=false;
		//	static int totalEnumCount = 0;
		public static StreamWriter obsoletewriter;

		// ** DB Item
		static ReflectorDO rf;

		// ** Properties
		public static string OldVer { get { return _oldBuild; } }
		public static string NewVer { get { return _newBuild; } }

		// ** Methods
		public static void Main (String [] args)
		{

			Console.WriteLine("start time = " + DateTime.Now);
		
	
			obsoletewriter = File.CreateText("obsolete.txt");
			long startTicks = DateTime.Now.Ticks;

			//ALWAYS FORCE THE CULTURE TO en-US
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

			StringBuilder codebase = new StringBuilder(Assembly.Load("mscorlib").CodeBase);
		
			//WEIRD CHANGE REQUIRED, CAME IN AT BUILD 2701
			if (codebase.ToString().IndexOf("///") > -1)
				codebase = codebase.Replace("file:///","");
			else
				codebase = codebase.Replace("file://","");

			codebase = codebase.Replace("/mscorlib.dll","");

			_codebase = codebase.Replace ("/", Path.DirectorySeparatorChar.ToString()).ToString();

			bool goodToGo = false;
			try 
			{
				goodToGo = ParseArgs(args);
			}
			catch (ArgumentException e) 
			{
				if (!suppress) 
				{
					Console.WriteLine(en + e.Message);
					Console.WriteLine(usage);
				}
			}
			catch (Exception e) 
			{
				if (!suppress) 
				{
					Console.WriteLine(en + e.ToString());
					Console.WriteLine(usage);
				}
			}

			if (!goodToGo)
				return;

			//find out WHICH files are to be split up
			//first thing is to check if this is a split file

			alSplitF = OpenFileList("reffiles\\splitfiles.txt");
			alSplitNamespaces = OpenFileList("reffiles\\splitNamespaces.txt");
			htGACdlls = OpenGACList("reffiles\\gacload.txt");
			GetSplitRanges();
			GetIntfcAdds();

			//only make a new connection if we are working with the DB...
			if (storeDB)
				rf = new ReflectorDO();

			if (_runStore) 
			{
				if (Directory.Exists(Directory.GetCurrentDirectory() + 
					Path.DirectorySeparatorChar + _buildNumber) == false) 
				{
					if (!suppress) 
					{					
						Console.WriteLine(Environment.NewLine + "The following required directory " + 
							"does not exist.");

						Console.WriteLine(Directory.GetCurrentDirectory() + 
							Path.DirectorySeparatorChar + _buildNumber);

						Console.WriteLine("It will be created." + Environment.NewLine);

					}
					//CREATE the directory...
					try 
					{
						Directory.CreateDirectory(Directory.GetCurrentDirectory() + 
							Path.DirectorySeparatorChar + _buildNumber);
					}
					catch (Exception e) 
					{
						Console.WriteLine("An exception occurred trying to create the directory:");
						Console.WriteLine(e.ToString());	
						return;
					}
				}

				if (_assembly.ToLower() == "full") 
				{

					DirectoryInfo di = new DirectoryInfo(fileDir);

					foreach (FileInfo f in di.GetFiles ("*.dll")) 
					{
						_assembly = f.Name;
						MakeStoreFiles();
					}
				} 
				else 
				{			
					MakeStoreFiles();
				}
			}

			if (_runCompare) 
			{

				if (makeComReport)
					// make the Com arraylist...
					//this is ALL dlls in the specified comparison location...
					MakeComList();

				if (comOnly == false) 
				{
					MakeReports();

					if (allDetails)
						unified.Close(useHTM);
				}

				if (makeComReport) 
				{
					MakeComCompat();
				}
			}

			if (_runCurrentCompare > CurrentCompare.Specific) 
			{
				MakeCurrentReport();

				if (allDetails)
					unified.Close(useHTM);
			}

			if (!suppress)
				obsoletewriter.Close();
			Console.WriteLine("\r\nLibCheck Done.\r\n");

			long endTicks = DateTime.Now.Ticks;

			TimeSpan duration = new TimeSpan(endTicks - startTicks);
			//Console.WriteLine("Enums: " + totalEnumCount);
			Console.WriteLine("End Time = " + DateTime.Now);
			Console.WriteLine("\r\nTotal Time = {0}\r\n", duration);

			//CLOSE THE RF
			if (rf != null)
				rf.Close();
		}

		// Parse the argument array
		static bool ParseArgs(string[] args) 
		{

			for (int i = 0; i < args.Length; i++) 
			{
				string arg = args[i].ToLower();

				if (arg == "-store") 
				{
					if (_runStore)
						throw new ArgumentException("Only specifiy the -Store option once.");
					if (args.Length <= i+2)
						throw new ArgumentException("Too few arguments for the -Store option.");

					_runStore = true;
					_assembly = args[++i];
					_buildNumber = args[++i];
				}
				else if (arg == "-compare") 
				{
					if (_runCompare)
						throw new ArgumentException("Only specifiy the -Compare option once.");
					if (args.Length <= i+2)
						throw new ArgumentException("Too few arguments for the -Compare option.");
					_oldBuild = args[++i];
					_newBuild = args[++i];

					if (_oldBuild.ToLower() == "current") 
					{
						_runCurrentCompare = CurrentCompare.Old;
						//TEMP: current compare can ONLY be done by dll
						byDll = true;
						_runCompare = false;
					}

					if (_newBuild.ToLower() == "current") 
					{
						_runCurrentCompare = CurrentCompare.New;
						//TEMP: current compare can ONLY be done by dll
						byDll = true;
						_runCompare = false;
					}
					else 
					{
						if (_runCurrentCompare == CurrentCompare.Specific)
							_runCompare = true;
					}
				}
				else if (arg == "/?" || arg == "-?") 
				{
					Console.Write(usage);
					return false;
				}
				else if (arg.StartsWith("-dbug")) 
				{
					_dbug = (arg.IndexOf(':') > 0) ? Convert.ToInt32 (arg.Split(new char [] {':'})[1]) : 1;
				}
				else if (arg.ToLower().StartsWith("-out")) 
				{
					outputLoc = args[i + 1] + "/";
					i++;
				}
				else if (arg.ToLower().StartsWith("-owners")) 
				{
					showOwners = true;
				}
				else if (arg.ToLower().StartsWith("-nochurn")) 
				{
					showChurn = false;
				}
				else if (arg.ToLower().StartsWith("-adds")) 
				{
					addsOnly = true;
				}
				else if (arg.ToLower().StartsWith("-full")) 
				{
					fullSpec = true;
					fileDir = args[i + 1];
					i++;
				}
				else if (arg.ToLower().StartsWith("-gacload"))
				{
					GACload = true;
				}
				else if (arg.ToLower().StartsWith("-ser")) 
				{
					addSer = true;
				}
				else if (arg.ToLower().StartsWith("-structmethod"))
				{
					addStructMethod = true;
				}
				else if (arg.ToLower().StartsWith("-struct")) 
				{
					addStruct = true;
				}
				
				else if (arg.ToLower().StartsWith("-bydll")) 
				{
					byDll = true;
				}
				else if (arg.ToLower().StartsWith("-byspace")) 
				{
					byDll = false;
				}
				else if (arg.ToLower().StartsWith("-db")) 
				{
					storeDB = true;
					storeAsFile = false;
				}
				else if (arg.ToLower().StartsWith("-soap")) 
				{
					storeSoap = true;
					storeAsFile = false;
				}
				else if (arg.ToLower().StartsWith("-file")) 
				{
					storeAsFile = true;
				}
				else if (arg.ToLower().StartsWith("-supp")) 
				{
					suppress = true;
				}
				else if (arg.ToLower().StartsWith("-header")) 
				{
					incHeader = true;
				}
				else if (arg.ToLower().StartsWith("-noclr")) 
				{
					noColor = true;
				}
				else if (arg.ToLower().StartsWith("-sumclr")) 
				{
					sumColor = true;
				}
				else if (arg.ToLower().StartsWith("-sumcho")) 
				{
					sumAll = false;
				}
				else if (arg.ToLower().StartsWith("-alldet")) 
				{
					allDetails = true;
				}
				else if (arg.ToLower().StartsWith("-nolink")) 
				{
					noLink = true;
				}
				else if (arg.ToLower().StartsWith("-htm")) 
				{
					useHTM = true;
				}
				else if (arg.ToLower().StartsWith("-com")) 
				{
					if (_runCompare == false) 
					{
						throw new ArgumentException("Ensure that you specify this is a comparison, before specifying the COM report switch.");

					}

					i+=1;
					if (i >= args.Length) 
					{
						throw new ArgumentException("The -com switch must be followed by 'f', 't', or 'only'");
					}

					if (args[i].ToLower().Equals("f")) 
					{
						makeComReport = false;
					} 
					else if (args[i].ToLower().Equals("t")) 
					{
						makeComReport = true;
					} 
					else if (args[i].ToLower().Equals("only")) 
					{
						makeComReport = true;
						comOnly = true;
					} 
					else 
					{
						throw new ArgumentException("The -com switch must " +
							"be followed by 'f', 't', or 'only'");
					}

				}
				else if (arg.ToLower().StartsWith("-split")) 
				{
					try 
					{
						numSplits = Convert.ToInt32(args[i + 1]);
					} 
					catch (Exception) 
					{
						throw new ArgumentException(
							"The -split option must be followed by either the number 4, 10, or 15.");
					}

					if (numSplits != 4 && numSplits != 10 && numSplits != 15 
						&& numSplits != 20 && numSplits != 30)
						throw new ArgumentException(
							"The -split option must be followed by either the number 4, 10, or 15.");
					i++;
				}
				else
					throw new ArgumentException("Unknown command line parameter '" + arg + "'");
			}
		
			if(args.Length == 0 || !(_runStore || _runCompare || _runCurrentCompare > (CurrentCompare)0))
				throw new ArgumentException("Please use one or more of the available options.");

			if ((storeDB && storeSoap) || (storeAsFile && (storeDB || storeSoap)))
				throw new ArgumentException("You cannot specify to store by DB and Soap Format, " +
					"or by db or soap, and by file.");

			if (noLink && !allDetails)
				throw new ArgumentException("You cannot specify the -nolink switch without also " 					+ "specifying the -alldetails switch.");

			if (_runStore && _assembly.ToLower() == "full" && fullSpec == false)
				throw new ArgumentException("You cannot specify the store to be full, without also " 					+ "specifying the -full switch.");

			if (numSplits == 0)
				numSplits = 10;

			return true;
		} // end ParseArgs()


		// Create store file: LibCheck -Store (<assembly> | All]) <buildNumber>
		static void MakeStoreFiles() 
		{
			ArrayList ignoreFiles = OpenFileList("reffiles\\ignorefiles.txt");
		
			DateTime startTime = DateTime.Now;

			bool everything = (_assembly.ToLower() == "all");

#if DOREPORTS
		  string reportFile = "";
		  reportFile = outputLoc + ((everything) ? "DllList" : _assembly) + "." + vers + ".Report";
		  reportWriter = new StreamWriter(reportFile, true);
		  reportWriter.WriteLine ("\r\nReport for build " + vers);
		  reportWriter.WriteLine ("--------------------------------------------------");
		  reportWriter.WriteLine ("   on {0:00}/{1:00}/{2}, {3,-2}:{4:00}:{5:00}\r\n", 
				new object [] { startTime.Month, startTime.Day, startTime.Year, 
				startTime.Hour, startTime.Minute, startTime.Second});
		  reportWriter.Flush();

		  string errorFile = _buildNumber + Path.DirectorySeparatorChar + ((everything) ? 
			(storeSoap ? "soap" : "binary") : _assembly) + ".Store.Err";

		  errorWriter = new StreamWriter(errorFile, false);

		  errorWriter.WriteLine ("{0}: created for build {1}", errorFile, vers);
		  errorWriter.WriteLine ("   on {0:00}/{1:00}/{2}, {3,-2}:{4:00}:{5:00}", new object [] {
				startTime.Month, startTime.Day, startTime.Year, startTime.Hour, 				startTime.Minute, startTime.Second});

		  if (_dbug != 0) errorWriter.WriteLine("_dbug = " + _dbug);
#endif

			string dllFullName = null;
			string storeFile = null;

			StringCollection splitStoreFiles = new StringCollection();

			if (everything) 
			{		// process all dll's in the ComPlus directory.
				ArrayList files = new ArrayList();

				foreach (string f in Directory.GetFiles (_codebase, "*.dll")) 
				{
					FileInfo fi = new FileInfo(f);
					String file = fi.Name.ToLower();

					if (!(ignoreFiles.Contains(file)))
						files.Add(file);


				}

				files.Sort(Comparer.Default);

				foreach (string dllName in files)
				{
					if (!suppress)
						Console.WriteLine();

					bool fileIsSplit = false;

					foreach(String s in alSplitF) 
					{
						if (dllName.ToLower().IndexOf(s.ToLower()) >= 0) 
						{
							fileIsSplit = true;
							break;
						}
					}

					dllFullName = _codebase + Path.DirectorySeparatorChar + dllName;

					bool goodToGo = GoodDllName(dllFullName);
#if DOREPORTS
				 errorWriter.WriteLine("\r\nGoodDllName({0}) = {1}", dllFullName, goodToGo);
#endif
					if (GoodDllName(dllFullName)) 
					{

						//new, for comcompat...
						//copy the file to the output dir...
						File.Copy(dllFullName, _buildNumber + Path.DirectorySeparatorChar + dllName, true);

						// here is where we retrieve all the files to be 
						// split from the input file...
						if ( fileIsSplit != true ) 
						{
							storeFile = _buildNumber + Path.DirectorySeparatorChar + 
								Path.GetFileName(dllName) + 
								( storeSoap ? ".soap" : ".binary" ) + ".store";
						} 
						else 
						{

							splitStoreFiles.Clear();

							for (int i=1;i<=numSplits;i++) 
							{
								splitStoreFiles.Add(_buildNumber + 
									Path.DirectorySeparatorChar + 
									Path.GetFileName(dllName) + "." +
									String.Format("{0:00}", i) + 
									( storeSoap ? ".soap" : ".binary" ) + ".store");
							}
						}

						if (!suppress)
							Console.WriteLine ("Creating Store {0} from file {1}...", storeFile, dllName);
#if DOREPORTS
					    errorWriter.WriteLine ("Creating Store {0}...", storeFile);
#endif

						try 
						{
							if ( fileIsSplit != true ) 
							{	
								CreateStore(dllFullName, storeFile, 0);
							}
							else 
							{
								for (int i=1;i<=numSplits;i++) 
								{
									htNamespaces = new Hashtable();
									CreateStore(dllFullName, splitStoreFiles[i-1], i);
								}
							}

						}
						catch(Exception e) 
						{
							Console.WriteLine(e.ToString());

#if DOREPORTS
						  errorWriter.WriteLine("\r\nException in: " + dllFullName);
						  errorWriter.WriteLine(e.ToString());
						  errorWriter.Flush();
#endif
							continue;
						}
					}
#if DOREPORTS
				    errorWriter.Flush();
#endif

					//NEW ADDITION
					htNamespaces = new Hashtable();
				}
			}
			else 
			{		// Process just this one assembly
				if (!suppress)
					Console.WriteLine();
				if (GoodAssemblyName(_assembly)) 
				{
					Module [] ma = new Module [0];
					try 
					{
						if (fullSpec) 
						{
							if(GACload  && htGACdlls.ContainsKey(_assembly.ToLower()))
							{
								ma=Assembly.Load(htGACdlls[_assembly.ToLower()].ToString()).GetModules();
							}
							else
							{
								ma = Assembly.LoadFrom(fileDir + "\\" + _assembly).GetModules();
							}
							dllFullName = fileDir + "\\" + _assembly;
						}
						else 
						{
							if(GACload && htGACdlls.ContainsKey(_assembly.ToLower()))
							{
								ma = Assembly.Load(htGACdlls[_assembly.ToLower()].ToString()).GetModules();
							}
							else
							{
								ma = Assembly.Load(_assembly).GetModules();
							}
							dllFullName = "";
						}

						foreach (Module m in ma) 
						{
							string dllName = m.Name.ToLower();

							if (ignoreFiles.Contains(dllName))
								continue;
							if (!suppress)
								Console.WriteLine();

							bool fileIsSplit = false;

							foreach(String s in alSplitF) 
							{
								if (dllName.ToLower().IndexOf(s.ToLower()) >= 0) 
								{
									fileIsSplit = true;
									break;
								}
							}


							if (dllFullName == "") 
							{

								dllFullName = _codebase + Path.DirectorySeparatorChar + dllName;
							}

							//  THIS IS WHERE WE CHECK TO SEE FOR SPLIT
							if ( fileIsSplit != true ) 
							{

								storeFile = _buildNumber +
									Path.DirectorySeparatorChar + 
									Path.GetFileName(dllName) + 
									(storeSoap ? ".soap":".binary") + ".store";
							}
							else 
							{

								splitStoreFiles.Clear();

								for (int i=1;i<=numSplits;i++) 
								{
									splitStoreFiles.Add(_buildNumber + 
										Path.DirectorySeparatorChar + 
										Path.GetFileName(dllName) + 
										"." + String.Format("{0:00}", i) + 								(storeSoap ? ".soap":".binary") + ".store");
								}
							}

							if (!suppress)
								Console.WriteLine ("Creating Store {0} from file {1}...",
									storeFile, dllName);

							try 
							{
								if ( fileIsSplit != true )
									CreateStore(dllFullName, storeFile, 0);
								else 
								{

									for (int i=1;i<=numSplits;i++) 
									{
										htNamespaces = new Hashtable();

										CreateStore(dllFullName, splitStoreFiles[i-1], i);
									}
								}
							}
							catch(Exception e) 
							{

								Console.WriteLine(e.ToString());
#if DOREPORTS
							  errorWriter.WriteLine("\r\nException in: " + dllFullName);
							  errorWriter.WriteLine(e.ToString());
#endif
								continue;
							}
						}
					}
					catch (Exception eo) 
					{
						Console.WriteLine (eo.ToString());
					}
				}
			}

			TimeSpan delta = DateTime.Now - startTime;
#if DOREPORTS
		      errorWriter.WriteLine(String.Format("Store files created, elapsed time: {0:00}:{1:00}:{2:00}.",
			delta.Hours, delta.Minutes, delta.Seconds));
		    errorWriter.Flush();
		    errorWriter.Close();

		    reportWriter.Flush();
		    reportWriter.Close();
#endif
		} //end makestorefiles

		// Create store file: LibCheck -Store (<assembly> | All]) <buildNumber>
		static void MakeCurrentStoreFiles(int num) 
		{

			ArrayList ignoreFiles = OpenFileList("reffiles\\ignorefiles.txt");
			DateTime startTime = DateTime.Now;

			//		bool everything = (_assembly.ToLower() == "all");

#if DOREPORTS
		  string reportFile = "";

//		  reportFile = outputLoc + ((everything) ? "DllList" : _assembly) + "." + vers + ".Report";
		  reportFile = outputLoc + _assembly + "." + vers + ".Report";

		  reportWriter = new StreamWriter(reportFile, true);
		  reportWriter.WriteLine ("\r\nReport for build " + vers);
		  reportWriter.WriteLine ("--------------------------------------------------");
		  reportWriter.WriteLine ("   on {0:00}/{1:00}/{2}, {3,-2}:{4:00}:{5:00}\r\n", 
				new object [] { startTime.Month, startTime.Day, startTime.Year,
				startTime.Hour, startTime.Minute, startTime.Second});
		  reportWriter.Flush();

		  string errorFile = _buildNumber + Path.DirectorySeparatorChar + _assembly + ".Store.Err";

		  if ( num < 0)
			errorWriter = new StreamWriter(errorFile, false);

		  errorWriter.WriteLine ("{0}: created for build {1}", errorFile, vers);
		  errorWriter.WriteLine ("   on {0:00}/{1:00}/{2}, {3,-2}:{4:00}:{5:00}", new object [] {
					startTime.Month, startTime.Day, startTime.Year, startTime.Hour, startTime.Minute, startTime.Second});
		  if (_dbug != 0) errorWriter.WriteLine("_dbug = " + _dbug);
#endif

			string dllFullName = null;
			string storeFile = null;
			//Console.WriteLine("p1");
			//Console.ReadLine();
			if (!suppress)
				Console.WriteLine();

			if (GoodAssemblyName(_assembly)) 
			{
				Module [] ma = new Module [0];
				try 
				{
					if (fullSpec) 
					{
						ma = Assembly.LoadFrom(fileDir + "\\" + 
							_assembly).GetModules();
						dllFullName = fileDir + "\\" + _assembly;
					} 
					else 
					{
						ma = Assembly.Load(_assembly).GetModules();
						dllFullName = "";
					}

					foreach (Module m in ma) 
					{
						string dllName = m.Name.ToLower();

						if (ignoreFiles.Contains(dllName))
							continue;

						if (!suppress)
							Console.WriteLine();

						if (dllFullName == "") 
						{
							dllFullName = _codebase + 
								Path.DirectorySeparatorChar + 
								dllName;
						}

						if (num < 0) 
							storeFile = _buildNumber + 
								Path.DirectorySeparatorChar + 
								Path.GetFileName(dllName) + 
								(storeSoap ? ".soap":".binary") +
								".store";
						else	
							storeFile = _buildNumber + 
								Path.DirectorySeparatorChar + 
								Path.GetFileName(dllName) + 
								"." + String.Format("{0:00}", num) + 								".Binary.Store";

						if (!suppress)
							Console.WriteLine ("using {0}...", dllName);

						try 
						{
							if (num < 0)
								CreateStore(dllFullName, storeFile, 0);
							else
								CreateStore(dllFullName, storeFile, num);
						}
						catch(Exception e) 
						{

							Console.WriteLine(e.ToString());

#if DOREPORTS
							  errorWriter.WriteLine("\r\nException in: " + dllFullName);
							  errorWriter.WriteLine(e.ToString());
#endif
							continue;
						}
					}
				}
				catch (Exception eo) 
				{
					Console.WriteLine (eo.ToString());
				}
			}

#if DOREPORTS
		    errorWriter.WriteLine(String.Format("Store files created"));
		    errorWriter.Flush();

		    reportWriter.Flush();
		    reportWriter.Close();
#endif
		
		} //end makecurrentstorefiles

		// Test to see if reflection is going to work against the file name
		static bool GoodDllName(string dllName) 
		{
			try 
			{
				Assembly a = null;
				string assemblyname=dllName.Substring(dllName.LastIndexOf("\\")+1);
				if(GACload && htGACdlls.ContainsKey(assemblyname.ToLower()))
				{
					a = Assembly.Load(htGACdlls[assemblyname.ToLower()].ToString());
				}
				else
				{
					a = Assembly.LoadFrom(dllName);	
				}
				if (a == null) 
				{
					if (!suppress) 
					{
						Console.WriteLine ("\r\nAssembly.LoadFrom('{0}') returned null", dllName);
					}
					return false;
				}
				a.GetTypes();
			}
			catch (BadImageFormatException) 
			{
				Console.WriteLine ("\r\n{0} is not a managed assembly", dllName);

#if DOREPORTS
			    errorWriter.WriteLine ("GoodDllName: BadImageFormatException - (not a managed assembly)");
#endif

				return false;
			}
			catch (ReflectionTypeLoadException) 
			{
				Console.WriteLine ("\r\nCould not retrieve some types from {0}", dllName);
#if DOREPORTS
			    errorWriter.WriteLine("GoodDllName: ReflectionTypeLoadException - (some types 'broken')");
#endif
				return true;
			}
			catch (Exception e) 
			{
				Console.WriteLine ("\r\nException encountered reading {0}", dllName);
				Console.WriteLine ("\r\n{0}", e.ToString());
#if DOREPORTS
			    errorWriter.WriteLine("GoodDllName: unexpected exception!");
			    errorWriter.WriteLine(e.ToString());
#endif
				return false;
			}
			return true;
		} 

		// Test to see if reflection is going to work against the assembly name
		static bool GoodAssemblyName(string assembly) 
		{

			try 
			{
				Assembly a = null;
				if (fullSpec) 
				{
					if(GACload && htGACdlls.ContainsKey(assembly.ToLower()))
					{
						a = Assembly.Load(htGACdlls[assembly.ToLower()].ToString());
					}
					else
					{
						a = Assembly.LoadFrom(fileDir + "\\" + assembly);
					}
				}
				else
				{
					if(GACload && htGACdlls.ContainsKey(assembly.ToLower()))
					{
						a = Assembly.Load(htGACdlls[assembly.ToLower()].ToString());
					}
					else
					{
						a = Assembly.Load(assembly);
					}
				}
				if (a == null) 
				{
					if (!suppress) 
					{
						Console.WriteLine ("\r\nAssembly.Load('{0}') returned null", assembly);
					}
					return false;
				}

				a.GetTypes();
			}
			catch (BadImageFormatException) 
			{
				Console.WriteLine ("\r\n{0} is not a managed assembly", assembly);
#if DOREPORTS
			  errorWriter.WriteLine ("GoodAssemblyName: BadImageFormatException - (not a managed assembly)");
#endif
				return false;
			}
			catch (ReflectionTypeLoadException e) 
			{
				Console.WriteLine ("\r\nCould not retrieve some types from {0}", assembly);
				foreach(Exception er in e.LoaderExceptions)
				{
					Console.WriteLine(er.ToString() + "\n");
				}
#if DOREPORTS
			  errorWriter.WriteLine("GoodAssemblyName: ReflectionTypeLoadException - (some types 'broken')");
#endif
				return true;
			}
			catch (Exception e) 
			{   
				if (e.ToString().IndexOf("was not found") >= 0) 
				{
					//assume that this is OK...
					return false;
				}

				Console.WriteLine ("\r\nException encountered reading {0}", assembly);
				Console.WriteLine ("\r\n{0}", e.ToString());
#if DOREPORTS
			    errorWriter.WriteLine("GoodAssemblyName: unexpected exception!");
			    errorWriter.WriteLine(e.ToString());
#endif
				return false;
			}
			return true;
		}


		public static void CreateStore(string inFile, string storeName, int winformsFileNum) 
		{

			if (inFile.IndexOf ("custommarshalers.dll") != -1) return ;
		
			Assembly a = null;

			//robvi
			//12/11/2003 Breaking Change: mscorlib.dll, System.XML and possibly other future files
			//can only be loaded from the GAC in Whidbey.  The list of files that must obtained from
			//the GAC is under reffiles\gacload.txt.
			String assemblyname=inFile.Substring(inFile.LastIndexOf("\\")+1);
			if(GACload && htGACdlls.ContainsKey(assemblyname.ToLower()))
			{
				a = Assembly.Load(htGACdlls[assemblyname.ToLower()].ToString());
			}
			else
			{
				a = Assembly.LoadFrom(inFile);
			}
		

			Hashtable typememberList = new Hashtable(64);	// hashtable of all members found, by full type name

			// gather members from list of modules
			if (!suppress) 
			{
				Console.Write(" Assembly {0,-44} - ", inFile);
#if DOREPORTS
			    errorWriter.Write(" Assembly {0,-44} - ", inFile);
#endif
			}
			
			int members = 0;
			int errorCount = 0;

			try 
			{
				if (byDll)
					CreateMemberListByDll(ref typememberList, a, out errorCount, 
						out members, winformsFileNum);
				else
					CreateMemberList(ref typememberList, a, out errorCount, 
						out members, winformsFileNum);
			}
#if DOREPORTS
		catch(Exception e) {
			  Console.WriteLine("could not load members.");
			  Console.WriteLine(e.ToString());
			  errorWriter.Write("could not load members of assembly ");
			  try { errorWriter.WriteLine(a.GetName().Name); }
			  catch { errorWriter.WriteLine("loaded from file " + inFile); }
			  errorWriter.WriteLine(e.ToString());
#else
			catch(Exception e) 
			{
				Console.WriteLine("could not load members.");
				Console.WriteLine(e.ToString());
#endif
				return;
			}

			if (errorCount==0) 
			{
				if (!suppress) 
				{
					Console.WriteLine("{0,5} members", members);
#if DOREPORTS
				    errorWriter.WriteLine("{0,5} members", members);
				    reportWriter.WriteLine("{0,-70}- {1,4} types retrieved", inFile, typememberList.Count);
#endif
				}
			}
			else 
			{
				if (!suppress) 
				{
					Console.WriteLine("{0,5} members, {1,2} exceptions encountered", members, errorCount);
#if DOREPORTS
			      errorWriter.WriteLine("{0,5} members, {1,2} exceptions encountered", members, errorCount);
			      reportWriter.WriteLine("{0,-70}- {1,4} types retrieved, {2,3} exceptions caught", inFile, typememberList.Count, errorCount);
#endif
				}
			}

			//write out the split files! COMPLETELY TEMPORARY, COMMENT OUT IF STILL HERE
			//ok, we want one file for each namespace, and the file contains the names of the dlls that have
			//files stored in it!
			//LEAVE THIS CODE IN HERE THOUGH
			/*
			if (true) {
				foreach (string sName in htNamespaces.Keys) {
					foreach (string sKey in ((Hashtable)htNamespaces[sName.ToLower()]).Keys) {
						string []names = sKey.Split(new char[] {' '});

						FileStream fs = new FileStream("splits" + "\\" + 
								names[0] + ".split.txt", FileMode.OpenOrCreate);
						fs.Position = 0;

						StreamReader sr = new StreamReader(fs);
						bool finish = false;

						while (sr.Peek() != -1) {
							string temp = sr.ReadLine();

							if (Path.GetFileName(inFile) == temp) 
								finish = true;
						}

						if (finish == false) {
							StreamWriter sw = new StreamWriter(fs);
							sw.WriteLine(Path.GetFileName(inFile));
							sw.Close();
						}
						sr.Close();
						fs.Close();
					}
				}
			}
			*/

			//NEW STUFF...
			//no need to write out the store if I was simply populating it...
			if (_runCurrentCompare > CurrentCompare.Specific)
				return;

			// write the hashtable of members out to the store file
			foreach (string sName in htNamespaces.Keys) 
			{

				//ok, if the attached hashtable is EMPTY, then don't persist it...
				if (((Hashtable)htNamespaces[sName.ToLower()]).Count <= 0) 
				{
					continue;
				}

				Hashtable htTemp = new Hashtable();
				string sFile = _buildNumber + "\\" + 
					sName + (storeSoap ? ".soap" : ".binary") + ".store";

				/*
							if (storeDB) {
								htTemp = LoadList(ref sFile, "");
							}
							else if (File.Exists(sFile)) {
								htTemp = LoadList(ref sFile, "");
							}
				*/
				//			if (storeDB == false && File.Exists(sFile)) {
				//				htTemp = LoadList(ref sFile, "", false); //LAST FALSE IS ARBITRARY, P1
				//			}

				if (storeDB == false && File.Exists(sFile)) 
				{
					//delete the file!
					try 
					{
						File.Delete(sFile);
					} 
					catch (Exception) {}
					//				htTemp = LoadList(ref sFile, "", false); //LAST FALSE IS ARBITRARY, P1
				}

				foreach (string sKey in ((Hashtable)htNamespaces[sName.ToLower()]).Keys) 
				{

					try 
					{
						htTemp.Add(sKey, ((Hashtable)htNamespaces[sName.ToLower()])[sKey]);
					} 
					catch (Exception) 
					{
						//DO NOTHING
					}

				}

				if (storeDB) 
				{
					rf.InsertKitType(htTemp,sFile,sName,_buildNumber);
				}
				else if (storeAsFile) 
				{
					StoreToFile(htTemp,sFile,sName,_buildNumber);
				}
				else 
				{
					FileStream s = null;

					try 
					{
						s = new FileStream(sFile, FileMode.Create);

						Console.Write("Serializing namespace '{0}'" +
							" with the {1} formatter ... ", sName, 
							(storeSoap ? "soap" : "binary"));

						if (storeSoap) 
						{
							SoapFormatter sFormatter = new SoapFormatter();
							sFormatter.Serialize(s, htTemp);
						}
						else 
						{
							BinaryFormatter bFormatter = new BinaryFormatter();
							bFormatter.Serialize(new BufferedStream (s), htTemp);
						}
					}
#if DOREPORTS
				catch(Exception e) {
					  Console.WriteLine(" *** serialization of memberList failed ***");
					  errorWriter.WriteLine("serialization to " + sName + " failed.");
					  errorWriter.WriteLine(e.ToString());
#else
					catch(Exception) 
					{
						Console.WriteLine(" *** serialization of memberList failed ***");
#endif
					}

					s.Close();
				}
			}

			if (!suppress)
				Console.WriteLine("Complete.\r\n");
		}


		// Create list of members for the store. Weed out non-public information.
		public static void CreateMemberList(ref Hashtable typememberList, Assembly a, 
			out int errorCount, out int memberCount, int winformsFile) 
		{

			Hashtable problems = new Hashtable(1);	// List of problem members by full type name
			problems.Add("System.Runtime.Remoting.Channels.SMTP.ISMTPMessage", new ArrayList());
			((ArrayList)problems["System.Runtime.Remoting.Channels.SMTP.ISMTPMessage"]).AddRange(new string [] {"EnvelopeFields", "Fields", });

			errorCount = 0;
			memberCount = 0;
			Type [] ta = null;

			string assemblyname = null;
			try { assemblyname = a.GetName().Name; }
			catch { assemblyname = "UnknownAssemblyName"; }

			try { ta = a.GetTypes(); }	// Retrieve all types within this assembly
			catch(ReflectionTypeLoadException e) 
			{
				Exception [] ea = e.LoaderExceptions;

#if DOREPORTS
			    errorWriter.Write("Exceptions in Assembly.GetTypes() for " + assemblyname);

			    for (int i = 0; i < ea.Length; i++)
				errorWriter.WriteLine(String.Format(" - {0:00}: {1}", i, ea[i].ToString()));
#endif
				ta = e.Types;
			}
			catch(Exception e) 
			{
				string message = "Assembly.GetTypes() failed on " + assemblyname;
				throw new ApplicationException(message, e);
			}

			splitRanges = GetCorrectSplit(a);

			foreach (Type t in ta) 
			{	// types loop

				
				bool isEnum = t.IsEnum;
				bool namespaceExists = false;
				bool spaceIsSplit = false;
				int outputNumber = 0;
				string nameComp = t.Namespace == null ? "generic" : t.Namespace.ToLower();

				//figure out if we need to change the name, in order to be split
				if (byDll) 
				{
					foreach (string s in alSplitF) 
					{
						if (nameComp == s.ToLower()) 
						{
							spaceIsSplit = true;
							break;
						}
					}
				}
				else 
				{
					foreach (string s in alSplitNamespaces) 
					{
						if (nameComp == s.ToLower()) 
						{
							spaceIsSplit = true;
							break;
						}
					}
				}

				//if this is a split file, figure out which split file this type belongs to...
				if (spaceIsSplit) 
				{

					for (int i=0;i< splitRanges.Count;i++) 
					{//(string tempString in splitRanges){

						string tempString = splitRanges[i];
						string startPoint = "";
						string endPoint = "";

						if (tempString.IndexOf(",") > 0) 
						{
							startPoint = (tempString.Substring(0, tempString.IndexOf(",")).Trim());

							endPoint = (tempString.Substring(tempString.IndexOf(",") + 1).Trim());
						} 
						else 
						{
							startPoint = tempString;
						}

						int compareStart = t.Name.Length < startPoint.Length ? 
							t.Name.Length : startPoint.Length;
						int compareEnd = t.Name.Length < endPoint.Length ? 
							t.Name.Length : endPoint.Length;

						//these are the different comparisons for the different ranges...
						if (String.Compare(t.Name.ToLower().Substring(0,
							compareStart), startPoint) >= 0 && 
							String.Compare(t.Name.ToLower().Substring(0,
							compareEnd), endPoint) <= 0 && 
							(i != 0 && i != (numSplits - 1))) 
						{
							outputNumber = i;
							break;
						} 
						else if (String.Compare(t.Name.ToLower().Substring(0, 
							compareStart), startPoint) <= 0 && i == 0) 
						{
							outputNumber = i;
							break;
						} 
						else if (String.Compare(t.Name.ToLower().Substring(0,
							compareStart), startPoint) >= 0 && 
							i == (numSplits - 1)) 
						{
							outputNumber = i;
							break;			
						}
					}

					nameComp = nameComp + "." + String.Format("{0:00}",outputNumber + 1);
				}

				foreach (string s in htNamespaces.Keys) 
				{
					if (s.ToLower() == nameComp) 
					{
						namespaceExists = true;
						break;
					}
				}

				if (namespaceExists == false)
					htNamespaces.Add(nameComp, new Hashtable());

				// P1: works well, have a good set of namespaces at this point...
				//now, have to make a separate hashtable for EACH ONE, in the process below.
				//this is where we need to modify our arraylist to, instead, a hashtable, storing a namespacename, and the hashtable value!
				//for each NEW entry, add the new element to our hashtable, and ensure we instantiate the hashtable entry for it. The key //is the namespace of course!

				// this if statement, and the switch inside it, ensures that only 
				// types in certain alphabetical
				//ranges are included in a windforms output file...
				//this is done because the file is otherwise, WAY too huge!
				if (winformsFile > 0) 
				{

					//retrieve the entry from the splitRanges collection
					//which corresponds to the winformsFIle specified
					string tempString = splitRanges[winformsFile - 1];

					string startPoint = "";
					string endPoint = "";

					if (tempString.IndexOf(",") > 0) 
					{
						startPoint = (tempString.Substring(0, tempString.IndexOf(",")).Trim());

						endPoint = (tempString.Substring(tempString.IndexOf(",") + 1).Trim());
					} 
					else 
					{
						startPoint = tempString;
					}

					int compareStart = t.Name.Length < startPoint.Length ? 
						t.Name.Length : startPoint.Length;
					int compareEnd = t.Name.Length < endPoint.Length ? 
						t.Name.Length : endPoint.Length;

					//these are the different comparisons for the different ranges...
					if (winformsFile == 1) 
					{
						if ( String.Compare(t.Name.ToLower().Substring(0, 
							compareStart), startPoint) > 0 )
							continue;
					} 
					else if (winformsFile == numSplits) 
					{
						if ( String.Compare(t.Name.ToLower().Substring(0,
							compareStart), startPoint) < 0 ) 
							continue;
					} 
					else 
					{
						if ( String.Compare(t.Name.ToLower().Substring(0,
							compareStart), startPoint) < 0 || 
							String.Compare(t.Name.ToLower().Substring(0,
							compareEnd), endPoint)>0)
							continue;
					}

				}

				if (t == null)
				{
#if DOREPORTS
				    errorWriter.WriteLine("Null type encountered in " + assemblyname);
#endif
					errorCount++;
					continue;
				}

				try { if (!t.IsPublic) continue; }
#if DOREPORTS
			catch(Exception e) {
				    errorWriter.Write("Type.IsPublic failed on type ");
				    errorWriter.WriteLine(t.FullName);
				    errorWriter.WriteLine(e.ToString());
#else
				catch(Exception) 
				{
#endif
					errorCount++;
					continue;
				}

				MemberInfo [] ma = null;

				// Retrieve all members of each "class"
				try { ma = t.GetMembers (allBindingsLookup|System.Reflection.BindingFlags.FlattenHierarchy); }	
#if DOREPORTS
			catch(Exception e) {
				    errorWriter.Write("Type.GetMembers() failed on type ");
				    errorWriter.WriteLine(t.FullName);
				    errorWriter.WriteLine(e.ToString());
#else
				catch(Exception) 
				{
#endif
					errorCount++;
					continue;
				}
#if DOREPORTS
			    if (_dbug > 0) errorWriter.WriteLine("  - Type: {0,-48} - {1:000} members", t.FullName, ma.Length);
#endif

				ArrayList memberList = new ArrayList(ma.Length);
				foreach (MemberInfo mi in ma) 
				{	// members loop
					if (problems.ContainsKey(t.FullName) && ((ArrayList)problems[t.FullName]).Contains(mi.Name)) 
					{
#if DOREPORTS
					    if (_dbug > 0) errorWriter.WriteLine("    -- Skipping member " + mi.Name);
#endif
						continue;
					}
#if DOREPORTS
				    if (_dbug > 0) errorWriter.WriteLine("    -- Member: {0}", mi.Name);
#endif

					try 
					{
						// Ignore non-public, non-family methods and constructors. Also ignore property accessors, event methods and cctor's.
						if (mi is MethodBase)
						{
							if ( !((MethodBase)mi).IsPublic && !((MethodBase)mi).IsFamily )
								continue;
							if ( ((MethodBase)mi).IsSpecialName && (
								mi.Name.StartsWith("get_")
								|| mi.Name.StartsWith("set_")
								|| mi.Name.StartsWith("add_")
								|| mi.Name.StartsWith("remove_")
								|| mi.Name.StartsWith("op_")	// TODO: Add operators back in once GenMemberInfo can compare operators.
								))
								continue;
							if ((mi is ConstructorInfo) && (mi.Name == ".cctor"))
								continue;
						}

						// Ignore non-public, non-family fields
						if (mi is FieldInfo && !( ((FieldInfo) mi).IsPublic || ((FieldInfo) mi).IsFamily ) )
							continue;

						// Ignore non-public, non-family events (add/remove method is non-public, non-family)
						if (mi is EventInfo && !( ((EventInfo) mi).GetAddMethod(true).IsPublic || ((EventInfo) mi).GetAddMethod(true).IsFamily ) )
							continue;

						// Ignore non-public, non-family properties (both getter and setter are non-public, non-family)
						if (mi is PropertyInfo && !(
							( ((PropertyInfo)mi).GetGetMethod(true) != null &&
							( ((PropertyInfo)mi).GetGetMethod(true).IsPublic || ((PropertyInfo)mi).GetGetMethod(true).IsFamily )) ||
							( ((PropertyInfo)mi).GetSetMethod(true) != null &&
							( ((PropertyInfo)mi).GetSetMethod(true).IsPublic || ((PropertyInfo)mi).GetSetMethod(true).IsFamily ))
							))
							continue;

						// Ignore non-public, non-family nested types
						if (mi is Type && !( ((Type) mi).IsNestedPublic || ((Type) mi).IsNestedFamily ) )
							continue;

						// For inherited members, only report ones that are not otherwise hidden
						if (mi.DeclaringType != mi.ReflectedType) 
						{

							MemberInfo[] others = t.GetMember(mi.Name, mi.MemberType, 
								allBindingsLookup);

							if (others.Length == 0) 
							{
								Console.WriteLine("Reflection error on {0} {1}.", t.FullName, mi.Name);
#if DOREPORTS
							    errorWriter.WriteLine("Type.GetMember({0}, {1}, BindingFlags.LookupAll) returned an empty list for {2}",
								mi.Name, mi.MemberType, t.FullName);
#endif
							}
							else if (others.Length == 1 && others[0] != mi) 
							{
								Console.WriteLine("Reflection error on {0} {1}.", t.FullName, mi.Name);
#if DOREPORTS
							errorWriter.WriteLine("Type.GetMember({0}, {1}, BindingFlags.LookupAll) returned a single, non-matching member for {2}",
								mi.Name, mi.MemberType, t.FullName);
#endif
							}
							else 
							{
								bool good = true;
								foreach (MemberInfo other in others) 
								{		// Filter loop
									if (mi.DeclaringType.IsAssignableFrom(other.DeclaringType) && other != mi) 
									{
										switch (mi.MemberType) 
										{
											case MemberTypes.Constructor :
											case MemberTypes.Method :
												if (GenParameterInfo.PSig((MethodBase)mi) == GenParameterInfo.PSig((MethodBase)other))
													good = false;
												break;
											case MemberTypes.Property :
												if (GenParameterInfo.PSig(((PropertyInfo)mi).GetIndexParameters()) == GenParameterInfo.PSig(((PropertyInfo)mi).GetIndexParameters()))
													good = false;
												break;
											case MemberTypes.Event :
											case MemberTypes.Field :
											case MemberTypes.NestedType :
												good = false;
												break;
											default :
#if DOREPORTS
										 errorWriter.WriteLine("Error MemberInfo.MemberType = '{0}' for {1}.{2}",
													((Enum)mi.MemberType).ToString(), t.FullName, mi.Name);
#endif

												break;
										}
										if (!good) break;					// if member unwanted, exit Filter loop
									}
								}

								// ignore "hidden" members, skip to next member info.
								if (!good) continue;	
							}
						}

						// ignore the value__ field on Enum's
						if (mi.Name == "value__" && (t.Equals(Type.GetType("System.Enum")) || 
							t.IsSubclassOf(Type.GetType("System.Enum"))))
							continue;

						// convert memberinfos to typemembers
						TypeMember tm = null;
						try 
						{
#if DOREPORTS
						    if (mi == null) errorWriter.WriteLine("mi == null");
#endif

							// Generate new TypeMember
							tm = new TypeMember(mi, t, addSer, isEnum, addStruct, addStructMethod, obsoletewriter);
						
				
						
						

						}
#if DOREPORTS
					catch(Exception e) {
						  errorWriter.Write("TypeMember construction failed on member ");
						  errorWriter.Write(t.FullName + " ");
						  try { errorWriter.WriteLine(mi.ToString()); }
						  catch { errorWriter.WriteLine("UnknownMember"); }
						  errorWriter.WriteLine(e.ToString());
#else
						catch(Exception) 
						{
#endif
							errorCount++;
							continue;
						}

						// Add member to list for this type
						memberList.Add (tm);

						//P12 Added
						//if this is an enum, then we only add ONE entry
						if (isEnum)
							break;
					}
#if DOREPORTS
				catch(Exception eA) {
					  errorWriter.Write("Exception encounterd while working on member ");
					  errorWriter.Write(t.FullName + " ");
					  errorWriter.WriteLine(mi.ToString());
					  errorWriter.WriteLine(eA.ToString());
#else
					catch(Exception) 
					{
#endif
						errorCount++;
						continue;
				
					}
				} // end members loop

				memberList.TrimToSize();
				if (memberList.Count > 0) 
				{	// Add member to hashtable by type.
					memberCount += memberList.Count;
					typememberList.Add(t.Namespace + " " + t.Name, memberList);
				}
#if DOREPORTS
			  errorWriter.Flush();
#endif
				//PART OF THE MOD
				//we want to ADD these changes if the hashtable already existed...
				Hashtable htTemp = new Hashtable();

				if (namespaceExists)
					htTemp = (Hashtable)htNamespaces[nameComp];

				htTemp.Add(t.Namespace + " " + t.Name, memberList);

				htNamespaces[nameComp] = htTemp;

			} // end types loop
	
		} // end CreateMemberList
	

		// Create list of members for the store. Weed out non-public information.
		public static void CreateMemberListByDll(ref Hashtable typememberList, Assembly a, 
			out int errorCount, out int memberCount, int winformsFile) 
		{

			Hashtable problems = new Hashtable(1);	// List of problem members by full type name
			problems.Add("System.Runtime.Remoting.Channels.SMTP.ISMTPMessage", new ArrayList());
			((ArrayList)problems["System.Runtime.Remoting.Channels.SMTP.ISMTPMessage"]).AddRange(new string [] {"EnvelopeFields", "Fields", });

			errorCount = 0;
			memberCount = 0;
			Type [] ta = null;

			string assemblyname = null;
			try { assemblyname = a.GetName().Name; }
			catch { assemblyname = "UnknownAssemblyName"; }

			try { ta = a.GetTypes(); }	// Retrieve all types within this assembly
			catch(ReflectionTypeLoadException e) 
			{
				Exception [] ea = e.LoaderExceptions;
#if DOREPORTS
			  errorWriter.Write("Exceptions in Assembly.GetTypes() for " + assemblyname);
			  for (int i = 0; i < ea.Length; i++)
				errorWriter.WriteLine(String.Format(" - {0:00}: {1}", i, ea[i].ToString()));
#endif
				ta = e.Types;
			}
			catch(Exception e) 
			{
				string message = "Assembly.GetTypes() failed on " + assemblyname;
				throw new ApplicationException(message, e);
			}

			splitRanges = GetCorrectSplit(a);

			foreach (Type t in ta) 
			{	// types loop

				if(!ht.Contains(t.FullName))
				{
					ht.Add(t.FullName,null);

				}

			
				if(t!=null)
				{
					//P12, based on this info, set a bool
					//Console.WriteLine("p1");
					//			if (t.IsEnum)
					//Console.WriteLine(t.FullName);
					bool isEnum = t.IsEnum;


	

					//ranges are included in a windforms output file...
					//this is done because the file is otherwise, WAY too huge!
					if (winformsFile > 0) 
					{

						//retrieve the entry from the splitRanges collection
						//which corresponds to the winformsFIle specified

						string tempString = splitRanges[winformsFile - 1];

						string startPoint = "";
						string endPoint = "";

						if (tempString.IndexOf(",") > 0) 
						{
							startPoint = (tempString.Substring(0, tempString.IndexOf(",")).Trim());

							endPoint = (tempString.Substring(tempString.IndexOf(",") + 1).Trim());
						} 
						else 
						{
							startPoint = tempString;
						}

						int compareStart = t.Name.Length < startPoint.Length ? 
							t.Name.Length : startPoint.Length;
						int compareEnd = t.Name.Length < endPoint.Length ? 
							t.Name.Length : endPoint.Length;

						//these are the different comparisons for the different ranges...
						if (winformsFile == 1) 
						{
							if ( String.Compare(t.Name.ToLower().Substring(0, 
								compareStart), startPoint) > 0 )
								continue;
						} 
						else if (winformsFile == numSplits) 
						{
							if ( String.Compare(t.Name.ToLower().Substring(0,
								compareStart), startPoint) < 0 ) 
								continue;
						} 
						else 
						{
							if ( String.Compare(t.Name.ToLower().Substring(0,
								compareStart), startPoint) < 0 || 
								String.Compare(t.Name.ToLower().Substring(0,
								compareEnd), endPoint)>0)
								continue;
						}

					}

					if (t == null) 
					{
#if DOREPORTS
				  errorWriter.WriteLine("Null type encountered in " + assemblyname);
#endif
						errorCount++;
						continue;
					}

					try 
					{
						//if we're doing serialization check we add everything regardless of visibility
						if(addSer==false)
						{
							if (!t.IsPublic) continue; 
						}
					}
#if DOREPORTS
			catch(Exception e) {
				  errorWriter.Write("Type.IsPublic failed on type ");
				  errorWriter.WriteLine(t.FullName);
				  errorWriter.WriteLine(e.ToString());
#else
					catch(Exception) 
					{
#endif
						errorCount++;
						continue;
					}
		
				
				

					//				if(t.IsEnum && t.IsPublic)
					//				{
					//					Console.WriteLine("\n" + "ENUM:" + t.FullName);
					//					totalEnumCount++;
					//				}

					MemberInfo[] ma= null;

					//P12
					// based on whether you decided it was an enum, make a very specific typemember, JUST for enum!


					// Retrieve all members of each "class"
					try { ma = t.GetMembers (allBindingsLookup); }	
#if DOREPORTS
			catch(Exception e) {
				  errorWriter.Write("Type.GetMembers() failed on type ");
				  errorWriter.WriteLine(t.FullName);
				  errorWriter.WriteLine(e.ToString());
#else
					catch(Exception) 
					{
#endif
						errorCount++;
						continue;
					}




					ArrayList memberList = new ArrayList(ma.Length);
					foreach (MemberInfo mi in ma) 
					{	// members loop

			
						if (problems.ContainsKey(t.FullName) && ((ArrayList)problems[t.FullName]).Contains(mi.Name)) 
						{
#if DOREPORTS
					  if (_dbug > 0) errorWriter.WriteLine("    -- Skipping member " + mi.Name);
#endif
							continue;
						}
#if DOREPORTS
				  if (_dbug > 0) errorWriter.WriteLine("    -- Member: {0}", mi.Name);
#endif

						try 
						{
							// Ignore non-public, non-family methods and constructors. Also ignore property accessors, event methods and cctor's.
							if (mi is MethodBase)
							{
								//robvi we want to grab protected internal methods too
								//if ( !((MethodBase)mi).IsPublic && !((MethodBase)mi).IsFamily )
								if ( !((MethodBase)mi).IsPublic && !((MethodBase)mi).IsFamily && !((MethodBase)mi).IsFamilyOrAssembly)
								{
									continue;
								}
								if ( ((MethodBase)mi).IsSpecialName && (
									mi.Name.StartsWith("get_")
									|| mi.Name.StartsWith("set_")
									|| mi.Name.StartsWith("add_")
									|| mi.Name.StartsWith("remove_")
									|| mi.Name.StartsWith("op_")	// TODO: Add operators back in once GenMemberInfo can compare operators.
									))
									continue;

								if ((mi is ConstructorInfo) && (mi.Name == ".cctor"))
								{
									continue;
								}
								
							}

							// Ignore non-public, non-family fields
							// Robvi:  we only ignore these fields if the layout is not explicit or sequential
						
							//if (mi is FieldInfo && !( ((FieldInfo) mi).IsPublic || ((FieldInfo) mi).IsFamily ) && !t.IsLayoutSequential && !t.IsExplicitLayout)
							if (mi is FieldInfo && !( ((FieldInfo) mi).IsPublic || ((FieldInfo) mi).IsFamily ) )
								continue;

							// Ignore non-public, non-family events (add/remove method is non-public, non-family)
							if (mi is EventInfo && !( ((EventInfo) mi).GetAddMethod(true).IsPublic || ((EventInfo) mi).GetAddMethod(true).IsFamily ) )
								continue;
					
							// robvi FamilyOrAssembly is considered family
							// Ignore non-public, non-family properties (both getter and setter are non-public, non-family)
							//	if ((mi is PropertyInfo) && mi.Name.Equals("RenderRightToLeft"))
							//	{Console.WriteLine("foo" + "\n");}

							if (mi is PropertyInfo && !(
								( 
								((PropertyInfo)mi).GetGetMethod(true) != null &&
								( ((PropertyInfo)mi).GetGetMethod(true).IsPublic || ((PropertyInfo)mi).GetGetMethod(true).IsFamily  || ((PropertyInfo)mi).GetGetMethod(true).IsFamilyOrAssembly  )
								) 
								||
								( 
								((PropertyInfo)mi).GetSetMethod(true) != null &&
								( ((PropertyInfo)mi).GetSetMethod(true).IsPublic || ((PropertyInfo)mi).GetSetMethod(true).IsFamily || ((PropertyInfo)mi).GetSetMethod(true).IsFamilyOrAssembly  )
								))
								)
								continue;

							// Ignore non-public, non-family nested types
							if (mi is Type && !( ((Type) mi).IsNestedPublic || ((Type) mi).IsNestedFamily ) )
								continue;

							// For inherited members, only report ones that are not otherwise hidden
							if (mi.DeclaringType != mi.ReflectedType) 
							{

								MemberInfo[] others = t.GetMember(mi.Name, mi.MemberType, 
									allBindingsLookup);

								if (others.Length == 0) 
								{
									Console.WriteLine("Reflection error on {0} {1}.", t.FullName, mi.Name);
#if DOREPORTS
							  errorWriter.WriteLine("Type.GetMember({0}, {1}, BindingFlags.LookupAll) returned an empty list for {2}",
								mi.Name, mi.MemberType, t.FullName);
#endif
								}
								else if (others.Length == 1 && others[0] != mi) 
								{
									Console.WriteLine("Reflection error on {0} {1}.", t.FullName, mi.Name);
#if DOREPORTS
							  errorWriter.WriteLine("Type.GetMember({0}, {1}, BindingFlags.LookupAll) returned a single, non-matching member for {2}",
								mi.Name, mi.MemberType, t.FullName);
#endif
								}
								else 
								{
									bool good = true;
									foreach (MemberInfo other in others) 
									{		// Filter loop
										if (mi.DeclaringType.IsAssignableFrom(other.DeclaringType) && other != mi) 
										{
											switch (mi.MemberType) 
											{
												case MemberTypes.Constructor :
												case MemberTypes.Method :
													if (GenParameterInfo.PSig((MethodBase)mi) == GenParameterInfo.PSig((MethodBase)other))
														good = false;
													break;
												case MemberTypes.Property :
													if (GenParameterInfo.PSig(((PropertyInfo)mi).GetIndexParameters()) == GenParameterInfo.PSig(((PropertyInfo)mi).GetIndexParameters()))
														good = false;
													break;
												case MemberTypes.Event :
												case MemberTypes.Field :
												case MemberTypes.NestedType :
													good = false;
													break;
												default :
#if DOREPORTS
										errorWriter.WriteLine("Error MemberInfo.MemberType = '{0}' for {1}.{2}",
													((Enum)mi.MemberType).ToString(), t.FullName, mi.Name);
#endif

													break;
											}
											if (!good) break;					// if member unwanted, exit Filter loop
										}
									}

									// ignore "hidden" members, skip to next member info.
									if (!good) continue;	
								}
							}

							// ignore the value__ field on Enum's
							if (mi.Name == "value__" && (t.Equals(Type.GetType("System.Enum")) || 
								t.IsSubclassOf(Type.GetType("System.Enum"))))
								continue;

							// convert memberinfos to typemembers
							TypeMember tm = null;
							try 
							{
#if DOREPORTS
						  if (mi == null) errorWriter.WriteLine("mi == null");
#endif

								// Generate new TypeMember
								
								tm = new TypeMember(mi, t, addSer, isEnum, addStruct, addStructMethod, obsoletewriter);
								
								//robvi:check if we have one a single private constructor with no parameters
								//add this to the shortkey, the reason for doing this here is that this is the
								//only time we parse through the private types
								ConstructorInfo[] constructors = t.GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
									if(constructors.Length==1)
									{
										ConstructorInfo singlector=constructors[0];
										if (singlector.IsPrivate&&singlector.GetParameters().Length==0)
										{
											tm.TypeKey+=":SingleCtor=T";
										}
									}
									else
										tm.TypeKey+=":SingleCtor=F";
								
								MemberInfo[] mems = t.GetMembers(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly);
								string stat=":AllStatic=F";
								if(mems.Length==1)
								{
									if(mems[0].Name==".ctor")
									{
										stat=":AllStatic=T";
									}
								}
								tm.TypeKey+=stat;
									


								//robvi Added: dump all obsolete members to a text file
								//This is often asked e.g. what's marked obsolete in Whidbey?
								//							if(tm.Misc.ToLower().IndexOf("obsoleteattribute-") > 0)
								//							{
								//								if(!ObsoleteHT.Contains(tm.FullName))
								//								{
								//									ObsoleteHT.Add(tm.FullName, null);
								//									//Console.WriteLine("Obsolete:" + tm.FullName);
								//									obsoletewriter.WriteLine(tm.FullName);
								//								}
								//							}
					
							}
#if DOREPORTS
					catch(Exception e) {
						  errorWriter.Write("TypeMember construction failed on member ");
						  errorWriter.Write(t.FullName + " ");
						  try { errorWriter.WriteLine(mi.ToString()); }
						  catch { errorWriter.WriteLine("UnknownMember"); }
						  errorWriter.WriteLine(e.ToString());
#else
						
							catch(Exception e) 
							{
#endif					
								Console.WriteLine(e.Message);
								errorCount++;
								continue;
							}

							// Add member to list for this type
							memberList.Add (tm);

							//P12 Added
							//if this is an enum, then we only add ONE entry
							if (isEnum)
								break;
						}
#if DOREPORTS
				catch(Exception eA) {
					  errorWriter.Write("Exception encounterd while working on member ");
					  errorWriter.Write(t.FullName + " ");
					  errorWriter.WriteLine(mi.ToString());
					  errorWriter.WriteLine(eA.ToString());
#else
						catch(Exception) 
						{
#endif
							errorCount++;
							continue;
				
						}
					} // end members loop

					memberList.TrimToSize();
					if (memberList.Count > 0) 
					{	// Add member to hashtable by type.
						memberCount += memberList.Count;
						//robvi try catch
						try
						{
							typememberList.Add(t.Namespace + " " + t.Name, memberList);
						}
						catch(Exception){}
					}
#if DOREPORTS
			  errorWriter.Flush();
#endif
				} // end types loop

				//copy them across...
				string output = "";
				output += Path.GetFileNameWithoutExtension(a.CodeBase).ToLower();
				if (winformsFile > 0)
					output += "." + String.Format("{0:00}",winformsFile);
				//			output += "." + winformsFile;

				htNamespaces = new Hashtable();		
				htNamespaces.Add(output,typememberList);
				/*
				//PART OF THE MOD
				//we want to ADD these changes if the hashtable already existed...
							Hashtable htTemp = new Hashtable();

							if (namespaceExists)
								htTemp = (Hashtable)htNamespaces[nameComp.ToLower()];

							htTemp.Add(t.Namespace + " " + t.Name, memberList);
							htNamespaces[nameComp.ToLower()] = htTemp;

						} // end types loop
				*/
			}//end if (t!=null)
		} // end CreateMemberListByDll

		// Create difference reports: LibCheck -Compare <oldNumber> <newNumber>
		static void MakeReports() 
		{

			int totAdds = 0;
			int totBreaks = 0;
			int totRemoves = 0;
			int totTotal = 0;
			int totTypeAdds = 0;
			int totTypeRemoves = 0;
			//int totSerTypeBreaks = 0;
	

			//A DUMMY VARIABLE AT THE MOMENT
			bool itemsAdded = false;

			//first thing is to define the compare file output location...
			//only set it to this default value if the user has not specified a location...
			if (outputLoc == "")
				outputLoc = _oldBuild + "to" + _newBuild + "/";

			try 
			{
				//attempt to create the specified directory.
				Directory.CreateDirectory(outputLoc);

				//also, create the 'api_change' directory, as a subdir

				Directory.CreateDirectory(outputLoc + "api_change");

				//create the compat, and non_compat directories...
				Directory.CreateDirectory(outputLoc + "Compat");
				Directory.CreateDirectory(outputLoc + "NonCompat");
			} 
			catch {}

			//POINT2
			if (addsOnly) 
			{
	

				AddsFile = new StreamWriter(outputLoc + "Results." + (useHTM ? "htm" : "html"));

				AddsFile.WriteLine("<html><Head></head>\r\n<body>\r\n");
				AddsFile.WriteLine("<center><h1>Summary Change Report, Adds Only</h1></center><hr>");
			}

			xmlReport = new XmlReport(outputLoc);

#if DOREPORTS
		  xmlReport = new XmlReport(outputLoc);
#endif

			string oldDir = _oldBuild + Path.DirectorySeparatorChar;
			string newDir = _newBuild + Path.DirectorySeparatorChar;

			// initialize the error report
			DateTime startTime = DateTime.Now;
			string errFile = String.Format( outputLoc + 
				"LibCheck.{0:00}{1:00}.{2:00}{3:00}.error",
				new object[] { startTime.Month, startTime.Day, startTime.Hour, startTime.Minute });
#if DOREPORTS
		  errorWriter = new StreamWriter(errFile);
		  if (_dbug != 0) errorWriter.WriteLine("_dbug = " + _dbug);
#endif
		
			// create and initialize the summary html report.
			//		summary = new ChurnReport(_oldBuild, _newBuild);
			if(addSer)
			{	
				summary = new ChurnReport(_oldBuild, _newBuild, outputLoc, false,
					incHeader, sumColor, allDetails, noLink, useHTM, makeComReport, true);
			}
			else if(addStruct||addStructMethod)
			{
				summary = new ChurnReport(_oldBuild, _newBuild, outputLoc, false,
					incHeader, sumColor, allDetails, noLink, useHTM, makeComReport, true, true);
			}
			else
			{
				summary = new ChurnReport(_oldBuild, _newBuild, outputLoc, showChurn,
					incHeader, sumColor, allDetails, noLink, useHTM, makeComReport);
			}

			// Get a list of all store files (old and new). Capitalization may be different
			ArrayList files = null;

			if (storeDB) 
			{
				files = rf.GetDistinctFiles(_oldBuild);

				ArrayList tempAl = rf.GetDistinctFiles(_newBuild);

				foreach(string s in tempAl)
					if (!files.Contains(s))
						files.Add(s);
			}
			else 
			{
				files = new ArrayList();

				foreach (string f in Directory.GetFiles(oldDir, "*.store")) 
				{
					files.Add(Path.GetFileName (f.ToLower()));
				}

				foreach (string f in Directory.GetFiles(newDir, "*.store")) 
				{
					if (!files.Contains(Path.GetFileName (f.ToLower()))) 
					{

						files.Add(Path.GetFileName (f.ToLower()));
					}
				}
			}

			files.Sort(Comparer.Default);

			//here is where we begin the process of creating the output from the store FILES

			bool fileIsSplit = false;
			bool fileAlreadyBegun = false;
			ArrayList alreadyProcessed = new ArrayList();

			// Load and compare Store files
			foreach (string f in files) 
			{

				bool exitLoop = false;

				foreach (string s in alreadyProcessed) 
				{
					if (s.ToLower() == f.ToLower()) 
					{
						exitLoop = true;
						break;
					}
				}

				if (exitLoop)
					continue;					

				bool lastFile = false;

				if (fileIsSplit == false) 
				{
					totAdds = 0;
					totTotal = 0;
					totRemoves = 0;
					totBreaks = 0;
					totTypeAdds = 0;
					totTypeRemoves = 0;
					//totSerTypeBreaks = 0;
				

					fileIsSplit = DetSplit(f);
				}


				if (fileIsSplit) 
				{
					bool getNext = false;
					bool doneLoop = false;
					
					//look at the NEXT file, and determine
					foreach (string s in files) 
					{

						doneLoop = false;

						if (getNext) 
						{
							// fileFound is the name of the last file being processed, short
							// splitFound is the number of the split file found
							string sPart = s.Substring(
								s.ToLower().IndexOf(fileFound.ToLower()) + 
								fileFound.Length + 1, 2);

							if (Char.IsDigit(sPart, 0) && Char.IsDigit(sPart, 1)) 
							{
								if (Convert.ToInt32(sPart) <= splitFound)
									lastFile = true;
							}
							else
								lastFile = true;

							break;
						}

						if (f == s) 
						{
							getNext = true;
							doneLoop = true;
						}
					}

					//indicates that the file IS split, and there is no 'next' item
					if (doneLoop)
						lastFile = true;		
				}

				string origVal = Path.GetFileName(f);

				string newName = newDir + origVal;
				string oldName = oldDir + origVal;

				try 
				{
					//if this is a split file, change the name of the assembly!
					string assembly = null;

					//this works because getfilename simply works with strings!
					if (storeDB) 
					{
						assembly = Path.GetFileName(f);
						if (assembly.IndexOf(".binary.store") >=0)
							assembly = 
								assembly.Substring(0,assembly.IndexOf(".binary.store"));
						if (assembly.IndexOf(".soap.store") >=0)
							assembly = 
								assembly.Substring(0,assembly.IndexOf(".soap.store"));
					}
					else 
					{
						assembly = Trim(f);
					}

					string assembly2 = assembly;

					if (fileIsSplit) 
					{
						//the four is determined from the fact that the end of the name will
						//be '.XX', where xx is the filenum.
						assembly2 = assembly.Substring(0, assembly.Length - 3);
					}

					//only do this if this is either the first time for a split file,
					//OR the file is not split...
					if ( fileIsSplit == false || fileAlreadyBegun == false ) 
					{
						xmlReport.WriteStartAssembly (Path.GetFileName (assembly), 
							_oldBuild, _newBuild);


#if DOREPORTS
					  xmlReport.WriteStartAssembly (Path.GetFileName (assembly), 
						_oldBuild, _newBuild);
#endif
					}

					Hashtable oldList = new Hashtable();

					if (!suppress) 
					{
						Console.Write("\r\nLoading Store {0} via the {1} formatter ...", 
							oldName, (storeSoap ? "soap":"binary"));
					}


					oldList = LoadList(ref oldName, oldDir, false);

					if (oldList != null) 
					{
						if (!suppress)
							Console.WriteLine(" {0} types.", oldList.Count);
					}

					if (oldList.Count == 0) 
					{
						if (lastFile == false) 
						{
							if (!suppress) 
							{
								Console.WriteLine(
									"The oldstore had zero entries, all entries will be marked as added...");
							}
						}
					}

					Hashtable newList = null;

					// Get the typemember hashtable from the new store file, which may not exist.
					if (!suppress) 
					{
						Console.Write("Loading Store {0} via the {1} formatter ...", 
							newName, (storeSoap ? "soap":"binary"));
					}

					newList = LoadList(ref newName, newDir, true);

					if (newList != null) 
					{
						if (!suppress) 
						{
							Console.WriteLine(" {0} types.", newList.Count);
						}
					}

					if (newList.Count == 0 && lastFile == false) 
					{
						if (!suppress) 
						{
							Console.WriteLine(
								"The newstore had zero entries, all entries will be marked as removed...");
						}
					}

					if (oldList == null || newList == null)
						throw new Exception("Error loading Store files, usage:" + usage);
#if DOREPORTS
				  errorWriter.WriteLine(String.Format(
						"\r\nStore '{0}', {1} types.\r\nStore '{2}', {3} types.",
						new object[] { oldName, oldList.Count, newName, newList.Count }));
#endif

					if (oldList.Count + newList.Count == 0 && lastFile == false)
						continue;

					if ( fileIsSplit == false) 
					{
						if (allDetails) 
						{
							if (unified == null)
								unified = new UnifiedReport(_oldBuild, 
									_newBuild, assembly, 
									outputLoc + "\\api_change\\", 
									oldVersion, newVersion,
									allDetails, useHTM);
						} 
						else 
						{
							unified = new UnifiedReport(_oldBuild, 
								_newBuild, assembly, 
								outputLoc + "\\api_change\\", 
								oldVersion, newVersion,
								allDetails, useHTM);
						}
					}
					else if ( fileAlreadyBegun == false ) 
					{
						//figure out what the output file name IS
						if (allDetails) 
						{
							if (unified == null)
								unified = new UnifiedReport(_oldBuild, 
									_newBuild, assembly2, 
									outputLoc + "\\api_change\\", 
									oldVersion, newVersion,
									allDetails, useHTM);
						} 
						else 
						{
							unified = new UnifiedReport(_oldBuild, 
								_newBuild, assembly2, 
								outputLoc + "\\api_change\\", 
								oldVersion, newVersion,
								allDetails, useHTM);
						}
					}					

					// calculate total members and breaking changes and write 
					// detail reports for this assembly.
					// note that only ONE line should be written for split reports, so keep
					// a running tally for these...
					//int added, removed,typesAdded, typesRemoved, total, breaking, serialBreaking;
					int added, removed,typesAdded, typesRemoved, total, breaking;
					Report(oldList, newList, assembly2, out added, out removed, out typesAdded, out typesRemoved,
						out total, out breaking, ref itemsAdded);

					totAdds += added;
					totRemoves += removed;
					totTotal += total;
					totBreaks += breaking;
					totTypeAdds += typesAdded;
					totTypeRemoves += typesRemoved;

					//robvi for serialization we only care if there are breaking changes
					if(addSer||addStruct||addStructMethod)
					{
						totAdds = 0;
						totRemoves = 0;
					}
				
					//DELETE the report, if there are no changes
					if ((fileIsSplit == false) || lastFile) 
					{
						if ((totAdds + totRemoves + totBreaks) <= 0) 
						{
							try 
							{
								//delete the file
								if (allDetails == false) 
								{
									if (unified != null)
										unified.Close(useHTM);

									File.Delete(outputLoc + "api_change\\"
										+ assembly2 + ".Unified." +
										(useHTM ? "htm" : "html"));
									unified = null;
								}
							}
							catch (Exception e) 
							{
								Console.WriteLine("An error occurred deleting the file:");
								Console.WriteLine(outputLoc + "\\api_change\\"
									+ assembly2 + ".Unified." +
									(useHTM ? "htm" : "html"));
								Console.WriteLine(e.ToString());
							}
						}
					}

#if DOREPORTS
				  xmlReport.AssemblyCompat (breaking == 0, _newBuild);
#endif

					if (fileIsSplit == false || fileAlreadyBegun == false) 
					{
						xmlReport.AssemblyCompat (breaking == 0, _newBuild);
						if (breaking == 0) 
						{
							File.Create (outputLoc + "\\Compat\\" + assembly2 + ".compat");
						} 
						else 
						{
							File.Create(outputLoc + "\\NonCompat\\" + assembly2 + ".noncompat");
						}
					}

					// write summary line out to summary report, echo summary to console.
					bool aAdded = oldName.EndsWith("NotFound");
					bool aRemoved = newName.EndsWith("NotFound");

					if ((fileIsSplit == false) || lastFile ) 
					{

						//robvi if/else
						if(!addSer&&!addStruct&&!addStructMethod)
						{
							if (!suppress)  
							{
						
								Console.WriteLine(summary.WriteRow(assembly2, 
									aAdded, aRemoved, 
									totAdds, totRemoves, totTotal, 
									totBreaks, allDetails,
									sumAll, useHTM));
						
							} 
							else 
							{
				
								summary.WriteRow(assembly2, aAdded, aRemoved, 
									totAdds, totRemoves, totTotal, 
									totBreaks, allDetails, sumAll, useHTM);
							
							}
						}
						else
						{	
							if(totBreaks!=0)
							{
								summary.WriteRow(assembly2, aAdded, aRemoved, 
									totAdds, totRemoves, totTotal, 
									totBreaks, allDetails, sumAll, useHTM);
							}
						}
						int change=totAdds+totRemoves;
						int asm1Count = oldList.Count;
						int asm2Count = newList.Count;
					//	xmlReport.WriteAssemblySummary(totTypeAdds.ToString(), totTypeRemoves.ToString(),asm1Count.ToString(), asm2Count.ToString());
					}
				}
#if DOREPORTS
			catch (Exception e) {
				  Console.WriteLine("Error comparing files: {0}, {1}", oldName, newName);
				  errorWriter.WriteLine("Error comparing files: {0}, {1}", oldName, newName);
				  errorWriter.WriteLine(e.ToString());
#else
				catch (Exception e) 
				{

					Console.WriteLine("Error comparing files: {0}, {1}", oldName, newName);
					Console.WriteLine(e.ToString());
#endif
				}
				finally 
				{
					if ((fileIsSplit == false) || lastFile) 
					{
					
						xmlReport.WriteEndAssembly (oldVersion, newVersion);
#if DOREPORTS
					  xmlReport.WriteEndAssembly (oldVersion, newVersion);
#endif

						if (unified != null) 
						{
							if (allDetails == false)
								unified.Close(useHTM);
						}

						fileAlreadyBegun = false;
						fileIsSplit = false;
					}
					else 
					{
						fileAlreadyBegun = true;	
					}
				}
			}

			TimeSpan delta = DateTime.Now - startTime;
#if DOREPORTS
		    errorWriter.WriteLine(String.Format("Report completed, total elapsed time: {0:00}:{1:00}:{2:00}.",
			delta.Hours, delta.Minutes, delta.Seconds));
		
		  errorWriter.Flush();
		  errorWriter.Close();
#endif

			xmlReport.Dispose();
#if DOREPORTS
		  xmlReport.Dispose();
#endif

			summary.Close(allDetails, noLink, showChurn, useHTM);

			//POINT3
			if (addsOnly) 
			{
				if (itemsAdded == false) 
				{
					AddsFile.WriteLine("<br><center><h2><font color=\"blue\">" +
						"Nothing to Report</h2></center><hr>");
				}

				AddsFile.WriteLine("</body></html>");
				AddsFile.Flush();
				AddsFile.Close();
			}

		} //end makereports


		//ALWAYS ASSUMES THAT THE COMPARISON IS CURRENT TO BUILD "X"
		static void MakeCurrentReport() 
		{
			int totAdds = 0;
			int totBreaks = 0;
			int totRemoves = 0;
			int totTotal = 0;

			bool itemsAdded = false;

			//first thing is to define the compare file output location...
			//only set it to this default value if the user has not specified a location...
			if (outputLoc == "") 
			{
				if ((_runCurrentCompare & CurrentCompare.Old) == CurrentCompare.Old)
					outputLoc = "currentto";
				else
					outputLoc = _oldBuild + "to";

				if ((_runCurrentCompare & CurrentCompare.New) == CurrentCompare.New)
					outputLoc += "current" + "/";
				else
					outputLoc += _newBuild + "/";
			}

			try 
			{
				//attempt to create the specified directory.
				Directory.CreateDirectory(outputLoc);


				//also, create the 'api_change' directory, as a subdir

				Directory.CreateDirectory(outputLoc + "api_change");

				//create the compat, and non_compat directories...
				Directory.CreateDirectory(outputLoc + "\\Compat");
				Directory.CreateDirectory(outputLoc + "\\NonCompat");
			} 
			catch {}

			//POINT2
			if (addsOnly) 
			{

				AddsFile = new StreamWriter(outputLoc + "Results." + (useHTM ? "htm" : "html"));

				AddsFile.WriteLine("<html><Head></head>\r\n<body>\r\n");
				AddsFile.WriteLine("<center><h1>Summary Change Report, Adds Only</h1></center><hr>");
			}

			xmlReport = new XmlReport(outputLoc);
#if DOREPORTS
		xmlReport = new XmlReport(outputLoc);
#endif
	
			string newDir = _newBuild + Path.DirectorySeparatorChar;
			string oldDir = _oldBuild + Path.DirectorySeparatorChar;

			// initialize the error report
			DateTime startTime = DateTime.Now;
			string errFile = String.Format( outputLoc + 
				"LibCheck.{0:00}{1:00}.{2:00}{3:00}.error",
				new object[] { startTime.Month, startTime.Day, startTime.Hour, 
								 startTime.Minute });
#if DOREPORTS
	        errorWriter = new StreamWriter(errFile);
	        if (_dbug != 0) errorWriter.WriteLine("_dbug = " + _dbug);
#endif
		
			// create and initialize the summary html report.
			summary = new ChurnReport(_oldBuild, _newBuild, outputLoc, 
				showChurn, incHeader, sumColor,allDetails, noLink, useHTM, makeComReport);

			// Get a list of all store files (old and new). Capitalization may be different
			ArrayList files = new ArrayList();
			//NO LONGER VALID, NEED TO RETRIEVE APPROP FILES AS YOU GO!
			//	    foreach (string f in Directory.GetFiles(oldDir, "*.Store"))
			//		files.Add(Path.GetFileName (f.ToLower()));

			//ONLY DO IF NOT USING DB!
			if (storeDB == false) 
			{
				if ((_runCurrentCompare & CurrentCompare.Old) == CurrentCompare.Old) 
				{
					foreach (string f in Directory.GetFiles(newDir, "*.store")) 
					{
						if (!files.Contains(Path.GetFileName (f.ToLower())))
							files.Add(Path.GetFileName (f.ToLower()));
					}
				} 
				else 
				{
					foreach (string f in Directory.GetFiles(oldDir, "*.store")) 
					{
						if (!files.Contains(Path.GetFileName (f.ToLower())))
							files.Add(Path.GetFileName (f.ToLower()));
					}
				}
			}
			else 
			{
				//get the filenames from the DB...
				ArrayList al = null;
				if ((_runCurrentCompare & CurrentCompare.Old) == CurrentCompare.Old)
					al = rf.GetDistinctFiles(_newBuild);
				else
					al = rf.GetDistinctFiles(_oldBuild);

				foreach (string f in al) 
				{
					if (!files.Contains(Path.GetFileName (f.ToLower())))
						files.Add(Path.GetFileName (f.ToLower()));
				}		
			}

			files.Sort(Comparer.Default);

			bool fileIsSplit = false;
			bool fileAlreadyBegun = false;
			ArrayList alreadyProcessed = new ArrayList();
			bool ignoreItems = false;

			// Load and compare Store files
			foreach (string f in files) 
			{

				bool exitLoop = false;

				foreach (string s in alreadyProcessed) 
				{
					if (s.ToLower() == f.ToLower()) 
					{
						exitLoop = true;
						break;
					}
				}

				if (exitLoop)
					continue;					

				bool lastFile = false;

				if (fileIsSplit == false) 
				{
					totAdds = 0;
					totTotal = 0;
					totRemoves = 0;
					totBreaks = 0;

					fileIsSplit = DetSplit(f);
				}


				//TEMP: Never say a file is split if doing a current compare...
				//if (_runCurrentCompare > CurrentCompare.Specific)
				//	fileIsSplit = false;
				//END TEMP, only applies to that one line. For the mean time, this means files will never be consolidated
				//FROM THIS CHANGE, code may be commented out at the moment...

				//WAS /*

				if (fileIsSplit) 
				{
					bool getNext = false;
					bool doneLoop = false;
					
					//look at the NEXT file, and determine
					foreach (string s in files) 
					{

						doneLoop = false;

						if (getNext) 
						{
							// fileFound is the name of the last file being processed
							// splitFound is the number of the split file found
							string sPart = s.Substring(
								s.ToLower().IndexOf(fileFound.ToLower()) + 
								fileFound.Length + 1, 2);

							if (Char.IsDigit(sPart, 0) && Char.IsDigit(sPart, 1)) 
							{
								if (Convert.ToInt32(sPart) <= splitFound)
									lastFile = true;
							}
							else
								lastFile = true;

							break;
						}

						if (f == s) 
						{
							getNext = true;
							doneLoop = true;
						}
					}

					//indicates that the file IS split, and there is no 'next' item
					if (doneLoop)
						lastFile = true;		
				}

				//WAS */

				string origVal = Path.GetFileName(f);			

				string newName = newDir + origVal;
				string oldName = oldDir + origVal;

				try 
				{
					//if this is a split file, change the name of the assembly!
					string assembly = Trim(f);
					string assembly2 = assembly;

					//the four is determined from the fact that the end of the name will
					//be '.XX', where xx is the filenum.
					if (fileIsSplit)
						assembly2 = assembly.Substring(0, assembly.Length - 3);

					//only do this if this is either the first time for a split file,
					//OR the file is not split...
					if ( fileIsSplit == false || fileAlreadyBegun == false ) 
					{
						xmlReport.WriteStartAssembly (Path.GetFileName (assembly), 
							_oldBuild, _newBuild);
#if DOREPORTS
				  xmlReport.WriteStartAssembly (Path.GetFileName (assembly), 
					_oldBuild, _newBuild);
#endif
					}

					Hashtable oldList = new Hashtable();

					if (!suppress)
						Console.Write("\r\nLoading Current Items ...");

					//THIS IS WHERE YOU NEED TO LOAD THE CURRENT LIST!
					//To do this, use the current system, and pass in the namespace you want to retrieve...
					//have a file which has the dlls that have entries from that namespace in it!
					//			oldList = LoadList(ref oldName, oldDir, false);

					string namespaceName = f.Substring(0, 
						f.ToLower().IndexOf((storeSoap ? ".soap":".binary") + ".store"));

					//figure out now if it has a number on the end, in which case it's split...
					int lastPos = namespaceName.LastIndexOf(".");
					int num = -1;

					if (lastPos >= 0 && lastPos != (namespaceName.Length - 1)) 
					{
						string possNum = namespaceName.Substring(lastPos + 1);

						try 
						{
							num = Convert.ToInt32(possNum);
						} 
						catch {}

						if (num >= 0)
							namespaceName = namespaceName.Substring(0,lastPos);
					}

					// based on the namespace name, load the approp split file to determine dll to load
					//			ArrayList dlls = LoadNameSpaces(namespaceName);

					//IT WILL HAVE TO KNOW WHERE THESE COME FROM!!! SO IT WILL HAVE A LOCATION, HARDCODED AT THIS POINT
					//assume the currently running location...
					if ((_runCurrentCompare & CurrentCompare.Old) == CurrentCompare.Old) 
					{
						//Console.WriteLine("p1");
						//Console.ReadLine();
						oldList = LoadSpecificList(_codebase, namespaceName, num);
						//if (oldList == null)
						//	Console.WriteLine("yup!");
						//Console.WriteLine("p2");
						//Console.ReadLine();
					} 
					else 
					{
						oldList = LoadList(ref oldName, oldDir, false);
					}

					if (oldList != null) 
					{
						if (!suppress) 
						{
							Console.WriteLine(" {0} types.", oldList.Count);
						}
					} 
					else 
					{
						//ASSUME this means zero entries...
						oldList = new Hashtable();
					}

					if (oldList.Count == 0) 
					{

						if (lastFile == false) 
						{
							if (!suppress) 
							{
								Console.WriteLine(
									"The current items had zero entries, " +
									"all entries will be marked as added...");
							}
						}
					}

					Hashtable newList = null;

					// Get the typemember hashtable from the new store file, which may not exist.
					if (!suppress)
						Console.Write("Loading Store {0}  ...", newName);

					if ((_runCurrentCompare & CurrentCompare.New) == CurrentCompare.New)
						newList = LoadSpecificList(_codebase, namespaceName, num);
					else
						newList = LoadList(ref newName, newDir, true);

					if (newList != null) 
					{
						if (!suppress) 
						{
							Console.WriteLine(" {0} types.", newList.Count);
						}
					}

					if (newList.Count == 0 && lastFile == false) 
					{
						if (!suppress) 
						{
							Console.WriteLine(
								"The newstore had zero entries, " + 
								"all entries will be marked as removed...");
						}
					}

					if (oldList == null || newList == null)
						throw new Exception("Error loading Store files, usage:" + usage);

					if (oldList.Count + newList.Count == 0 && lastFile == false)
						continue;

					if ( fileIsSplit == false) 
					{

						if (allDetails) 
						{
							if (unified == null)
								unified = new UnifiedReport(_oldBuild, _newBuild, assembly, 
									outputLoc + "\\api_change\\", oldVersion, newVersion,
									allDetails, useHTM);
						} 
						else 
						{
							unified = new UnifiedReport(_oldBuild, _newBuild, assembly, 
								outputLoc + "api_change\\", oldVersion, newVersion,
								allDetails, useHTM);
						}
					}
					else if ( fileAlreadyBegun == false ) 
					{
						//figure out what the output file name IS
						if (allDetails) 
						{
							if (unified == null)
								unified = new UnifiedReport(_oldBuild, _newBuild, assembly2, 
									outputLoc + "\\api_change\\", oldVersion, newVersion,
									allDetails, useHTM);
						} 
						else 
						{
							unified = new UnifiedReport(_oldBuild, _newBuild, assembly2, 
								outputLoc + "\\api_change\\", oldVersion, newVersion,
								allDetails, useHTM);
						}
					}					

					int added, removed, total, breaking, typesAdded, typesRemoved;
					//POINT1
					Report(oldList, newList, assembly2, out added, out removed, out typesAdded, out typesRemoved,
						out total, out breaking, ref itemsAdded);

					totAdds += added;
					totRemoves += removed;
					totTotal += total;
					totBreaks += breaking;

					//DELETE the report, if there are no changes
					if ((fileIsSplit == false) || lastFile) 
					{
						if ((totAdds + totRemoves + totBreaks) <= 0) 
						{
							try 
							{
								//delete the file
								if (allDetails == false) 
								{
									if (unified != null)
										unified.Close(useHTM);

									File.Delete(outputLoc + "api_change\\"
										+ assembly2 + ".Unified." +
										(useHTM ? "htm" : "html"));
									unified = null;
								}
							} 
							catch (Exception e) 
							{
								Console.WriteLine("Error occurred deleting the file:");
								Console.WriteLine(outputLoc + "\\api_change\\"
									+ assembly2 +".Unified." +
									(useHTM ? "htm" : "html"));
								Console.WriteLine(e.ToString());
							}
						}
					}

#if DOREPORTS
			  xmlReport.AssemblyCompat (breaking == 0, _newBuild);
#endif

					if (fileIsSplit == false || fileAlreadyBegun == false) 
					{
						xmlReport.AssemblyCompat (breaking == 0, _newBuild);

						if (breaking == 0)
							File.Create (outputLoc + "\\Compat\\" + assembly2 + ".compat");
						else
							File.Create(outputLoc + "\\NonCompat\\" + assembly2 + ".noncompat");
					}

					// write summary line out to summary report, echo summary to console.
					bool aRemoved = newName.EndsWith("NotFound");

					if ((fileIsSplit == false) || lastFile ) 
					{
						//robvi 
						if(!addSer&&!addStruct&&!addStructMethod)
						{
							if (!suppress) 
							{
								Console.WriteLine(summary.WriteRow(assembly2, false, 
									aRemoved, totAdds, totRemoves, 
									totTotal, totBreaks, allDetails, 
									sumAll, useHTM));
							}
							else 
							{
								summary.WriteRow(assembly2, false, aRemoved, 
									totAdds, totRemoves, totTotal, totBreaks, 
									allDetails, sumAll, useHTM);
						
							}
						}
							//robvi when it's a serial report we just report type breaks per assembly
						else
						{
							if(totBreaks!=0)
							{
								summary.WriteRow(assembly2, false, aRemoved, 
									totAdds, totRemoves, totTotal, totBreaks, 
									allDetails, sumAll, useHTM);
							}

						}

				
				
					}
				}
#if DOREPORTS
		catch (Exception e) {
			  errorWriter.WriteLine(e.ToString());
#else
				catch (Exception) 
				{
#endif
				}
				finally 
				{
					//this is the boolean that gets set because multiple beta1
					// classes are being amalgamated into one beta2 class
					if (ignoreItems)
						ignoreItems = false;

					if ((fileIsSplit == false) || lastFile) 
					{

						xmlReport.WriteEndAssembly (oldVersion, newVersion);
#if DOREPORTS
				  xmlReport.WriteEndAssembly (oldVersion, newVersion);
#endif

						if (unified != null)
							if (allDetails == false)
								unified.Close(useHTM);

						fileAlreadyBegun = false;
						fileIsSplit = false;
					} 
					else 
					{
						fileAlreadyBegun = true;	
					}
				}
			}

			//POINT3
			if (addsOnly) 
			{
				if (itemsAdded == false) 
				{
					AddsFile.WriteLine("<br><center><h2><font color=\"blue\">" +
						"Nothing to Report</h2></center><hr>");
				}

				AddsFile.WriteLine("</body></html>");
				AddsFile.Flush();
				AddsFile.Close();
			}
		} //end makecurrentreport

		// load a store given a file name (predictive, can request one that doesn't exist)
		public static Hashtable LoadSpecificList (string location, string space, int num) 
		{

			bool prevSpec = fullSpec;

			fullSpec = true;
			fileDir = location;
			//need to set the name of the _assembly object to the approp dll...

			_assembly = space + ".dll";

			MakeCurrentStoreFiles(num);
			//Console.WriteLine("p2");
			//Console.ReadLine();
			fullSpec = prevSpec;

			//if doing this, there will only ever be ONE ENTRY...
			//return that entry since it is itself a hashtable
			if (num < 0)
				return (Hashtable)(htNamespaces[space.ToLower()]);
			else
				return (Hashtable)(htNamespaces[(space + "." + 
					String.Format("{0:00}",num)).ToLower()]);
			//			return (Hashtable)(htNamespaces[(space + "." + num).ToLower()]);
		}

		// load a store given a file name (predictive, can request one that doesn't exist)
		public static Hashtable LoadList (ref string name, string dir, bool newVers) 
		{

			if (storeDB) 
			{			

				return rf.GetKitType(name);
			}
			else if (storeAsFile) 
			{
				return GetFromFile(name, newVers);
			}
			else 
			{
				try 
				{

					FileStream s2 = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
					DateTime t = DateTime.Now;

					if (!suppress) 
					{
						Console.WriteLine ("Starting Deserialize: {0}", t);
					}

					Hashtable result;

					if (storeSoap) 
					{
						SoapFormatter sFormatter = new SoapFormatter();
						result = (Hashtable)sFormatter.Deserialize(s2);
					}
					else 
					{
						BinaryFormatter bFormatter = new BinaryFormatter();
						result = (Hashtable)bFormatter.Deserialize(s2);
					}

					if (!suppress)
						Console.WriteLine ("Finished Deserialize: {0}", DateTime.Now - t);

					s2.Close();

					return result;
				}
#if DOREPORTS
		    catch(FileNotFoundException e) {
			  errorWriter.WriteLine("Could not find store file, " + name);
			  errorWriter.WriteLine(e.ToString());
#else
				catch(FileNotFoundException) 
				{
#endif
					name = dir + "NotFound";
					return new Hashtable(0);
				}
				catch(Exception e) 
				{
					Console.WriteLine("Could not load store file, " + name);
					Console.WriteLine(e.ToString());
#if DOREPORTS
			  errorWriter.WriteLine("Could not load store file, " + name);
			  errorWriter.WriteLine(e.ToString());
#endif
					return null;
				}
			}
		}
	
		// returns the assembly given a file name of a certain format.
		public static string Trim (string s)
		{
			int index = s.LastIndexOf (".");
			s = s.Remove (index, s.Length-index);
			index = s.LastIndexOf (".");
			s = s.Remove (index, s.Length-index);

			return s;
		}
 
	
		// Create unified and split reports. Also, calculate breaking changes.
		public static void Report (Hashtable oldList, Hashtable newList, string assembly,
			out int added, out int removed, out int typesAdded, out int typesRemoved, out int total, 
			out int breaking, ref bool itemsAdded) 
		{
			// initialize counters
			added = removed = typesAdded = typesRemoved = total = breaking = 0;
		
			// collect all unique types from both lists and calculate total members in most recent list.
			ArrayList alltypes = new ArrayList();

			foreach(string type in oldList.Keys) 
			{

				alltypes.Add(type.Trim());
			
				// check if this is in the new list, if not it has been removed
				if (!newList.ContainsKey(type))
				{
					typesRemoved++;
				}

				if (newList == null || newList.Count <= 0)
					total += ((ArrayList)oldList[type]).Count;
			}

			foreach(string type in newList.Keys) 
			{
				total += ((ArrayList)newList[type]).Count;
		
				// check if this type is in the old list, if not it was added
				if (!oldList.ContainsKey(type))
				{
					typesAdded++;
				}


				if (!alltypes.Contains(type.Trim()))
					alltypes.Add(type.Trim());
			}

			alltypes.Sort();

			// report all changes
			foreach (string type in alltypes) 
			{
			
				//MAKE THE TOTAL REPORT
		

				if (type == null || type == "") 
				{
#if DOREPORTS
				  errorWriter.WriteLine(" Blank or null type key found (and skipped).");
#endif
					continue;
				}

				// pick up the new and old member arraylists for this type
				ArrayList oldmembers = (oldList.ContainsKey(type)) ? (ArrayList)oldList[type] : new ArrayList(0);
				ArrayList newmembers = (newList.ContainsKey(type)) ? (ArrayList)newList[type] : new ArrayList(0);
			
				//initialize the member counts for this type for each particular assembly
				int asm1Members, asm2Members;
				asm1Members = asm2Members=0;

				// Count the original number of members
				if (oldmembers != null)
				{
					asm1Members = oldmembers.Count;
				}
		
				if (newmembers != null)
				{
					asm2Members = newmembers.Count;
				}


				
#if DOREPORTS
			  errorWriter.WriteLine("Type: '{0,-48}' - old members: {1,3}, new members: {2,3}", 
					type, oldmembers.Count, newmembers.Count);
#endif

				//if (oldmembers.Count + newmembers.Count == 0)
				//	continue;

				// generate reports and count new and removed members and breaking changes
				breaking += ReportMembers(oldmembers, newmembers, ref added, 
					ref removed, ref itemsAdded, asm1Members, asm2Members);
			}
		}

		public static void  DoXmlReport (SortedList changes, int asm1Members, int asm2Members) 
		{
			string typeName = "";
			int added, removed;
			added=0;
			removed=0;
			foreach (DictionaryEntry e in changes) 
			{
				
				object [] values = (object[])e.Value;
			
				CompareResults res = ((CompareResults)values[2]);

				if (res == CompareResults.Breaking || 
					res == CompareResults.Different ||
					res == CompareResults.NonBreaking) 
				{
					if (values[0] != null) 
					{ //Removed breakers
						TypeMember otm = (TypeMember)values[0];
						removed++;
                    
						if (typeName != otm.TypeName) 
						{
							xmlReport.EndTypeSection(oldVersion, newVersion);
							xmlReport.StartTypeSection(otm, oldVersion, newVersion);
							typeName = otm.TypeName;
						}
						xmlReport.MemberRemoved (otm);			
					}

					if (values[1] != null) 
					{ //Added Breakers
						TypeMember ntm = (TypeMember)values[1];
						added++;

						if (typeName != ntm.TypeName) 
						{
							xmlReport.EndTypeSection(oldVersion, newVersion);
							xmlReport.StartTypeSection(ntm, oldVersion, newVersion);
							typeName = ntm.TypeName;
						}
						xmlReport.MemberAdded (ntm);
					}
				}
			
			}

			//only report if something has changed.
			if((added+removed)>0) 
			{
				int changed = added+removed;
				//		System.Type type;
				//robvi			type = System.Reflection.
				
				xmlReport.WriteTypeSummary(added.ToString(), removed.ToString(), asm1Members.ToString(), asm2Members.ToString());
			}
			xmlReport.EndTypeSection(oldVersion, newVersion);
		}

		//THIS IS WHERE BREAKING CHANGES ARE CALCULATED...
		// handle reporting for each type.
		public static int ReportMembers(ArrayList oldlist, ArrayList newlist, 
			ref int added, ref int removed, ref bool itemsAdded, int asm1Members, int asm2Members) 
		{
			//Console.WriteLine("p0");
			int breaking = 0;
			bool itemsRemoved = false;
			////if (oldlist == null || newlist == null || oldlist[0] == null || newlist[0] == null)
			//Console.WriteLine("p0");
			//		TypeMember temptmold = (oldlist.Count > 0) ? (TypeMember)oldlist[0] : null;
			//		TypeMember temptmnew = (newlist.Count > 0) ? (TypeMember)newlist[0] : null;
			//		bool typeAdded = false;
			//		TypeMember.EvalTypeChange(temptmold, temptmnew, alIntfcAdds, addSer, out typeAdded);
			//		Console.WriteLine(typeAdded);
			//		Console.ReadLine();
			//Console.WriteLine("p1");
			TypeMember oldtm = (oldlist.Count > 0) ? (TypeMember)oldlist[0] : null;
			TypeMember newtm = (newlist.Count > 0) ? (TypeMember)newlist[0] : null;
			//Console.WriteLine("p2");
			////if (oldtm.IsEnum && newtm == null)
			//Console.WriteLine("p1");
			if (oldtm == null && newtm == null) 
			{
#if DOREPORTS
			  errorWriter.WriteLine("bad member lists passed to ReportMembers.");
#endif
				return 0;
			}

			// Determine the level of change between types.
			string okey = (oldtm != null) ? oldtm.TypeKey : "";
			string nkey = (newtm != null) ? newtm.TypeKey : "";
			CompareResults typediff;
			bool typeAdded = false;

			try 
			{ 
				
				//HERE IS WHERE WE COMPARE TYPE CHANGES!!!
				typediff = TypeMember.EvalTypeChange(oldtm, newtm,
					alIntfcAdds, addSer, out typeAdded, addStruct, addStructMethod);

				if (typediff == CompareResults.Breaking) 
				{
					breaking++;
				}

				//Console.WriteLine("Could not compare Containing Types");
#if DOREPORTS
                } catch (Exception e) {
			  errorWriter.Write("Could not compare Containing Types: ");
			  errorWriter.Write(((oldtm != null) ? oldtm.TypeName : "<null>") + ", ");
			  errorWriter.WriteLine(((newtm != null) ? newtm.TypeName : "<null>") + ", ");
			  errorWriter.WriteLine(e.ToString());
#else
			} 
			catch (Exception) 
			{
#endif
				typediff = CompareResults.Same;
				//Console.WriteLine(e.Message);
				return 0; //???
			}
			//Console.WriteLine("p5");
			//P12 ONLY do this if it is not an enum...
			//if (oldtm.IsEnum) {
			//	Console.WriteLine("old = null ?" + oldlist);
			//	Console.WriteLine(newlist);
			//}
			//P12, check if is an enum...

			SortedList changes = new SortedList();
			//Console.WriteLine("p6: {0}, {1}", oldtm == null, newtm == null);
			if ((oldtm != null && oldtm.IsEnum) || (newtm != null && newtm.IsEnum)) 
			{
				//Console.WriteLine("p7: " + (oldtm == null).ToString() + (newtm == null).ToString());
				//DON'T DO THIS
			} 
			else 
			{
				changes = GetChanges(oldlist, newlist, ref added, ref removed,
					ref itemsRemoved, typeAdded);
				//Console.WriteLine("p4");
				//P12 TEMPORARY, to have this inside the if, move it out once fixed...

				DoXmlReport (changes, asm1Members, asm2Members);
#if DOREPORTS
		  DoXmlReport (changes);
#endif
				//robvi
				if(!addSer&&!addStruct&&!addStructMethod)
				{
					if (changes.Count == 0)
						return 0;
				}
			}
			int tempAdded = added;
			int tempRemoved = removed;
			int preTotal = added + removed;
			bool noRows = false;
			bool goodToGo = true;
			string tempName = "";

			foreach (DictionaryEntry entry in changes) 
			{

				object[] value = (object[])entry.Value;

				if ( value[0] != null ) 
				{
					tempName = "<b>" + ((TypeMember)value[0]).Namespace + "." +
						((TypeMember)value[0]).TypeName + "</b>";
					//			    if (addsOnly == false) {
					if (unified.RemovedMember(((TypeMember)value[0]).MemberToHtml("black"), 
						true, ref tempRemoved, true ) ||
						(CompareResults)value[2] == CompareResults.Breaking) 
					{
						noRows = true;

						//NOTE: this becomes an unnecessary check if you include the outer check
						//					if (addsOnly == false)
						break;
					}
					//			    }
				}

				if (value[1] != null) 
				{
					tempName = "<b>" + ((TypeMember)value[1]).Namespace + "." +
						((TypeMember)value[1]).TypeName + "</b>";

					if (unified.AddedMember(((TypeMember)value[1]).MemberToHtml("black"), 
						true, ref tempAdded, true ) ||
						(CompareResults)value[2] == CompareResults.Breaking) 
					{
						noRows = true;

						if (addsOnly) 
						{
							foreach (DictionaryEntry de in changes) 
							{
								if (((object[])de.Value)[0] != null) 
								{
									goodToGo = true;
									break;
								}
								goodToGo = false;
							}
						}

						if (goodToGo == false && itemsRemoved == false) 
						{

							//WRITE THIS TO THE SHORT SUMMARY FILE...
							AddsFile.WriteLine("<br>\r\n<b><font color=\"red\">" + 
								"Type Added:</font> " +
								((TypeMember)value[1]).Namespace + "." +
								((TypeMember)value[1]).TypeName + "</b>\r\n<p><hr>");
							itemsAdded = true;
						}

						if (itemsRemoved)
							goodToGo = true;

						break;
					}
				}
			}

			//if (oldtm.IsEnum)
			//Console.WriteLine("p1: " + noRows);
			//P12 Modified
			//Console.WriteLine("p1: {0}, {1}", oldtm==null, newtm==null);
			if ( noRows || (((oldtm != null && oldtm.IsEnum) || 
				(newtm != null && newtm.IsEnum)) &&
				(typediff == CompareResults.Breaking || 
				typediff == CompareResults.NonBreaking))) 
			{
				// What???
				//PASS THROUGH A BOOLEAN, indicating whether to check for the misc value on this type
				//ONLY do this for the first dictionaryentry checked, if the first doesn't have it, NONE will
				//Console.WriteLine("p2");
				// write type to the split and unified reports
				ReportType( oldtm, newtm, typediff, changes );
				//Console.WriteLine("p3");
				//if it is the second condition, that is, it is an enum, then
				//figure out if it is breaking. If not, remove one from the breaking total
				//			if (noRows == false) {
				//				breaking--;
				//			}
				//if one of these is an enum, we ened to remove the entry...

			}

				//robvi
			else if((addSer||addStruct||addStructMethod)&&typediff == CompareResults.Breaking)
			{
				ReportType( oldtm, newtm, typediff, changes );
			}
			else 
			{
				added = tempAdded;
				removed = tempRemoved;
				return 0;
			}

			string membertext = null;
			string  color;

			foreach (DictionaryEntry entry in changes) 
			{

				object[] value = (object[])entry.Value;

				CompareResults memberdiff = (CompareResults)value[2];	// third element is change level
				color = "black";

				if (memberdiff == CompareResults.Breaking) 
				{	// calculate breaking changes
					if (noColor == false)
						color = "B0000"; //red
					else
						color = "black";
					if(!addSer&&!addStruct&&!addStructMethod)
						breaking++;
				}

				//if (addsOnly == false) {
				if (value[0] != null) 
				{
					/*
					if (((TypeMember)value[0]).TypeName == "CallType") {
						Console.WriteLine("yup: OLD, " + ((TypeMember)value[0]).TypeName);
						Console.ReadLine();
					}
					*/
					// SHOULDN'T THIS BE HERE???
					// color = "red";
					bool wasBlack = false;

					if (color == "black") 
					{
						wasBlack = true;
					}

					//				color = "B0000"; //red

					TypeMember otm = (TypeMember)value[0];

					membertext = otm.MemberToHtml(color);

					// changes the output format of enum members...
					membertext = DetermineIfEnum(membertext);
				
	
					//where the hell does 18 come from???
					if (otm.Misc != null && otm.Misc.IndexOf("obsoleteattribute-") >= 0 &&
						((value[1] != null && ((TypeMember)value[1]).Misc != null &&
						otm.Misc != ((TypeMember)value[1]).Misc) ||
						(value[1] != null && ((TypeMember)value[1]).Misc == null) ||
						value[1] == null)) 
					{
						//						if (noColor)
						int loc = otm.Misc.IndexOf("obsoleteattribute-");
						string sub = otm.Misc.Substring(loc + 
							"obsoleteattribute-".Length);
						int end = sub.IndexOf(";") >=0 ? 
							sub.IndexOf(";") : -1;

						if (end >=0)
							sub = sub.Substring(0,end);

						membertext = "<font color=\"black\">" + 
							"[Obsolete: " +	sub +
							"]" + membertext.Substring(20);
						//						else
						//							membertext = "<font color=\"B0000\">" + "[Obsolete] //" +
						//								membertext.Substring(20);
					}

					// compile text for unified report
					unified.RemovedMember(membertext, wasBlack, ref removed, false);

					// write member to split report
					if (wasBlack)
						color = "black";
				}
				//}

				if (value[1] != null) 
				{
					/*
					if (((TypeMember)value[1]).TypeName == "CallType") {
						Console.WriteLine("yup: " + ((TypeMember)value[1]).TypeName);
						Console.ReadLine();
					}
					*/
					// DON'T!!! Some adds are breaking, such as adding an 
					// abstract member to an abstract type...
					// ALWAYS MAKE ADDS BLACK!
					//	color = "black";

					TypeMember ntm = (TypeMember)value[1];

					membertext = ntm.MemberToHtml(color);

					//changes the output format of enum members...
					membertext = DetermineIfEnum(membertext);


					if (ntm.Misc != null && ntm.Misc.IndexOf("obsoleteattribute-") >= 0 &&
						((value[0] != null && ((TypeMember)value[0]).Misc != null &&
						ntm.Misc != ((TypeMember)value[0]).Misc) ||
						(value[0] != null && ((TypeMember)value[0]).Misc == null) ||
						value[0] == null)) 
					{

						int loc = ntm.Misc.IndexOf("obsoleteattribute-");
						string sub = ntm.Misc.Substring(loc + 
							"obsoleteattribute-".Length);
						int end = sub.IndexOf(";") >=0 ? 
							sub.IndexOf(";") : -1;

						if (end >=0)
							sub = sub.Substring(0,end);

						membertext = "<font color=\"black\">" + 
							"[Obsolete: " +	sub +
							"]" + membertext.Substring(20);

						//						membertext = "<font color=\"black\">" + "[Obsolete] " +
						//							membertext.Substring(20);
					}

					// compile text for unified report
					unified.AddedMember(membertext, (color == "black"), ref added, false);
				}

			}

			//WEIRD LOGIC, a workaround at this point...
			if ( (added + removed) > 0 || preTotal == 0) 
			{
				if (goodToGo) 
				{
					if (addsOnly && (added > 0) && unified.GetAddedMember().Trim() != "") 
					{
						//write this to the short summary file!
						AddsFile.WriteLine("<br>\r\n<b><font color=\"blue\">" +
							"Type Name ==></font></b> " +
							tempName + "\r\n<br><b><font color=\"red\">" + 
							"Members Added:</font></b> " + 
							unified.GetAddedMember() + "\r\n<p><hr>");

						itemsAdded = true;
					}
				}
				//this was MOVED, since we want to write it out, regardless of good to go!
				if(!addSer&&!addStruct&&!addStructMethod)
					unified.WriteMemberRow();	// write member info to unified report
			}

			return breaking;
		}


		// Remove common members from both lists
		// TODO: If type is changed, are all members reported as changed?
		public static SortedList GetChanges( ArrayList olist, ArrayList nlist, ref int added, ref int removed, ref bool itemsRemoved, bool typeAdded)
		{
			// list by MemberShortKey of old TypeMember, new TypeMember pairs, and change level
			// necessary since olist and nlist are by ref, event if they appear not to be.
			ArrayList oldlist = new ArrayList(olist);		

			ArrayList newlist = new ArrayList(nlist);

			SortedList results = new SortedList(Comparer.Default, oldlist.Count);

			// remove common members. add any which have changed to the list along with their change level
			int max = newlist.Count;

			CompareResults memberdiff;
			// for each new TypeMember
			for (int i = 0; i < max;) 
			{											
				//Console.WriteLine (i);

				bool found = false;

				TypeMember ntm = (TypeMember)newlist[i];

				
				

				// search for an old TypeMember with the same signature
				for (int j = 0; j < oldlist.Count; j++) 
				{

					// FIRST COMPARISON CHECK IS HERE!
					TypeMember otm = (TypeMember)oldlist[j];

					if (otm.MemberShortKey != ntm.MemberShortKey)
						continue;

					// OK, So here, you have found a match: type and name...
					// the TypeMember.EvalMemberChange procedure will 
					// determine how LARGE the change was...
					// evaluate the change level when a match is found.
					memberdiff = TypeMember.EvalMemberChange(otm, ntm, alIntfcAdds, typeAdded);

					//Console.WriteLine (memberdiff);
					switch (memberdiff) 
					{

							// add any changes to the change list
						case CompareResults.Breaking:

							//P12 added
							//Console.WriteLine(ntm.MemberShortKey);
							//Console.ReadLine();
							goto case CompareResults.NonBreaking;


						case CompareResults.NonBreaking:

							//A MOD BY CHRISKG, 3/7/2001, please verify if required...
							if (memberdiff == CompareResults.NonBreaking) 
							{
								added++;
								removed++;
							}

							try 
							{

								results.Add(ntm.MemberShortKey, 
									new object[] { otm, ntm, memberdiff });

#if DOREPORTS
					} catch (Exception e) {
						  errorWriter.WriteLine("Error adding change for: " +
							"'{0}', '{1}'.", otm.FullName, ntm.FullName);

						  errorWriter.WriteLine(e.ToString());
#else
							} 
							catch (Exception) 
							{
#endif
							}
							goto case CompareResults.Same;
						case CompareResults.Same:

							found = true;
							newlist.RemoveAt(i);
							oldlist.RemoveAt(j);
							max--;
							itemsRemoved = true;
							continue;

						case CompareResults.Different:	// nothing should evaluate as different at this level

						default:
#if DOREPORTS
					errorWriter.WriteLine("Error in LibCheck: " +
						"RemoveRange:\r\n\told short key: " + 
						"'{0}'\r\n\tnew short key: '{1}'\r\n\t" +
						" - difference level: {2}",
						otm.MemberShortKey, ntm.MemberShortKey,
						((Enum)memberdiff).ToString());
#endif

							continue;
					}
				}

				if (!found) 
				{												

					// if no matches found, then this is a new member

					
					added++;
			
					//robvi Added for Ambiguity problem
					//here we want to check for overloaded methods that differ by reference types
					//at this point we now ntm is an added member, check if this member is overloading
					//an existing method
					//ROBVI adding ambiguity check portion here
				
					//IsAmbiguous(ntm, newlist);


					memberdiff = TypeMember.EvalMemberChange(null, ntm, alIntfcAdds,
						typeAdded);



					
			

					try 
					{

						results.Add(ntm.MemberShortKey, new object[] { null, ntm, memberdiff });

#if DOREPORTS
				} catch (Exception e) {
					  errorWriter.WriteLine("Error adding change for: null, '{0}'.", ntm.FullName);
					  errorWriter.WriteLine(e.ToString());
#else
					} 
					catch (Exception) 
					{
#endif
					}

					i++;
				}
			}

			// for all members left in the old member list then this is a removed member
			foreach (TypeMember otm in oldlist) 
			{	

				// if there's an entry in the change list, there's something wrong
				if (results.ContainsKey(otm.MemberShortKey))
#if DOREPORTS
				errorWriter.WriteLine("Error in LibCheck: RemoveRange: {0} was not removed " + 
				"from old list, but shows up in the change list anyway.", otm.FullName);
#endif

					removed++;
				//			bool typeAdded = false;
				memberdiff = TypeMember.EvalMemberChange(otm, null, alIntfcAdds, typeAdded);

				try 
				{

					results.Add(otm.MemberShortKey, new object[] { otm, null, memberdiff });
			
#if DOREPORTS
			} catch (Exception e) {
				errorWriter.WriteLine("Error adding change for: '{0}', null.", otm.FullName);
				errorWriter.WriteLine(e.ToString());
#else
				} 
				catch (Exception) 
				{
#endif
				}
			}

			return results;
		}
	
		// report type and include a change row if necessary.
		public static void ReportType(TypeMember otm, TypeMember ntm, 
			CompareResults diff, SortedList changes) 
		{
			string writeVal = "";
			bool writeToReport = false;


			if (addStructMethod) 
			{
			
				string added = "";
				string removed ="";
				string extraVal;
				int istart;
				int iend;

		

				if (otm.Misc.ToLower().IndexOf("structmethods") >= 0 || 
					ntm.Misc.ToLower().IndexOf("structmethods") >= 0) 
				{

					istart = otm.Misc.ToLower().IndexOf("structmethods");
					iend = otm.Misc.ToLower().IndexOf(";", istart + 1);

					if (iend < 0)
						iend = otm.Misc.Length - 1;

					string otmser;
					if(istart<0)
						otmser="";
					else
						otmser = otm.Misc.Substring(istart, iend - istart);
				

					istart = ntm.Misc.ToLower().IndexOf("structmethods");
					iend = ntm.Misc.ToLower().IndexOf(";", istart + 1);

					if (iend < 0)
						iend = ntm.Misc.Length - 1;

					string ntmser;
					if(istart<0)
						ntmser="";
					else
						ntmser = ntm.Misc.Substring(istart, iend - istart);
				
				

		
					
					if (ntmser != otmser)
					{
						bool ocomvistype, ocomvisasm;
						bool ncomvistype, ncomvisasm;
					
						string oldcom = otm.Misc.Substring(otm.Misc.LastIndexOf(";")+1);
						string newcom = ntm.Misc.Substring(ntm.Misc.LastIndexOf(";")+1);
						
						ocomvistype = oldcom.Substring(0,oldcom.IndexOf(","))=="comvistype=True" ?  true : false;
						ocomvisasm = oldcom.Substring(oldcom.IndexOf(",")+1)=="comvisasm=True" ? true : false;

						ncomvistype = newcom.Substring(0,newcom.IndexOf(","))=="comvistype=True" ? true : false;
						ncomvisasm = newcom.Substring(newcom.IndexOf(",")+1)=="comvisasm=True" ? true : false;
						

						writeToReport = true;
						//robvi added this to write out what's been removed or added:
						Hashtable oldSerFields = new Hashtable();
						Hashtable newSerFields = new Hashtable();
						int start = otmser.IndexOf("=");
						int sep = 0;
						string sub;
					
						while(start < otmser.Length)
						{
							sep = otmser.IndexOf(',',start+1);
							if(sep==-1)//end case
							{
								sub = otmser.Substring(start+1);
								oldSerFields.Add(sub,null);
								break;
							}
							sub = otmser.Substring(start+1,sep-(start+1));
							if(!oldSerFields.ContainsKey(sub))
								oldSerFields.Add(sub, null);
							start=sep;
						}

						sep = 0;
						start = ntmser.IndexOf("=");
						while(start < ntmser.Length)
						{
							sep = ntmser.IndexOf(',',start+1);
							if(sep==-1)//end case
							{
								sub = ntmser.Substring(start+1);
								if(!newSerFields.ContainsKey(sub))
									newSerFields.Add(sub,null);
								break;
							}
							sub = ntmser.Substring(start+1,sep-(start+1));
							if(!newSerFields.ContainsKey(sub))
							{
								newSerFields.Add(sub, null);
							}
							start=sep;
						}
					
						//compare the two hashtables to find out what's been added or removed
					
											foreach(String fName in oldSerFields.Keys)
											{
												if(fName!="")
												{
													if(!newSerFields.ContainsKey(fName))
													{
														int eqindex=0;
														string type, name;
														eqindex = fName.IndexOf("(");
														type = fName.Substring(0,eqindex);
														name = fName.Substring(eqindex);
														removed+=fName + ",<br>";
														unified.RemovedSerialMember(type + " " + name);
													}
												}
											}
						Console.WriteLine("type:" + otm.FullName);
						foreach(String fName in newSerFields.Keys)
						{
							if(fName!="")
							{
								if(!oldSerFields.ContainsKey(fName))
								{
									//int temp=0;
									int eqindex=0;
									string type, name;
									eqindex = fName.IndexOf("(");
									type = fName.Substring(0,eqindex);
									name = fName.Substring(eqindex);
									added+=fName + ",<br>";
									type = type.Replace("*)", ")");
									type = type.Replace("*",",");
									name = name.Replace("*)", ")");
									name = name.Replace("*",",");

									//we know this method is not marked with [ComVisible(false)]
									//we only add this to the report if the following conditions are met:
									
									if(!ocomvisasm) //the assembly is ComVisible(false) but the type is ComVisible(true)
									{
										if(ocomvistype)
											unified.AddedSerialMember(type + " " + name);
										Console.WriteLine("type:m:" + name);
									}	
										//the assembly is ComVisible(true) and the type is ComVisible(true)
										//if the assembly is ComVisible(true) but the type is not, we don't mar
									else 
									{
										if(ocomvistype)
											unified.AddedSerialMember(type + " " + name);	
										Console.WriteLine("type:m:" + name);
									}


									

								}
							}
						}
				
						extraVal = "The structlayout signature for this type has changed. "; 
				
						extraVal = "<br><font color = red>" + extraVal + "</font>";
						writeVal = extraVal;
					}

				}
			
				
			}


			//robvi start
			//This section was added to detect structure layout breaking changes for the interop team.
			//If instance fields are added, removed, reordered, or have their offsets changed it is breaking.
			if (addStruct) 
			{
			
				string added = "";
				string removed ="";
				string extraVal;
				int istart;
				int iend;

		

				if (otm.Misc.ToLower().IndexOf("structfields") >= 0 || 
					ntm.Misc.ToLower().IndexOf("structfields") >= 0) 
				{

					istart = otm.Misc.ToLower().IndexOf("structfields");
					iend = otm.Misc.ToLower().IndexOf(";", istart + 1);

					if (iend < 0)
						iend = otm.Misc.Length - 1;

					string otmser;
					if(istart<0)
						otmser="";
					else
						otmser = otm.Misc.Substring(istart, iend - istart);
				

					istart = ntm.Misc.ToLower().IndexOf("structfields");
					iend = ntm.Misc.ToLower().IndexOf(";", istart + 1);

					if (iend < 0)
						iend = ntm.Misc.Length - 1;

					string ntmser;
					if(istart<0)
						ntmser="";
					else
						ntmser = ntm.Misc.Substring(istart, iend - istart);
				
				

		
					
					if (ntmser != otmser)
					{
					
						writeToReport = true;
						//robvi added this to write out what's been removed or added:
						Hashtable oldSerFields = new Hashtable();
						Hashtable newSerFields = new Hashtable();
						int start = otmser.IndexOf("=");
						int sep = 0;
						string sub;
					
						while(start < otmser.Length)
						{
							sep = otmser.IndexOf(',',start+1);
							if(sep==-1)//end case
							{
								sub = otmser.Substring(start+1);
								oldSerFields.Add(sub,null);
								break;
							}
							sub = otmser.Substring(start+1,sep-(start+1));
							if(!oldSerFields.ContainsKey(sub))
								oldSerFields.Add(sub, null);
							start=sep;
						}

						sep = 0;
						start = ntmser.IndexOf("=");
						while(start < ntmser.Length)
						{
							sep = ntmser.IndexOf(',',start+1);
							if(sep==-1)//end case
							{
								sub = ntmser.Substring(start+1);
								if(!newSerFields.ContainsKey(sub))
									newSerFields.Add(sub,null);
								break;
							}
							sub = ntmser.Substring(start+1,sep-(start+1));
							if(!newSerFields.ContainsKey(sub))
							{
								newSerFields.Add(sub, null);
							}
							start=sep;
						}
					
						//compare the two hashtables to find out what's been added or removed
					
						foreach(String fName in oldSerFields.Keys)
						{
							if(fName!="")
							{
								if(!newSerFields.ContainsKey(fName))
								{
									int eqindex=0;
									string type, name;
									eqindex = fName.IndexOf("=");
									type = fName.Substring(0,eqindex);
									name = fName.Substring(eqindex+1);
									removed+=fName + ",<br>";
									unified.RemovedSerialMember(type + " " + name);
								}
							}
						}
						Console.WriteLine("type:" + otm.FullName);
						foreach(String fName in newSerFields.Keys)
						{
							if(fName!="")
							{
								if(!oldSerFields.ContainsKey(fName))
								{
									int eqindex=0;
									string type, name;
									eqindex = fName.IndexOf("=");
									type = fName.Substring(0,eqindex);
									name = fName.Substring(eqindex+1);
									added+=fName + ",<br>";
									unified.AddedSerialMember(type + " " + name);
								}
							}
						}
				
						extraVal = "The structlayout signature for this type has changed. "; 
						extraVal += "\nPlease review the fields for this type.";
						extraVal = "<br><font color = red>" + extraVal + "</font>";
						writeVal = extraVal;
					}

				}
			
				
			}
		

			//Changes added for serialization field comparison 
			//Added by Robvi 09/15/2003
			if (addSer) 
			{
			

				string extraVal;
				int istart;
				int iend;

		
				//Get the serfields string, comma delimited ended with a semicolon
				if (otm.Misc.ToLower().IndexOf("serfields") >= 0 && 
					ntm.Misc.ToLower().IndexOf("serfields") >= 0) 
				{

					istart = otm.Misc.ToLower().IndexOf("serfields");
					iend = otm.Misc.ToLower().IndexOf(";", istart + 1);

					if (iend < 0)
						iend = otm.Misc.Length - 1;

					string otmser = otm.Misc.Substring(istart, iend - istart);

					istart = ntm.Misc.ToLower().IndexOf("serfields");
					iend = ntm.Misc.ToLower().IndexOf(";", istart + 1);

					if (iend < 0)
						iend = ntm.Misc.Length - 1;

					string ntmser = ntm.Misc.Substring(istart, iend - istart);
					
					string ntmtemp = ntmser.Replace("=F,", ",");
					ntmtemp = ntmser.Replace("=T,", ",");
					string otmtemp = otmser.Replace("=F,", ",");
					otmtemp = otmser.Replace("=T,", ",");


		
					//Check if the fields match, if anything has been added or removed it is a break
					if (otmtemp != ntmtemp)
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
								unified.RemovedSerialMember(type + " " + name);
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
								unified.AddedSerialMember(type + " " + name);
							}
						}

						//write the change out to the report
						if (breakingadd || breakingremove)
						{
							writeToReport = true;
							extraVal = "The serialization signature for this type has changed. " +
								"Please review the serializable fields of this type.<br>";
				
							extraVal = "<br><font color = red>" + extraVal + "</font>";
							writeVal = extraVal;
						}
					}

				}
				//Get the serialization details of the type
				//serializeable=X,sealed=Y,controlledSer=Z;
				//more string parsing first...

				string oserializeable, osealed, ocontrolledSer;
				string nserializeable, nsealed, ncontrolledSer;
				istart = otm.Misc.ToLower().IndexOf("serializeable=");
				iend = otm.Misc.ToLower().IndexOf(";", istart + 1);

				if (iend < 0)
					iend = otm.Misc.Length - 1;

				string otypeser = otm.Misc.Substring(istart, iend - istart);

				istart = otypeser.IndexOf("serializeable");
				iend = otypeser.IndexOf(",", istart+1);
				oserializeable = otypeser.Substring(istart, iend - istart);
				istart = otypeser.IndexOf("sealed");
				iend = otypeser.IndexOf(",", istart+1);
				osealed = otypeser.Substring(istart, iend - istart);
				istart = otypeser.IndexOf("controlledSer");
				ocontrolledSer = otypeser.Substring(istart);

				istart = ntm.Misc.ToLower().IndexOf("serializeable=");
				iend = ntm.Misc.ToLower().IndexOf(";", istart + 1);

				if (iend < 0)
					iend = ntm.Misc.Length - 1;

				string ntypeser = ntm.Misc.Substring(istart, iend - istart);
				

				istart = ntypeser.IndexOf("serializeable");
				iend = ntypeser.IndexOf(",", istart+1);
				nserializeable = ntypeser.Substring(istart, iend - istart);
				istart = ntypeser.IndexOf("sealed");
				iend = ntypeser.IndexOf(",", istart+1);
				nsealed = ntypeser.Substring(istart, iend - istart);
				istart = ntypeser.IndexOf("controlledSer");
				ncontrolledSer = ntypeser.Substring(istart);



				//This is where the comparison is done 
				//[Serializable] and not ISerializable and not Sealed must stay [Serializable]
 
				//[Serializable] must stay [Serializeable]
				if((oserializeable.CompareTo("serializeable=True")==0)&&(nserializeable.CompareTo("serializeable=False")==0))
				{
					writeToReport = true;
					extraVal = "The serialization signature for this type has changed. " +
						"Serializeable type no long is serializeable.";
					extraVal = "<br><font color = red>" + extraVal + "</font>";
					writeVal = extraVal;
				}
			
				if((oserializeable.CompareTo("serializeable=True")==0)&&(osealed.CompareTo("sealed=False")==0)&&(ocontrolledSer.CompareTo("controlledSer=False")==0))
				{
					if(ncontrolledSer.CompareTo("controlledSer=True")==0)
					{
						writeToReport = true;
						extraVal = "The serialization signature for this type has changed. " +
							"Unsealed Serializeable type no longer serializeable";
						extraVal = "<br><font color = red>" + extraVal + "</font>";
						writeVal = extraVal;
					}

				}

				//[Serializable] and ISerializable Must stay ISerializable
				if((oserializeable.CompareTo("serializeable=True")==0)&&(ocontrolledSer.CompareTo("controlledSer=True")==0))
				{
					if(ncontrolledSer.CompareTo("controlledSer=False")==0)
					{
						writeToReport = true;
						extraVal = "The serialization signature for this type has changed. " +
							"ISerializeable type no longer implements Iserializeable.";
						extraVal = "<br><font color = red>" + extraVal + "</font>";
						writeVal = extraVal;
					}
				}	
			
		

				//[Serializeable] and not ISerializeable must have new fields marked with [OptionallySerializeable] to do Whidbey M2.3
				
			}

			// pick the color based on type change level.
			string color = (diff == CompareResults.Breaking) ? "B0000" : "black";

			//note, ONLY use red if not 'noColor' was set
			if (noColor)
				color = "black";

			TypeMember tm = (ntm != null) ? ntm : otm;		// use the new signature, unless absent.
			string typesig = Colorize(tm.TypeFullName, color);	// "generic" short signature
			string typestring = Colorize(tm.TypeString, color);	// "generic" full signature

			string newtypesig = (ntm != null) ? Colorize(ntm.TypeString, color) : "n/a";	// specific signatures
			string oldtypesig = (otm != null) ? Colorize(otm.TypeString, color) : "n/a";

			//robvi
			if(addSer||addStruct||addStructMethod)
			{
			
				if(writeToReport)
				{
					unified.WriteTypeRow(typestring, tm.TypeFullName, writeVal, null, false);
					unified.WriteSerialMemberRow();
				}
			}
			else
			{
				if (diff == CompareResults.Same) 
				{
					unified.WriteTypeRow(typestring, tm.TypeFullName, writeVal, tm.Owners, showOwners);
				} 
				else 
				{
					unified.WriteTypeRow(typesig, tm.TypeFullName, writeVal, tm.Owners, showOwners);
					unified.WriteTypeSubRow(oldtypesig, newtypesig, alIntfcAdds, noColor);
				}
			}
		}

		// add a font color tag
		private static string Colorize(string str, string color) 
		{
			return String.Format("<font color='{0}'>{1}</font>", color, str);
		}

		static String DetermineIfEnum(String textToCheck) 
		{


			if (textToCheck != null && textToCheck.Length > 60 && 
				(textToCheck.Trim().ToLower().StartsWith("<font color=\"black\">field: public static const") ||
				textToCheck.Trim().ToLower().StartsWith("<font color=\"red\">field: public static const"))) 
			{

				String makeColor = "";

				if (textToCheck.Trim().ToLower().StartsWith("<font color=\"black\">field: public static const"))
					makeColor = "<font color=\"black\">";
				else
					makeColor = "<font color=\"red\">";

				//carve up the parts

				String initialString = textToCheck.Trim().Substring(60);
				String enumSpace = "";
				String enumName = "";
				String enumNameAndSpace = "";
				String enumValue = "";

				for (int i=0;i < initialString.Length;i++) 
				{
					if (initialString.Substring(i, 2) == "\">") 
					{
						enumSpace = initialString.Substring(0, i);
						initialString = initialString.Substring(i + 2);
						break;
					}
				}

				for (int i=0;i < initialString.Length;i++) 
				{
					if (initialString.Substring(i, 7) == "</span>") 
					{
						enumName = initialString.Substring(0, i);
						initialString = initialString.Substring(i + 7);
						break;
					}
				}

				enumNameAndSpace = enumSpace + "." + enumName;

				//now, determine if the the type is an Enum

				try 
				{
					Type t = Type.GetType(enumNameAndSpace);

					if (t.IsEnum) 
					{

						//parse the member for the name
						for (int i=0;i < initialString.Length;i++) 
						{
							if (initialString.Substring(i, 3) == "<b>") 
							{

								enumValue = initialString.Substring(i + 3);

								for (int j=0;j < enumValue.Length;j++) 
								{
									if (initialString.Substring(j, 4) == "</b>") 
									{
										enumValue = enumValue.Substring(0, j - 4);

										//rewrite the output line...
										return makeColor + "Enum Element: " + "<span title=\"" + enumSpace + "\"</span><b>" + enumValue + "</b></font>";

									}
								}

								break;
							}
						}
	
					}

				} 
				catch {}
			}

			return textToCheck;
		}

		private static ArrayList OpenFileList ( String filetoopen ) 
		{

			FileInfo f = new FileInfo( filetoopen );

			StreamReader sr = new StreamReader(f.OpenRead());

			ArrayList alSplitFiles = new ArrayList();

			while (sr.Peek() > -1) 
			{
				alSplitFiles.Add(sr.ReadLine());
			}

			sr.Close();

			return alSplitFiles;
		}

		//ROBVI
		//Reads in the textfile gacload.txt and loads these assemblies from the GAC into a Hashtable
		//the hashtable is used to translate between .dll names and the names stored in the GAC.

		private static Hashtable OpenGACList(String filetoopen)
		{
			FileInfo f = new FileInfo (filetoopen);
			StreamReader sr = new StreamReader(f.OpenRead());
			Hashtable dllmap = new Hashtable();

			while(sr.Peek() > -1)
			{
				String full = sr.ReadLine();
				String key = full.Substring(0,full.IndexOf(":"));
				key = key.ToLower();
				String val = full.Substring(full.IndexOf(":")+1);
				dllmap.Add(key,val);
			}
			return(dllmap);
		}



		// allows personal control of the splitting of dlls
		private static void GetSplitRanges() 
		{

			int found = -1;
			string tempEntry = "";

			StreamReader sr =  new StreamReader("reffiles\\splitRanges.txt");

			htRanges = new Hashtable();

			while (sr.Peek() > -1) 
			{
				string temp = sr.ReadLine().Trim().ToLower();

				found = temp.IndexOf(",");

				if (found < 0) //SHOULD NEVER HAPPEN
					continue; //ignore this item
				else 
				{

					if (temp.Substring(0, found).Trim() == numSplits.ToString()) 
					{
						//organize by the next entry...
						string sub = temp.Substring(found + 1);

						int innerItem = sub.IndexOf(",");

						if (innerItem < 0) //SHOULD NEVER HAPPEN
							continue; //ignore this item
						else 
						{
							string entry = sub.Substring(0, innerItem).Trim().ToLower();

							if (tempEntry != entry) 
							{
								if (splitRanges != null)
									htRanges[tempEntry] = splitRanges;

								splitRanges = new StringCollection();
								tempEntry = entry;
							}

							splitRanges.Add(sub.Substring(innerItem + 1));
						}
					}
				}

			}

			//NOTE: the LAST stringcollection will not ahve been added in all probability
			//even if it has, doing it again won't hurt
			if (splitRanges != null)
				htRanges[tempEntry] = splitRanges;
		}

		//this ASSUMES that a range for "all" always exists
		private static StringCollection GetCorrectSplit(Assembly a) 
		{

			if (htRanges.Contains(Path.GetFileName(a.CodeBase).ToLower()))
				return (StringCollection)htRanges[Path.GetFileName(a.CodeBase).ToLower()];
			else
				return (StringCollection)htRanges["all"];
		}

		private static void GetIntfcAdds() 
		{

			alIntfcAdds = new ArrayList();

			StreamReader sr = new StreamReader("reffiles\\breakIntfcAdds.txt");

			while (sr.Peek() > -1) 
			{
				alIntfcAdds.Add(sr.ReadLine().Trim());
			}
		}

		private static bool DetSplit(String searchName) 
		{

			if (byDll) 
			{
				int i = searchName.ToLower().IndexOf(".binary.store");

				if (i >= 0) 
				{
					string temp = searchName.Substring(0,i);

					foreach(String s in alSplitF) 
					{
						//have to ensure that this is indeed a split by checking
						//the input file for a numeric entry, where expected...
						if (temp.Length >= 3) 
						{
							if (temp.Substring(0,temp.Length - 3) == 
								Path.GetFileNameWithoutExtension(s)) 
							{

								string sPart = temp.Substring( temp.Length - 2, 2);

								if (Char.IsDigit(sPart, 0) && Char.IsDigit(sPart, 1)) 
								{
									fileFound = temp.Substring(0,temp.Length-3);
									splitFound = Convert.ToInt32(sPart);
									return true;
								}
							}
						}
					}
				} 
				else 
				{
					return false;
				}
			}
			else 
			{
				foreach(String s in alSplitNamespaces) 
				{
					if (searchName.ToLower().IndexOf(s.ToLower()) >= 0) 
					{
						//have to ensure that this is indeed a split by checking
						//the input file for a numeric entry, where expected...
						string sPart = searchName.Substring(
							searchName.ToLower().IndexOf(s.ToLower()) +
							s.Length + 1, 2);

						if (Char.IsDigit(sPart, 0) && Char.IsDigit(sPart, 1)) 
						{
							fileFound = s;
							splitFound = Convert.ToInt32(sPart);
							return true;
						}
					}
				}
			}

			return false;		
		}

		public static ArrayList LoadNameSpaces(String spaceName) 
		{

			ArrayList spaces = new ArrayList();

			foreach (string f in Directory.GetFiles("splits","*.split.txt")) 
			{
				string temp = Path.GetFileName(f);

				if (temp.Substring(0,temp.ToLower().IndexOf(".split.txt")).ToLower() == 
					spaceName.ToLower()) 
				{

					StreamReader sr = new StreamReader(f);

					while (sr.Peek() > -1)
						spaces.Add(sr.ReadLine().Trim());

					return spaces;
				}
			}

			return spaces;	
		}

		private static void StoreToFile(Hashtable ht, string file, string section, string buildnum) 
		{

			FileStream fs;

			try 
			{
				fs = new FileStream(file, FileMode.Create);
			} 
			catch 
			{
				fs = new FileStream(file, FileMode.Append, FileAccess.Write);
			}

			StreamWriter sw = new StreamWriter(fs);

			try 
			{
				foreach(string typememberID in ht.Keys) 
				{

					string tmid = typememberID;
					bool written = false;

					foreach (TypeMember tm in (ArrayList)ht[typememberID]) 
					{
						if (written == false) 
						{
							sw.WriteLine("nt");
							sw.WriteLine("version=" + tm.Version.ToString());
							sw.WriteLine("tmid="+typememberID.Trim());
							sw.WriteLine("tmisinh="+tm.IsInherited);
							sw.WriteLine("tmtypnam="+tm.TypeName);
							sw.WriteLine("tmtypkey="+tm.TypeKey);
							sw.WriteLine("tmtypstr="+tm.TypeString);
							sw.WriteLine("tmtypshtkey="+tm.TypeShortKey);
							sw.WriteLine("tmtypknd="+tm.TypeKind);
							sw.WriteLine("tmisabstyp="+tm.IsAbstractType);
						

							//P12 Added
							sw.WriteLine("tmisenum="+tm.IsEnum);

							//			    if (tm.IsEnum) {
							//				sw.WriteLine("tmmisc=" + ((tm.Misc == null) ? "" : tm.Misc));
							//				sw.Flush();
							//			    }
							//Console.WriteLine("P7");

							//P11 Added
							//			    sw.WriteLine("tmisenum="+tm.IsEnum);
							written = true;
						}

						//P12 added
						if (tm.IsEnum) 
						{
							sw.WriteLine("tmmisc=" + ((tm.Misc == null) ? "" : tm.Misc));
							sw.Flush();
						} 
						else 
						{

							//P11 added this entire if
							//			if (tm.Misc == null || 
							//					(tm.Misc != null && tm.Misc.IndexOf("ignorethiselement") < 0)) {


							sw.WriteLine("nm");
							sw.WriteLine("tmmemnam="+tm.MemberName);	
							sw.WriteLine("tmmemkey="+tm.MemberKey);	
							sw.WriteLine("tmmemshtsig="+tm.MemberShortSig);
							sw.WriteLine("tmmemstr="+tm.MemberString);
							sw.WriteLine("tmmemknd="+tm.MemberKind);
							sw.WriteLine("tmisabsmem="+tm.IsAbstractMember);
							sw.WriteLine("tmmisc=" + ((tm.Misc == null) ? "" : tm.Misc));
							sw.WriteLine("tmnamspc="+tm.Namespace);
							sw.Flush();

						}
					}
				}
			}

			catch (Exception e) 
			{
				Console.WriteLine(e.ToString());
			}

			fs.Close();
		}

		private static Hashtable GetFromFile (string file, bool newVers) 
		{

			Hashtable htTemp = new Hashtable();
			StreamReader sr = null;

			try 
			{	
				sr = File.OpenText(file);

				string input = "";
				string outid = "";
				ArrayList al = new ArrayList();
				bool tmisinherit = false;
				string tmtypename = "";
				string tmtypekey = "";
				string tmtypestring = "";
				string tmtypeshortkey = "";
				string tmVersion = "";
				TypeKinds tmtypekind = 0;
				bool tmisabstracttype = false;
				bool tmisenum = false;
				string tmmisc = "";
				//		string tmname = "";
				//		string tmsig = "";

				if (newVers)
					newVersion = "";
				else
					oldVersion = "";

				//load each type in turn...
				while (sr.Peek() > -1) 
				{
					if (input == "")
						input = sr.ReadLine();

					/*
								//if the first line begins with 'Environment =', process it!
								if (input.StartsWith("Environment =")) {
									if (newVers)
										newVersion = input.Substring("Environment =".Length).Trim());
									else
										oldVersion = input.Substring("Environment =".Length).Trim());

									input = sr.ReadLine();
								}
					*/

					string tmid = "";

					if (input.StartsWith("nt")) 
					{

						//load the type info...
						input = sr.ReadLine();
						//if line is included for compatibility...
						if (input.StartsWith("version=")) 
						{
							tmVersion = ProcessEntry(ref sr, "version=", "tmid=", "", ref input);
						}

						if (newVers)
							newVersion = tmVersion;
						else
							oldVersion = tmVersion;

						tmid = ProcessEntry(ref sr, "tmid=", "tmisinh=", "", ref input);

						tmisinherit = Convert.ToBoolean(
							ProcessEntry(ref sr, "tmisinh=", "tmtypnam=", "", ref input));

						tmtypename = ProcessEntry(ref sr, "tmtypnam=", "tmtypkey=", "", ref input);
						tmtypekey = ProcessEntry(ref sr, "tmtypkey=", "tmtypstr=", "", ref input);
						tmtypestring = ProcessEntry(ref sr, "tmtypstr=", "tmtypshtkey=", "", ref input);
						tmtypeshortkey = ProcessEntry(ref sr, "tmtypshtkey=", "tmtypknd=", "", ref input);
						tmtypekind = (TypeKinds)Enum.Parse(typeof(TypeKinds),
							ProcessEntry(ref sr, "tmtypknd=", "tmisabstyp=", "", ref input));
						//			    tmisabstracttype = Convert.ToBoolean(
						//				ProcessEntry(ref sr, "tmisabstyp=", "nm", "", ref input));
						//P12 Added and Changed
						tmisabstracttype = Convert.ToBoolean(
							ProcessEntry(ref sr, "tmisabstyp=", "tmisenum", "", ref input));

						tmisenum = Convert.ToBoolean(
							ProcessEntry(ref sr, "tmisenum=", "nm", "tmmisc=", ref input));

						if (tmisenum) 
						{
							tmmisc = ProcessEntry(ref sr, "tmmisc=", "nt", "", ref input);
						}

						if (outid != tmid) 
						{
							if (outid != "") 
							{
								htTemp.Add(outid,al);
								al = new ArrayList();
							}

							outid = tmid;
						}
					}

					//P12 


					if (tmisenum) 
					{
						TypeMember tm = new TypeMember();
						//				input = sr.ReadLine();
						//				if (tmVersion.Trim() != "")
						//					tm.Version = new Version(tmVersion);
						tm.IsInherited = tmisinherit;
						tm.TypeName = tmtypename;
						tm.TypeKey = tmtypekey;
						tm.TypeString = tmtypestring;
						tm.TypeShortKey = tmtypeshortkey;
						tm.TypeKind = tmtypekind;
						tm.IsAbstractType = tmisabstracttype;
						tm.Misc = tmmisc;
						//				tm.MemberName = tmname;
						//				tm.MemberShortSig = tmsig;
						tm.IsEnum = tmisenum;
						//				tm.MemberName = tmtypename;
						//				tm.MemberKey = tmtypekey;
						//				tm.MemberShortKey = tmtypeshortkey;

						al.Add(tm);
						//				input = sr.ReadLine();

						// P12 stop of add
					} 
					else if (input.StartsWith("nm")) 
					{
						TypeMember tm = new TypeMember();

						input = sr.ReadLine();
						if (tmVersion.Trim() != "")
							tm.Version = new Version(tmVersion);
						tm.IsInherited = tmisinherit;
						tm.TypeName = tmtypename;
						tm.TypeKey = tmtypekey;
						tm.TypeString = tmtypestring;
						tm.TypeShortKey = tmtypeshortkey;
						tm.TypeKind = tmtypekind;
						tm.IsAbstractType = tmisabstracttype;
						tm.IsEnum = tmisenum;

						tm.MemberName = ProcessEntry(ref sr, "tmmemnam=", "tmmemkey=", "", ref input);
						tm.MemberKey = ProcessEntry(ref sr, "tmmemkey=", "tmmemshtsig=", "", ref input);
						tm.MemberShortSig = ProcessEntry(ref sr, "tmmemshtsig=", "tmmemstr=", "", ref input);
						tm.MemberString = ProcessEntry(ref sr, "tmmemstr=", "tmmemknd=", "", ref input);
						tm.MemberKind = (MemberTypes)Enum.Parse(typeof(MemberTypes),
							ProcessEntry(ref sr, "tmmemknd=", "tmisabsmem=", "", ref input));
						tm.IsAbstractMember = Convert.ToBoolean(
							ProcessEntry(ref sr, "tmisabsmem=", "tmmisc=", "", ref input));
						tm.Misc = ProcessEntry(ref sr, "tmmisc=", "tmnamspc=", "", ref input);
						tm.Namespace = ProcessEntry(ref sr, "tmnamspc=", "nm", "", ref input);

						al.Add(tm);
					}
				}

				//just to ensure this one was added! may be duplicate
				try 
				{
					htTemp.Add(outid,al);
				} 
				catch (Exception) {}

				//something's going wrong here!
				//foreach (ArrayList a in htTemp.Values)
				//Console.WriteLine(((TypeMember)(a[0])).TypeName);
				//Console.WriteLine(htTemp.Count);
				sr.Close();
				return htTemp;
			} 
			catch (Exception e) 
			{
				Console.WriteLine(e.Message);
				try 
				{
					if (sr != null)
						sr.Close();
				} 
				catch (Exception) {}

				return htTemp;
			}
		}

		private static string ProcessEntry(ref StreamReader sr, string thisItem, string nextItem,
			string nextItem2,
			ref string initial) 
		{

			string output = "";
			string input = initial;

			if (input.StartsWith(thisItem)) 
			{
				output = input.Substring(thisItem.Length);

				while (true) 
				{

					input = sr.ReadLine();

					if (input == null || input.StartsWith(nextItem) 
						|| (nextItem2 != "" && input.StartsWith(nextItem2))
						|| (thisItem == "tmnamspc=" && (input == "" || input == "nt"))) 
					{

						initial = input;
						break;
					}
					else
						output += Environment.NewLine + input;
				}
			}

			return output;
		}

		private static void MakeComCompat() 
		{
			// we need a summary page!

			bool errorsExisted = false;
			if (outputLoc == "") 
			{
				outputLoc = _oldBuild + "to" + _newBuild + "/";
			}

			StreamWriter comCompatErrors = new StreamWriter(outputLoc + "ComCompatErrors.txt");

			StreamWriter sw = new StreamWriter(outputLoc +
				"\\ComCompat.htm" + (useHTM ? "" : "l"));
			bool headerWritten = false;

			foreach ( string nextDll in comDlls ) 
			{

				ArrayList errors = null;
				ArrayList warnings = null;
				ArrayList otherDiffs = null;

				try 
				{
		
					int success = ComCompat.RunComCompat(
						_oldBuild + Path.DirectorySeparatorChar + nextDll,
						_newBuild + Path.DirectorySeparatorChar + nextDll,
						out errors, out warnings, out otherDiffs);

					string errString = "";
					string warnString = "";
					string otherString = "";
					string totString = "";

					// make a new html file...
					if (errors != null || warnings != null || otherDiffs != null) 
					{
						if (headerWritten) 
						{
							totString += "<hr>";
						}
						totString += "<b><h2>Compatibility Results for " + nextDll + "</h2></b>";
						if (errors != null) 
						{
							foreach (string s in errors)
								errString += Environment.NewLine + "<tr><td>" + s + "</td></tr>";

							if (errString.Length > 0) 
							{
								totString += Environment.NewLine + 
									"<table border=1 width=800><tr><td bgcolor=\"#FF0000\"><b>Errors</b></td></tr>" +
									errString + "</table><p>";
							}
						}

						if (warnings != null) 
						{
							foreach (string s in warnings)
								warnString += Environment.NewLine + "<tr><td>" + s + "</td></tr>";

							if (warnString.Length > 0) 
							{
								totString += Environment.NewLine + 
									"<table border=1 width=800><tr><td bgcolor=\"#FFFF00\"><b>Warnings</b></td></tr>" +
									warnString + "</table><p>";
							}
						}

						if (otherDiffs != null) 
						{
							foreach (string s in otherDiffs)
								otherString += Environment.NewLine + "<tr><td>" + s + "</td></tr>";

							if (otherString.Length > 0) 
							{
								totString += Environment.NewLine + 
									"<table border=1 width=800><tr><td bgcolor=\"#55FF55\"><b>Other Differences</b></td></tr>" +
									otherString + "</table><p>";
							}
						}

						totString = "<center>" + totString + "</center>";

						if (!headerWritten) 
						{
							sw.WriteLine("<html><title>Com Compatibility Results</title><body>");
							sw.WriteLine("<center><h1>Com Compatibility Results</h1></center>");
							sw.WriteLine("<hr>");
							sw.WriteLine("<p>");
							sw.WriteLine("The following issues are a summary of the Com Compatability results for this comparison.");
							sw.WriteLine(" If there are no items present, then no issues were found. Issues may be in one of three ");
							sw.WriteLine("categories:");
							sw.WriteLine("<br>");
							sw.WriteLine("<center><table border = 0 width=800><tr><td>");
							sw.WriteLine("<ul>");
							sw.WriteLine("<li><b>Errors:</b> These are actual differences which will lead to Com Incompatibility between your assemblies");
							sw.WriteLine("<li><b>Warnings:</b> These are differences which do not cause Com incompatibilty, but are major differences");
							sw.WriteLine("<li><b>Other Diffs:</b> These differences may be entirely intentional, and are often a reflection of reasonable changes. They are included here for reference purposes");
							sw.WriteLine("</ul>");
							sw.WriteLine("</td></tr></table></center>");
							sw.WriteLine("<hr><p>");
							headerWritten = true;
						}

						sw.WriteLine(totString);

						//			groupReportComCompat.Visible = true;
						//			linkReportComCompat.Text = "Report for Com Compatibility Results";
						//			linkReportComCompat.Links.Clear();
						//			linkReportComCompat.Links.Add( 0,linkReportComCompat.Text.Length,
						//				outputLoc + "\\ComCompat.htm" + 
						//				(checkUseHTML.Checked ? "l" : ""));
						//			if ((linkReportComCompat.Links[0]).LinkData.ToString().Trim() != "")
						//			{
						//				groupReportComCompat.Visible = true;
						//				groupNoResults.Visible = false;
						//			}
					}
				} // the try
				catch (Exception e) 
				{
					comCompatErrors.WriteLine("\r\nImport failed for Assembly {0}:, \r\n{1}\r\n",
						nextDll, e.ToString());
					errorsExisted = true;
				}
			} //END OF THE LOOP

			if (!headerWritten) 
			{
				sw.WriteLine("<html><title>Com Compatibility Results</title><body>");
				sw.WriteLine("<center><h1>Com Compatibility Results</h1></center>");
				sw.WriteLine("<hr>");
				sw.WriteLine("<p>");
				sw.WriteLine("There were no COM Compatiblity results to report.");
			}

			sw.WriteLine("<hr></body></html>");
			sw.Close();

			if (errorsExisted) 
			{
				Console.WriteLine("\r\nErrors occurred in attempting to perform a Com Compatibility comparison");
				Console.WriteLine("of some assemblies. Please see 'ComCompatErrors.txt' for details\r\n");
			} 
			else 
			{
				Console.WriteLine("\r\nCom Reports Generated successfully\r\n");		
			}
			comCompatErrors.Close();
		}


		//added for ambiguous problem - we want to check if an existing overloaded method exists
		//already for the member
		private static bool IsAmbiguous(TypeMember member, ArrayList newlist)
		{
			bool isConflict = false;
			if(member.MemberKind.Equals(System.Reflection.MemberTypes.Method))
			{
				foreach(TypeMember existingmember in newlist)
				{
					if(member.Name == existingmember.Name)
					{
						ArrayList oldparams, newparams;
						oldparams = getParams(existingmember);
						newparams = getParams(member);
						if (oldparams.Count == newparams.Count)
						{
							for (int i=0; i < newparams.Count-1; i++)
							{
							
								if(!isRefType(newparams[i].ToString())) 
								{
									//we have a value type
									if(!isRefType(oldparams[i].ToString()))
									{
										if(newparams[i].ToString()==oldparams[i].ToString())
										{
											//continue
										}
										else
										{
											isConflict = false;
										}
									}
									else
									{
										isConflict = false;
									}
								}
								else
								{
									//we have reference type
									if(newparams[i].ToString()==oldparams[i].ToString())
									{
										//do nothing
									}
									else
									{
										if(isRefType(oldparams[i].ToString()))
										{
											if (oldparams[i].ToString() == newparams[i].ToString())
											{
												isConflict = false;
											}
											else
											{
												isConflict = true;
												Console.WriteLine(member.TypeFullName + " " + member.Name + " " + member.Misc);
												Console.WriteLine("		newparams:" + newparams[i].ToString());
												Console.WriteLine("		oldparams:" + oldparams[i].ToString());
											}
										}
										else
										{
											isConflict = false;
										}
									}								
								}
							}					
						}
					}
				}
			}
			return (isConflict);
		}

		private static bool isRefType(String paramname)
		{
			string assemblyname, typefullname;
			
			if (paramname.IndexOf(".")>-1)
			{
				//Some of the assembly names have multiple "."  e.g. System.Windows.Forms
				//we have to see if there is repitition since the assembly names and type names
				//are only "." delimited
				string temp;
				temp = paramname.Substring(0, paramname.IndexOf("."));
				
					
				if (paramname.IndexOf(temp,temp.Length)>-1)
				{
					assemblyname = paramname.Substring(0,paramname.IndexOf(temp,temp.Length)-1);
					typefullname = paramname.Substring(paramname.IndexOf(temp,temp.Length));
				}
				else
				{
					assemblyname = (paramname.Substring(0, paramname.IndexOf(".")));
					typefullname = (paramname.Substring(paramname.IndexOf(".")+1));
				}
				
				if(typefullname.IndexOf("&")>-1) //remove "out" and extra modifiers
				{
					typefullname = typefullname.Substring(0, typefullname.IndexOf("&"));
				}
				if(typefullname.IndexOf("[")>-1) //remove "[]"
				{
					typefullname = typefullname.Substring(0,typefullname.IndexOf("["));
				}
				if(typefullname.IndexOf(" ") >-1) //remove space
				{
					typefullname = typefullname.Substring(0,typefullname.IndexOf(" "));
				}
				Assembly a;
				
				if(GACload && htGACdlls.ContainsKey(assemblyname.ToLower()))
						a = Assembly.Load(htGACdlls[assemblyname.ToLower()].ToString());
				else
						a = Assembly.LoadFrom(_codebase + "\\" + assemblyname + ".dll");
				Type t = a.GetType(typefullname);

				if (t.IsValueType)
					return (false);
				else
					return (true);
			}
			else
			{
				Console.WriteLine("Error loading assembly");
				return(false);
			
			}
		}

		//just a helper to get the args in a nice array
		private static ArrayList getParams(TypeMember member)
		{
			if(member.MemberShortKey.IndexOf("Parameters=") >-1)
			{
				ArrayList parameters = new ArrayList();
				String paramstoparse = member.MemberShortKey.Substring(member.MemberShortKey.IndexOf("Parameters=")+11);
				if (paramstoparse.IndexOf(";")>-1)
				{
					paramstoparse = paramstoparse.Substring(0, paramstoparse.IndexOf(";"));
				}
				while (paramstoparse.Length>0)
				{
					if(paramstoparse.IndexOf(",") > -1)
					{
						String remaininglist = paramstoparse.Substring(paramstoparse.IndexOf(",")+1);
						parameters.Add(paramstoparse.Substring(0, paramstoparse.IndexOf(",")));
						paramstoparse = remaininglist;
					}
					else
					{
						parameters.Add(paramstoparse);
						paramstoparse="";
					}
				}
				return(parameters);
			}
			return(null);
		}
	


	private static void MakeComList() {
		string[] sOrigin = Directory.GetFiles(_oldBuild, "*.dll");
		string[] sChange = Directory.GetFiles(_newBuild, "*.dll");

		bool found = false;
		comDlls = new ArrayList();

		//ONLY allow compat comparisons between dlls in both locations
		for (int i=0;i<sOrigin.Length;i++) {
			for (int j=0;j<sChange.Length;j++) {
				if (Path.GetFileName(sOrigin[i]).Equals(Path.GetFileName(sChange[j]))) {
					found = true;
					break;
				}
			}

			if (found) {
				comDlls.Add(Path.GetFileName(sOrigin[i]));
			}

			found = false;
		}
	}

    } // end LibChk
}
