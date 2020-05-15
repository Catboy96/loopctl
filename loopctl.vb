Imports Microsoft.Win32
Imports System.Windows.Forms
Imports System.Text
Imports System.Diagnostics

Module loopctl
    Sub Main()
        If IO.File.Exists(Application.StartupPath & "\loopctl-applist.csv") Then
            AppList()
        Else
            GenList()
        End If

        End
    End Sub

    Private Function GetLoopbackExempt() As String
        Dim pro As New Process()
        Dim inf As New ProcessStartInfo("CheckNetIsolation.exe", "LoopbackExempt -s") With {
            .UseShellExecute = False,
            .RedirectStandardOutput = True
        }
        pro.StartInfo = inf
        pro.Start()

        Dim out As String
        Using sr As System.IO.StreamReader = pro.StandardOutput
            out = sr.ReadToEnd()
        End Using
        Return out
    End Function

    Private Sub GenList()
        Dim reg As RegistryKey
        Try
            reg = Registry.CurrentUser.OpenSubKey(
                    "Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppContainer\\Mappings")
        Catch ex As Exception
            ErrHandler($"Cannot open registry: {ex.Message}", "Error when generating app list")
        End Try

        Dim exempt As String = GetLoopbackExempt()

        Dim sw As IO.StreamWriter
        Try
            sw = New IO.StreamWriter(Application.StartupPath & "\loopctl-applist.csv", False, Encoding.UTF8)
            sw.WriteLine("Loopback,App,SID")
            For Each sid As String In reg.GetSubKeyNames
                Dim en As Integer = 0
                If exempt.Contains(sid) Then en = 1

                sw.WriteLine($"{en},{reg.OpenSubKey(sid).GetValue("DisplayName")},{sid}")
            Next
            sw.Close()
            sw.Dispose()
            InfHandler("Modify ""loopctl-applist.csv"" and re-run loopctl to apply changes.", "App list generated")
        Catch ex As Exception
            ErrHandler($"Cannot write file: {ex.Message}", "Error when generating app list")
        End Try
    End Sub

    Private Sub AppList()
        Dim sr As IO.StreamReader
        Try
            sr = New IO.StreamReader(Application.StartupPath & "\loopctl-applist.csv", Encoding.UTF8)
            Dim first_line = sr.ReadLine()
            If Not first_line = "Loopback,App,SID" Then ErrHandler("""loopctl-applist.csv"" is invalid. It might be broken.", "Error when parsing file")

            While Not sr.EndOfStream
                Dim record As String = sr.ReadLine
                Select Case record.Split(",")(0)
                    Case "0"
                        Dim a As String = $"CheckNetIsolation.exe loopbackexempt -a -p={record.Split(",")(2)}"
                        Shell($"CheckNetIsolation.exe loopbackexempt -d -p={record.Split(",")(2)}")
                    Case "1"
                        Shell($"CheckNetIsolation.exe loopbackexempt -a -p={record.Split(",")(2)}")
                End Select
            End While
            sr.Close()
            sr.Dispose()

            Try
                IO.File.Delete(Application.StartupPath & "\loopctl-applist.csv")
            Catch ex As Exception
                ErrHandler($"Cannot delete ""loopctl-applist.csv"": {ex.Message}", "Error when cleaning file")
            End Try

            InfHandler("Loopback exempt updated.", "Success")
        Catch ex As Exception
            ErrHandler($"Cannot read ""loopctl-applist.csv"": {ex.Message}", "Error when reading file")
        End Try
    End Sub

    Private Sub InfHandler(inf As String, title As String)
        MessageBox.Show(inf, title, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub ErrHandler(err As String, title As String)
        MessageBox.Show(err, title, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End
    End Sub

End Module
