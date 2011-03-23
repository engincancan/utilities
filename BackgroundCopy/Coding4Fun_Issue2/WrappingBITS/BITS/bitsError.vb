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
Public Class bitsError
    Dim m_Description As String

    Public Property Description() As String
        Get
            Return m_Description
        End Get
        Set(ByVal Value As String)
            m_Description = Value
        End Set
    End Property
    Public Sub New(ByVal bErr As BackgroundCopyManager.IBackgroundCopyError)

        bErr.GetErrorDescription(Convert.ToUInt32(System.Threading.Thread.CurrentThread.CurrentUICulture.LCID), m_Description)
    End Sub


End Class
