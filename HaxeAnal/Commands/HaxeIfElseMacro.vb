Public Class HaxeIfElseMacro
    Inherits CommandRun
    Implements IHaxeMember
    Implements IOpCodeCap

    Private _value As String
    Private _started As Boolean
    Private _ended As Boolean
    Private _keyword As String

    Public Sub New(str As String)
        MyBase.New(str)
    End Sub

    Public Overrides ReadOnly Property MultiLineCommand As Boolean
        Get
            Return False
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
            Return IsEnded
        End Get
    End Property

    Public ReadOnly Property Value As String Implements IOpCodeCap.Value
        Get
            Return Me._value
        End Get
    End Property

    Public ReadOnly Property Childs As List(Of IHaxeMember) Implements IHaxeMember.Childs
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property OpCodes As IOpCodeCap() Implements IOpCodeCap.OpCodes
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property MainKeyword As String Implements IOpCodeCap.MainKeyword
        Get
            Return _keyword
        End Get
    End Property

    Public ReadOnly Property CodeType As OpCodeType Implements IOpCodeCap.CodeType
        Get
            Return OpCodeType.MacroIfElse
        End Get
    End Property

    Public ReadOnly Property Parent As IOpCodeCap Implements IOpCodeCap.Parent
        Get
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property LocalValues As List(Of HaxeField) Implements IOpCodeCap.LocalValues
        Get
            Return Nothing
        End Get
    End Property

    Public Overrides Sub AnalyzeToData(reader As IO.TextReader)
        For Each x In Split(FirstLineCommand.Trim, " ", 2)
            Select Case x
                Case "#if", "#else", "#elseif", "#end"
                    _started = True
                    _keyword = x
                    If x = "#end" Then _ended = True
                Case Else
                    If Not IsStarted Then
                        Throw Exceptions.IdefenderExpection
                    End If
                    _value = x
            End Select
        Next
    End Sub
End Class
