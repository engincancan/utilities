/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  Form1
 *
 * Description: CLR Profiler interface and logic
 */
#define USESAFEHANDLES
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Xml;
#if(USESAFEHANDLES)
using Microsoft.Win32.SafeHandles;
#endif

namespace CLRProfiler
{

    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class MainForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem exitMenuItem;
        private System.ComponentModel.IContainer components;

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem fontMenuItem;
        private System.Windows.Forms.MenuItem logFileOpenMenuItem;
        private System.Windows.Forms.MenuItem profileApplicationMenuItem;
        private System.Windows.Forms.Button startApplicationButton;
        private System.Windows.Forms.CheckBox profilingActiveCheckBox;
        private System.Windows.Forms.Button killApplicationButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer checkProcessTimer;
        private System.Windows.Forms.MenuItem setCommandLineMenuItem;
        private System.Windows.Forms.FontDialog fontDialog;

        internal Font font;
        private Process profiledProcess;
        private string processFileName;
        private string serviceName;
        private string serviceAccountSid;
        private string serviceStartCommand;
        private string serviceStopCommand;
        private string logFileName;
        private long logFileStartOffset;
        private long logFileEndOffset;
        internal ReadNewLog log;
        internal ReadLogResult lastLogResult;
        private NamedManualResetEvent loggingActiveEvent;
        private NamedManualResetEvent forceGcEvent;
        private NamedManualResetEvent loggingActiveCompletedEvent;
        private NamedManualResetEvent forceGcCompletedEvent;
        private NamedManualResetEvent callGraphActiveEvent;
        private NamedManualResetEvent callGraphActiveCompletedEvent;
        private System.Windows.Forms.Button showHeapButton;
        private string commandLine = "";
        private string workingDirectory = "";
        private string logDirectory;
#if(USESAFEHANDLES)
        private SafeFileHandle pipeHandle;
#else
		private IntPtr pipeHandle;
#endif
        private System.Windows.Forms.MenuItem menuItem8;
        private System.Windows.Forms.MenuItem viewTimeLineMenuItem;
        private FileStream pipe;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        internal static MainForm instance;
        private System.Windows.Forms.MenuItem saveAsMenuItem;
        private System.Windows.Forms.MenuItem viewHistogramAllocatedMenuItem;
        private System.Windows.Forms.MenuItem viewHistogramRelocatedMenuItem;
        private System.Windows.Forms.MenuItem viewHistogramFinalizerMenuItem;
        private System.Windows.Forms.MenuItem viewHistogramCriticalFinalizerMenuItem;
        private System.Windows.Forms.MenuItem viewHeapGraphMenuItem;
        private System.Windows.Forms.MenuItem viewCallGraphMenuItem;
        private System.Windows.Forms.MenuItem viewAllocationGraphMenuItem;
        private System.Windows.Forms.MenuItem viewObjectsByAddressMenuItem;
        private System.Windows.Forms.MenuItem viewHistogramByAgeMenuItem;
        private System.Windows.Forms.MenuItem profileASP_NETmenuItem;
        private System.Windows.Forms.MenuItem profileServiceMenuItem;
        private System.Windows.Forms.MenuItem viewFunctionGraphMenuItem;
        private System.Windows.Forms.MenuItem viewModuleGraphMenuItem;
        private System.Windows.Forms.MenuItem viewClassGraphMenuItem;
        private System.Windows.Forms.MenuItem viewCommentsMenuItem;
        private System.Windows.Forms.CheckBox allocationsCheckBox;
        private System.Windows.Forms.CheckBox callsCheckBox;
        private System.Windows.Forms.MenuItem viewCallTreeMenuItem;
        private System.Windows.Forms.MenuItem viewAssemblyGraphMenuItem;
        private bool saveNever;
        private bool gcOnLogFileComments;

        private bool noUI;
        private string nameToUse;
        private bool profileAllocations, profileCalls, profilingActive;

		private bool help;
		internal string prevlogFileName;
		internal string currlogFileName;
		internal string difflogFileName;
		internal Graph.GraphType graphtype = Graph.GraphType.Invalid;
		internal bool runaswindow = false;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem viewComparisonMenuItem;
		internal bool viewdiff = false;

        private enum ReportKind
        {
            NoReport,
            AllocationReport,
            RelocationReport,
            SurvivorReport,
            SurvivorDifferenceReport,
            HeapDumpReport,
            FinalizerReport,
            CriticalFinalizerReport,
            CommentReport,
        };

        private ReportKind reportKind;
        private string startMarker = null;
        private string endMarker = null;
        private System.Windows.Forms.MenuItem viewSummaryMenuItem;
        private string[] timeMarker = new string[0];

        internal void CommandLineError(string message)
        {
            if  (runaswindow)
            {
                MessageBox.Show(message, "Report Profiler");
                ShowUsage();
            }
            else 
            {
                Console.WriteLine(message);
            }
        }

        internal void CommandLineError(string message, params object[] args)
        {
            CommandLineError(string.Format(message, args));
        }

        internal string FetchArgument(string[] arguments, ref int i)
        {
            i++;
            if(arguments.Length > i)
                return arguments[i];
            else
            {
                CommandLineError("Option {0} expects an argument", arguments[i-1]);
                return "";
            }
        }

