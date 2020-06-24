Imports System.IO
Imports System.Xml
Imports System.Threading
Imports Microsoft.Win32
Imports System.Globalization

#Region "TODO"
'   top users !OK!
'   fix times !OK!
'   fix list sizes !OK!
'   fix nfos !OK!
'   show character after download !OK!
'   dont switch controls !OK!
'   total file size !OK!
'   update banner !OK!
'   auto refresh lists !OK!
'-------------------------------------------
'   rename character
'   auto refresh lists option
'   default tab
#End Region

Public Class Form1

    Dim curChar As FileInfo
    Dim removePreviewEvent As New EventHandler(AddressOf RemovePreview)
    Dim uploadThread As Thread
    Dim watchTransferThread As Thread
    Dim getCharFeedThread As Thread
    Dim getPreviewsThread As Thread
    Dim downloadThread As Thread
    Dim active As Boolean = False
    Dim WithEvents removePreviewMenu As New ContextMenu
    Dim WithEvents removePreviewMenuItem As MenuItem

    Public Sub LoadColorControls()
        ClrDBCharsNeedPreviews.BackColor = dbCharsColors.NeedPreviewsColor
        ClrDBCharsNotInCharsList.BackColor = dbCharsColors.NotInCharsListColor
        ClrDBCharsDefault.BackColor = dbCharsColors.DefaultColor
    End Sub

    Private Sub DoPreviews(ByVal curChar As String)

        prevs.Clear()
        Directory.CreateDirectory(ag3Chars & curChar)

        For Each sFile As String In Directory.GetFiles(ag3Chars & curChar, "*.jpg")
            prevs.Add(sFile, Basename(sFile))
        Next

        If File.Exists(ag3Chars & curChar & "_v.bmp") Then

            SaveJpeg(New Bitmap(ag3Chars & curChar & "_v.bmp"), ag3Chars & curChar & "\_v - " & curChar & ".jpg", 95)

            If prevs.Pics.Contains("_v - " & curChar & ".jpg") Then
                prevs.Remove("_v - " & curChar & ".jpg")
            End If

            prevs.Add(ag3Chars & curChar & "\_v - " & curChar & ".jpg", "_v - " & curChar & ".jpg")

        End If

        prevs.StartPics()

    End Sub

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        Mes("AG3DBC terminated")
        End
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If threads.IsRunning Then
            e.Cancel = True
        End If
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        isLoading = True

        If CheckAG3Install() Then

            Mes("AG3DBC launched")

            prevs = New Previews(Pic, PicFrame)
            watch = New TransferWatch(PB, LblTrans, LblPercent)
            threads = New AG3DBCThreads()
            oSearch = New Search2(Me)
            Control.CheckForIllegalCrossThreadCalls = False

            LoadTabs()
            LoadXml()
            DeactivateClient()
            LoadSettings()
            LoadBanners()
            LoadControls()
            LoadHelp()
            LoadListColumns(ListDBChars)

        Else
            MsgBox("Artificial Girl 3 was not found on your system. Please install the game and try again.", MsgBoxStyle.Critical)
            End
        End If

        isLoading = False

    End Sub

    Private Sub LoadControls()
        switchControls.Add(Tab)
        switchControls.Add(TabOptions)
    End Sub

    Private Sub CmdUpload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdUpload.Click

        If NotNothing(ListChars.SelectedItem) Then

            If MsgBox("Are you absolutely sure you want to upload " & ListChars.SelectedItem & " to the AG3DB?", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then

                Dim result As MsgBoxResult = MsgBox("Do you want to include the entire character structure?" & endl & "(choose no if no there are no modded files inside the character's folder)", MsgBoxStyle.Question + MsgBoxStyle.YesNoCancel)

                If result <> MsgBoxResult.Cancel Then
                    prevs.Pause()
                    threads.AddParameter(ListChars.SelectedItem)
                    threads.AddParameter(TxtTags.Text)
                    threads.AddParameter(result = MsgBoxResult.Yes)
                    threads.AddParameter(ftp)
                    threads.AddParameter(watch)
                    threads.Start(AG3DBCThreadType.UploadChar)
                End If

            End If

        Else
            Mes("No character selected.", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub CmdRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdRefresh.Click
        LoadLists(ListChars, LblCharacters)
    End Sub

    Private Sub ListChars_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListChars.SelectedIndexChanged

        If NotNothing(ListChars.SelectedItem) Then
            DoPreviews(ListChars.SelectedItem)
            LblCharSize.Text = "Approximate Filesize: " & FileSize(prevs.Filesizes + JS3CMI_SIZE)
            TxtTags.Text = oTags.GetDBTag(ListDBChars, ListChars.SelectedItem)
        End If

    End Sub

    Private Sub LblRegister_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)
        Process.Start(SERVER & "register.php")
    End Sub

    Private Sub CmdAddPreviews_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdAddPreviews.Click

        If NotNothing(ListChars.SelectedItem) Then

            Dim files() As String = OpenFiles("JPGs (*.jpg, *.jpeg)|*.jpg;*.jpeg", , "Choose previews to add to " & ListChars.SelectedItem)

            If NotNothing(files) Then

                Dim count As Integer = 1

                Directory.CreateDirectory(ag3Chars & ListChars.SelectedItem)

                For Each sFile As String In files

                    While File.Exists(ag3Chars & ListChars.SelectedItem & "\" & count & " - " & ListChars.SelectedItem & ".jpg")
                        count += 1
                    End While

                    File.Copy(sFile, ag3Chars & ListChars.SelectedItem & "\" & count & " - " & ListChars.SelectedItem & ".jpg", True)

                Next

                Mes("Added " & Plural("preview", files.Length) & " to " & ListChars.SelectedItem)
                DoPreviews(ListChars.SelectedItem)

            End If

        Else
            Mes("No character selected", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub CmdRemovePreview_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles CmdRemovePreview.MouseClick

        If NotNothing(ListChars.SelectedItem) Then

            Dim oDir As New DirectoryInfo(ag3Chars & ListChars.SelectedItem)

            removePreviewMenu.MenuItems.Clear()

            For Each oFile As FileInfo In oDir.GetFiles("*.jpg")

                removePreviewMenuItem = New MenuItem(oFile.Name)
                removePreviewMenu.MenuItems.Add(oFile.Name, removePreviewEvent)

            Next

            removePreviewMenu.Show(sender, e.Location)

        Else
            Mes("No character selected", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub RemovePreview(ByVal sender As System.Object, ByVal e As System.EventArgs)

        If NotNothing(ListChars.SelectedItem) Then

            prevs.Pause()
            LoadPic(Pic, PicFrame, GetPreviewPath(ListChars.SelectedItem) & sender.text)

            If MsgBox("Are you sure you want to remove " & sender.Text & "?" & endl & "(the file will also be deleted from your hard disk)", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                prevs.Remove(sender.Text)
                RemoveFile(ag3Chars & ListChars.SelectedItem & "\" & sender.text)
                Mes("Removed preview " & sender.text)
                DoPreviews(ListChars.SelectedItem)
            Else
                DoPreviews(ListChars.SelectedItem)
            End If

        End If

    End Sub

    Private Sub CmdLogin_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdLogin.Click

        CmdLogin.Enabled = False

        Mes("Attempting to login...", AG3DBCMessageType.Normal, False)

        If Login(TxtUsername.Text, TxtPassword.Text, ChkRemember.Checked) Then

            ActivateClient()
            LoadLists(ListChars, LblCharacters)
            Mes("Logged in successfully as " & GetUsername() & "! :D", AG3DBCMessageType.Success)
            Tab_Click(Tab, Nothing)

            LblLoggedInAs.Text = "Logged in as " & GetUsername()

        Else

            Mes("Unable to log into the database." & endl & "Bad username and/or password.", AG3DBCMessageType.Err, True)

            CmdLogin.Enabled = True

        End If

    End Sub

    Private Sub CmdLogout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdLogout.Click

        If Not threads.IsRunning AndAlso MsgBox("Are you sure you want to logout?", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Logout()
            LblLoggedInAs.Text = "Not logged in"
        End If

    End Sub

    Private Sub LoadSettings()

        ChkRemember.Checked = (GetSetting("login", "remember") = "1")
        ChkRndPreviews.Checked = (GetSetting("randomPreviews") = "1")
        ChkBackend.Checked = (GetSetting("backendProcesses") = ProcessWindowStyle.Normal)
        TxtPreviewTime.Text = GetSetting("previewTime")
        CboPriority.SelectedIndex = CInt(GetSetting("programPriority"))
        prevs.Random = ChkRndPreviews.Checked
        backend = GetSetting("backendProcesses")
        prevs.PicTime = Val(TxtPreviewTime.Text)
        dbCharsColors = LoadDBCharsColors()

        Select Case GetSetting("afterCharDownload")
            Case "0" : OptDBCharDownloadNever.Checked = True
            Case "1" : OptDBCharDownloadAlways.Checked = True
            Case "2" : OptDBCharDownloadAsk.Checked = True
        End Select

        LoadColorControls()
        CboPriority_SelectedIndexChanged(Nothing, Nothing)

        If ChkRemember.Checked Then

            TxtUsername.Text = GetSetting("login/username")
            TxtPassword.Text = PwdUnHash(GetSetting("login/password"))

            CmdLogin_Click(Nothing, Nothing)

        End If

        LoadListColumns(ListDBChars)
        LoadListColumns(ListTopChars)
        LoadListColumns(ListUserChars)
        LoadListColumns(ListTopUsers)
        LoadListColumns(ListUserRatings)

    End Sub

    Private Function SelectedDBChar() As ListViewItem
        Return SelectedListItem(ListDBChars)
    End Function

#Region "banners"

    Dim banners As New Collection
    Dim bannerAuthors As New Collection
    Dim bannerIndex As Integer = 0

    Public Sub LoadBanners()

        banners.Add(My.Resources.banner2_1)
        bannerAuthors.Add("mjanes42")

        banners.Add(My.Resources.banner1)
        bannerAuthors.Add("TheShadow")

        banners.Add(My.Resources.banner3)
        bannerAuthors.Add("Hentaijin")

        TimerBanner_Tick(Nothing, Nothing)
        TimerBanner.Start()

    End Sub

    Private Sub TimerBanner_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TimerBanner.Tick

        If bannerIndex + 1 > banners.Count Then
            bannerIndex = 1
        Else
            bannerIndex += 1
        End If

        LblBannerAuthor.Text = "Banner by " & bannerAuthors(bannerIndex)
        Banner.Image = banners(bannerIndex)

    End Sub

#End Region

#Region "form stuff"

    Public Sub Tab_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Tab.Click, TabStatistics.Click

        If NotNothing(sender.SelectedTab) AndAlso Not clickedTabs.Contains(sender.SelectedTab.Name) Then

            clickedTabs.Add(sender.SelectedTab.Name, sender.SelectedTab.Name)

            Select Case sender.SelectedTab.Name
                Case "TabDBChars" : CmdRefreshDBList_Click(Nothing, Nothing)
                Case "TabStats" : CmdUserChars_Click(Nothing, Nothing)
                Case "TabUserRatings" : CmdUserRatings_Click(Nothing, Nothing)
                Case "TabTopChars" : CmdTopChars_Click(Nothing, Nothing)
                Case "TabTopUsers" : CmdTopUsers_Click(Nothing, Nothing)
            End Select

        End If

    End Sub

#End Region

#Region "options"

    Private Sub ChkRemember_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles ChkRemember.KeyPress

        If e.KeyChar = vbCr Then
            CmdLogin_Click(Nothing, Nothing)
        End If

    End Sub

    Private Sub ChkRemember_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ChkRemember.MouseClick
        SetSetting("login", Math.Abs(CInt(ChkRemember.Checked)), "remember")
        SaveXml()
    End Sub

    Private Sub ChangeGameExe(ByVal button As Button)

        Dim gameExe As String
        Dim type As String

        If button.Text = "Play" Then
            gameExe = OpenFile("EXEs (.exe)|*.exe", ag3Path, "Choose the Artificial Girl 3 Play EXE")
            type = "gameExe"
        Else
            gameExe = OpenFile("EXEs (.exe)|*.exe", ag3Path, "Choose the Artificial Girl 3 Make EXE")
            type = "makeExe"
        End If

        If Len(gameExe) Then

            If IsNothing(xmlSettings.SelectSingleNode("/settings/" & type)) Then
                xmlSettings.SelectSingleNode("/settings").AppendChild(NewXmlNode(xmlSettings, type, gameExe))
            Else
                xmlSettings.SelectSingleNode("/settings/" & type).InnerText = gameExe
            End If

            SaveXml()
            Process.Start(gameExe)
            Mes("Changed AG3 " & button.Text & " exe to " & Basename(gameExe))

        End If

    End Sub

    Private Sub CmdAG3EXEs_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles CmdPlayAG3.MouseUp, CmdMakeExe.MouseUp

        If e.Button = Windows.Forms.MouseButtons.Left Then

            Dim gameExe As String

            If sender.text = "Play" Then
                gameExe = GetSetting("gameExe")
            Else
                gameExe = GetSetting("makeExe")
            End If

            If File.Exists(gameExe) Then
                Process.Start(gameExe)
                Mes("Launched AG3 " & sender.text)
            Else
                ChangeGameExe(sender)
            End If

        Else
            ChangeGameExe(sender)
        End If

    End Sub

    Private Sub CboPriority_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CboPriority.SelectedIndexChanged

        Select Case CboPriority.SelectedIndex
            Case 0
                threadPriority = Threading.ThreadPriority.Highest
                processPriority = ProcessPriorityClass.RealTime
            Case 1
                threadPriority = Threading.ThreadPriority.AboveNormal
                processPriority = ProcessPriorityClass.High
            Case 2
                threadPriority = Threading.ThreadPriority.Normal
                processPriority = ProcessPriorityClass.Normal
            Case 3
                threadPriority = Threading.ThreadPriority.BelowNormal
                processPriority = ProcessPriorityClass.BelowNormal
        End Select

        threads.ThreadPriority = threadPriority

        SetSetting("programPriority", CboPriority.SelectedIndex)
        SaveXml()

    End Sub

    Private Sub ChkBackend_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChkBackend.CheckedChanged

        If ChkBackend.Checked Then
            backend = ProcessWindowStyle.Normal
        Else
            backend = ProcessWindowStyle.Hidden
        End If

        SetSetting("backendProcesses", backend)
        SaveXml()

    End Sub

    Private Sub TxtPreviewTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TxtPreviewTime.TextChanged

        If NotNothing(xmlSettings) Then

            If Val(TxtPreviewTime.Text) > 1 Then
                TxtPreviewTime.Text = Val(TxtPreviewTime.Text)
            Else
                TxtPreviewTime.Text = 1
            End If

            prevs.PicTime = Val(TxtPreviewTime.Text)

            SetSetting("previewTime", TxtPreviewTime.Text)
            SaveXml()

        End If

    End Sub

    Private Sub ChkRndPreviews_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChkRndPreviews.CheckedChanged

        SetSetting("randomPreviews", Math.Abs(CInt(ChkRndPreviews.Checked)))
        SaveXml()

        prevs.Random = ChkRndPreviews.Checked

    End Sub

    Private Sub CmdCharsPath_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdCharsPath.Click

        If Directory.Exists(ag3Chars) Then
            Process.Start(ag3Chars)
            Mes("Opened AG3 characters folder")
        End If

    End Sub

#End Region

#Region "download"

    Private Sub CmdRefreshDBList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdRefreshDBList.Click
        threads.AddParameter(ListDBChars)
        threads.AddParameter(LblCharCount)
        threads.AddParameter(ListChars)
        threads.Start(AG3DBCThreadType.GetDBCharsFeed)
    End Sub

    Private Sub Lists_ColumnWidthChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnWidthChangedEventArgs) Handles ListDBChars.ColumnWidthChanged, ListTopUsers.ColumnWidthChanged, ListTopChars.ColumnWidthChanged, ListUserChars.ColumnWidthChanged

        If Not isLoading Then
            SaveListColumns(sender)
        End If

    End Sub

    Private Sub ListDBChars_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListDBChars.SelectedIndexChanged

        If NotNothing(SelectedDBChar()) Then

            curDBChar = SelectedDBChar()

            Dim nfo As CharacterNfo = SelectedDBChar().Tag

            prevs.Clear()
            Directory.CreateDirectory(GetDBPreviewPath(nfo.Username))

            For Each oFile As FileInfo In AG3DBC.GetDBPreviews(nfo)
                prevs.Add(oFile.FullName)
            Next

            If NeedPreviews(nfo) Then
                Switch()
                StartThread(getPreviewsThread, AddressOf GetDBPreviews, threadPriority)
            End If

            prevs.Restart()

        End If

    End Sub

    Private Sub GetDBPreviews()

        active = True

        Dim nfo As CharacterNfo = SelectedDBChar().Tag

        Try

            ftp = FTPConnect()

            Directory.CreateDirectory(GetDBPreviewPath(nfo.Username))
            prevs.Clear()
            watch.StartWatch(nfo.PreviewsSize, threadPriority, ftp)

            For Each preview As String In nfo.Previews

                If Not File.Exists(GetDBPreviewPath(nfo.Username, preview)) Then
                    Mes("Getting preview " & preview & "...")
                    ftp.Download(FTP_DIR & "chars/" & nfo.Username & "/" & preview, GetDBPreviewPath(nfo.Username, preview), True)
                    prevs.Add(GetDBPreviewPath(nfo.Username, preview))
                End If

            Next

            watch.StopWatch()
            prevs.StartPics()
            Mes("Successfully downloaded all previews for " & nfo.CharacterName & "!", AG3DBCMessageType.Success)

            SelectedDBChar().ForeColor = GetDBCharColor(nfo)
            SelectedDBChar().SubItems(7).Text = "Yes"
            ChangeFullListItem(SelectedDBChar(), ListDBChars.Tag)

        Catch ex As Exception
            LogError(ex)
            watch.StopWatch()
            Mes("There was an error while trying to download the previews for " & nfo.CharacterName & ".", AG3DBCMessageType.Err, True)
        End Try

        Switch()
        ListDBChars.Select()
        ListDBChars.Focus()

        active = False

    End Sub

    Private Sub CmdDownload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdDownload.Click

        If NotNothing(SelectedDBChar()) Then

            Dim nfo As CharacterNfo = SelectedDBChar().Tag
            Dim ok As Boolean = True
            Dim newCharName As String = nfo.CharacterName

            Mes("Attempting to download " & nfo.CharacterName & "...")
            Switch()

            If File.Exists(ag3Chars & newCharName & AG3_CHAR_EXT) Then

                Mes("Character already exists. Prompting user...")

                Dim result As MsgBoxResult = MsgBox("The character " & nfo.CharacterName & " already exists in your AG3 characters folder." & endl & "Would you like to overwrite, rename or cancel the character download?" & endl & endl & "Yes = Overwrite   No = Rename   Cancel = Cancel", MsgBoxStyle.YesNoCancel + MsgBoxStyle.Question)

                If result = MsgBoxResult.No Then

                    While newCharName = nfo.CharacterName
                        newCharName = InputBox("Input the new name of the character " & nfo.CharacterName & ":", "Rename " & nfo.CharacterName, newCharName)
                    End While

                    ok = (Len(newCharName) > 0)

                ElseIf result = MsgBoxResult.Cancel Then
                    ok = False
                End If

            End If

            If ok Then

                active = True

                downloadThread = New Thread(AddressOf DownloadChar)
                downloadThread.Start(newCharName)

            Else
                Mes("Character download cancelled.")
                Switch()
            End If

        Else
            Mes("No character selected.", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub DownloadChar(ByVal charName As Object)

        Dim nfo As CharacterNfo = SelectedDBChar().Tag

        ftp = FTPConnect()

        Try
            If charName = nfo.CharacterName Then
                Mes("Downloading " & nfo.CharacterName & "...")
            Else
                Mes("Downloading " & nfo.CharacterName & " as " & charName & "...")
            End If

            watch.StartWatch(nfo.Size, threadPriority, ftp)
            ftp.Download(FTP_DIR & "chars/" & nfo.Username & "/" & nfo.CharacterName & AG3DB_EXT, ag3DBChars & charName & AG3DB_EXT, True)
            ExtractDownloadedChar(charName, nfo)
            watch.StopWatch()
            downloads.FinishDownload(nfo.CharacterId, SelectedDBChar(), ListChars)
            LoadLists(ListChars, LblCharacters, True)

            If crcs.Contains(charName) Then
                crcs.Remove(charName)
            End If

            nfo.Hits += 1
            SelectedDBChar().ForeColor = dbCharsColors.DefaultColor
            SelectedDBChar().SubItems(3).Text = nfo.Hits

            ChangeFullListItem(SelectedDBChar(), ListDBChars.Tag)
            crcs.Add(Conversion.Hex(New CRC32().GetCrc32(ag3Chars & charName & AG3_CHAR_EXT)).ToUpper, charName)

            If charName = nfo.CharacterName Then
                Mes("Sucessfully downloaded " & nfo.CharacterName & "! :D", AG3DBCMessageType.Success)
            Else
                Mes("Sucessfully downloaded " & nfo.CharacterName & " as " & charName & "! :D", AG3DBCMessageType.Success)
            End If

            SelectedDBChar().EnsureVisible()

            If Not OptDBCharDownloadAlways.Checked Then

                If OptDBCharDownloadNever.Checked Or (OptDBCharDownloadAsk.Checked AndAlso MsgBox("Show " & charName & " in Local Characters list now?", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.No) Then
                    Exit Try
                End If

            End If

            LoadLists(ListChars, LblCharacters)

            For i As Integer = 0 To ListChars.Items.Count - 1

                If ListChars.Items.Item(i).tolower = charName.tolower Then
                    ListChars.SelectedIndex = i : Exit For
                End If

            Next

            Tab.SelectedTab = Tab.TabPages("TabChars")

        Catch ex As Exception
            LogError(ex)
            Mes("There was an unexpected error trying to download " & nfo.CharacterName & ".", AG3DBCMessageType.Err, True)
            watch.StopWatch()
        End Try

        Switch()

        active = False

    End Sub

#End Region

    'Private Sub CboDBCharNames_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs)
    '    If e.KeyChar = vbCr Then
    '        FilterDBCharsList(CboDBCharDates.Text, CboDBCharNames.Text, "")
    '    End If
    'End Sub

    'Private Sub CboDBCharNames_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
    '    FilterDBCharsList(CboDBCharDates.SelectedItem, CboDBCharNames.SelectedItem, "")
    'End Sub

    Private Sub CmdRate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdRate.Click

        If CboRate.SelectedIndex >= 0 Then

            If NotNothing(SelectedDBChar()) Then

                Dim nfo As CharacterNfo = SelectedDBChar().Tag

                Switch()

                If ratings.CanRate(nfo) Then
                    ratings.RateChar(nfo, CboRate.SelectedItem, SelectedDBChar())
                    ListDBChars.Select()
                End If

                Switch()

            Else
                Mes("No character selected.", AG3DBCMessageType.Alert)
            End If

        Else
            Mes("Please choose a score before rating.", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub TxtUsername_TextChanged(ByVal sender As System.Object, ByVal e As Windows.Forms.KeyPressEventArgs) Handles TxtPassword.KeyPress, TxtUsername.KeyPress

        If e.KeyChar = vbCr Then
            CmdLogin_Click(Nothing, Nothing)
        End If

    End Sub

    Private Sub Lists_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles ListDBChars.ColumnClick, ListTopChars.ColumnClick, ListTopUsers.ColumnClick, ListUserChars.ColumnClick, ListUserRatings.ColumnClick

        Dim list As ListView = sender

        If sender.sorting = SortOrder.Ascending Then
            sender.sorting = SortOrder.Descending
        Else
            sender.sorting = SortOrder.Ascending
        End If

        SortListView(sender, e.Column, sender.sorting)

    End Sub

    Private Sub CmdUserChars_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdUserChars.Click
        threads.AddParameter(ListUserChars)
        threads.AddParameter(LblUserChars)
        threads.Start(AG3DBCThreadType.GetUserCharsFeed)
    End Sub

    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start(SERVER)
    End Sub

    Private Sub ListUserChars_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListUserChars.SelectedIndexChanged, ListUserRatings.SelectedIndexChanged

        If NotNothing(SelectedListItem(sender)) Then
            DoPreviews(SelectedListItem(sender).Tag.CharacterName)
        End If

    End Sub

    Private Sub CmdTopUsers_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdTopUsers.Click
        threads.AddParameter(ListTopUsers)
        threads.Start(AG3DBCThreadType.GetTopUsersFeed)
    End Sub

    Private Sub CmdTopChars_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdTopChars.Click
        threads.AddParameter(ListTopChars)
        threads.Start(AG3DBCThreadType.GetTopCharsFeed)
    End Sub

    Private Sub ListTopChars_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListTopChars.SelectedIndexChanged

        If NotNothing(SelectedListItem(sender)) Then

            Dim nfo As CharacterNfo = SelectedListItem(sender).Tag
            Dim root As New DirectoryInfo(ag3Previews & nfo.Username)

            prevs.Clear()

            If root.Exists Then

                For Each prev As FileInfo In root.GetFiles("* - " & nfo.CharacterName & ".jpg")
                    prevs.Add(prev.FullName)
                Next

                prevs.StartPics()

            End If

        End If

    End Sub

    Private Sub CmdTopCharsShowDBChar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdTopCharsShowDBChar.Click

        If NotNothing(SelectedListItem(ListTopChars)) Then

            Dim nfo As CharacterNfo = SelectedListItem(ListTopChars).Tag
            Dim name As String = nfo.CharacterName.ToLower
            Dim item As ListViewItem = Nothing
            Dim found As Boolean = False

            For Each item In ListDBChars.Items

                If item.SubItems(1).Text.ToLower = name Then
                    found = True : Exit For
                End If

            Next

            If found Then
                Tab.SelectedTab = Tab.TabPages("TabDBChars")
                item.Selected = True
                item.EnsureVisible()
            Else
                Mes(nfo.CharacterName & " was not found in the AG3DB Characters list.", AG3DBCMessageType.Alert)
            End If

        Else
            Mes("No character selected.", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub CmdRenameChar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdRenameChar.Click

        If NotNothing(ListChars.SelectedItem) Then

            Dim newName As String = InputBox("Enter new name for " & ListChars.SelectedItem, "Rename " & ListChars.SelectedItem, ListChars.SelectedItem)

            While File.Exists(ag3Chars & newName & AG3_CHAR_EXT)
                MsgBox("Character " & newName & " already exists.", MsgBoxStyle.Critical)
                newName = InputBox("Enter new name for " & ListChars.SelectedItem, "Rename " & ListChars.SelectedItem, ListChars.SelectedItem)
            End While

            If Len(newName) AndAlso newName <> ListChars.SelectedItem Then

                Dim oldIndex As Integer = ListChars.SelectedIndex

                RenameChar(ag3Chars & ListChars.SelectedItem, ListChars.SelectedItem, newName)

                For Each oFile As FileInfo In New DirectoryInfo(ag3Chars).GetFiles(ListChars.SelectedItem & "*")
                    oFile.MoveTo(ag3Chars & oFile.Name.Replace(ListChars.SelectedItem, newName))
                Next

                ListChars.Items.Remove(ListChars.SelectedItem)
                ListChars.Items.Insert(oldIndex, newName)

                ListChars.SelectedIndex = oldIndex

            End If

        Else
            Mes("No character selected.", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub CmdRemoveChar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdRemoveChar.Click

        If NotNothing(ListChars.SelectedItem) Then

            If RemoveChar(ListChars.SelectedItem) = MsgBoxResult.Yes Then

                Dim index As Integer = ListChars.SelectedIndex

                ListChars.Items.RemoveAt(index)

                If index < ListChars.Items.Count Then
                    ListChars.SelectedIndex = index
                ElseIf ListChars.Items.Count > 0 Then
                    ListChars.SelectedIndex = ListChars.Items.Count - 1
                End If

                LblCharacters.Text = "Characters: " & ListChars.Items.Count

            End If

        Else
            Mes("No character selected.", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub CmdUpdateTags_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdUpdateTags.Click

        If NotNothing(ListChars.SelectedItem) Then

            If IsCharOwner(ListDBChars, ListChars.SelectedItem) Then

                If TxtTags.Text <> oTags.GetDBTag(ListDBChars, ListChars.SelectedItem) Then
                    threads.AddParameter(ListChars.SelectedItem)
                    threads.AddParameter(TxtTags.Text)
                    threads.AddParameter(ListDBChars)
                    threads.Start(AG3DBCThreadType.UpdateTags)
                Else
                    Mes("Tags are identical to those in the database.", AG3DBCMessageType.Alert)
                End If

            Else
                Mes("You are not the owner of " & ListChars.SelectedItem & " or it does not exist in the database.", AG3DBCMessageType.Err, True)
            End If

        Else
            Mes("No character selected.", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub ClrDBChars_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ClrDBCharsNeedPreviews.Click, ClrDBCharsDefault.Click, ClrDBCharsNotInCharsList.Click

        Dim settingsNode As String = "dbCharsColors/"
        Dim curColor As Color

        Select Case sender.name
            Case "ClrDBCharsNeedPreviews"
                settingsNode += "needPreviewsColor"
                curColor = dbCharsColors.NeedPreviewsColor
            Case "ClrDBCharsNotInCharsList"
                settingsNode += "notInCharsListColor"
                curColor = dbCharsColors.NotInCharsListColor
            Case "ClrDBCharsDefault"
                settingsNode += "defaultColor"
                curColor = dbCharsColors.DefaultColor
        End Select

        Dim newColor As Color = ChooseColor(curColor)

        If newColor <> Color.Empty Then

            SetSetting(settingsNode, newColor.ToArgb)
            SaveXml()

            dbCharsColors = LoadDBCharsColors()
            sender.backcolor = newColor

            For Each item As ListViewItem In ListDBChars.Items
                item.ForeColor = GetDBCharColor(item.Tag)
            Next

        End If

    End Sub

    Private Sub Mes1_Click(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Mes1.MouseUp

        If e.Button = Windows.Forms.MouseButtons.Left AndAlso File.Exists(ag3dbclogsPath & GetSTDToday() & ".txt") Then
            Process.Start(ag3dbclogsPath & GetSTDToday() & ".txt")
        ElseIf e.Button = Windows.Forms.MouseButtons.Right AndAlso File.Exists(ag3dbclogsPath & GetSTDToday() & " (errors).txt") Then
            Process.Start(ag3dbclogsPath & GetSTDToday() & " (errors).txt")
        End If

    End Sub

    Private Sub OptDBCharDownload_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OptDBCharDownloadNever.CheckedChanged, OptDBCharDownloadAlways.CheckedChanged, OptDBCharDownloadAsk.CheckedChanged

        Select Case sender.name
            Case "OptDBCharDownloadNever" : SetSetting("afterCharDownload", "0")
            Case "OptDBCharDownloadAlways" : SetSetting("afterCharDownload", "1")
            Case "OptDBCharDownloadAsk" : SetSetting("afterCharDownload", "2")
        End Select

        SaveXml()

    End Sub

    Private Sub CboDefaultTab_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CboDefaultTab.SelectedIndexChanged
        If CboDefaultTab.SelectedIndex <> -1 Then
            SetSetting("defaultTab", CboDefaultTab.SelectedIndex)
            SaveXml()
        End If
    End Sub

    Private Sub CmdSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdSearch.Click

        Dim curTab As TabPage = Tab.SelectedTab

        If curTab.Name = "TabStats" Then
            curTab = TabStatistics.SelectedTab
        End If

        If Not oSearch.SearchOpen Or curTab.Name <> sender.tag Then

            oSearch.CloseSearch()

            Select Case curTab.Name
                Case "TabStats" : oSearch.GenerateSearch(ListUserChars)
                Case "TabUserChars" : oSearch.GenerateSearch(ListUserChars)
                Case "TabUserRatings" : oSearch.GenerateSearch(ListUserRatings)
                Case "TabTopChars" : oSearch.GenerateSearch(ListTopChars)
                Case "TabTopUsers" : oSearch.GenerateSearch(ListTopUsers)
                Case "TabDBChars" : oSearch.GenerateSearch(ListDBChars)
            End Select

            sender.tag = curTab.Name

        End If

    End Sub

    Private Sub CmdUserRatings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdUserRatings.Click
        threads.AddParameter(ListUserRatings)
        threads.AddParameter(LblUserRating)
        threads.Start(AG3DBCThreadType.GetUserRatingsFeed)
    End Sub
    Dim table As New TableLayoutPanel
    Dim radio1 As New RadioButton()
    Dim radio2 As New RadioButton()
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        radio1.Text = "Match Any"
        radio2.Text = "Match All"
        radio1.Anchor = AnchorStyles.Right
        radio2.Anchor = AnchorStyles.Left
        table.Size = New Size(335, 35)
        table.Location = New Point(100, 300)
        table.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetDouble
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        table.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        table.Controls.Add(radio1, 0, 0)
        table.Controls.Add(radio2, 1, 0)
        Tab.TabPages("TabAbout").Controls.Add(table)
    End Sub

End Class



