
Welcome to the Great Internet Mersenne Prime Search!

To use this program you must agree to the terms and conditions,
prize rules, etc. at http://mersenne.org/legal/

In case you ever forget, the GIMPS web site is at http://mersenne.org
My email address is woltman@alum.mit.edu.  For networking questions,
contact Scott Kurowski at primenet@mersenne.org.


FILE LIST
---------

readme.txt	This file.
prime95.exe	The program to factor and run Lucas-Lehmer tests on
		Mersenne numbers.
prime95.chm	The help file in HTML help format.
whatsnew.txt	A list of new features in prime95.exe.
stress.txt	A discussion of issues relating to stress testing a computer.
undoc.txt	A list of formerly undocumented and unsupported features.
prime.txt	A file containing your preferences.  The menu choices
		and dialog boxes are used to change your preferences.
local.txt	Like prime.txt, this file contains more preferences.
		The reason there are two files is discussed later.
worktodo.txt	A list of exponents the program will be factoring
		and/or Lucas-Lehmer testing.
results.txt	Prime95.exe writes its results to this file.
prime.log	A text file listing all messages that have been sent
		to the PrimeNet server.
prime.spl	A binary file of messages that have not yet been sent to
		the PrimeNet server.
pNNNNNNN &	Intermediate files produced by prime95.exe to resume
qNNNNNNN	computation where it left off.
eNNNNNNN	Intermediate files produced during ECM factoring.
fNNNNNNN	Intermediate files produced during trial factoring.
mNNNNNNN	Intermediate files produced during P-1 factoring.


WHAT IS THIS PROGRAM?
---------------------

This program is used to find Mersenne Prime numbers.  See
http://www.utm.edu/research/primes/mersenne.shtml for a good
description of Mersenne primes.  Mersenne numbers can be proved
composite (not prime) by either finding a factor or by running
a Lucas-Lehmer primality test.


INSTRUCTIONS
------------

There are two ways to use this program.  The automatic way uses
a central server, which we call the PrimeNet server, to get work to do
and report your results.  Anyone with Internet access, including AOL,
CompuServe, and dial-up ISP users should use this method.  You do not
need a permanent connection to the Internet.

The second method is the manual method.  It requires a little more work
and monitoring.  I recommend this for computers with no Internet access
or with some kind of firewall problem that prevents the automatic method
from working.

If you are running this program at your place of employment, you must
first GET PERMISSION from your network administrator, boss, or both.
This is especially true if you are installing the software on several machines.
Many companies have policies that prohibit running unauthorized software.
Violating that policy could result in termination and/or prosecution.


INSTRUCTIONS FOR THE AUTOMATIC METHOD
-------------------------------------

1)  Run the setup program, p95setup.exe or download and unzip prime95.zip.
    You've probably done this already since you are reading this file.
2)  Connect to the Internet.  Create an account at http://mersenne.org
3)  Run prime95.exe.  You will see 4 dialog boxes:
3a) In the welcome dialog box, choose "Join GIMPS!".
3b) In the second dialog box, enter your user id and optional computer name.
    If you are using several computers, use the same user ID but a unique computer
    name on each machine.  An easy-to-remember user ID will be helpful if you
    plan to visit the PrimeNet server's web page to view reports on your
    progress.
3c) In the third dialog box, fill in roughly how many hours a day you leave
    your computer running.  Click OK.
3d) In the fourth dialog box, leave the "Use Primenet..." checkbox
    checked.  Do not turn this checkbox off even if you disconnect
    from the Internet.   Check the "Use a dial-up..." checkbox
    if you use a modem to connect to the Internet.  Note that prime95
    will not dial-up to connect to the Internet, rather it waits
    for a time when you are already connected to contact the server.
    Click OK.  Prime95 will now contact the PrimeNet server to get some
    work for your computer to do.
4a) If a proxy server is the causing connection troubles, see the
    later section on "SETTING UP A PROXY SERVER".
4b) If the program will not connect to the server, then
    you will have to use the manual method described below.
