Imports System.IO

Public NotInheritable Class HaxeType
    Inherits HaxeMember
    Implements IGenericTypeCap

    Public Structure HaxeBaseTypeImport

        Public Enum ImportTypeEnum
            Extend
            Implement
        End Enum

        Public Property ImportType As ImportTypeEnum

        Public Property TypeNmae As String
    End Structure


    Friend Enum ChildTypeCode
        None
        Generic
        [Return]
    End Enum

    Private _name As String

    Private _ended As Boolean

    Private ReadOnly _types As List(Of HaxeBaseTypeImport)

    Private _started As Boolean

    Private ReadOnly _loockedKeywords As Stack(Of String)

    Private ReadOnly _generictype As List(Of HaxeType)


    Public Overrides ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property BaseTypes As List(Of HaxeBaseTypeImport)
        Get
            Return _types
        End Get
    End Property

    Friend Sub New(text As String)
        Me.New(text, Nothing)
    End Sub

    Private Sub New(text As String, parent As HaxeMember)
        MyBase.New(text, parent)
        _loockedKeywords = New Stack(Of String)()
        _generictype = New List(Of HaxeType)()
        _types = New List(Of HaxeBaseTypeImport)()
    End Sub

    Friend Sub New(text As String, parent As HaxeMember, child As ChildTypeCode)
        Me.New(text, parent)
        Select Case child
            Case ChildTypeCode.Generic
                IsGenericType = True
        End Select
        _name = Join({parent.Name, FirstLineCommand}, ".")
    End Sub
    Private Function IsMatchDouble(x As String) As Boolean
        Return (IsTypedef And (x = "final" Or x = "abstract")) Or (IsInterface And (x = "abstract")) Or (IsClass Or IsInterface Or IsTypedef)
    End Function

    Public Overrides Sub AnalyzeToData(reader As TextReader)
        Dim text = Trim(FirstLineCommand)
        Dim importactive As Boolean = False
        Dim importtype As HaxeBaseTypeImport.ImportTypeEnum
        Dim l As Object = Nothing
        If reader IsNot Nothing Then
            Do
                If Not CheckIfMacro(text) Then
                    For Each x In Split(text)
                        If Not String.IsNullOrEmpty(x) Then
                            x = x.Trim()
                            If x.StartsWith("//") Then
                                Exit For
                            End If
                            If Not IsStarted Then
                                If (IsTypedef And x.Contains("="c)) Or (Split(x, "{").Length > 1) Then
                                    _started = True
                                    x = Replace(Replace(x, "{", ""), "=", "")
                                    If String.IsNullOrEmpty(x) Then
                                        Continue For
                                    End If
                                End If

                                If importactive Then
                                    _types.Add(New HaxeBaseTypeImport() With {.ImportType = importtype, .TypeNmae = x})
                                    importactive = False
                                    If IsClass And _types.Where(
                                Function(z)
                                    Return z.ImportType = HaxeBaseTypeImport.ImportTypeEnum.Extend
                                End Function).Count() > 1 Or IsTypedef Then
                                        Throw Exceptions.InvalidImportBaseClassExpection
                                    End If
                                Else
                                    Select Case x
                                        Case "abstract", "interface", "class", "typedef", "final"
                                            If (IsMatchDouble(x)) Then
                                                Throw Exceptions.IdefenderExpection
                                            End If
                                            _loockedKeywords.Push(x)
                                        Case "extends", "implements"
                                            importtype = If(x = "implements", HaxeBaseTypeImport.ImportTypeEnum.Implement, HaxeBaseTypeImport.ImportTypeEnum.Extend)
                                            importactive = True
                                        Case "public", "private"
                                            Throw Exceptions.IdefenderExpection
                                        Case Else
                                            If _name = Nothing Then
                                                _name = x
                                                If _name.Contains(">"c) Then
                                                    Dim arr = Split(Replace(_name, ">", ""), "<")
                                                    _name = arr.First()
                                                    CheckName(_name)
                                                    Dim f = Split(arr.Last(), ",")
                                                    If f.Length <> f.Distinct().Count() Then
                                                        Throw Exceptions.SameNameToGenericTypeExpection
                                                    End If
                                                    For Each g In f
                                                        CheckName(g)
                                                        _generictype.Add(New HaxeType(g, Me, ChildTypeCode.Generic))
                                                    Next
                                                Else
                                                    CheckName(_name)
                                                End If
                                            Else
                                                Throw Exceptions.IdefenderExpection
                                            End If

                                    End Select
                                End If
                            Else
                                Select Case x
                                    Case "public", "private", "static", "final", "overload"
                                        Continue For
                                    Case "function"
                                        l = New HaxeMethod(text, Me)
                                    Case "var"
                                        l = New HaxeField(text, Me)
                                End Select
                                If l IsNot Nothing Then
                                    Childs.Add(l)
                                    l?.AnalyzeToData(reader)
                                    l = Nothing
                                End If
                                Exit For
                            End If
                            If x.EndsWith("}") Then
                                _ended = True
                            End If
                        End If
                    Next
                End If
                text = reader.ReadLine()
            Loop Until reader.Peek() = -1 Or IsEnded
        End If
    End Sub

    Public Overrides ReadOnly Property IsStatic As Boolean
        Get
            Return _loockedKeywords.Contains("static")
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

    Public Overrides ReadOnly Property IsAbstract As Boolean
        Get
            Return _loockedKeywords.Contains("abstract")
        End Get
    End Property

    Public Overrides ReadOnly Property IsTypedef As Boolean
        Get
            Return _loockedKeywords.Contains("typedef")
        End Get
    End Property

    Public Overrides ReadOnly Property IsGenericType As Boolean

    Public ReadOnly Property GenericTypes As HaxeType() Implements IGenericTypeCap.GenericTypes
        Get
            Return _generictype.ToArray()
        End Get
    End Property

    Public ReadOnly Property Fields As HaxeField()
        Get
            Return Childs.Where(Function(x) TypeOf x Is HaxeField).ToArray
        End Get
    End Property

    Public ReadOnly Property Methods As HaxeMethod()
        Get
            Return Childs.Where(Function(x) TypeOf x Is HaxeMethod).ToArray
        End Get
    End Property

#Region "Unused Properties"

    Public Overrides ReadOnly Property IsArray As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsPrivate As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsPublic As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsMacro As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsInline As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsOverride As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsField As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsMethod As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property IsProperty As Boolean
        Get
            Return False
        End Get
    End Property
#End Region

    Public Overrides ReadOnly Property IsFinal As Boolean
        Get
            Return _loockedKeywords.Contains("final")
        End Get
    End Property


    Public Overrides ReadOnly Property IsClass As Boolean
        Get
            Return _loockedKeywords.Contains("class")
        End Get
    End Property

    Public Overrides ReadOnly Property IsInterface As Boolean
        Get
            Return _loockedKeywords.Contains("interface")
        End Get
    End Property

    Public Overrides ReadOnly Property IsOverloads As Boolean
        Get
            Return False
        End Get
    End Property
End Class
