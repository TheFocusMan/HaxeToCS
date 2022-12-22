Imports System.IO

Public Class HaxeAnalyzer
#Disable Warning IDE0044 ' Add readonly modifier
    Private _text As TextReader

    Private _childs As List(Of CommandRun)

    Public ReadOnly Property Childs As List(Of CommandRun)
        Get
            Return _childs
        End Get
    End Property

    Public Sub New(text As TextReader)
        _text = text
        _childs = New List(Of CommandRun)()
    End Sub

    Public Sub Analyze()
        Dim d = _text.ReadLine()
        Do
            Dim gf As CommandRun = Nothing
            For Each v In Split(d)
                Select Case v
                    Case "class", "interface", "typedef"
                        gf = New HaxeType(d)
                    Case "import", "using", "package"
                        Dim b = Split(d, ";", 2)
                        d = b.Last()
                        Select Case v
                            Case "import"
                                gf = New HaxeImport(LTrim(b.First() + ";"))
                            Case "using"
                                gf = New HaxeUsing(LTrim(b.First() + ";"))
                            Case "package"
                                gf = New HaxeNamespace(LTrim(b.First() + ";"))
                        End Select
                End Select
                If Not IsNothing(gf) Then
                    gf.AnalyzeToData(_text)
                    _childs.Add(gf)
                    Exit For
                End If
            Next
            CheckIfMacro(d)
            d = _text.ReadLine()
        Loop Until _text.Peek() = -1
        IsAnalyzed = True
    End Sub

    Public Function TryAnalyze() As Boolean
        Try
            Analyze()
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Protected Sub CheckIfMacro(str As String)
        str = str.Trim
        If str.StartsWith("#if") Or str.StartsWith("#else") Or str.StartsWith("#end") Then
            Dim v As New HaxeIfElseMacro(str)
            v.AnalyzeToData(Nothing)
            _childs.Add(v)
        End If
    End Sub
    Public Property IsAnalyzed As Boolean

End Class
#Enable Warning IDE0044 ' Add readonly modifier
