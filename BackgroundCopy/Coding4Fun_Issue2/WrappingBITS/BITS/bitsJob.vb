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
    Public Class Job
        'Wrapper class that corresponds to IBackgroundCopyJob

        Protected Friend m_Job As IBackgroundCopyJob

        Public Overrides Function ToString() As String
            'Override default ToString behaviour to support direct use in
            'Console.WriteLine and other locations where ToString is used.
            Return String.Format("{0} ({1})", Me.DisplayName, Me.StateString)
        End Function

        Public Function CreationTime() As Date
            'Time the Job was created
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_6r3n.asp
            Dim times As _BG_JOB_TIMES
            m_Job.GetTimes(times)

            Return ConvertToDateTime(times.CreationTime)
        End Function

        Public Function GetError() As bitsError
            Dim err As IBackgroundCopyError
            m_Job.GetError(err)

            If Not err Is Nothing Then
                Dim bErr As New bitsError(err)
                Return bErr
            Else
                Return Nothing
            End If
        End Function

        Public Function ModificationTime() As Date
            'Time the job was last modified or bytes were transferred.
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_6r3n.asp
            Dim times As _BG_JOB_TIMES
            m_Job.GetTimes(times)
            Return ConvertToDateTime(times.ModificationTime)
        End Function

        Public Function TransferCompletionTime() As Date
            'Time the job entered the Transferred state (Job.State)
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_6r3n.asp
            Dim times As _BG_JOB_TIMES
            m_Job.GetTimes(times)
            Return ConvertToDateTime(times.TransferCompletionTime)
        End Function

        Private Function ConvertToDateTime(ByVal value As _FILETIME) As DateTime
            'Utility Function to Convert from the _FILETIME structures returned
            'by BITS into a Sytem.DateTime value usable by .NET programs.
            'Localization
            Dim fileTime(7) As Byte
            fileTime.Copy(BitConverter.GetBytes(value.dwHighDateTime), 0, fileTime, 4, 4)

            fileTime.Copy(BitConverter.GetBytes(value.dwLowDateTime), 0, fileTime, 0, 4)
            Return DateTime.FromFileTime(BitConverter.ToInt64(fileTime, 0))
        End Function

        Public Function Files() As FileCollection
            'Returns a collection of all the Files in this particular job
            'Uses the IBackgroundCopyJob::EnumFiles routine
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_3v77.asp

            Dim myFileList As New FileCollection()
            Dim jobFiles As IEnumBackgroundCopyFiles
            Dim retrievedFile As IBackgroundCopyFile
            Dim currentFile As BITSFile

            'It is no longer normal practice to use hungarian notation,
            'but in this case the data type is the only difference between
            'these two variables.
            Dim uintFetched As UInt32 = Convert.ToUInt32(0)
            Dim intFetched As Integer
            m_Job.EnumFiles(jobFiles)
            Do
                jobFiles.Next(Convert.ToUInt32(1), retrievedFile, uintFetched)
                intFetched = Convert.ToInt32(uintFetched)
                If intFetched = 1 Then
                    myFileList.Add(New BITSFile(retrievedFile))
                End If
            Loop While intFetched = 1
            Return myFileList
        End Function

        Public Sub Cancel()
            'Possible Errors
            'BG_S_UNABLE_TO_DELETE_FILES    Job was successfully canceled; however, the service was unable to delete the temporary files associated with the job. 
            'BG_E_INVALID_STATE             Cannot cancel a job whose state is BG_JOB_STATE_CANCELLED or BG_JOB_STATE_ACKNOWLEDGED.
            'Corresponds to IBackgroundCopyJob::Cancel
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_02i4.asp
            m_Job.Cancel()
        End Sub

        Public Sub Complete()
            'Corresponds to IBackgroundCopyJob::Complete
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_0o6d.asp
            m_Job.Complete()
        End Sub

        Public Sub Suspend()
            'Corresponds to IBackgroundCopyJob::Suspend
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_5sdg.asp
            'TODO Check if job is already suspended?
            m_Job.Suspend()
        End Sub

        Public Sub ResumeJob()
            'Corresponds to IBackgroundCopyJob::Resume
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_8c2t.asp
            m_Job.Resume()
        End Sub

        Public Sub TakeOwnership()
            'Corresponds to IBackgroundCopyJob::TakeOwnership
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_3els.asp
            'Converts ownership to the current user
            m_Job.TakeOwnership()
        End Sub

        Public Sub AddFile(ByVal localFileName As String, ByVal remoteFileName As String)
            'Corresponds to IBackgroundCopyJob::AddFile
            'Note: AddFileSet not implemented in this wrapper
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_406d.asp
            'TODO Check if local file already exists?
            Try
                m_Job.AddFile(remoteFileName, localFileName)
            Catch comX As COMException
                'COM error usually indicates BITS library error
                'Localization
                Throw New BitsException(String.Format("Error Adding File ({0}).", comX.Message), comX)
            End Try
        End Sub

        Public ReadOnly Property OwnerSID() As String
            'BITS only returns the SID, SIDUtilities.LookupSID is used
            'to convert into an account name.
            'Corresponds to IBackgroundCopyJob::GetOwner
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_3prm.asp
            Get
                Dim SID As String
                Try
                    m_Job.GetOwner(SID)
                    Return SID
                Catch comX As COMException
                    'COM Error, indicates error in BITS call, likely one of these;
                    'E_INVALIDARG   SID cannot be null 
                    'E_ACCESSDENIED User does not have permission to 
                    '               retrieve the ownership information for the job. 
                    Throw New BitsException(String.Format("Error Getting Owner SID ({0}).", comX.Message), comX)
                End Try
            End Get
        End Property

        Public Function GetOwnerName() As String
            'See OwnerSID above
            Try
                Dim SID As String = Me.OwnerSID
                Return Utilities.GetAccountName(SID)
            Catch comException As comException
                'COM Error, indicates error in BITS call 
                Throw New Exception(comException.Message, comException)
            Catch genericException As Exception
                Throw New Exception(genericException.Message, genericException)
            End Try

        End Function

        Public ReadOnly Property ID() As System.Guid
            'Corresponds to IBackgroundCopyJob::GetId
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_1p5w.asp
            Get
                Try
                    Dim jobID As BackgroundCopyManager.GUID
                    m_Job.GetId(jobID)
                    Return Utilities.ConvertToGUID(jobID)
                    'BITS returns a GUID structure, which ConvertToGUID converts into
                    'a System.GUID for ease of use within .NET
                Catch comX As COMException
                    'COM error usually indicates BITS library error
                    'Localization
                    Throw New BitsException(String.Format("Error Getting Job ID ({0}).", comX.Message), comX)
                End Try
            End Get
        End Property

        Public ReadOnly Property State() As JobState
            'Corresponds to IBackgroundCopyJob::GetState
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_444l.asp
            Get
                Try
                    Dim bgState As BG_JOB_STATE
                    m_Job.GetState(bgState)
                    Return CType(bgState, JobState)
                Catch comX As COMException
                    'COM error usually indicates BITS library error
                    'Localization
                    Throw New BitsException(String.Format("Error Getting Job State ({0}).", comX.Message), comX)
                End Try
            End Get
        End Property

        Public ReadOnly Property StateString() As String
            'Returns the state (see State() property above) as a string
            Get
                Try
                    Dim myState As JobState
                    Dim myStateString As String
                    myState = Me.State
                    myStateString = Manager.JobStates(CInt(myState))
                    Return myStateString
                Catch comX As COMException
                    'COM error usually indicates BITS library error
                    'Localization
                    Throw New BitsException(String.Format("Error Getting Job State ({0}).", comX.Message), comX)
                End Try
            End Get
        End Property

        Public ReadOnly Property Progress() As JobProgress
            'Corresponds to IBackgroundCopyJob::GetProgress
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_9ewj.asp
            Get
                Try
                    Dim tmpProgress As _BG_JOB_PROGRESS
                    m_Job.GetProgress(tmpProgress)
                    Return New JobProgress(tmpProgress)
                Catch comX As COMException
                    'COM error usually indicates BITS library error
                    'Localization
                    Throw New BitsException(String.Format("Error Getting Job Progress ({0}).", comX.Message), comX)
                End Try
            End Get
        End Property

        Public ReadOnly Property ErrorCount() As Int64
            'sometimes called GetInterruptionCount in the online SDK
            'Corresponds to IBackgroundCopyJob::GetErrorCount
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_8flg.asp
            Get
                Dim countErrors As UInt32
                m_Job.GetErrorCount(countErrors)
                Return Convert.ToInt64(countErrors)
            End Get
        End Property

        Public Property DisplayName() As String
            'Corresponds to IBackgroundCopyJob::GetDisplayName/SetDisplayName
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_604l.asp (GET)
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_0gpx.asp (SET)
            Get
                Try
                    Dim jobName As String
                    m_Job.GetDisplayName(jobName)
                    Return jobName
                Catch comX As COMException
                    'COM error usually indicates BITS library error
                    'Localization
                    Throw New BitsException(String.Format("Error Getting Job Display Name ({0}).", comX.Message), comX)
                End Try
            End Get
            Set(ByVal value As String)
                Try
                    m_Job.SetDisplayName(value)
                Catch comX As COMException
                    'COM error usually indicates BITS library error
                    'Localization
                    Throw New BitsException(String.Format("Error Setting Job Name ({0}).", comX.Message), comX)
                End Try
            End Set
        End Property

        Public Property Description() As String
            'Corresponds to IBackgroundCopyJob::GetDescription/SetDescription
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_8h7y.asp (GET)
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_2xta.asp (SET)
            Get
                Dim jobDesc As String
                m_Job.GetDescription(jobDesc)
                Return jobDesc
            End Get
            Set(ByVal value As String)
                m_Job.SetDescription(value)
            End Set
        End Property

        Protected Friend Property NotifyInterface() As IBackgroundCopyCallback
            'Corresponds to IBackgroundCopyJob::GetNotifyInterface/SetNotifyInterface
            'Retrieves the current object set for callbacks on events
            'Nothing if no callback has been set.
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_849x.asp (GET)
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_0q79.asp (SET)
            'The JobEvents class wraps this functionality, so this property
            'is only available within the BITS assembly
            Get
                Dim Notify As IBackgroundCopyCallback
                Dim Callback As Object
                m_Job.GetNotifyInterface(Callback)
                Notify = CType(Callback, IBackgroundCopyCallback)
                Return Notify
            End Get
            Set(ByVal value As IBackgroundCopyCallback)
                Dim Callback As Object
                Callback = CObj(value)
                m_Job.SetNotifyInterface(Callback)
            End Set
        End Property

        Protected Friend Property NotifyFlags() As NotificationTypes
            'Corresponds to IBackgroundCopyJob::GetNotifyFlags/SetNotifyFlags
            'See NotifyInterface above
            'These Flags control what events are sent to the callback interface
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_0s6r.asp (GET)
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_58s3.asp (SET)
            Get
                Dim flags As NotificationTypes
                Dim value As UInt32
                m_Job.GetNotifyFlags(value)
                Return CType(Convert.ToInt32(value), NotificationTypes)
            End Get
            Set(ByVal value As NotificationTypes)
                Dim uintvalue As UInt32
                uintvalue = Convert.ToUInt32(value)
                m_Job.SetNotifyFlags(uintvalue)
            End Set
        End Property

        Public Property Priority() As JobPriority
            'Corresponds to IBackgroundCopyJob::SetPriority/GetPriority
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_2fp5.asp (GET)
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_9sah.asp (SET)
            Get
                Dim myPriority As BG_JOB_PRIORITY
                m_Job.GetPriority(myPriority)
                Return CType(myPriority, JobPriority)
            End Get
            Set(ByVal value As JobPriority)
                m_Job.SetPriority(CType(value, BG_JOB_PRIORITY))
            End Set
        End Property

        Public Property ProxySettings() As ProxySettings
            'Corresponds to IBackgroundCopyJob::GetProxySettings/SetProxySettings
            'Wrapped using the ProxySettings class
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_7usz.asp (GET)
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_9q2b.asp (SET)
            Get
                Dim jobUsage As BG_JOB_PROXY_USAGE
                Dim jobProxyList As String
                Dim jobProxyBypassList As String
                m_Job.GetProxySettings(jobUsage, jobProxyList, jobProxyBypassList)
                Return New ProxySettings(jobUsage, jobProxyList, jobProxyBypassList)
            End Get
            Set(ByVal value As ProxySettings)
                Dim jobUsage As BG_JOB_PROXY_USAGE
                jobUsage = CType(value.Usage, BG_JOB_PROXY_USAGE)
                m_Job.SetProxySettings(jobUsage, value.ProxyList, value.ProxyBypassList)
            End Set
        End Property

        Public Property MinimumRetryDelay() As Int64
            'Corresponds to IBackgroundCopyJob::SetMinimumRetryDelay/GetMinimumRetryDelay
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_6im1.asp (GET)
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_377d.asp (SET)
            Get
                Dim Delay As UInt32
                m_Job.GetMinimumRetryDelay(Delay)
                Return Convert.ToInt64(Delay)
            End Get
            Set(ByVal value As Int64)
                Dim Delay As UInt32 = Convert.ToUInt32(value)
                m_Job.SetMinimumRetryDelay(Delay)
            End Set
        End Property
        Public Property NoProgressTimeout() As Int64
            'Corresponds to IBackgroundCopyJob::GetNoProgressTimeout/SetNoProgressTimeout
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_09tg.asp (GET)
            'http://msdn.microsoft.com/library/en-us/bits/refdrz1_6yes.asp (SET)
            Get
                Dim Timeout As UInt32
                m_Job.GetNoProgressTimeout(Timeout)
                Return Convert.ToInt64(Timeout)
            End Get
            Set(ByVal value As Int64)
                Dim Timeout As UInt32 = Convert.ToUInt32(value)
                m_Job.SetNoProgressTimeout(Timeout)
            End Set
        End Property

        Protected Friend Sub New(ByVal bitsJob As IBackgroundCopyJob)
            m_Job = bitsJob
        End Sub
    End Class

End Namespace