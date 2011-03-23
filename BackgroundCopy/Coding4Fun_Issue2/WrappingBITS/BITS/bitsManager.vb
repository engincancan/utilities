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

    Public Class Manager
        'Wrapper class that corresponds to IBackgroundCopyManager
        'http://msdn.microsoft.com/library/en-us/bits/refdrz1_3cmq.asp

        'Const S_OK As Integer = 0
        'Const S_FALSE As Integer = 1
        Private DEFAULT_RETRY_PERIOD As Integer = 1209600 '20160 minutes (1209600 seconds)
        Private DEFAULT_RETRY_DELAY As Integer = 600 ' 10 minutes (600 seconds)

        'Set variable to maximum UINT64 value
        Private BG_SIZE_UNKNOWN As UInt64 = UInt64.Parse("18446744073709551615")

        'Localization
        Friend Shared JobStates() As String = {"Queued", _
                                               "Connecting", _
                                               "Transferring", _
                                               "Suspended", _
                                               "Error", _
                                               "Transient Error", _
                                               "Transferred", _
                                               "Acknowledged", _
                                               "Cancelled"}

        Private g_BCM As BackgroundCopyManager.IBackgroundCopyManager

        Shared Sub New()
            'Initialize JobStates with localized values
        End Sub

        Public Sub New()
            Try
                'The impersonation level must be at least RPC_C_IMP_LEVEL_IMPERSONATE
                Utilities.CoInitializeSecurity(IntPtr.Zero, -1, _
                    IntPtr.Zero, IntPtr.Zero, _
                    Utilities.RPC_C_AUTHN_LEVEL_CONNECT, _
                    Utilities.RPC_C_IMP_LEVEL_IMPERSONATE, _
                    IntPtr.Zero, 0, IntPtr.Zero)
            Catch generic As Exception
                'Localization
                Throw New Exception("Error Initializing Security.", generic)
            End Try

            Try
                g_BCM = New BackgroundCopyManager.BackgroundCopyManager()
            Catch comX As COMException
                'COM error usually indicates BITS library error
                'Localization
                Throw New BitsException("Error Initializing Background Copy Manager.", comX)
            End Try
        End Sub


        Public Overloads Function GetListofJobs(ByVal whichJobs As JobType) As JobCollection
            Dim myJobList As New JobCollection()
            Dim Jobs As IEnumBackgroundCopyJobs
            Dim retrievedJob As IBackgroundCopyJob
            Dim currentJob As Job

            'It is no longer normal practice to use hungarian notation,
            'but in this case the data type is the only difference between
            'these two variables.
            Dim uintFetched As UInt32 = Convert.ToUInt32(0)
            Dim intFetched As Integer

            Try
                g_BCM.EnumJobs(Convert.ToUInt32(whichJobs), Jobs)
                Do
                    Jobs.Next(Convert.ToUInt32(1), retrievedJob, uintFetched)
                    intFetched = Convert.ToInt32(uintFetched)
                    If intFetched = 1 Then
                        myJobList.Add(New Job(retrievedJob))
                    End If
                Loop While intFetched = 1
                Return myJobList
            Catch comX As COMException
                'COM error usually indicates BITS library error
                'Localization
                Throw New BitsException(String.Format("Error Enumerating Jobs ({0}).", comX.Message), comX)
            End Try
        End Function

        Public Overloads Function GetListofJobs() As JobCollection
            'Default to returning the current user's jobs
            Return GetListofJobs(JobType.CurrentUser)
        End Function

        Public Overloads Function CreateJob(ByVal JobName As String) As Job
            Return CreateJob(JobName, "")
        End Function
        Public Overloads Function CreateJob(ByVal JobName As String, ByVal Description As String) As Job
            Return CreateJob(JobName, Description, DEFAULT_RETRY_PERIOD, DEFAULT_RETRY_DELAY)
        End Function

        Public Overloads Function CreateJob(ByVal JobName As String, _
            ByVal Description As String, _
            ByVal RetryPeriod As Int64, _
            ByVal RetryDelay As Int64) As Job

            Dim newJob As BackgroundCopyManager.IBackgroundCopyJob
            Dim newJobID As BackgroundCopyManager.GUID

            Try
                g_BCM.CreateJob(JobName, BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, newJobID, newJob)

                Dim myJob As New Job(newJob)
                myJob.Description = Description

                If RetryPeriod <> DEFAULT_RETRY_PERIOD Then
                    myJob.NoProgressTimeout = RetryPeriod
                End If

                If RetryDelay <> DEFAULT_RETRY_DELAY Then
                    myJob.MinimumRetryDelay = RetryDelay
                End If
                Return myJob
            Catch comX As COMException
                'COM error usually indicates BITS library error
                'Localization
                Throw New BitsException(String.Format("Error Creating Job ({0}).", comX.Message), comX)
            End Try
        End Function

        'Retrieve a specific job given its ID
        Public Function GetJob(ByVal JobID As System.Guid) As Job
            Try
                Dim foundJob As BackgroundCopyManager.IBackgroundCopyJob
                g_BCM.GetJob(Utilities.ConvertToBITSGUID(JobID), foundJob)
                Return New Job(foundJob)
            Catch comX As COMException
                'COM error usually indicates BITS library error
                'Localization
                Throw New BitsException(String.Format("Error Getting Job ({0}).", comX.Message), comX)
            End Try
        End Function
    End Class
End Namespace

