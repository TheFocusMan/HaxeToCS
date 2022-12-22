Public Interface IOpCodeCap
    ReadOnly Property OpCodes As IOpCodeCap()
    ReadOnly Property Value As String
    ReadOnly Property MainKeyword As String
    ReadOnly Property CodeType As OpCodeType
    ReadOnly Property Parent As IOpCodeCap
    ReadOnly Property LocalValues As List(Of HaxeField)
End Interface

Public Enum OpCodeType
    None
    Method
    [Call]
    [Case]
    ValueSet
    ValueCreate
    MacroIfElse
End Enum
