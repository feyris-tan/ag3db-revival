Imports System.Threading
Imports System.IO
Imports System.Xml
Imports System.Text.RegularExpressions
Imports Microsoft.Win32
'Imports codeparser.net

#Region "dataTypes"

Public Enum AG3DBType
    Character = 0
    Cloth = 1
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

Public Enum AG3DBDetailType
    Tag = 0
    Description = 1
End Enum

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
    Private _invokeObj As Object



    Private Sub DoStart(ByVal func As Object)

        If IsNothing(_invokeObj) Then

            Select Case func
                Case AG3DBCThreadType.GetDBCharsFeed Or _
                AG3DBCThreadType.GetUserRatingsFeed Or _
                AG3DBCThreadType.GetUserCharsFeed Or _
                AG3DBCThreadType.GetUserStatsFeed Or _
                AG3DBCThreadType.GetTopCharsFeed Or _
                AG3DBCThreadType.GetTopUsersFeed Or _
                AG3DBCThreadType.GetSets
                    _invokeObj = _feeds
                Case AG3DBCThreadType.SearchTags
                    _invokeObj = _search
                Case AG3DBCThreadType.UploadChar
                    _invokeObj = _upload
                Case AG3DBCThreadType.UpdateTags
                    _invokeObj = _tags
            End Select

        End If

        _invokeObj.GetType.GetMethod(func.ToString).Invoke(_invokeObj, _params.ToArray)

        _threadType = AG3DBCThreadType.NullFeed
        _invokeObj = Nothing

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
        _invokeObj = Nothing
    End Sub

    Public Property InvokeObject() As Object
        Get
            Return _invokeObj
        End Get
        Set(ByVal value As Object)
            _invokeObj = value
        End Set
    End Property

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

