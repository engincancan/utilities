using System;
using System.Collections;

/// <summary>
/// Type library compatibility checker.
/// </summary>
namespace ComComparer 
{
	public class ComCompat
	{
		static string oldTlbName = null;
		static string newTlbName = null;
		static AssemblyComparer comparer = null;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		public static int RunComCompat(string oldAssembly, string newAssembly,
			out ArrayList errors, out ArrayList warnings, out ArrayList otherDiffs)
		{
			// not needed in WinChurn
			// PrintHeader();

			// not needed in WinChurn
			/*
			if (args.Length <= 1)
			{
				PrintUsage();
				return (int)ReturnCodes.Help;
			}
			else
			{
			*/
			oldTlbName = oldAssembly;
			newTlbName = newAssembly;
			//}

			try
			{

				comparer = new AssemblyComparer(oldTlbName, newTlbName);

				comparer.CheckCompatibility();
				errors = comparer.Errors;
				warnings = comparer.Warnings;
				otherDiffs = comparer.OtherDiffs;

				//			foreach (object o in comparer.Errors)
				//				Console.WriteLine(o);
				/*
							if (args.Length > 2 && (args[2] == "/errorsonly" || args[2] == "-errorsonly"))
							{
								foreach (object o in comparer.Warnings)
									Console.WriteLine(o);

								foreach (object o in comparer.OtherDiffs)
									Console.WriteLine(o);
							}
				*/
				//			comparer.PrintSummary();
			}
			catch (ApplicationException e)
			{
				errors = null;
				warnings = null;
				otherDiffs = null;
				PrintError(e.Message);
				return (int)ReturnCodes.FatalError;
			}

			// Return a code representing the most severe thing that happens
			if (comparer.Errors.Count > 0)
				return (int)ReturnCodes.CompatErrors;
			else if (comparer.Warnings.Count > 0)
				return (int)ReturnCodes.CompatWarnings;
			else if (comparer.OtherDiffs.Count > 0)
				return (int)ReturnCodes.CompatOtherDiffs;
			else
				return (int)ReturnCodes.Success;
		}

		/// <summary>
		/// Print a description of using this utility.
		/// </summary>
		private static void PrintUsage()
		{
			Console.WriteLine("Syntax: ComCompat <OldAssembly> <NewAssembly> [Options]");
			Console.WriteLine("");
			Console.WriteLine("This tool ensures that the COM APIs exposed by NewAssembly");
			Console.WriteLine("are compatible with the COM APIs exposed by OldAssembly.");
			Console.WriteLine("");
			Console.WriteLine("Options:");
			Console.WriteLine("    /errorsonly     Don't display individual warnings or other differences");
			Console.WriteLine("");
			Console.WriteLine("It produces three kinds of messages:");
			Console.WriteLine("");
			Console.WriteLine("  1. Errors - changes that break compatibility.");
			Console.WriteLine("  2. Warnings - changes that could break compatibility, based on installation.");
			Console.WriteLine("                These are only given for GUID changes of non-interfaces.");
			Console.WriteLine("  3. Other Diffs - minor changes should not break compatibility.");
			Console.WriteLine("");
			Console.WriteLine("It determines that NewAssembly is compatible with OldAssembly if:");
			Console.WriteLine("");
			Console.WriteLine("  1. NewAssembly has at least all the types that OldAssembly has, with the");
			Console.WriteLine("     same names and mostly the same flags.  A change in GUID for a");
			Console.WriteLine("     non-interface only produces a warning, because it can be compatible if");
			Console.WriteLine("     the old types are still registered under their old GUIDs, and policy");
			Console.WriteLine("     redirects requests for the old types to the new types.");
			Console.WriteLine("  2. All interfaces in NewAssembly have the same IIDs as they do in OldAssembly.");
			Console.WriteLine("  3. All coclasses from OldAssembly list at least the same implemented");
			Console.WriteLine("     interfaces and source interfaces in NewAssembly, and have the same");
			Console.WriteLine("     default interfaces.");
			Console.WriteLine("  4. Interfaces from OldAssembly look identical in NewAssembly (although");
			Console.WriteLine("     parameter names are allowed to change case), ignoring help-related info");
			Console.WriteLine("     such as helpstrings and ignoring IDL custom attributes.");
			Console.WriteLine("     (An error also is reported if dispinterface methods change ordering,");
			Console.WriteLine("      which is more strict than necessary.)");
		}

		/// <summary>
		/// Print the logo.
		/// </summary>
		private static void PrintHeader()
		{
			Console.WriteLine("Type Library Compatibility Verifier 1.0.0215.0");
			Console.WriteLine("Contact anathan with any questions/comments.");
		}

		/// <summary>
		/// Print a fatal error.
		/// </summary>
		/// <param name="message">The message to print</param>
		private static void PrintError(string message)
		{
			//COMMENTED OUT BY KIT. No console output in this location
			//Console.WriteLine("FATAL ERROR: " + message);
		}
	}

	internal enum ReturnCodes
	{
		Help = 1,
		Success = 2,
		FatalError = 3,
		CompatErrors = 4,
		CompatWarnings = 5,
		CompatOtherDiffs = 6
	}
}