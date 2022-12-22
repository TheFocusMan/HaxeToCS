Imports System.IO

Public NotInheritable Class HaxeUsing
    Inherits HaxeCommand

    Public Sub New(text As String)
        MyBase.New(text)
    End Sub

    Private _class As String

    Public Overloads Shared Function GetKeyWordCommand() As String
        Return "using"
    End Function

    Public Overrides ReadOnly Property MultiLineCommand As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsStarted As Boolean
        Get
            Return FirstLineCommand.StartsWith("using")
        End Get
    End Property

    Public Overrides ReadOnly Property IsEnded As Object
        Get
            Return FirstLineCommand.Contains(";"c)
        End Get
    End Property

    Public Overrides ReadOnly Property IsAnalyzed As Boolean
        Get
            Return UsingClassName <> Nothing
        End Get
    End Property

    Public ReadOnly Property UsingClassName As String
        Get
            Return _class
        End Get
    End Property

    Public Overrides Sub AnalyzeToData(reader As TextReader)
        Dim text = reader.ReadLine()
        If IsNothing(UsingClassName) Then
            Dim value As String = LTrim(Replace(Replace(RTrim(FirstLineCommand), "using", ""), ";"c, Nothing))
            If Split(value).Length > 1 Or RegexMatchCodeForUnwanted(value) Or Not IsStarted Then
                Throw Exceptions.IdefenderExpection
            End If
            _class = value
        End If
    End Sub
End Class
