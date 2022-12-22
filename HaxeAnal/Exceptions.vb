Public Class Exceptions
    Public Shared ReadOnly Property IdefenderExpection As Exception
        Get
            Return New ArgumentException("Indfender Exeption")
        End Get
    End Property

    Public Shared ReadOnly Property InvalidKeyWordsForMethodVars As Exception
        Get
            Return New ArgumentException("Invalid Keywords For Method Vars")
        End Get
    End Property

    Public Shared ReadOnly Property InvalidImportBaseClassExpection As Exception
        Get
            Return New ArgumentException("Not valid Imports")
        End Get
    End Property

    Public Shared ReadOnly Property InvalidNamespaceExpection As Exception
        Get
            Return New ArgumentException("Invalid Namespace")
        End Get
    End Property

    Public Shared ReadOnly Property SameNameToGenericTypeExpection As Exception
        Get
            Return New ArgumentException("Cant be generic type with the same name")
        End Get
    End Property

    Public Shared ReadOnly Property PropertyInvalidAccess As Exception
        Get
            Return New ArgumentException("Cant define an acess to property")
        End Get
    End Property

    Public Shared ReadOnly Property EmptyParameter As Exception
        Get
            Return New ArgumentException("Empty parameter")
        End Get
    End Property
End Class
