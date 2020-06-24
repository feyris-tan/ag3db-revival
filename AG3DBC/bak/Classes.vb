Imports System.Threading
Imports System.IO
Imports System.Xml
Imports Microsoft.Win32

#Region "dataTypes"

Public Enum AG3DBType
    Characters = 0
    Clothes = 1
End Enum

Public Enum AG3DBCMessageType
    Normal = 0
    Success = 1
    Err = 2
    Alert = 3
End Enum

Public Structure UserInfo
    Dim username As String
    Dim password As String
    Dim userId As String
End Structure

Public Enum AG3DBCThreadType
    NullFeed = 0
    GetDBCharsFeed = 1
    GetUserCharsFeed = 2
    GetUserRatingsFeed = 3
    GetUserStatsFeed = 4
    GetTopCharsFeed = 5
    GetTopUsersFeed = 6
    SearchTags = 7
    UploadChar = 8
    UpdateTags = 9
End Enum

Public Structure CharNfo
    Dim charId As String
    Dim charName As String
    Dim username As String
    Dim userId As String
    Dim hits As Integer
    Dim rating As Double
    Dim previews As Collection
    Dim size As Double
    Dim listItem As ListViewItem
    Dim tags As String
    Dim previewsSize As Double
    Dim previewsSizes As Collection
    Dim crc32 As String
    Dim listIndex As Integer
End Structure

Public Structure DBCharColor
    Dim NeedPreviewsColor As Color
    Dim NotInCharsListColor As Color
    Dim DefaultColor As Color
End Structure

#End Region

Public Class AG3DBCThreads

    Private _tags As New Tags
    Private _upload As New Upload
    Private _feeds As New Feeds
    Private _search As New Search
    Private _download As New Download
    Private _params As New ArrayList
    Private _thread As Thread
    Private _priority As Threading.ThreadPriority
    Private _threadType As AG3DBCThreadType

    Private Sub DoStart(ByVal func As Object)

        Select Case func
            Case AG3DBCThreadType.GetDBCharsFeed : _feeds.GetType.GetMethod(func.ToString).Invoke(_feeds, _params.ToArray)
            Case AG3DBCThreadType.GetUserRatingsFeed : _feeds.GetType.GetMethod(func.ToString).Invoke(_feeds, _params.ToArray)
            Case AG3DBCThreadType.GetUserCharsFeed : _feeds.GetType.GetMethod(func.ToString).Invoke(_feeds, _params.ToArray)
            Case AG3DBCThreadType.GetUserStatsFeed : _feeds.GetType.GetMethod(func.ToString).Invoke(_feeds, _params.ToArray)
            Case AG3DBCThreadType.GetTopCharsFeed : _feeds.GetType.GetMethod(func.ToString).Invoke(_feeds, _params.ToArray)
            Case AG3DBCThreadType.GetTopUsersFeed : _feeds.GetType.GetMethod(func.ToString).Invoke(_feeds, _params.ToArray)
            Case AG3DBCThreadType.SearchTags : _search.GetType.GetMethod(func.ToString).Invoke(_search, _params.ToArray)
            Case AG3DBCThreadType.UploadChar : _upload.GetType.GetMethod(func.ToString).Invoke(_upload, _params.ToArray)
            Case AG3DBCThreadType.UpdateTags : _tags.GetType.GetMethod(func.ToString).Invoke(_tags, _params.ToArray)
        End Select

        _threadType = AG3DBCThreadType.NullFeed

        _params.Clear()
        SetLastActivity()

    End Sub

    Public Sub Start(ByVal threadType As AG3DBCThreadType)

        _thread = New Thread(AddressOf DoStart)
        _thread.Priority = _priority
        _threadType = threadType

        _thread.Start(threadType)

    End Sub

    Public Sub AddParameter(ByVal parameter As Object)
        _params.Add(parameter)
    End Sub

    Public Sub ClearParameters()
        _params.Clear()
    End Sub

    Private Function MakeTypes(ByVal params As ArrayList) As Type()

        If _params.Count Then

            Dim types(_params.Count - 1) As Type

            For i As Integer = 0 To params.Count - 1
                types(i) = params(i).GetType
            Next

            Return types

        Else
            Return Nothing
        End If

    End Function

    Public Sub New()
        _threadType = AG3DBCThreadType.NullFeed
    End Sub

    Public ReadOnly Property IsRunning() As Boolean
        Get
            Return (NotNothing(_thread) AndAlso _thread.IsAlive)
        End Get
    End Property

    Public ReadOnly Property ThreadType() As AG3DBCThreadType
        Get
            Return _threadType
        End Get
    End Property

    Public Property ThreadPriority() As ThreadPriority
        Get
            Return _priority
        End Get
        Set(ByVal value As ThreadPriority)
            _priority = value
        End Set
    End Property

End Class

