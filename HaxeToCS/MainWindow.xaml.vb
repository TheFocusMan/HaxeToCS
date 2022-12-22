Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Xml
Imports ICSharpCode.AvalonEdit.Highlighting
Imports ICSharpCode.AvalonEdit.Highlighting.Xshd
Imports System.Windows.Threading

#Disable Warning IDE0044 ' Add readonly modifier
Class MainWindow
    Private _haxeFormat As IHighlightingDefinition
    Private _csharpFormat As IHighlightingDefinition
    Private _classrules As HighlightingRule
    Private _haxeAnal As HaxeAnalyzer
    Public Sub New()

        ' This call is required by the designer.

        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        _haxeFormat = GetHilightFromResource(My.Resources.HaxeCodeTheme)
        _csharpFormat = GetHilightFromResource(My.Resources.CSharpDarkMode)

        _classrules = New HighlightingRule With {
            .Color = _haxeFormat.GetNamedColor("ClassType"),
            .Regex = New Regex("\b(class)(class)\b?")
        }
        _haxeFormat.MainRuleSet.Rules.Insert(0, _classrules)
        _csharpFormat.MainRuleSet.Rules.Insert(0, _classrules)
        ritchtextbox1.SyntaxHighlighting = _haxeFormat
        ritchtextbox2.SyntaxHighlighting = _csharpFormat
        ritchtextbox1.Text = My.Resources.MainResource
    End Sub

    Private Shared Function GetHilightFromResource(var As Byte()) As IHighlightingDefinition
        Using c As Stream = New MemoryStream(var)
            Using reader As New XmlTextReader(c)
                Dim k = HighlightingLoader.LoadXshd(reader)
                Return HighlightingLoader.Load(k, HighlightingManager.Instance)
            End Using
        End Using
    End Function

    Private Sub AnalyzeCodeClick(sender As Object, e As RoutedEventArgs)
        If Not String.IsNullOrEmpty(ritchtextbox1.Text) Then
            Using reader As New StringReader(ritchtextbox1.Text)
                Dim stringsf As New List(Of String)
                _haxeAnal = New HaxeAnalyzer(reader)
                _haxeAnal.TryAnalyze()
                For Each anal As Object In _haxeAnal.Childs
                    stringsf.Add(If(TypeOf anal Is HaxeImport, anal.TypeImportedName,
                                 If(TypeOf anal Is HaxeUsing, anal.UsingClassName,
                                 If(TypeOf anal Is HaxeType, anal.Name, Nothing))))
                Next
                stringsf.RemoveAll(Function(x) x Is Nothing)
                stringsf.Sort(Function(x, y) y.CompareTo(x))
                Dim arr = stringsf.Distinct.ToArray()
                _classrules.Regex = New Regex(Format("\b({0})({0})\b?", Join(arr, "|")))
                ritchtextbox1.TextArea.TextView.Redraw()
            End Using
        End If
    End Sub

    Private Sub Ritchtextbox1_TextChanged(sender As Object, e As EventArgs) Handles ritchtextbox1.TextChanged
        _classrules.Regex = New Regex("\b(class)(class)\b?")
        ritchtextbox1.TextArea.TextView.Redraw()
    End Sub

    Private Sub ConvertCodeClick(sender As Object, e As RoutedEventArgs)
        ritchtextbox2.Text = Convert(_haxeAnal)
        ritchtextbox2.TextArea.TextView.Redraw()
    End Sub

    Private Sub NewFileCreate(sender As Object, e As RoutedEventArgs)
        ritchtextbox1.Text = ""
    End Sub

    Private Sub OpenFileClick(sender As Object, e As RoutedEventArgs)
        Dim dialog As New Microsoft.Win32.OpenFileDialog With {
            .Filter = "Haxe Files(*.hx)|*.hx",
            .Multiselect = False
        }
        If dialog.ShowDialog() Then
            ritchtextbox1.Text = File.ReadAllText(dialog.FileName)
        End If
    End Sub
End Class
#Enable Warning IDE0044 ' Add readonly modifier