5)  Disable screen savers or use the "blank screen" screen saver.  If this
    is not practical, consider raising prime95's priority to 4 or 5.

    The "Start at Bootup" menu choice (on by default) will run prime95
    every time you boot your computer.


MANUAL METHOD INSTRUCTIONS
--------------------------

1)  Visit http://mersenne.org/update/ to create a userid for yourself and
    http://mersenne.org/manual_assignment/ to get an exponent or two to
    work on.  Copy these exponents to a file called worktodo.txt.
2)  Run prime95.exe.  You will see 3 dialog boxes:
2a) In the welcome dialog box, choose "Join GIMPS!".
2b) In the second dialog box, fill in roughly how many hours a day you leave
    your computer running.  Click OK.
2c) In the third dialog box, uncheck "Use PrimeNet to get work and report
    results", click OK.
3)  Disable screen savers or use the "blank screen" screen saver.  If this
    is not practical, consider raising prime95's priority to 4 or 5.
4)  When done with your exponents, use the web pages again to send the
    file "results.txt" to the PrimeNet server and get more work.

    The "Start at Bootup" menu choice (on by default) will run prime95
    every time you boot your computer.


NOTES
-----

Running prime95 may significantly increase your electric bill.  The amount
depends on your computer and your local electric rates.

It can take many CPU weeks to test a large Mersenne number.  This program
can be safely interrupted by using the ESC key to write intermediate results
to disk.  This program also saves intermediate results to disk every 30 minutes
in case there is a power failure.

You can compare your computer's speed with other users by checking the
web page http://mersenne.org/report_benchmarks/.  If you are much slower than
comparable machines, there are several utilities available (such as
TaskInfo2002, http://www.iarsn.com/) that can find programs that are
stealing prime95's CPU cycles.

You can get several reports of your PrimeNet activity at any time
by logging in at http://mersenne.org/.

If you have overclocked your machine, I recommend running the torture
test for a couple of days.  The longer you run the torture test
the greater the chance that you will uncover an error caused by
overheating or overstressed memory.

Depending on the exponent being tested, the program may decide that it
would be wise to invest some time checking for small factors before
running a Lucas-Lehmer test.


SETTING UP A PROXY SERVER
-------------------------

Choose the "Connection..." button in the Test/Primenet dialog box.  Fill in
the proxy information.


SETTING AVAILABLE MEMORY
------------------------

Stage 2 of P-1 factoring step prior to running a Lucas-Lehmer test is
slightly more effective if it is given more memory to work with.  However,
if you let the program use too much memory then the performance of ALL programs
will suffer.  The good news is that 98% of the time the program uses a minimal
amount of memory.  In fact, the program will work just fine if never
let it use more than the minimum.

So how do you intelligently choose the available memory settings?  Below
are some steps you might take to figure this out:

1)  Be conservative.  It is better to set the available memory too low
than too high.  Setting the value too high can cause thrashing which
slows down all programs.  Remember, the program will only use the
extra memory in stage 2 of P-1 factoring (about 12 hours a month).

2)  Start with how much memory is installed in your machine.  Allow a
reasonable amount of memory for the OS and whatever background tasks
you run (say 100 or 200MB).  This represents the maximum value you should use.
The program won't let you enter more than 90% of installed memory.

3)  Assuming you run your machine 24 hours a day, what hours of the
day do you not use your computer?  Make these your nighttime hours and
let the program use a lot of memory during these hours.  But reduce this
value if you also run batch jobs at night.

4)  Factor in the information below about minimum, reasonable, and
desirable memory amounts for some sample exponents.  If you choose a
value below the minimum, that is OK.  The program will simply skip
stage 2 of P-1 factoring.

	Exponent	Minimum		Reasonable	Desirable
	--------	-------		----------	---------
	20000000	 40MB		   80MB		 120MB
	33000000	 65MB		  125MB		 185MB
	50000000	 85MB		  170MB		 250MB

