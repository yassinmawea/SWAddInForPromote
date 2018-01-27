Imports System.Windows.Forms

Public Class CompleteForm
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()
        Application.Exit()
    End Sub

    Private Sub CompleteForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CenterToScreen()
    End Sub

    Private Sub CompleteForm_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        TopMost = True
    End Sub

    Public Sub Open()
        Debug.Print("In Open Method")
        Try
            Show()
            Refresh()

        Catch ex As Exception
            Debug.Print("Ex catched -> " + ex.ToString)
        End Try
    End Sub
End Class