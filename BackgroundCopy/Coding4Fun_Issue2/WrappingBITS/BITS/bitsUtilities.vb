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
Imports SIDUtilities

Namespace BITS
    Friend Class Utilities
        Public Const RPC_C_AUTHN_LEVEL_CONNECT As Integer = 2
        Public Const RPC_C_IMP_LEVEL_IMPERSONATE As Integer = 3
        Declare Auto Function CoInitializeSecurity Lib "ole32.dll" (ByVal secDesc As IntPtr, ByVal cAuthSvc As Integer, ByVal asAuthSvc As IntPtr, ByVal reserved1 As IntPtr, ByVal authnLevel As Integer, ByVal impLevel As Integer, ByVal authList As IntPtr, ByVal capabilities As Integer, ByVal reserved3 As IntPtr) As Integer
        Declare Auto Function IsEqualGUID Lib "ole32.dll" (ByRef rguid1 As BackgroundCopyManager.GUID, ByRef rguid2 As BackgroundCopyManager.GUID) As Boolean

        Public Shared Function GetAccountName(ByVal SID As String) As String
            Dim ownerName As New LookupSID()
            Return ownerName.GetName(SID)
        End Function
        Public Shared Function ConvertToGUID(ByVal bitsGUID As BackgroundCopyManager.GUID) As System.Guid
            Dim myGUID As New System.Guid(bitsGUID.Data1, bitsGUID.Data2, bitsGUID.Data3, bitsGUID.Data4(0), bitsGUID.Data4(1), bitsGUID.Data4(2), bitsGUID.Data4(3), bitsGUID.Data4(4), bitsGUID.Data4(5), bitsGUID.Data4(6), bitsGUID.Data4(7))
            Return myGUID
        End Function

        Public Shared Function ConvertToBITSGUID(ByVal value As System.Guid) As BackgroundCopyManager.GUID
            Dim myGUID As New BackgroundCopyManager.GUID()
            Dim inGUID As Byte()
            Dim Data1 As System.UInt32
            Dim Data2 As System.UInt16
            Dim Data3 As System.UInt16
            Dim Data4(8) As Byte
            inGUID = value.ToByteArray
            Data1 = System.BitConverter.ToUInt32(inGUID, 0)
            Data2 = System.BitConverter.ToUInt16(inGUID, 4)
            Data3 = System.BitConverter.ToUInt16(inGUID, 6)
            inGUID.Copy(inGUID, 8, Data4, 0, 8)
            myGUID.Data1 = Data1
            myGUID.Data2 = Data2
            myGUID.Data3 = Data3
            myGUID.Data4 = Data4
            Return myGUID
        End Function
    End Class
End Namespace