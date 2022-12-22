Imports System.IO
Imports NUnit.Framework

Namespace HaxeAnalyzeTest

    Public Class Tests

        <SetUp>
        Public Sub Setup()

        End Sub

        <Test>
        Public Sub TestKeyWords()
            Dim sf As New List(Of String)
            Dim Gg As New Dictionary(Of String, Integer)
            sf.AddRange(HaxeKeywords.FunctialKeyWords)
            sf.AddRange(HaxeKeywords.MemberKeyWords)
            For Each a As String In sf
                If Gg.ContainsKey(a) Then
                    Gg(a) += 1
                Else
                    Gg.Add(a, 1)
                End If
                If Gg(a) > 1 Then
                    TestContext.WriteLine("Waring The Value {0} In ""{1}"" More Than One time", NameOf(HaxeKeywords), a)
                End If
            Next
            Assert.Pass()
        End Sub

        <Test>
        Public Sub TestHaxeReaderImport()
            Dim a As New HaxeImport("import e.*;")
            If a.TryAnalyzeToData(Nothing) Then
                TestContext.WriteLine(a.NamespaceRoot + "." + a.TypeImportedName)
                Assert.Pass()
            Else
                Assert.Fail("סעמק ערס!")
            End If
        End Sub

        <Test>
        Public Sub TestHaxeReaderNamespace()
            Dim a As New HaxeNamespace("package;")
            If a.TryAnalyzeToData(Nothing) Then
                TestContext.WriteLine(a.NamespaceName)
                Assert.Pass()
            Else
                Assert.Fail("סעמק ערס!")
            End If
        End Sub

        <Test>
        Public Sub TestHaxeAnalyze()
            Using reader As New StringReader("import sex.sex; using hi; class ff extends f{function ss(?Sd:Null) {}}")
                Try
                    Dim f As New HaxeAnalyzer(reader)
                    f.Analyze()
                    TestContext.WriteLine("או יה")
                Catch ex As Exception
                    Assert.Fail("סעמק ערס!")
                End Try
            End Using
            Assert.Pass()
        End Sub

    End Class

End Namespace