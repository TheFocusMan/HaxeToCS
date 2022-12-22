Imports System.Text

Module CodeConvertor

    Private _tabs As Integer = -1

    Public Function Convert(code As HaxeAnalyzer) As String
        If Not code.IsAnalyzed Then Throw New ArgumentException("Code not yet initalized")
        Dim builder As New StringBuilder
        Dim command As String
        Dim ithasnamepace As Boolean = False
        For Each childs In code.Childs
            If CheckIfElseMacro(childs, builder) Then
                Continue For
            End If
            If TypeOf childs Is HaxeImport Then
                command = String.Format("using {0};", CType(childs, HaxeImport).NamespaceRoot)
                If Not builder.ToString().Contains(command) Then builder.AppendLine(command)
            ElseIf TypeOf childs Is HaxeNamespace Then
                Dim namespaces = CType(childs, HaxeNamespace)
                If Not String.IsNullOrEmpty(namespaces.NamespaceName) Then
                    command = String.Format("namespace {0}", namespaces.NamespaceName)
                    builder.AppendLine(command)
                    builder.AppendLine("{")
                    ithasnamepace = True
                End If
            ElseIf TypeOf childs Is HaxeUsing Then
                command = String.Format("using static {0};", CType(childs, HaxeUsing).UsingClassName)
                If Not builder.ToString().Contains(command) Then builder.AppendLine(command)
            Else
                WriteBlock(childs, builder)
            End If
        Next
        If ithasnamepace Then builder.AppendLine("}") 'Close Namespace
        Return builder.ToString
    End Function

    Private Sub WriteMethodCode(child As IOpCodeCap, builder As StringBuilder)
        _tabs += 1
        If Not (child.CodeType = OpCodeType.ValueCreate) Then
            builder.AppendLine(ShikhpulString(vbTab, _tabs) & String.Format("{0} {1}", child.MainKeyword, child.Value).TrimStart)
            If child.OpCodes?.Length > 0 Then
                builder.AppendLine(ShikhpulString(vbTab, _tabs) & "{")
                For Each v In child.OpCodes
                    WriteMethodCode(v, builder)
                Next
                builder.AppendLine(ShikhpulString(vbTab, _tabs) & "}")
            End If
        End If
        _tabs -= 1
    End Sub

    Private Function ShikhpulString(str As String, count As Integer) As String
        Dim text As String = ""
        For i As Integer = 1 To count
            text &= str
        Next
        Return text
    End Function

    Private Function RetunNullAsDynamic(type As HaxeType) As String
        Return ConvertCoreTypes(If(type Is Nothing, "dynamic", type.FirstLineCommand))
    End Function

    Private Sub WriteBlock(childs As IHaxeMember, builder As StringBuilder)
        Dim command As String
        builder.AppendLine()
        Dim lst As New List(Of String)
        _tabs += 1
        If TypeOf childs Is HaxeType Then
            Dim Val As HaxeType = childs
            If Val.IsStatic Then lst.Add("static")
            If Val.IsFinal Then lst.Add("sealed")
            If Val.IsClass Then lst.Add("class")
            If Val.IsInterface Then lst.Add("interface")
            If Val.IsTypedef Then lst.Add("struct")
            command = Join(lst.ToArray())
            command = String.Format("{0} {1}", command, Val.Name)
            If Val.BaseTypes.Count > 0 Then
                command = String.Format("{0} {1}", command, ":")
                For Each v In Val.BaseTypes
                    command = String.Format("{0} {1}", command, v.TypeNmae)
                    If Val.BaseTypes.IndexOf(v) <> Val.BaseTypes.Count - 1 Then
                        command = String.Format("{0} {1}", command, ",")
                    End If
                Next
            End If
            builder.AppendLine(command)
            builder.AppendLine("{")
            For Each x In Val.Childs
                If TypeOf x Is HaxeField Then
                    Dim y As HaxeField = x
                    If y.FieldKind = HaxeField.FieldTypes.Field Then
                        lst.Clear()
                        _tabs += 1
                        If y.IsPublic Then lst.Add("public")
                        If y.IsPrivate Then lst.Add("private")
                        If y.IsStatic Then lst.Add("static")
                        If y.IsFinal Then lst.Add("const")
                        If y.IsOverloads Then lst.Add("new")
                        command = Join(lst.ToArray())
                        command = LTrim(String.Format("{0} {1} {2}", command, RetunNullAsDynamic(y.FieldType), y.Name))
                        If Not String.IsNullOrEmpty(y.FieldValue) Then
                            command = String.Format("{0} = {1}", command, y.FieldValue)
                        End If
                        command += ";"
                        builder.AppendLine(ShikhpulString(vbTab, _tabs) & command)
                        _tabs -= 1
                    End If
                ElseIf TypeOf x Is HaxeMethod Then
                    WriteBlock(x, builder)
                End If
                CheckIfElseMacro(x, builder)
            Next
        End If
        If TypeOf childs Is HaxeMethod Then
            Dim val As HaxeMethod = childs
            If val.IsPublic Then lst.Add("public")
            If val.IsPrivate Then lst.Add("private")
            If val.IsStatic Then lst.Add("static")
            If val.IsOverride Then lst.Add("override")
            If val.IsFinal Then lst.Add("sealed")
            If val.IsOverloads Then lst.Add("new")
            command = Join(lst.ToArray())
            Dim arr = Join(Val.MethodParameters.Select(
            Function(x)
                Dim typename = If(x.IsNullable, ConvertCoreTypes(x.FieldType.FirstLineCommand) & "?", ConvertCoreTypes(x.FieldType.FirstLineCommand))
                If String.IsNullOrEmpty(x.FieldValue) Then
                    Return String.Format("{0} {1}", typename, x.Name)
                Else
                    Return String.Format("{0} {1} = {2}", typename, x.Name, x.FieldValue) ' טעינת אופציות לא חובה
                End If
            End Function).ToArray, ",")
            builder.AppendLine(ShikhpulString(vbTab, _tabs) & String.Format("{0} {1}({2})", command,
                                             If(val.Name = "new", val.Parent.Name,
                                             String.Format("{0} {1}", RetunNullAsDynamic(val.ReturnType), val.Name)), arr))
            If Val.Name = "new" Then
                If Val.OpCodes(0)?.Value.Contains("super") Then
                    builder.AppendLine(ShikhpulString(vbTab, _tabs) & vbTab &
                        String.Format(": base{0}", Val.OpCodes(0).Value.Substring(5, Val.OpCodes(0).Value.Length - 6)))
                    Val.RemoveFirstOpCode()
                End If
            End If
            builder.AppendLine(ShikhpulString(vbTab, _tabs) & "{")
            For Each v In val.LocalValues
                command = ""
                _tabs += 1
                If Not String.IsNullOrEmpty(v.FieldValue) Then
                    command = String.Format("{0} {1} = {2}", RetunNullAsDynamic(v.FieldType), v.Name, v.FieldValue)
                Else
                    command = String.Format("{0} {1}", RetunNullAsDynamic(v.FieldType), v.Name)
                End If
                builder.AppendLine(ShikhpulString(vbTab, _tabs) & command)
                _tabs -= 1
            Next
            For Each v In val.OpCodes
                WriteMethodCode(v, builder)
            Next
        End If
        builder.AppendLine(ShikhpulString(vbTab, _tabs) & "}")
        builder.AppendLine()
        _tabs -= 1
    End Sub

    Private Function CheckIfElseMacro(childs As CommandRun, builder As StringBuilder) As Boolean
        If TypeOf childs Is HaxeIfElseMacro Then
            Dim val1 As HaxeIfElseMacro = childs
            builder.AppendLine(String.Format("{0} {1}", If(val1.MainKeyword = "#end", "#endif", val1.MainKeyword), val1.Value))
            Return True
        End If
        Return False
    End Function

    Private Function ConvertCoreTypes(str As String) As String
        Dim arr() As String = {"Int", "Float", "Dynamic", "Bool", "Void", "String"} ' בדיקה על סוגים חסינים
        Dim ret = str
        For Each v In arr
            ret = Replace(ret, v, LCase(v))
        Next
        Return ret
    End Function

End Module