For example, my machine is a dual-processor with 512MB of memory.
I guess Windows XP can survive on 100MB of memory.  Thus, I set
the available memory to (512 - 100) or ~400MB.  This is my nighttime setting.
During the day, I set the available memory to 80MB.  I can always stop
prime95 if it is doing P-1 factoring and I detect memory thrashing.  More
casual users will probably want to set the daytime memory to 8MB so they
don't have to worry about prime95 impacting system performance.

If at all in doubt, leave the settings at 8MB.  The worst that will
happen is you end up running a Lucas-Lehmer primality test when stage 2
of P-1 factoring would have found a factor.


PROGRAM OUTPUT
--------------

On screen you will see:

Factoring M400037 to 2^54 is 3.02% complete. Time: 0.121 sec.
	This means prime95 is trying to find a small factor of 2^400037-1.
	It is 3.02% of the way though looking at factors below 2^54.  When
	this completes it may start looking for factors less than 2^55.
Iteration: 941400 / 1667747 [56.45%].  Per iteration time: 0.109 sec.
	This means prime95 just finished the 941400th iteration of a
	Lucas-Lehmer primality test.  The program must execute 1667747
	iterations to complete the primality test.  The average iteration
	took 0.109 seconds.

The results file and screen will include lines that look like:

M2645701 has a factor: 13412891051374103
	This means to 2^2645701-1 is not prime.  It is divisible
	by 13412891051374103.
M2123027 no factor to 2^57, WV1: 14780E25
	This means 2^2123027-1 has no factors less than 2^57.  The Mersenne
	number may or may not be prime.  A Lucas-Lehmer test is needed
	to determine the primality of the Mersenne number.  WV1 is
	the program version number.  14780E25 is a checksum to guard
	against email transmission errors.
M1992031 is not prime. Res64: 6549369F4962ADE0. WV1: B253EF24,1414032,00000000
	This means 2^1992031-1 is not prime - a Lucas-Lehmer test says so.
	The last 64 bits of the last number in the Lucas-Lehmer sequence
	is 6549369F4962ADE0.  At some future date, another person will verify
	this 64-bit result by rerunning the Lucas-Lehmer test.  WV1 is the
	program	version number.  B253EF24 is a checksum to guard against email
	transmission errors.  1414032 can be ignored it is used as part
	of the double-checking process.  The final 00000000 value is a set
	of 4 counters.  These count the number of errors that occurred during
	the Lucas-Lehmer test.
M11213 is prime! WV1: 579A579A
	This means 2^11213-1 is a Mersenne prime!  WV1 is the program
	version number.  579A579A is a checksum to guard against email
	transmission errors.


RUNNING PRIME95 ON SEVERAL COMPUTERS
------------------------------------

The easiest way to do this is to first set up prime95 on one computer.
Next copy all the files to the second computer.  Delete the local.txt
file and worktodo.txt files.  These files contain information that
is specific to the first computer.  Start prime95 on the second
computer and use Test/Primenet to give the second computer a unique
computer name.  Repeat this process for all the computers you wish to
run prime95 on.

If you do not follow the instruction above, be sure you use 
Test/Primenet to give each computer the same
user id and different computer name.


TEST MENU
---------

The PrimeNet menu choice identifies your computer to the server.
The "Use PrimeNet..." option can be turned on to switch from the
manual method to the automatic method.

The Worker Threads menu choice is used to choose the type of work
you'd like to execute as well as adjust priority and affinity.
You should not need to change the priority.  You might raise the priority
if you just cannot live without a screen saver, or if you are running some
ill-behaved program that is using CPU cycles for no good reason.
The work type should usually be left set to "Whatever makes the most sense".
However, if you are running a slow computer and don't mind waiting several
months for a single Lucas-Lehmer test to complete OR you are running a faster
computer and get better thoughput if one core does an easier type of work,
then choose a different type of work to do.

The Status menu choice will tell you what exponents you are working on.
It will also estimate how long that will take and your chances of finding
a new Mersenne prime.

The Continue menu choice lets you resume prime95 after you have stopped it.

The Stop menu choice lets you stop the program.  When you continue,
you will pick up right where you left off.  This is the same as hitting
the ESC key.

