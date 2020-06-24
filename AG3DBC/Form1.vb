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
'   CHANGE UPLOAD2
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
    Dim _curList As ListView

    Public Sub LoadColorControls()
        ClrDBCharsNeedPreviews.BackColor = dbCharsColors.NeedPreviewsColor
        ClrDBCharsNotInCharsList.BackColor = dbCharsColors.NotInCharsListColor
        ClrDBCharsDefault.BackColor = dbCharsColors.DefaultColor
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

            LoadGlobals(Me)
            Mes("AG3DBC launched (version " & AG3DBC_VERSION & ")")

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
            LoadLists(Tab)
            Directory.CreateDirectory(ag3Previews)
            Directory.CreateDirectory(ag3DBChars)
            Directory.CreateDirectory(ag3dbcLogsPath)

        Else
            MsgBox("Artificial Girl 3 was not found on your system. Please install the game and try again.", MsgBoxStyle.Critical)
            End
        End If

        isLoading = False

        If AG3DBC.API.FirstRun Then
            Dim warn As String = String.Format("You are running AG3DB-Revival for the first time. I just created revival-settings.xml. Please edit that file and point it to the correct Azusarkus URL. The current URL is now {0} - this is probably not what you want.", AG3DBC.API.ServerUrl)
            MsgBox(warn, MsgBoxStyle.OkOnly, Nothing)
        End If
    End Sub

    Private Sub LoadControls()

        switchControls.Add(Tab)
        switchControls.Add(TabOptions)

        TabNews.Tag = Nothing

    End Sub

    Private Sub LoadLists(ByVal startObj As Control)

        If startObj.GetType().Name = "TabControl" Then

            For Each tabPage As TabPage In DirectCast(startObj, TabControl).TabPages
                LoadLists(tabPage)
            Next

        ElseIf startObj.GetType().Name = "TabPage" Then

            For Each ctrl As Control In startObj.Controls

                If ctrl.GetType().Name = "ListView" Then
                    LoadListColumns(ctrl)
                    AddHandler DirectCast(ctrl, ListView).ColumnClick, AddressOf Lists_ColumnClick
                    AddHandler DirectCast(ctrl, ListView).ColumnWidthChanged, AddressOf Lists_ColumnWidthChanged
                    AddHandler DirectCast(ctrl, ListView).MouseUp, AddressOf CopyListItemToClip
                ElseIf ctrl.GetType().Name = "TabControl" Then

                    For Each tabPage As TabPage In DirectCast(ctrl, TabControl).TabPages
                        LoadLists(tabPage)
                    Next

                End If

            Next

        End If

    End Sub

    Private Sub CopyListItemToClip(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        If e.Button = Windows.Forms.MouseButtons.Right Then

            Dim list As ListView = sender
            Dim text As String = list.GetItemAt(e.X, e.Y).GetSubItemAt(e.X, e.Y).Text
            Dim m As New ContextMenu
            Dim s As MenuItem

            s = New MenuItem("Copy " & text & " to Clipboard", New EventHandler(AddressOf DoSubItewmClip))
            s.Tag = text

            m.MenuItems.Add(s)
            m.Show(sender, e.Location)

        End If

    End Sub

    Private Sub DoSubItewmClip(ByVal sender As Object, ByVal e As System.EventArgs)
        Clipboard.SetText(sender.tag)
    End Sub

    Private Sub CmdUpload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdUpload.Click

        If NotNothing(SelectedListItem(ListLocals)) Then

            Dim nfo As CharacterNfo = SelectedListItem(ListLocals).Tag

            If MsgBox("Are you absolutely sure you want to upload " & nfo.CharacterName & " to the AG3DB?", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then

                Dim result As MsgBoxResult = MsgBox("Do you want to include the entire character structure?" & endl & "(choose no if no there are no modded files inside the character's folder)", MsgBoxStyle.Question + MsgBoxStyle.YesNoCancel)

            If result <> MsgBoxResult.Cancel Then
                    prevs.Pause()
                    threads.AddParameter(nfo.CharacterName)
                    threads.AddParameter(nfo.Tags)
                    threads.AddParameter(nfo.Description)
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
        LoadLocals(ListLocals, LblCharacters)
    End Sub

    Private Sub LblRegister_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)
        Process.Start(AG3DBC.API.ServerUrl & "register.php")
    End Sub

    Private Sub CmdAddPreviews_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdAddPreviews.Click

        If NotNothing(SelectedListItem(ListLocals)) Then

            Dim nfo As CharacterNfo = SelectedListItem(ListLocals).Tag
            Dim files() As String = OpenFiles("JPGs (*.jpg, *.jpeg)|*.jpg;*.jpeg", , "Choose previews to add to " & nfo.CharacterName)

            If NotNothing(files) Then

                Dim count As Integer = 1

                Directory.CreateDirectory(ag3Chars & nfo.CharacterName)

                For Each sFile As String In files

                    While File.Exists(ag3Chars & nfo.CharacterName & "\" & count & " - " & nfo.CharacterName & ".jpg")
                        count += 1
                    End While

                    File.Copy(sFile, ag3Chars & nfo.CharacterName & "\" & count & " - " & nfo.CharacterName & ".jpg", True)

                Next

                Mes("Added " & Plural("preview", files.Length) & " to " & nfo.CharacterName)
                nfo.DoLocalPreviews()

            End If

        Else
            Mes("No character selected", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub CmdRemovePreview_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles CmdRemovePreview.MouseClick

        If NotNothing(SelectedListItem(ListLocals)) Then

            Dim nfo As CharacterNfo = SelectedListItem(ListLocals).Tag
            Dim oDir As New DirectoryInfo(ag3Chars & nfo.CharacterName)

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

        If NotNothing(SelectedListItem(ListLocals)) Then

            Dim nfo As CharacterNfo = SelectedListItem(ListLocals).Tag

            prevs.Pause()
            LoadPic(Pic, PicFrame, GetPreviewPath(nfo.CharacterName) & sender.text)

            If MsgBox("Are you sure you want to remove " & sender.Text & "?" & endl & "(the file will also be deleted from your hard disk)", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                prevs.Remove(sender.Text)
                RemoveFile(ag3Chars & nfo.CharacterName & "\" & sender.text)
                Mes("Removed preview " & sender.text)
                nfo.DoLocalPreviews()
            Else
                nfo.DoLocalPreviews()
            End If

        End If

    End Sub

    Private Sub CmdLogin_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdLogin.Click

        CmdLogin.Enabled = False

        Mes("Attempting to login...", AG3DBCMessageType.Normal, False)

        If Login(TxtUsername.Text, TxtPassword.Text, ChkRemember.Checked) Then

            ActivateClient()
            LoadLocals(ListLocals, LblCharacters)
            Mes("Logged in successfully as " & GetUsername() & "! :D", AG3DBCMessageType.Success)
            Tab_Click(Tab, Nothing)

            LblLoggedInAs.Text = "Logged in as " & GetUsername()

        Else
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

        ChkAutoPreviews.Checked = (GetSetting("autoPreviews") = "1")
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
        LoadListColumns(ListLocals)

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
        'threads.AddParameter(1)
        threads.AddParameter(ListDBChars)
        threads.AddParameter(LblCharCount)
        threads.AddParameter(ListLocals)
        threads.Start(AG3DBCThreadType.GetDBCharsFeed)
    End Sub

    Private Sub ListDBChars_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListDBChars.SelectedIndexChanged

        If NotNothing(SelectedDBChar()) Then

            curDBChar = SelectedDBChar()

            Dim nfo As CharacterNfo = SelectedDBChar().Tag

            If ChkAutoPreviews.Checked AndAlso nfo.HavePreviews Then
                nfo.GetPreviews()
            End If

        End If

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
            downloads.FinishDownload(nfo.CharacterId, SelectedDBChar(), ListLocals)
            LoadLocals(ListLocals, LblCharacters)

            Dim crc As String = Conversion.Hex(New CRC32().GetCrc32(ag3Chars & charName & AG3_CHAR_EXT)).ToUpper

            If localNfos.Contains(crc) Then
                localNfos.Remove(crc)
            End If

            nfo.Hits += 1
            SelectedDBChar().ForeColor = dbCharsColors.DefaultColor
            SelectedDBChar().SubItems(3).Text = nfo.Hits

            ChangeFullListItem(SelectedDBChar(), ListDBChars.Tag)
            nfo.Description = nfo.Description
            nfo.Tags = nfo.Tags
            localNfos.Add(nfo, crc)

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

            LoadLocals(ListLocals, LblCharacters)

            For Each item As ListViewItem In ListLocals.Items

                If item.SubItems(2).Text.ToLower = charName.tolower Then
                    item.Selected = True
                    item.EnsureVisible()
                    Exit For
                End If

            Next

            Tab.SelectedTab = Tab.TabPages("TabLocals")

        Catch ex As Exception
            LogError(ex)
            Mes("There was an unexpected error trying to download " & nfo.CharacterName & ".", AG3DBCMessageType.Err, True)
            watch.StopWatch()
        End Try

        Switch()

        active = False

    End Sub

#End Region

    Private Sub CmdRate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdRate.Click

        If CboRate.SelectedIndex >= 0 Then

            If NotNothing(SelectedDBChar()) Then

                Dim nfo As CharacterNfo = SelectedDBChar().Tag

                Switch()

                If ratings.CanRate(nfo, SelectedDBChar()) Then
                    ratings.Rate(nfo, CboRate.SelectedItem, SelectedDBChar())
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

    Private Sub Lists_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs)

        Dim list As ListView = sender

        If sender.sorting = SortOrder.Ascending Then
            sender.sorting = SortOrder.Descending
        Else
            sender.sorting = SortOrder.Ascending
        End If

        SortListView(sender, e.Column, sender.sorting)

    End Sub

    Private Sub Lists_ColumnWidthChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnWidthChangedEventArgs)

        If Not isLoading Then
            SaveListColumns(sender)
        End If

    End Sub

    Private Sub CmdUserChars_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdUserChars.Click
        threads.AddParameter(ListUserChars)
        threads.AddParameter(LblUserChars)
        threads.Start(AG3DBCThreadType.GetUserCharsFeed)
    End Sub

    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start(AG3DBC.API.ServerUrl)
    End Sub

    Private Sub ListUserChars_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListUserChars.SelectedIndexChanged, ListUserRatings.SelectedIndexChanged

        If NotNothing(Selected(sender, False)) Then
            DirectCast(Selected(sender).Tag, CharacterNfo).DoLocalPreviews()
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
            DirectCast(Selected(sender).Tag, CharacterNfo).DoDBPreviews()
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

        If NotNothing(SelectedListItem(ListLocals)) Then

            Dim nfo As CharacterNfo = SelectedListItem(ListLocals).Tag
            Dim newName As String = InputBox("Enter new name for " & nfo.CharacterName, "Rename " & nfo.CharacterName, nfo.CharacterName)

            While File.Exists(ag3Chars & newName & AG3_CHAR_EXT)
                MsgBox("Character " & newName & " already exists.", MsgBoxStyle.Critical)
                newName = InputBox("Enter new name for " & nfo.CharacterName, "Rename " & nfo.CharacterName, nfo.CharacterName)
            End While

            If Len(newName) AndAlso newName <> nfo.CharacterName Then

                Dim oldIndex As Integer = nfo.CharacterName

                RenameChar(ag3Chars & nfo.CharacterName, nfo.CharacterName, newName)

                For Each oFile As FileInfo In New DirectoryInfo(ag3Chars).GetFiles(nfo.CharacterName & "*")
                    oFile.MoveTo(ag3Chars & oFile.Name.Replace(nfo.CharacterName, newName))
                Next

                ListLocals.Items.Remove(SelectedListItem(ListLocals))
                ListLocals.Items.Insert(oldIndex, newName)

                ListLocals.Items(oldIndex).Selected = True

            End If

        Else
            Mes("No character selected.", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub CmdRemoveChar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdRemoveChar.Click

        If NotNothing(SelectedListItem(ListLocals)) Then

            Dim nfo As CharacterNfo = SelectedListItem(ListLocals).Tag

            If RemoveChar(nfo.CharacterName) = MsgBoxResult.Yes Then

                Dim index As Integer = SelectedListItem(ListLocals).Index

                ListLocals.Items.RemoveAt(index)

                If index < ListLocals.Items.Count Then
                    ListLocals.Items(index).Selected = True
                ElseIf ListLocals.Items.Count > 0 Then
                    ListLocals.Items(ListLocals.Items.Count - 1).Selected = True
                End If

                LblCharacters.Text = "Characters: " & ListLocals.Items.Count

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

        If e.Button = Windows.Forms.MouseButtons.Left AndAlso File.Exists(ag3dbcLogsPath & GetSTDToday() & ".txt") Then
            Process.Start(ag3dbcLogsPath & GetSTDToday() & ".txt")
        ElseIf e.Button = Windows.Forms.MouseButtons.Right AndAlso File.Exists(ag3dbcLogsPath & GetSTDToday() & " (errors).txt") Then
            Process.Start(ag3dbcLogsPath & GetSTDToday() & " (errors).txt")
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
                Case "TabLocals" : oSearch.GenerateSearch(ListLocals)
            End Select

            sender.tag = curTab.Name

        End If

    End Sub

    Private Sub CmdUserRatings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdUserRatings.Click
        threads.AddParameter(ListUserRatings)
        threads.AddParameter(LblUserRating)
        threads.Start(AG3DBCThreadType.GetUserRatingsFeed)
    End Sub

    Private Sub ListLocals_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListLocals.SelectedIndexChanged

        If NotNothing(Selected(ListLocals, False)) Then
            DirectCast(Selected(ListLocals).Tag, CharacterNfo).DoLocalPreviews()
        End If

    End Sub

    'Private Sub CmdDetails_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdDetails.Click

    '    If NotNothing(SelectedListItem(ListLocals)) Then
    '        FrmEditDetails.ShowMe(SelectedListItem(ListLocals).Tag, SelectedListItem(ListLocals))
    '    Else
    '        Mes("No character selected.", AG3DBCMessageType.Alert)
    '    End If

    'End Sub

    Private Sub CmdUpdateDetails_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdUpdateDetails.Click

        If NotNothing(SelectedListItem(ListLocals)) Then

            Dim nfo As CharacterNfo = SelectedListItem(ListLocals).Tag

            If MsgBox("Are you sure you want to update the details of " & nfo.Type.ToString.ToLower & " " & nfo.CharacterName & " in the AG3DB?", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then

                Dim d As New Details()

                d.UpdateDetails(nfo)

            End If
        Else
            Mes("Nothing selected.", AG3DBCMessageType.Alert)
        End If

    End Sub

    Private Sub CmdViewDesc_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        If NotNothing(Selected(ListDBChars)) Then
            Dim frmDesc As New DescForm(Selected(ListDBChars).Tag)
        End If

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        HtmlHelp.DocumentText = File.ReadAllText(appPath & "\help.html")

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

        Dim retVal As String = GetHttpResponseString(AG3DBC.API.ServerUrl & "testConnect.php")

        If retVal = "0" Then
            Mes("You can communicate with the AG3DB webserver.", AG3DBCMessageType.Success, True)
        Else
            Mes(retVal, AG3DBCMessageType.Err, True)
        End If

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        threads.AddParameter(ListSets)
        threads.Start(AG3DBCThreadType.GetSets)
    End Sub

    Private Sub ListSets_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListSets.SelectedIndexChanged

        If NotNothing(Selected(sender, False)) Then

            Dim nfo As SetNfo = Selected(sender).Tag

            ListSetObjs.Items.Clear()

            For Each obj As String In nfo.Objects
                ListSetObjs.Items.Add(obj)
            Next

        End If

    End Sub

    'Private Sub ListSetObjs_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListSetObjs.SelectedIndexChanged

    '    If NotNothing(Selected(sender, False)) Then

    '        Dim nfo As CharacterNfo = Selected(sender, False).Tag

    '        If nfo.NeedPreviews Then
    '            threads.AddParameter(nfo)
    '            threads.Start(AG3DBCThreadType.GetPreviews)
    '        Else
    '            nfo.DoDBPreviews()
    '        End If

    '    End If

    'End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click

        Dim list As ListView = GetSelectedList(Tab)

        Pages.ListView = list
        Pages.MakePages(25)
        Pages.MainForm = Me
        Pages.CallBack = Me.GetType.GetMethod("abc")
    End Sub

    Public Sub abc(ByVal page As Integer)
        MsgBox("Page " & page)
    End Sub

    Private Sub ChkAutoPreviews_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChkAutoPreviews.CheckedChanged
        SetSetting("autoPreviews", Math.Abs(CInt(ChkAutoPreviews.Checked)))
        SaveXml()
    End Sub

    Private Sub CmdGetPreview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdGetPreview.Click

    End Sub
End Class



