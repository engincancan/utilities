BurnCDCC(TM)
~~~~~~~~~~~~
This utility will burn an optical disc from a disc image file.  These
files most often have the .ISO file extension.

BurnCDCC is intended for use by customers of TeraByte, Inc.


INSTALLATION:
~~~~~~~~~~~~~
Simply copy the BURNCDCC.EXE file to a folder on your hard drive.  Create
a shortcut on your Desktop or Start menu for easy access.

If desired, you may add command line options to the shortcut's Target box,
to have BurnCDCC launch with your desired options already in place.
(Command line options are explained below.)


REQUIREMENTS:
~~~~~~~~~~~~~
* Writable CD/DVD/BD drive
* Microsoft Windows

FYI:
~~~~
* Under WinNT/2000/XP/2003 the program may need to run as a member of the
  Administrators group for devices to be detected.

* Under WinNT, no disc can be in the drive when starting the program or no
  devices will be detected.


USE:
~~~~
File Image Field:
  Supply the name of the file image.  This file usually has the .ISO
  file extension.

Device:
  If you have more than one burning device, then select which device to use.

Read Verify:
  Check this option to verify the integrity of the disc after burning.

SAO:
  Enables session-at-once (SAO) recording for CD.
  Note: The burning device in use must support SAO for this feature to work.

Finalize:
  Leave this checked to have the discs finalized (no more writing allowed).

Auto Eject:
  Automaticaly eject the drive tray when needed.  Please note that under
  WinNT/2000/XP/2003, if this option is unchecked, the drive tray will
  open when the Start button is clicked, but will close automatically.

Speed:
  If you have underrun problems then you may have to choose a slower speed.
  If you select the Optimal speed setting, the drive's firmware will be
  relied upon to select the best burning speed for the media in use.
  Note: The speed can be changed during the burn, but it will not take
        effect unless the burn process is stopped and restarted.


COMMAND LINE OPTIONS:
~~~~~~~~~~~~~~~~~~~~~
The command line options are used to set default values.

/I=d:\imagefilepath\filename.iso
/D=device_index (0 equals first device)
/V to enable Read Verify
/O to enable SAO recording
/F to enable Finalize Disc
/J to disable Auto Eject
/S=speed_index (0=1X, 1=2X, ..., 8=MAX)

Examples:

burncdcc.exe /v /j

burncdcc.exe /j /s=5 /i=d:\images\discimage.iso

burncdcc.exe /f /d=1 /v


TROUBLESHOOTING:
~~~~~~~~~~~~~~~~
* If you experience problems with a particular burning device, that drive
  may require a firmware update.  You will find firmware updates at the
  drive manufacturer's web site.

* If you experience problems with certain optical media, you may need to
  apply firmware updates to your optical drive(s), as explained above.
  You could also simply try a different brand/batch of media.

* If you have a problem that only occurs after BurnCDCC closes the drive
  tray automatically, you may be able to work around the problem by closing
  the tray yourself, waiting a moment for the drive to spin up, and then
  clicking OK.

* If you experience problems under Win9x/Me, it may be due to the following:

    a) ASPI drivers that are old or proprietary.  Reinstall the ASPI drivers
       that came with your version of Windows or visit www.adaptec.com for
       the latest version.

    b) Drivers (VxDs) that are not stable, or an active program conflicting
       with the burn process.  These are typically installed as part of a
       full burning software package.

    c) Auto insert notification.


--End of Document--
