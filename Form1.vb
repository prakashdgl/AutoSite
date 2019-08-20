﻿Imports System.Text.RegularExpressions
Imports FastColoredTextBoxNS

Public Class Form1

    ' https://stackoverflow.com/a/8182507
    Sub walkTree(ByVal directory As IO.DirectoryInfo, ByVal pattern As String, ByVal parentNode As TreeNode, ByVal key As String, ByVal incRoot As Boolean)
        Dim dirnode = parentNode
        If incRoot Then
            dirnode = parentNode.Nodes.Add(directory.Name)
            dirnode.Tag = directory.FullName
        End If
        For Each file In directory.GetFiles(pattern)
            Dim node = dirnode.Nodes.Add(file.Name)
            node.Tag = file.FullName
            node.ImageKey = key
            node.SelectedImageKey = key
        Next
        For Each subDir In directory.GetDirectories
            walkTree(subDir, pattern, dirnode, key, True)
        Next
    End Sub

    Private Sub checkOpen()
        Panel1.Visible = (SiteTree.Nodes.Count < 1)
        CloseSite.Enabled = (SiteTree.Nodes.Count > 0)
    End Sub

    Private Sub iconTheme()
        If My.Settings.iconTheme = "xp" Then
            XPitem.Checked = True
            SiteTree.ImageList = XP
        ElseIf My.Settings.iconTheme = "vs2017" Then
            VS2017item.Checked = True
            SiteTree.ImageList = VS2017
        End If
    End Sub

    Private Sub refreshTree(ByVal root As TreeNode)
        Dim templatePath = root.Text & "\templates"
        Dim inPath = root.Text & "\in"
        Dim includePath = root.Text & "\includes"

        root.Nodes.Clear()

        Dim pages = root.Nodes.Add("Pages")
        pages.ImageKey = "Page"
        pages.SelectedImageKey = "Page"

        Dim templates = root.Nodes.Add("Templates")
        templates.ImageKey = "Template"
        templates.SelectedImageKey = "Template"

        Dim includes = root.Nodes.Add("Includes")
        includes.ImageKey = "Include"
        includes.SelectedImageKey = "Include"

        walkTree(My.Computer.FileSystem.GetDirectoryInfo(inPath), "*", pages, "Page", False)
        walkTree(My.Computer.FileSystem.GetDirectoryInfo(templatePath), "*", templates, "Template", False)
        walkTree(My.Computer.FileSystem.GetDirectoryInfo(includePath), "*", includes, "Include", False)

        root.Expand()
        pages.Expand()
        templates.Expand()
        includes.Expand()
    End Sub

    Private Sub openSite(ByVal path As String, ByVal autoload As Boolean)
        checkOpen()
        If My.Computer.FileSystem.DirectoryExists(path) Then
            Dim templatePath = path & "\templates"
            Dim inPath = path & "\in"
            Dim includePath = path & "\includes"

            If Not (My.Computer.FileSystem.DirectoryExists(templatePath) And My.Computer.FileSystem.DirectoryExists(inPath) And My.Computer.FileSystem.DirectoryExists(includePath)) Then
                If autoload = False Then
                    If MsgBox("AutoSite will create a site in the folder located at " & path & ". Is this OK?", MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
                        Exit Sub
                    End If
                Else
                    Exit Sub
                End If
            End If
            If Not My.Computer.FileSystem.DirectoryExists(templatePath) Then
                My.Computer.FileSystem.CreateDirectory(templatePath)
            End If
            If Not My.Computer.FileSystem.DirectoryExists(inPath) Then
                My.Computer.FileSystem.CreateDirectory(inPath)
            End If
            If Not My.Computer.FileSystem.DirectoryExists(includePath) Then
                My.Computer.FileSystem.CreateDirectory(includePath)
            End If

            SiteTree.Nodes.Clear()

            Dim root = SiteTree.Nodes.Add(path)
            root.ImageKey = "Folder"
            root.SelectedImageKey = "Folder"

            refreshTree(root)
        End If
        checkOpen()
        My.Settings.openProject = path
    End Sub

    Sub panelUpdate()
        EdSplit.Panel2Collapsed = Not (PreviewPanel.Checked)
        CoreSplit.Panel1Collapsed = Not (ExplorerPanel.Checked)
        EdSplit.Panel1Collapsed = Not (EditorPanel.Checked)
        CoreSplit.Panel2Collapsed = ((EditorPanel.Checked = False) And (PreviewPanel.Checked = False))

        My.Settings.explorerOpen = ExplorerPanel.Checked
        My.Settings.editorOpen = EditorPanel.Checked
        My.Settings.browserOpen = PreviewPanel.Checked
    End Sub

    Private Sub OpenFolder_Click(ByVal sender As Object, ByVal e As EventArgs) Handles OpenFolder.Click, LinkLabel1.Click
        If FolderBrowser.ShowDialog() = DialogResult.OK Then
            If My.Computer.FileSystem.DirectoryExists(FolderBrowser.SelectedPath) Then
                openSite(FolderBrowser.SelectedPath, False)
            End If
        End If
    End Sub

    Private Sub AboutItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles AboutItem.Click
        About.ShowDialog()
    End Sub

    Private Sub CloseSite_Click(ByVal sender As Object, ByVal e As EventArgs) Handles CloseSite.Click
        SiteTree.Nodes.Clear()
        checkOpen()
    End Sub

    Private Sub VirtualSpace_Click(ByVal sender As Object, ByVal e As EventArgs) Handles VirtualSpace.Click
        If VirtualSpace.Checked Then
            VirtualSpace.Checked = False
        Else
            VirtualSpace.Checked = True
        End If
        FastColoredTextBox1.VirtualSpace = VirtualSpace.Checked
    End Sub

    Private Sub WideCaret_Click(ByVal sender As Object, ByVal e As EventArgs) Handles WideCaret.Click
        If WideCaret.Checked Then
            WideCaret.Checked = False
        Else
            WideCaret.Checked = True
        End If
        FastColoredTextBox1.WideCaret = WideCaret.Checked
    End Sub

    Private Sub WordWrap_Click(ByVal sender As Object, ByVal e As EventArgs) Handles WordWrap.Click
        If WordWrap.Checked Then
            WordWrap.Checked = False
        Else
            WordWrap.Checked = True
        End If
        FastColoredTextBox1.WordWrap = WordWrap.Checked
    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        If My.Settings.explorerOpen Then
            ExplorerPanel.Checked = True
            CoreSplit.Panel1Collapsed = False
        Else
            ExplorerPanel.Checked = False
            CoreSplit.Panel1Collapsed = True
        End If

        If My.Settings.editorOpen Then
            EditorPanel.Checked = True
            EdSplit.Panel1Collapsed = False
        Else
            EditorPanel.Checked = False
            EdSplit.Panel1Collapsed = True
        End If

        If My.Settings.browserOpen Then
            PreviewPanel.Checked = True
            EdSplit.Panel2Collapsed = False
        Else
            PreviewPanel.Checked = False
            EdSplit.Panel2Collapsed = True
        End If

        If (EdSplit.Panel1Collapsed = True) And (EdSplit.Panel2Collapsed = True) Then
            CoreSplit.Panel2Collapsed = True
        Else
            CoreSplit.Panel2Collapsed = False
        End If

        If My.Settings.iconTheme = "auto" Then
            If My.Computer.Info.OSFullName.Contains("XP") Then
                My.Settings.iconTheme = "xp"
            Else
                My.Settings.iconTheme = "vs2017"
            End If
        End If

        iconTheme()

        If My.Settings.engine = "python" Then
            EnginePython.Checked = True
            EngineApricot.Checked = False
        Else
            EnginePython.Checked = False
            EngineApricot.Checked = True
        End If

        If My.Computer.FileSystem.DirectoryExists(My.Settings.openProject) Then
            openSite(My.Settings.openProject, True)
        End If
    End Sub

    Private Sub TreeView1_DoubleClick(ByVal sender As Object, ByVal e As TreeNodeMouseClickEventArgs) Handles SiteTree.NodeMouseDoubleClick
        If e.Button = Windows.Forms.MouseButtons.Left Then
            If My.Computer.FileSystem.FileExists(e.Node.Tag) Then
                FastColoredTextBox1.Text = My.Computer.FileSystem.ReadAllText(e.Node.Tag)
            End If
            Try
                WebBrowser1.Navigate(e.Node.Tag)
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Sub ExplorerPanel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ExplorerPanel.Click
        If ExplorerPanel.Checked Then
            ExplorerPanel.Checked = False
        Else
            ExplorerPanel.Checked = True
        End If
        panelUpdate()
    End Sub

    Private Sub EditorPanel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles EditorPanel.Click
        If EditorPanel.Checked Then
            EditorPanel.Checked = False
        Else
            EditorPanel.Checked = True
        End If
        panelUpdate()
    End Sub

    Private Sub PreviewPanel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles PreviewPanel.Click
        If PreviewPanel.Checked Then
            PreviewPanel.Checked = False
        Else
            PreviewPanel.Checked = True
        End If
        panelUpdate()
    End Sub

    Private Sub SiteTree_Layout(ByVal sender As Object, ByVal e As LayoutEventArgs) Handles SiteTree.Layout
        checkOpen()
    End Sub

    Private Sub VS2017item_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles VS2017item.Click
        If Not VS2017item.Checked Then
            VS2017item.Checked = True
            XPitem.Checked = False
            My.Settings.iconTheme = "vs2017"
        End If
        iconTheme()
    End Sub

    Private Sub XPitem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles XPitem.Click
        If Not XPitem.Checked Then
            VS2017item.Checked = False
            XPitem.Checked = True
            My.Settings.iconTheme = "xp"
        End If
        iconTheme()
    End Sub

    Private Sub SiteTree_BeforeLabelEdit(ByVal sender As System.Object, ByVal e As System.Windows.Forms.NodeLabelEditEventArgs) Handles SiteTree.BeforeLabelEdit
        If e.Node.Tag = "" Then
            e.CancelEdit = True
        End If
    End Sub

    Private Sub SiteTree_AfterLabelEdit(ByVal sender As System.Object, ByVal e As System.Windows.Forms.NodeLabelEditEventArgs) Handles SiteTree.AfterLabelEdit
        Try
            Dim newpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(e.Node.Tag), e.Label)
            Try
                My.Computer.FileSystem.MoveFile(e.Node.Tag, newpath)
                e.Node.Tag = newpath
            Catch exx As Exception
                Try
                    My.Computer.FileSystem.MoveDirectory(e.Node.Tag, newpath)
                    e.Node.Tag = newpath
                Catch exxx As Exception
                    e.CancelEdit = True
                End Try
            End Try
        Catch ex As Exception
        End Try
    End Sub

    Private Sub SiteTree_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles SiteTree.AfterSelect

    End Sub

    Public Overridable Sub HTMLSyntaxHighlight(ByVal range As Range)
        Dim BlueBoldStyle As Style = New TextStyle(Brushes.Blue, Nothing, FontStyle.Bold)
        Dim BlueStyle As Style = New TextStyle(Brushes.Blue, Nothing, FontStyle.Regular)
        Dim BoldStyle As Style = New TextStyle(Nothing, Nothing, FontStyle.Bold Or FontStyle.Underline)
        Dim BrownStyle As Style = New TextStyle(Brushes.Brown, Nothing, FontStyle.Italic)
        Dim GrayStyle As Style = New TextStyle(Brushes.Gray, Nothing, FontStyle.Regular)
        Dim GreenStyle As Style = New TextStyle(Brushes.Green, Nothing, FontStyle.Italic)
        Dim MagentaStyle As Style = New TextStyle(Brushes.Magenta, Nothing, FontStyle.Regular)
        Dim MaroonStyle As Style = New TextStyle(Brushes.Maroon, Nothing, FontStyle.Regular)
        Dim RedStyle As Style = New TextStyle(Brushes.Red, Nothing, FontStyle.Regular)
        Dim BlackStyle As Style = New TextStyle(Brushes.Black, Nothing, FontStyle.Regular)

        Dim AStagStyle As New TextStyle(Brushes.Green, Nothing, FontStyle.Bold)
        Dim ASconStyle As New TextStyle(Brushes.Turquoise, Nothing, FontStyle.Bold)

        Dim HTMLCommentRegex1 = New Regex("(<!--.*?-->)|(<!--.*)", RegexOptions.Singleline Or RegexOptions.Compiled)
        Dim HTMLCommentRegex2 = New Regex("(<!--.*?-->)|(.*-->)", RegexOptions.Singleline Or RegexOptions.RightToLeft Or RegexOptions.Compiled)
        Dim HTMLTagRegex = New Regex("<|/>|</|>", RegexOptions.Compiled)
        Dim HTMLTagNameRegex = New Regex("<(?<range>[!\w:]+)", RegexOptions.Compiled)
        Dim HTMLEndTagRegex = New Regex("</(?<range>[\w:]+)>", RegexOptions.Compiled)
        Dim HTMLTagContentRegex = New Regex("<[^>]+>", RegexOptions.Compiled)
        Dim HTMLAttrRegex = New Regex("(?<range>[\w\d\-]{1,20}?)='[^']*'|(?<range>[\w\d\-]{1,20})=""[^""]*""|(?<range>[\w\d\-]{1,20})=[\w\d\-]{1,20}", RegexOptions.Compiled)
        Dim HTMLAttrValRegex = New Regex("[\w\d\-]{1,20}?=(?<range>'[^']*')|[\w\d\-]{1,20}=(?<range>""[^""]*"")|[\w\d\-]{1,20}=(?<range>[\w\d\-]{1,20})", RegexOptions.Compiled)
        Dim HTMLEntityRegex = New Regex("\&(amp|gt|lt|nbsp|quot|apos|copy|reg|#[0-9]{1,8}|#x[0-9a-f]{1,8});", RegexOptions.Compiled Or RegexOptions.IgnoreCase)
        Dim CommentStyle = GreenStyle
        Dim TagBracketStyle = BlueStyle
        Dim TagNameStyle = MaroonStyle
        Dim AttributeStyle = RedStyle
        Dim AttributeValueStyle = BlueStyle
        Dim HtmlEntityStyle = RedStyle
        range.tb.CommentPrefix = Nothing
        range.tb.LeftBracket = "<"c
        range.tb.RightBracket = ">"c
        range.tb.LeftBracket2 = "("c
        range.tb.RightBracket2 = ")"c
        range.tb.AutoIndentCharsPatterns = ""
        range.ClearStyle(CommentStyle, TagBracketStyle, TagNameStyle, AttributeStyle, AttributeValueStyle, HtmlEntityStyle, ASconStyle, AStagStyle)
        range.SetStyle(CommentStyle, HTMLCommentRegex1)
        range.SetStyle(CommentStyle, HTMLCommentRegex2)
        range.SetStyle(TagBracketStyle, HTMLTagRegex)
        range.SetStyle(TagNameStyle, HTMLTagNameRegex)
        range.SetStyle(TagNameStyle, HTMLEndTagRegex)
        range.SetStyle(AttributeStyle, HTMLAttrRegex)
        range.SetStyle(AttributeValueStyle, HTMLAttrValRegex)
        range.SetStyle(HtmlEntityStyle, HTMLEntityRegex)
        range.SetStyle(CommentStyle, "\[(.*?)=(.*?)\](.*?)\[\/\1(.{1,2})\]", RegexOptions.Singleline)
        range.SetStyle(CommentStyle, "\[#.*?#\]", RegexOptions.Singleline)
        range.ClearFoldingMarkers()
        range.SetFoldingMarkers("<head", "</head>", RegexOptions.IgnoreCase)
        range.SetFoldingMarkers("<body", "</body>", RegexOptions.IgnoreCase)
        range.SetFoldingMarkers("<table", "</table>", RegexOptions.IgnoreCase)
        range.SetFoldingMarkers("<form", "</form>", RegexOptions.IgnoreCase)
        range.SetFoldingMarkers("<div", "</div>", RegexOptions.IgnoreCase)
        range.SetFoldingMarkers("<script", "</script>", RegexOptions.IgnoreCase)
        range.SetFoldingMarkers("<tr", "</tr>", RegexOptions.IgnoreCase)
    End Sub

    Private Sub FastColoredTextBox1_TextChanged(ByVal sender As System.Object, ByVal e As FastColoredTextBoxNS.TextChangedEventArgs) Handles FastColoredTextBox1.TextChanged
        'HTMLSyntaxHighlight(e.ChangedRange)
        'Dim GreenStyle As New TextStyle(Brushes.Green, Nothing, FontStyle.Bold)
        'Dim TurqStyle As New TextStyle(Brushes.Turquoise, Nothing, FontStyle.Bold)
        ''clear style of changed range
        'FastColoredTextBox1.Range.ClearStyle(TurqStyle)
        'FastColoredTextBox1.Range.ClearStyle(GreenStyle)
        ''comment highlighting
        'FastColoredTextBox1.Range.SetStyle(TurqStyle, "\[(.*?)=(.*?)\](.*?)\[\/\1(.{1,2})\]", RegexOptions.Singleline)
        'FastColoredTextBox1.Range.SetStyle(GreenStyle, "\[#.*?#\]", RegexOptions.Singleline)
        ''for atteql, value, text in re.findall(r'\[(.*)=(.*?)\](.*)\[\/\1.*\]', template):
    End Sub

    Private Sub EdSplit_SplitterMoved(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LayoutEventArgs) Handles EdSplit.Layout
        CoreSplit.Panel2Collapsed = (PreviewPanel.Checked = False And EditorPanel.Checked = False)
    End Sub

    Private Sub ExitItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitItem.Click
        Me.Close()
    End Sub

    Private Sub BuildSite_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BuildSite.Click
        If My.Computer.FileSystem.FileExists("AutoSitePy.exe") Then
            If Not My.Computer.FileSystem.DirectoryExists(SiteTree.Nodes(0).Text & "\out") Then
                My.Computer.FileSystem.CreateDirectory(SiteTree.Nodes(0).Text & "\out")
            End If
            Dim startinfo As New ProcessStartInfo
            startinfo.FileName = "AutoSitePy.exe"
            startinfo.Arguments = "--auto --dir """ & SiteTree.Nodes(0).Text & """"
            Process.Start(startinfo)
            WebBrowser1.Navigate(SiteTree.Nodes(0).Text & "\out")
        Else
            MsgBox("Python 3-based build engine wasn't found. Put AutoSitePy.exe in the same folder as AutoSite XL.exe.", MsgBoxStyle.Exclamation, "Engine not found")
        End If
    End Sub

    Private Sub OpenInDefault_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenInDefault.Click
        Try
            Process.Start(SiteTree.SelectedNode.Tag)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub SiteTree_NodeMouseClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeNodeMouseClickEventArgs) Handles SiteTree.NodeMouseClick
        If e.Button = Windows.Forms.MouseButtons.Right Then
            If e.Node.Tag = "" Then
                OpenInDefault.Enabled = False
            Else
                OpenInDefault.Enabled = True
            End If
            ContextMenu1.Show(SiteTree, e.Location)
        End If
    End Sub

    Private Sub MenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RefreshItem.Click
        If SiteTree.Nodes.Count > 0 Then
            refreshTree(SiteTree.Nodes(0))
        End If
    End Sub
End Class
