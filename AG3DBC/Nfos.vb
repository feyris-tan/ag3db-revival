Imports System.Xml
Imports System.IO

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
    Dim description As String
    Dim previewsSize As Double
    Dim previewsSizes As Collection
    Dim crc32 As String
    Dim listIndex As Integer
    Dim node As XmlNode
    Dim type As AG3DBType
    Dim dateTime As String
    Dim oFile As FileInfo
    Dim setId As String
    Dim listView As ListView
End Structure

Public Structure SetInfo
    Dim setId As String
    Dim setName As String
    Dim setTime As String
    Dim setUserId As String
    Dim listIndex As Integer
    Dim objs As Collection
End Structure

Public Class CharacterNfo

    Private _nfo As CharNfo

    Private Sub Build(ByVal type As AG3DBType, ByVal node As XmlNode, ByVal listIndex As Integer, ByVal listView As ListView, ByVal listItem As ListViewItem)

        _nfo.charId = XmlNodeText(node.SelectSingleNode("id"))
        _nfo.charName = XmlNodeText(node.SelectSingleNode("charName"))
        _nfo.hits = Val(XmlNodeText(node.SelectSingleNode("hits")))
        _nfo.rating = Val(XmlNodeText(node.SelectSingleNode("rating")))
        _nfo.userId = XmlNodeText(node.SelectSingleNode("userId"))
        _nfo.username = XmlNodeText(node.SelectSingleNode("username"))
        _nfo.size = Val(XmlNodeText(node.SelectSingleNode("size")))
        _nfo.tags = XmlNodeText(node.SelectSingleNode("tags"))
        '_nfo.description = XmlNodeText(node.SelectSingleNode("description"))
        '_nfo.previewsSize = Val(XmlNodeText(node.SelectSingleNode("previewsSize")))
        _nfo.crc32 = XmlNodeText(node.SelectSingleNode("crc32"))
        _nfo.listIndex = listIndex
        _nfo.previews = New Collection
        _nfo.node = node
        _nfo.oFile = Nothing
        _nfo.type = type
        _nfo.previewsSizes = New Collection
        _nfo.previewsSize = 0
        _nfo.listItem = listItem
        _nfo.listView = listView

        For Each prev As FileInfo In GetDBPreviews(_nfo.username, _nfo.charName)
            _nfo.previews.Add(prev.Name, prev.Name)
            _nfo.previewsSizes.Add(prev.Length, prev.Name)
        Next

    End Sub

    Private Sub Build(ByVal type As AG3DBType, ByVal oFile As FileInfo, ByVal listIndex As Integer, ByVal listView As ListView, ByVal listItem As ListViewItem)

        _nfo.oFile = oFile
        _nfo.node = Nothing
        _nfo.type = type
        '_nfo.charId = XmlNodeText(node.SelectSingleNode("id"))
        _nfo.charName = RemoveExtention(oFile.Name)
        _nfo.size = 0
        _nfo.dateTime = GetSTDDateTime(oFile.CreationTime)
        _nfo.tags = GetDetail(AG3DBDetailType.Tag)
        _nfo.description = GetDetail(AG3DBDetailType.Description)
        '_nfo.hits = Val(XmlNodeText(node.SelectSingleNode("hits")))
        '_nfo.rating = Val(XmlNodeText(node.SelectSingleNode("rating")))
        '_nfo.userId = XmlNodeText(node.SelectSingleNode("userId"))
        '_nfo.username = XmlNodeText(node.SelectSingleNode("username"))
        '_nfo.tags = XmlNodeText(node.SelectSingleNode("tags"))
        '_nfo.previewsSize = Val(XmlNodeText(node.SelectSingleNode("previewsSize")))
        _nfo.crc32 = Conversion.Hex(New CRC32().GetCrc32(oFile.FullName)).ToUpper
        _nfo.listIndex = listIndex
        _nfo.previews = New Collection
        _nfo.previewsSizes = New Collection
        _nfo.listItem = listItem
        _nfo.listView = listView

        'For Each prev As FileInfo In GetPreviews(_nfo.charName)
        '    _nfo.previews.Add(prev.Name, prev.Name)
        '    _nfo.previewsSizes.Add(prev.Length, prev.Name)
        '    _nfo.size += prev.Length
        'Next

        _nfo.size += oFile.Length * 2

    End Sub

    Public Sub New(ByVal type As AG3DBType, ByVal node As XmlNode, ByVal listIndex As Integer, ByVal listView As ListView, ByVal listItem As ListViewItem)
        Build(type, node, listIndex, listView, listItem)
    End Sub

    Public Sub New(ByVal type As AG3DBType, ByVal oFile As FileInfo, ByVal listIndex As Integer, ByVal listView As ListView, ByVal listItem As ListViewItem)
        Build(type, oFile, listIndex, listView, listItem)
    End Sub

    Private Function GetDetail(ByVal detailType As AG3DBDetailType) As String

        For Each node As XmlNode In GetSettingNode("details").SelectNodes(detailType.ToString.ToLower)

            If node.Attributes("type").Value = _nfo.type AndAlso node.Attributes("name").Value = _nfo.charName Then
                Return XmlNodeText(node).Replace("&gt;", ">").Replace("&lt;", "<")
            End If

        Next

        Return ""

    End Function

    Private Sub SetDetail(ByVal detailType As AG3DBDetailType, Optional ByVal detail As String = "")

        Dim have As Boolean = False
        Dim detailNode As XmlNode = Nothing

        For Each detailNode In GetSettingNode("details").SelectNodes(detailType.ToString.ToLower)

            If detailNode.Attributes("type").Value = _nfo.type AndAlso detailNode.Attributes("name").Value = _nfo.charName Then
                have = True : Exit For
            End If

        Next

        If have Then

            If detail <> "" Then
                detailNode.InnerText = detail.Replace(">", "&gt;").Replace("<", "&lt;")
            Else
                GetSettingNode("details").RemoveChild(detailNode)
            End If

        ElseIf detail <> "" Then

            Dim newNode As XmlNode = NewXmlNode(xmlSettings, detailType.ToString.ToLower, detail.Replace(">", "&gt;").Replace("<", "&lt;"))

            newNode.Attributes.Append(NewXmlAttribute(xmlSettings, "type", _nfo.type))
            newNode.Attributes.Append(NewXmlAttribute(xmlSettings, "name", _nfo.charName))
            GetSettingNode("details").AppendChild(newNode)

        End If

        SaveXml()

    End Sub

    Public ReadOnly Property HavePreviews() As Boolean
        Get
            Return _nfo.previews.Count > 0
        End Get
    End Property

    Public Sub GetPreviews()
        threads.InvokeObject = Me
        threads.Start(AG3DBCThreadType.DoGetPreviews)
    End Sub

    Private Sub DoGetPreviews()

        Dim w As New WebFormPost(AG3DBC.API.ServerUrl & "\ag3dbc\getPreviews.php")

        Try

            w.AddFormElement("username", GetUsername())
            w.AddFormElement("userId", GetUserId())
            w.AddFormElement("userPwd", GetUserPwd())
            w.AddFormElement("typeId", "0")
            w.AddFormElement("objId", _nfo.charId)
            w.AddFormElement("objUsername", _nfo.username)
            w.AddFormElement("name", _nfo.charName)
            w.Submit()

            If w.Response.StartsWith("0|") Then

                Dim allExists As Boolean = True
                Dim xml As New XmlDocument
                Dim size As Long = 0

                _nfo.previews.Clear()
                _nfo.previewsSizes.Clear()

                xml.LoadXml(w.Response.Split("|")(1))

                For Each preview As XmlNode In xml.SelectNodes("/previews/preview")
                    _nfo.previews.Add(preview.InnerXml, preview.InnerXml)
                    _nfo.previewsSizes.Add(Val(preview.Attributes("size").Value), preview.InnerXml)
                    size += Val(preview.Attributes("size").Value)
                Next

                Dim item As ListViewItem = Selected(Me.List)
                Dim count As Integer = 0

                Switch()

                ftp = FTPConnect()

                Directory.CreateDirectory(GetDBPreviewPath(_nfo.username))
                prevs.Clear()
                watch.StartWatch(size, threadPriority, ftp)

                For Each preview As String In _nfo.previews

                    count += 1

                    If Not File.Exists(GetDBPreviewPath(_nfo.username, preview)) Then
                        Mes("Getting preview " & preview & " (" & count & "/" & _nfo.previews.Count & ")...")
                        ftp.Download(FTP_DIR & "chars/" & _nfo.username & "/" & preview, GetDBPreviewPath(_nfo.username, preview), True)
                    End If

                    prevs.Add(GetDBPreviewPath(_nfo.username, preview))

                Next

                watch.StopWatch()
                prevs.StartPics()
                Mes("Successfully downloaded all previews for " & _nfo.charName & "!", AG3DBCMessageType.Success)

                item.ForeColor = GetDBCharColor(Me)
                item.SubItems(7).Text = "Yes"
                ChangeFullListItem(item, Me.List.Tag)

            Else

                Dim err As String

                Select Case w.Response
                    Case "1"
                        err = "Parameters not properly set."
                End Select

            End If

        Catch ex As Exception
            LogError(ex)
            watch.StopWatch()
            Mes("There was an error while trying to download the previews for " & _nfo.charName & ".", AG3DBCMessageType.Err, True)
        End Try

        Switch()
        Me.List.Select()
        Me.List.Focus()
        Me.DoDBPreviews()

    End Sub

    Public Sub DoLocalPreviews()

        prevs.Clear()
        Directory.CreateDirectory(ag3Chars & _nfo.charName)

        For Each sFile As String In Directory.GetFiles(ag3Chars & _nfo.charName, "*.jpg")
            prevs.Add(sFile, Basename(sFile))
        Next

        If File.Exists(ag3Chars & _nfo.charName & "_v.bmp") Then

            SaveJpeg(New Bitmap(ag3Chars & _nfo.charName & "_v.bmp"), ag3Chars & _nfo.charName & "\_v - " & _nfo.charName & ".jpg", 95)

            If prevs.Pics.Contains("_v - " & _nfo.charName & ".jpg") Then
                prevs.Remove("_v - " & _nfo.charName & ".jpg")
            End If

            prevs.Add(ag3Chars & _nfo.charName & "\_v - " & _nfo.charName & ".jpg", "_v - " & _nfo.charName & ".jpg")

        End If

        prevs.StartPics()

    End Sub

    Public Sub DoDBPreviews()

        prevs.Clear()
        Directory.CreateDirectory(GetDBPreviewPath(_nfo.username))

        For Each oFile As FileInfo In New DirectoryInfo(ag3Previews & _nfo.username).GetFiles("* - " & _nfo.charName & ".jpg")
            prevs.Add(oFile.FullName)
        Next

        prevs.Restart()

    End Sub

    Public ReadOnly Property IsRated() As Boolean

        Get

            For Each node As XmlNode In GetSettingNode("ratings")

                If node.Attributes("charId").Value = _nfo.charId Then
                    Return True
                End If

            Next

            Return False

        End Get

    End Property

    Public Sub Rebuild()

        If NotNothing(_nfo.node) Then
            Build(_nfo.type, _nfo.node, _nfo.listIndex, _nfo.listView, _nfo.listItem)
        Else
            Build(_nfo.type, _nfo.oFile, _nfo.listIndex, _nfo.listView, _nfo.listItem)
        End If

    End Sub

    Public ReadOnly Property IsOwner() As Boolean
        Get
            Return _nfo.userId <> GetUserId()
        End Get
    End Property

    Public Property Description() As String
        Get
            Return _nfo.description
        End Get
        Set(ByVal value As String)
            _nfo.description = value
            SetDetail(AG3DBDetailType.Description, value)
        End Set
    End Property

    Public ReadOnly Property TotalSize() As Long
        Get

            If _nfo.type = AG3DBType.Character Then
                Return DirSize(ag3Chars & _nfo.charName) + (JS3CMI_SIZE * 2)
            Else
                Return 0
            End If

        End Get

    End Property

    Public ReadOnly Property DateTime() As String
        Get
            Return _nfo.dateTime
        End Get
    End Property

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

    Public ReadOnly Property PreviewSizes() As Collection
        Get
            Return _nfo.previewsSizes
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

    Public Property Tags() As String
        Get
            Return _nfo.tags
        End Get
        Set(ByVal value As String)
            _nfo.tags = value
            SetDetail(AG3DBDetailType.Tag, value)
        End Set
    End Property

    Public ReadOnly Property PreviewsSize() As Double

        Get

            Dim size As Double = 0

            For Each prevSize As Double In _nfo.previewsSizes
                size += prevSize
            Next

            Return size

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

    Public ReadOnly Property ListItem() As ListViewItem
        Get
            Return _nfo.listItem
        End Get
    End Property

    Public ReadOnly Property List() As ListView
        Get
            Return _nfo.listView
        End Get
    End Property

    Public ReadOnly Property Node() As XmlNode
        Get
            Return _nfo.node
        End Get
    End Property

    Public ReadOnly Property Type() As AG3DBType
        Get
            Return _nfo.type
        End Get
    End Property

End Class

Public Class SetNfo

    Private _nfo As SetInfo

    Public Sub New(ByVal node As XmlNode, ByVal listIndex As Integer)

        _nfo.setId = XmlNodeText(node.SelectSingleNode("setId"))
        _nfo.setName = XmlNodeText(node.SelectSingleNode("setName"))
        _nfo.setTime = XmlNodeText(node.SelectSingleNode("setTime"))
        _nfo.setUserId = XmlNodeText(node.SelectSingleNode("setUserId"))
        _nfo.objs = New Collection
        _nfo.listIndex = listIndex

    End Sub

    Public ReadOnly Property Objects() As Collection
        Get
            Return _nfo.objs
        End Get
    End Property

    Public ReadOnly Property Id() As String
        Get
            Return _nfo.setId
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            Return _nfo.setName
        End Get
    End Property

    Public ReadOnly Property TimeCreated() As String
        Get
            Return _nfo.setTime
        End Get
    End Property

    Public ReadOnly Property UserId() As String
        Get
            Return _nfo.setUserId
        End Get
    End Property

    Public ReadOnly Property ListIndex() As Integer
        Get
            Return _nfo.listIndex
        End Get
    End Property

End Class