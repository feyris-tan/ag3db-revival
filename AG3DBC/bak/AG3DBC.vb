Imports System.IO
Imports System.Xml
Imports System.Threading
Imports Microsoft.Win32

Module AG3DBC

    Public Const AG3DBC_VERSION As String = "1.01.1"
    Public Const AG3_REGKEY As String = "Software\illusion\JS3"
    Public Const AG3DBC_REGKEY As String = "Software\AG3DBC"
    Public Const AG3DB_EXT As String = ".7z"
    Public Const SERVER As String = "http://ag3.illusionist.sosavalanche.net/"
    Public Const FTP_USER As String = "ag3"
    Public Const FTP_PWD As String = "93cc8cabcb8b96"
    Public Const FTP_ADDR As String = "ftp://sosavalanche.net"
    Public Const FTP_DIR As String = "ag3files/"
    Public Const MAX_FILE_SIZE As Double = 1024 * 1024 * 5
    Public Const MAX_PREV_SIZE As Double = 1024 * 150
    Public Const AG3_CHAR_EXT As String = ".js3cmi"
    Public Const AG3_CHAR_UP As String = ".js3up"
    Public Const UPLOAD_WAIT_TIME As Integer = 60
    Public Const RATE_WAIT_TIME As Integer = 3600 * 24 * 7
    Public Const NO_RATING As String = "(n/a)"
    Public Const JS3CMI_SIZE As Double = 1092
    Public Const ERROR_SEPERATOR As String = "--------------------------------"
    Public Const HF_AG3DBC_POST As String = "http://www.hongfire.com/forum/showthread.php?p=1387827#post1387827"
    Public AG3DB_BIRTHDAY As New Date(2007, 11, 18, 0, 0, 0)

    Public ag3Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(AG3_REGKEY)
    Public ag3dbcReg As RegistryKey = Registry.CurrentUser.OpenSubKey(AG3DBC_REGKEY, True)
    Public ag3Path As String = ag3Reg.GetValue("INSTALLDIR")
    Public ag3Data As String = ag3Path & "Data\"
    Public ag3Chars As String = ag3Data & "save\m_cha\"
    Public ag3Previews As String = appPath & "\Previews\"
    Public ag3DBChars As String = appPath & "\tempChars\"
    Public ag3dbclogsPath As String = appPath & "\Logs\"
    Public zip7exe As String = appPath & "\Tools\7z\7z.exe"
    Public ag3Decrypt As String = appPath & "\Tools\AG3Decrypt.exe"
    Public tabs As New Collection
    Public xmlSettings As XmlDocument
    Public xmlFile As String = appPath & "\settings.xml"
    Public threadPriority As ThreadPriority
    Public processPriority As ProcessPriorityClass
    Public backend As Diagnostics.ProcessWindowStyle
    Public ftp As Utilities.FTP.FTPclient
    Public watch As TransferWatch
    Public threads As AG3DBCThreads
    Public downloads As New Download
    Public ratings As New Rating
    Public oTags As New Tags
    Public oSearch As Search2
    Public switchControls As New Collection
    Public dontSwitchControls As New Collection
    Public clickedTabs As New Collection
    Public prevs As Previews
    Public isLoading As Boolean
    Public crcs As New Collection
    Public dbCharsColors As DBCharColor
    Public curDBChar As ListViewItem

    Public frm As Form = Form1
    Private Tab As TabControl = frm.Controls("Tab")
    Private CmdLogin As Button = frm.Controls("GrpLogin").Controls("CmdLogin")
    Private CmdLogout As Button = frm.Controls("GrpLogin").Controls("CmdLogout")
    Private TxtUsername As TextBox = frm.Controls("GrpLogin").Controls("TxtUsername")
    Private TxtPassword As TextBox = frm.Controls("GrpLogin").Controls("TxtPassword")
    Private Status As StatusStrip = frm.Controls("Status")
    Private ChkRemember As CheckBox = frm.Controls("GrpLogin").Controls("ChkRemember")
    Private TT As Control = frm.Controls("TT")
    Private statusTimerThread As Thread
    Private lastMes As String = ""
    Private mesStartTime As Date = Nothing
    Private CboDefaultTab As ComboBox = frm.Controls("TabOptions").Controls("TabOptionsMisc").Controls("CboDefaultTab")

    Public Sub LoadHelp()
        Form1.HtmlHelp.DocumentText = File.ReadAllText(appPath & "\help.html")
    End Sub

    Public Sub LogError(Optional ByVal ex As Exception = Nothing, Optional ByVal extraInfo As String = "")

        Dim str As String = ERROR_SEPERATOR & endl & "Error on " & GetSTDToday() & " @ " & GetSTDTime() & " (using v" & AG3DBC_VERSION & ")" & endl

        If NotNothing(ex) Then

            If threads.ThreadType <> AG3DBCThreadType.NullFeed Then
                str += "From " & threads.ThreadType.ToString & ": " & ex.Message & endl
            Else
                str += "From " & ex.Source & ": " & ex.Message & endl
            End If

            str += "Stack Trace:" & endl & ex.StackTrace & endl

            If NotNothing(ex.InnerException) Then
                str += "InnerException:" & endl
                str += "   From " & ex.Source & ": " & ex.Message & endl
                str += "   Stack Trace:" & endl & ex.StackTrace.Replace(endl, endl & "   ")
            End If

        End If

        If Len(extraInfo) Then
            str += "Other info:" & endl & extraInfo
        End If

        File.AppendAllText(ag3dbclogsPath & GetSTDToday() & " (errors).txt", str)

    End Sub

    Public Sub LoadLists(ByVal list As ListBox, ByVal LblCharacters As Label, Optional ByVal saveIndex As Boolean = False)

        Dim root As New DirectoryInfo(ag3Chars)
        Dim curChar As String = list.SelectedItem

        list.Items.Clear()
        prevs.Clear()
        crcs.Clear()

        For Each oFile As FileInfo In root.GetFiles("*.js3cmi")
            list.Items.Add(RemoveExtention(oFile.Name))
            crcs.Add(Conversion.Hex(New CRC32().GetCrc32(oFile.FullName)).ToUpper, RemoveExtention(oFile.Name))
        Next

        If saveIndex AndAlso NotNothing(curChar) AndAlso list.Items.Contains(curChar) Then
            list.SelectedItem = curChar
        End If

        LblCharacters.Text = "Characters: " & list.Items.Count

        Mes("Character list refreshed (" & list.Items.Count & " total)")

    End Sub

    Public Sub CopyListView(ByVal listSource As ListView, ByVal listDest As ListView)

        Dim i As Integer
        Dim newItem As ListViewItem

        listDest.Items.Clear()

        For Each item As ListViewItem In listSource.Items

            newItem = New ListViewItem(item.Text)
            newItem.ForeColor = item.ForeColor
            newItem.Tag = item.Tag

            For i = 1 To item.SubItems.Count - 1
                newItem.SubItems.Add(item.SubItems(i))
            Next

            listDest.Items.Add(newItem)

        Next

    End Sub

    Public Sub ChangeFullListItem(ByVal item As ListViewItem, ByVal fullList As ListView)

        Dim fullItem As ListViewItem = fullList.Items(item.Tag.listindex)

        fullItem.ForeColor = item.ForeColor
        fullItem.Tag = item.Tag

        For i As Integer = 0 To item.SubItems.Count - 1
            fullItem.SubItems(i) = item.SubItems(i)
        Next

    End Sub

    Public Function LoadDBCharsColors() As DBCharColor

        Dim colors As DBCharColor

        Try
            colors.DefaultColor = Color.FromArgb(CInt(GetSetting("dbCharsColors/defaultColor")))
        Catch
            colors.DefaultColor = Color.Black
        End Try

        Try
            colors.NeedPreviewsColor = Color.FromArgb(CInt(GetSetting("dbCharsColors/needPreviewsColor")))
        Catch
            colors.NeedPreviewsColor = Color.Blue
        End Try

        Try
            colors.NotInCharsListColor = Color.FromArgb(CInt(GetSetting("dbCharsColors/notInCharsListColor")))
        Catch
            colors.NotInCharsListColor = Color.Green
        End Try

        Return colors

    End Function

    Public Function GetDBCharColor(ByVal nfo As CharacterNfo) As System.Drawing.Color

        If NeedPreviews(nfo) Then
            Return dbCharsColors.NeedPreviewsColor
        ElseIf Not InCharList(nfo.CharacterName, nfo.CRC32) Then
            Return dbCharsColors.NotInCharsListColor
        Else
            Return dbCharsColors.DefaultColor
        End If

    End Function

    Public Function InCharList(ByVal charName As String, ByVal crc As String) As Boolean
        Return crcs.Contains(charName) AndAlso crcs(charName) = crc
    End Function

    Public Function CheckAG3Install() As Boolean

        If IsNothing(ag3Reg) Then
            Return False
        End If

        If IsNothing(ag3dbcReg) Then

            If Not Disclamer() Then
                End
            End If

            ag3dbcReg = Registry.CurrentUser.CreateSubKey(AG3DBC_REGKEY, RegistryKeyPermissionCheck.ReadWriteSubTree)

            ag3dbcReg.SetValue("curChar", "")
            ag3dbcReg.SetValue("uploadId", "")
            ag3dbcReg.SetValue("uploadPwd", "")

        End If

        Directory.CreateDirectory(ag3Previews)
        Directory.CreateDirectory(ag3DBChars)
        Directory.CreateDirectory(ag3dbclogsPath)

        Return True

    End Function

    Public Function IsCharOwner(ByVal list As ListView, ByVal charName As String)

        Dim nfo As CharacterNfo
        Dim userId As String = GetUserId()

        For Each item As ListViewItem In list.Items

            nfo = item.Tag

            If nfo.CharacterName = charName AndAlso nfo.UserId <> userId Then
                Return False
            End If

        Next

        Return True

    End Function

    Public Sub ExtractDownloadedChar(ByVal charName As String, ByVal nfo As CharacterNfo)

        Mes("Extracting character structure...")

        Dim dlFolder As String = ag3DBChars & (New DirectoryInfo(ag3DBChars).GetDirectories().Length)
        Dim zip7params As String = "x " & qq(ag3DBChars & charName & AG3DB_EXT) & " -y -o" & qq(dlFolder)

        Directory.CreateDirectory(dlFolder)
        StartProcess(zip7exe, zip7params, backend, processPriority)

        If charName <> nfo.CharacterName Then
            RenameChar(dlFolder, nfo.CharacterName, charName)
        End If

        CopyDir(dlFolder, ag3Chars)
        EmptyDir(ag3DBChars)

        If Directory.Exists(ag3Chars & charName & "\" & charName) Then
            StartProcess(ag3Decrypt, qq(ag3Chars & charName & "\" & charName), backend, processPriority)
            RemoveDir(ag3Chars & charName & "\" & charName)
        End If

    End Sub

    Public Sub SaveListColumns(ByVal list As ListView)

        Dim listNode As XmlNode = GetSettingNode("lists/" & list.Name)

        If IsNothing(listNode) Then
            listNode = NewXmlNode(xmlSettings, list.Name)
            GetSettingNode("lists").AppendChild(listNode)
        Else
            listNode.RemoveAll()
        End If

        For i As Integer = 0 To list.Columns.Count - 1
            listNode.AppendChild(NewXmlNode(xmlSettings, "column" & i, list.Columns(i).Width))
        Next

        SaveXml()

    End Sub

    Public Sub LoadListColumns(ByVal list As ListView)

        Dim listNode As XmlNode = GetSettingNode("lists/" & list.Name)

        If NotNothing(listNode) Then

            For i As Integer = 0 To list.Columns.Count - 1

                If NotNothing(listNode.SelectSingleNode("column" & i)) Then
                    list.Columns(i).Width = Val(XmlNodeText(listNode.SelectSingleNode("column" & i)))
                Else
                    list.Columns(i).Width = 60
                End If

            Next

        End If

    End Sub

    Public Function GetNfo(ByVal node As XmlNode) As Collection

        Dim c As New Collection

        For Each curNode As XmlNode In node.ChildNodes
            c.Add(XmlNodeText(curNode), curNode.Name)
        Next

        Return c

    End Function

    Public Function Disclamer() As Boolean

        Dim str As String = ""

        str += "Before you use this program you MUST agree to the following terms:" & endl
        str += endl
        str += "1) YOU WILL NOT ALTER THE PROGRAMS'S SETTINGS OR ATTEMPT TO DECOMPLIE/RECOMPLIE THIS PROGRAM." & endl
        str += "2) YOU WILL NOT TRY AND USE THIS PROGRAM TO CONDUCT ANY KIND OF MALICIOUS ACTIVITY TOWARDS THE WEB SERVER. THIS INCLUDES HACKING AND SPAMMING." & endl
        str += endl
        str += "Failure to comply with these terms will result in an immediate ban and could lead to the server going down FOR GOOD (so don't ruin it for everybody)." & endl
        str += endl
        str += "CAN YOU COMPLY WITH THESE TERMS?"

        Return (MsgBox(str, MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo) = MsgBoxResult.Yes)

    End Function

    Public Function ParseRetVal(ByVal retVal As String) As String()

        Try

            Dim spl() As String = Split(retVal, "|")

            If spl.GetValue(0) = "0" AndAlso spl.Length > 1 Then
                Return spl
            Else
                Mes("Error parsing server response.", AG3DBCMessageType.Alert, True)
                Return Nothing
            End If

        Catch ex As Exception
            LogError(ex)
            Mes("Error parsing server response.", AG3DBCMessageType.Alert, True)
            Return Nothing
        End Try

    End Function

    Public Function RemoveChar(ByVal charName As String) As MsgBoxResult

        If MsgBox("Are you sure you want to remove " & charName & "?", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then

            Dim result As MsgBoxResult = MsgBox("Also delete " & charName & AG3_CHAR_EXT & "?", MsgBoxStyle.Question + MsgBoxStyle.YesNoCancel)

            If result = MsgBoxResult.Cancel Then
                Return result
            ElseIf result = MsgBoxResult.Yes Then
                RemoveFile(ag3Chars & charName & AG3_CHAR_EXT)
            End If

            RemoveFile(ag3Chars & charName & "_v.bmp")
            RemoveFile(ag3Chars & charName & ".js3up")
            RemoveDir(ag3Chars & charName)

            If result = MsgBoxResult.Yes Then
                Mes("Completely removed " & charName & ".")
            Else
                Mes("Removed " & charName & ".")
            End If

            Return result

        End If

        Return MsgBoxResult.No

    End Function

    Public Sub RenameChar(ByVal startPath As String, ByVal oldCharName As String, ByVal newCharName As String)

        Dim root As New DirectoryInfo(startPath)

        For Each oDir As DirectoryInfo In root.GetDirectories()
            RenameChar(oDir.FullName, oldCharName, newCharName)
        Next

        For Each oFile As FileInfo In root.GetFiles()
            If oFile.Name.Contains(oldCharName) Then
                oFile.MoveTo(oFile.Directory.FullName & "\" & oFile.Name.Replace(oldCharName, newCharName))
            End If
        Next

        If root.Name.Contains(oldCharName) Then
            root.MoveTo(root.Parent.FullName & "\" & newCharName)
        End If

    End Sub

    Public Function NeedPreviews(ByVal nfo As CharacterNfo) As Boolean

        Dim allExists As Boolean = True

        For Each preview As String In nfo.Previews

            If Not File.Exists(GetDBPreviewPath(nfo.Username, preview)) Then
                allExists = False
                Exit For
            End If

        Next

        Return Not allExists

    End Function

    Public Sub CancelUpload()

        watch.StopWatch()

        Dim queryString As String = ""

        queryString += "id=" & ag3dbcReg.GetValue("uploadId")
        queryString += "&pwd=" & ag3dbcReg.GetValue("uploadPwd")
        queryString += "&userId=" & GetUserId()

        ag3dbcReg.SetValue("curChar", "")
        ag3dbcReg.SetValue("uploadId", "")
        ag3dbcReg.SetValue("uploadPwd", "")
        'MsgBox(GetHttpResponseString(SERVER & "cancelUpload.php?" & queryString))
        Mes("Upload cancelled.", AG3DBCMessageType.Err, False)

    End Sub

    Public Function Login(ByVal username As String, ByVal password As String, ByVal remember As Boolean) As Boolean

        If username <> "" AndAlso password <> "" Then

            Dim userNfo As UserInfo = AuthenticateUser(username, password)

            If NotNothing(userNfo.username) Then

                TxtUsername.Text = userNfo.username

                If ag3dbcReg.GetValue("uploadId") <> "" Or ag3dbcReg.GetValue("uploadId") <> "" Then
                    CancelUpload()
                End If

                SetSetting("login/userId", userNfo.userId)
                SetSetting("login/username", userNfo.username)
                SetSetting("login/password", userNfo.password)
                SetSetting("login", Math.Abs(CInt(remember)), "remember")

            Else
                SetSetting("login/userId", "")
                SetSetting("login/username", "")
                SetSetting("login/password", "")
            End If

            SaveXml()

            Return NotNothing(userNfo.username)

        Else
            Return False
        End If

    End Function

    Public Sub Logout()

        ChkRemember.Checked = False

        SetSetting("login/userId", "")
        SetSetting("login/username", "")
        SetSetting("login/password", "")
        SetSetting("login", "0", "remember")
        SaveXml()
        DeactivateClient()
        Mes("Logged out successfully")

    End Sub

    Public Sub Mes(ByVal message As String, Optional ByVal messageType As AG3DBCMessageType = AG3DBCMessageType.Normal, Optional ByVal displayMsgbox As Boolean = False, Optional ByVal showTimeSpan As Boolean = True)

        Dim msg As MsgBoxStyle
        Dim theMes As String = "[" & GetSTDTime() & "] " & message.Replace(endl, " ")

        Select Case messageType
            Case AG3DBCMessageType.Alert
                Status.Items("Mes1").ForeColor = Color.Brown
                msg = MsgBoxStyle.Exclamation
            Case AG3DBCMessageType.Err
                Status.Items("Mes1").ForeColor = Color.Red
                msg = MsgBoxStyle.Critical
            Case AG3DBCMessageType.Success
                Status.Items("Mes1").ForeColor = Color.Blue
                msg = MsgBoxStyle.Information
            Case AG3DBCMessageType.Normal
                Status.Items("Mes1").ForeColor = Color.Black
                msg = MsgBoxStyle.Information
        End Select

        If Not showTimeSpan Then
            mesStartTime = Nothing
        End If

        If NotNothing(mesStartTime) Then
            'theMes += " (" & (Now - mesStartTime).Milliseconds & "s)"
        End If

        mesStartTime = Now()
        Status.Items("Mes1").Text = theMes
        Status.Items("Mes1").ToolTipText = theMes

        File.AppendAllText(ag3dbclogsPath & GetSTDToday() & ".txt", theMes & endl)

        If displayMsgbox Then
            MsgBox(message, msg)
        End If

    End Sub

    Public Sub Switch()

        For Each control As Control In switchControls

            If Not dontSwitchControls.Contains(control.Name) Then
                control.Enabled = Not control.Enabled
            End If

        Next

    End Sub

    Public Sub LoadXml()

        xmlSettings = NewXmlDocument(xmlFile, "settings")

        LoadXmlNode(xmlSettings, "/settings/login")
        LoadXmlNode(xmlSettings, "/settings/login", "remember", "0")
        LoadXmlNode(xmlSettings, "/settings/login/username")
        LoadXmlNode(xmlSettings, "/settings/login/password")
        LoadXmlNode(xmlSettings, "/settings/login/userId")
        LoadXmlNode(xmlSettings, "/settings/gameExe")
        LoadXmlNode(xmlSettings, "/settings/makeExe")
        LoadXmlNode(xmlSettings, "/settings/backendProcesses", , "1")
        LoadXmlNode(xmlSettings, "/settings/programPriority", , "2")
        LoadXmlNode(xmlSettings, "/settings/previewTime", , "3")
        LoadXmlNode(xmlSettings, "/settings/randomPreviews", , "0")
        LoadXmlNode(xmlSettings, "/settings/afterCharDownload", , "0")
        LoadXmlNode(xmlSettings, "/settings/ratings")
        LoadXmlNode(xmlSettings, "/settings/lastActivity", , "0")
        LoadXmlNode(xmlSettings, "/settings/lists")
        LoadXmlNode(xmlSettings, "/settings/dbCharsColors")
        LoadXmlNode(xmlSettings, "/settings/dbCharsColors/needPreviewsColor", , "-16776961")
        LoadXmlNode(xmlSettings, "/settings/dbCharsColors/notInCharsListColor", , "-16744448")
        LoadXmlNode(xmlSettings, "/settings/dbCharsColors/defaultColor", , "-16777216")
        LoadXmlNode(xmlSettings, "/settings/defaultTab", , "2")
        SaveXml()

    End Sub

    Public Sub SaveXml()
        xmlSettings.Save(xmlFile)
    End Sub

    Public Function CreateCharStructure(ByVal sChar As String, ByVal fullStructure As Boolean) As Boolean

        Try

            cd(ag3Chars)

            If Directory.Exists(ag3Chars & sChar) Then

                Dim zip7param As String = ""

                zip7param += "a -t7z -mx9 -r0 " & qq(GetCharUploadName(sChar)) & " -xr!*.pp "

                If fullStructure Then
                    zip7param += qq(sChar & "\" & sChar & ".js3csd") & " "
                    zip7param += qq(sChar & "\" & sChar & ".js3csi") & " "
                    zip7param += qq(sChar & "\" & sChar & "\*") & " "
                    StartProcess(ag3Decrypt, qq(sChar & "\" & sChar & ".pp"))
                End If

                zip7param += qq(sChar & ".js3cmi") & " "
                zip7param += qq(sChar & ".js3up") & " "
                zip7param += qq(sChar & "_v.bmp") & " "
                zip7param += qq(sChar & "\* - " & sChar & ".jpg") & " "

                RemoveFile(ag3Chars & sChar & ".7z")
                StartProcess(zip7exe, zip7param, backend, processPriority)
                RemoveDir(ag3Chars & sChar & "\" & sChar)

                Return True

            Else
                Return False
            End If

        Catch ex As Exception

            LogError(ex)
            RemoveFile(ag3Chars & sChar & ".7z")
            RemoveDir(ag3Chars & sChar & "\" & sChar)

            Return False

        End Try

    End Function

    Public Sub DeactivateClient()

        For Each tabPage As TabPage In Tab.TabPages

            If tabPage.Name <> "TabAbout" And tabPage.Name <> "TabHelp" Then
                Tab.TabPages.Remove(tabPage)
            End If

        Next

        Tab.SelectedTab = Tab.TabPages("TabAbout")
        CmdLogin.Enabled = True
        CmdLogout.Enabled = False
        TxtUsername.Text = ""
        TxtPassword.Text = ""
        TxtUsername.Enabled = True
        TxtPassword.Enabled = True
        ChkRemember.Enabled = True
        frm.Controls("LblRegister").Visible = True
        frm.Controls("LblLoggedInAs").Visible = False

        dontSwitchControls.Clear()
        clickedTabs.Clear()
        prevs.Clear()
        CboDefaultTab.Items.Clear()
        Mes("Not logged in")

    End Sub

    Public Sub ActivateClient()

        Tab.TabPages.Clear()

        For Each tabPage As TabPage In tabs
            Tab.TabPages.Add(tabPage)
            CboDefaultTab.Items.Add(tabPage.Text)
        Next

        CboDefaultTab.SelectedIndex = Val(GetSetting("defaultTab"))
        CmdLogin.Enabled = False
        CmdLogout.Enabled = True
        Tab.SelectedIndex = Val(GetSetting("defaultTab"))
        TxtUsername.Enabled = False
        TxtPassword.Enabled = False
        ChkRemember.Enabled = False
        frm.Controls("LblRegister").Visible = False
        frm.Controls("LblLoggedInAs").Visible = True

        dontSwitchControls.Add(TxtUsername, TxtUsername.Name)
        dontSwitchControls.Add(TxtPassword, TxtPassword.Name)
        dontSwitchControls.Add(frm.Controls("GrpLogin").Controls("CmdLogin"), "CmdLogin")
        dontSwitchControls.Add(frm.Controls("GrpLogin").Controls("ChkRemember"), "ChkRemember")

    End Sub

#Region "settaaaAAARRRRsss and gettaaaAAARRRRssss"

    Public Function GetLastActivity() As String

        Dim time As New Date(CLng(GetSetting("lastActivity")))

        Return time.Year & "-" & Format(time.Month, "00") & "-" & Format(time.Day, "00") & " @ " & Format(time.Hour, "00") & ":" & Format(time.Minute, "00") & Format(time.Second, "00")

    End Function

    Public Sub SetLastActivity()
        SetSetting("lastActivity", Now.Ticks)
        SaveXml()
    End Sub

    Public Function FTPConnect() As Utilities.FTP.FTPclient
        Return New Utilities.FTP.FTPclient(FTP_ADDR, FTP_USER, PwdUnHash(FTP_PWD))
    End Function

    Public Function GetDBPreviewPath(ByVal username As String) As String
        Return ag3Previews & username
    End Function

    Public Function GetDBPreviewPath(ByVal username As String, ByVal preview As String) As String
        Return ag3Previews & username & "\" & preview
    End Function

    Public Function GetDBPreviews(ByVal charNfo As CharacterNfo) As FileInfo()
        Return New DirectoryInfo(ag3Previews & charNfo.Username).GetFiles("* - " & charNfo.CharacterName & ".jpg")
    End Function

    Public Function GetPreviews(ByVal charName As String) As FileInfo()
        Return New DirectoryInfo(ag3Chars & charName).GetFiles("*.jpg")
    End Function

    Public Function GetPreviewPath(ByVal charName As String) As String
        Return ag3Chars & charName & "\"
    End Function

    Public Function GetCharUploadName(ByVal charName As String) As String
        Return charName & AG3DB_EXT
    End Function

    Public Function AuthenticateUser(ByVal username As String, ByVal password As String) As UserInfo

        Dim query As String = "authenticate.php?version=" & PwdHash(AG3DBC_VERSION) & "&username=" & username & "&password=" & PwdHash(password)
        Dim retVal As String = GetHttpResponseString(SERVER & query)

        If retVal = "5" Then
            Mes("Your version of AG3DBC is out of date. Please download the most recent version.", AG3DBCMessageType.Err, True)
            Process.Start(HF_AG3DBC_POST)
            End
        End If

        If Len(retVal) > 1 AndAlso retVal.Substring(0, 2) = "0|" Then

            Dim spl() As String = retVal.Split("|")
            Dim nfo As UserInfo

            nfo.userId = spl(1)
            nfo.username = spl(2)
            nfo.password = spl(3)

            Return nfo

        Else
            LogError(Nothing, "Server returned '" & retVal & "'")
        End If

        Return Nothing

    End Function

    Public Function CharacterExists(ByVal characterName As String) As Boolean

        Dim retVal As String = GetHttpResponseString(SERVER & "checkDB.php?type=characterName&value=" & characterName)

        Return retVal <> "0"

    End Function

    Public Sub LoadTabs()

        For Each tabPage As TabPage In Tab.TabPages
            tabs.Add(tabPage, tabPage.Name)
        Next

    End Sub

    Public Function GetUserId()
        Return GetSetting("login/userId")
    End Function

    Public Function GetUserPwd()
        Return GetSetting("login/password")
    End Function

    Public Function GetUsername()
        Return GetSetting("login/username")
    End Function

    Public Sub SetSetting(ByVal setting As String, ByVal value As String, Optional ByVal attribute As String = "")

        If NotNothing(xmlSettings) Then

            If Len(attribute) Then
                xmlSettings.SelectSingleNode("/settings/" & setting).Attributes(attribute).Value = value
            Else
                xmlSettings.SelectSingleNode("/settings/" & setting).InnerText = value
            End If

        End If

    End Sub

    Public Function GetSetting(ByVal setting As String, Optional ByVal attribute As String = "") As String

        If Len(attribute) Then
            Return xmlSettings.SelectSingleNode("/settings/" & setting).Attributes(attribute).Value
        Else
            Return XmlNodeText(xmlSettings.SelectSingleNode("/settings/" & setting))
        End If

    End Function

    Public Function GetSettingNode(ByVal setting As String) As XmlNode
        Return xmlSettings.SelectSingleNode("/settings/" & setting)
    End Function

#End Region

End Module