The Exit menu choice lets you exit the program.


ADVANCED MENU
-------------

You should not need to use the Advanced menu.  This menu choice is
provided only for those who are curious.  Note that many of the menu choices
are grayed while testing is in progress.  Choose Test/Stop to activate
these menu choices.

The Test choice can be used to run a Lucas-Lehmer test on one Mersenne
number.  Enter the Mersenne number's exponent - this must be a prime
number between 5 and 560000000.

The Time choice can be used to see how long each iteration of a Lucas-Lehmer
test will take on your computer and how long it will take to test a
given exponent.  For example, if you want to know how long a Lucas-Lehmer
test will take to test the exponent 876543, choose Advanced/Time and
enter 876543 for 100 iterations.

The ECM choice lets you factor numbers of the form k*b^n+c using the
Elliptic Curve Method of factoring.  ECM requires a minimum of 192 times
the FFT size.  Thus, ECM factoring of F20 which uses a 64K FFT will use
a minimum of 192 * 64K or 12MB of memory.  You can also edit the
worktodo.txt file directly.  For example:
	ECM2=k,b,n,c,B1,B2,curves_to_run

The P-1 choice lets you factor numbers of the form k*b^n+c using
the P-1 method of factoring.  You can also edit the worktodo.txt file
directly.  For example:
	Pminus1=k,b,n,c,B1,B2

Round off checking.  This option will slow the program down by several percent.
This option displays the smallest and largest "convolution error".  The
convolution error must be less than 0.49 or the results will be incorrect.
There really is no good reason to turn this option on.

The Manual Communication menu choice should only be used if the
automatic detection of an Internet connection is not working for you.
Using this option means you have to remember to communicate with the
server every week or two (by using this same menu choice).

The Unreserve Exponent choice lets you tell the server to unreserve
an exponent you have been assigned.  You might do this if a second computer
you had been running GIMPS on died or if you had been assigned an exponent
of one work type (such as a first-time-test) and now you have switched to
another work type (such as 10,000,000 digit numbers).  Any work you have
done on the unreserved exponent will be lost.

The Quit GIMPS menu choice is used when you no longer want this computer
to work on the GIMPS project.  You may rejoin at a later date.
If you are a PrimeNet user your unfinished work will be returned to the
server.  If you are a manual user, you need to send me email containing
your results.txt file.


OPTIONS MENU
------------

