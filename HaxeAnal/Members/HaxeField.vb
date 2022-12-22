Imports System.IO

Public NotInheritable Class HaxeField
    Inherits HaxeMember

    Private ReadOnly _loockedKeywords As Stack(Of String)
    Private _fieldtype As HaxeType
    Private _fieldname As String
    Private _fieldvalue As String
    Private _propertyacess As PropertyAceessorEnum
    Private _fieldkind As FieldTypes
    Private _nullable As Boolean

    Public ReadOnly Property IsNullable
        Get
            Return _nullable
        End Get
    End Property

    <Flags>
    Public Enum PropertyAceessorEnum
        None = 0
        [Get] = 2
        [Set] = 4
        Dynamic = 8
        Null = 16
        [Default] = 32
        Never = 64
    End Enum

    Public Enum FieldTypes
        [Property]
        MethodParameter
        MethodLocalVarable
        Field
    End Enum

    Public Sub New(text As String, parent As HaxeMember)
        Me.New(text, parent, FieldTypes.Field)
    End Sub

    Friend Sub New(text As String, parent As HaxeMember, kind As FieldTypes)
        MyBase.New(text, parent)
        _loockedKeywords = New Stack(Of String)()
        _fieldkind = kind
    End Sub

    Public ReadOnly Property FieldKind As FieldTypes
        Get
            Return _fieldkind
        End Get
    End Property

    Public Overrides ReadOnly Property IsPrivate As Boolean
        Get
            Return _loockedKeywords.Contains("private")
        End Get
    End Property

    Public Overrides ReadOnly Property IsPublic As Boolean
        Get
            Return _loockedKeywords.Contains("public")
        End Get
    End Property

    Public Overrides ReadOnly Property IsStarted As Boolean
        Get
            Return _loockedKeywords.Contains("var") Or _fieldkind = FieldTypes.MethodParameter
        End Get
    End Property

    Public Overrides ReadOnly Property IsEnded As Object
        Get
            Return _ended Or _fieldkind = FieldTypes.MethodParameter
        End Get
    End Property
    Private _ended As Boolean

    Public Overrides ReadOnly Property IsAnalyzed As Boolean
        Get
            Return Not IsNothing(FieldType)
        End Get
    End Property

    Public ReadOnly Property PropertyAcessor As PropertyAceessorEnum
        Get
            Return _propertyacess
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return _fieldname
        End Get
    End Property

    Public ReadOnly Property FieldType As HaxeType
        Get
            Return _fieldtype
        End Get
    End Property

    Public Overrides ReadOnly Property IsField As Boolean
        Get
            Return _fieldkind = FieldTypes.Field
        End Get
    End Property

    Public ReadOnly Property FieldValue As String
        Get
            Return _fieldvalue
        End Get
    End Property

    Public Overrides ReadOnly Property IsProperty As Boolean
        Get
            Return _fieldkind = FieldTypes.Property
        End Get
    End Property

    Public Overrides ReadOnly Property IsStatic As Boolean
        Get
            Return _loockedKeywords.Contains("static")
        End Get
    End Property

    Public Overrides ReadOnly Property IsOverloads As Boolean
        Get
            Return _loockedKeywords.Contains("overload")
        End Get
    End Property

#Region "Unused Properties"
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

    Public Overrides ReadOnly Property IsMacro As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsFinal As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsInline As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsGenericType As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsOverride As Boolean
        Get
            Return False
        End Get
    End Property

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

    Public Overrides ReadOnly Property IsMethod As Boolean
        Get
            Return False
        End Get
    End Property
#End Region

    Public Overrides Sub AnalyzeToData(reader As TextReader)
        Dim commandtext = Split(Me.FirstLineCommand, "//").First()
        For Each x In Split(commandtext)
            If Not String.IsNullOrEmpty(x) Then
                x = x.Trim()
                Select Case x
                    Case "var", "public", "static", "private"
                        _loockedKeywords.Push(x)
                    Case Else
                        If x.Contains(":"c) Then
                            Dim text = Split(commandtext.Trim, ":")
                            Dim text1 = text(0)
                            Dim text2 = Split(text(1), "=")(0).Trim()
                            For Each v In _loockedKeywords
                                text1 = Replace(text1, v, "")
                                text1 = Replace(text1, " ", "")
                            Next
                            If _fieldkind <> FieldTypes.Property Then
                                If x.StartsWith("?") And _fieldkind = FieldTypes.MethodParameter Then
                                    _nullable = True
                                    _fieldname = text1.Substring(1)
                                Else
                                    _fieldname = text1
                                End If
                                CheckName(_fieldname)
                            End If
                            _fieldtype = New HaxeType(text2)
                            SetValue(commandtext, reader)
                            Exit For
                        ElseIf x.Contains("="c) Then
                            SetValue(commandtext, reader)
                            Exit For
                        ElseIf x.Contains("("c) Then
                            _fieldkind = FieldTypes.Property
                            _fieldname = Split(x, "(").First().Trim()
                            Dim arr = Split(Split(Split(commandtext, "=").First(), "(").Last(), ")")
                            Dim text3 = Replace(arr.First(), " ", "")
                            arr = Split(text3, ",")
                            If arr.Length <> 2 Then
                                Throw Exceptions.PropertyInvalidAccess
                            Else
                                _propertyacess = PropertyAceessorEnum.None
                                For Each v In arr
                                    Dim parsed = [Enum].Parse(GetType(PropertyAceessorEnum), UCase(v(0)) + v.Substring(1))
                                    _propertyacess = If(_propertyacess = PropertyAceessorEnum.None, parsed, _propertyacess Or parsed)
                                Next
                            End If
                            CheckName(_fieldname)
                        ElseIf _fieldname Is Nothing Then
                            _fieldname = x
                            CheckName(_fieldname)
                        Else
                            Throw Exceptions.IdefenderExpection
                        End If
                End Select
                If (IsPrivate Or IsPublic Or IsStatic) And (FieldKind = FieldTypes.MethodParameter Or FieldKind = FieldTypes.MethodLocalVarable) Then
                    Throw Exceptions.InvalidKeyWordsForMethodVars
                End If
            End If
        Next
    End Sub

    Private Sub SetValue(commandtext As String, reader As TextReader)
        Dim startedread As Boolean
        If reader IsNot Nothing Then
            Do
                Dim arr = Split(Split(commandtext, "//").First.Trim, "=")
                If _fieldkind = FieldTypes.MethodParameter Then
                    _ended = commandtext.EndsWith(",") Or _ended = commandtext.EndsWith(")")
                Else
                    _ended = commandtext.EndsWith(";")
                End If
                If IsEnded Then Exit Do
                If startedread Then
                    _fieldvalue &= arr.First
                End If
                If arr.Length > 1 Then
                    Dim text = arr.Last().Trim()
                    text = If(_fieldkind <> FieldTypes.MethodParameter, text.TrimEnd(";"), text)
                    If String.IsNullOrEmpty(text) Then
                        Throw Exceptions.EmptyParameter
                    End If
                    _fieldvalue &= text
                    startedread = True
                End If
                commandtext = reader.ReadLine()
            Loop Until reader.Peek() = -1 Or IsEnded
        End If
    End Sub
End Class
