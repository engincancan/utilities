Imports Microsoft.Msdn.Samples.BITS

Module TestBITS

    Sub Main()
        Dim myBITS As New Manager()
        Dim Jobs As JobCollection
        Dim currentJob As Job
        Dim currentProgress As JobProgress
        Dim exitLoop As Boolean = False

        Jobs = myBITS.GetListofJobs()
        Do
            For Each currentJob In Jobs
                currentProgress = currentJob.Progress()
                Console.WriteLine("{0} {1}/{2} ({3})", _
                    currentJob.DisplayName, _
                    currentProgress.BytesTransferred, _
                    currentProgress.BytesTotal, _
                    currentJob.GetOwnerName())
                If currentJob.State = JobState.Transferred Then
                    currentJob.Complete()
                End If
            Next
        Loop Until Jobs.Count = 0
        Console.ReadLine()
    End Sub

End Module
