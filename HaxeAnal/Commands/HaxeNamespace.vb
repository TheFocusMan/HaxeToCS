Imports System.IO

Public NotInheritable Class HaxeNamespace
    Inherits HaxeCommand

    Private _namespace As String

    Public Sub New(text As String)
        MyBase.New(text)
    End Sub

    Public Overloads Shared Function GetKeyWordCommand() As String
        Return "package"
    End Function

    Public Overrides ReadOnly Property MultiLineCommand As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsStarted As Boolean
        Get
            Return FirstLineCommand.StartsWith("package")
        End Get
    End Property

    Public Overrides ReadOnly Property IsEnded As Object
        Get
            Return FirstLineCommand.Contains(";"c)
        End Get
    End Property

    Public Overrides ReadOnly Property IsAnalyzed As Boolean
        Get
            Return NamespaceName <> Nothing
        End Get
    End Property

    Public ReadOnly Property NamespaceName As String
        Get
            Return _namespace
        End Get
    End Property

    Public Overrides Sub AnalyzeToData(reader As TextReader)
        If IsNothing(NamespaceName) Then
            Dim value As String = LTrim(Replace(Replace(RTrim(FirstLineCommand), "package", ""), ";"c, Nothing))
            If Split(value).Length > 1 Or RegexMatchCodeForUnwanted(value) Or Not IsStarted Then
                Throw Exceptions.IdefenderExpection
            End If
            _namespace = value
        End If
    End Sub
End Class
