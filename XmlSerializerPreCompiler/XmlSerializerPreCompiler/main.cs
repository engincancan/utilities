// main.cs: An XmlSerializer debugging tool
// Author: Chris Sells [csells@microsoft.com]
#region Copyright
// Copyright (C) 2003 Microsoft Corporation
// All rights reserved.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF
// MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
#endregion
#region TODO
// -Add support for pulling out the XmlSerializer-generated assembly
//  so that it doesn't have to be compiled again at run-time
#endregion
#region History
// 6/18/03: Initial release
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;

class XmlSerializerPreCompiler {
  static bool Empty(string s) { return s.Length == 0 || s == string.Empty; }

  static int Main(string[] args) {
    // Check usage
    string usage =
      "usage: XmlSerializerPreCompiler.exe [/?] <assemblyFileName> <typeName>\n" +
      "The XmlSerializerPreCompiler tools checks to see if a type can be\n" +
      "serialized by the XmlSerializer class and if it can't, shows the\n" +
      "errors so that the type can be modified.";
    if( args.Length != 2 || (args.Length == 1 && (args[0] == "/?" || args[0] == "-?")) ) {
      Console.WriteLine(usage);
      return 1;
    }

    // Load type
    string assemblyFileName = args[0];
    string typeName = args[1];
    Assembly assem = null;
    Type type = null;

    try {
      string assemblyFullPath = Path.GetFullPath(assemblyFileName);
      assem = Assembly.LoadFile(assemblyFullPath);
      type = assem.GetType(typeName, true, true);
    }
    catch( Exception ex ) {
      Console.WriteLine("Error: " + ex.Message);
      Console.WriteLine(usage);
      return 1;
    }

    // Attempt to serialize, showing errors if it fails
    try {
      XmlSerializer serializer = new XmlSerializer(type);
    }
    catch( Exception ex ) {
      Console.WriteLine("Error: " + ex.Message);

      // HACK: Pull our assembly base file name from exception message
      Regex regex = new Regex(@"File or assembly name (?<baseFileName>.*).dll");
      Match match = regex.Match(ex.Message);
      string baseFileName = match.Groups["baseFileName"].Value;
      if( Empty(baseFileName) ) return 1;

      string outputPath = Path.Combine(Path.GetTempPath(), baseFileName + ".out");
      Console.WriteLine((new StreamReader(outputPath)).ReadToEnd());
      Console.WriteLine();

      string csPath = Path.Combine(Path.GetTempPath(), baseFileName + ".0.cs");
      Console.WriteLine("XmlSerializer-produced source:\n" + csPath);
      return 1;
    }

    Console.WriteLine("No errors producing an XmlSerializer using {0} from {1}", typeName, assemblyFileName);
    return 0;
  }

}




