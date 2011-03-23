Option Strict On
Option Explicit On 
Imports System
Imports System.Runtime.InteropServices

Public Class LookupSID

    Private Declare Unicode Function _
        ConvertStringSidToSidW Lib "advapi32.dll" (ByVal StringSID As String, _
                                                   ByRef SID As IntPtr) As Boolean
    Private Declare Unicode Function _
        LookupAccountSidW Lib "advapi32.dll" (ByVal lpSystemName As String, _
                                              ByVal SID As IntPtr, _
                                              ByVal Name As Text.StringBuilder, _
                                              ByRef cbName As Long, _
                                              ByVal DomainName As Text.StringBuilder, _
                                              ByRef cbDomainName As Long, _
                                              ByRef psUse As Integer) As Boolean

    Shared Function GetName(ByVal SID As String) As String
        Const size As Integer = 255
        Dim domainName As String
        Dim userName As String
        Dim cbUserName As Long = size
        Dim cbDomainName As Long = size
        Dim ptrSID As New IntPtr(0)
        Dim psUse As Integer = 0
        Dim bufName As New Text.StringBuilder(size)
        Dim bufDomain As New Text.StringBuilder(size)
        If ConvertStringSidToSidW(SID, ptrSID) Then
            If LookupAccountSidW(String.Empty, _
                                ptrSID, bufName, _
                                cbUserName, bufDomain, _
                                cbDomainName, psUse) Then
                userName = bufName.ToString
                domainName = bufDomain.ToString
                Return String.Format("{0}\{1}", domainName, userName)
            Else
                Return ""
            End If
        Else
            Return ""
        End If

    End Function

End Class
