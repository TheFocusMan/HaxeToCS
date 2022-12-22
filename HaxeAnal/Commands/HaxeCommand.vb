Imports System.Text.RegularExpressions

Public MustInherit Class HaxeCommand
    Inherits CommandRun

    Protected Sub New(text As String)
        MyBase.New(text)
    End Sub

    Public Shared Function GetKeyWordCommand() As String
        Return Nothing
    End Function
End Class