The CPU menu choice tells you what CPU the program has detected and
lets you set how much memory the program can use (see the earlier section
on "Setting available memory".

The Preferences menu choice lets you control how often a line is
written to the main window and how often a line is written to
the results file.  It also lets you change how often
intermediate files (to guard against power failure and crashes)
are created.  You can control how often the program checks to
see if you are connected to the Internet.  The program polls
whenever it has new data to send to or work to get from the PrimeNet
server.  If you are low on disk space, you can select one intermediate 
file instead of two.  However, if you crash in the middle of writing
the one intermediate file, you may have to restart an exponent from
scratch.  You can also tell the program to be quiet, rather than 
beeping like crazy, if a new Mersenne prime is found.  You can also
make prime95 go idle whenever your laptop is running on battery power
(may not work under Windows NT/2000/XP).

The Torture Test choice will run a continuous self test.  This is great
for testing machines for hardware problems.  See the file stress.txt
for a more in-depth discussion of stress testing and hardware problems.

The Benchmark choice times the program on several FFT lengths.  You can
then compare your computer's speed to others list at
http://mersenne.org/report_benchmarks/

The Tray Icon choice will cause prime95 to have a small icon on the taskbar
when it is minimized.  You can activate or hide the program by double-clicking
on the small icon.  If you place the cursor over the small icon, a tooltip will
display the current status.

The No Icon choice is only enabled if the Advanced Menu is activated
with the password.  Using this menu choice means there will be no
prime95 icon on the taskbar once you minimize the program - making it very
hard to reactivate!  You can reactivate the program by trying to execute
prime95 a second time.  Alternatively, you can turn this feature off by
editing prime.txt and change the line "HideIcon=1" to "HideIcon=0",
then reboot.

Checking the "Start at Bootup" menu choice will run prime95 as a service
that starts when your computer boots up.  Windows NT/2000/XP users need
administrator privileges to use this feature.  Vista has eliminated support
for running prime95 as a service.  Running prime95 is better than creating
a shortcut to prime95 in the startup folder because services run even when
no one is logged on.  These are the details you should be aware of:
	In Windows 95/98/Me:
The HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunServices
registry entry is created.  You will not be able to tell any difference between
prime95 running as a service and prime95 running as an ordinary process.
	Windows Vista and Windows NT/2000/XP without admin privileges:
In this case the menu text is changed to "Start at Logon".  The
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run registry entry
is created.  Prime95 will run only when you are logged in.
	In Windows NT/2000/XP with administrator privileges:
There are a few minor quirks when running as a service.  You shouldn't run
into these quirks in normal operation.  You can only change this option once
and it will take effect when you exit the program.  The prime95 window will
only appear on one user's desktop.  Finally, if you are worried about giving
users access to a GUI service running in the Local System account, you can
turn off the "Allow service to interact with desktop" option in the services
control panel applet or run the GUI-less NT service version available at
http://mersenne.org/freesoft/  These options are more secure than
using the No Icon menu choice.


COMMAND LINE ARGUMENTS
----------------------

-An		Obsolete.
		This is used to run two or more copies of prime95
		from the same directory.  Using this command line argument
		causes prime95 to use a different set of filenames for the
		INI files, the results file, the log file, and the spool file.
		Just use a different value of n for each copy of
		prime95.exe you start.
-t		Run the torture test.  Same as Options/Torture Test.
-Wdirectory	This tells prime95 to find all its files in a different
		directory than the executable.


POSSIBLE HARDWARE FAILURE
-------------------------

If the message "Possible hardware failure, consult the readme file."
appears in the results.txt file, then prime95's error-checking has
detected a problem.  After waiting 5 minutes, the program will continue
testing from the last save file.

Could it be a software problem?  If the error is ILLEGAL SUMOUT and you are
running Windows 95/98/Me, then there is some chance that this is a software
problem.  A device driver or VxD may not be saving and restoring FPU state
correctly.  Windows NT/2000/XP/Vista protects prime95 from bad device drivers.
In that case ILLEGAL SUMOUT is very likely a true hardware error. The good news
is that prime95 recovers very well from ILLEGAL SUMOUT errors.  Try seeing
if the problem occurs only when a specific device is active or a specific
program is running.

The other two errors messages, SUMINP != SUMOUT and ROUND OFF > 0.40 are
caused by one of two things:
	1)  For reasons too complicated to go into here, the program's error
	checking is not	perfect.  Some errors can be missed and some correct
	results flagged as an error.  If you get the message "Disregard last
	error..." upon continuing from the last save file, then you may have
	found the rare case where a good result was flagged as an error.
	2)  A true hardware error.

If you do not get the "Disregard last error..." message or this happens
more than once, then your machine is a good candidate for a torture test.
See the stress.txt file for more information.

Running the program on a computer with hardware problems may produce
incorrect results.  This won't hurt the GIMPS project since all results
will be double-checked.  However, you could be wasting your CPU time.
If you are getting several errors during each primality test, then I
recommend using your machine to factor Mersenne numbers or run one of
the many less strenuous distributed computing projects available.


LUCAS-LEHMER DETAILS
--------------------

This program uses the Lucas-Lehmer primality test to see if 2**p-1 is prime.
The Lucas sequence is defined as:
	L[1] = 4
	L[n+1] = (L[n]**2 - 2) mod (2**p - 1)
2**p-1 is prime if and only if L[p-1] = 0.

This program uses a discrete weighted transform (see Mathematics of
Computation, January 1994) to square numbers in the Lucas-Lehmer sequence.


DISCLAIMER
----------

THIS PROGRAM AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.


THANKS
------

Happy hunting and thanks for joining the search,
George Woltman
woltman@alum.mit.edu

