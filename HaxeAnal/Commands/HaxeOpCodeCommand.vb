Imports System.IO
Imports System.Text.RegularExpressions

Public Class HaxeOpCodeCommand
    Inherits HaxeCommand
    Implements IOpCodeCap

    Private _ended As Boolean

    Public Shared Function CreateSingleCommand(text As String, parent As IOpCodeCap) As IOpCodeCap
        Dim op As New HaxeOpCodeCommand(text, False, False, parent)
        op.AnalyzeToData(Nothing)
        Return op
    End Function

    Public Shared Function CreateMultiCommand(text As String, textreader As TextReader, parent As IOpCodeCap) As IOpCodeCap
        Dim op As New HaxeOpCodeCommand(text, True, False, parent)
        op.AnalyzeToData(textreader)
        Return op
    End Function

    Public Shared Function CreteAnyCommand(text As String, textreader As TextReader, parent As IOpCodeCap) As IOpCodeCap
        Dim op As New HaxeOpCodeCommand(text, False, True, parent)
        op.AnalyzeToData(textreader)
        If op.IsAnalyzed Then Return op
        Return Nothing
    End Function

    Public Shared Function CreateCommand(text As String, textreader As TextReader, parent As IOpCodeCap) As IOpCodeCap
        text = Split(text, "//").First
        Dim arr = Split(text)
        Dim s As IOpCodeCap = CheckIfMacro(text)
        If s IsNot Nothing Then Return s
        If arr(0).Contains("return") Then
            Return CreateSingleCommand(text, parent)
        End If
        Dim arr1 As String() = {"if", "else", "try", "for", "switch"}
        For Each v In arr1
            If arr(0).StartsWith(v) Then
                Return CreateMultiCommand(text, textreader, parent)
            End If
        Next
        Return CreteAnyCommand(text, textreader, parent)
    End Function


    Private _value As String
    Private _keyword As String
    Private ReadOnly _nokeyword As Boolean

    Private Sub New(text As String, multi As Boolean, nokey As Boolean, parent As IOpCodeCap)
        MyBase.New(text)
        MultiLineCommand = multi
        _opcodes = New List(Of IOpCodeCap)
        _nokeyword = nokey
        _parent = parent
        LocalValues = New List(Of HaxeField)
    End Sub

    Public ReadOnly Property Value As String Implements IOpCodeCap.Value
        Get
            Return _value
        End Get
    End Property

    Public ReadOnly Property MainKeyword As String Implements IOpCodeCap.MainKeyword
        Get
            Return _keyword
        End Get
    End Property

    Public Overrides ReadOnly Property MultiLineCommand As Boolean

    Public Overrides ReadOnly Property IsStarted As Boolean
        Get
            Return _keyword IsNot Nothing
        End Get
    End Property

    Public Overrides ReadOnly Property IsEnded As Object
        Get
            Return _ended
        End Get
    End Property
    Private _anal As Boolean
    Public Overrides ReadOnly Property IsAnalyzed As Boolean
        Get
            Return _anal
        End Get
    End Property

    Private ReadOnly _opcodes As List(Of IOpCodeCap)

    Public ReadOnly Property OpCodes As IOpCodeCap() Implements IOpCodeCap.OpCodes
        Get
            Return _opcodes.ToArray()
        End Get
    End Property

    Public ReadOnly Property CodeType As OpCodeType Implements IOpCodeCap.CodeType
        Get
            Return _optype
        End Get
    End Property

    Public ReadOnly Property Parent As IOpCodeCap Implements IOpCodeCap.Parent
        Get
            Return _parent
        End Get
    End Property

    Public ReadOnly Property LocalValues As List(Of HaxeField) Implements IOpCodeCap.LocalValues

    Private ReadOnly _parent As IOpCodeCap
    Private _optype As OpCodeType = OpCodeType.None

    Public Overrides Sub AnalyzeToData(reader As IO.TextReader)
        If _nokeyword Then
            Dim arr = Split(FirstLineCommand.Trim, "=", 2)
            If FirstLineCommand.Trim.StartsWith("var") Then
                Dim local As New HaxeField(FirstLineCommand.Trim, Nothing, HaxeField.FieldTypes.MethodLocalVarable)
                local.AnalyzeToData(reader)
                Me._parent.LocalValues.Add(local)
                _optype = OpCodeType.ValueCreate
                _anal = True
                Exit Sub
            End If
            If arr.Length > 1 Then
                _optype = OpCodeType.ValueSet
                _value = FirstLineCommand.Trim
                _anal = True
                Exit Sub
            End If
            Dim methodmatch As New Regex("\b[\d\w_]+(?=\s*" & Regex.Escape("(") & ")")
            If methodmatch.IsMatch(FirstLineCommand.Trim) Then
                _optype = OpCodeType.Call
                _value = FirstLineCommand.Trim
                _anal = True
                Exit Sub
            End If
        Else
            Dim arr = Split(FirstLineCommand.Trim, If(FirstLineCommand.Contains("("c), "(", " "), 2)
            If arr.ElementAtOrDefault(1) IsNot Nothing And FirstLineCommand.Contains("("c) Then arr(1) = "(" & arr(1)
            _value = arr.ElementAtOrDefault(1)?.TrimEnd("{"c)
            _keyword = arr(0)
            If MultiLineCommand Then
                Dim text = Split(Join(arr).Trim, "//").First
                Do
                    If text.Contains("{"c) Then
                        CreateUntilEnd(text.Substring(text.IndexOf("{")), reader)
                        Continue Do
                    ElseIf text.EndsWith(";") Then
                        Dim op = CreateCommand(text.Replace(Join(arr), ""), reader, Me)
                        If op IsNot Nothing Then _opcodes.Add(op)
                        _ended = True
                        _anal = True
                    End If
                    text = Split(reader.ReadLine().Trim, "//").First
                Loop Until reader.Peek() = -1 Or IsEnded
            Else
                _ended = _keyword.EndsWith(";"c) OrElse _value.EndsWith(";"c)
            End If
        End If

    End Sub

    Private Sub CreateUntilEnd(text As String, reader As IO.TextReader)
        If MultiLineCommand Then
            Do
                Dim op = CreateCommand(text, reader, Me)
                If op IsNot Nothing Then _opcodes.Add(op)
                If text.Contains("}"c) Then
                    _ended = True
                    _anal = True
                    Exit Do
                End If
                text = Split(reader.ReadLine().Trim, "//").First
            Loop Until reader.Peek() = -1 Or IsEnded
        End If
    End Sub

    Private Shared Function CheckIfMacro(str As String) As IOpCodeCap
        str = str.Trim
        If str.StartsWith("#if") Or str.StartsWith("#else") Or str.StartsWith("#end") Then
            Dim v As New HaxeIfElseMacro(str)
            v.AnalyzeToData(Nothing)
            Return v
        End If
        Return Nothing
    End Function
End Class
