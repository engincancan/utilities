H2benchw -- by Harald Bögeholz & Lars Bremer / c't Magazin für Computertechnik
Copyright (C) 2009 Heise Zeitschriften Verlag GmbH & Co. KG
==============================================================================

H2benchw runs as a Win32 console application (at the command prompt)
under Windows NT, 2000, XP and Vista. It accesses the hard drive
directly by opening it as a phyisical drive. Do to this, it needs
administrator privileges. In order to obtain reproducable results, you
should not run any other applications while benchmarking and leave the
machine alone. (H2benchw maximizes it's own process just to be sure.)

On the command line, the number of the hard disk you want to test and
any options for the test procedure must be given. When calling the
program without parameters, a short help text is displayed. All
options must be entered in lower case letters. The number of the hard
disk you want to test is the most important option. The first physical
disk is number 0, the next one is number 1, and so on. If you enter
the disk number without any options, H2benchw displays geometry
(cylinders, heads, sectors) and capacity of the disk. For IDE drives
you should additionally get model number, serial number and firmware
revision. If there are several disks in the computer, it is
recommended to run H2benchw in this way first, to make sure that you
select the correct disk later.

H2benchw ignores partitioning and accesses the disk directly, so that
all data will be lost when writing on the disk. For this reason,
H2benchw normally operates in read-only mode. The option "-!" activates
write measurements. For safety reasons this will not work if there are
any partitions on the disk.

H2benchw measurement consists of five components: interface transfer
rate, data integrity check, zone measurement, the measurement of
access times and application profiles. For a detailed description
please see c't 8/02, pp 194ff, c't 15/02, pp 150ff, and c't 23/02, pp
160ff (in German).

The option "-c <n>" (-c stands for "core test" for historical reasons)
activates the interface transfer rate measurement at <n> percent of
the total capacity. This measurement is usually done at 0 percent
since that is the fastest zone for most hard drives.

The option "-d <n>" activates the integrity check. The first <n>
sectors of the disk are fully checked, the rest ist only sampled.
Instead of a number you can use the word "max" to fully test the whole
disk. For more information see below under "Data integrity check".

The option "-z" carries out the zone measurement.

The option "-s" activates the seek measurement.

The option "-p" measures application profiles. Caution: This
measurement is no longer meaningful since access profiles of the OS
and applications have changed. Furthermore h2benchw makes some
assumptions that are no longer valid for todays hard drives so that
some profiles yield unrealistically good results.

When you enter the option "-a", all measurements are carried out; this
option combines "-c", "-z", "-s", and "-p", and aditionally "-d 20480"
if -! is also specified.

Use the option "-w <filename>" to enter a <filename> for storing the
results. H2benchw adds the extensions ".txt" and ".ps" to the given
base name and generates two result files.

The file with the extension ".txt" is an ASCII file, the first part of
which is a summary of the results. The second part contains detailed
results of the zone measurements. Please note that the zone
measurement has up to 1000 measuring points for both reading and
writing tests. Each result is given on a separate line, so that the
results can be evaluated with other programs. Before printing out the
.txt file, use a text editor to cut off the second part of the file --
otherwise you print out more than 30 pages of "garbage".

The file with the extension ".ps" is in PostScript format and
summarizes all results on a single page (DIN A4). Those equipped with
a postscript printer can simply send the file to the printer, for
example with the command

copy /b filename.ps prn

If you do not own a PostScript printer or if you want to display the
results on the screen, you can use the freeware interpreter
GhostScript or GhostView. These programs are freely available on the
internet for practically all operating systems. For current versions,
see http://www.cs.wisc.edu/~ghost/.

The following options can be used for documentation purposes: "-tt"
followed by the name of the hard disk (in quotation marks), assigns
the "title" of the test, "-ts" is meant for additional information
such as the media type in case of removable media. In the same way,
"-tb" is used for the BIOS, "-tc" for the CPU, "-tm" for the
motherboard, and "-ta" for the adapter (SCSI or EIDE). Since these
last components are rarely changed, you can also put them in
environment variables (i.e. put some SET commands into a batch file or
set the variables through the control panel in XP under
System->Advanced->Environment Variables). The relevant variables are
named H2BTITLE, H2BMEDIUM, H2BBIOS, H2BCPU, H2BBOARD, and H2BADAPTER.
They will be referred to when the command line does not contain any
relevant data.

In most cases, you only have to enter the name of the hard disk for
the test. Here is a typical command:

h2benchw 1 -a -! -tt "harddisk 42" -w 42

This tests the second hard disk connected to the computer and stores
the results in the files "42.txt" and "42.ps".


Data integrity check
====================

Originally H2benchw was a benchmark only, i.e., it measures how fast
you can read and write data but doesn't check the data for
correctness. When IDE drives crossed the 128 GByte boundary it seemed
a good idea to implement a data integrity check (option -d <n>). It
works as follows:

First it writes (different!) test patterns to the first <n> sectors of
the drive. The default is 20480 sectors (10 MByte). Instead of <n> you
can type "max" to test the whole disk. Next H2benhcw writes five
sectors each around LBAs that are powers of two (in case of driver
bugs these are the critical addresses). Finally the test writes a
pseudo-random sequence of sectors for a certain time (default: 10
seconds). The duration of this last phase can be changed with the
option -dt <s> where <s> specifies the number of seconds.

After a short breather H2bench reads the test patterns written before
and checks them. In case of an error it aborts with a message giving
the location of the error and the expected and found value. The test
data has the LBA of each sector stored in the first dword of that
sector. If there is a mismatch here that usually means that some
sector has been written to the wrong address. The actual value in the
first dword then shows which sector the data should have been written
to.

CAUTION: If you run the integrity check read-only, i.e., without the
option -!, the appropriate amount of test data must of cause have been
written to the disk in a previous run, otherwise you get an error
message.


Language options
================

H2benchw currently "speaks" three languages: German, English and
Dutch, the default being German. The command line option "-deutsch"
selects German, "-english" selects English and "-nederlands" selects
Dutch.

The language can also be pre-selected using the environment variable
H2BLANGUAGE. This avoids specifying the command line option each time.
You should usually set H2BLANGUAGE in the control panel. Examples:

SET H2BLANGUAGE=deutsch     for German

SET H2BLANGUAGE=english     for English

SET H2BLANGUAGE=nederlands  for Dutch


Known limitations
=================

H2benchw works with 32-bit values for the logical block addresses
(LBA). That means it won't work with drives larger than 2 TByte
(hopefully I haven't forgotten a sign bit somewhere and built in a bug
at 1 TByte ;-) ).

