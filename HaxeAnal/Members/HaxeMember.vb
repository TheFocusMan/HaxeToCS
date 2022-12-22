Public MustInherit Class HaxeMember
    Inherits CommandRun
    Implements IHaxeMember

    Public MustOverride ReadOnly Property Name As String

    Protected Friend Sub New(text As String, parent As HaxeMember)
        MyBase.New(text)
        Childs = New List(Of IHaxeMember)()
        Me.Parent = parent
    End Sub

    Public Overrides ReadOnly Property MultiLineCommand As Boolean
        Get
            Return True
        End Get
    End Property

    Public ReadOnly Property Childs As List(Of IHaxeMember) Implements IHaxeMember.Childs

    Public ReadOnly Property Parent As HaxeMember

    Protected Sub ThrowIfNotEnded()
        If Not IsEnded Then
            Throw Exceptions.IdefenderExpection
        End If
    End Sub

    Protected Function CheckIfMacro(str As String) As Boolean
        str = str.Trim
        If str.StartsWith("#if") Or str.StartsWith("#else") Or str.StartsWith("#end") Then
            Dim v As New HaxeIfElseMacro(str)
            v.AnalyzeToData(Nothing)
            Childs.Add(v)
            Return True
        End If
        Return False
    End Function

    Public MustOverride ReadOnly Property IsAbstract As Boolean

    Public MustOverride ReadOnly Property IsTypedef As Boolean

    Public MustOverride ReadOnly Property IsArray As Boolean

    Public MustOverride ReadOnly Property IsPrivate As Boolean

    Public MustOverride ReadOnly Property IsPublic As Boolean

    Public MustOverride ReadOnly Property IsMacro As Boolean

    Public MustOverride ReadOnly Property IsFinal As Boolean

    Public MustOverride ReadOnly Property IsInline As Boolean

    Public MustOverride ReadOnly Property IsGenericType As Boolean

    Public MustOverride ReadOnly Property IsOverride As Boolean

    Public MustOverride ReadOnly Property IsOverloads As Boolean

    Public MustOverride ReadOnly Property IsClass As Boolean

    Public MustOverride ReadOnly Property IsInterface As Boolean

    Public MustOverride ReadOnly Property IsField As Boolean

    Public MustOverride ReadOnly Property IsProperty As Boolean

    Public MustOverride ReadOnly Property IsMethod As Boolean

    Public MustOverride ReadOnly Property IsStatic As Boolean
End Class