        public MainForm(string[] arguments)
        {
            int i;
            string logFileName = "";
            bool onlyCreateLog = false;
            processFileName = null;

            profileAllocations = profileCalls = profilingActive = true;

            for(i = 0; i < arguments.Length && !onlyCreateLog; i++)
            {
                switch (arguments[i])
                {
                    case    "-na":
                    case    "/na":
                        profileAllocations = false;
                        break;

                    case    "-nc":
                    case    "/nc":
                        profileCalls = false;
                        break;

                    case    "-np":
                    case    "/np":
                        profilingActive = false;
                        break;

                    case    "-o":
                    case    "/o":
                        logFileName = FetchArgument(arguments, ref i);
                        break;

                    case    "-p":
                    case    "/p":
                        processFileName = FetchArgument(arguments, ref i);
                        if (processFileName != null)
                            onlyCreateLog = true;
                        break;

                    case    "-diff":
                    case    "/diff":
                        graphtype = Graph.GraphType.AllocationGraph;
                        break;

                    case    "-lo":
                    case    "/lo":
                        prevlogFileName = FetchArgument(arguments, ref i);
                        break;

                    case    "-ln":
                    case    "/ln":
                        currlogFileName = FetchArgument(arguments, ref i);
                        break;

                    case    "-ld":
                    case    "/ld":
                        difflogFileName = FetchArgument(arguments, ref i);
                        break;

                    case    "-l":
                    case    "/l":
                        logFileName = FetchArgument(arguments, ref i);
                        break;

                    case    "-a":
                    case    "/a":
                        reportKind = ReportKind.AllocationReport;
                        break;

                    case    "-r":
                    case    "/r":
                        reportKind = ReportKind.RelocationReport;
                        break;

                    case    "-s":
                    case    "/s":
                        reportKind = ReportKind.SurvivorReport;
                        break;

                    case    "-sd":
                    case    "/sd":
                        reportKind = ReportKind.SurvivorDifferenceReport;
                        break;

                    case    "-h":
                    case    "/h":
                        reportKind = ReportKind.HeapDumpReport;
                        break;

                    case    "-f":
                    case    "/f":
                        reportKind = ReportKind.FinalizerReport;
                        break;

                    case    "-cf":
                    case    "/cf":
                        reportKind = ReportKind.CriticalFinalizerReport;
                        break;

                    case    "-c":
                    case    "/c":
                        reportKind = ReportKind.CommentReport;
                        break;

                    case    "-b":
                    case    "/b":
                        startMarker = FetchArgument(arguments, ref i);
                        break;

                    case    "-e":
                    case    "/e":
                        endMarker = FetchArgument(arguments, ref i);
                        break;

                    case    "-t":
                    case    "/t":
                        do
                        {
                            string[] newTimeMarker = new String[timeMarker.Length+1];
                            for (int j = 0; j < timeMarker.Length; j++)
                                newTimeMarker[j] = timeMarker[j];
                            newTimeMarker[timeMarker.Length] = FetchArgument(arguments, ref i);
                            timeMarker = newTimeMarker;
                        }
                        while (i+1 < arguments.Length && arguments[i+1][0] != '-' && arguments[i+1][0] != '/');
                        break;

                    case    "-w":
                    case    "/w":
                        this.runaswindow = true;
                        break;

                    case    "-?":
                    case    "/?":
                    case    "-help":
                    case    "/help":
                        help = true;
                        break;

                    case    "-gc":
                    case    "/gc":
                        gcOnLogFileComments = true;
                        break;

                    default:
                        if (arguments[i][0] == '-' || arguments[i][0] == '/')
                            CommandLineError("Unrecognized option {0}", arguments[i]);
                        else if (File.Exists(arguments[i]))
                            logFileName = arguments[i];
                        else
                            CommandLineError("Log file {0} not found", arguments[i]);
                        break;
                }
            }

            if (graphtype == Graph.GraphType.AllocationGraph)
            {
                if (File.Exists(prevlogFileName) && File.Exists(currlogFileName))
                {
                    viewdiff = true;
                    return;
                }
                else
                {
                    String s = "";
                    if (!File.Exists(prevlogFileName))
                    {
                        s += String.Format("Previous build log File not exists or command line missing -lo. \n");
                    }
                    if (!File.Exists(currlogFileName))
                    {
                        s += String.Format("New build log file not exists or command line missing -ln \n");
                    }
                    CommandLineError(s);
                }
            }
            else if (reportKind != ReportKind.NoReport)
            {
                if (logFileName == "")
                    CommandLineError("Need -l logFileName for report");
                else if (!File.Exists(logFileName))
                    CommandLineError("Log file {0} not found", logFileName);
                else
                {
                    switch (reportKind)
                    {
                        case    ReportKind.AllocationReport:
                            Reports.AllocationReport(logFileName, startMarker, endMarker);
                            break;

                        case    ReportKind.RelocationReport:
                            Reports.RelocationReport(logFileName, startMarker, endMarker);
                            break;

                        case    ReportKind.SurvivorReport:
                            Reports.SurvivorReport(logFileName, startMarker, endMarker, timeMarker);
                            break;

                        case    ReportKind.SurvivorDifferenceReport:
                            Reports.SurvivorDifferenceReport(logFileName, startMarker, endMarker);
                            break;

                        case    ReportKind.HeapDumpReport:
                            Reports.HeapDumpReport(logFileName, startMarker, endMarker);
                            break;

                        case    ReportKind.FinalizerReport:
                            Reports.FinalizerReport(false, logFileName, startMarker, endMarker);
                            break;

                        case    ReportKind.CriticalFinalizerReport:
                            Reports.FinalizerReport(true, logFileName, startMarker, endMarker);
                            break;

                        case    ReportKind.CommentReport:
                            Reports.CommentReport(logFileName);
                            break;
                    }
                }
            }
            else
            {
                if (onlyCreateLog)
                {
                    /* treat everything after the exe name to profile as arguments */
                    for(; i < arguments.Length; i++)
                    {
                        commandLine += arguments[i] + ' ';
                    }
                }

                /* basic initialization stuff */
                CreatePipe(false);
                instance = this;

                if (!onlyCreateLog)
                {
                    /* standard UI operation */
                    if (help)
                    {
                        ShowUsage();
                    }
                    noUI = false;
                    //
                    // Required for Windows Form Designer support
                    //
                    InitializeComponent();

                    font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));;

                    if (logFileName != "" && File.Exists(logFileName))
                        LoadLogFile(logFileName);

                    EnableDisableViewMenuItems();
                }
                else
                {
                    /* command-line only */

                    noUI = true;
                    try
                    {
                        string fileName = (logFileName == "" ? Path.ChangeExtension(processFileName, ".log") : logFileName);
                        string fullPath = Path.GetFullPath(fileName);
                        logDirectory = Path.GetDirectoryName(fullPath);
                        nameToUse = Path.GetFileName(fileName);
                        startApplicationButton_Click(null, null);
                        while (profiledProcess != null && !ProfiledProcessHasExited())
                            Thread.Sleep(100);
                    }
                    catch(Exception e)
                    {
                        throw new Exception(e.Message + '\n' + e.StackTrace);
                        // Console.WriteLine("There was a problem profiling {0}", processFileName);
                    }
                }
            }
    	}

        private void ShowUsage()
	    {
		    graphtype = Graph.GraphType.Invalid;
		    String s = "";
		    s += String.Format("Usage: {0} [-o logName] [-na] [-nc] -[np] [-p exeName [args]]\n", Path.GetFileName(Application.ExecutablePath));
		    s += "  -p exeName args\t- application to profile and its arguments (must go at the end)\n";
		    s += "  -o logName\t- file to write the resulting profile log to\n";
            s += "  -na\t\t- don't record allocations\n";
            s += "  -nc\t\t- don't record calls\n";
            s += "  -np\t\t- start with profiling active off\n";
            s += "or\n";
            s += String.Format("{0} -a -l logName [-b <start marker>] [-e <end marker>]\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -a               \t - asks for an allocation report\n";
            s += "  -l logName       \t - names the input log file\n";
            s += "  -b <start marker>\t - only report allocations after this log file comment\n";
            s += "  -e <end marker>  \t - only report allocations before this log file comment\n";
            s += "or\n";
            s += String.Format("{0} -r -l logName [-b <start marker>] [-e <end marker>]\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -r               \t - asks for a relocation report\n";
            s += "  -l logName       \t - names the input log file\n";
            s += "  -b <start marker>\t - only report relocations after this log file comment\n";
            s += "  -e <end marker>  \t - only report relocations before this log file comment\n";
            s += "or\n";
            s += String.Format("{0} -s -l logName [-b <start marker>] [-e <end marker>] [-t <time marker]\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -s               \t - asks for a surviving objects report\n";
            s += "  -l logName       \t - names the input log file\n";
            s += "  -b <start marker>\t - only report objects allocated after this log file comment\n";
            s += "  -e <end marker>  \t - only report objects allocated before this log file comment\n";
            s += "  -t <time marker> \t - report survivors at this log file comment\n";
            s += "or\n";
            s += String.Format("{0} -[c]f -l logName [-b <start marker>] [-e <end marker>]\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -[c]f            \t - asks for a [critical] finalizer report\n";
            s += "  -l logName       \t - names the input log file\n";
            s += "  -b <start marker>\t - only report finalizers queued after this log file comment\n";
            s += "  -e <end marker>  \t - only report finalizers queued before this log file comment\n";
            s += "or\n";
            s += String.Format("{0} -sd -l logName [-b <start marker>] [-e <end marker>]\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -sd              \t - asks for a survivor difference report\n";
            s += "  -l logName       \t - names the input log file\n";
            s += "  -b <start marker>\t - first instant in time to gather survivors\n";
            s += "  -e <end marker>  \t - second instant in time to compare survivorship agains\n";
            s += "or\n";
            s += String.Format("{0} -h -l logName [-b <start marker>] [-e <end marker>]\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -h               \t - asks for a heap dump report\n";
            s += "  -l logName       \t - names the input log file\n";
            s += "  -b <start marker>\t - only report heap dumps after this log file comment\n";
            s += "  -e <end marker>  \t - only report heap dumps before this log file comment\n";
            s += "                   \t - (giving only -b reports the first heap after the log file comment)\n";
            s += "or\n";
            s += String.Format("{0} -c -l logName\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -c               \t - asks for a list of the comments in the input log file\n";
            s += "or\n";
            s += String.Format("{0} -diff [-lo OldlogName -ln NewlogName] [-ld DifflogName][-w]\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -diff            \t- asks for a difference report\n";
            s += "  -lo logName      \t- old resulting profile log file\n";
            s += "  -ln logName      \t- new resulting profile log file\n";
            s += "  -ld logName      \t- diff result of old and new profile log file\n";
            s += "  -w               \t- run as window (otherwise textual report)\n";

		    MessageBox.Show(s, "CLR Profiler");
		    return;
	    }

        private bool ProfiledProcessHasExited()
        {
            if (profiledProcess == null)
                return true;

            try
            {
                return profiledProcess.HasExited;
            }
            catch
            {
                return Process.GetProcessById(profiledProcess.Id) == null;
            }

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) 
        {
            if (disposing) 
            {
                if(components != null)  
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.profilingActiveCheckBox = new System.Windows.Forms.CheckBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.callsCheckBox = new System.Windows.Forms.CheckBox();
            this.allocationsCheckBox = new System.Windows.Forms.CheckBox();
            this.logFileOpenMenuItem = new System.Windows.Forms.MenuItem();
            this.setCommandLineMenuItem = new System.Windows.Forms.MenuItem();
            this.profileApplicationMenuItem = new System.Windows.Forms.MenuItem();
            this.profileASP_NETmenuItem = new System.Windows.Forms.MenuItem();
            this.killApplicationButton = new System.Windows.Forms.Button();
            this.startApplicationButton = new System.Windows.Forms.Button();
            this.checkProcessTimer = new System.Windows.Forms.Timer(this.components);
            this.fontMenuItem = new System.Windows.Forms.MenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.profileServiceMenuItem = new System.Windows.Forms.MenuItem();
            this.saveAsMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.exitMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.viewSummaryMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHistogramAllocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHistogramRelocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHistogramFinalizerMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHistogramCriticalFinalizerMenuItem = new System.Windows.Forms.MenuItem();
            this.viewObjectsByAddressMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHistogramByAgeMenuItem = new System.Windows.Forms.MenuItem();
            this.viewAllocationGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewAssemblyGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewFunctionGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewModuleGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewClassGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHeapGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewCallGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewTimeLineMenuItem = new System.Windows.Forms.MenuItem();
            this.viewCommentsMenuItem = new System.Windows.Forms.MenuItem();
            this.viewCallTreeMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.viewComparisonMenuItem = new System.Windows.Forms.MenuItem();
            this.showHeapButton = new System.Windows.Forms.Button();
            this.fontDialog = new System.Windows.Forms.FontDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // profilingActiveCheckBox
            // 
            this.profilingActiveCheckBox.Checked = true;
            this.profilingActiveCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.profilingActiveCheckBox.Location = new System.Drawing.Point(160, 8);
            this.profilingActiveCheckBox.Name = "profilingActiveCheckBox";
            this.profilingActiveCheckBox.Size = new System.Drawing.Size(127, 24);
            this.profilingActiveCheckBox.TabIndex = 8;
            this.profilingActiveCheckBox.Text = "Profiling active";
            this.profilingActiveCheckBox.CheckedChanged += new System.EventHandler(this.profilingActiveCheckBox_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.callsCheckBox);
            this.groupBox1.Controls.Add(this.allocationsCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(160, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(152, 56);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            // 
            // callsCheckBox
            // 
            this.callsCheckBox.Checked = true;
            this.callsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.callsCheckBox.Location = new System.Drawing.Point(8, 32);
            this.callsCheckBox.Name = "callsCheckBox";
            this.callsCheckBox.Size = new System.Drawing.Size(136, 16);
            this.callsCheckBox.TabIndex = 1;
            this.callsCheckBox.Text = "Calls";
            this.callsCheckBox.CheckedChanged += new System.EventHandler(this.callsCheckBox_CheckedChanged);
            // 
            // allocationsCheckBox
            // 
            this.allocationsCheckBox.Checked = true;
            this.allocationsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.allocationsCheckBox.Location = new System.Drawing.Point(8, 16);
            this.allocationsCheckBox.Name = "allocationsCheckBox";
            this.allocationsCheckBox.Size = new System.Drawing.Size(136, 16);
            this.allocationsCheckBox.TabIndex = 0;
            this.allocationsCheckBox.Text = "Allocations";
            this.allocationsCheckBox.CheckedChanged += new System.EventHandler(this.allocationsCheckBox_CheckedChanged);
            // 
            // logFileOpenMenuItem
            // 
            this.logFileOpenMenuItem.Index = 0;
            this.logFileOpenMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.logFileOpenMenuItem.Text = "Open Log File...";
            this.logFileOpenMenuItem.Click += new System.EventHandler(this.fileOpenMenuItem_Click);
            // 
            // setCommandLineMenuItem
            // 
            this.setCommandLineMenuItem.Index = 5;
            this.setCommandLineMenuItem.Text = "Set Parameters...";
            this.setCommandLineMenuItem.Click += new System.EventHandler(this.setCommandLineMenuItem_Click);
            // 
            // profileApplicationMenuItem
            // 
            this.profileApplicationMenuItem.Index = 1;
            this.profileApplicationMenuItem.Text = "Profile Application...";
            this.profileApplicationMenuItem.Click += new System.EventHandler(this.profileApplicationMenuItem_Click);
            // 
            // profileASP_NETmenuItem
            // 
            this.profileASP_NETmenuItem.Index = 2;
            this.profileASP_NETmenuItem.Text = "Profile ASP.NET";
            this.profileASP_NETmenuItem.Click += new System.EventHandler(this.profileASP_NETmenuItem_Click);
            // 
            // killApplicationButton
            // 
            this.killApplicationButton.Location = new System.Drawing.Point(8, 40);
            this.killApplicationButton.Name = "killApplicationButton";
            this.killApplicationButton.Size = new System.Drawing.Size(136, 23);
            this.killApplicationButton.TabIndex = 4;
            this.killApplicationButton.Text = "Kill Application";
            this.killApplicationButton.Click += new System.EventHandler(this.killApplicationButton_Click);
            // 
            // startApplicationButton
            // 
            this.startApplicationButton.Location = new System.Drawing.Point(8, 8);
            this.startApplicationButton.Name = "startApplicationButton";
            this.startApplicationButton.Size = new System.Drawing.Size(136, 23);
            this.startApplicationButton.TabIndex = 1;
            this.startApplicationButton.Text = "Start Application...";
            this.startApplicationButton.Click += new System.EventHandler(this.startApplicationButton_Click);
            // 
            // checkProcessTimer
            // 
            this.checkProcessTimer.Enabled = true;
            this.checkProcessTimer.Tick += new System.EventHandler(this.checkProcessTimer_Tick);
            // 
            // fontMenuItem
            // 
            this.fontMenuItem.Index = 0;
            this.fontMenuItem.Text = "Font...";
            this.fontMenuItem.Click += new System.EventHandler(this.fontMenuItem_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(168, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Profile:";
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem5,
            this.menuItem8});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.logFileOpenMenuItem,
            this.profileApplicationMenuItem,
            this.profileASP_NETmenuItem,
            this.profileServiceMenuItem,
            this.saveAsMenuItem,
            this.setCommandLineMenuItem,
            this.menuItem3,
            this.exitMenuItem});
            this.menuItem1.Text = "File";
            // 
            // profileServiceMenuItem
            // 
            this.profileServiceMenuItem.Index = 3;
            this.profileServiceMenuItem.Text = "Profile Service...";
            this.profileServiceMenuItem.Click += new System.EventHandler(this.profileServiceMenuItem_Click);
            // 
            // saveAsMenuItem
            // 
            this.saveAsMenuItem.Enabled = false;
            this.saveAsMenuItem.Index = 4;
            this.saveAsMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.saveAsMenuItem.Text = "Save Profile As...";
            this.saveAsMenuItem.Click += new System.EventHandler(this.saveAsMenuItem_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 6;
            this.menuItem3.Text = "-";
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Index = 7;
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 1;
            this.menuItem5.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fontMenuItem});
            this.menuItem5.Text = "Edit";
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 2;
            this.menuItem8.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.viewSummaryMenuItem,
            this.viewHistogramAllocatedMenuItem,
            this.viewHistogramRelocatedMenuItem,
            this.viewHistogramFinalizerMenuItem,
            this.viewHistogramCriticalFinalizerMenuItem,
            this.viewObjectsByAddressMenuItem,
            this.viewHistogramByAgeMenuItem,
            this.viewAllocationGraphMenuItem,
            this.viewAssemblyGraphMenuItem,
            this.viewFunctionGraphMenuItem,
            this.viewModuleGraphMenuItem,
            this.viewClassGraphMenuItem,
            this.viewHeapGraphMenuItem,
            this.viewCallGraphMenuItem,
            this.viewTimeLineMenuItem,
            this.viewCommentsMenuItem,
            this.viewCallTreeMenuItem,
            this.menuItem2,
            this.viewComparisonMenuItem});
            this.menuItem8.Text = "View";
            // 
            // viewSummaryMenuItem
            // 
            this.viewSummaryMenuItem.Index = 0;
            this.viewSummaryMenuItem.Text = "Summary";
            this.viewSummaryMenuItem.Click += new System.EventHandler(this.viewSummaryMenuItem_Click);
            // 
            // viewHistogramAllocatedMenuItem
            // 
            this.viewHistogramAllocatedMenuItem.Index = 1;
            this.viewHistogramAllocatedMenuItem.Text = "Histogram Allocated Types";
            this.viewHistogramAllocatedMenuItem.Click += new System.EventHandler(this.viewHistogram_Click);
            // 
            // viewHistogramRelocatedMenuItem
            // 
            this.viewHistogramRelocatedMenuItem.Index = 2;
            this.viewHistogramRelocatedMenuItem.Text = "Histogram Relocated Types";
            this.viewHistogramRelocatedMenuItem.Click += new System.EventHandler(this.viewHistogramRelocatedMenuItem_Click);
            // 
            // viewHistogramFinalizerMenuItem
            // 
            this.viewHistogramFinalizerMenuItem.Index = 3;
            this.viewHistogramFinalizerMenuItem.Text = "Histogram Finalized Types";
            this.viewHistogramFinalizerMenuItem.Click += new System.EventHandler(this.viewHistogramFinalizerMenuItem_Click);
            // 
            // viewHistogramCriticalFinalizerMenuItem
            // 
            this.viewHistogramCriticalFinalizerMenuItem.Index = 4;
            this.viewHistogramCriticalFinalizerMenuItem.Text = "Histogram Critical Finalized Types";
            this.viewHistogramCriticalFinalizerMenuItem.Click += new System.EventHandler(this.viewHistogramCriticalFinalizerMenuItem_Click);
            // 
            // viewObjectsByAddressMenuItem
            // 
            this.viewObjectsByAddressMenuItem.Index = 5;
            this.viewObjectsByAddressMenuItem.Text = "Objects by Address";
            this.viewObjectsByAddressMenuItem.Click += new System.EventHandler(this.viewByAddressMenuItem_Click);
            // 
            // viewHistogramByAgeMenuItem
            // 
            this.viewHistogramByAgeMenuItem.Index = 6;
            this.viewHistogramByAgeMenuItem.Text = "Histogram by Age";
            this.viewHistogramByAgeMenuItem.Click += new System.EventHandler(this.viewAgeHistogram_Click);
            // 
            // viewAllocationGraphMenuItem
            // 
            this.viewAllocationGraphMenuItem.Index = 7;
            this.viewAllocationGraphMenuItem.Text = "Allocation Graph";
            this.viewAllocationGraphMenuItem.Click += new System.EventHandler(this.viewAllocationGraphmenuItem_Click);
            // 
            // viewAssemblyGraphMenuItem
            // 
            this.viewAssemblyGraphMenuItem.Index = 8;
            this.viewAssemblyGraphMenuItem.Text = "Assembly Graph";
            this.viewAssemblyGraphMenuItem.Click += new System.EventHandler(this.viewAssemblyGraphmenuItem_Click);
            // 
            // viewFunctionGraphMenuItem
            // 
            this.viewFunctionGraphMenuItem.Index = 9;
            this.viewFunctionGraphMenuItem.Text = "Function Graph";
            this.viewFunctionGraphMenuItem.Click += new System.EventHandler(this.viewFunctionGraphMenuItem_Click);
            // 
            // viewModuleGraphMenuItem
            // 
            this.viewModuleGraphMenuItem.Index = 10;
            this.viewModuleGraphMenuItem.Text = "Module Graph";
            this.viewModuleGraphMenuItem.Click += new System.EventHandler(this.viewModuleGraphMenuItem_Click);
            // 
            // viewClassGraphMenuItem
            // 
            this.viewClassGraphMenuItem.Index = 11;
            this.viewClassGraphMenuItem.Text = "Class Graph";
            this.viewClassGraphMenuItem.Click += new System.EventHandler(this.viewClassGraphMenuItem_Click);
            // 
            // viewHeapGraphMenuItem
            // 
            this.viewHeapGraphMenuItem.Index = 12;
            this.viewHeapGraphMenuItem.Text = "Heap Graph";
            this.viewHeapGraphMenuItem.Click += new System.EventHandler(this.viewHeapGraphMenuItem_Click);
            // 
            // viewCallGraphMenuItem
            // 
            this.viewCallGraphMenuItem.Index = 13;
            this.viewCallGraphMenuItem.Text = "Call Graph";
            this.viewCallGraphMenuItem.Click += new System.EventHandler(this.viewCallGraphMenuItem_Click);
            // 
            // viewTimeLineMenuItem
            // 
            this.viewTimeLineMenuItem.Index = 14;
            this.viewTimeLineMenuItem.Text = "Time Line";
            this.viewTimeLineMenuItem.Click += new System.EventHandler(this.viewTimeLineMenuItem_Click);
            // 
            // viewCommentsMenuItem
            // 
            this.viewCommentsMenuItem.Index = 15;
            this.viewCommentsMenuItem.Text = "Comments";
            this.viewCommentsMenuItem.Click += new System.EventHandler(this.viewCommentsMenuItem_Click);
            // 
            // viewCallTreeMenuItem
            // 
            this.viewCallTreeMenuItem.Index = 16;
            this.viewCallTreeMenuItem.Text = "Call Tree";
            this.viewCallTreeMenuItem.Click += new System.EventHandler(this.viewCallTreeMenuItem_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 17;
            this.menuItem2.Text = "-----------------------------";
            // 
            // viewComparisonMenuItem
            // 
            this.viewComparisonMenuItem.Index = 18;
            this.viewComparisonMenuItem.Text = "Comparison with ...";
            this.viewComparisonMenuItem.Click += new System.EventHandler(this.viewComparisonMenuItem_Click);
            // 
            // showHeapButton
            // 
            this.showHeapButton.Location = new System.Drawing.Point(8, 72);
            this.showHeapButton.Name = "showHeapButton";
            this.showHeapButton.Size = new System.Drawing.Size(136, 23);
            this.showHeapButton.TabIndex = 9;
            this.showHeapButton.Text = "Show Heap now";
            this.showHeapButton.Click += new System.EventHandler(this.showHeapButton_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileName = "doc1";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(320, 104);
            this.Controls.Add(this.showHeapButton);
            this.Controls.Add(this.profilingActiveCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.killApplicationButton);
            this.Controls.Add(this.startApplicationButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Menu = this.mainMenu;
            this.Name = "MainForm";
            this.Text = "CLR Profiler";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] arguments)
        {
            consoleCtrl = new ConsoleCtrl();
            consoleCtrl.ControlEvent += new ConsoleCtrl.ControlEventHandler(ConsoleEventHandler);
            MainForm f = new MainForm(arguments);
			if (!f.viewdiff && f.reportKind == MainForm.ReportKind.NoReport && f.processFileName == null)
			{
				Application.Run(f);
			}
			else
			{
				try
				{
					ReportForm _MgrForm = new ReportForm(f);
					if (f.runaswindow)
					{
						Application.Run(_MgrForm);
					}
				}
				catch
				{
					Application.Exit();
				}
			}
        }

        static ConsoleCtrl consoleCtrl;

        static bool ConsoleEventHandler(ConsoleCtrl.ConsoleEvent consoleEvent)
        {
            MainForm instance = MainForm.instance;
            if (instance == null || (instance.CheckProcessTerminate() && instance.CheckFileSave()))
                Application.Exit();
            return true;
        }

        class ConsoleCtrl
        {
            internal enum ConsoleEvent
            {
                CTRL_C = 0,           // From wincom.h
                CTRL_BREAK = 1,
                CTRL_CLOSE = 2,
                CTRL_LOGOFF = 5,
                CTRL_SHUTDOWN = 6
            }

            internal delegate bool ControlEventHandler(ConsoleEvent consoleEvent);

            internal event ControlEventHandler ControlEvent;

            ControlEventHandler eventHandler;

            internal ConsoleCtrl()
            {
                // save this to a private var so the GC doesn't collect it...
                eventHandler = new ControlEventHandler(Handler);
                SetConsoleCtrlHandler(eventHandler, true);
            }

            private bool Handler(ConsoleEvent consoleEvent)
            {
                if (ControlEvent != null)
                    return ControlEvent(consoleEvent);
                return false;
            }

            [DllImport("kernel32.dll")]
            static extern bool SetConsoleCtrlHandler(ControlEventHandler e, bool add);
        }

        private void ViewGraph(ReadLogResult logResult, string exeName, Graph.GraphType graphType)
        {
            string fileName = log.fileName;
            if (exeName != null)
                fileName = exeName;
            Graph graph = null;
            string title = "";
            switch (graphType)
            {
                case    Graph.GraphType.CallGraph:
                    graph = logResult.callstackHistogram.BuildCallGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.CallGraph;
                    title = "Call Graph for: ";
                    break;

                case Graph.GraphType.AssemblyGraph:
                    graph = logResult.callstackHistogram.BuildAssemblyGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.AssemblyGraph;
                    title = "Assembly Graph for: ";
                    break;

                case    Graph.GraphType.AllocationGraph:
                    graph = logResult.allocatedHistogram.BuildAllocationGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.AllocationGraph;
                    title = "Allocation Graph for: ";
                    break;

                case    Graph.GraphType.HeapGraph:
                    graph = logResult.objectGraph.BuildTypeGraph(new FilterForm());
                    title = "Heap Graph for: ";
                    break;

                case    Graph.GraphType.FunctionGraph:
                    graph = logResult.functionList.BuildFunctionGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.FunctionGraph;
                    title = "Function Graph for: ";
                    break;

                case    Graph.GraphType.ModuleGraph:
                    graph = logResult.functionList.BuildModuleGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.ModuleGraph;
                    title = "Module Graph for: ";
                    break;

                case    Graph.GraphType.ClassGraph:
                    graph = logResult.functionList.BuildClassGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.ClassGraph;
                    title = "Class Graph for: ";
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
            title += fileName + " " + commandLine;
            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Visible = true;
        }

        private void readLogFile(ReadNewLog log, ReadLogResult logResult, string exeName, Graph.GraphType graphType)
        {
            log.ReadFile(logFileStartOffset, logFileEndOffset, logResult);
            ViewGraph(logResult, exeName, graphType);
        }

        private void EnableDisableViewMenuItems()
        {
            if (lastLogResult == null)
            {
                viewAllocationGraphMenuItem.Enabled = false;
                viewCallGraphMenuItem.Enabled = false;
                viewHeapGraphMenuItem.Enabled = false;
                viewHistogramAllocatedMenuItem.Enabled = false;
                viewHistogramRelocatedMenuItem.Enabled = false;
                viewHistogramFinalizerMenuItem.Enabled = false;
                viewHistogramCriticalFinalizerMenuItem.Enabled = false;
                viewHistogramByAgeMenuItem.Enabled = false;
                viewObjectsByAddressMenuItem.Enabled = false;
                viewTimeLineMenuItem.Enabled = false;
                viewFunctionGraphMenuItem.Enabled = false;
                viewModuleGraphMenuItem.Enabled = false;
                viewClassGraphMenuItem.Enabled = false;
                viewCallTreeMenuItem.Enabled = false;
                viewAssemblyGraphMenuItem.Enabled = false;
				viewComparisonMenuItem.Enabled = false;
                viewSummaryMenuItem.Enabled = false;
            }
            else
            {
                viewAllocationGraphMenuItem.Enabled = !lastLogResult.allocatedHistogram.Empty;
                viewCallGraphMenuItem.Enabled = !lastLogResult.callstackHistogram.Empty;
                viewAssemblyGraphMenuItem.Enabled = !lastLogResult.callstackHistogram.Empty;
                viewHeapGraphMenuItem.Enabled = !lastLogResult.objectGraph.empty;
                viewHistogramAllocatedMenuItem.Enabled = !lastLogResult.allocatedHistogram.Empty;
                viewHistogramRelocatedMenuItem.Enabled = !lastLogResult.relocatedHistogram.Empty;
                viewHistogramFinalizerMenuItem.Enabled = !lastLogResult.finalizerHistogram.Empty;
                viewHistogramCriticalFinalizerMenuItem.Enabled = !lastLogResult.criticalFinalizerHistogram.Empty;
                viewHistogramByAgeMenuItem.Enabled = lastLogResult.liveObjectTable != null;
                viewObjectsByAddressMenuItem.Enabled = lastLogResult.liveObjectTable != null;
                viewTimeLineMenuItem.Enabled = lastLogResult.sampleObjectTable != null;
                viewFunctionGraphMenuItem.Enabled = !lastLogResult.functionList.Empty;
                viewModuleGraphMenuItem.Enabled = !lastLogResult.functionList.Empty;
                viewClassGraphMenuItem.Enabled = !lastLogResult.functionList.Empty;
                viewCallTreeMenuItem.Enabled = lastLogResult.hadCallInfo;
				viewComparisonMenuItem.Enabled = !lastLogResult.functionList.Empty;
                viewSummaryMenuItem.Enabled = true;
            }
            viewCommentsMenuItem.Enabled = (log != null && log.commentEventList.count != 0);
        }

        private void LoadLogFile(string logFileName)
        {
            this.logFileName = logFileName;
            logFileStartOffset = 0;
            logFileEndOffset = long.MaxValue;
                
            processFileName = null;

            log = new ReadNewLog(logFileName);
            lastLogResult = null;
            ObjectGraph.cachedGraph = null;
            ReadLogResult readLogResult = GetLogResult();
            log.ReadFile(logFileStartOffset, logFileEndOffset, readLogResult);
            lastLogResult = readLogResult;
            Text = "Analyzing " + logFileName;
            EnableDisableViewMenuItems();
            viewSummaryMenuItem_Click(null, null);
        }

        private void fileOpenMenuItem_Click(object sender, System.EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
                return;

            saveAsMenuItem.Enabled = false;

            openFileDialog.FileName = "*.log";
            openFileDialog.Filter = "Allocation Logs | *.log";
            if (   openFileDialog.ShowDialog() == DialogResult.OK
                && openFileDialog.CheckFileExists)
            {
                LoadLogFile(openFileDialog.FileName);
            }
        }

        private void SaveFile()
        {
            string baseName = Path.GetFileNameWithoutExtension(processFileName);
            string fileName = Path.ChangeExtension(processFileName, ".log");
            int count = 0;
            while (File.Exists(fileName))
            {
                count++;
                fileName = string.Format("{0}_{1}.log", baseName, count);
            }
            saveFileDialog.FileName = fileName;
            saveFileDialog.Filter = "Allocation Logs | *.log";
            DialogResult retValue = saveFileDialog.ShowDialog();
            if (retValue == DialogResult.OK)
            {
                try
                {
                    if (File.Exists(saveFileDialog.FileName) && saveFileDialog.FileName != logFileName)
                    {
                        File.Delete(saveFileDialog.FileName);
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Cannot delete existing file " + saveFileDialog.FileName, "Failure");
                    return;
                }
                try
                {
                    File.Move(logFileName, saveFileDialog.FileName);
                }
                catch
                {
                    MessageBox.Show(this, "Cannot rename log file " + logFileName + " to " + saveFileDialog.FileName, "Failure");
                    return;
                }
                if(!noUI)
                {
                    saveAsMenuItem.Enabled = false;
                }
                if (log != null)
                    log.fileName = saveFileDialog.FileName;
            }
        }

        private bool CheckFileSave()
        {
            if (saveAsMenuItem != null && saveAsMenuItem.Enabled)
            {
                if (saveNever)
                {
                    File.Delete(logFileName);
                }
                else
                {
                    SaveFileForm saveFileForm = new SaveFileForm();
                    saveFileForm.processFileNameLabel.Text = processFileName;
                    switch (saveFileForm.ShowDialog())
                    {
                        case    DialogResult.Yes:
                            SaveFile();
                            break;

                        case    DialogResult.No:
                            File.Delete(logFileName);
                            saveAsMenuItem.Enabled = false;
                            break;

                        case    DialogResult.Cancel:
                            return false;

                        case    DialogResult.Retry:
                            saveNever = true;
                            break;
                    }
                }
            }
            return true;
        }

        private bool CheckProcessTerminate()
        {
            if (killApplicationButton != null && killApplicationButton.Enabled)
            {
                KillProcessForm killProcessForm = new KillProcessForm();
                killProcessForm.processFileNameLabel.Text = processFileName;
                switch (killProcessForm.ShowDialog())
                {
                    case    DialogResult.Yes:
                        if (profiledProcess != null)
                        {
                            killApplicationButton_Click(null, null);
                            saveAsMenuItem.Enabled = true;
                        }
                        break;

                    case    DialogResult.No:
                        profiledProcess = null;
                        break;

                    case    DialogResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private void exitMenuItem_Click(object sender, System.EventArgs e)
        {
            if (CheckProcessTerminate() && CheckFileSave())
                Application.Exit();
        }

        private void fontMenuItem_Click(object sender, System.EventArgs e)
        {
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                font = fontDialog.Font;
            }
        }

        private void profileApplicationMenuItem_Click(object sender, System.EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
                return;

            openFileDialog.FileName = "*.exe";
            openFileDialog.Filter = "Applications | *.exe";
            if (   openFileDialog.ShowDialog() == DialogResult.OK
                && openFileDialog.CheckFileExists)
            {
                processFileName = openFileDialog.FileName;
                Text = "Profiling: " + processFileName + " " + commandLine;
                startApplicationButton.Text = "Start Application";
                killApplicationButton.Text = "Kill Application";
                serviceName = null;
                startApplicationButton_Click(null, null);
            }
        }

        private void StopIIS()
        {
            // stop IIS
            Text = "Stopping IIS ";
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
            processStartInfo.Arguments = "/c net stop iisadmin /y";
            Process process = Process.Start(processStartInfo);
            while (!process.HasExited)
            {
                Text += ".";
                Thread.Sleep(100);
                Application.DoEvents();
            }
            if (process.ExitCode != 0)
            {
                Text += string.Format(" Error {0} occurred", process.ExitCode);
            }
            else
                Text = "IIS stopped";           
        }

        private bool StartIIS()
        {
            Text = "Starting IIS ";
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
            processStartInfo.Arguments = "/c net start w3svc";
            Process process = Process.Start(processStartInfo);
            while (!process.HasExited)
            {
                Text += ".";
                Thread.Sleep(100);
                Application.DoEvents();
            }
            if (process.ExitCode != 0)
            {
                Text += string.Format(" Error {0} occurred", process.ExitCode);
                return false;
            }
            Text = "IIS running";
            return true;
        }

        private void StopService(string serviceName, string stopCommand)
        {
            // stop service
            Text = "Stopping " + serviceName + " ";
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
            processStartInfo.Arguments = "/c " + stopCommand;
            Process process = Process.Start(processStartInfo);
            while (!process.HasExited)
            {
                Text += ".";
                Thread.Sleep(1000);
            }
            if (process.ExitCode != 0)
            {
                Text += string.Format(" Error {0} occurred", process.ExitCode);
            }
            else
                Text = serviceName + " stopped";
        }

        private Process StartService(string serviceName, string startCommand)
        {
            Text = "Starting " + serviceName + " ";
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
            processStartInfo.Arguments = "/c " + startCommand;
            Process process = Process.Start(processStartInfo);
            return process;
        }

        private static unsafe int wcslen(char *s)
        {
            char *e;
            for (e = s; *e != '\0'; e++)
                ;
            return (int)(e - s);
        }

        private string[] GetServicesEnvironment()
        {
            Process[] servicesProcesses = Process.GetProcessesByName("services");
            if (servicesProcesses == null || servicesProcesses.Length != 1)
            {
                servicesProcesses = Process.GetProcessesByName("services.exe");
                if (servicesProcesses == null || servicesProcesses.Length != 1)
                    return new string[0];
            }
            Process servicesProcess = servicesProcesses[0];
            IntPtr processHandle = OpenProcess(0x20400, false, servicesProcess.Id);
            if (processHandle == IntPtr.Zero)
                return new string[0];
            IntPtr tokenHandle = IntPtr.Zero;
            if (!OpenProcessToken(processHandle, 0x20008, ref tokenHandle))
                return new string[0];
            IntPtr environmentPtr = IntPtr.Zero;
            if (!CreateEnvironmentBlock(out environmentPtr, tokenHandle, false))
                return new String[0];
            unsafe
            {
                string[] envStrings = null;
                // rather than duplicate the code that walks over the environment, 
                // we have this funny loop where the first iteration just counts the strings,
                // and the second iteration fills in the strings
                for (int i = 0; i < 2; i++)
                {
                    char *env = (char *)environmentPtr.ToPointer();
                    int count = 0;
                    while (true)
                    {
                        int len = wcslen(env);
                        if (len == 0)
                            break;
                        if (envStrings != null)
                            envStrings[count] = new String(env);
                        count++;
                        env += len + 1;
                    }
                    if (envStrings == null)
                        envStrings = new string[count];
                }
                return envStrings;
            }
        }

        private string[] CombineEnvironmentVariables(string[] a, string[] b)
        {
            string[] c = new string[a.Length + b.Length];
            int i = 0;
            foreach (string s in a)
                c[i++] = s;
            foreach (string s in b)
                c[i++] = s;
            return c;
        }

        private Microsoft.Win32.RegistryKey GetServiceKey(string serviceName)
        {
            Microsoft.Win32.RegistryKey localMachine = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey key = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" + serviceName, true);
            return key;
        }

        private void SetEnvironmentVariables(string serviceName, string[] environment)
        {
            Microsoft.Win32.RegistryKey key = GetServiceKey(serviceName);
            if (key != null)
                key.SetValue("Environment", environment);
        }

        private void DeleteEnvironmentVariables(string serviceName)
        {
            Microsoft.Win32.RegistryKey key = GetServiceKey(serviceName);
            if (key != null)
                key.DeleteValue("Environment");
        }

        private string EnvKey(string envVariable)
        {
            int index = envVariable.IndexOf('=');
            Debug.Assert(index >= 0);
            return envVariable.Substring(0, index);
        }

        private string EnvValue(string envVariable)
        {
            int index = envVariable.IndexOf('=');
            Debug.Assert(index >= 0);
            return envVariable.Substring(index+1);
        }

        private Microsoft.Win32.RegistryKey GetAccountEnvironmentKey(string serviceAccountSid)
        {
            Microsoft.Win32.RegistryKey users = Microsoft.Win32.Registry.Users;
            return users.OpenSubKey(serviceAccountSid + @"\Environment", true);
        }

        private void SetAccountEnvironment(string serviceAccountSid, string[] profilerEnvironment)
        {
            Microsoft.Win32.RegistryKey key = GetAccountEnvironmentKey(serviceAccountSid);
            if (key != null)
            {
                foreach (string envVariable in profilerEnvironment)
                {
                    key.SetValue(EnvKey(envVariable), EnvValue(envVariable));
                }
            }
        }

        private void ResetAccountEnvironment(string serviceAccountSid, string[] profilerEnvironment)
        {
            Microsoft.Win32.RegistryKey key = GetAccountEnvironmentKey(serviceAccountSid);
            if (key != null)
            {
                foreach (string envVariable in profilerEnvironment)
                {
                    key.DeleteValue(EnvKey(envVariable));
                }
            }
        }

        private string CreateUsageString()
        {
            int index = 0;
            string[] usageStrings = new string[] {"none", "objects", "trace", "both"};
            if ((noUI && profileAllocations) || (!noUI && allocationsCheckBox.Checked))
            {
                index |= 1;
            }
            if ((noUI && profileCalls) || (!noUI && callsCheckBox.Checked))
            {
                index |= 2;
            }

            return usageStrings[index];
        }

        private string CreateInitialString()
        {
            int flags = 0;
            if ((noUI && profileAllocations && profilingActive) || (!noUI && allocationsCheckBox.Checked && profilingActiveCheckBox.Checked))
            {
                flags |= 1;
            }
            if ((noUI && profileCalls && profilingActive) || (!noUI && callsCheckBox.Checked && profilingActiveCheckBox.Checked))
            {
                flags |= 2;
            }
            return flags.ToString();
        }

        private string[] CreateProfilerEnvironment(string tempDir)
        {
            return new string[]
            { 
                "Cor_Enable_Profiling=0x1",
                "COR_PROFILER={8C29BC4E-1F57-461a-9B51-1200C32E6F1F}",
                "OMV_SKIP=0",
                "OMV_FORMAT=v2",
                "OMV_STACK=1",
                "OMV_DynamicObjectTracking=0x1",
                "OMV_PATH=" + tempDir,
                "OMV_USAGE=" + CreateUsageString(),
                "OMV_FORCE_GC_ON_COMMENT="+(gcOnLogFileComments ? "1" : "0"),
                "OMV_INITIAL_SETTING=" + CreateInitialString()
            };
        }

        private string GetServiceAccountName(string serviceName)
        {
            Microsoft.Win32.RegistryKey key = GetServiceKey(serviceName);
            if (key != null)
                return key.GetValue("ObjectName") as string;
            return null;
        }

        private string LookupAccountSid(string accountName)
        {
            int sidLen = 0;
            byte[] sid = new byte[sidLen];
            int domainNameLen = 0;
            int peUse;
            StringBuilder domainName = new StringBuilder();
            LookupAccountName(Environment.MachineName, accountName, sid, ref sidLen, domainName, ref domainNameLen, out peUse);

            sid = new byte[sidLen];
            domainName = new StringBuilder(domainNameLen);
            string stringSid = null;
            if (LookupAccountName(Environment.MachineName, accountName, sid, ref sidLen, domainName, ref domainNameLen, out peUse))
            {
                IntPtr stringSidPtr;
                if (ConvertSidToStringSidW(sid, out stringSidPtr))
                {
                    try
                    {
                        stringSid = Marshal.PtrToStringUni(stringSidPtr);
                    }
                    finally
                    {
                        LocalFree(stringSidPtr);
                    }
                }
            }
            return stringSid;
        }

        private string[] ReplaceTempDir(string[] env, string newTempDir)
        {
            for (int i = 0; i < env.Length; i++)
            {
                if (env[i].StartsWith("TEMP="))
                    env[i] = "TEMP=" + newTempDir;
                else if (env[i].StartsWith("TMP="))
                    env[i] = "TMP=" + newTempDir;
            }
            return env;
        }

        private void ProfileService()
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
                return;

            RegisterDLL.Register();

            StopService(serviceName, serviceStopCommand);

            string logDir = GetLogDir();
            string[] profilerEnvironment = CreateProfilerEnvironment(logDir);

            // set environment variables

            // this is a bit intricate - if the service is running as LocalSystem, we need to set the environment
            // variables in the registry for the service, otherwise it's better to temporarily set it for the account,
            // assuming we can find out the account SID
            string serviceAccountName = GetServiceAccountName(serviceName);
            if (serviceAccountName.StartsWith(@".\"))
                serviceAccountName = Environment.MachineName + serviceAccountName.Substring(1);
            if (serviceAccountName != null && serviceAccountName != "LocalSystem")
            {
                serviceAccountSid = LookupAccountSid(serviceAccountName);
            }
            if (serviceAccountSid != null)
            {
                SetAccountEnvironment(serviceAccountSid, profilerEnvironment);
            }
            else
            {
                string[] baseEnvironment = GetServicesEnvironment();
                baseEnvironment = ReplaceTempDir(baseEnvironment, GetTempDir());
                string[] combinedEnvironment = CombineEnvironmentVariables(baseEnvironment, profilerEnvironment);
                SetEnvironmentVariables(serviceName, combinedEnvironment);
            }

            Process cmdProcess = StartService(serviceName, serviceStartCommand);

            // wait for service to start up and connect
            Text = string.Format("Waiting for {0} to start up", serviceName);

            Thread.Sleep(1000);
            int pid = WaitForProcessToConnect(logDir, "Waiting for service to start common language runtime");
            if (pid > 0)
            {
                profiledProcess = Process.GetProcessById(pid);

                Text = "Profiling: " + serviceName;
                startApplicationButton.Text = "Start " + serviceName;
                killApplicationButton.Text = "Kill " + serviceName;
                processFileName = serviceName;
            }

            /* Delete the environment variables as early as possible, so that even if CLRProfiler crashes, the user's machine
             * won't be screwed up.
             * */

            if (serviceAccountSid != null)
            {
                ResetAccountEnvironment(serviceAccountSid, profilerEnvironment);
            }
            else
            {
                DeleteEnvironmentVariables(serviceName);
            }
        }
        
        private string GetASP_NETaccountName()
        {
            try
            {
                XmlDocument machineConfig = new XmlDocument();
                string runtimePath = RuntimeEnvironment.GetRuntimeDirectory();
                string configPath = Path.Combine(runtimePath, @"CONFIG\machine.config");
                machineConfig.Load(configPath);
                XmlNodeList elemList = machineConfig.GetElementsByTagName("processModel");
                for (int i = 0; i < elemList.Count; i++)
                {
                    XmlAttributeCollection attributes = elemList[i].Attributes;
                    XmlAttribute userNameAttribute = attributes["userName"];
                    if (userNameAttribute != null)
                    {
                        string userName = userNameAttribute.InnerText;
                        if (userName == "machine")
                            return "ASPNET";
                        else if (userName == "SYSTEM")
                            return null;
                        else
                            return userName;
                    }
                }
            }
            catch
            {
                // swallow all exceptions here
            }
            return "ASPNET";
        }

        private void profileASP_NETmenuItem_Click(object sender, System.EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
                return;

            RegisterDLL.Register();

            StopIIS();

            // set environment variables

            string logDir = GetLogDir();
            string[] profilerEnvironment = CreateProfilerEnvironment(logDir);

            string[] baseEnvironment = GetServicesEnvironment();
            baseEnvironment = ReplaceTempDir(baseEnvironment, GetTempDir());
            string[] combinedEnvironment = CombineEnvironmentVariables(baseEnvironment, profilerEnvironment);
            SetEnvironmentVariables("IISADMIN", combinedEnvironment);
            SetEnvironmentVariables("W3SVC", combinedEnvironment);
            SetEnvironmentVariables("WAS", combinedEnvironment);

            string asp_netAccountName = GetASP_NETaccountName();
            string asp_netAccountSid = null;
            if (asp_netAccountName != null)
            {
                asp_netAccountSid = LookupAccountSid(asp_netAccountName);
                if (asp_netAccountSid != null)
                    SetAccountEnvironment(asp_netAccountSid, profilerEnvironment);
            }

            if (StartIIS())
            {
                // wait for worker process to start up and connect
                Text = "Waiting for ASP.NET worker process to start up";

                Thread.Sleep(1000);
                int pid = WaitForProcessToConnect(logDir, "Waiting for ASP.NET to start common language runtime - this is the time to load your test page");
                if (pid > 0)
                {
                    profiledProcess = Process.GetProcessById(pid);

                    Text = "Profiling: ASP.NET";
                    startApplicationButton.Text = "Start ASP.NET";
                    killApplicationButton.Text = "Kill ASP.NET";
                    processFileName = "ASP.NET";
                }
            }

            /* Delete the environment variables as early as possible, so that even if CLRProfiler crashes, the user's machine
             * won't be screwed up.
             * */ 
            DeleteEnvironmentVariables("IISADMIN");
            DeleteEnvironmentVariables("W3SVC");
            DeleteEnvironmentVariables("WAS");

            if (asp_netAccountSid != null)
                ResetAccountEnvironment(asp_netAccountSid, profilerEnvironment);

            serviceName = null;
        }

        struct SECURITY_ATTRIBUTES
        {
            public uint   nLength; 
            public IntPtr lpSecurityDescriptor; 
            public int    bInheritHandle; 
        }; 

#if(USESAFEHANDLES)
        [DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
        private static extern SafeFileHandle CreateNamedPipe(
            string lpName,         // pointer to pipe name
            uint dwOpenMode,       // pipe open mode
            uint dwPipeMode,       // pipe-specific modes
            uint nMaxInstances,    // maximum number of instances
            uint nOutBufferSize,   // output buffer size, in bytes
            uint nInBufferSize,    // input buffer size, in bytes
            uint nDefaultTimeOut,  // time-out time, in milliseconds
            ref SECURITY_ATTRIBUTES lpSecurityAttributes  // pointer to security attributes
            );
#else
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		private static extern IntPtr CreateNamedPipe(
			string lpName,         // pointer to pipe name
			uint dwOpenMode,       // pipe open mode
			uint dwPipeMode,       // pipe-specific modes
			uint nMaxInstances,    // maximum number of instances
			uint nOutBufferSize,   // output buffer size, in bytes
			uint nInBufferSize,    // input buffer size, in bytes
			uint nDefaultTimeOut,  // time-out time, in milliseconds
			ref SECURITY_ATTRIBUTES lpSecurityAttributes  // pointer to security attributes
			);
#endif

        [DllImport("Kernel32.dll")]
        private static extern IntPtr OpenProcess(
            uint dwDesiredAccess,  // access flag
            bool bInheritHandle,    // handle inheritance option
            int dwProcessId       // process identifier
            );

        [DllImport("Advapi32.dll")]
        private static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            uint DesiredAccess,
            ref IntPtr TokenHandle
            );

        [DllImport("UserEnv.dll")]
        private static extern bool  CreateEnvironmentBlock(
                out IntPtr lpEnvironment,
                IntPtr hToken,
                bool bInherit);

        [DllImport("UserEnv.dll")]
        private static extern bool DestroyEnvironmentBlock(
                IntPtr lpEnvironment);

        [DllImport("Advapi32.dll")]
        private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
            string StringSecurityDescriptor,
            uint StringSDRevision,
            out IntPtr SecurityDescriptor,
            IntPtr SecurityDescriptorSize
            );

        [DllImport("Kernel32.dll")]
        private static extern bool LocalFree(IntPtr ptr);

        [DllImport("Advapi32.dll")]
        private static extern bool ConvertSidToStringSidW(byte[] sid, out IntPtr stringSid);

        [DllImport("Advapi32.dll")]
        private static extern bool LookupAccountName(string machineName, string accountName, byte[] sid,
                                 ref int sidLen, StringBuilder domainName, ref int domainNameLen, out int peUse);

#if(USESAFEHANDLES)
        [DllImport("Kernel32.dll")]
        private static extern bool ConnectNamedPipe(
            SafeFileHandle hNamedPipe,  // handle to named pipe to connect
            IntPtr lpOverlapped         // pointer to overlapped structure
            );

        [DllImport("Kernel32.dll")]
        private static extern bool DisconnectNamedPipe(
            SafeFileHandle hNamedPipe   // handle to named pipe
            );
#else
		[DllImport("Kernel32.dll")]
		private static extern bool ConnectNamedPipe(
			IntPtr hNamedPipe,          // handle to named pipe to connect
			IntPtr lpOverlapped         // pointer to overlapped structure
			);
		[DllImport("Kernel32.dll")]
		private static extern bool DisconnectNamedPipe(
			IntPtr hNamedPipe   // handle to named pipe
			);
#endif

        [DllImport("Kernel32.dll")]
        private static extern int GetLastError();

        [DllImport("Kernel32.dll")]
        private static extern bool ReadFile(
            IntPtr hFile,                // handle of file to read
            byte[] lpBuffer,             // pointer to buffer that receives data
            uint nNumberOfBytesToRead,  // number of bytes to read
            out uint lpNumberOfBytesRead, // pointer to number of bytes read
            IntPtr lpOverlapped    // pointer to structure for data
            );

#if(USESAFEHANDLES)
        private bool CreatePipe(bool blockingPipe)
        {
            SECURITY_ATTRIBUTES sa;
            sa.nLength = 12;
            sa.bInheritHandle = 0;
            if (!ConvertStringSecurityDescriptorToSecurityDescriptor("D: (A;OICI;GRGW;;;AU)", 1, out sa.lpSecurityDescriptor, IntPtr.Zero))
                return false;
            uint flags = 4 | 2 | 0;
            if (!blockingPipe)
                flags |= 1;
            pipeHandle = CreateNamedPipe(@"\\.\pipe\OMV_PIPE", 3, flags, 1, 512, 512, 1000, ref sa);
            LocalFree(sa.lpSecurityDescriptor);
            if (pipeHandle.IsInvalid)
                return false;
            pipe = new FileStream(pipeHandle, FileAccess.ReadWrite, 512, false);
            return true;
        }

		private void ClosePipe()
        {
            pipe.Close();
            pipe = null;
            pipeHandle = null;
        }
#else
		private bool CreatePipe(bool blockingPipe)
		{
			SECURITY_ATTRIBUTES sa;
			sa.nLength = 12;
			sa.bInheritHandle = 0;
			if (!ConvertStringSecurityDescriptorToSecurityDescriptor("D: (A;OICI;GRGW;;;AU)", 1, out sa.lpSecurityDescriptor, IntPtr.Zero))
				return false;
			uint flags = 4 | 2 | 0;
			if (!blockingPipe)
				flags |= 1;
			pipeHandle = CreateNamedPipe(@"\\.\pipe\OMV_PIPE", 3, flags, 1, 512, 512, 1000, ref sa);
			LocalFree(sa.lpSecurityDescriptor);
			if (pipeHandle == (IntPtr)(-1))
				return false;
			pipe = new FileStream(pipeHandle, FileAccess.ReadWrite, true, 512, false);
			return true;
		}
		private void ClosePipe()
		{
			pipe.Close();
			pipe = null;
			pipeHandle = IntPtr.Zero;
		}
#endif

        private NamedManualResetEvent CreateEvent(string baseName, int pid)
        {
            string eventName = string.Format("{0}_{1:x8}", baseName, pid);
            return new NamedManualResetEvent(eventName, false);
        }

        private void CreateEvents(int pid)
        {
            try
            {
                loggingActiveEvent = CreateEvent("Global\\OMV_TriggerObjects", pid);
                loggingActiveCompletedEvent = CreateEvent("Global\\OMV_TriggerObjects_Completed", pid);
                forceGcEvent = CreateEvent("Global\\OMV_ForceGC", pid);
                forceGcCompletedEvent = CreateEvent("Global\\OMV_ForceGC_Completed", pid);
                callGraphActiveEvent = CreateEvent("Global\\OMV_Callgraph", pid);
                callGraphActiveCompletedEvent = CreateEvent("Global\\OMV_Callgraph_Completed", pid);
            }
            catch
            {
                MessageBox.Show("Could not create events - in case you are profiling a service, " +
                    "start the profiler BEFORE starting the service");
                throw;
            }
        }

        private void ClearEvents()
        {
            loggingActiveEvent = null;
            forceGcEvent = null;
            loggingActiveCompletedEvent = null;
            forceGcCompletedEvent = null;
            callGraphActiveEvent = null;
            callGraphActiveCompletedEvent = null;
        }

        private string GetTempDir()
        {
            if (logDirectory != null)
                return logDirectory;

            string tempDir = null;
            string winDir = Environment.GetEnvironmentVariable("WINDIR");
            if (winDir != null)
            {
                tempDir = winDir + @"\TEMP";
                if (!Directory.Exists(tempDir))
                    tempDir = null;
            }
            if (tempDir == null)
            {
                tempDir = Environment.GetEnvironmentVariable("TEMP");
                if (tempDir == null)
                {
                    tempDir = Environment.GetEnvironmentVariable("TMP");
                    if (tempDir == null)
                        tempDir = @"C:\TEMP";
                }
            }
            return  tempDir;
        }

        private string GetLogDir()
        {
            if (logDirectory != null)
                return logDirectory;

            return  GetTempDir();
        }

        private int WaitForProcessToConnect(string tempDir, string text)
        {
            ConnectNamedPipe(pipeHandle, IntPtr.Zero);

            int pid = 0;

            byte[] buffer = new byte[9];
            int readBytes = 0;
            WaitingForConnectionForm waitingForConnectionForm = null;
            int beginTickCount = Environment.TickCount;
            while (true)
            {
                try
                {
                    readBytes += pipe.Read(buffer, readBytes, 9 - readBytes);
                }
                catch (System.IO.IOException)
                {
                }
                if (readBytes == 9)
                    break;
                Application.DoEvents();
                if (waitingForConnectionForm != null && waitingForConnectionForm.DialogResult == DialogResult.Cancel)
                {
                    pid = -1;
                    break;
                }
                if (waitingForConnectionForm == null && Environment.TickCount - beginTickCount >= 300)
                {
                    waitingForConnectionForm = new WaitingForConnectionForm();
                    waitingForConnectionForm.textLabel.Text = text;
                    waitingForConnectionForm.Visible = true;
                }
                Thread.Sleep(100);
            }

            if (waitingForConnectionForm != null)
                waitingForConnectionForm.Visible = false;
    
            if (pid == -1)
                return pid;

            if (readBytes == 9)
            {
                char[] charBuffer = new char[9];
                for (int i = 0; i < buffer.Length; i++)
                    charBuffer[i] = Convert.ToChar(buffer[i]);
                pid = Int32.Parse(new String(charBuffer, 0, 8), NumberStyles.HexNumber);

                CreateEvents(pid);

                string fileName = (nameToUse == null || nameToUse == "" ? string.Format("pipe_{0}.log", pid) : nameToUse);
                byte[] fileNameBuffer = new Byte[fileName.Length+1];
                for (int i = 0; i < fileName.Length; i++)
                    fileNameBuffer[i] = (byte)fileName[i];
                fileNameBuffer[fileName.Length] = 0;
                pipe.Write(fileNameBuffer, 0, fileNameBuffer.Length);
                pipe.Flush();
                logFileName = tempDir + "\\" + fileName;
                log = new ReadNewLog(logFileName);
                lastLogResult = null;
                ObjectGraph.cachedGraph = null;
                if (!noUI)
                {
                    EnableDisableViewMenuItems();
                }
                while (true)
                {
                    try
                    {
                        if (pipe.Read(buffer, 0, 1) == 0) // && GetLastError() == 109/*ERROR_BROKEN_PIPE*/)
                        {
                            DisconnectNamedPipe(pipeHandle);
                            break;
                        }
                    }
                    catch (System.IO.IOException)
                    {
                        DisconnectNamedPipe(pipeHandle);
                        break;
                    }
                }
            }
            else
            {
                string error = string.Format("Error {0} occurred", GetLastError());
                MessageBox.Show(error);
            }

            if(!noUI)
            {
                killApplicationButton.Enabled = true;
                showHeapButton.Enabled = true;
            }

            logFileStartOffset = 0;
            logFileEndOffset = long.MaxValue;

            return pid;
        }

        private void startApplicationButton_Click(object sender, System.EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
                return;

            if (processFileName == null)
            {
                profileApplicationMenuItem_Click(null, null);
                return;
            }
            else if (processFileName == "ASP.NET")
            {
                profileASP_NETmenuItem_Click(null, null);
                return;
            }
            else if (serviceName != null)
            {
                ProfileService();
                return;
            }

            RegisterDLL.Register();

            if (processFileName == null)
                return;

            if (profiledProcess == null || ProfiledProcessHasExited())
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(processFileName);
                processStartInfo.EnvironmentVariables["Cor_Enable_Profiling"] = "0x1";
                processStartInfo.EnvironmentVariables["COR_PROFILER"] = "{8C29BC4E-1F57-461a-9B51-1200C32E6F1F}";

                processStartInfo.EnvironmentVariables["OMV_USAGE"] = CreateUsageString();

                processStartInfo.EnvironmentVariables["OMV_SKIP"] = "0";
                string tempDir = GetTempDir();
                processStartInfo.EnvironmentVariables["OMV_PATH"] = tempDir;
                processStartInfo.EnvironmentVariables["OMV_STACK"] = "1";
                processStartInfo.EnvironmentVariables["OMV_FORMAT"] = "v2";
                processStartInfo.EnvironmentVariables["OMV_DynamicObjectTracking"] = "0x1";
                processStartInfo.EnvironmentVariables["OMV_FORCE_GC_ON_COMMENT"] = gcOnLogFileComments ? "1" : "0";
                processStartInfo.EnvironmentVariables["OMV_INITIAL_SETTING"] = CreateInitialString();

                if (commandLine != null)
                    processStartInfo.Arguments = commandLine;

                if (workingDirectory != null)
                    processStartInfo.WorkingDirectory = workingDirectory;

                processStartInfo.UseShellExecute = false;

                profiledProcess = Process.Start(processStartInfo);

                WaitForProcessToConnect(tempDir, "Waiting for application to start common language runtime");
            }
        }

        private ReadLogResult GetLogResult()
        {
            ReadLogResult readLogResult = lastLogResult;
            if (readLogResult == null)
            {
                readLogResult = new ReadLogResult();
            }
            readLogResult.liveObjectTable = new LiveObjectTable(log);
            readLogResult.sampleObjectTable = new SampleObjectTable(log);
            readLogResult.allocatedHistogram = new Histogram(log);
            readLogResult.callstackHistogram = new Histogram(log);
            readLogResult.relocatedHistogram = new Histogram(log);
            readLogResult.finalizerHistogram = new Histogram(log);
            readLogResult.criticalFinalizerHistogram = new Histogram(log);
            readLogResult.createdHandlesHistogram = new Histogram(log);
            readLogResult.destroyedHandlesHistogram = new Histogram(log);
            if (readLogResult.objectGraph != null)
                readLogResult.objectGraph.Neuter();
            readLogResult.objectGraph = new ObjectGraph(log, 0);
            readLogResult.functionList = new FunctionList(log);
            readLogResult.hadCallInfo = readLogResult.hadAllocInfo = false;
            readLogResult.handleHash = new Dictionary<ulong, HandleInfo>();

            // We may just have turned a lot of data into garbage - let's try to reclaim the memory
            GC.Collect();

            return readLogResult;
        }

        private void ToggleEvent(NamedManualResetEvent toggleEvent, NamedManualResetEvent toggleEventCompleted)
        {
            if (profiledProcess != null && !ProfiledProcessHasExited() || logFileName != null)
            {
                if (toggleEvent != null)
                {
                    toggleEvent.Set();
                    if (toggleEventCompleted.Wait(10*1000))
                        toggleEventCompleted.Reset();
                    else
                        MessageBox.Show("There was no response from the application");
                }
            }
        }

        private void profilingActiveCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            if(allocationsCheckBox.Checked)
            {
                ToggleEvent(loggingActiveEvent, loggingActiveCompletedEvent);
            }
            if(callsCheckBox.Checked)
            {
                ToggleEvent(callGraphActiveEvent, callGraphActiveCompletedEvent);
            }

            // disable check boxes if profiling is turned off
            allocationsCheckBox.Enabled = callsCheckBox.Enabled = profilingActiveCheckBox.Checked;

            if (profiledProcess != null && !ProfiledProcessHasExited() || logFileName != null)
            {
                Stream s = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long offset = s.Length;
                s.Close();
                if (profilingActiveCheckBox.Checked)
                    logFileStartOffset = offset;
                else
                {
                    logFileEndOffset = offset;
                    if (logFileStartOffset >= logFileEndOffset && profiledProcess != null && !ProfiledProcessHasExited())
                        MessageBox.Show("No new data found", "",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                if (!profilingActiveCheckBox.Checked && logFileStartOffset < logFileEndOffset && logFileName != null)
                {
                    ReadLogResult readLogResult = GetLogResult();
                    log.ReadFile(logFileStartOffset, logFileEndOffset, readLogResult);
                    lastLogResult = readLogResult;
                    EnableDisableViewMenuItems();
                    viewSummaryMenuItem_Click(null, null);
                }
            }
        }

        private void allocationsCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            ToggleEvent(loggingActiveEvent, loggingActiveCompletedEvent);
        }

        private void callsCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            ToggleEvent(callGraphActiveEvent, callGraphActiveCompletedEvent);
        }

        private void checkProcessTimer_Tick(object sender, System.EventArgs e)
        {
            bool processRunning = profiledProcess != null && !ProfiledProcessHasExited();

            killApplicationButton.Enabled = processRunning;
            showHeapButton.Enabled = processRunning;

            if (profiledProcess != null && ProfiledProcessHasExited())
            {
                profiledProcess = null;
                logFileEndOffset = int.MaxValue;
                if (log != null)
                {
                    ReadLogResult readLogResult = GetLogResult();

                    log.ReadFile(logFileStartOffset, logFileEndOffset, readLogResult);
                    lastLogResult = readLogResult;
                    EnableDisableViewMenuItems();
                    ClearEvents();
                    saveAsMenuItem.Enabled = true;
                    viewSummaryMenuItem_Click(null, null);
                }
            }
        }

        private void killApplicationButton_Click(object sender, System.EventArgs e)
        {
            if (profiledProcess != null)
            {
                if (killApplicationButton.Text == "Kill ASP.NET")
                {
                    StopIIS();
                    StartIIS();
                }
                else if (serviceName != null)
                {
                    StopService(serviceName, serviceStopCommand);
                }
                else
                {
                    profiledProcess.Kill();
                }
            }
        }

        private void setCommandLineMenuItem_Click(object sender, System.EventArgs e)
        {
            SetParameterForm setCommandLineForm = new SetParameterForm();
            setCommandLineForm.commandLineTextBox.Text = commandLine;
            setCommandLineForm.workingDirectoryTextBox.Text = workingDirectory;
            setCommandLineForm.logDirectoryTextBox.Text = GetTempDir();
            setCommandLineForm.gcOnLogFileCommentsCheckBox.Checked = gcOnLogFileComments;
            if (setCommandLineForm.ShowDialog() == DialogResult.OK)
            {
                commandLine = setCommandLineForm.commandLineTextBox.Text;
                workingDirectory = setCommandLineForm.workingDirectoryTextBox.Text;
                logDirectory = setCommandLineForm.logDirectoryTextBox.Text;
                gcOnLogFileComments = setCommandLineForm.gcOnLogFileCommentsCheckBox.Checked;
            }
        }

        private long logFileOffset()
        {
            Stream s = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long offset = s.Length;
            s.Close();

            return offset;
        }

        private void showHeapButton_Click(object sender, System.EventArgs e)
        {
            forceGcCompletedEvent.Wait(1);
            forceGcCompletedEvent.Reset();
            long startOffset = logFileOffset();
            forceGcEvent.Set();
            const int maxIter = 10; // give up after ten minutes
            for (int iter = 0; iter < maxIter; iter++)
            {
                long lastOffset = logFileOffset();
                if (forceGcCompletedEvent.Wait(60*1000))
                {
                    forceGcCompletedEvent.Reset();
                    long saveLogFileStartOffset = logFileStartOffset;
                    logFileStartOffset = startOffset;
                    logFileEndOffset = logFileOffset();
                    ReadLogResult logResult = GetLogResult();
                    readLogFile(log, logResult, processFileName, Graph.GraphType.HeapGraph);
                    lastLogResult = logResult;
                    EnableDisableViewMenuItems();
                    logFileStartOffset = saveLogFileStartOffset;
                    break;
                }
                else
                {
                    // Hmm, the app didn't get back to us in 60 seconds
                    // If the log file is growing, assume the app is still dumping
                    // the heap, otherwise something is obviously wrong.
                    if (logFileOffset() == lastOffset)
                    {
                        MessageBox.Show("There was no response from the application");
                        break;
                    }
                }
            }
        }

        private void viewByAddressMenuItem_Click(object sender, System.EventArgs e)
        {
            ViewByAddressForm viewByAddressForm = new ViewByAddressForm();
            viewByAddressForm.Visible = true;
        }

        private void viewTimeLineMenuItem_Click(object sender, System.EventArgs e)
        {
            TimeLineViewForm timeLineViewForm = new TimeLineViewForm();
            timeLineViewForm.Visible = true;
        }

        private void viewHistogram_Click(object sender, System.EventArgs e)
        {
            HistogramViewForm histogramViewForm = new HistogramViewForm();
            histogramViewForm.Visible = true;       
        }

        private void saveAsMenuItem_Click(object sender, System.EventArgs e)
        {
            SaveFile();
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !CheckProcessTerminate() || !CheckFileSave();
        }

        private void viewHistogramRelocatedMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string title = "Histogram by Size for Relocated Objects";
                HistogramViewForm histogramViewForm = new HistogramViewForm(lastLogResult.relocatedHistogram, title);
                histogramViewForm.Show();
            }
        }

        private void viewHistogramFinalizerMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string title = "Histogram by Size for Finalized Objects";
                HistogramViewForm histogramViewForm = new HistogramViewForm(lastLogResult.finalizerHistogram, title);
                histogramViewForm.Show();
            }
        }

        private void viewHistogramCriticalFinalizerMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string title = "Histogram by Size for Critical Finalized Objects";
                HistogramViewForm histogramViewForm = new HistogramViewForm(lastLogResult.criticalFinalizerHistogram, title);
                histogramViewForm.Show();
            }
        }

        private void viewAgeHistogram_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string title = "Histogram by Age for Live Objects";
                AgeHistogram ageHistogram = new AgeHistogram(lastLogResult.liveObjectTable, title);
                ageHistogram.Show();
            }
        }

        private void viewAllocationGraphmenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.AllocationGraph);
        }

        private void viewAssemblyGraphmenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.AssemblyGraph);
        }

        private void viewHeapGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.HeapGraph);       
        }

        private void viewCallGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.CallGraph);
        }
       
        private void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                startApplicationButton_Click(null, null);
            }
        }

        private void profileServiceMenuItem_Click(object sender, System.EventArgs e)
        {
            ProfileServiceForm profileServiceForm = new ProfileServiceForm();
            if (profileServiceForm.ShowDialog() == DialogResult.OK)
            {
                serviceName         = profileServiceForm.serviceNameTextBox.Text;
                serviceStartCommand = profileServiceForm.startCommandTextBox.Text;
                serviceStopCommand  = profileServiceForm.stopCommandTextBox.Text;

                ProfileService();
            }
        }

        private void viewFunctionGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.FunctionGraph);
        }

        private void viewModuleGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.ModuleGraph);
        }

        private void viewClassGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.ClassGraph);      
        }

        private void viewCommentsMenuItem_Click(object sender, System.EventArgs e)
        {
            if (log != null && log.commentEventList.count != 0)
            {
                ViewCommentsForm viewCommentsForm = new ViewCommentsForm(log);
                viewCommentsForm.Visible = true;
            }
        }

        private void viewCallTreeMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null && lastLogResult.hadCallInfo)
            {
                CallTreeForm callTreeForm = new CallTreeForm(log.fileName, lastLogResult);
            }
        }

		private void viewComparisonMenuItem_Click(object sender, System.EventArgs e)
		{
			currlogFileName = this.logFileName;
			
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			openFileDialog1.FileName = "*.log";
			openFileDialog1.Filter = "Allocation Logs | *.log";
			if (openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.CheckFileExists)
			{
				prevlogFileName = openFileDialog1.FileName;
			}
			this.runaswindow = true;
			graphtype = Graph.GraphType.AllocationGraph;
			try
			{
				ReportForm _MgrForm = new ReportForm(this);
				_MgrForm.Visible = true;
			}
			catch
			{
				/* errors already told user, so continue. */
			}
		}

        private void viewSummaryMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string scenario = log.fileName;
                if (processFileName != null)
                    scenario = processFileName + " " + commandLine;
                SummaryForm summaryForm = new SummaryForm(log, lastLogResult, scenario);
                summaryForm.Show();
            }
        }
    }
}
