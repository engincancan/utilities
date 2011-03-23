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
Imports System.IO
Imports System.IO.IsolatedStorage
Imports System.Windows.Forms
Imports Microsoft.Msdn.Samples


Public Class frmBackgroundCopy
    Inherits System.Windows.Forms.Form

#Region "Class level instance variables"
    'Keep an instance of BITS, the user settings 
    'and the current set of BITS jobs
    Private m_Options As Options
    Private m_BITS As BITS.Manager
    Private m_Jobs As BITSJobCollection
#End Region

    Private Sub SetupBITS()
        'create an instance of the BITS manager
        'Load the current set of jobs into the local
        'jobs collection and then bind to the DataGrid
        If m_BITS Is Nothing Then
            m_BITS = New BITS.Manager()
        End If
        LoadJobsCollection(m_BITS)
        SetupDataBinding()
    End Sub

    Private Sub SetupDataBinding()
        FormatDataGrid()
        Me.dgJobs.DataSource = m_Jobs
    End Sub

    Private Sub FormatDataGrid()
        'icky, boring code to format the grid
        'start out with a table style created from
        'our data source (the jobs collection)
        Dim ts As New DataGridTableStyle( _
            DirectCast(Me.BindingContext(m_Jobs), _
            CurrencyManager))

        ts.AllowSorting = True
        ts.AlternatingBackColor = Color.LightBlue
        ts.ColumnHeadersVisible = True
        ts.ForeColor = Color.Black
        ts.GridLineColor = Color.Black
        ts.GridLineStyle = DataGridLineStyle.Solid
        ts.PreferredColumnWidth = 50
        ts.ReadOnly = True
        ts.RowHeadersVisible = True
        ts.PreferredRowHeight = 22

        ts.GridColumnStyles.Clear()

        'create each column one at a time.
        'I could have left some of them in their default
        'state, or even let the grid auto-generate all of
        'them, but I like the control I get from doing them
        'myself.

        Dim gcs As DataGridColumnStyle
        gcs = New DataGridTextBoxColumn()
        gcs.MappingName = "JobStatus"
        gcs.HeaderText = "Status"
        gcs.ReadOnly = True

        ts.GridColumnStyles.Add(gcs)

        gcs = New DataGridTextBoxColumn()
        gcs.MappingName = "JobDateStarted"
        gcs.HeaderText = "Date Started"
        gcs.ReadOnly = True

        ts.GridColumnStyles.Add(gcs)

        'set up the progress bar column
        Dim prog As _
            New WindowsForms.DataGridProgressColumnStyle()
        prog.ProgressBarBackground = New SolidBrush(ts.BackColor)
        prog.ProgressBarBrush = Brushes.Indigo
        prog.textColor = Color.White
        prog.altTextColor = Color.Black
        prog.MappingName = "JobProgress"
        prog.HeaderText = "Progress"
        prog.ReadOnly = True

        ts.GridColumnStyles.Add(prog)


        gcs = New DataGridTextBoxColumn()
        gcs.MappingName = "JobCompleted"
        gcs.HeaderText = "Date Completed"
        gcs.ReadOnly = True

        ts.GridColumnStyles.Add(gcs)

        gcs = New DataGridTextBoxColumn()
        gcs.MappingName = "JobSource"
        gcs.HeaderText = "Source URL"
        gcs.ReadOnly = True

        ts.GridColumnStyles.Add(gcs)

        gcs = New DataGridTextBoxColumn()
        gcs.MappingName = "JobTarget"
        gcs.HeaderText = "Destination"
        gcs.ReadOnly = True

        ts.GridColumnStyles.Add(gcs)

        dgJobs.TableStyles.Add(ts)

    End Sub

    Private Sub LoadJobsCollection(ByVal mgr As BITS.Manager)
        'populate my local collection from the 
        '"real" collection returned by the BITS manager.
        m_Jobs = New BITSJobCollection()
        Dim myJobs As BITS.JobCollection = mgr.GetListofJobs(BITS.JobType.CurrentUser)
        Dim myJob As BITS.Job

        For Each myJob In myJobs
            m_Jobs.Add(New BITSJob(mgr, myJob.ID))
        Next
        Dim myBITSJob As BITSJob
        For Each myBITSJob In m_Jobs
            myBITSJob.Refresh()
        Next
    End Sub

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()
        m_Options = UserSettings.GetSettings

        'Add any initialization after the InitializeComponent() call
        Me.SetupBITS()

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
    Friend WithEvents refreshJobs As System.Windows.Forms.Timer
    Friend WithEvents dgJobs As System.Windows.Forms.DataGrid
    Friend WithEvents addNew As System.Windows.Forms.Button
    Friend WithEvents cancelAll As System.Windows.Forms.Button
    Friend WithEvents exitApp As System.Windows.Forms.Button
    Friend WithEvents mnuDG As System.Windows.Forms.ContextMenu
    Friend WithEvents mnuCancelJob As System.Windows.Forms.MenuItem
    Friend WithEvents mnuSuspendResume As System.Windows.Forms.MenuItem
    Friend WithEvents bkgCopyMenu As System.Windows.Forms.MainMenu
    Friend WithEvents mnuFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuSetDefaultLocation As System.Windows.Forms.MenuItem
    Friend WithEvents mnuExit As System.Windows.Forms.MenuItem
    Friend WithEvents mnuCancelAllJobs As System.Windows.Forms.MenuItem
    Friend WithEvents mnuNewJob As System.Windows.Forms.MenuItem
    Friend WithEvents defaultSaveLocation As Microsoft.Samples.WinForms.Extras.FolderBrowser
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(frmBackgroundCopy))
        Me.addNew = New System.Windows.Forms.Button()
        Me.cancelAll = New System.Windows.Forms.Button()
        Me.exitApp = New System.Windows.Forms.Button()
        Me.dgJobs = New System.Windows.Forms.DataGrid()
        Me.refreshJobs = New System.Windows.Forms.Timer(Me.components)
        Me.mnuDG = New System.Windows.Forms.ContextMenu()
        Me.mnuCancelJob = New System.Windows.Forms.MenuItem()
        Me.mnuSuspendResume = New System.Windows.Forms.MenuItem()
        Me.bkgCopyMenu = New System.Windows.Forms.MainMenu()
        Me.mnuFile = New System.Windows.Forms.MenuItem()
        Me.mnuNewJob = New System.Windows.Forms.MenuItem()
        Me.mnuCancelAllJobs = New System.Windows.Forms.MenuItem()
        Me.mnuSetDefaultLocation = New System.Windows.Forms.MenuItem()
        Me.mnuExit = New System.Windows.Forms.MenuItem()
        Me.defaultSaveLocation = New Microsoft.Samples.WinForms.Extras.FolderBrowser()
        CType(Me.dgJobs, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'addNew
        '
        Me.addNew.Anchor = (System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right)
        Me.addNew.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.addNew.Location = New System.Drawing.Point(368, 207)
        Me.addNew.Name = "addNew"
        Me.addNew.Size = New System.Drawing.Size(72, 23)
        Me.addNew.TabIndex = 0
        Me.addNew.Text = "Add New"
        '
        'cancelAll
        '
        Me.cancelAll.Anchor = (System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right)
        Me.cancelAll.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cancelAll.Location = New System.Drawing.Point(448, 207)
        Me.cancelAll.Name = "cancelAll"
        Me.cancelAll.Size = New System.Drawing.Size(72, 23)
        Me.cancelAll.TabIndex = 1
        Me.cancelAll.Text = "Cancel All"
        '
        'exitApp
        '
        Me.exitApp.Anchor = (System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right)
        Me.exitApp.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.exitApp.Location = New System.Drawing.Point(528, 207)
        Me.exitApp.Name = "exitApp"
        Me.exitApp.Size = New System.Drawing.Size(48, 23)
        Me.exitApp.TabIndex = 2
        Me.exitApp.Text = "&Exit"
        '
        'dgJobs
        '
        Me.dgJobs.AllowDrop = True
        Me.dgJobs.Anchor = (((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right)
        Me.dgJobs.CaptionFont = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.dgJobs.CaptionText = "Drag Links from Your Browser into the Grid to Start Download"
        Me.dgJobs.DataMember = ""
        Me.dgJobs.HeaderForeColor = System.Drawing.SystemColors.ControlText
        Me.dgJobs.Location = New System.Drawing.Point(8, 16)
        Me.dgJobs.Name = "dgJobs"
        Me.dgJobs.ReadOnly = True
        Me.dgJobs.Size = New System.Drawing.Size(568, 183)
        Me.dgJobs.TabIndex = 3
        '
        'refreshJobs
        '
        Me.refreshJobs.Enabled = True
        Me.refreshJobs.Interval = 1000
        '
        'mnuDG
        '
        Me.mnuDG.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuCancelJob, Me.mnuSuspendResume})
        '
        'mnuCancelJob
        '
        Me.mnuCancelJob.Index = 0
        Me.mnuCancelJob.Text = "Cancel Job"
        '
        'mnuSuspendResume
        '
        Me.mnuSuspendResume.Index = 1
        Me.mnuSuspendResume.Text = "Suspend Job"
        '
        'bkgCopyMenu
        '
        Me.bkgCopyMenu.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFile})
        '
        'mnuFile
        '
        Me.mnuFile.Index = 0
        Me.mnuFile.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuNewJob, Me.mnuCancelAllJobs, Me.mnuSetDefaultLocation, Me.mnuExit})
        Me.mnuFile.Text = "&File"
        '
        'mnuNewJob
        '
        Me.mnuNewJob.Index = 0
        Me.mnuNewJob.Text = "&New Job"
        '
        'mnuCancelAllJobs
        '
        Me.mnuCancelAllJobs.Index = 1
        Me.mnuCancelAllJobs.Text = "&Cancel All Jobs"
        '
        'mnuSetDefaultLocation
        '
        Me.mnuSetDefaultLocation.Index = 2
        Me.mnuSetDefaultLocation.Text = "&Set Default Download Location"
        '
        'mnuExit
        '
        Me.mnuExit.Index = 3
        Me.mnuExit.Text = "E&xit"
        '
        'defaultSaveLocation
        '
        Me.defaultSaveLocation.Description = "Please select the folder where downloads should be placed:"
        Me.defaultSaveLocation.ShowTextBox = True
        Me.defaultSaveLocation.StartLocation = Microsoft.Samples.WinForms.Extras.FolderBrowser.FolderID.MyComputer
        '
        'frmBackgroundCopy
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(584, 241)
        Me.Controls.AddRange(New System.Windows.Forms.Control() {Me.dgJobs, Me.exitApp, Me.cancelAll, Me.addNew})
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Menu = Me.bkgCopyMenu
        Me.Name = "frmBackgroundCopy"
        Me.Text = "Background Copy"
        CType(Me.dgJobs, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub refreshJobs_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles refreshJobs.Tick
        'The classes returned by BITS are static "snapshots"
        'of the job at that moment in time. The progress and status
        'won't change unless you keep requesting new snapshots
        'you could also do this by hooking a bunch of BITS events and then
        'refreshing only when you catch a change. This works fairly well though
        'and one check per second isn't very intensive... could make it slower
        'if you wished.
        Dim myBITSJob As BITSJob
        For Each myBITSJob In m_Jobs
            myBITSJob.Refresh()
            Select Case myBITSJob.JobStatus
                Case BITS.JobState.Transferred
                    Dim myJob As BITS.Job
                    Try
                        myJob = m_BITS.GetJob(myBITSJob.JobID)
                        If Not myJob Is Nothing Then
                            myJob.Complete()
                        End If
                    Catch ex As Exception
                        Debug.WriteLine(ex.ToString)
                    End Try
                Case BITS.JobState.Errors
                    Try
                        Dim myJob As BITS.Job = m_BITS.GetJob(myBITSJob.JobID)
                        Debug.WriteLine(myJob.GetError.Description)
                    Catch ex As Exception
                        Debug.WriteLine("Exception: " & ex.ToString)
                    End Try
            End Select
        Next
        Me.dgJobs.Refresh()
    End Sub

    Private Sub dgJobs_DragDrop(ByVal sender As Object, _
            ByVal e As DragEventArgs) _
        Handles dgJobs.DragDrop

        'allow user to drag links into the grid.

        Try
            If e.Data.GetDataPresent(DataFormats.Text, _
                    True) Then
                e.Effect = DragDropEffects.Link
                'should contain the URL if a link is dragged in from IE
                Dim sURL As String = _
                    CStr(e.Data.GetData(DataFormats.Text, True))
                Dim myURI As New Uri(sURL)
                Dim fileName As String = _
                    Path.GetFileName(myURI.LocalPath)
                Dim localPath As String = _
                    Path.Combine( _
                            Me.m_Options.defaultSaveLocation, _
                            fileName)

                Dim newJob As BITS.Job
                newJob = Me.m_BITS.CreateJob(fileName)
                newJob.AddFile(localPath, sURL)
                newJob.ResumeJob()

                Dim myJob As New BITSJob(m_BITS, newJob.ID)
                myJob.Refresh()
                m_Jobs.Add(myJob)


                'If this grid was bound to a dataview then I 
                'wouldn't have to refresh it, because the dataview
                'implements IBindingList and fires an event whenever
                'its contents have changed and the display should be
                'refreshed. Since my collection doesn't implement 
                'IBindingList (though it could), I have to force the
                'refresh.
                CType(Me.BindingContext(dgJobs.DataSource), _
                    CurrencyManager).Refresh()

            Else
                e.Effect = DragDropEffects.None
            End If
        Catch ex As Exception
            e.Effect = DragDropEffects.None
            MsgBox(ex.ToString)
        End Try
    End Sub

    Private Sub dgJobs_DragEnter(ByVal sender As Object, _
            ByVal e As DragEventArgs) _
        Handles dgJobs.DragEnter

        Try
            If e.Data.GetDataPresent(DataFormats.Text, True) Then
                e.Effect = DragDropEffects.Link
            Else
                e.Effect = DragDropEffects.None
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try

    End Sub



    Private Sub cancelAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cancelAll.Click
        Dim myJob As BITSJob

        For Each myJob In Me.m_Jobs
            myJob.CancelJob()
        Next

        Dim cm As CurrencyManager = DirectCast(Me.BindingContext(dgJobs.DataSource), CurrencyManager)
        cm.Refresh()

    End Sub

    Private Sub exitApp_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles exitApp.Click
        Me.Close()
    End Sub

    Private Sub dgJobs_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles dgJobs.MouseDown
        If CBool(e.Button And MouseButtons.Right) Then
            'Get selected row
            Dim row As Integer = dgJobs.HitTest(e.X, e.Y).Row

            If row <> -1 Then
                dgJobs.CurrentRowIndex = row
                Me.m_Jobs(row).Refresh()
                If Me.m_Jobs(row).JobStatus = BITS.JobState.Suspended Then
                    mnuSuspendResume.Text = "Resume"
                Else
                    mnuSuspendResume.Text = "Suspend"
                End If
                mnuDG.Show(dgJobs, New Point(e.X, e.Y))
            End If
        End If
    End Sub

    Private Sub mnuSetDefaultLocation_Click( _
            ByVal sender As System.Object, _
            ByVal e As System.EventArgs) _
        Handles mnuSetDefaultLocation.Click

        If defaultSaveLocation.ShowDialog() = _
            DialogResult.OK Then
            m_Options.defaultSaveLocation = _
                defaultSaveLocation.DirectoryPath
        End If
    End Sub



    Private Sub frmBackgroundCopy_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Closed
        UserSettings.SaveSettings(m_Options)
    End Sub

    Private Sub mnuCancelJob_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuCancelJob.Click
        If dgJobs.CurrentRowIndex <> -1 Then
            Dim myJob As BITSJob
            myJob = Me.m_Jobs(dgJobs.CurrentRowIndex)
            If MsgBox(String.Format("Cancel {0} now?", myJob.JobSource), _
                MsgBoxStyle.YesNo, "Cancel Job") = _
                MsgBoxResult.Yes Then
                myJob.CancelJob()
            End If
        End If
    End Sub

    Private Sub mnuSuspendResume_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuSuspendResume.Click
        If dgJobs.CurrentRowIndex <> -1 Then
            Dim myJob As BITSJob
            myJob = Me.m_Jobs(dgJobs.CurrentRowIndex)
            If mnuSuspendResume.Text = "Suspend" Then
                myJob.SuspendJob()
            Else
                myJob.ResumeJob()
            End If
            myJob.Refresh()
        End If
    End Sub

    Private Sub addNew_Click(ByVal sender As Object, _
            ByVal e As System.EventArgs) _
            Handles addNew.Click
        Dim newJobDialog As New addNewJob()
        newJobDialog.Options = Me.m_Options
        Dim newJobResult As DialogResult

        newJobResult = newJobDialog.ShowDialog

        If newJobResult = DialogResult.OK Then

            Dim sURL As String = _
                newJobDialog.SourceURL

            Dim myURI As New Uri(sURL)
            Dim localPath As String = newJobDialog.Target
            Dim fileName As String = _
                Path.GetFileName(localPath)

            Dim newJob As BITS.Job
            newJob = Me.m_BITS.CreateJob(fileName)
            newJob.AddFile(localPath, sURL)
            newJob.ResumeJob()

            Dim myJob As New BITSJob(m_BITS, newJob.ID)
            myJob.Refresh()
            m_Jobs.Add(myJob)

            CType(Me.BindingContext(dgJobs.DataSource), _
                CurrencyManager).Refresh()

        End If
    End Sub
End Class

Public Class BITSJob
    Implements System.ComponentModel.IDataErrorInfo

    Dim m_JobID As Guid
    Dim m_JobProgress As Double
    Dim m_JobSource As String
    Dim m_JobTarget As String
    Dim m_JobStatus As BITS.JobState
    Dim m_JobDateStarted As Date
    Dim m_JobCompleted As Date
    Dim m_BITSManager As BITS.Manager
    Dim m_JobError As String
    Dim m_Job As BITS.Job
    Dim m_Completed As Boolean = False

    Public Property ElapsedTime() As Integer
        Get
            Dim ts As TimeSpan = Now.Subtract(Me.JobDateStarted)
            Return CInt(ts.TotalMinutes())

        End Get
        Set(ByVal Value As Integer)

        End Set
    End Property

    Public Sub Refresh()
        m_Job = GetJob()
        RefreshProperties(m_Job)
    End Sub
    Private Sub RefreshProperties(ByVal myJob As BITS.Job)

        If Not myJob Is Nothing AndAlso myJob.Files.Count >= 1 Then
            m_JobProgress = GetProgress(myJob.Files(0).Progress)
            m_JobStatus = myJob.State
            m_JobSource = myJob.Files(0).RemoteName
            m_JobTarget = myJob.Files(0).LocalName
            m_JobDateStarted = myJob.CreationTime
            If myJob.Progress.FilesTransferred = myJob.Progress.FilesTotal Then
                m_JobCompleted = myJob.TransferCompletionTime
            Else
                m_JobCompleted = Nothing
            End If
        ElseIf myJob Is Nothing Then

        End If

    End Sub

    Private Function GetProgress(ByVal prog As BITS.FileProgress) As Double
        Dim progress As Double
        progress = CDbl((prog.BytesTransferred / prog.BytesTotal))
        Return progress
    End Function

    Public Property JobID() As Guid
        Get
            Return m_JobID
        End Get
        Set(ByVal Value As Guid)
            m_JobID = Value
        End Set
    End Property
    Public Property JobProgress() As Double
        Get
            Return m_JobProgress
        End Get
        Set(ByVal Value As Double)
            m_JobProgress = Value
        End Set
    End Property
    Public Property JobSource() As String
        Get
            Return m_JobSource
        End Get
        Set(ByVal Value As String)
            m_JobSource = Value
        End Set
    End Property
    Public Property JobTarget() As String
        Get
            Return m_JobTarget
        End Get
        Set(ByVal Value As String)
            m_JobTarget = Value
        End Set
    End Property
    Public Property JobStatus() As BITS.JobState
        Get
            Return m_JobStatus
        End Get
        Set(ByVal Value As BITS.JobState)
            m_JobStatus = Value
        End Set
    End Property
    Public Property JobDateStarted() As Date
        Get
            Return m_JobDateStarted
        End Get
        Set(ByVal Value As Date)
            m_JobDateStarted = Value
        End Set
    End Property
    Public Property JobCompleted() As Date
        Get
            Return m_JobCompleted
        End Get
        Set(ByVal Value As Date)
            m_JobCompleted = Value
        End Set
    End Property



    Friend Sub New(ByVal mgr As BITS.Manager, ByVal JobID As Guid)
        m_BITSManager = mgr
        m_JobID = JobID
    End Sub

    Private Function GetJob() As BITS.Job
        Try
            Return m_BITSManager.GetJob(m_JobID)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Sub CancelJob()
        Dim myJob As BITS.Job
        myJob = GetJob()
        If Not myJob Is Nothing Then myJob.Cancel()
        RefreshProperties(myJob)
    End Sub

    Public Sub SuspendJob()
        Dim myJob As BITS.Job
        myJob = GetJob()
        If Not myJob Is Nothing Then myJob.Suspend()
        RefreshProperties(myJob)

    End Sub

    Public Sub ResumeJob()
        Dim myJob As BITS.Job
        myJob = GetJob()
        If Not myJob Is Nothing Then myJob.ResumeJob()
        RefreshProperties(myJob)

    End Sub

    Public Sub CompleteJob()
        Dim myJob As BITS.Job = GetJob()
        If Not myJob Is Nothing AndAlso _
            myJob.State = BITS.JobState.Transferred Then
            myJob.Complete()
            RefreshProperties(myJob)
        End If
    End Sub

    <System.ComponentModel.Bindable(False)> Public ReadOnly Property JobError() As String Implements System.ComponentModel.IDataErrorInfo.Error
        Get
            Dim job As BITS.Job = GetJob()
            If Not job Is Nothing AndAlso job.State = BITS.JobState.Errors Then
                Return m_Job.GetError.Description
            Else
                Return Nothing
            End If
        End Get
    End Property

    Default Public ReadOnly Property Item( _
        ByVal columnName As String) As String Implements System.ComponentModel.IDataErrorInfo.Item
        Get
            Return Nothing
        End Get
    End Property
End Class

