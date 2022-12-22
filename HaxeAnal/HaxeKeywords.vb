Public Class HaxeKeywords
    Public Shared ReadOnly MemberKeyWords() As String = {"package", "interface", "class", "abstract", "implements", "var", "null", "cast",
        "typedef", "untyped", "macro", "new", "dynamic", "enum", "extends", "extern", "operator", "override", "true", "overload", "final",
        "private", "public", "static", "function", "super", "false"}
    Public Shared ReadOnly FunctialKeyWords() As String = {"if", "try", "catch", "continue", "break", "case", "do", "else", "import",
        "in", "return", "default", "switch", "while", "using", "throw", "for"}
#If WINDOWS Then
    Public Shared MemberKeyWordColor As Color = Color.FromRgb(86,156,214)
    Public Shared FunctialKeyWordColor As Color = Color.FromRgb(216,160,223)
    Public Shared TypeColor As Color = Color.FromRgb(78, 201, 176)
    Public Shared CommentColor As Color = Color.FromRgb(87, 166, 74)
#End If
End Class