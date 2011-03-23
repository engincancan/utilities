


MyUninstaller v1.36
Copyright (c) 2003 - 2007 Nir Sofer
Web Site: http://www.nirsoft.net



Description
===========

MyUninstaller is an alternative utility to the standard Add/Remove applet
of Windows operating system. It displays the list of all installed
application, and allows you to uninstall an application, delete an
uninstall entry, and save the list of all installed applications into a
text file or HTML file.
MyUninstaller also provides additional information for most installed
applications that the standard Add/Remove applet doesn't display: product
name, company, version, uninstall string, installation folder and more.



Important Notice
================

When you uninstall a software, MyUninstaller utility is not directly
responsible to the uninstall process. MyUninstaller simply run the
uninstall module provided by the software that you want to uninstall. So,
if from some reason the uninstall process fails, you should contact the
author of the software that you want to uninstall, not to the author of
MyUninstaller.



Versions History
================




04/08/2007
1.36

* Added "Change Software Installation" option, for programs that
  support installation change.


23/05/2007
1.35

* New option: Show Windows Updates. If you have too much entries of
  Microsoft security updates and hotfixes, uncheck this option in order
  to hide all these updates.


25/03/2006
1.34

* Fixed bug: System error message is displayed if one or more programs
  are installed on removable drive that cannot be accessed.


14/05/2005
1.33

* Added support for Windows XP visual styles.


12/04/2005
1.32

* New option: Analyze Special Shortcuts. (In previous versions, this
  option was always turned on). If during loading process of
  MyUninstaller, one or more setup programs starts with no reason, you
  should turn this option off in order to solve this problem.
* New option: Open Entry In RegEdit


26/03/2005
1.31

* Fixed F8/F9 keys problem


26/10/2004
1.30

* New Column: Installer. Displays the name of the installer software
  that is used to install/uninstall the program.
* New Column: Root Key.
* New Option: Don't Load Icons. If you use this option, the icons of
  each uninstall entry won't be loaded, but MyUninstaller will load the
  entries faster than before.
* New Option: Mark New Installations. Mark all new installations with
  yellow color. The mark is removed after making a refresh (F5). Works
  only in quick mode.
* New Option: Advanced Mode - for power users only !!
  Allows you to delete and uninstall multiple items at once.
* New Option: Select another font. Allows you to view all uninstall
  entries in any font you like.
* Fixed translation problems: filters list, N/A string
* A little improvement in analyzing algorithm.


23/03/2004
1.20

* Quick Mode: Allows you to load MyUninstaller much faster, by saving
  all installed software information into a special ini file, and load it
  when it needed.
* New column: 'Registry Key'. Displays the name of the uninstall
  Registry key.
* Translation: Allows you to easily translate MyUninstaller to other
  languages.


03/11/2003
1.11

* MyUninstaller now also displays the installed software of the current
  logged on user (under HKEY_CURRENT_USER key)


20/10/2003
1.10

* Find dialog-box - allows you to easily find the uninstall entry that
  you are looking for.
* When you save the installed applications information to text/HTML
  file, the order of fields in the saved file is the same as the order of
  columns that you select to display.
* View HTML report of installed applications.
* Choose Columns Dialog-Box.
* Save installed applications information to XML file.
* Added support for quiet uninstall. (QuietUninstallString in the
  Registry)


07/06/2003
1.00
First Release



License
=======

This utility is released as freeware. You are allowed to freely
distribute this utility via floppy disk, CD-ROM, Internet, or in any
other way, as long as you don't charge anything for this. If you
distribute this utility, you must include all files in the distribution
package, without any modification !



Disclaimer
==========

The software is provided "AS IS" without any warranty, either expressed
or implied, including, but not limited to, the implied warranties of
merchantability and fitness for a particular purpose. The author will not
be liable for any special, incidental, consequential or indirect damages
due to loss of data or any other reason.



Using MyUninstaller
===================

The MyUninstaller utility is a standalone executable. It doesn't require
any installation process or additional DLL's. Just copy the executable
file to any folder you want, and run it.
After your run it, MyUninstaller loads the list of the installed
applications from the Registry, and gathers information about each
installation item. The loading and analyzing process might take between 5
and 15 seconds and even more, depending on your hardware, operating
system, and the quantity of the installed application on your computer.
After the analyzing process is finished, MyUninstaller displays the
details of all installed applications on your computer. Be aware that
some columns, like version, product name, company and others, are
displayed for most installed applications, but not for all of them. In
rare cases, It's also possible that wrong information about an installed
application will be displayed.
The following table provides basic information about each available
column:




Entry Name
The entry name of the installed application as it appears in the Registry.

Registry Key
The key name of the installed application in the Registry.

Product Name
The name of the product. This information is extracted from the
executable of the installed application.

