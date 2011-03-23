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
    Public Class InnerEvents
        Implements IBackgroundCopyCallback
        Private m_jobEvents As jobEvents

        Public Property jobEvents() As jobEvents
            Get
                Return m_jobEvents
            End Get
            Set(ByVal value As jobEvents)
                m_jobEvents = value
            End Set
        End Property

        Public Sub JobError(ByVal pJob As BackgroundCopyManager.IBackgroundCopyJob, _
            ByVal pError As BackgroundCopyManager.IBackgroundCopyError) _
            Implements BackgroundCopyManager.IBackgroundCopyCallback.JobError

            m_jobEvents.ErrorEvent(New Job(pJob), pError)
        End Sub

        Public Sub JobModification(ByVal pJob As BackgroundCopyManager.IBackgroundCopyJob, _
            ByVal dwReserved As System.UInt32) _
            Implements BackgroundCopyManager.IBackgroundCopyCallback.JobModification

            m_jobEvents.ModificationEvent(New Job(pJob))
        End Sub

        Public Sub JobTransferred(ByVal pJob As BackgroundCopyManager.IBackgroundCopyJob) _
            Implements BackgroundCopyManager.IBackgroundCopyCallback.JobTransferred
            m_jobEvents.TransferredEvent(New Job(pJob))
        End Sub

    End Class

    Public Class JobEventArgs
        Inherits System.EventArgs
        Private m_Job As Job
        Private m_displayName As String

        Protected Friend Sub New(ByVal eventJob As Job)
            m_Job = eventJob
            m_displayName = eventJob.DisplayName
        End Sub

        Public ReadOnly Property Job() As Job
            Get
                Return m_Job
            End Get
        End Property

        Public ReadOnly Property JobName() As String
            Get
                Return m_displayName
            End Get
        End Property
    End Class

    Public Class JobErrorEventArgs
        Inherits JobEventArgs
        Private mErrorInfo As BackgroundCopyManager.IBackgroundCopyError

        Private errorContext As BackgroundCopyManager.BG_ERROR_CONTEXT
        Private errorCode As Integer

        Public ReadOnly Property context() As BackgroundCopyManager.BG_ERROR_CONTEXT
            Get
                Return errorContext
            End Get
        End Property

        Public ReadOnly Property code() As Integer
            Get
                Return errorCode
            End Get
        End Property

        Private Function GetCurrentLCID() As Integer
            Return System.Threading.Thread.CurrentThread.CurrentUICulture.LCID()
        End Function
        Public Overloads Function GetErrorDescription() As String
            Return GetErrorDescription(GetCurrentLCID())
        End Function
        Public Overloads Function GetErrorContextDescription() As String
            Return GetErrorContextDescription(GetCurrentLCID())
        End Function

        Public Overloads Function GetErrorDescription(ByVal LCID As Integer) As String
            Dim errorDesc As String
            mErrorInfo.GetErrorDescription(Convert.ToUInt32(LCID), errorDesc)
            Return errorDesc
        End Function
        Public Overloads Function GetErrorContextDescription(ByVal LCID As Integer) As String
            Dim errorContextDesc As String
            mErrorInfo.GetErrorContextDescription(Convert.ToUInt32(LCID), errorContextDesc)
            Return errorContextDesc
        End Function

        Protected Friend Sub New(ByVal eventJob As Job, ByVal errorInfo As IBackgroundCopyError)
            MyBase.New(eventJob)
            mErrorInfo = errorInfo
        End Sub
    End Class

    Public Class JobEvents
        Public Event JobError(ByVal sender As Object, ByVal e As JobErrorEventArgs)
        Public Event JobModification(ByVal sender As Object, ByVal e As JobEventArgs)
        Public Event JobTransferred(ByVal sender As Object, ByVal e As JobEventArgs)
        Private myCallback As New innerEvents()

        Protected Friend Sub ErrorEvent(ByVal errorJob As Job, ByVal errorInfo As BackgroundCopyManager.IBackgroundCopyError)
            RaiseEvent JobError(Me, New JobErrorEventArgs(errorJob, errorInfo))
        End Sub

        Protected Friend Sub ModificationEvent(ByVal modifiedJob As Job)
            RaiseEvent JobModification(Me, New JobEventArgs(modifiedJob))
        End Sub

        Protected Friend Sub TransferredEvent(ByVal transferredJob As Job)
            RaiseEvent JobTransferred(Me, New JobEventArgs(transferredJob))
        End Sub

        Public Sub New()
            myCallback.jobEvents = Me
        End Sub

        Public Overloads Sub AddJob(ByVal jobToMonitor As Job, ByVal notifyType As NotificationTypes)
            jobToMonitor.NotifyInterface = myCallback
            jobToMonitor.NotifyFlags = notifyType
        End Sub

        Public Overloads Sub AddJob(ByVal jobToMonitor As Job)
            'Note that, by default, JobModification is not included
            'this is a frequently occuring event and should not be
            'registered for unless it is needed.
            AddJob(jobToMonitor, NotificationTypes.JobError _
                        Or NotificationTypes.JobTransferred)
        End Sub
    End Class
End Namespace