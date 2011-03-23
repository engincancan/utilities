using System;
using System.IO;
using System.Diagnostics;

public class Run {

   public static void Main (string [] args) {
     
        string newVer;
	   string version = null;
        if (args.Length != 0) newVer = args[0];
        else {
		
           // version= Environment.Version.ToString();
	   using (StreamReader s1 = new StreamReader ("version.txt")){
   	      version = s1.ReadLine();
           }

           //string [] v = version.Split(new char [] {'.'});
           int bld = Int32.Parse(version);
	    version = bld.ToString();
           bld++;
//           newVer = v[0] + "." + v[1] + "." + bld.ToString() + "." + v[3] ;
             newVer = bld.ToString();
        }
	StreamWriter w = new StreamWriter ("version.txt");
	w.WriteLine (newVer);   
	w.Close();
        Console.WriteLine (newVer);
	//Console.ReadLine ();
	Process.Start (@"run.bat", version +","+newVer);
        
        

   }

}