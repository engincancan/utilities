using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;

// Creates a type library from an assembly
namespace ComComparer 
{
	public class Exporter : ITypeLibExporterNotifySink
	{
		private TypeLibConverter converter;

		// Constructor
		public Exporter()
		{
			this.converter = new TypeLibConverter();
		}

		// Creates a type library with the given filename, library name,
		// and help file from the assembly specified by assemblyFilename
		public UCOMITypeLib Export(string assemblyFilename, string tlbFilename)
		{
			Assembly assembly;

			try
			{

				// Load the assembly
				assembly = Assembly.LoadFrom(assemblyFilename);
			}
			catch
			{
				throw new ApplicationException("Unable to load the assembly '" +
					assemblyFilename + "'.");
			}

			return Export(assembly, tlbFilename);
		}

		// Creates a type library with the given filename, library name,
		// and help file from the loaded input assembly
		public UCOMITypeLib Export(Assembly assembly, string tlbFilename)
		{
			UCOMITypeLib returnTypeLib;

			if (tlbFilename == null)
				tlbFilename = assembly.GetName().Name + ".tlb";

			// Create a type library from the assembly
			returnTypeLib = (UCOMITypeLib)converter.ConvertAssemblyToTypeLib(
				assembly,
				tlbFilename,
				0,
				this);

			return returnTypeLib;
		}

		// Implementation of ITypeLibExporterNotifySink.ReportEvent
		public void ReportEvent(ExporterEventKind eventKind, int eventCode, 
			string eventMsg)
		{
			// Get the MessageType for the corresponding ExporterEventKind
			if (eventKind == ExporterEventKind.ERROR_REFTOINVALIDASSEMBLY)
				throw new ApplicationException(eventMsg);
		}

		// Implementation of ITypeLibExporterNotifySink.ResolveRef
		public object ResolveRef(Assembly assembly)
		{
			// Export the assembly
			return Export(assembly, null);
		}
	}
}