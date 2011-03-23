// DebugMonitor.cs
// Author: Adam Nathan
// Date: 5/11/2003
// For more information, see http://blogs.gotdotnet.com/anathan
//
// Retrieves messages passed to OutputDebugString from all processes
// that are not running under a debugger.  See the DBMon SDK sample
// for more details.
using System;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace ClrSpy
{
	// Delegate for the DebugMonitor.Message event
	public delegate void DebugMessageEventHandler(object sender, DebugMessageEventArgs e);

	public class DebugMessageEventArgs : EventArgs
	{
		public DebugMessageEventArgs(string message, int pid)
		{
			Message = message;
			ProcessID = pid;
		}

		public string Message;
		public int ProcessID;
	}

	public class DebugMonitor
	{
		private IntPtr pPID;
		private IntPtr pMessage;
		private IntPtr bufferEvent;
		private IntPtr dataEvent;
		private Thread monitorThread;
	
		public event DebugMessageEventHandler Message;

		public DebugMonitor()
		{
			SECURITY_ATTRIBUTES attributes = new SECURITY_ATTRIBUTES();
			SECURITY_DESCRIPTOR descriptor = new SECURITY_DESCRIPTOR();
		
			IntPtr sharedFile = IntPtr.Zero;
			IntPtr pDescriptor = IntPtr.Zero;
			IntPtr pAttributes = IntPtr.Zero;
			try
			{
				pDescriptor = Marshal.AllocHGlobal(Marshal.SizeOf(descriptor));
				pAttributes = Marshal.AllocHGlobal(Marshal.SizeOf(attributes));

				attributes.nLength = Marshal.SizeOf(attributes);
				attributes.bInheritHandle = true;
				attributes.lpSecurityDescriptor = pDescriptor;

				if (!InitializeSecurityDescriptor(ref descriptor, 1 /*SECURITY_DESCRIPTOR_REVISION*/))
					throw new ApplicationException("InitializeSecurityDescriptor failed: " + Marshal.GetLastWin32Error());

				if (!SetSecurityDescriptorDacl(ref descriptor, true, IntPtr.Zero, false))
					throw new ApplicationException("SetSecurityDescriptorDacl failed: " + Marshal.GetLastWin32Error());

				Marshal.StructureToPtr(descriptor, pDescriptor, false);
				Marshal.StructureToPtr(attributes, pAttributes, false);

				bufferEvent = CreateEvent(pAttributes, false, false, "DBWIN_BUFFER_READY");

				if (bufferEvent == IntPtr.Zero) 
					throw new ApplicationException("CreateEvent (bufferEvent) failed: " + Marshal.GetLastWin32Error());

				if (Marshal.GetLastWin32Error() == 183 /*ERROR_ALREADY_EXISTS*/)
					throw new AlreadyRunningException();

				dataEvent = CreateEvent(pAttributes, false, false, "DBWIN_DATA_READY");

				if (dataEvent == IntPtr.Zero)
					throw new ApplicationException("CreateEvent (dataEvent) failed: " + Marshal.GetLastWin32Error());

				sharedFile = CreateFileMapping(new IntPtr(-1), pAttributes, 4 /*PAGE_READWRITE*/, 0, 4096, "DBWIN_BUFFER");

				if (sharedFile == IntPtr.Zero) 
					throw new ApplicationException("CreateFileMapping failed: " + Marshal.GetLastWin32Error());
			}
			finally
			{
				Marshal.FreeHGlobal(pDescriptor);
				Marshal.FreeHGlobal(pAttributes);
			}

			IntPtr sharedMemory = MapViewOfFile(sharedFile, 4 /*FILE_MAP_READ*/, 0, 0, 512);

			if (sharedMemory == IntPtr.Zero) 
				throw new ApplicationException("MapViewOfFile failed: " + Marshal.GetLastWin32Error());

			// The first 4 bytes are the process ID, and the remaining bytes are the message
			pPID = sharedMemory;
			pMessage = new IntPtr((long)sharedMemory + 4);
		}

		// Start monitoring on a background thread
		public void Start()
		{
			if (monitorThread != null && monitorThread.IsAlive)
				throw new ApplicationException("Monitoring is already in progress.");
	
			monitorThread = new Thread(new ThreadStart(RunMonitor));
			monitorThread.IsBackground = true; // Don't make thread prevent process exit
			monitorThread.Start();
		}

		// Abort the monitoring thread
		public void Stop()
		{
			if (monitorThread != null) monitorThread.Abort();
		}
	
		// The monitoring method run on the background thread
		private void RunMonitor()
		{
			while (true)
			{
				SetEvent(bufferEvent);

				if (WaitForSingleObject(dataEvent, 0xFFFFFFFF /*INFINITE*/) != 0 /*WAIT_OBJECT_0*/)
					throw new ApplicationException("WaitForSingleObject failed: " + Marshal.GetLastWin32Error());

				try
				{
					if (Message != null) Message(this, new DebugMessageEventArgs(Marshal.PtrToStringAnsi(pMessage), Marshal.ReadInt32(pPID)));
				}
				catch
				{
					// Don't let a failure during the message processing stop the monitoring process
				}
			}
		}

		#region Interop definitions
		private struct SECURITY_DESCRIPTOR 
		{
			internal byte Revision;
			internal byte Sbz1;
			internal short Control;
			internal IntPtr Owner;
			internal IntPtr Group;
			internal IntPtr Sacl;
			internal IntPtr Dacl;
		}

		private struct SECURITY_ATTRIBUTES 
		{
			internal int nLength;
			internal IntPtr lpSecurityDescriptor;
			internal bool bInheritHandle;
		}

		[DllImport("advapi32.dll", SetLastError=true)]
		private static extern bool InitializeSecurityDescriptor([In] ref SECURITY_DESCRIPTOR pSecurityDescriptor, int dwRevision);

		[DllImport("advapi32.dll", SetLastError=true)]
		private static extern bool SetSecurityDescriptorDacl([In] ref SECURITY_DESCRIPTOR pSecurityDescriptor, bool bDaclPresent, IntPtr pDacl, bool bDaclDefaulted);

		[DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
		private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern bool SetEvent(IntPtr hEvent);

		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		[DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
		private static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);
		#endregion
	}

	[Serializable]
	public class AlreadyRunningException : Exception
	{
		public AlreadyRunningException() : base() {}
		public AlreadyRunningException(string message) : base(message) {}
		public AlreadyRunningException(string message, Exception inner) : base(message, inner) {}
		protected AlreadyRunningException(SerializationInfo info, StreamingContext context) : base(info, context) {}
	}
}