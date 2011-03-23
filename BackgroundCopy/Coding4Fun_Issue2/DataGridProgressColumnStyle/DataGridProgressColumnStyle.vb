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
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Globalization
Imports System.Diagnostics

Public Class DataGridProgressColumnStyle
    Inherits DataGridColumnStyle

    ' UI Constants
    Private xMargin As Integer = 0
    Private yMargin As Integer = 2

    Public ProgressBarBrush As Brush = Brushes.White
    Public ProgressBarBackground As Brush = Brushes.Black
    Public textColor As Color = Color.Blue
    Public altTextColor As Color = Color.Black
    Public textFont As Font = New Font("Arial", 12, FontStyle.Bold)


    Public Sub New()
    End Sub


    Protected Overrides Function GetPreferredHeight(ByVal g As System.Drawing.Graphics, ByVal value As Object) As Integer
        Return Math.Min(Me.GetPreferredSize(g, value).Height, Me.GetMinimumHeight())
    End Function

    Protected Overrides Sub Abort(ByVal rowNum As Integer)
        RollBack()
        EndEdit()
    End Sub
    Private Sub RollBack()

    End Sub
    Private Sub EndEdit()
        Me.Invalidate()
    End Sub

    Protected Overrides Function GetPreferredSize(ByVal g As System.Drawing.Graphics, ByVal value As Object) As System.Drawing.Size
        Dim text As String = String.Format("{0:P}", LookupValue(value))
        Dim dataGridLineWidth As Integer = CInt(IIf(Me.DataGridTableStyle.GridLineStyle = DataGridLineStyle.Solid, CObj(1), CObj(0)))
        Dim preferredSize As Size = Size.Ceiling(g.MeasureString(text, textFont))
        preferredSize.Width += xMargin * 2 + dataGridLineWidth + 20 '(20 is size of drop down arrow)
        preferredSize.Height += yMargin
        Return preferredSize
    End Function

    Protected Overrides Function GetMinimumHeight() As Integer
        Return 10 + yMargin

    End Function

    Protected Overrides Function Commit(ByVal dataSource As System.Windows.Forms.CurrencyManager, ByVal rowNum As Integer) As Boolean
        Return True
    End Function

    Protected Overloads Overrides Sub Paint(ByVal g As System.Drawing.Graphics, ByVal bounds As System.Drawing.Rectangle, ByVal source As System.Windows.Forms.CurrencyManager, ByVal rowNum As Integer, ByVal backBrush As System.Drawing.Brush, ByVal foreBrush As System.Drawing.Brush, ByVal alignToRight As Boolean)
        'main painting routine
        Dim value As Double = LookupValue(GetColumnValueATRow(source, rowNum))
        Dim text As String = String.Format("{0:P}", value)

        Dim rect As RectangleF = System.Drawing.RectangleF.FromLTRB(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom)

        Dim format As New StringFormat(StringFormatFlags.NoWrap)
        format.Trimming = StringTrimming.None


        If alignToRight Then format.FormatFlags = format.FormatFlags Or StringFormatFlags.DirectionRightToLeft
        If Me.Alignment = HorizontalAlignment.Left Then
            format.Alignment = StringAlignment.Near
        ElseIf Me.Alignment = HorizontalAlignment.Center Then
            format.Alignment = StringAlignment.Center
        ElseIf Me.Alignment = HorizontalAlignment.Right Then
            format.Alignment = StringAlignment.Far
        End If

        'g.FillRectangle(backBrush, rect)

        'We want painting to leave a little padding around the rectangle,
        'so reduce the size of rectangle by the margin
        'rect.Offset(0, yMargin)
        'rect.Height -= yMargin
        g.FillRectangle(Me.ProgressBarBackground, rect)

        Dim progressWidth As Double = rect.Width * value
        Dim progressRect As RectangleF = rect
        progressRect.Width = CSng(progressWidth)
        g.DrawString(text, textFont, New SolidBrush(altTextColor), rect, format)
        g.FillRectangle(Me.ProgressBarBrush, progressRect)
        g.DrawString(text, textFont, New SolidBrush(textColor), progressRect, format)
    End Sub

    Protected Overloads Overrides Sub Paint(ByVal g As System.Drawing.Graphics, ByVal bounds As System.Drawing.Rectangle, ByVal source As System.Windows.Forms.CurrencyManager, ByVal rowNum As Integer, ByVal alignToRight As Boolean)
        Dim backBrush As Brush = New SolidBrush(Me.DataGridTableStyle.BackColor)
        Dim foreBrush As Brush = New SolidBrush(Me.DataGridTableStyle.ForeColor)

        Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight)

        backBrush.Dispose()
        foreBrush.Dispose()
    End Sub

    Protected Overloads Overrides Sub Paint(ByVal g As System.Drawing.Graphics, ByVal bounds As System.Drawing.Rectangle, ByVal source As System.Windows.Forms.CurrencyManager, ByVal rowNum As Integer)
        Paint(g, bounds, source, rowNum, False)
    End Sub

    Protected Overloads Overrides Sub Edit(ByVal source As System.Windows.Forms.CurrencyManager, ByVal rowNum As Integer, ByVal bounds As System.Drawing.Rectangle, ByVal [readOnly] As Boolean, ByVal instantText As String, ByVal cellIsVisible As Boolean)

    End Sub


    Protected Overrides Sub SetDataGridInColumn(ByVal value As System.Windows.Forms.DataGrid)
        MyBase.SetDataGridInColumn(value)
    End Sub

    Protected Overrides Sub UpdateUI(ByVal source As System.Windows.Forms.CurrencyManager, ByVal rowNum As Integer, ByVal instantText As String)
        Me.Invalidate()
    End Sub

    Private Function LookupValue(ByVal value As Object) As Double
        If value Is Nothing OrElse IsDBNull(value) Then Return Nothing
        Return CDbl(value)
    End Function

    Protected Overrides Sub ConcedeFocus()
    End Sub
End Class
