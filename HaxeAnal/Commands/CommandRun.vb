Imports System.IO
Imports System.Text.RegularExpressions

Public MustInherit Class CommandRun
    Public MustOverride ReadOnly Property MultiLineCommand As Boolean

    Public MustOverride ReadOnly Property IsStarted As Boolean

    Public MustOverride ReadOnly Property IsEnded

    Public ReadOnly Property FirstLineCommand As String

    Friend Sub New(text As String)
        FirstLineCommand = text
    End Sub

    Protected Shared Function RegexMatchCodeForUnwanted(k As String) As Boolean
        Dim ahg As New Regex("[\0--/:<-?{-~!""^`\[\]]")
        Return ahg.IsMatch(k)
    End Function

    Protected Shared Function RegexMatchAllForUnwanted(k As String) As Boolean
        Dim ahg As New Regex("[$-/:-?{-~!""^_`\[\]]")
        Return ahg.IsMatch(k)
    End Function

    Protected Shared Function RegexMatchAllForUnwantedNoBracks(k As String) As Boolean
        Dim ahg As New Regex("[\0--/:-?[-^`{-\255]")
        Return ahg.IsMatch(k)
    End Function

    Protected Shared Function RegexMatchForName(k As String) As Boolean
        Dim ahg As New Regex("[\0--/:-?[-^`{-\255]")
        Dim num As New Regex("[0-9]")
        Return ahg.IsMatch(k) Or num.IsMatch(k(0))
    End Function

    Protected Shared Sub CheckName(s As String)
        If RegexMatchForName(s) Then
            Throw Exceptions.IdefenderExpection
        End If
    End Sub


    Public Function TryAnalyzeToData(reader As TextReader) As Boolean
        Try
            AnalyzeToData(reader)
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Public MustOverride Sub AnalyzeToData(reader As TextReader)

    Public MustOverride ReadOnly Property IsAnalyzed As Boolean
End Class
