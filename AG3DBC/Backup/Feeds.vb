Imports System.Xml
Imports System.IO

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
    GetSets = 10
    DoGetPreviews = 11
    GetSetObjects = 12
End Enum

Public Class Feeds

    Public Sub GetDBCharsFeed(ByVal list As ListView, ByVal lblCharCount As Label, ByVal ListLocals As ListView)

        list.Visible = False
        list.Sorting = SortOrder.None

        Mes("Refreshing AG3DB character list...")
        prevs.Clear()
        list.Items.Clear()
        Switch()

        Dim response() As String = GetFeed(AG3DBCThreadType.GetDBCharsFeed)

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
                    Dim nfo As New CharacterNfo(AG3DBType.Character, node, list.Tag.Items.Count, list, newItem)

                    newItem.Tag = nfo

                    newItem.SubItems.Add(node.SelectSingleNode("charName").InnerText.Trim)
                    newItem.SubItems.Add(node.SelectSingleNode("username").InnerText.Trim)
                    newItem.SubItems.Add(Format(CInt(node.SelectSingleNode("hits").InnerText.Trim), "###,##0"))
                    newItem.SubItems.Add(ratings.GetRating(node.SelectSingleNode("rating").InnerText.Trim))
                    newItem.SubItems.Add(node.SelectSingleNode("tags").InnerText.Trim)

                    If InLocalsList(node.SelectSingleNode("crc32").InnerText.Trim) Then
                        newItem.SubItems.Add("Yes")
                    Else
                        newItem.SubItems.Add("No")
                    End If

                    If nfo.HavePreviews() Then
                        newItem.SubItems.Add("Yes")
                    Else
                        newItem.SubItems.Add("No")
                    End If

                    'If nfo.Description <> "" AndAlso nfo.Description <> "ERROR" Then
                    '    newItem.SubItems.Add("Yes")
                    'Else
                    '    newItem.SubItems.Add("No")
                    'End If

                    If nfo.IsRated Then
                        newItem.SubItems.Add("Yes")
                    Else
                        newItem.SubItems.Add("No")
                    End If

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

    Public Sub GetSets(ByVal list As ListView)

        list.Visible = False
        list.Sorting = SortOrder.None

        Mes("Refreshing AG3DB Sets list...")
        prevs.Clear()
        list.Items.Clear()
        Switch()

        Dim response() As String = GetFeed(AG3DBCThreadType.GetSets)
        Dim ok As Boolean = True

        If NotNothing(response) Then

            Dim xml As New XmlDocument
            Dim nfo As SetNfo = Nothing
            Dim newItem As ListViewItem = Nothing

            Try

                If IsNothing(list.Tag) Then
                    list.Tag = New ListView()
                Else
                    list.Tag.items.clear()
                End If

                Mes("Populating AG3DB Sets list...")
                xml.LoadXml(response(1))

                For Each node As XmlNode In xml.SelectNodes("/sets/set")

                    nfo = New SetNfo(node, list.Tag.Items.Count)
                    newItem = New ListViewItem(nfo.Name)
                    newItem.Tag = nfo

                    list.Tag.Items.Add(newItem.Clone)

                Next

                CopyListView(list.Tag, list)
                Mes("Sets list refresh successful!", AG3DBCMessageType.Success)

            Catch ex As Exception
                list.Items.Clear()
                LogError(ex)
                Mes("Error populating AG3DB Sets list.", AG3DBCMessageType.Err, True)
            End Try

        End If

        Switch()
        list.Visible = True

    End Sub

    Public Sub GetSetObjects(ByVal list As ListView, ByVal nfo As CharacterNfo)

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
                    Dim nfo As New CharacterNfo(AG3DBType.Character, node, list.Tag.Items.Count, list, newItem)

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

    Public Sub GetUserStatsFeed(ByVal listRatings As ListView, ByVal ListLocals As ListView, ByVal lblUserRating As Label, ByVal lblUserChars As Label)
        listRatings.Visible = False
        ListLocals.Visible = False
        GetUserCharsFeed(ListLocals, lblUserChars)
        GetUserRatingsFeed(listRatings, lblUserRating)
        listRatings.Visible = True
        ListLocals.Visible = True
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
                    Dim nfo As New CharacterNfo(AG3DBType.Character, node, list.Tag.Items.Count, list, newItem)

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
                    Dim nfo As New CharacterNfo(AG3DBType.Character, node, list.Tag.Items.Count, list, newItem)

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

        Dim request As New WebFormPost(SERVER & "ag3dbc\feeds.php")

        request.AddFormElement("userId", GetUserId())
        request.AddFormElement("username", GetUsername())
        request.AddFormElement("userPwd", GetUserPwd())

        Select Case feed
            Case AG3DBCThreadType.GetDBCharsFeed : request.AddFormElement("type", "dbChars")
            Case AG3DBCThreadType.GetUserRatingsFeed : request.AddFormElement("type", "userRatings")
            Case AG3DBCThreadType.GetUserCharsFeed : request.AddFormElement("type", "userCharsTest")
            Case AG3DBCThreadType.GetTopCharsFeed : request.AddFormElement("type", "topChars")
            Case AG3DBCThreadType.GetTopUsersFeed : request.AddFormElement("type", "topUsers")
            Case AG3DBCThreadType.GetSets : request.AddFormElement("type", "sets")
            Case AG3DBCThreadType.GetSetObjects : request.AddFormElement("type", "setsObjs")
        End Select

        Dim retVal As String = request.SubmitAndGetResponse()

        If retVal.StartsWith("0|") Then
            Return ParseRetVal(retVal)
        Else

            Select Case retVal
                Case "1"
                    Mes("Error: Parameters not properly set.", AG3DBCMessageType.Err, True)
                Case "2"
                    Mes("Error: Invalid feed type.", AG3DBCMessageType.Err, True)
                Case "3"
                    Mes("Error: Invalid database login.", AG3DBCMessageType.Err, True)
                Case ""
                Case Else
                    Mes(retVal, AG3DBCMessageType.Err, True)
            End Select

            Return Nothing

        End If

    End Function

    Public Sub New()
    End Sub
End Class

