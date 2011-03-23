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
    Public Class JobProgress
        'Wrapper class for the _BG_JOB_PROGRESS structure
        'http://msdn.microsoft.com/library/en-us/bits/refdrz1_9tmb.asp

        Protected m_Progress As BackgroundCopyManager._BG_JOB_PROGRESS
        Public Sub New(ByVal jobProgress As BackgroundCopyManager._BG_JOB_PROGRESS)
            m_Progress = jobProgress
        End Sub

        Public ReadOnly Property BytesTotal() As Decimal
            Get
                Return Convert.ToDecimal(m_Progress.BytesTotal)
            End Get
        End Property

        Public ReadOnly Property BytesTransferred() As Decimal
            Get
                Return Convert.ToDecimal(m_Progress.BytesTransferred)
            End Get
        End Property

        Public ReadOnly Property FilesTotal() As Decimal
            Get
                Return Convert.ToDecimal(m_Progress.FilesTotal)
            End Get
        End Property

        Public ReadOnly Property FilesTransferred() As Decimal
            Get
                Return Convert.ToDecimal(m_Progress.FilesTransferred)
            End Get
        End Property
    End Class

    Public Class FileProgress
        Protected m_Progress As BackgroundCopyManager._BG_FILE_PROGRESS
        Public Sub New(ByVal fileProgress As BackgroundCopyManager._BG_FILE_PROGRESS)
            m_Progress = fileProgress
        End Sub

        Public ReadOnly Property BytesTotal() As Decimal
            Get
                Return Convert.ToDecimal(m_Progress.BytesTotal)
            End Get
        End Property

        Public ReadOnly Property BytesTransferred() As Decimal
            Get
                Return Convert.ToDecimal(m_Progress.BytesTransferred)
            End Get
        End Property

        Public ReadOnly Property Completed() As Boolean
            Get
                Return CBool(m_Progress.Completed)
            End Get
        End Property

    End Class
End Namespace