Public Class Feeds

    Public Sub GetDBCharsFeed(ByVal list As ListView, ByVal lblCharCount As Label, ByVal listChars As ListBox)

        list.Visible = False
        list.Sorting = SortOrder.None

        Mes("Refreshing AG3DB character list...")
        prevs.Clear()
        list.Items.Clear()
        Switch()

        Dim response() As String = GetFeed(AG3DBCThreadType.GetDBCharsFeed)
        Dim ok As Boolean = True

        If NotNothing(response) Then

            Dim xml As New XmlDocument

            Try

                If IsNothing(list.Tag) Then
                    list.Tag = New ListView()
                Else
                    list.Tag.items.clear()
                End If

                Mes("Populating AG3DB Characters list...")
                xml.LoadXml(response(1))

                For Each node As XmlNode In xml.SelectNodes("/chars/char")

                    Dim newItem As New ListViewItem(node.SelectSingleNode("date").InnerText.Trim)
                    Dim nfo As New CharacterNfo(node, list.Tag.Items.Count)

                    newItem.Tag = nfo

                    newItem.SubItems.Add(node.SelectSingleNode("charName").InnerText.Trim)
                    newItem.SubItems.Add(node.SelectSingleNode("username").InnerText.Trim)
                    newItem.SubItems.Add(Format(CInt(node.SelectSingleNode("hits").InnerText.Trim), "###,##0"))
                    newItem.SubItems.Add(ratings.GetRating(node.SelectSingleNode("rating").InnerText.Trim))
                    newItem.SubItems.Add(node.SelectSingleNode("tags").InnerText.Trim)

                    If InCharList(node.SelectSingleNode("charName").InnerText.Trim, node.SelectSingleNode("crc32").InnerText.Trim) Then
                        newItem.SubItems.Add("Yes")
                    Else
                        newItem.SubItems.Add("No")
                    End If

                    If Not NeedPreviews(nfo) Then
                        newItem.SubItems.Add("Yes")
                    Else
                        newItem.SubItems.Add("No")
                    End If

                    'If ratings.IsRated(node.SelectSingleNode("charId").InnerText.Trim) Then
                    '    newItem.SubItems.Add("Yes")
                    'Else
                    '    newItem.SubItems.Add("No")
                    'End If

                    newItem.ForeColor = GetDBCharColor(nfo)

                    list.Tag.Items.Add(newItem.Clone)

                Next

                CopyListView(list.Tag, list)

                lblCharCount.Text = "Characters: " & list.Items.Count

                Mes("Character list refresh successful!", AG3DBCMessageType.Success)

            Catch ex As Exception
                lblCharCount.Text = "Characters: n/a"
                list.Items.Clear()
                LogError(ex)
                Mes("Error populating AG3DB Characters list.", AG3DBCMessageType.Err, True)
            End Try

        End If

        Switch()
        list.Visible = True

    End Sub

    Public Sub GetTopUsersFeed(ByRef list As ListView)

        list.Visible = False

        Switch()
        Mes("Sending request to server...")
        list.Items.Clear()

        Dim response() As String = GetFeed(AG3DBCThreadType.GetTopUsersFeed)

        If NotNothing(response) Then

            Try

                If IsNothing(list.Tag) Then
                    list.Tag = New ListView()
                Else
                    list.Tag.items.clear()
                End If

                Dim xml As New XmlDocument

                Mes("Populating Top Users list...")
                xml.LoadXml(response(1))

                For Each node As XmlNode In xml.SelectNodes("/users/user")

                    Dim newItem As New ListViewItem(Val(list.Tag.Items.Count) + 1)

                    newItem.SubItems.Add(node.SelectSingleNode("username").InnerText.Trim)
                    newItem.SubItems.Add(ratings.GetRating(node.SelectSingleNode("rating").InnerText.Trim))
                    newItem.SubItems.Add(node.SelectSingleNode("ratings").InnerText.Trim)
                    newItem.SubItems.Add(Format(CInt(node.SelectSingleNode("chars").InnerText.Trim), "###,##0"))
                    newItem.SubItems.Add(Format(CInt(node.SelectSingleNode("hits").InnerText.Trim), "###,##0"))
                    newItem.SubItems.Add(Format(CInt(node.SelectSingleNode("ratings").InnerText.Trim), "###,##0"))
                    list.Tag.items.Add(newItem.Clone)

                Next

                CopyListView(list.Tag, list)
                Mes("Top characters list refreshed successfully!", AG3DBCMessageType.Success)

            Catch ex As Exception
                LogError(ex)
                Mes("Error try to populate top characters list.", AG3DBCMessageType.Err, True)
            End Try

        End If

        list.Visible = True

        Switch()

    End Sub

    Public Sub GetTopCharsFeed(ByVal list As ListView)

        list.Visible = False

        Switch()
        Mes("Sending request to server...")
        list.Items.Clear()

        Dim response() As String = GetFeed(AG3DBCThreadType.GetTopCharsFeed)

        If NotNothing(response) Then

            Dim xml As New XmlDocument

            Try

                If IsNothing(list.Tag) Then
                    list.Tag = New ListView()
                Else
                    list.Tag.items.clear()
                End If

                Mes("Populating Top Characters list...")
                xml.LoadXml(response(1))

                For Each node As XmlNode In xml.SelectNodes("/chars/char")

                    Dim newItem As New ListViewItem(Val(list.Tag.Items.Count) + 1)
                    Dim nfo As New CharacterNfo(node, list.Tag.Items.Count)

                    newItem.Tag = nfo

                    newItem.SubItems.Add(node.SelectSingleNode("charName").InnerText.Trim)
                    newItem.SubItems.Add(node.SelectSingleNode("username").InnerText.Trim)
                    newItem.SubItems.Add(ratings.GetRating(node.SelectSingleNode("rating").InnerText.Trim))
                    newItem.SubItems.Add(Format(CInt(node.SelectSingleNode("ratings").InnerText.Trim), "###,##0"))
                    newItem.SubItems.Add(Format(CInt(node.SelectSingleNode("hits").InnerText.Trim), "###,##0"))
                    list.Tag.Items.Add(newItem.Clone)

                Next

                CopyListView(list.Tag, list)
                Mes("Top characters list refreshed successfully!", AG3DBCMessageType.Success)

            Catch ex As Exception
                LogError(ex)
                Mes("Error try to populate top characters list.", AG3DBCMessageType.Err, True)
            End Try

        End If

        list.Visible = True

        Switch()

    End Sub

    Public Sub GetUserStatsFeed(ByVal listRatings As ListView, ByVal listChars As ListView, ByVal lblUserRating As Label, ByVal lblUserChars As Label)
        listRatings.Visible = False
        listChars.Visible = False
        GetUserCharsFeed(listChars, lblUserChars)
        GetUserRatingsFeed(listRatings, lblUserRating)
        listRatings.Visible = True
        listChars.Visible = True
    End Sub

    Public Sub GetUserCharsFeed(ByVal list As ListView, ByVal lblUserChars As Label)

        list.Visible = False

        Switch()
        Mes("Sending request to server...")
        list.Items.Clear()

        lblUserChars.Text = "Your characters: n/a"

        Dim response() As String = GetFeed(AG3DBCThreadType.GetUserCharsFeed)

        If NotNothing(response) Then

            Try

                If IsNothing(list.Tag) Then
                    list.Tag = New ListView()
                Else
                    list.Tag.items.clear()
                End If

                Dim xml As New XmlDocument

                Mes("Populating Your Characters list...")
                xml.LoadXml(response(1))

                For Each node As XmlNode In xml.SelectNodes("/chars/char")

                    Dim newItem As New ListViewItem(node.SelectSingleNode("date").InnerText.Trim)
                    Dim nfo As New CharacterNfo(node, list.Tag.Items.Count)

                    newItem.Tag = nfo

                    newItem.SubItems.Add(node.SelectSingleNode("charName").InnerText.Trim)
                    newItem.SubItems.Add(Format(CInt(node.SelectSingleNode("hits").InnerText.Trim), "###,##0"))

                    If node.SelectSingleNode("rating").InnerText.Trim = "0" Then
                        newItem.SubItems.Add(NO_RATING)
                    Else
                        newItem.SubItems.Add(ratings.GetRating(node.SelectSingleNode("rating").InnerText.Trim))
                    End If

                    list.Tag.Items.Add(newItem.Clone)

                Next

                CopyListView(list.Tag, list)
                Mes("User characters list refreshed successfully!", AG3DBCMessageType.Success)

                lblUserChars.Text = "Your characters: " & list.Tag.Items.Count

            Catch ex As Exception
                LogError(ex)
                Mes("Error try to populate user characters list.", AG3DBCMessageType.Err, True)
            End Try

        End If

        Switch()

        list.Visible = True

    End Sub

    Public Sub GetUserRatingsFeed(ByVal list As ListView, ByVal lblUserRating As Label)

        list.Visible = False

        Switch()
        Mes("Sending request to server...")
        list.Items.Clear()

        lblUserRating.Text = "Your rating score: n/a"

        Dim response() As String = GetFeed(AG3DBCThreadType.GetUserRatingsFeed)

        If NotNothing(response) Then

            Try

                If IsNothing(list.Tag) Then
                    list.Tag = New ListView()
                Else
                    list.Tag.items.clear()
                End If

                Dim xml As New XmlDocument

                Mes("Populating Your Ratings list...")
                xml.LoadXml(response(1))

                For Each node As XmlNode In xml.SelectNodes("/ratings/rating")

                    Dim newItem As New ListViewItem(node.SelectSingleNode("time").InnerText.Trim)
                    Dim nfo As New CharacterNfo(node, list.Tag.Items.Count)

                    newItem.Tag = nfo

                    newItem.SubItems.Add(node.SelectSingleNode("charName").InnerText.Trim)
                    newItem.SubItems.Add(node.SelectSingleNode("username").InnerText.Trim)
                    newItem.SubItems.Add(node.SelectSingleNode("rating").InnerText.Trim)
                    list.Tag.Items.Add(newItem.Clone)

                Next

                CopyListView(list.Tag, list)
                Mes("User ratings list refreshed successfully!", AG3DBCMessageType.Success)

                lblUserRating.Text = "Your rating score: " & Format(CDbl(xml.SelectSingleNode("/ratings/userRating").InnerText.Trim), "0.0") & " (" & list.Tag.Items.Count & " ratings)"

            Catch ex As Exception
                LogError(ex)
                Mes("Error try to populate user ratings list.", AG3DBCMessageType.Err, True)
            End Try

        End If

        Switch()

        list.Visible = True

    End Sub

    Public Function GetFeed(ByVal feed As AG3DBCThreadType) As String()

        Dim queryString As String = "feeds.php?"

        queryString += "userId=" & GetUserId()
        queryString += "&username=" & GetUsername()
        queryString += "&userPwd=" & GetUserPwd()

        Select Case feed
            Case AG3DBCThreadType.GetDBCharsFeed : queryString += "&type=dbChars"
            Case AG3DBCThreadType.GetUserRatingsFeed : queryString += "&type=userRatings"
            Case AG3DBCThreadType.GetUserCharsFeed : queryString += "&type=userChars"
            Case AG3DBCThreadType.GetTopCharsFeed : queryString += "&type=topChars"
            Case AG3DBCThreadType.GetTopUsersFeed : queryString += "&type=topUsers"
        End Select

        Dim retVal As String = GetHttpResponseString(SERVER & queryString)
        Dim ok As Boolean = True

        Select Case retVal
            Case "1"
                Mes("Error: Parameters not properly set.", AG3DBCMessageType.Err, True)
                ok = False
            Case "2"
                Mes("Error: Invalid feed type.", AG3DBCMessageType.Err, True)
                ok = False
            Case "3"
                Mes("Error: Invalid database login.", AG3DBCMessageType.Err, True)
                ok = False
        End Select

        If ok Then
            Return ParseRetVal(retVal)
        Else
            Return Nothing
        End If

    End Function

    Public Sub New()

    End Sub

