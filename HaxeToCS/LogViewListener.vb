Public Class LogViewListener
    Inherits TraceListener

    Private _textbox As RichTextBox
    Private _pathgrath As Paragraph

    Public Sub New(textbox As RichTextBox)
        _textbox = textbox
        _pathgrath = textbox.Document.Blocks(0)
    End Sub

    Public Overrides Sub Write(message As String)
        _pathgrath.Inlines.Add(New Run(message))
    End Sub

    Public Overrides Sub WriteLine(message As String)
        Write(message)
        _pathgrath.Inlines.Add(New LineBreak)
    End Sub
End Class
