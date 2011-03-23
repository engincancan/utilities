
===============================================================================
 Tail for Win32 - a Windows version of the UNIX 'tail -f' command.
 
 Author: Paul Perkins (paul@objektiv.org.uk)
 
 Copyright(c)
 
 This program is free software; you can redistribute it and/or modify it 
 under the terms of the GNU General Public License as published by the Free 
 Software Foundation; either version 2 of the License, or (at your option) 
 any later version.
 
 This program is distributed in the hope that it will be useful, but 
 WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
 or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for 
 more details.
 
 You should have received a copy of the GNU General Public License along with 
 this program; if not, write to the Free Software Foundation, Inc., 59 Temple 
 Place, Suite 330, Boston, MA 02111-1307 USA

 $Id: readme.txt,v 1.26 2006/01/23 14:13:49 paulperkins Exp $
 
===============================================================================

Contents
--------

1. Legal Bits
2. Introduction
3. What Is It?
4. How do I use it?
5. Latest Changes
6. Workspaces
7. Configuration
8. Feedback
9. Known Issues
10. Mailing List.
11. Acknowledgements.

--------------
1. Legal Bits
--------------

See file header.

In summary, Tail is free and so is the source, but I retain the copyright. If 
you wish to modify and give it away, you may do so, but you must give me the 
source changes you made. See the GPL license at 
http://www.opensource.org/licenses/gpl-license.html for full details.


----------------
2. Introduction
----------------

Ever wanted to just type 'tail -f error_log' on Windows?

Envious of your Unix friends that can track changes to a file, in real time, 
as they happen?

Well, now you can with the Objektiv Tail for Win32 you'll be happily monitoring 
your log files like you've never done before.

What's even better, you can track several files all at the same time with the 
patented "Multiple Document Interface"TM technology, with each file having 
its own, fabulous window.


---------------
3. What Is It?
---------------

Tail for Win32 is a Windows version of the Unix 'tail' utility. It can be used
to monitor changes to a text file in real time - ideal for watching error logs.

The program was written in C++ using MFC, and uses Win32 file change 
notifications to monitor when a file changes.

The application uses a 'plugin' architecture, under which all changes are
passed through external DLLs that can perform various functionality.

The first plugin is a MAPI filter. This DLL sends emails via MAPI when a 
keyword is found in the file being monitored. 

--------------------
4. How Do I Use It?
--------------------

Simple - just use 'File', 'Open' and select the file to be monitored. Once open,
the program will re-read any changes and scroll to the bottom of the file, 
keeping the most recent changes visible.

You can even watch many different files at the same time - great for monitoring
the status of your web server.

The new syntax highlighting means that you can look out for particular keywords.
You can also display only those lines containing the keywords. See the 
configuration section later to enable this feature.


------------------
5. Latest Changes
------------------