On some multi processor or dual-core systems there is a problem with
the timers yielding incorrect results. If in doubt, please repeat the
measurement on a single core system.


Questions
=========

Please send any questions about H2benchw in German or English to
bo@ct.heise.de (Harald Boegeholz). I will take the liberty not to
respond to questions already answered in the FAQ below.


FAQ
===

?  H2benchw always aborts with a read error.

:  In cases of physical defects, the current version of H2benchw cannot
   measure the continuous transfer rate (zone measurement). With luck,
   some of the other measurements still work if they don't hit one of
   the defective sectors.


?  When calling up the hard disk benchmark H2benchw, I always get an
   error message stating that the timer does not run monotonically,
   and the program is aborted.

:  On some chipsets the system timer is buggy: The lower bits are
   sometimes read out wrong so that it looks like time is going
   backwards.

   Up to date this error message has been reported only under Windows
   2000. Windows NT and XP seem to compensate the error. If you see
   this error message under Windows XP, I would appreciate an exact
   description of your system and the circumstances under which it
   occured. Please email to bo@ct.heise.de.

   In the meantime you can use the option "-Tnocheck" to suppress the
   timer check -- but not the underlying problem! When using this
   option, H2benchw produces more or less correct results, but the
   results are not reliable. 


?  I can't reproduce your results: For my SCSI drive I get much lower
   write rates than you do.

:  On SCSI drives we always enable the write cache for benchmarking.
   Without write cache SCSI drives write only about half as fast as
   they read.


?  I have H2benchw versions 2.15 and 2.3. Which one is the latest
   version?

:  The number after the dot is not meant as a decimal fraction but as
   a minor version number. After 2.9 (two-nine) came 2.10 (two-ten),
   then 2.11 (two-eleven) and so on. The current version is 3.13.


?  My IDE drive achieves a maximum transfer rate of about 7 MByte/s
   [sometimes even only 2 MByte/s] when attached to the onboard EIDE
   channel of my board although the test results published in c't
   magazine are much better. When I connect the drive to another
   system the performance is better.

:  Probably your drive is running in PIO mode instead of (Ultra) DMA
   mode. Check this in the device manager (Windows 2000/XP) or with
   the program dmacheck which is available for download from
   Microsoft.


?  I am using an IDE RAID array. Why doesn't H2benchw display the
   model numbers an serial numbers of the drives?

:  Reading the ATA device information is currently only implemented
   for single IDE drives. On some host adapters it doesn't even work
   for single drives, sorry.
