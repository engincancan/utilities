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

    Public Class JobCollection
        Inherits System.Collections.CollectionBase
        'Typed Collection for Jobs

        Default Public Property Item(ByVal index As Integer) As Job
            Get
                Return CType(Me.List(index), Job)
            End Get
            Set(ByVal value As Job)
                Me.List(index) = value
            End Set
        End Property

        Protected Friend Function Add(ByVal value As Job) As Integer
            Return Me.List.Add(value)
        End Function

        Protected Friend Sub Insert(ByVal index As Integer, ByVal value As Job)
            Me.List.Insert(index, value)
        End Sub

        Private Function IndexOf(ByVal value As Job) As Integer
            Return Me.List.IndexOf(value)
        End Function

        Private Function Contains(ByVal value As Job) As Boolean
            Return Me.List.Contains(value)
        End Function

        Private Sub Remove(ByVal value As Job)
            Me.List.Remove(value)
        End Sub

        Public Sub CopyTo(ByVal array() As Job, ByVal index As Integer)
            Me.List.CopyTo(array, index)
        End Sub

    End Class


    Public Class FileCollection
        Inherits System.Collections.CollectionBase
        'Typed Collection for File objects

        Friend Sub New()
        End Sub

        Default Public Property Item(ByVal index As Integer) As BITSFile
            Get
                Return CType(Me.List(index), BITSFile)
            End Get
            Set(ByVal value As BITSFile)
                Me.List(index) = value
            End Set
        End Property

        Protected Friend Function Add(ByVal value As BITSFile) As Integer
            Return Me.List.Add(value)
        End Function

        Protected Friend Sub Insert(ByVal index As Integer, ByVal value As BITSFile)
            Me.List.Insert(index, value)
        End Sub

        Private Function IndexOf(ByVal value As BITSFile) As Integer
            Return Me.List.IndexOf(value)
        End Function

        Private Function Contains(ByVal value As BITSFile) As Boolean
            Return Me.List.Contains(value)
        End Function

        Private Sub Remove(ByVal value As BITSFile)
            Me.List.Remove(value)
        End Sub

        Public Sub CopyTo(ByVal array() As BITSFile, ByVal index As Integer)
            Me.List.CopyTo(array, index)
        End Sub
    End Class

End Namespace