4.2.12    Backed out (bad) fix for Bug #900921. Fixes bug #1417823.
          Now compiled using Visual Studio 6 (prevents missing MFC7.1 DLL problem).
          Corrected version number (bug #1413493).

4.2.11    Fixed over-highlighting issue (Bug #1000820), where the background 
          highlight stays on forever.
          Implemented feature request #1068560 - do not show messagebox
          when Tail detects that the file has been deleted. Simply update
          the status bar. Thanks, nerochiaro!
          Resized Keyword column in Keyword window (Bug #1019400).
          Resized first segment of status bar to prevent chopping of time (Bug #1368248).
          Set and restore font colour (Bug #900921). (This is only partly fixed...)
          Fixed crash when closing empty workspace (Bug #1337505).
          Do not accept dropped folders (Bug #1344804).

4.2.10    Fixed overwriting of file on Ctrl+S (Bug #968230).
          Fix to Tally window where it would not initialise properly and
          display junk.
          Fixed bug #951860 - command-line no longer working. This also fixes
          bug #931544 - Send To broken. Big thanks to mausoma for the patch!

4.2.9     Fixed memory error on keyword problem (bug #910037). This happened
          when adding a keyword for the very first time, and an update to the 
          tailed file occurs whilst the keyword dialog is up.
          
4.2.8     More work on workspaces. Now reads and writes workspace files.
          Fixed truncating of very long lines (Bug #888936).
          Fixed incorrect colouring of keywords when keyword has 
          spaces (Bug #888934).
          Fixed pause functionality (Bug #872231).
          Added Windows XP styles via manifest file. 
          (<http://www.codeproject.com/cpp/xpstylemfc.asp>)

4.2.7     Tallywindow now repopulates correctly when keywords added or deleted.
          Started work on additional thread for file processing routine. This allows
          the main thread to return to waiting for a file change, meaning that file
          changes are less likely to be missed. (This has not been an issue, it just
          makes for a more efficient design.) [Note, the this code has been disabled.]
          Added word wrap menu item (Request ID 843559). Thanks to Jacob Dall for the 
          suggestion.
          Moved plugin configuration menu item into the Settings menu.
          Moved font menu item into the Settings menu.
          More work on the workspace window. Most options are not yet functioning.
          Added setting of background colour. Thanks to Justin Buist for the suggestion.
          Added setting of highlight colour.
          [Added CSettings class to hold global config. May break the whole app!]

4.2.6     Started work on workspace window. Thanks to Oliver Smith for his
          contribution (See Acknowledgments section). The workspace window
          simply keeps track of opened documents at the moment.
          Added Hans Dietrich to the Acknowledgements section for his contribution.
          Fixed the Tally window (displaying garbage when re-opened).
          Made keyword window populate itself with correct colours selected.

4.2.5     Fixed plugin configuration dialog (not showing plugins!)

4.2.4     Add/remove keywords from the 'Keywords' dialog now works as it should.
          (Albeit with a small memory leak.)

4.2.3     Now uses RichEdit2 control to allow full-line highlighting. Currently 
          only highlights to the end of the string. Will work on highlighting 
          line to edge of window.
          Started work on making app live in system tray on minimise.
          Removed spurious 'Events' menu item.
          New 'hotlist' with grid-like control from Hans Dietrich.
          'Display Hotlist' menu item has been moved to the 'Settings' menu
          and renamed 'Keyword Configuration'.
          Keywords are now stored in the registry - 'keywords.cfg' is no more!
          Keywords can now be selectively enabled, and each keyword can
          selectively fire a plugin.
          Each keyword can now have a different highlighting colour.

4.2.2     New fixes from Warlock:
          o Multiple keywords per line.
          o Scroll to bottom now works in XP.
          o 'Ignore hot items at start' works as advertised.
          o More work on 'large files' functionality.
          o Font size can now be changed.
          o Cleanup of assertion failures.
          o Removed main button bar, as not really of any use.
          Thanks, Warlock!

          My bits:
          Added version logging to debug file.
          Various code cleanups.
          (Pausing seems to be broken again!)		  

4.2.1-2   Added 'Pause' and 'Resume' menu bar. Operates only on active view.
          Now behaves properly if file truncated.
          If file has disappeared, wait patiently.
          Started event log monitoring code - not complete, but sort-of works.

4.2.1-1   Added 'Paused' menu item - you can now (again) stop the app
          from scrolling to the bottom of the file.
          Added Tally Window (thanks to Dave 'PhuptDuck' for that 
          suggestion!).
          The debug log now only gets written to if it already exists. All
          entries will be at the full debug level. Create a file called 
          'tail.log' in the same directory as the main EXE to turn on 
          logging.
          Added drag-and-drop (thanks again to Dave).

4.2.1     First GPL release. Tail is no longer shareware and now includes
          the source!!
          Added very large file support. If the file is larger than a set 
          limit on startup, then the last chunk of the file is read and 
          processed.

To stay informed of changes to Tail, please join the mailing list. See the 
'Mailing List' section for details.


-----------------
6. Workspaces
-----------------

The workspace dialog (tree control in the left-hand pane) is desgined to 
be a collection of files that are normally monitored together. Good 
examples of this would be the Apache access_log and error_log file, 
or the various files that are normally found in /var/log on Unix boxes.

The workspace dialog can contain a number of workspaces, each of which is
a logical grouping of files.

Clicking on a single file within the workspace will bring that file's window
to the top, or clicking on the workspace itself brings all windows contained
within that workspace to the top.

Right-clicking on a workspace item will let you add the file to another 
workspace, or remove it from the current workspace.

Right-clicking a workspace will allow you to do the various operations that 
might make sense... [As you can guess, this has not yet been implemented ;) ]

Eventually, the workspaces will be able to be saved and loaded, keeping their
collections of files.

-----------------
7. Configuration
-----------------

Tail is a fairly simple application. A small number of configuration 
settings exist. These are as follows:


Polling Sleep Period
--------------------

This registry key controls the time that tail will sleep for before
checking if a file has changed. (Note, that this setting only applies
to files that are not on a local hard disc. Tail will normally use
Win32 file change notifications to detect changes - a much more 
efficient way to do it!) The setting is in miliseconds.
The key lives at 'HKCU\Software\Objektiv\Tail for Win32\Settings\Timeout'.

Other Registry Entries
----------------------

The only other registry entries are used to store the position of the
window, and the state of the tick-boxes against some menu items.
All keys live at 'HKCU\Software\Objektiv\Tail for Win32\'.


------------
8. Feedback
------------

Please mail me with any bugs, comments or other requests at:

	paul@objektiv.org.uk

or visit the tail website at

	http://www.sourceforge.net/projects/tailforwin32

and leave a message in the forum or in the bug tracker.

---------------
9. Known Issues
---------------

o The Maximize, Minimize and Close button disappear when a file is
  opened that was last opened maximised. I'm not sure why this is!

o SAMBA was a major sticking point. Due to the way that SAMBA
flushes files to disc, I may miss changes. 

o Unix files. The Unix line ends are different to Windows, and
sometimes fool the highlighting functionality. Please let me know
if you see this and, if possible, please do send me the file you're
tailing so that I can work out what's going on.

o Some menu items do not maintain their state between sessions.

o Odd 'xxx in a pickle' error messages. Whenever these occur, please
  let me know.

o Various applications do not seem to flush log files to the filesystem
  in a way that I can detect. This seems to affect IIS in particular.

o Small memory leak in Keyword configuration dialog.

-----------------
10. Mailing List.
-----------------

Please join the mailing list. You will be kept up to date with all new 
developments and code-fixes as they happen.

Even if you find that Tail doesn't quite fit your needs, join the list to 
hear when that new feature is added that will make it worth your while 
downloading it.

Please go to the website to find details of the list.

-------------------
11. Acknowledgments
-------------------

Thanks to *you* for using Tail!

Must mention the kind people that have lent a hand to test the 
various versions of Tail on various platforms, and those people that 
have sent many suggestions for enhancements. 

And of course, I really *must* say thaks to the people that were kind 
enough to give me money to keep the development effort going!! You
know who you are!


Big thanks go to the following developers for their skillful contributions 
to the development of Tail:

  mausoma
  Muhammad Ali Shah (Ragamuffin)
  Wan Lee (Warlock)

During the coding of Tail, I've been in situations where I need to code
solutions quickly. Rather than pulling out my hair, I've tried to use
other open solutions. Therefore, I'd like to thank the authors of these:

Hans Dietrich - Keyword combo/checkbox list control
    http://www.codeproject.com/listctrl/xlistctrl.asp

Oliver Smith - Docking 'workspace' window 
    http://www.codeguru.com/docking/devstudio_like_controlbar_2.shtml