Public Class Upload

    Public Sub UploadChar(ByVal charName As String, ByVal tags As String, ByVal desc As String, ByVal fullCharStructure As Boolean, ByVal ftp As Utilities.FTP.FTPclient, ByVal watch As TransferWatch)

        Switch()

        If oDetails.CheckDetails(desc, tags) Then

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
                    StartUpload(Split(retVal, "|"), charName, tags, desc, ftp, watch) : Exit Sub
                Else
                    Mes("There was an error creating the AG3DB formatted character file.", AG3DBCMessageType.Err, True)
                End If

            End If

        End If

        Switch()

    End Sub

    Private Sub StartUpload(ByVal uploadInfo() As String, ByVal charName As String, ByVal tags As String, ByVal desc As String, ByVal ftp As Utilities.FTP.FTPclient, ByVal watch As TransferWatch)

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
                FinishUpload(charName, tags, desc, bytesTotal)

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

    Private Sub FinishUpload(ByVal charName As String, ByVal tags As String, ByVal desc As String, ByVal bytesTotal As Double)

        Dim up As New WebFormPost(AG3DBC.API.ServerUrl & "upload2.php")
        Dim ok As Boolean = False

        up.AddFormElement("name", charName)
        up.AddFormElement("typeId", "0")
        up.AddFormElement("userId", GetUserId())
        up.AddFormElement("username", GetUsername())
        up.AddFormElement("userPwd", GetUserPwd())
        up.AddFormElement("uploadPwd", ag3dbcReg.GetValue("uploadPwd"))
        up.AddFormElement("uploadId", ag3dbcReg.GetValue("uploadId"))
        up.AddFormElement("size", bytesTotal)
        up.AddFormElement("crc32", Conversion.Hex(New CRC32().GetCrc32(ag3Chars & RemoveExtention(charName) & AG3_CHAR_EXT)).ToUpper)
        up.AddFormElement("tags", tags)
        up.AddFormElement("desc", desc)
        up.Submit()

        Select Case up.Response
            Case "0" : ok = True
            Case "1" : Mes("Bad parameters." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case "2" : Mes("Invalid upload session." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case "3" : Mes("User does not exist in the database." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case "4" : Mes("Character already exists in the database." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case "5" : Mes("Tags too long." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case "6" : Mes("Character file not found on server." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
            Case Else
                LogError(, "Server response: '" & up.Response)
                Mes("An unexpected error occured." & endl & "Upload transaction cancelled.", AG3DBCMessageType.Err, True)
        End Select

        RemoveFile(ag3Chars & charName & AG3DB_EXT)

        If ok Then
            Mes(charName & " uploaded successfully!", AG3DBCMessageType.Success, True)
        Else
            CancelUpload()
        End If

    End Sub

    Public Function InitUpload(ByVal charName As String, ByVal tags As String) As String

        Dim url As String = AG3DBC.API.ServerUrl & "startUpload.php?name=" & charName
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

    Public Sub GetPreviews(ByVal nfo As CharacterNfo)

        If NotNothing(Selected(nfo.List)) Then

            Dim item As ListViewItem = Selected(nfo.List)
            Dim count As Integer = 0

            Try

                Switch()

                ftp = FTPConnect()

                Directory.CreateDirectory(GetDBPreviewPath(nfo.Username))
                prevs.Clear()
                watch.StartWatch(nfo.PreviewsSize, threadPriority, ftp)

                For Each preview As String In nfo.Previews

                    count += 1

                    If Not File.Exists(GetDBPreviewPath(nfo.Username, preview)) Then
                        Mes("Getting preview " & preview & " (" & count & "/" & nfo.Previews.Count & ")...")
                        ftp.Download(FTP_DIR & "chars/" & nfo.Username & "/" & preview, GetDBPreviewPath(nfo.Username, preview), True)
                        prevs.Add(GetDBPreviewPath(nfo.Username, preview))
                    End If

                Next

                watch.StopWatch()
                prevs.StartPics()
                Mes("Successfully downloaded all previews for " & nfo.CharacterName & "!", AG3DBCMessageType.Success)

                item.ForeColor = GetDBCharColor(nfo)
                item.SubItems(7).Text = "Yes"
                ChangeFullListItem(item, nfo.List.Tag)

            Catch ex As Exception
                LogError(ex)
                watch.StopWatch()
                Mes("There was an error while trying to download the previews for " & nfo.CharacterName & ".", AG3DBCMessageType.Err, True)
            End Try

            Switch()
            nfo.List.Select()
            nfo.List.Focus()
            nfo.DoDBPreviews()

        End If

    End Sub

    Public Sub FinishDownload(ByVal charId As String, ByVal item As ListViewItem, ByVal list As ListView)

        Dim queryString As String = "downloadHit.php?"
        Dim nfo As CharacterNfo = item.Tag

        queryString += "id=" & nfo.CharacterId
        queryString += "&name=" & nfo.CharacterName
        queryString += "&userId=" & nfo.UserId
        queryString += "&type=chars"

        Dim retVal As String = GetHttpResponseString(AG3DBC.API.ServerUrl & queryString)

        item.SubItems(3).Text = Format(nfo.Hits + 1, "###,###,###")

    End Sub

    Public Sub New()
    End Sub
End Class

Public Class Details

    Public Sub UpdateDetails(ByVal nfo As CharacterNfo)

        If CheckDetails(nfo.Description, nfo.Tags) Then

            Dim wb As New WebFormPost(AG3DBC.API.ServerUrl & "details.php")

            wb.AddFormElement("username", GetUsername())
            wb.AddFormElement("userId", GetUserId())
            wb.AddFormElement("userPwd", GetUserPwd())
            wb.AddFormElement("name", nfo.CharacterName)
            wb.AddFormElement("crc32", nfo.CRC32)
            wb.AddFormElement("desc", nfo.Description)
            wb.AddFormElement("tags", nfo.Tags)
            wb.AddFormElement("action", "update")
            wb.Submit()

            Select Case wb.Response
                Case "0" : Mes("Details for " & nfo.CharacterName & " update successfully!", AG3DBCMessageType.Success, True)
                Case "1" : Mes("Bad parameters.", AG3DBCMessageType.Err, True)
                Case "2" : Mes("Incorrect login.", AG3DBCMessageType.Err, True)
                Case "3" : Mes("You are not the owner of " & nfo.CharacterName & ".", AG3DBCMessageType.Err, True)
                Case "4" : Mes("Inproper tag format", AG3DBCMessageType.Err, True)
                Case "5" : Mes("Invalid action", AG3DBCMessageType.Err, True)
                Case Else
                    LogError(Nothing, wb.Response)
                    Mes("An unexpected error occured. Please check the error log", AG3DBCMessageType.Err, True)
            End Select

        End If

    End Sub

    Public Function CheckDetails(ByVal desc As String, ByVal tags As String) As Boolean

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

                If desc.Length > MAX_COMMENTS_SIZE Then
                    Mes("Comments are longer then the maximum allowed of " & MAX_COMMENTS_SIZE & " characters.", AG3DBCMessageType.Err, True) : Return False
                End If

                Return True

            Else
                Mes("Too many tags. The maximum is 10 tags.", AG3DBCMessageType.Err, True) : Return False
            End If

        Catch ex As Exception
            LogError(ex)
            Return False
        End Try

    End Function

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

    Public Function CanRate(ByVal nfo As CharacterNfo, ByVal item As ListViewItem) As Boolean

        If GetUserId() = nfo.UserId Then
            Mes("You cannot rate your own character.", AG3DBCMessageType.Alert)
            Return False
        End If

        If item.SubItems(9).Text = "Yes" Then
            Mes("You have already rated " & nfo.CharacterName & ".", AG3DBCMessageType.Alert)
            Return False
        End If

        Return True

    End Function

    Public Sub Rate(ByRef charNfo As CharacterNfo, ByVal rating As Integer, ByRef listItem As ListViewItem)

        Mes("Sending rating request to server...")

        Dim retVal As String = GetHttpResponseString(AG3DBC.API.ServerUrl & "rate.php?characterId=" & charNfo.CharacterId & "&userId=" & GetUserId() & "&rating=" & rating & "&rate")
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
                Mes("You are either the owner of the " & charNfo.CharacterName & " or you have already rated " & charNfo.CharacterName & ".", AG3DBCMessageType.Err, True)
                SaveRating(charNfo.CharacterId)
                ok = False
                listItem.SubItems(9).Text = "Yes"
                ChangeFullListItem(listItem, listItem.ListView.Tag)
            Case ""
                Mes("No response from server.", AG3DBCMessageType.Err, True)
                ok = False
        End Select

        If ok Then

            Dim spl() As String

            Try
                spl = Split(retVal, "|")

                If spl(0) = "0" Then

                    charNfo.Rating = GetRating(spl(1))
                    listItem.SubItems(4).Text = GetRating(charNfo.Rating)
                    listItem.SubItems(9).Text = "Yes"

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

    'Public Sub UpdateTags(ByVal charName As String, ByVal tags As String, ByVal list As ListView)

    '    Switch()

    '    If CheckTags(tags) Then

    '        Dim queryString As String = "tags.php?action=update"
    '        Dim crc As New CRC32

    '        queryString += "&name=" & charName
    '        queryString += "&userId=" & GetUserId()
    '        queryString += "&username=" & GetUsername()
    '        queryString += "&userPwd=" & GetUserPwd()
    '        queryString += "&tags=" & tags.Replace(" ", "%20")
    '        queryString += "&crc32=" & Conversion.Hex(crc.GetCrc32(ag3Chars & charName & AG3_CHAR_EXT)).ToUpper

    '        Mes("Attempting to update tags for " & charName & "...")

    '        Dim retVal As String = GetHttpResponseString(SERVER & queryString)

    '        Select Case retVal
    '            Case "0"
    '                SetDBTag(list, charName, tags)
    '                Mes("Tags updated successfully!", AG3DBCMessageType.Success, True)
    '            Case "1" : Mes("Parameters not set.", AG3DBCMessageType.Err, True)
    '            Case "2" : Mes("Invalid action.", AG3DBCMessageType.Err, True)
    '            Case "3" : Mes("Incorrect login.", AG3DBCMessageType.Err, True)
    '            Case "4" : Mes("You are not the owner of " & charName & " or it does not exist in the database.", AG3DBCMessageType.Err, True)
    '            Case "5" : Mes("Tags length too long.", AG3DBCMessageType.Err, True)
    '            Case Else : Mes("Unexpected server response.", AG3DBCMessageType.Err, True)
    '        End Select

    '    End If

    '    Switch()

    'End Sub



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
    Private myRnd As Random

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
        Me.myRnd = New Random()
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
                _index = myRnd.Next(1, _pics.Count)
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
                _index = myRnd.Next(1, _pics.Count)
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

'Public Class phpBBCode

'    Private _phpbbCode As String
'    Private _cssFile As String
'    Private _noBRTags() As String = {"list"}
'    Private _tags() As String = {"url", "email", "img", "list", "*", "size", "color", "center", "left", "right", "indent", "strike", "noparse", "spoiler"}

'    Public Function Decode2Html() As String

'        Dim bbCodeConfig As New codeparser.net.ParserConfiguration()
'        Dim html As String = _phpbbCode

'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("url", "<a href={1} class='url' target='_blank'>{0}</a>", "<a href={0} class='url' target='_blank'>{0}</a>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("email", "<a href='mailto:{1}' class='email' target='_blank'>{0}</a>", "<a href='mailto:{0}' class='email' target='_blank'>{0}</a>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("img", "<img class='img' src='{0}' />"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("list", "<ul class='list'>{0}</ul>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("*", "<li>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("size", "<font class='size' size={1}>{0}</font>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("color", "<font class='color' color={1}>{0}</font>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("center", "<div class='center'>{0}</div>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("left", "<div class='left'>{0}</div>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("right", "<div class='right'>{0}</div>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("indent", "<blockquote class='indent'><div>{0}</div></blockquote>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("strike", "<strike class='strike'>{0}</strike>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("noparse", "{0}", False))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("i", "<i class='i'>{0}</i>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("b", "<b class='b'>{0}</b>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("u", "<u class='u'>{0}</u>"))
'        bbCodeConfig.TagConfigurations.Add(New _
'            TagConfiguration("spoiler", _
'                "<div class='spoiler'>" & _
'                "   <div style='display:none'>" & _
'                "   {0}<br></div><input type='button' class='button' value='Show Spoiler' onclick='changeSpoilerDisplay(this)'></div>"))

'        bbCodeConfig.ThrowExceptionOnInvalidTag = False

'        Try

'            html = html.Replace("<", "&lt;")
'            html = html.Replace(">", "&gt;")
'            html = Regex.Replace(html, "(\s)(http:\/\/|ftp:\/\/)([^\s,]+)(\s)", "$1[url]$2$3[/url]$4")
'            html = Regex.Replace(html, "(\s)([A-z0-9._%-]+@[A-z0-9.-]+\.[A-z]{2,4})(\s)", "$1[email]$2[/email]$3", RegexOptions.Multiline)
'            html = DoBRs(html)
'            html = New codeparser.net.Parser(bbCodeConfig).Parse(html)

'            Return ShowHtml(html)

'        Catch ex As Exception
'            Return "ERROR"
'        End Try

'    End Function

'    Private Function DoBRs(ByVal bbCode As String) As String

'        Dim newBBCode As String = ""
'        Dim tag As String
'        Dim br As Boolean

'        For Each line As String In Split(bbCode, endl)

'            br = True

'            For Each tag In _noBRTags

'                If line.EndsWith("[" & tag & "]") Or line.EndsWith("[/" & tag & "]") Then
'                    br = False : Exit For
'                End If

'            Next

'            If br Then
'                newBBCode += line & "<br />" & endl
'            Else
'                newBBCode += line & endl
'            End If

'        Next

'        Return newBBCode.Substring(0, Len(newBBCode) - Len(endl))

'    End Function

'    Private Function ShowHtml(ByVal html As String) As String

'        Return "<html><head><title>Description</title>" & _
'               "<script type='text/javascript'>" & endl & File.ReadAllText(ag3dbcToolsPath & "bbcode/javascript.js") & endl & "</script>" & _
'               "<style>" & endl & File.ReadAllText(ag3dbcToolsPath & "/bbcode/styles.css") & endl & "</style>" & _
'               "</head><body>" & endl & endl & html & endl & endl & "</body></html>"

'    End Function

'    Public Function ParseLists(ByVal phpBBCode As String) As String

'        If phpBBCode.Contains("[list") Then
'            phpBBCode = Regex.Replace(phpBBCode, "\[list\=[A-z]\](.*)\[/list\]", "<ol style='list-style-type: lower-alpha'>$1</ol>")
'            phpBBCode = Regex.Replace(phpBBCode, "\[list\=[0-9]\](.*)\[/list\]", "<ol>$1</ol>")
'            phpBBCode = Regex.Replace(phpBBCode, "/\[list\](.+?)\[\/list\]/is", "<ul>$1</ul>")
'            phpBBCode = phpBBCode.Replace("[*]", "<li>")
'        End If

'        Return phpBBCode

'    End Function

'    Public Sub New(ByVal phpBBcode As String)
'        _phpbbCode = phpBBcode
'    End Sub

'End Class

Public Class DescForm

    Private DEFAULT_SIZE As New Size(300, 300)
    Private MARGIN As New Size(10, 10)
    Private MIN_SIZE As New Size(MARGIN.Width + 5, MARGIN.Height + 5)

    Private _frm As Form = Nothing
    Private _web As WebBrowser = Nothing
    Private _nfo As CharacterNfo = Nothing

    Public Sub New(ByVal nfo As CharacterNfo)

        If Len(nfo.Description) AndAlso nfo.Description <> "ERROR" Then

            _frm = New Form()
            _web = New WebBrowser()

            AddHandler _frm.Resize, AddressOf Resize
            AddHandler _frm.Closing, AddressOf Closed

            _frm.StartPosition = FormStartPosition.CenterScreen
            _frm.Icon = Form1.Icon
            _frm.Size = DEFAULT_SIZE
            _frm.Text = nfo.CharacterName & "'s Description"
            _web.Location = MARGIN
            _web.ScrollBarsEnabled = True
            '_web.DocumentText = New phpBBCode(nfo.Description).Decode2Html()

            _frm.Controls.Add(_web)
            LoadFrmState()
            _frm.Show()
            Resize(Nothing, Nothing)

        End If

    End Sub

    Private Sub LoadFrmState()

        If GetSetting("descForm/pos/x") <> "" AndAlso GetSetting("descForm/pos/y") <> "" Then
            _frm.Location = New Point(Val(GetSetting("descForm/pos/x")), Val(GetSetting("descForm/pos/y")))
        End If

        If GetSetting("descForm/size/width") = "" Or GetSetting("descForm/size/height") = "" Then
            _frm.Size = DEFAULT_SIZE
        Else
            _frm.Size = New Point(Val(GetSetting("descForm/size/width")), Val(GetSetting("descForm/size/height")))
        End If

        If GetSetting("descForm/windowState") = "" Or GetSetting("descForm/windowState") = "1" Then
            _frm.WindowState = FormWindowState.Normal
        Else
            _frm.WindowState = CInt(GetSetting("descForm/windowState"))
        End If

    End Sub

    Private Sub Closed(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
        SetSetting("descForm/size/width", _frm.Width)
        SetSetting("descForm/size/height", _frm.Height)
        SetSetting("descForm/pos/x", _frm.Location.X)
        SetSetting("descForm/pos/y", _frm.Location.Y)
        SetSetting("descForm/windowState", _frm.WindowState)
        SaveXml()
    End Sub

    Private Sub Resize(ByVal sender As Object, ByVal e As System.EventArgs)

        If _frm.Width < MIN_SIZE.Width Then
            _frm.Width = MIN_SIZE.Width
        End If

        If _frm.Height < MIN_SIZE.Height Then
            _frm.Height = MIN_SIZE.Height
        End If

        _web.Width = _frm.Width - (MARGIN.Width * 2) - (SystemInformation.Border3DSize.Width * 2)
        _web.Height = _frm.Height - (MARGIN.Height * 2) - SystemInformation.MenuHeight - (SystemInformation.Border3DSize.Height * 2)

    End Sub

End Class