End Class

Public Class Upload

    Public Sub UploadChar(ByVal charName As String, ByVal tags As String, ByVal fullCharStructure As Boolean, ByVal ftp As Utilities.FTP.FTPclient, ByVal watch As TransferWatch)

        Switch()

        If oTags.CheckTags(tags) Then

            Mes("Sending upload request to server...")

            Dim retVal As String = InitUpload(charName, tags)
            Dim ok As Boolean = True

            Select Case retVal
                Case "1"
                    Mes("Unable to log into the database." & endl & "Bad upload parameters.", AG3DBCMessageType.Err, True)
                    ok = False
                Case "2"
                    Mes("Unable to log into the database." & endl & "Bad username and/or password.", AG3DBCMessageType.Err, True)
                    ok = False
                Case "3"
                    Mes("Unable to upload character to the database." & endl & "You must wait at least 1 minute before uploading another character.", AG3DBCMessageType.Err, True)
                    ok = False
                Case "4"
                    Mes("Tags length too long.", AG3DBCMessageType.Err, True)
                    ok = False
                Case "5"
                    Mes("Unable to upload character to the database." & endl & "Character already exists.", AG3DBCMessageType.Err, True)
                    ok = False
            End Select

            If ok Then

                Mes("Request accepted!")
                Mes("Creating AG3DB formatted character file...")

                If CreateCharStructure(charName, fullCharStructure) Then
                    StartUpload(Split(retVal, "|"), charName, tags, ftp, watch) : Exit Sub
                Else
                    Mes("There was an error creating the AG3DB formatted character file.", AG3DBCMessageType.Err, True)
                End If

            End If

        End If

        Switch()

    End Sub

    Private Sub StartUpload(ByVal uploadInfo() As String, ByVal charName As String, ByVal tags As String, ByVal ftp As Utilities.FTP.FTPclient, ByVal watch As TransferWatch)

        Try
            ag3dbcReg.SetValue("curChar", charName)
            ag3dbcReg.SetValue("uploadId", uploadInfo(0))
            ag3dbcReg.SetValue("uploadPwd", uploadInfo(1))
        Catch ex As Exception
            Mes("Error parsing upload passkey.", AG3DBCMessageType.Alert, True)
            CancelUpload()
            Switch()
            Exit Sub
        End Try

        Dim oFile As FileInfo
        Dim bytesTotal As Double = 0
        Dim curNfo As New FileInfo(ag3Chars & GetCharUploadName(charName))

        If curNfo.Length > MAX_FILE_SIZE Then

            Mes("Filesize of character is too big. Must be less than " & FileSize(MAX_FILE_SIZE) & " (size is " & FileSize(curNfo.Length) & ").", AG3DBCMessageType.Err, True)
            Switch()

            Exit Sub

        End If

        ftp = FTPConnect()
        bytesTotal = curNfo.Length + New FileInfo(ag3Chars & charName & AG3_CHAR_EXT).Length

        For Each oFile In GetPreviews(charName)

            If oFile.Length > MAX_PREV_SIZE Then

                Mes("Preview " & oFile.Name & "'s file size (" & FileSize(oFile.Length) & ") is greater than the allowed " & FileSize(MAX_PREV_SIZE) & " per preview.", AG3DBCMessageType.Err, True)
                Switch()

                Exit Sub

            End If

            bytesTotal += oFile.Length

        Next

        watch.StartWatch(bytesTotal, threadPriority, ftp)
        Mes("Uploading " & charName & "...")

        Dim ok As Boolean = ftp.Upload(ag3Chars & GetCharUploadName(charName), FTP_DIR & "chars/" & GetUsername() & "/" & GetCharUploadName(charName))

        If ok Then

            ok = ftp.Upload(ag3Chars & charName & AG3_CHAR_EXT, FTP_DIR & "chars/" & GetUsername() & "/" & charName & AG3_CHAR_EXT)

            If ok Then

                For Each oFile In GetPreviews(charName)

                    Mes("Uploading Preview " & oFile.Name & "...")

                    ok = ftp.Upload(oFile.FullName, FTP_DIR & "chars/" & GetUsername() & "/" & Basename(oFile.FullName))

                    If Not ok Then
                        Mes("There was an error uploading preview " & oFile.Name & "." & endl & "Upload will now terminate", AG3DBCMessageType.Err, True)
                        CancelUpload()
                    End If

                Next

                watch.StopWatch()
                FinishUpload(charName, tags, bytesTotal)

            Else
                Mes("There was an error uploading the file.", AG3DBCMessageType.Err, True)
                CancelUpload()
            End If

        Else
            Mes("There was an error uploading the file.", AG3DBCMessageType.Err, True)
            CancelUpload()
        End If

        Switch()

    End Sub

    Private Sub FinishUpload(ByVal charName As String, ByVal tags As String, ByVal bytesTotal As Double)

        Dim loginNode As XmlNode = GetSettingNode("login")
        Dim queryString As String = ""
        Dim crc As New CRC32

        queryString += "name=" & charName
        queryString += "&userId=" & GetUserId()
        queryString += "&username=" & GetUsername()
        queryString += "&userPwd=" & GetUserPwd()
        queryString += "&uploadPwd=" & ag3dbcReg.GetValue("uploadPwd")
        queryString += "&uploadId=" & ag3dbcReg.GetValue("uploadId")
        queryString += "&size=" & bytesTotal
        queryString += "&crc32=" & Conversion.Hex(crc.GetCrc32(ag3Chars & RemoveExtention(charName) & AG3_CHAR_EXT)).ToUpper
        queryString += "&tags=" & tags

        Dim retVal As String = GetHttpResponseString(SERVER & "upload.php?" & queryString)
        Dim ok As Boolean = False

        Select Case retVal
            Case "0" : ok = True
            Case "1" : Mes("Bad parameters." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case "2" : Mes("Invalid upload session." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case "3" : Mes("User does not exist in the database." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case "4" : Mes("Character already exists in the database." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case "5" : Mes("Tags too long." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case "6" : Mes("Character file not found on server." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case Else : Mes("An unexpected error occured." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
        End Select

        RemoveFile(ag3Chars & charName & AG3DB_EXT)

        If ok Then
            Mes(charName & " uploaded successfully!", AG3DBCMessageType.Success, True)
        Else
            CancelUpload()
        End If

    End Sub

    Public Function InitUpload(ByVal charName As String, ByVal tags As String) As String

        Dim url As String = SERVER & "startUpload.php?name=" & charName
        Dim crc As New CRC32

        url += "&username=" & GetUsername()
        url += "&userId=" & GetUserId()
        url += "&userPwd=" & GetUserPwd()
        url += "&crc32=" & Conversion.Hex(crc.GetCrc32(ag3Chars & charName & AG3_CHAR_EXT)).ToUpper
        url += "&tags=" & tags

        Return GetHttpResponseString(url)

    End Function

    Public Sub New()

    End Sub
End Class

Public Class Download

    Public Sub FinishDownload(ByVal charId As String, ByVal item As ListViewItem, ByVal list As ListBox)

        Dim queryString As String = "downloadHit.php?"
        Dim nfo As CharacterNfo = item.Tag

        queryString += "id=" & nfo.CharacterId
        queryString += "&name=" & nfo.CharacterName
        queryString += "&userId=" & nfo.UserId
        queryString += "&type=chars"

        Dim retVal As String = GetHttpResponseString(SERVER & queryString)

        item.SubItems(3).Text = Format(nfo.Hits + 1, "###,###,###")

    End Sub

    Public Sub New()

    End Sub

End Class

Public Class Comments

    Public Sub New()
    End Sub
End Class

Public Class Rating

    Public Function IsRated(ByVal charId As String) As Boolean

        For Each node As XmlNode In GetSettingNode("ratings")

            If node.Attributes("charId").Value = charId Then
                Return True
            End If

        Next

        Return False

    End Function

    Public Function CanRate(ByVal charNfo As CharacterNfo) As Boolean

        If GetUserId() = charNfo.UserId Then
            Mes("You cannot rate your own character.", AG3DBCMessageType.Alert)
            Return False
        End If

        Dim node As XmlNode
        Dim curTime As Date = Now

        For Each node In GetSettingNode("ratings").ChildNodes

            If (Now - (New Date(CLng(node.Attributes("time").Value)))).TotalSeconds > RATE_WAIT_TIME Then
                RemoveXmlNode(node)
            End If

        Next

        SaveXml()

        For Each node In GetSettingNode("ratings").ChildNodes

            If node.Attributes("charId").Value = charNfo.CharacterId Then
                Mes("You must wait at least 1 week before you can rate " & charNfo.CharacterName & " again.", AG3DBCMessageType.Alert)
                Return False
            End If

        Next

        Return True

    End Function

    Public Sub RateChar(ByRef charNfo As CharacterNfo, ByVal rating As Integer, ByRef listItem As ListViewItem)

        Mes("Sending rating request to server...")

        Dim retVal As String = GetHttpResponseString(SERVER & "rate.php?characterId=" & charNfo.CharacterId & "&userId=" & GetUserId() & "&rating=" & rating & "&rate")
        Dim ok As Boolean = True

        Select Case retVal
            Case "1"
                Mes("Bad rating parameters.", AG3DBCMessageType.Err, True)
                ok = False
            Case "2"
                Mes("Invalid rating score.", AG3DBCMessageType.Err, True)
                ok = False
            Case "3"
                Mes("Character to rate does not exist.", AG3DBCMessageType.Err, True)
                ok = False
            Case "4"
                Mes("You are either the owner of the " & charNfo.CharacterName & " or you must wait at least 1 week before rating " & charNfo.CharacterName & " again.", AG3DBCMessageType.Err, True)
                SaveRating(charNfo.CharacterId)
                ok = False
        End Select

        If ok Then

            Dim spl() As String

            Try
                spl = Split(retVal, "|")

                If spl(0) = "0" Then

                    charNfo.Rating = GetRating(spl(1))
                    listItem.SubItems(4).Text = GetRating(charNfo.Rating)

                    SaveRating(charNfo.CharacterId)
                    ChangeFullListItem(listItem, listItem.ListView.Tag)
                    Mes("You gave " & charNfo.Username & "'s " & charNfo.CharacterName & " a score of " & rating & "! Its rating is now " & GetRating(spl(1)) & "!", AG3DBCMessageType.Success)

                Else
                    Mes("Error parsing server response.", AG3DBCMessageType.Err, True)
                    Exit Sub
                End If

            Catch ex As Exception
                LogError(ex)
                Mes("Error parsing server response.", AG3DBCMessageType.Err, True)
                Exit Sub
            End Try

        End If

    End Sub

    Public Sub SaveRating(ByVal charId As String)

        Dim newNode As XmlNode = NewXmlNode(xmlSettings, "rate")

        newNode.Attributes.Append(NewXmlAttribute(xmlSettings, "charId", charId))
        newNode.Attributes.Append(NewXmlAttribute(xmlSettings, "time", Now.Ticks))
        GetSettingNode("ratings").AppendChild(newNode)
        SaveXml()

    End Sub

    Public Function GetRating(ByVal rating As Object) As String

        Try

            rating = Val(rating).ToString("F2")

            If rating.startswith("0") Then
                Return NO_RATING
            Else
                Return rating
            End If

        Catch ex As Exception
            Return rating.ToString
        End Try

    End Function

End Class

Public Class Search

    Private _lastItem As ListViewItem

    Public Sub SearchTags(ByVal list As ListView, ByVal fullList As ListView, ByVal tags As String, ByVal matchAny As Boolean)

        If NotNothing(SelectedListItem(list)) Then
            _lastItem = SelectedListItem(list)
        End If

        list.Visible = False

        list.Items.Clear()

        If tags.Length Then

            Dim nfo As CharacterNfo
            Dim arrTags() As String = Split(tags, " ")
            Dim tag As String

            If matchAny Then

                For Each item As ListViewItem In fullList.Items

                    nfo = item.Tag

                    For Each tag In arrTags

                        If nfo.Tags.ToLower.Contains(tag) Then
                            list.Items.Add(item.Clone) : Exit For
                        End If

                    Next

                Next

            Else

                Dim found As Boolean

                For Each item As ListViewItem In fullList.Items

                    found = True
                    nfo = item.Tag

                    For Each tag In arrTags

                        If Not nfo.Tags.ToLower.Contains(tag) Then
                            found = False : Exit For
                        End If

                    Next

                    If found Then
                        list.Items.Add(item.Clone)
                    End If

                Next

            End If

        Else

            CopyListView(fullList, list)

            If NotNothing(_lastItem) Then
                list.Items(_lastItem.Tag.listindex).selected = True
                list.Items(_lastItem.Tag.listindex).EnsureVisible()
            End If

        End If

        list.Visible = True

    End Sub

    Public Sub New()
    End Sub
End Class

Public Class Tags

    Public Sub UpdateTags(ByVal charName As String, ByVal tags As String, ByVal list As ListView)

        Switch()

        If CheckTags(tags) Then

            Dim queryString As String = "tags.php?action=update"
            Dim crc As New CRC32

            queryString += "&name=" & charName
            queryString += "&userId=" & GetUserId()
            queryString += "&username=" & GetUsername()
            queryString += "&userPwd=" & GetUserPwd()
            queryString += "&tags=" & tags.Replace(" ", "%20")
            queryString += "&crc32=" & Conversion.Hex(crc.GetCrc32(ag3Chars & charName & AG3_CHAR_EXT)).ToUpper

            Mes("Attempting to update tags for " & charName & "...")

            Dim retVal As String = GetHttpResponseString(SERVER & queryString)

            Select Case retVal
                Case "0"
                    SetDBTag(list, charName, tags)
                    Mes("Tags updated successfully!", AG3DBCMessageType.Success, True)
                Case "1" : Mes("Parameters not set.", AG3DBCMessageType.Err, True)
                Case "2" : Mes("Invalid action.", AG3DBCMessageType.Err, True)
                Case "3" : Mes("Incorrect login.", AG3DBCMessageType.Err, True)
                Case "4" : Mes("You are not the owner of " & charName & " or it does not exist in the database.", AG3DBCMessageType.Err, True)
                Case "5" : Mes("Tags length too long.", AG3DBCMessageType.Err, True)
                Case Else : Mes("Unexpected server response.", AG3DBCMessageType.Err, True)
            End Select

        End If

        Switch()

    End Sub

    Public Function CheckTags(ByVal tags As String) As Boolean

        Try

            Dim spl() As String = Split(tags, " ")

            If spl.Length <= 10 Then

                For Each tag As String In spl

                    If tag.Length <= 16 Then

                        For Each c As Char In tag

                            If Not (c >= "0" AndAlso c <= "9") AndAlso Not (c >= "A" AndAlso c <= "z") AndAlso c <> "_" Then
                                Mes("Invalid character " & c & " found in " & tag, AG3DBCMessageType.Err, True) : Return False
                            End If

                        Next

                    Else
                        Mes("Tag '" & tag & " is over 16 characters.", AG3DBCMessageType.Err, True) : Return False
                    End If

                Next

                Return True

            Else
                Mes("Too many tags. The maximum is 10 tags.", AG3DBCMessageType.Err, True) : Return False
            End If

        Catch ex As Exception
            LogError(ex)
            Return False
        End Try

    End Function

    Public Sub SetDBTag(ByVal list As ListView, ByVal charName As String, ByVal tags As String)

        For Each item As ListViewItem In list.Items

            If item.SubItems(1).Text.ToLower = charName.ToLower Then
                item.SubItems(5).Text = tags : Exit Sub
                item.Tag.tags = tags
            End If

        Next

    End Sub

    Public Function GetDBTag(ByVal list As ListView, ByVal charName As String) As String

        For Each item As ListViewItem In list.Items

            If item.SubItems(1).Text.ToLower = charName.ToLower Then
                Return item.SubItems(5).Text
            End If

        Next

        Return ""

    End Function

    Public Sub New()
    End Sub
End Class

Public Class TransferWatch

    Private _lblTrans As ToolStripLabel
    Private _lblPercent As ToolStripLabel
    Private _pb As System.Windows.Forms.ToolStripProgressBar
    Private _ftp As Utilities.FTP.FTPclient
    Private _bytesTotal As Double
    Private _thread As Thread

    Public Sub New(ByVal progressBar As ToolStripProgressBar, ByVal lblTrans As ToolStripLabel, ByVal lblPercent As ToolStripLabel)

        _pb = progressBar
        _lblTrans = lblTrans
        _lblPercent = lblPercent

        ResetDisplay()

    End Sub

    Public Sub StartWatch(ByVal bytesTotal As Double, ByVal priority As ThreadPriority, ByVal ftp As Utilities.FTP.FTPclient)
        _thread = New Thread(AddressOf Watch)
        _ftp = ftp
        _thread.Priority = priority
        _thread.Start(bytesTotal)
    End Sub

    Public Sub StopWatch()

        If NotNothing(_thread) Then
            _thread.Abort()
            _thread = Nothing
        End If

        ResetDisplay()

    End Sub

    Public Sub FreezeWatch()

        If NotNothing(_thread) Then
            _thread.Abort()
            _thread = Nothing
        End If

    End Sub

    Public Sub ResetDisplay()
        _lblTrans.Text = ""
        _lblPercent.Text = "Idle"
        _pb.Value = 0
    End Sub

    Private Sub Watch(ByVal bytesTotal As Object)

        Dim startTime As Date = Now

        _pb.Maximum = bytesTotal

        While True

            _lblTrans.Text = FileSize(_ftp.GlobalBytesTransfered, , "", "") & " / " & FileSize(bytesTotal, , "", "") & " @ " & FileSize(_ftp.TransferRate, FileSizes.Kilobytes, "", "") & "/s"
            _lblPercent.Text = Percent(_ftp.GlobalBytesTransfered, bytesTotal, "0%")
            _pb.Value = _ftp.GlobalBytesTransfered

            Thread.Sleep(50)

        End While

    End Sub

End Class

Public Class Previews

    Private _index As Integer = 0
    Private _pics As New Collection
    Private _rnd As Boolean = False
    Private _picTime As Integer = 3
    Private _thread As Thread = Nothing
    Private _openPics As Boolean = True
    Private _filesizes As New Collection
    Private WithEvents _picBox As PictureBox
    Private WithEvents _picFrame As Control

    Private Sub OpenPic(ByVal sender As Object, ByVal e As EventArgs) Handles _picBox.DoubleClick, _picFrame.DoubleClick

        If _openPics AndAlso _pics.Count AndAlso _index > 0 AndAlso _index <= _pics.Count Then

            If File.Exists(_pics(_index)) Then
                Process.Start(_pics(_index))
            End If

        End If

    End Sub

    Public Sub New(ByVal picBox As PictureBox, ByVal picFrame As Control)
        _picBox = picBox
        _picFrame = picFrame
    End Sub

    Public Sub Add(ByVal picFile As String, Optional ByVal key As String = "")

        If File.Exists(picFile) Then

            If Len(key) Then
                _pics.Add(picFile, key)
                _filesizes.Add(New FileInfo(picFile).Length, key)
            Else
                _pics.Add(picFile, picFile)
                _filesizes.Add(New FileInfo(picFile).Length, picFile)
            End If

        End If

    End Sub

    Public Sub Remove(ByVal picFile As String)
        _pics.Remove(picFile)
        _filesizes.Remove(picFile)
    End Sub

    Public Sub Remove(ByVal index As Integer)
        _pics.Remove(index)
        _filesizes.Remove(index)
    End Sub

    Public Sub Clear()
        _pics.Clear()
        _filesizes.Clear()
        Pause()
        _picBox.Image = Nothing
    End Sub

    Public Sub StartPics()

        If _pics.Count Then

            If _rnd Then
                _index = rnd.Next(1, _pics.Count)
            Else
                _index = 1
            End If

            LoadPic(_picBox, _picFrame, _pics(_index))

            If _pics.Count > 1 Then
                _thread = New Thread(AddressOf Inc)
                _thread.Start()
            End If

        Else
            _picBox.Image = Nothing
        End If

    End Sub

    Public Sub Pause()

        If NotNothing(_thread) Then
            _thread.Abort()
            _thread = Nothing
        End If

    End Sub

    Public Sub Restart()
        Pause()
        StartPics()
    End Sub

    Private Sub Inc()

        Thread.Sleep(_picTime * 1000)

        While True

            If _rnd Then
                _index = rnd.Next(1, _pics.Count)
            Else

                If _index >= _pics.Count Then
                    _index = 1
                Else
                    _index += 1
                End If

            End If

            LoadPic(_picBox, _picFrame, _pics(_index))
            Thread.Sleep(_picTime * 1000)

        End While

    End Sub

    Public Sub ShowPic(ByVal picFile As String)
        If File.Exists(picFile) Then
            Pause()
            LoadPic(_picBox, _picFrame, picFile)
        End If
    End Sub

    Public Property Random() As Boolean
        Get
            Return _rnd
        End Get
        Set(ByVal value As Boolean)
            _rnd = value
        End Set
    End Property

    Public ReadOnly Property Pics() As Collection
        Get
            Return _pics
        End Get
    End Property

    Public ReadOnly Property Running() As Boolean
        Get
            If NotNothing(_thread) Then
                Return _thread.IsAlive
            End If
        End Get
    End Property

    Public ReadOnly Property Count() As Integer
        Get
            Return _pics.Count
        End Get
    End Property

    Public Property PicTime() As Integer
        Get
            Return _picTime
        End Get
        Set(ByVal value As Integer)
            _picTime = value
        End Set
    End Property

    Public Property CanOpenPics() As Boolean
        Get
            Return _openPics
        End Get
        Set(ByVal value As Boolean)
            _openPics = value
        End Set
    End Property

    Public ReadOnly Property Filesizes() As Double

        Get

            Dim size As Double = 0

            For Each pic As Double In _filesizes
                size += pic
            Next

            Return size

        End Get

    End Property

End Class

Public Class CharacterNfo

    Private _nfo As CharNfo

    Public Sub New(ByVal node As XmlNode, ByVal listIndex As Integer)

        _nfo.charId = XmlNodeText(node.SelectSingleNode("id"))
        _nfo.charName = XmlNodeText(node.SelectSingleNode("charName"))
        _nfo.hits = Val(XmlNodeText(node.SelectSingleNode("hits")))
        _nfo.rating = Val(XmlNodeText(node.SelectSingleNode("rating")))
        _nfo.userId = XmlNodeText(node.SelectSingleNode("userId"))
        _nfo.username = XmlNodeText(node.SelectSingleNode("username"))
        _nfo.size = Val(XmlNodeText(node.SelectSingleNode("size")))
        _nfo.tags = XmlNodeText(node.SelectSingleNode("tags"))
        _nfo.previewsSize = Val(XmlNodeText(node.SelectSingleNode("previewsSize")))
        _nfo.crc32 = XmlNodeText(node.SelectSingleNode("crc32"))
        _nfo.listIndex = listIndex
        _nfo.previews = New Collection
        '_nfo.previewsSizes = New Collection

        For Each prev As XmlNode In node.SelectNodes("preview")
            _nfo.previews.Add(prev.InnerText.Trim, prev.InnerText.Trim)
            '_nfo.previewsSizes.Add(Val(prev.Attributes("size").Value), prev.InnerText.Trim)
        Next

    End Sub

    Public Property CharacterId() As String
        Get
            Return _nfo.charId
        End Get
        Set(ByVal value As String)
            _nfo.charId = value
        End Set
    End Property

    Public Property CharacterName() As String
        Get
            Return _nfo.charName
        End Get
        Set(ByVal value As String)
            _nfo.charName = value
        End Set
    End Property

    Public Property UserId() As String
        Get
            Return _nfo.userId
        End Get
        Set(ByVal value As String)
            _nfo.userId = value
        End Set
    End Property

    Public Property Username() As String
        Get
            Return _nfo.username
        End Get
        Set(ByVal value As String)
            _nfo.username = value
        End Set
    End Property

    Public Property Hits() As Integer
        Get
            Return _nfo.hits
        End Get
        Set(ByVal value As Integer)
            _nfo.hits = value
        End Set
    End Property

    Public Property Rating() As Double
        Get
            Return _nfo.rating
        End Get
        Set(ByVal value As Double)
            _nfo.rating = value
        End Set
    End Property

    Public ReadOnly Property Previews() As Collection
        Get
            Return _nfo.previews
        End Get
    End Property

    Public ReadOnly Property Preview(ByVal index As Integer) As String
        Get
            Return _nfo.previews(index)
        End Get
    End Property

    Public ReadOnly Property Preview(ByVal name As String) As String
        Get
            Return _nfo.previews(name)
        End Get
    End Property

    Public ReadOnly Property Size() As Double
        Get
            Return _nfo.size
        End Get
    End Property

    Public ReadOnly Property Tags() As String
        Get
            Return _nfo.tags
        End Get
    End Property

    Public ReadOnly Property PreviewsSize() As Double
        Get
            Return _nfo.previewsSize
        End Get
    End Property

    Public ReadOnly Property CRC32() As String
        Get
            Return _nfo.crc32
        End Get
    End Property

    Public ReadOnly Property ListIndex() As Integer
        Get
            Return _nfo.listIndex
        End Get
    End Property

End Class