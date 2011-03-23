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
Imports System
Imports System.Collections

'------------------------------------------------------------------------------
'<copyright from='1997' to='2001' company='Microsoft Corporation'>
'   Copyright (c) Microsoft Corporation. All Rights Reserved.   
'   Information Contained Herein is Proprietary and Confidential.       
'</copyright> 
'------------------------------------------------------------------------------
'

'<summary>
'    <para>
'      A collection that stores <see cref='BackgroundCopy.BITSJob'/> objects.
'   </para>
'</summary>
'<seealso cref='BackgroundCopy.BITSJobCollection'/>
<Serializable()> _
Public Class BITSJobCollection
    Inherits CollectionBase

    '<summary>
    '    <para>
    '      Initializes a new instance of <see cref='BackgroundCopy.BITSJobCollection'/>.
    '   </para>
    '</summary>
    Public Sub New()
        MyBase.New()
    End Sub

    '<summary>
    '    <para>
    '      Initializes a new instance of <see cref='BackgroundCopy.BITSJobCollection'/> based on another <see cref='BackgroundCopy.BITSJobCollection'/>.
    '   </para>
    '</summary>
    '<param name='value'>
    '      A <see cref='BackgroundCopy.BITSJobCollection'/> from which the contents are copied
    '</param>
    Public Sub New(ByVal value As BITSJobCollection)
        MyBase.New()
        Me.AddRange(value)
    End Sub

    '<summary>
    '    <para>
    '      Initializes a new instance of <see cref='BackgroundCopy.BITSJobCollection'/> containing any array of <see cref='BackgroundCopy.BITSJob'/> objects.
    '   </para>
    '</summary>
    '<param name='value'>
    '      A array of <see cref='BackgroundCopy.BITSJob'/> objects with which to intialize the collection
    '</param>
    Public Sub New(ByVal value() As BITSJob)
        MyBase.New()
        Me.AddRange(value)
    End Sub

    '<summary>
    '<para>Represents the entry at the specified index of the <see cref='BackgroundCopy.BITSJob'/>.</para>
    '</summary>
    '<param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
    '<value>
    '   <para> The entry at the specified index of the collection.</para>
    '</value>
    '<exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
    Default Public Property Item(ByVal index As Integer) As BITSJob
        Get
            Return CType(List(index), BITSJob)
        End Get
        Set(ByVal Value As BITSJob)
            List(index) = value
        End Set
    End Property

    '<summary>
    '   <para>Adds a <see cref='BackgroundCopy.BITSJob'/> with the specified value to the 
    '   <see cref='BackgroundCopy.BITSJobCollection'/> .</para>
    '</summary>
    '<param name='value'>The <see cref='BackgroundCopy.BITSJob'/> to add.</param>
    '<returns>
    '   <para>The index at which the new element was inserted.</para>
    '</returns>
    '<seealso cref='BackgroundCopy.BITSJobCollection.AddRange'/>
    Public Function Add(ByVal value As BITSJob) As Integer
        Return List.Add(value)
    End Function

    '<summary>
    '<para>Copies the elements of an array to the end of the <see cref='BackgroundCopy.BITSJobCollection'/>.</para>
    '</summary>
    '<param name='value'>
    '   An array of type <see cref='BackgroundCopy.BITSJob'/> containing the objects to add to the collection.
    '</param>
    '<returns>
    '  <para>None.</para>
    '</returns>
    '<seealso cref='BackgroundCopy.BITSJobCollection.Add'/>
    Public Overloads Sub AddRange(ByVal value() As BITSJob)
        Dim i As Integer = 0
        Do While (i < value.Length)
            Me.Add(value(i))
            i = (i + 1)
        Loop
    End Sub

    '<summary>
    '    <para>
    '      Adds the contents of another <see cref='BackgroundCopy.BITSJobCollection'/> to the end of the collection.
    '   </para>
    '</summary>
    '<param name='value'>
    '   A <see cref='BackgroundCopy.BITSJobCollection'/> containing the objects to add to the collection.
    '</param>
    '<returns>
    '  <para>None.</para>
    '</returns>
    '<seealso cref='BackgroundCopy.BITSJobCollection.Add'/>
    Public Overloads Sub AddRange(ByVal value As BITSJobCollection)
        Dim i As Integer = 0
        Do While (i < value.Count)
            Me.Add(value(i))
            i = (i + 1)
        Loop
    End Sub

    '<summary>
    '<para>Gets a value indicating whether the 
    '   <see cref='BackgroundCopy.BITSJobCollection'/> contains the specified <see cref='BackgroundCopy.BITSJob'/>.</para>
    '</summary>
    '<param name='value'>The <see cref='BackgroundCopy.BITSJob'/> to locate.</param>
    '<returns>
    '<para><see langword='true'/> if the <see cref='BackgroundCopy.BITSJob'/> is contained in the collection; 
    '  otherwise, <see langword='false'/>.</para>
    '</returns>
    '<seealso cref='BackgroundCopy.BITSJobCollection.IndexOf'/>
    Public Function Contains(ByVal value As BITSJob) As Boolean
        Return List.Contains(value)
    End Function

    '<summary>
    '<para>Copies the <see cref='BackgroundCopy.BITSJobCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
    '   specified index.</para>
    '</summary>
    '<param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='BackgroundCopy.BITSJobCollection'/> .</para></param>
    '<param name='index'>The index in <paramref name='array'/> where copying begins.</param>
    '<returns>
    '  <para>None.</para>
    '</returns>
    '<exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='BackgroundCopy.BITSJobCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
    '<exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
    '<exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
    '<seealso cref='System.Array'/>
    Public Sub CopyTo(ByVal array() As BITSJob, ByVal index As Integer)
        List.CopyTo(array, index)
    End Sub

    '<summary>
    '   <para>Returns the index of a <see cref='BackgroundCopy.BITSJob'/> in 
    '      the <see cref='BackgroundCopy.BITSJobCollection'/> .</para>
    '</summary>
    '<param name='value'>The <see cref='BackgroundCopy.BITSJob'/> to locate.</param>
    '<returns>
    '<para>The index of the <see cref='BackgroundCopy.BITSJob'/> of <paramref name='value'/> in the 
    '<see cref='BackgroundCopy.BITSJobCollection'/>, if found; otherwise, -1.</para>
    '</returns>
    '<seealso cref='BackgroundCopy.BITSJobCollection.Contains'/>
    Public Function IndexOf(ByVal value As BITSJob) As Integer
        Return List.IndexOf(value)
    End Function

    '<summary>
    '<para>Inserts a <see cref='BackgroundCopy.BITSJob'/> into the <see cref='BackgroundCopy.BITSJobCollection'/> at the specified index.</para>
    '</summary>
    '<param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
    '<param name=' value'>The <see cref='BackgroundCopy.BITSJob'/> to insert.</param>
    '<returns><para>None.</para></returns>
    '<seealso cref='BackgroundCopy.BITSJobCollection.Add'/>
    Public Sub Insert(ByVal index As Integer, ByVal value As BITSJob)
        List.Insert(index, value)
    End Sub

    '<summary>
    '   <para>Returns an enumerator that can iterate through 
    '      the <see cref='BackgroundCopy.BITSJobCollection'/> .</para>
    '</summary>
    '<returns><para>None.</para></returns>
    '<seealso cref='System.Collections.IEnumerator'/>
    Public Shadows Function GetEnumerator() As BITSJobEnumerator
        Return New BITSJobEnumerator(Me)
    End Function

    '<summary>
    '   <para> Removes a specific <see cref='BackgroundCopy.BITSJob'/> from the 
    '   <see cref='BackgroundCopy.BITSJobCollection'/> .</para>
    '</summary>
    '<param name='value'>The <see cref='BackgroundCopy.BITSJob'/> to remove from the <see cref='BackgroundCopy.BITSJobCollection'/> .</param>
    '<returns><para>None.</para></returns>
    '<exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
    Public Sub Remove(ByVal value As BITSJob)
        List.Remove(value)
    End Sub
End Class

Public Class BITSJobEnumerator
    Inherits Object
    Implements IEnumerator

    Private baseEnumerator As IEnumerator

    Private temp As IEnumerable

    Public Sub New(ByVal mappings As BITSJobCollection)
        MyBase.New()
        Me.temp = CType(mappings, IEnumerable)
        Me.baseEnumerator = temp.GetEnumerator
    End Sub

    Public ReadOnly Property Current() As BITSJob
        Get
            Return CType(baseEnumerator.Current, BITSJob)
        End Get
    End Property

    ReadOnly Property IEnumerator_Current() As Object Implements IEnumerator.Current
        Get
            Return baseEnumerator.Current
        End Get
    End Property

    Public Function MoveNext() As Boolean
        Return baseEnumerator.MoveNext
    End Function

    Function IEnumerator_MoveNext() As Boolean Implements IEnumerator.MoveNext
        Return baseEnumerator.MoveNext
    End Function

    Public Sub Reset()
        baseEnumerator.Reset()
    End Sub

    Sub IEnumerator_Reset() Implements IEnumerator.Reset
        baseEnumerator.Reset()
    End Sub
End Class
