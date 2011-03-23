IronRuby in Silverlight


== Generating an application

script/sl ruby MyApp

Will create "MyApp" in the current directory. 


== Run an app

script/chr /b:MyApp\index.html

This will launch a browser pointed at the application.


== Running Samples

script/chr /b

Will open a browser pointed at the "/silverlight" directory. Navigate to the 
any Silverlight sample in the "/silverlight/samples" directory 
(index.html file).

For more information on the "chr" script, run "script/chr" for help.


== Package

  /bin              IronRuby and IronPython binaries for Silverlight
  /samples          Samples for IronRuby and IronPython in Silverlight
  /script           "chr" and "sl" script
  README.txt        This file

== License

Read the License.* files at the root of this release.
