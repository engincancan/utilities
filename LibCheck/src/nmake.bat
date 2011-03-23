del *.dll,*.pdb,*.exe

pushd SigHelper
call nmake.bat
popd

pushd ChurnReports
call nmake.bat
popd

pushd ComCompat
call nmake.bat
popd

copy SigHelper\SigHelper.dll SigHelper.dll
copy ChurnReports\ChurnReports.dll ChurnReports.dll
copy ComCompat\ComCompat.dll ComCompat.dll

csc /r:SigHelper.dll /r:ChurnReports.dll /r:ComCompat.dll /r:System.dll /r:System.Web.Services.DLL /r:System.Data.dll /out:LibCheck.exe /w:4 LibCheck.cs Owners2.cs TypeMember.cs ReflectorDO.cs DataObject.cs xmlreport.cs