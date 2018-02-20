Public Class ProgressForm
    Dim percentage As Integer
    Dim currentValue As Integer = 0


    Private Sub ProgressForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CenterToScreen()
    End Sub

    Private Sub ProgressForm_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        TopMost = True

        BackgroundWorker1.WorkerSupportsCancellation = True
        BackgroundWorker1.WorkerReportsProgress = True

        BackgroundWorker1.RunWorkerAsync()


    End Sub


    Private Sub BackgroundWorker1_DoWork(sender As Object, e As ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = False
        Debug.Print("In Open ")
        Try

            Show()
            Refresh()

            Do
                System.Threading.Thread.Sleep(1000)
                'currentValue = ProgressBar1.Value
                currentValue = currentValue + 2

                BackgroundWorker1.ReportProgress(currentValue)


                Refresh()

                Debug.Print("currentValue -> " & currentValue)


                If currentValue = 96 Then
                    currentValue = 0
                End If

            Loop Until Not Visible

        Catch ex As Exception
            Close()
            BackgroundWorker1.CancelAsync()
            If (BackgroundWorker1.CancellationPending = True) Then
                e.Cancel = True
            End If
        End Try


    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged


        ProgressBar1.Value = e.ProgressPercentage
    End Sub

    Delegate Sub CloseFromCallback([text] As String)

    Private Sub CloseForm(ByVal [text] As String)

        If Me.InvokeRequired Then
            Dim d As New CloseFromCallback(AddressOf CloseForm)
            Me.Invoke(d, New Object() {[text]})
        Else
            Me.Close()
        End If
    End Sub

    Delegate Sub ShowFormCallback([text] As String)

    Private Sub ShowForm(ByVal [text] As String)

        If Me.InvokeRequired Then
            Dim d As New ShowFormCallback(AddressOf ShowForm)
            Me.Invoke(d, New Object() {[text]})
        Else
            Me.Show()
        End If
    End Sub

    Delegate Sub RefreshFormCallback([text] As String)

    Private Sub RefreshForm(ByVal [text] As String)

        If Me.InvokeRequired Then
            Dim d As New RefreshFormCallback(AddressOf RefreshForm)
            Me.Invoke(d, New Object() {[text]})
        Else
            Me.Close()
        End If
    End Sub

End Class