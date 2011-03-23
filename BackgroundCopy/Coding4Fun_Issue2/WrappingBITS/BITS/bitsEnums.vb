'Copyright (C) 2003 Microsoft Corporation
'All rights reserved.
'
'THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER
'EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF
'MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
'
'Date: February, 2003
'Author: Duncan Mackenzie
'http://www.gotdotnet.com/Community/User/viewprofile.aspx?userid=00011A674C38C375  
'Requires the release version of .NET Framework 
Option Strict On
Option Explicit On 

Imports System
Imports System.Runtime.InteropServices
Imports BackgroundCopyManager


Namespace BITS
    Public Enum JobType As Integer
        CurrentUser = 0
        AllUsers = 1
    End Enum

    <Flags()> Public Enum NotificationTypes As Integer
        JobTransferred = 1
        JobError = 2
        JobModification = 8
    End Enum

    Public Enum JobPriority As Integer
        Foreground = 0
        High = 1
        Normal = 2
        Low = 3
    End Enum

    Public Enum JobState As Integer
        Queued = 0
        Connecting = 1
        Transferring = 2
        Suspended = 3
        Errors = 4
        TransientError = 5
        Transferred = 6
        Acknowledged = 7
        Cancelled = 8
    End Enum

    Public Enum ProxyUsage As Integer
        NoProxy = 1
        Override = 2
        Preconfig = 0
    End Enum

End Namespace
