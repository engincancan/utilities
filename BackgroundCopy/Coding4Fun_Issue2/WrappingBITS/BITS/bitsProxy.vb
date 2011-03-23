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
    Public Class ProxySettings
        Private m_Usage As BG_JOB_PROXY_USAGE
        Private m_sProxyList As String
        Private m_sProxyBypassList As String

        Friend Sub New(ByVal Usage As BG_JOB_PROXY_USAGE, ByVal ProxyList As String, ByVal BypassList As String)
            m_Usage = Usage
            m_sProxyList = ProxyList
            m_sProxyBypassList = BypassList
        End Sub

        Public Property Usage() As ProxyUsage
            Get
                Return CType(m_Usage, ProxyUsage)
            End Get
            Set(ByVal value As ProxyUsage)
                m_Usage = CType(value, BG_JOB_PROXY_USAGE)
            End Set
        End Property

        Public Property ProxyList() As String
            Get
                Return m_sProxyList
            End Get
            Set(ByVal value As String)
                m_sProxyList = value
            End Set
        End Property

        Public Property ProxyBypassList() As String
            Get
                Return m_sProxyBypassList
            End Get
            Set(ByVal value As String)
                m_sProxyBypassList = value
            End Set
        End Property

    End Class
End Namespace