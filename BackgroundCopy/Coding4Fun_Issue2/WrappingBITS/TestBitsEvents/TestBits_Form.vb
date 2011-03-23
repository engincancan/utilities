Option Strict On
Option Explicit On 

Imports Microsoft.Msdn.Samples.BITS
Imports System
Imports System.Windows.Forms
Imports System.Text

Public Class frmTestBITS
    Inherits System.Windows.Forms.Form
    Dim WithEvents myEvents As New JobEvents()
    Dim myJobs As JobCollection

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents lbJobs As System.Windows.Forms.ListBox
    Friend WithEvents txtEvents As System.Windows.Forms.TextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.lbJobs = New System.Windows.Forms.ListBox()
        Me.txtEvents = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'lbJobs
        '
        Me.lbJobs.Location = New System.Drawing.Point(24, 16)
        Me.lbJobs.Name = "lbJobs"
        Me.lbJobs.Size = New System.Drawing.Size(240, 134)
        Me.lbJobs.TabIndex = 0
        '
        'txtEvents
        '
        Me.txtEvents.Location = New System.Drawing.Point(24, 160)
        Me.txtEvents.Multiline = True
        Me.txtEvents.Name = "txtEvents"
        Me.txtEvents.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtEvents.Size = New System.Drawing.Size(240, 96)
        Me.txtEvents.TabIndex = 1
        Me.txtEvents.Text = ""
        '
        'frmTestBITS
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(292, 266)
        Me.Controls.AddRange(New System.Windows.Forms.Control() {Me.txtEvents, Me.lbJobs})
        Me.Name = "frmTestBITS"
        Me.Text = "Testing BITS Wrapper"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub frmTestBITS_Load(ByVal sender As System.Object, _
                                 ByVal e As System.EventArgs) _
                                 Handles MyBase.Load
        Dim myBITS As New Manager()
        myJobs = myBITS.GetListofJobs(JobType.AllUsers)
        Dim currentJob As Job
        For Each currentJob In myJobs
            myEvents.AddJob(currentJob)
        Next
        lbJobs.DataSource = myJobs
    End Sub

    Private Sub myEvents_JobModification(ByVal sender As Object, _
                                         ByVal e As JobEventArgs) _
                                         Handles myEvents.JobModification
        Dim sb As New StringBuilder()
        sb.AppendFormat("{0} ({1})", _
                e.JobName, _
                e.Job.StateString)
        sb.Append(Environment.NewLine)
        sb.Append(txtEvents.Text)
        txtEvents.Text = sb.ToString
    End Sub

    Private Sub myEvents_JobError(ByVal sender As Object, _
                                  ByVal e As JobErrorEventArgs) _
                                  Handles myEvents.JobError

        Dim sb As New StringBuilder()
        sb.AppendFormat("{0} ({1})", _
                e.JobName, _
                e.GetErrorDescription())
        sb.Append(Environment.NewLine)
        sb.Append(txtEvents.Text)
        txtEvents.Text = sb.ToString
    End Sub

    Private Sub myEvents_JobTransferred(ByVal sender As Object, _
                                        ByVal e As JobEventArgs) _
                                        Handles myEvents.JobTransferred
        Dim sb As New StringBuilder()
        sb.AppendFormat("{0} ({1})", _
                e.JobName, _
                "Transferred")
        sb.Append(Environment.NewLine)
        sb.Append(txtEvents.Text)
        txtEvents.Text = sb.ToString
    End Sub
End Class
