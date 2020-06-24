'Public Class FrmEditDetails

'    Private _nfo As CharacterNfo
'    Private _item As ListViewItem
'    Private _open As Boolean
'    Private MIN_SIZE As New Size(651, 605)

'    Public Sub ShowMe(ByVal nfo As CharacterNfo, ByVal listItem As ListViewItem)

'        Me.Text = "Edit " & nfo.CharacterName & "'s details"
'        TxtDesc.Text = nfo.Description
'        TxtTags.Text = nfo.Tags
'        _nfo = nfo
'        _item = listItem
'        _open = True

'        CmdPreview_Click(Nothing, Nothing)
'        Me.Show()

'    End Sub

'    Public ReadOnly Property Open() As Boolean
'        Get
'            Return _open
'        End Get
'    End Property

'    Private Sub CmdPreview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdPreview.Click
'        WebPrev.DocumentText = New phpBBCode(TxtDesc.Text).Decode2Html()
'    End Sub

'    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmd.Click
'        Try
'            MsgBox(Split(WebPrev.DocumentText, "<body>")(1))
'        Catch ex As Exception

'        End Try

'    End Sub

'    Private Sub CmdSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdSave.Click

'        _nfo.Description = TxtDesc.Text
'        _nfo.Tags = TxtTags.Text
'        _item.SubItems(3).Text = TxtTags.Text
'        _item.SubItems(4).Text = TxtDesc.Text

'        ChangeFullListItem(_item, _item.ListView.Tag)
'        Mes("Saved details for " & _nfo.CharacterName & ".", AG3DBCMessageType.Success)

'    End Sub

'    Private Sub CmdSaveClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdSaveClose.Click
'        CmdSave_Click(Nothing, Nothing)
'        CmdCancel_Click(Nothing, Nothing)
'    End Sub

'    Private Sub CmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdCancel.Click
'        _open = False
'        Me.Hide()
'    End Sub

'    Private Sub Quicks_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

'        Dim sInput As String = ""
'        Dim colorBox As New ColorDialog()

'        colorBox.SolidColorOnly = True
'        colorBox.AnyColor = False

'        Select Case sender.name.tolower

'            Case "cmdlist"

'                Dim cur As String = InputBox("Enter list items (Cancel to stop)", "List")

'                While Len(cur)
'                    sInput += "[*]" & cur & endl
'                    cur = InputBox("Enter list items (Cancel to stop)", "List")
'                End While

'                If Len(sInput) Then
'                    InsertvBulletinCode("[list]" & endl & sInput & "[/list]")
'                End If

'            Case "cmdurl"

'                sInput = InputBox("Enter URL:", , "http://")

'                If Len(sInput) Then
'                    InsertvBulletinCode("[url='" & sInput & "'][/url]")
'                End If

'            Case "cmdimage"

'                sInput = InputBox("Enter image URL:", , "http://")

'                If Len(sInput) Then
'                    InsertvBulletinCode("[img]" & sInput & "[/img]")
'                End If

'            Case "cmdcolor"

'                If colorBox.ShowDialog() = Windows.Forms.DialogResult.OK Then

'                    If colorBox.Color.IsNamedColor Then
'                        InsertvBulletinCode("[color='" & colorBox.Color.Name & "'][/color]")
'                    Else
'                        InsertvBulletinCode("[color='#" & colorBox.Color.Name.Substring(2) & "'][/color]")
'                    End If

'                End If

'            Case "cmdfont"

'                Dim f As New FontDialog()

'                f.ShowColor = False
'                f.ShowEffects = False
'                f.ScriptsOnly = True

'                If f.ShowDialog = Windows.Forms.DialogResult.OK Then
'                    InsertvBulletinCode("[font='" & f.Font.Name & "'][/font]")
'                End If

'            Case "cmdsize"

'                sInput = InputBox("Enter size (examples: 1, -1, +1):")

'                If Len(sInput) Then
'                    InsertvBulletinCode("[size='" & sInput & "'][/size]")
'                End If

'            Case "cmdemail"

'                sInput = InputBox("Enter email address:")

'                If Len(sInput) Then
'                    InsertvBulletinCode("[email='" & sInput & "']" & sInput & "[/email]")
'                End If

'            Case "cmdstrike"
'                InsertvBulletinCode("[strike][/strike]")
'            Case "cmdbold"
'                InsertvBulletinCode("[b][/b]")
'            Case "cmditalic"
'                InsertvBulletinCode("[i][/i]")
'            Case "cmdunderline"
'                InsertvBulletinCode("[u][/u]")
'            Case "cmdcenter"
'                InsertvBulletinCode("[center][/center]")
'            Case "cmdleft"
'                InsertvBulletinCode("[left][/left]")
'            Case "cmdright"
'                InsertvBulletinCode("[right][/right]")
'            Case "cmdspoiler"
'                InsertvBulletinCode("[spoiler][/spoiler]")
'            Case "cmdnoparse"
'                InsertvBulletinCode("[noparse][/noparse]")
'            Case "cmdindent"
'                InsertvBulletinCode("[indent][/indent]")

'        End Select

'    End Sub

'    Private Sub InsertvBulletinCode(ByVal code As String)

'        Dim selStart As Integer = TxtDesc.SelectionStart

'        If TxtDesc.SelectionLength > 0 Then
'            TxtDesc.Text = TxtDesc.Text.Substring(0, selStart) & TxtDesc.Text.Substring(selStart + TxtDesc.SelectionLength)
'        End If

'        TxtDesc.Text = TxtDesc.Text.Substring(0, selStart) & code & TxtDesc.Text.Substring(selStart)
'        TxtDesc.SelectionStart = selStart + code.IndexOf("]", 0) + 1

'        TxtDesc.ScrollToCaret()
'        TxtDesc.Focus()

'    End Sub

'    Private Sub FrmEditDetails_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

'        For Each ctrl As Control In GrpQuicks.Controls
'            AddHandler ctrl.Click, AddressOf Quicks_Click
'        Next

'    End Sub

'    Private Sub FrmEditDetails_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize

'        If Me.Width < MIN_SIZE.Width Then
'            Me.Width = MIN_SIZE.Width
'        End If

'        If Me.Height < MIN_SIZE.Height Then
'            Me.Height = MIN_SIZE.Height
'        End If

'        WebPrev.Width = Me.Width - ((WebPrev.Location.X * 2) + SystemInformation.VerticalScrollBarWidth)
'        WebPrev.Height = Me.Height - (WebPrev.Location.Y + SystemInformation.MenuBarButtonSize.Height + (SystemInformation.Border3DSize.Height * 2) + CmdPreview.Height + (8 * 2))
'        WebFrame.Width = WebPrev.Width + 4
'        WebFrame.Height = WebPrev.Height + 4
'        TxtDesc.Width = WebFrame.Width
'        GrpQuicks.Width = WebFrame.Width
'        TxtTags.Width = WebFrame.Width

'        For Each btn As Control In Me.Controls

'            If btn.Name.ToLower.StartsWith("cmd") Then
'                btn.Top = WebPrev.Location.Y + WebPrev.Size.Height + 8
'            End If

'        Next

'    End Sub

'End Class