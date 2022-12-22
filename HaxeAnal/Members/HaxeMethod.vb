Imports System.IO

Public NotInheritable Class HaxeMethod
    Inherits HaxeMember
    Implements IOpCodeCap

    Private ReadOnly _loockedKeywords As Stack(Of String)
    Private _started As Boolean

    Private _name As String

    Private _ended As Boolean

    Private _return As HaxeType
    Private ReadOnly _generictype As List(Of HaxeType)
    Private ReadOnly _parameters As List(Of HaxeField)
    Private ReadOnly _opcodes As List(Of IOpCodeCap)
    Private ReadOnly _fields As List(Of HaxeField)

    Public Sub RemoveFirstOpCode()
        _opcodes.RemoveAt(0)
    End Sub

    Public ReadOnly Property OpCodes As IOpCodeCap() Implements IOpCodeCap.OpCodes
        Get
            Return _opcodes.ToArray()
        End Get
    End Property

    Public ReadOnly Property GenericTypes As HaxeType()
        Get
            Return _generictype.ToArray()
        End Get
    End Property

    Public ReadOnly Property MethodParameters As HaxeField()
        Get
            Return _parameters.ToArray()
        End Get
    End Property


    Public ReadOnly Property ReturnType As HaxeType
        Get
            Return _return
        End Get
    End Property

    Public Sub New(text As String, parent As HaxeMember)
        MyBase.New(text, parent)
        _generictype = New List(Of HaxeType)
        _parameters = New List(Of HaxeField)
        _loockedKeywords = New Stack(Of String)
        _opcodes = New List(Of IOpCodeCap)
        _fields = New List(Of HaxeField)
    End Sub

    Public Overrides ReadOnly Property IsPrivate As Boolean
        Get
            Return _loockedKeywords.Contains("private")
        End Get
    End Property

    Public Overrides ReadOnly Property IsOverloads As Boolean
        Get
            Return _loockedKeywords.Contains("overload")
        End Get
    End Property

    Public Overrides ReadOnly Property IsPublic As Boolean
        Get
            Return _loockedKeywords.Contains("public")
        End Get
    End Property

    Public Overrides ReadOnly Property IsMacro As Boolean
        Get
            Return _loockedKeywords.Contains("macro")
        End Get
    End Property

    Public Overrides ReadOnly Property IsFinal As Boolean
        Get
            Return _loockedKeywords.Contains("final")
        End Get
    End Property

    Public Overrides ReadOnly Property IsInline As Boolean
        Get
            Return _loockedKeywords.Contains("inline")
        End Get
    End Property

    Public Overrides ReadOnly Property IsMethod As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides ReadOnly Property IsOverride As Boolean
        Get
            Return _loockedKeywords.Contains("overide")
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    Public Overrides ReadOnly Property IsStarted As Boolean
        Get
            Return _started
        End Get
    End Property

    Public Overrides ReadOnly Property IsEnded As Object
        Get
            Return _ended
        End Get
    End Property

    Public Overrides ReadOnly Property IsAnalyzed As Boolean
        Get
            Throw New NotImplementedException()
        End Get
    End Property

#Region "Unused Properties"
    Public Overrides ReadOnly Property IsClass As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsInterface As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsField As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsGenericType As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsProperty As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsAbstract As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsTypedef As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsArray As Boolean
        Get
            Return False
        End Get
    End Property
#End Region

    Public Overrides Sub AnalyzeToData(reader As TextReader)
        Dim text = FirstLineCommand.Trim
        Do
            If Not IsStarted Then
                Dim arr = Split(text, "(").Last()
                Dim arr1 = Split(Replace(text, ">", ""), "<", 1)
                text = text.Replace(arr, arr.Replace(" ", ""))
            End If
            For Each x In Split(text)
                If Not IsStarted Then
                    If Split(x, "{").Length > 1 Then
                        _started = True
                        x = Replace(x, "{", "")
                        If String.IsNullOrEmpty(x) Then
                            Continue For
                        End If
                    End If
                    Select Case x
                        Case "function", "public", "static", "private", "macro", "final", "overide", "inline"
                            _loockedKeywords.Push(x)
                        Case "return"
                            If text.EndsWith(";"c) Then
                                _started = True
                                _ended = True
                                _opcodes.Add(HaxeOpCodeCommand.CreateCommand(text, reader, Me))
                            End If 'One line command
                        Case Else
                            If _name = Nothing Then
                                _name = x
                                If _name.Split("(").First.Contains(">"c) Then
                                    Dim arr = Split(Replace(_name.Split("(").First, ">", ""), "<")
                                    _name = arr.First()
                                    Dim f = Split(arr.Last(), ",")
                                    If f.Length <> f.Distinct().Count() Then
                                        Throw Exceptions.SameNameToGenericTypeExpection
                                    End If
                                    For Each g In f
                                        CheckName(g)
                                        _generictype.Add(New HaxeType(g, Me, HaxeType.ChildTypeCode.Generic))
                                    Next
                                End If
                                If x.Contains("("c) Then
                                    Dim arr = Split(Split(text, "(").Last(), ")")
                                    _name = Split(x, "(").First().Trim()
                                    Dim text3 = Replace(arr.First(), " ", "")
                                    If Not String.IsNullOrEmpty(text3) Then
                                        arr = Split(text3, ",")
                                        For Each v In arr
                                            Dim parm As New HaxeField(v, Me, HaxeField.FieldTypes.MethodParameter)
                                            _parameters.Add(parm)
                                            parm.AnalyzeToData(Nothing)
                                        Next
                                    End If
                                Else
                                    Throw Exceptions.IdefenderExpection
                                End If
                                If x.Contains(":"c) Then
                                    Dim type = Split(text, ":").Last
                                    _return = New HaxeType(type, Me, HaxeType.ChildTypeCode.Return)
                                End If
                                If String.IsNullOrEmpty(_name) Then
                                    _name = "Lambda"
                                Else
                                    CheckName(_name)
                                End If
                            ElseIf Not String.IsNullOrEmpty(x) Then
                                Throw Exceptions.IdefenderExpection
                            End If
                    End Select
                End If
                If x.EndsWith("}") Then
                    _ended = True
                End If
            Next
            If IsStarted And Not String.IsNullOrEmpty(text) Then
                Dim command = HaxeOpCodeCommand.CreateCommand(text, reader, Me)
                If command IsNot Nothing Then _opcodes.Add(command)
            End If
            text = reader.ReadLine().Trim
        Loop Until reader.Peek() = -1 Or IsEnded
    End Sub

    Public Overrides ReadOnly Property IsStatic As Boolean
        Get
            Return _loockedKeywords.Contains("static")
        End Get
    End Property

    Public ReadOnly Property Value As String Implements IOpCodeCap.Value
        Get
            Return ""
        End Get
    End Property

    Public ReadOnly Property MainKeyword As String Implements IOpCodeCap.MainKeyword
        Get
            Return "function"
        End Get
    End Property

    Public ReadOnly Property CodeType As OpCodeType Implements IOpCodeCap.CodeType
        Get
            Return OpCodeType.Method
        End Get
    End Property

    Private ReadOnly Property IOpCodeCap_Parent As IOpCodeCap Implements IOpCodeCap.Parent
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property LocalValues As List(Of HaxeField) Implements IOpCodeCap.LocalValues
        Get
            Return _fields
        End Get
    End Property
End Class