Version
The version of the product. This information is extracted from the
executable or from the Registry entry of the installed application.

Company
The company or the author that created the application. This information
is extracted from the executable or from the Registry entry of the
installed application.

Description
The description of the product. This information is extracted from the
executable of the installed application.

Obsolete
If the value in this column is "Yes", the uninstall entry is probably
obsolete, and the application is no longer installed on your computer.

Uninstall
Tells you whether the application provides an uninstall support. If the
value in this column is "No", you cannot uninstall the application.
entries with no uninstall support are displayed only if you check the
"Show Items With No Uninstall" option in the View menu.

Installation Folder
The folder that contains the installed application.

Web Site
The Web site of the installed application. Be aware that most
applications do not provide this information.

Installation Date
This column is only available in Windows NT, Windows 2000 and Windows XP.
It shows the date that the application has been installed. Be aware that
this value is taken from Registry key of the installed application. If
you modify something in the Registry key of the installed application,
this column may contain wrong installation date.

Uninstall String
The uninstall string as it appears in the Registry.

Quiet Uninstall
Specifies whether this item supports Quiet Uninstall option. Be aware
that most installed software do not support this option.

Installer
Specifies the installer software that is used to install/uninstall the
program. MyUninstaller can identify the following installers: Windows
Installer, Inno Setup, Wise Installer, InstallShield, MindVision VISE
Installer, Ghost Installer, Nullsoft Install System, Gentee Installer,
and ZipInstaller.

Root Key
Specifies the root key of the uninstall entry. most programs add the
uninstall entries under HKEY_LOCAL_MACHINE, but there are some programs
that also allows you to install for the current user - under
HKEY_CURRENT_USER.

You can change the location of a column by dragging the column header to
another location. You can also increase or decrease the width of each
column. these settings (the location and the width of all columns) are
automatically saved for you and loaded in the next time that you run the
MyUninstaller utility.



Uninstalling an application
===========================

In order to uninstall an application, select the desired entry, and from
the File menu (or from the pop-up menu) select "Uninstall Selected
Software". The uninstall applet of the selected application will be
launched instantly



Removing an uninstall entry
===========================

In some circumstances, an application is not removed properly and the
uninstall entry remains on your system. If you find an obsolete uninstall
entry of an application that is no longer installed on your computer, you
can remove the entry from the installed applications list. In order to
remove an uninstall entry, select the item that you want to remove, and
from the File menu (or from the pop-up menu), select "Delete Selected
Entry".



Quick Mode
==========

By default, each time that you run MyUninstaller utility, it
automatically scans and analyzes the installed applications on your
computer. On computer with dozens of installed applications, this loading
process may be quite slow. If you want to load MyUninstaller in much
faster way, turn on the quick mode option. When quick mode is enabled,
the information gathered by MyUninstaller is saved into a special INI
file under your 'Application Data' folder, and in the next time you run
MyUninstaller, the installed applications information will be loaded from
that INI file, instead of reanalyzing your system. However, if you have
installed new software since the last time you ran MyUninstaller, you
have to refresh the software list by pressing F5 key.

In order to enable or disable the quick mode, select the 'Quick Mode'
option from the View menu.



Advanced Mode
=============

When Advanced Mode is turned on, you are allowed to select multiple
items, and then delete or uninstall them at once. This option is turned
off by default, and you should not turn it on, unless you're really a
power user.



Translating MyUninstaller to other languages
============================================

Starting from version 1.20, you are allowed to easily translate
MyUninstaller to other language.
In order to do that, follow the instructions below:
1. Run MyUninstaller with /savelangfile parameter:
   myuninst.exe /savelangfile
   a file named 'myuninst_lng.ini' will be created in the folder of
   MyUninstaller.
2. Open the created language file in Notepad or in any other text
   editor.
3. Translate all menus, dialog-boxes, and string entries to the
   desired language. Optionally, you can also add your name and/or a link
   to your Web site. (TranslatorName and TranslatorURL values) If you add
   this information, it'll be used in the 'About' window.
4. After you finish the translation, Run MyUninstaller, and all
   translated strings will be loaded from the language file.
   If you want to run MyUninstaller without the translation, simply
   rename the language file, or move it to another folder.



Command-line options
====================



/stext <Filename>
Save the list of all installed applications into a regular text file.

/stab <Filename>
Save the list of all installed applications into a tab-delimited text
file.

/stabular <Filename>
Save the list of all installed applications into a tabular text file.

/shtml <Filename>
Save the list of all installed applications into HTML file.

/sxml <Filename>
Save the list of all installed applications to XML file.

/NoLoadSettings
Run MyUninstaller without loading your last settings. (window position,
columns size, etc. )




Feedback
========

If you have any problem, suggestion, comment, or you found a bug in my
utility, you can send a message to nirsofer@yahoo.com


