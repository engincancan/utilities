call "C:\Program Files\Microsoft Visual Studio .NET 2003\Common7\Tools\vsvars32.bat"
csc /target:exe /out:lessmsi.exe /reference:wix.dll *.cs 

