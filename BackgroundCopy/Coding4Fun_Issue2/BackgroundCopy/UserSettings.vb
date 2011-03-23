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

Imports System.IO
Imports System.IO.IsolatedStorage
Imports Microsoft.Win32

Public Class UserSettings
    'some shared functions for utility purposes; 
    'saving settings, loading settings...
    Private Shared Function _
        GetIEDefaultSaveLocation() As String
        'When you download a file from IE, 
        'it defaults to the last location 
        'you downloaded a file to, this code 
        'just pulls that location out of the 
        'registry. If the setting isn't available,
        'the user's My Documents folder is used
        'as a default.
        Dim saveDir As String
        Try
            Dim IEMain As RegistryKey
            IEMain = Registry.CurrentUser.OpenSubKey _
                ("Software\Microsoft\Internet Explorer\Main", _
                 False)

            saveDir = CStr(IEMain.GetValue("Save Directory", _
                                    Nothing))
            If saveDir Is Nothing Then
                'default to user's My Documents folder
                saveDir = Environment.GetFolderPath( _
                    Environment.SpecialFolder.Personal)
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
            saveDir = Directory.GetDirectoryRoot( _
                Application.StartupPath)
        End Try
        Return saveDir
    End Function

    Public Shared Function _
        GetSettings() As Options
        'load the settings from the user's isolated storage
        Try
            Dim m_Options As Options
            Dim settingsPath As String = "settings.xml"
            Dim isf As IsolatedStorageFile
            isf = IsolatedStorageFile.GetUserStoreForAssembly

            If isf.GetFileNames(settingsPath).Length > 0 Then
                Dim myXMLSerializer As New _
                    Xml.Serialization.XmlSerializer( _
                    GetType(Options))
                m_Options = CType( _
                    myXMLSerializer.Deserialize( _
                    New IsolatedStorageFileStream _
                    (settingsPath, IO.FileMode.Open, _
                        IO.FileAccess.Read)), Options)
            Else
                m_Options = New Options()
                m_Options.defaultSaveLocation = _
                    GetIEDefaultSaveLocation()
            End If
            Debug.WriteLine(m_Options.defaultSaveLocation)
            Return m_Options
        Catch ex As System.Exception
            MsgBox(ex.ToString)
            Return Nothing
        End Try
    End Function

    Public Shared Sub SaveSettings( _
        ByVal currentSettings As Options)
        Try
            Dim isf As IsolatedStorageFile
            isf = IsolatedStorageFile.GetUserStoreForAssembly

            Dim settingsPath As String = "settings.xml"
            Dim myXMLSerializer As _
                New Xml.Serialization.XmlSerializer( _
                GetType(Options))
            myXMLSerializer.Serialize( _
                New IsolatedStorageFileStream( _
                    settingsPath, IO.FileMode.Create, _
                    IO.FileAccess.ReadWrite), _
                currentSettings)
        Catch ex As System.Exception
            MsgBox(ex.ToString)
        End Try
    End Sub
End Class

<Serializable()> Public Class Options
    'you could just add additional properties to this
    'class to allow for more options to be set/saved
    Dim m_defaultSaveLocation As String

    Public Property defaultSaveLocation() As String
        Get
            Return m_defaultSaveLocation
        End Get
        Set(ByVal Value As String)
            Try
                If Path.IsPathRooted(Value) AndAlso _
                    Not Path.HasExtension(Value) Then
                    If Not Directory.Exists(Value) Then
                        Directory.CreateDirectory(Value)
                    End If
                    m_defaultSaveLocation = Value
                End If
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try
        End Set
    End Property
End Class

