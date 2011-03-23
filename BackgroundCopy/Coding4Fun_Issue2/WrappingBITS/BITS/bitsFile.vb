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
    Public Class BITSFile
        Dim m_File As BackgroundCopyManager.IBackgroundCopyFile

        Friend Sub New(ByVal jobFile As IBackgroundCopyFile)
            m_File = jobFile
        End Sub

        Public ReadOnly Property Progress() As FileProgress
            Get
                Dim tmpFileProgress As _BG_FILE_PROGRESS
                m_File.GetProgress(tmpFileProgress)
                Return New FileProgress(tmpFileProgress)
            End Get
        End Property

        Public ReadOnly Property LocalName() As String
            Get
                Dim tmpLocalName As String
                m_File.GetLocalName(tmpLocalName)
                Return tmpLocalName
            End Get
        End Property

        Public ReadOnly Property RemoteName() As String
            Get
                Dim tmpRemoteName As String
                m_File.GetRemoteName(tmpRemoteName)
                Return tmpRemoteName
            End Get
        End Property
    End Class
End Namespace