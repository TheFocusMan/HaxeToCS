Imports System.IO

Public NotInheritable Class HaxeImport
    Inherits HaxeCommand

    Private _namespace As String
    Private _classImported As String

    Public Overloads Shared Function GetKeyWordCommand() As String
        Return "import"
    End Function

    Public Sub New(text As String)
        MyBase.New(text)
    End Sub

    Public Overrides ReadOnly Property IsAnalyzed As Boolean
        Get
            Return NamespaceRoot <> Nothing
        End Get
    End Property

    Public Overrides ReadOnly Property MultiLineCommand As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsStarted As Boolean
        Get
            Return FirstLineCommand.StartsWith("import")
        End Get
    End Property

    Public Overrides ReadOnly Property IsEnded As Object
        Get
            Return FirstLineCommand.Contains(";"c)
        End Get
    End Property

    Public ReadOnly Property NamespaceRoot As String
        Get
            Return _namespace
        End Get
    End Property

    Public ReadOnly Property TypeImportedName As String
        Get
            Return _classImported
        End Get
    End Property

    Private _calledas As String

    Public ReadOnly Property ImportedCallAs As String
        Get
            Return _calledas
        End Get
    End Property

    Public Overrides Sub AnalyzeToData(reader As TextReader)
        If IsNothing(NamespaceRoot) Then
            Dim value As String = LTrim(Replace(Replace(RTrim(FirstLineCommand), "import", ""), ";"c, Nothing))
            Dim arr = Split(value, "as", 2)
            If arr.Length > 1 Then
                value = arr.First.Trim
                _calledas = arr.Last.Trim
                CheckName(_calledas)
            End If
            Dim arrayval = New Stack(Split(value, "."))
            Dim lastval = arrayval.Pop()
            If value.EndsWith("*") And Len(lastval) = 1 Then
                value = Replace(value, "*", "All")
                lastval = "All"
            End If
            If value.Contains(" "c) Or RegexMatchCodeForUnwanted(value) Or Not IsStarted Or String.IsNullOrEmpty(value) Then
                Throw Exceptions.InvalidNamespaceExpection
            End If
            _namespace = Join(arrayval.ToArray.Reverse.ToArray, ".")
            _classImported = lastval
        End If
    End Sub
End Class
