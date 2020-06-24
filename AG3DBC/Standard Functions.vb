Imports System.IO
Imports System.Diagnostics.Process
Imports System.Threading
Imports System.Text.RegularExpressions
Imports System.Net
Imports System.Xml
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Public Enum FileSystemType
    Directory = 1
    File = 2
End Enum

Public Class MyXml

    Inherits XmlDocument

    Private _filename As String

    Public Property Filename() As String
        Get
            Return _filename
        End Get
        Set(ByVal value As String)
            _filename = value
        End Set
    End Property

End Class

Public Class TreeSearcher

    Private _tree As TreeView

    Public Sub New(ByVal tree As TreeView)
        _tree = tree
    End Sub

End Class


'Public Enum HttpResponseType
'    Str = 1
'    Binary = 2
'End Enum

'Public Class HttpResponse

'    Private _wc As WebClient
'    Private _url As String

'    Public Sub New(ByVal url As String)
'        _url = url
'        _wc = New WebClient()
'        _wc.
'    End Sub

'    public property ResponseType as 

'    Public Property URL() As String
'        Get
'            Return _url
'        End Get
'        Set(ByVal value As String)
'            _url = value
'        End Set
'    End Property

'        Try
'            Return wc.DownloadString(url).Trim
'        Catch ex As Exception
'            Return ex.Message
'        End Try
'End Class
Public Class StreamOps

    Public Shared Sub RemoveBytes(ByVal stream As Stream, ByVal start As Long, ByVal length As Long)

        Dim bytesAfter(stream.Length - (start + length) - 1) As Byte

        stream.Seek(start + length, SeekOrigin.Begin)
        stream.Read(bytesAfter, 0, bytesAfter.Length)
        stream.Seek(start, SeekOrigin.Begin)
        stream.Write(bytesAfter, 0, bytesAfter.Length)
        stream.SetLength(stream.Length - length)

    End Sub

    Public Shared Function GetBytes(ByVal stream As Stream, Optional ByVal start As Long = 0, Optional ByVal length As Long = 0) As Byte()

        If length = 0 Then
            length = stream.Length
        End If

        If start >= length Then
            Throw New ApplicationException("Start index out of bounds (" & start & " in " & stream.Length & ").")
        End If

        Dim bytes(length - 1) As Byte

        stream.Read(bytes, start, length)

        Return bytes

    End Function

    Public Shared Sub WriteString(ByVal stream As Stream, ByVal str As String)

        Dim bytes() As Byte = System.Text.Encoding.ASCII.GetBytes(str)

        stream.Write(bytes, 0, bytes.Length)
        stream.WriteByte(0)

    End Sub

    Public Shared Function ReadString(ByVal stream As Stream) As String

        Dim curChar As Byte
        Dim str As String = ""

        curChar = stream.ReadByte()

        While curChar <> 0 Or stream.Position = stream.Length
            str += Chr(curChar)
            curChar = stream.ReadByte()
        End While

        Return str

    End Function

End Class

Public Class TreeOps

    Public Shared Function GetAllSelected(ByVal start As Object) As TreeNode()

        Dim list As New ArrayList()

        If start.GetType().ToString() = GetType(TreeView).ToString() Then

            For Each node As TreeNode In start.nodes
                DoGetAllSelected(node, list)
            Next

        Else
            DoGetAllSelected(start, list)
        End If

        Return list.ToArray(GetType(TreeNode))

    End Function

    Private Shared Sub DoGetAllSelected(ByVal start As TreeNode, ByRef list As ArrayList)

        For Each node As TreeNode In start.Nodes

            If node.Checked Then
                list.Add(node)
            End If

            If node.Nodes.Count Then
                DoGetAllSelected(node, list)
            End If
        Next

    End Sub
End Class

Public Class GZIP

    Public Shared Function CompressBytes(ByVal bytes() As Byte) As Byte()

        Dim ms As New MemoryStream()
        Dim zip As New ICSharpCode.SharpZipLib.GZip.GZipOutputStream(ms)

        zip.Write(bytes, 0, bytes.Length)
        zip.Close()

        Return ms.ToArray()

    End Function

    Public Shared Function UncompressBytes(ByVal bytes() As Byte) As Byte()

        Dim uncompressed(4096 - 1) As Byte
        Dim zip As New ICSharpCode.SharpZipLib.GZip.GZipInputStream(New MemoryStream(bytes))
        Dim ms As New MemoryStream()
        Dim size As Integer

        While True

            size = zip.Read(uncompressed, 0, uncompressed.Length)

            If (size > 0) Then
                ms.Write(uncompressed, 0, size)
            Else
                Exit While
            End If

        End While

        Dim retBytes() As Byte = ms.ToArray()

        zip.Close()
        ms.Close()

        Return retBytes

    End Function

End Class

Public Class WebFormPost

    Private _formElementsNames As New Collection
    Private _formElementsValues As New Collection
    Private _webRequest As HttpWebRequest = Nothing
    Private _requestStream As StreamWriter = Nothing
    Private _response As String = ""
    Private _symbols() As Char = {"&", "+", "?", "="}
    Private _exception As Exception

    Public Sub New(ByVal url As String)
        _webRequest = HttpWebRequest.Create(url)
    End Sub

    Public Sub Submit()

        Dim postStr As String = ""

        For i As Integer = 1 To _formElementsNames.Count
            postStr += _formElementsNames(i) & "=" & _formElementsValues(i) & "&"
        Next

        _webRequest.Method = "POST"
        _webRequest.ContentType = "application/x-www-form-urlencoded"

        _requestStream = New IO.StreamWriter(_webRequest.GetRequestStream())

        If Len(postStr) Then
            _requestStream.Write(postStr.Substring(0, Len(postStr) - 1))
        End If

        _requestStream.Close()

        Try

            Dim reader As New IO.StreamReader(_webRequest.GetResponse().GetResponseStream())

            _response = reader.ReadToEnd()
            _exception = Nothing

        Catch ex As Exception
            _response = ex.Message
            _exception = ex
        End Try

    End Sub

    Public Function SubmitAndGetResponse() As String
        Submit()
        Return _response
    End Function

    Public ReadOnly Property Response() As String
        Get
            Return _response
        End Get
    End Property

    Public Sub AddFormElement(ByVal name As String, ByVal value As String)
        _formElementsNames.Add(name, name)
        _formElementsValues.Add(ReplaceSymbols(value))
    End Sub

    Private Function ReplaceSymbols(ByVal value As String) As String

        For Each symbol As String In _symbols
            value = value.Replace(symbol, "%" & Conversion.Hex(Asc(symbol)).ToUpper)
        Next

        Return value

    End Function

End Class

Public Class BFF

    Private _result As DialogResult
    Private _bff As New FolderBrowserDialog

    Public Sub New(Optional ByVal startPath As String = "", Optional ByVal title As String = "")

        If Len(startPath) Then
            _bff.SelectedPath = startPath
        End If

        _bff.Description = title
        _result = _bff.ShowDialog

    End Sub

    Public ReadOnly Property Ok() As Boolean
        Get
            Return (_result = DialogResult.OK)
        End Get
    End Property

    Public ReadOnly Property Path() As String
        Get
            Return _bff.SelectedPath
        End Get
    End Property

End Class

Public Class WebData

    Private wreq As HttpWebRequest = Nothing
    Private wres As HttpWebResponse = Nothing
    Private url As String = ""
    Private _get As String = ""
    Private meIsUrl As Boolean = False
    Private urlFilename As String = ""
    Private bLength As Long = 0
    Private resBytesRead As Long = 0
    Private paramDownload1 As String
    Private paramDownload2 As Boolean
    Private paramDownload3 As Integer
    Private _downloadRate As Long

    Public Sub New(ByVal url As String)

        Try

            If Len(url) Then

                Me.url = url
                Me.urlFilename = GetDirName(url, "/")
                Me.wreq = WebRequest.Create(url)

                If Not IsNothing(Me.wreq) Then
                    Me.wres = Me.wreq.GetResponse()
                    Me.bLength = wres.ContentLength
                    Me.meIsUrl = True
                End If

            End If

        Catch

        End Try

    End Sub

    Public ReadOnly Property IsUrl() As Boolean
        Get
            Return Me.meIsUrl
        End Get
    End Property

    Public ReadOnly Property ByteLength() As Long
        Get
            Return Me.bLength
        End Get
    End Property

    Public ReadOnly Property Filename() As String
        Get
            Return Me.urlFilename
        End Get
    End Property

    Public ReadOnly Property BytesDownloaded() As Double
        Get
            Return Me.resBytesRead
        End Get
    End Property

    Public Function Download(ByVal localFilename As String, Optional ByVal overwrite As Boolean = False, Optional ByVal resumeOffet As Integer = 0) As Boolean

        '    Me.paramDownload1 = localFilename
        '    Dim t As New Thread(AddressOf DoDownload)
        '    t.Start()

        'End Function

        'Private Sub DoDownload()

        'Dim localFilename As String = Me.paramDownload1
        'Dim overwrite As Boolean = Me.paramDownload2
        'Dim resumeOffet As Integer = Me.paramDownload3

        Try

            If Not IsNothing(Me.wres) Then

                Dim str As Stream = Me.wres.GetResponseStream()
                Dim inBuf(wres.ContentLength) As Byte
                Dim bytesToRead As Integer = CInt(inBuf.Length)
                Dim bytesRead As Integer = 0
                Dim n As Integer

                Me.resBytesRead = 0

                While bytesToRead > 0

                    n = str.Read(inBuf, bytesRead, bytesToRead)

                    If n = 0 Then
                        Exit While
                    End If

                    bytesRead += n
                    Me.resBytesRead = bytesRead
                    bytesToRead -= n

                End While

                Dim fstr As FileStream

                If overwrite Then
                    fstr = New FileStream(localFilename, FileMode.Create, FileAccess.Write)
                Else
                    fstr = New FileStream(localFilename, FileMode.CreateNew, FileAccess.Write)
                End If

                fstr.Write(inBuf, 0, bytesRead)
                str.Close()
                fstr.Close()

                Return True

            Else
                Return False
            End If

        Catch
            Return False
        End Try

    End Function

    Private Sub perSecond()

        Dim lastBytes As Long = 0

        While True
            _downloadRate = 0
        End While
    End Sub

End Class

Public Enum SpecialFolders
    DesktopAllUsers = 0
    StartMenuAllUsers = 1
    DesktopCurrentUser = 4
    ProgramFiles = 2
    System32 = 3
End Enum

Public Enum FileSizes
    Bytes = 0
    Kilobytes = 1
    Megabytes = 2
    Gigabytes = 3
    Terabytes = 4
    Perabytes = 5
End Enum

Public Class ListItem

    Private _display As String
    Private _nfo As Collection
    Private _tag As Object

    Public Sub New(ByVal Text As String, Optional ByVal Info As Collection = Nothing, Optional ByVal Tag As Object = Nothing)
        Me._display = Text
        Me._nfo = IIf(NotNothing(Info), Info, New Collection)
        Me._tag = Tag
    End Sub

    Public Overrides Function ToString() As String
        Return _display
    End Function

    Public Property Text() As String

        Get
            Return Me._display
        End Get
        Set(ByVal value As String)
            Me._display = value
        End Set

    End Property

    Public Property Info() As Collection

        Get
            Return Me._nfo
        End Get
        Set(ByVal value As Collection)
            Me._nfo = value
        End Set

    End Property

    Public Property Tag() As Object

        Get
            Return Me._tag
        End Get
        Set(ByVal value As Object)
            Me._tag = value
        End Set

    End Property

End Class

Public Class CRC32

    ' This is v2 of the VB CRC32 algorithm provided by Paul
    ' (wpsjr1@succeed.net) - much quicker than the nasty
    ' original version I posted.  Excellent work!

    Private crc32Table() As Integer
    Private Const BUFFER_SIZE As Integer = 1024

    Public Function GetCrc32(ByRef stream As System.IO.Stream) As Integer

        Dim crc32Result As Integer
        crc32Result = &HFFFFFFFF

        Dim buffer(BUFFER_SIZE) As Byte
        Dim readSize As Integer = BUFFER_SIZE

        Dim count As Integer = stream.Read(buffer, 0, readSize)
        Dim i As Integer
        Dim iLookup As Integer
        Dim tot As Integer = 0
        Do While (count > 0)
            For i = 0 To count - 1
                iLookup = (crc32Result And &HFF) Xor buffer(i)
                crc32Result = ((crc32Result And &HFFFFFF00) \ &H100) And 16777215 ' nasty shr 8 with vb :/
                crc32Result = crc32Result Xor crc32Table(iLookup)
            Next i
            count = stream.Read(buffer, 0, readSize)
        Loop

        GetCrc32 = Not (crc32Result)

    End Function

    Public Function GetCrc32(ByRef filename As String) As Integer

        Dim fs As New FileStream(filename, FileMode.Open)
        Dim crc As Integer = GetCrc32(fs)

        fs.Close()
        fs.Dispose()

        Return crc

    End Function

    Public Sub New()

        ' This is the official polynomial used by CRC32 in PKZip.
        ' Often the polynomial is shown reversed (04C11DB7).
        Dim dwPolynomial As Integer = &HEDB88320
        Dim i As Integer, j As Integer

        ReDim crc32Table(256)
        Dim dwCrc As Integer

        For i = 0 To 255
            dwCrc = i
            For j = 8 To 1 Step -1
                If (dwCrc And 1) Then
                    dwCrc = ((dwCrc And &HFFFFFFFE) \ 2&) And &H7FFFFFFF
                    dwCrc = dwCrc Xor dwPolynomial
                Else
                    dwCrc = ((dwCrc And &HFFFFFFFE) \ 2&) And &H7FFFFFFF
                End If
            Next j
            crc32Table(i) = dwCrc
        Next i
    End Sub

End Class

Public Class SuperXml

    Private _xml As XmlDocument

    Public Sub New(ByVal xmlFile As String)

    End Sub

    Public Sub LoadXml(ByVal xml As String)

        _xml = New XmlDocument

        _xml.LoadXml(xml)

    End Sub

End Class

Module STD

    Public Const KB As Long = 1024
    Public Const MB As Long = 1024 * 1024
    Public Const GB As Long = 1024 * 1024 * 1024

    Private byteSizes As String() = Split("bytes KBs MBs GBs TBs")
    Private oBFF As New FolderBrowserDialog
    Private oOpenFile As New OpenFileDialog
    Private oSaveFile As New SaveFileDialog

    Public appPath As String = Environment.CurrentDirectory
    Public endl As String = vbNewLine
    Public nl As String = vbNewLine
    Public null As Object = Nothing
    Public xmlHeader As String = "<?xml version='1.0' encoding='UTF-8' standalone='yes'?>"
    Public rnd As New Random
    Public crc As New CRC32()

    Public Function ConcatBytes(ByVal a() As Byte, ByVal b() As Byte) As Byte()

        Dim bytes(a.Length + b.Length - 1) As Byte

        Array.Copy(a, bytes, a.Length)
        Array.Copy(b, 0, bytes, a.Length, b.Length)

        Return bytes

    End Function

    Public Function Href(ByVal url As String) As Exception

        Try
            Process.Start(url)
        Catch ex As Exception
            Dim a As Object = ex.Message
        End Try

        Return Nothing

    End Function

    Public Function GetFirstTreeNode(ByVal tree As TreeView) As TreeNode
        Return IIf(tree.Nodes.Count > 0, tree.Nodes(0), Nothing)
    End Function

    Public Function AskQuestion(ByVal question As String) As MsgBoxResult
        Return MsgBox(question, MsgBoxStyle.Question + MsgBoxStyle.YesNoCancel)
    End Function

    Public Sub AddSortedListViewItem(ByVal list As ListView, ByVal newItem As ListViewItem, Optional ByVal column As Integer = 0)

        For Each item As ListViewItem In list.Items

            If newItem.SubItems(column).Text.ToLower <= item.SubItems(column).Text Then
                list.Items.Insert(item.Index, newItem) : Exit Sub
            End If

        Next

        list.Items.Add(newItem)

    End Sub

    Public Function SpecialFolder(ByVal folder As Environment.SpecialFolder) As String
        Return Environment.GetFolderPath(folder)
    End Function

    Public Function SearchTree(ByVal startNode As TreeNode, ByVal searchString As String) As TreeNode

        If startNode.Text.ToLower = searchString.ToLower Then
            Return startNode
        End If

        For Each node As TreeNode In startNode.Nodes
            Return SearchTree(node, searchString)
        Next

        Return Nothing

    End Function

    Public Function StringPad(ByVal str As String, ByVal padStr As String, ByVal padLen As Integer, Optional ByVal rightToLeft As Boolean = True) As String

        If rightToLeft Then

            For i As Integer = 1 To padLen - str.Length
                str = padStr & str
            Next

        Else

            For i As Integer = 1 To padLen - str.Length
                str += padStr
            Next

        End If

        Return str

    End Function

    Public Sub AddSortedTreeNode(ByVal parentNode As Object, ByVal childNode As TreeNode, Optional ByVal checkExistance As Boolean = False)

        Dim value = childNode.Text.ToLower

        If checkExistance Then

            For Each node As TreeNode In parentNode.nodes

                If value = node.Text.ToLower Then
                    Exit Sub
                ElseIf childNode.Text.ToLower < node.Text.ToLower Then
                    parentNode.nodes.insert(node.Index, childNode) : Exit Sub
                End If

            Next

        Else

            For Each node As TreeNode In parentNode.nodes

                If childNode.Text.ToLower < node.Text.ToLower Then
                    parentNode.nodes.insert(node.Index, childNode) : Exit Sub
                End If

            Next

        End If

        parentNode.nodes.add(childNode)

    End Sub

    Public Function ConcatArrays(ByVal array1() As Object, ByVal array2() As Object) As Object()

        Dim arr(array1.Length + array2.Length - 1) As Object
        Dim i As Long

        For i = 0 To array1.Length - 1
            arr(i) = array1(i)
        Next

        For i = array1.Length To array1.Length + array2.Length - 1
            arr(i) = array2(i - array1.Length - 1)
        Next

        Return arr

    End Function

    Public Function SelectedTreeNodes(ByVal tree As TreeView) As ArrayList

        Dim nodes As New ArrayList

        For Each node As TreeNode In tree.Nodes

            If node.Checked Then
                nodes.Add(node)
            End If

        Next

        Return nodes

    End Function

    Public Sub XmlAddSortedChild(ByVal root As XmlNode, ByVal newNode As XmlNode, ByVal fieldName As String)

        Dim value As String = XmlNodeText(newNode(fieldName)).ToLower

        For Each curNode As XmlNode In root.ChildNodes

            If value < XmlNodeText(curNode(fieldName)).ToLower Then
                root.InsertBefore(newNode, curNode) : Exit Sub
            End If

        Next

        root.AppendChild(newNode)

    End Sub

    Public Function ControlHitTest(ByVal control1 As Control, ByVal control2 As Control) As Boolean

        Dim c1p11 As New Point(control1.Location.X, control1.Location.Y)
        Dim c1p12 As New Point(control1.Location.X + control1.Size.Width, control1.Location.Y)
        Dim c1p21 As New Point(control1.Location.X, control1.Location.Y + control1.Size.Height)
        Dim c1p22 As New Point(control1.Location.X + control1.Size.Width, control1.Location.Y + control1.Size.Height)
        Dim c2p11 As New Point(control2.Location.X, control2.Location.Y)
        Dim c2p12 As New Point(control2.Location.X + control2.Size.Width, control2.Location.Y)
        Dim c2p21 As New Point(control2.Location.X, control2.Location.Y + control2.Size.Height)
        Dim c2p22 As New Point(control2.Location.X + control2.Size.Width, control2.Location.Y + control2.Size.Height)

        If c1p11.X > c1p11.X Then

            If control1.Location.Y Then
            End If

        End If
    End Function

    Public Function DirSize(ByVal startPath As String, Optional ByVal recursive As Boolean = True) As Long

        Dim root As New DirectoryInfo(startPath)
        Dim size As Long = 0

        For Each oDir As DirectoryInfo In root.GetDirectories()
            size += DirSize(oDir.FullName)
        Next

        For Each oFile As FileInfo In root.GetFiles()
            size += oFile.Length
        Next

        Return size

    End Function

    Public Function ChooseColor() As Color

        Dim colorBox As New ColorDialog
        Dim result As DialogResult
        Dim retColor As Color

        colorBox.AnyColor = True
        colorBox.AllowFullOpen = True
        colorBox.SolidColorOnly = False
        colorBox.FullOpen = True
        result = colorBox.ShowDialog()
        retColor = colorBox.Color

        colorBox.Dispose()

        If result = DialogResult.OK Then
            Return colorBox.Color
        Else
            Return Color.Empty
        End If

    End Function

    Public Function ChooseColor(ByVal defaultColor As Color) As Color

        Dim colorBox As New ColorDialog
        Dim result As DialogResult
        Dim retColor As Color

        colorBox.AnyColor = True
        colorBox.AllowFullOpen = True
        colorBox.SolidColorOnly = False
        colorBox.Color = defaultColor
        colorBox.FullOpen = True
        result = colorBox.ShowDialog()
        retColor = colorBox.Color

        colorBox.Dispose()

        If result = DialogResult.OK Then
            Return colorBox.Color
        Else
            Return Color.Empty
        End If

    End Function

    Public Function GetSTDDateTime(Optional ByVal dateTime As Date = Nothing, Optional ByVal format As String = "yyyy-MM-dd @ HH:mm:ss") As String

        If dateTime.Ticks = 0 Then
            dateTime = Now
        End If

        Return dateTime.ToString(format)

    End Function

    Public Function GetSTDTime(Optional ByVal dateTime As Date = Nothing, Optional ByVal format As String = "HH:mm:ss") As String

        If dateTime.Ticks = 0 Then
            dateTime = Now
        End If

        Return dateTime.ToString(format)

    End Function

    Public Sub SortListView(ByVal list As ListView, ByVal column As Integer, ByVal sortOrder As SortOrder, Optional ByVal useTag As Boolean = False)

        If list.Items.Count > 1 Then

            list.ListViewItemSorter = New ListViewComparer(column, sortOrder, useTag)

            list.Sort()

        End If

    End Sub

    Public Function DownloadFile(ByVal url As String, ByVal localFile As String, Optional ByVal fileMode As IO.FileMode = FileMode.Create) As Boolean

        Try

            Dim wc As New WebClient
            Dim data() As Byte = wc.DownloadData(url)
            Dim fs As New FileStream(localFile, fileMode)

            fs.Write(data, 0, data.Length)
            fs.Close()
            fs.Dispose()

            Return True

        Catch ex As Exception
            Return False
        End Try

    End Function

    'Public Sub StartThread(ByVal thread As Thread, ByVal subroutine As System.Threading.ThreadStart, Optional ByVal theardPriority As ThreadPriority = Threading.ThreadPriority.Normal, Optional ByVal threadParameter As Object = Nothing)

    '    If NotNothing(thread) AndAlso thread.IsAlive Then
    '        Throw New ApplicationException("Thread is already running")
    '    End If

    '    thread = New Thread(subroutine)
    '    thread.Priority = theardPriority

    '    If IsNothing(threadParameter) Then
    '        thread.Start()
    '    Else
    '        thread.Start(threadParameter)
    '    End If

    'End Sub

    Public Sub ListViewAddSorted(ByVal list As ListView, ByVal newItem As ListViewItem, ByVal order As SortOrder)

        If order = SortOrder.Ascending Then

            For Each item As ListViewItem In list.Items

                If newItem.Text.ToLower < item.Text.ToLower Then
                    list.Items.Insert(item.Index, newItem)
                    Exit Sub
                End If

            Next

            list.Items.Add(newItem)

        Else

            For Each item As ListViewItem In list.Items

                If newItem.Text.ToLower > item.Text.ToLower Then
                    list.Items.Insert(item.Index, newItem)
                    Exit Sub
                End If

            Next

            list.Items.Add(newItem)

        End If

    End Sub

    Public Function LoadXmlNode(ByVal xml As XmlDocument, ByVal nodePath As String, Optional ByVal attribute As String = "", Optional ByVal defaultValue As String = "") As String

        Dim value As String

        If IsNothing(xml.SelectSingleNode(nodePath)) Then
            xml.SelectSingleNode(ParentName(nodePath, "/")).AppendChild(NewXmlNode(xml, Basename(nodePath, "/"), defaultValue))
            value = defaultValue
        Else
            value = XmlNodeText(xml.SelectSingleNode(nodePath))
        End If

        If Len(attribute) Then

            If IsNothing(xml.SelectSingleNode(nodePath).Attributes(attribute)) Then
                xml.SelectSingleNode(nodePath).Attributes.Append(NewXmlAttribute(xml, attribute, defaultValue))
                Return defaultValue
            Else
                Return xml.SelectSingleNode(nodePath).Attributes(attribute).Value
            End If

        End If

        Return value

    End Function

    'Public Function NumPadding(ByVal number As Integer, ByVal padding As Integer)

    '    dim 
    'End Function

    Public Sub RemoveFile(ByVal filename As String)

        If File.Exists(filename) Then
            File.Delete(filename)
        End If

    End Sub

    Public Sub cd(ByVal sPath As String)

        If Directory.Exists(sPath) Then
            My.Computer.FileSystem.CurrentDirectory = sPath
        Else
            Throw New ApplicationException("Path does not exist")
        End If

    End Sub

    Public Function Percent(ByVal value As Integer, ByVal max As Integer, Optional ByVal stringFormat As String = "0.00%") As String

        If max <> 0 Then
            Return Format(value / max, stringFormat)
        Else
            Return "0.00%"
        End If

    End Function

    Public Function Dec2Hex(ByVal number As Long) As String
        Return Conversion.Hex(number).ToUpper
    End Function

    Public Function Hex2Dec(ByVal hex As String) As Integer
        Return Val("&H" & hex)
    End Function

    Public Function PwdHash(ByVal password As String) As String

        Dim hash As String = ""
        Dim ascii As Integer

        For i As Integer = 1 To password.Length

            ascii = Asc(password.Substring(i - 1, 1))

            If ascii < 16 Then
                hash += "0" & Hex((Math.Abs(ascii - 255)))
            Else
                hash += Hex((Math.Abs(ascii - 255)))
            End If

        Next

        Return hash.ToLower

    End Function

    Public Function PwdUnHash(ByVal hash As String) As String

        Dim unhash As String = ""
        Dim hex As String
        Dim ascii As Integer

        For i As Integer = 1 To hash.Length Step 2
            hex = hash.Substring(i - 1, 2)
            ascii = Hex2Dec(hex)
            unhash += Chr(Math.Abs(ascii - 255))
        Next

        Return unhash

    End Function

    Public Function GetHttpResponseString(ByVal url As String) As String

        Dim wc As New WebClient

        Try
            Return wc.DownloadString(url).Trim
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    Public Function ParentName(ByVal str As String, Optional ByVal delimeter As String = "\") As String

        If str.Contains(delimeter) Then

            Dim spl() As String = Split(str, delimeter)
            Dim str2 As String = ""

            For i As Integer = 0 To spl.Length - 2
                str2 += spl.GetValue(i) & delimeter
            Next

            Return str2.Substring(0, str2.Length - 1)

        Else
            Return str
        End If

    End Function

    Public Function Basename(ByVal str As String, Optional ByVal delimeter As String = "\") As String

        If str.Contains(delimeter) Then
            Dim spl() As String = Split(str, delimeter)
            Return spl.GetValue(spl.Length - 1)
        Else
            Return str
        End If

    End Function

    Public Function NewXmlDocument(ByVal xmlFile As String, Optional ByVal rootNode As String = "settings", Optional ByVal comment As String = "") As XmlDocument

        Dim xml As New XmlDocument

        If Not File.Exists(xmlFile) Then
            File.WriteAllText(xmlFile, xmlHeader & endl & "<" & rootNode & ">" & endl & "</" & rootNode & ">")
        End If

        Try
            xml.Load(xmlFile)
        Catch ex As Exception

            If ex.Message.ToLower.Contains("root element is missing") Then
                xml.AppendChild(NewXmlNode(xml, rootNode))
                xml.Save(xmlFile)
            End If

        End Try

        If IsNothing(xml.SelectSingleNode("/" & rootNode)) Then
            xml.AppendChild(NewXmlNode(xml, rootNode))
            xml.Save(xmlFile)
        End If

        Return xml

    End Function

    Public Sub RemoveXmlNode(ByVal node As XmlNode)
        node.ParentNode.RemoveChild(node)
    End Sub

    Public Function GetDrive(ByVal sFileSystem As String) As String

        If RegExMatch(sFileSystem, "\:\\") Then

            Dim spl() As String = Split(sFileSystem, ":\")

            Return spl.GetValue(0) & ":\"

        Else
            Return sFileSystem
        End If

    End Function

    Public Function RemoveDrive(ByVal sFileSystem As String) As String

        If RegExMatch(sFileSystem, "\:\\") Then

            Dim spl() As String = Split(sFileSystem, ":\")

            Return spl.GetValue(1)

        Else
            Return sFileSystem
        End If

    End Function

    Public Function Arr2Col(ByVal arr As System.Array) As Collection
        Return Array2Collection(arr)
    End Function

    Public Function Array2Collection(ByVal arr As System.Array) As Collection

        Dim c As New Collection
        Dim obj As Object

        For Each obj In arr
            c.Add(obj)
        Next

        Return c

    End Function

    Public Function GetClientXPos(ByVal control As Control) As Integer
        Return control.PointToClient(Windows.Forms.Cursor.Position).X
    End Function

    Public Function GetClientYPos(ByVal control As Control) As Integer
        Return control.PointToClient(Windows.Forms.Cursor.Position).Y
    End Function

    Public Function GetClientPos(ByVal control As Control) As Point
        Return control.PointToClient(Windows.Forms.Cursor.Position)
    End Function

    Public Function NewXmlDocument() As XmlDocument

        Dim xml As New XmlDocument

        xml.PreserveWhitespace = False

        Return xml

    End Function

    Public Function NewXmlAttribute(ByVal xml As XmlDocument, ByVal name As String, ByVal value As String) As XmlAttribute

        Dim att As XmlAttribute = xml.CreateAttribute(name)

        att.Value = value

        Return att

    End Function

    Public Function NotNothing(ByVal obj As Object) As Boolean
        Return Not IsNothing(obj)
    End Function

    Public Function XmlNodeText(ByVal node As XmlNode) As String

        If NotNothing(node) Then
            Return node.InnerText.Trim
        Else
            Return ""
        End If

    End Function

    Public Function XmlNodeText(ByVal node As XmlElement) As String

        If NotNothing(node) Then
            Return node.InnerText.Trim
        Else
            Return ""
        End If

    End Function

    Public Function NewXmlNode(ByVal xml As XmlDocument, ByVal name As String, Optional ByVal innerXml As String = "") As XmlNode

        Dim newNode As XmlNode = xml.CreateNode(XmlNodeType.Element, name, "")

        newNode.InnerXml = innerXml.Replace("&", "&#38;")

        Return newNode

    End Function

    Public Function GetNodeRoot(ByVal node As TreeNode) As TreeNode

        While Not IsNothing(node.Parent)
            node = node.Parent
        End While

        Return node

    End Function

    Public Function SearchTree(ByVal search As String, ByVal startNode As TreeNode) As TreeNode

        Dim retNode As TreeNode = DoSearchTree(search, startNode)

        If NotNothing(retNode) Then
            Return retNode
        End If

        While True

            While NotNothing(startNode.Parent)
                startNode = startNode.Parent
            End While

            If IsNothing(startNode.NextNode) Then
                Return Nothing
            End If

            startNode = startNode.NextNode
            retNode = DoSearchTree(search, startNode)

            If NotNothing(retNode) Then
                Return retNode
            End If

        End While

        Return Nothing

    End Function

    Private Function DoSearchTree(ByVal search As String, ByVal startNode As TreeNode) As TreeNode

        If startNode.Text.ToLower.Contains(search.ToLower) Then
            Return startNode
        End If

        Dim curNode As TreeNode
        Dim searchNode As TreeNode

        For Each curNode In startNode.Nodes

            searchNode = DoSearchTree(search, curNode)

            If Not IsNothing(searchNode) Then
                Return searchNode
            End If

        Next

        Return Nothing

    End Function

    Public Function SelectedListItem(ByVal list As ListView) As ListViewItem

        If list.SelectedItems.Count Then
            Return list.SelectedItems(0)
        End If

        Return Nothing

    End Function

    Public Function Plural(ByVal str As String, ByVal number As Long) As String

        If number = 1 Then
            Return number & " " & str
        Else

            If str.Substring(str.Length - 1, 1).ToLower = "y" Then
                Return number & " " & str.Substring(0, str.Length - 1) & "ies"
            Else
                Return number & " " & str & "s"
            End If

        End If

    End Function

    Public Function GetSTDNow(Optional ByVal delimiter As String = " ") As String
        Return Now.Year & "-" & Format(Now.Month, "00") & "-" & Format(Now.Day, "00") & delimiter & Format(Now.Hour, "00") & ":" & Format(Now.Minute, "00") & ":" & Format(Now.Second, "00")
    End Function

    Public Function GetSTDToday() As String
        Return Now.Year & "-" & Format(Now.Month, "00") & "-" & Format(Now.Day, "00")
    End Function

    Public Sub IllegalThreads()
        Control.CheckForIllegalCrossThreadCalls = False
    End Sub

    Public Function RegExMatch(ByVal expression As String, ByVal pattern As String) As Boolean
        Return Regex.IsMatch(expression, pattern)
    End Function

    'Public Function CreateShortcut(ByVal destinationPath As String, ByVal shortFilename As String, ByVal targetFileName As String, Optional ByVal description As String = "", Optional ByVal iconNumber As Integer = 0) As Boolean

    '    Try

    '        Dim shell As IWshRuntimeLibrary.WshShell = New IWshRuntimeLibrary.WshShellClass
    '        Dim shortcut As IWshRuntimeLibrary.WshShortcut = shell.CreateShortcut(destinationPath & "\" & shortFilename & ".lnk")

    '        shortcut.TargetPath = targetFileName
    '        shortcut.IconLocation = targetFileName & "," & iconNumber.ToString

    '        If Len(description) Then
    '            shortcut.Description = description
    '        Else
    '            shortcut.Description = shortFilename
    '        End If

    '        shortcut.Save()

    '        Return True

    '    Catch
    '        Return False
    '    End Try

    'End Function

    'Public Function SpecialFolder(ByVal folder As SpecialFolders) As String

    '    Dim shell As IWshRuntimeLibrary.WshShell = New IWshRuntimeLibrary.WshShell

    '    Select Case folder
    '        Case SpecialFolders.DesktopAllUsers : Return shell.SpecialFolders.Item(0).ToString
    '        Case SpecialFolders.DesktopCurrentUser : Return shell.SpecialFolders.Item(4).ToString
    '        Case Else : Return ""
    '    End Select

    'End Function

    Public Function MousePos(ByVal form As Control) As Point
        Return form.PointToClient(Windows.Forms.Cursor.Position)
    End Function

    Public Sub ShowCollection(ByVal collection As Collection)

        Dim index As Long
        Dim str As String = ""

        For index = 1 To collection.Count
            str += collection(index).ToString & vbNewLine
        Next

        MsgBox(str.Substring(0, Len(str) - 2))

    End Sub

    Public Function InArray(ByVal arr As System.Array, ByVal search As Object) As Boolean

        Dim index As Long

        For index = 0 To arr.Length - 1

            If arr.GetValue(index).Equals(search) Then
                Return True
            End If

        Next

        Return False

    End Function

    Public Function PrintDir(ByVal startPath As String, Optional ByVal level As Integer = 0) As String

        If Directory.Exists(startPath) Then

            Dim str As String = ""

            If level = 0 Then
                str += startPath & vbNewLine
            End If

            Dim root As New DirectoryInfo(startPath)
            Dim oDir As DirectoryInfo
            Dim oFile As FileInfo
            Dim spacer As String = ""
            Dim xxx As Integer

            For xxx = 1 To level + 1
                spacer += "  "
            Next

            For Each oDir In root.GetDirectories()
                str += spacer & oDir.Name & "\" & vbNewLine
                str += PrintDir(oDir.FullName, level + 1)
            Next

            For Each oFile In root.GetFiles()
                str += spacer & oFile.Name & vbNewLine
            Next

            Return str

        Else
            Return "Does not exist (" & startPath & ")"
        End If

    End Function

    Public Function GetNormalTime() As String

        Dim sTime As String = ""

        If Now.Hour <= 12 Then
            sTime += Now.Hour & ":"
        Else
            sTime += Now.Hour Mod 12 & ":"
        End If

        sTime += Format(Now.Minute, "00") & ":" & Format(Now.Second, "00")

        If Now.Hour < 12 Then
            sTime += " AM"
        Else
            sTime += " PM"
        End If

        Return sTime

    End Function

    Public Function CopyFile(ByVal source As String, ByVal destination As String, Optional ByVal overwrite As Boolean = True) As Boolean

        Try

            If File.Exists(source) And source <> destination Then

                File.Copy(source, destination, overwrite)

                Return True

            Else
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Sub KillProcess(ByVal processName As String)

        Dim processes() As Process = Process.GetProcesses

        Try

            For Each p As Process In processes

                If p.ProcessName = processName Then
                    p.Kill()
                End If

            Next

        Catch : End Try

    End Sub

    Public Function Null2String(ByVal value As String)
        If IsNothing(value) Then
            Return ""
        Else
            Return value
        End If
    End Function

    Public Function LoadPic(ByVal picBox As PictureBox, ByVal picFrame As Object, ByVal picFile As String) As Boolean

        Try

            picBox.SizeMode = PictureBoxSizeMode.StretchImage
            picBox.Image = Nothing

            Dim fs As New FileStream(picFile, FileMode.Open)
            Dim oPic As Image = New Bitmap(fs)
            Dim width As Long = oPic.Width
            Dim height As Long = oPic.Height
            Dim ratio As Double = width / height

            fs.Close()

            picBox.Left = picFrame.Left + 2
            picBox.Top = picFrame.Top + 2

            If (height > picFrame.Height - 4) And (width > picFrame.Width - 4) Then

                If ratio > (picFrame.width / picFrame.height) Then
                    width = picFrame.Width - 4
                    height = Int(width / ratio)
                    picBox.Top = ((picFrame.Height - height) / 2) + picBox.Top - 2
                Else
                    height = picFrame.Height - 4
                    width = Int(height * ratio)
                    picBox.Left = ((picFrame.Width - width) / 2) + picBox.Left - 2
                End If

            Else
                picBox.Left = ((picFrame.Width - width) / 2) + picBox.Left - 2
                picBox.Top = ((picFrame.Height - height) / 2) + picBox.Top - 2
            End If

            picBox.Size = New System.Drawing.Size(width, height)
            picBox.Image = oPic

            Return True

        Catch
            picBox.Image = Nothing : Return False
        End Try



    End Function

    Public Function Echo(ByVal str As String) As String

        Dim curChar As Long
        Dim strLen As Long = str.Length
        Dim retVal As String = ""

        For curChar = 0 To strLen - 1

            If str(curChar) = "\" Then

                curChar += 1

                Select Case (str(curChar))

                    Case "n" : retVal += vbNewLine
                    Case "t" : retVal += vbTab
                    Case "q" : retVal += Chr(34)
                    Case Else : retVal += str(curChar)

                End Select

            Else
                retVal += str(curChar)
            End If

        Next

        Return retVal

    End Function

    Public Function MoveFile(ByVal source As FileInfo, ByVal destination As String, Optional ByVal overwrite As Boolean = True) As Boolean

        If source.Exists Then

            Dim desExists As Boolean = File.Exists(destination)

            If source.FullName <> destination Then

                If desExists And overwrite Then

                    File.Delete(destination)
                    source.MoveTo(destination)
                    Return True

                ElseIf Not desExists Then

                    source.MoveTo(destination)
                    Return True

                Else
                    Return False
                End If

            Else
                Return False
            End If

        End If

    End Function

    Public Function MoveFile(ByVal source As String, ByVal destination As String, Optional ByVal overwrite As Boolean = True) As Boolean

        If File.Exists(source) Then

            Dim desExists As Boolean = File.Exists(destination)

            If desExists And overwrite Then

                File.Delete(destination)
                File.Move(source, destination)
                Return True

            ElseIf Not desExists Then

                File.Move(source, destination)
                Return True

            Else
                Return False
            End If

        Else
            Return False
        End If

    End Function

    Public Function SaveFile(Optional ByVal filter As String = "All files|*.*", Optional ByVal initialDirectory As String = "", Optional ByVal title As String = "Save file...", Optional ByVal initialFilename As String = "") As String

        oSaveFile.AddExtension = True
        oSaveFile.Filter = filter
        oSaveFile.Title = title
        oSaveFile.FileName = initialFilename

        If Directory.Exists(initialDirectory) Then
            oSaveFile.InitialDirectory = initialDirectory
        Else
            oSaveFile.InitialDirectory = ""
        End If

        If oSaveFile.ShowDialog() = DialogResult.OK Then
            Return oSaveFile.FileName
        Else
            Return ""
        End If

    End Function

    Public Function OpenFile(Optional ByVal filter As String = "All files|*.*", Optional ByVal initialDirectory As String = "", Optional ByVal title As String = "Open file...", Optional ByVal initialFilename As String = "") As String

        oOpenFile.AddExtension = True
        oOpenFile.Filter = filter
        oOpenFile.Title = title
        oOpenFile.FileName = initialFilename
        oOpenFile.Multiselect = False
        oOpenFile.CheckFileExists = True

        If Directory.Exists(initialDirectory) Then
            oOpenFile.InitialDirectory = initialDirectory
        Else
            oOpenFile.InitialDirectory = ""
        End If

        If oOpenFile.ShowDialog() = DialogResult.OK Then
            Return oOpenFile.FileName
        Else
            Return ""
        End If

    End Function

    Public Function OpenFiles(Optional ByVal filter As String = "All files|*.*", Optional ByVal initialDirectory As String = "", Optional ByVal title As String = "Open files...") As String()

        oOpenFile.AddExtension = True
        oOpenFile.Filter = filter
        oOpenFile.Title = title
        oOpenFile.Multiselect = True
        oOpenFile.CheckFileExists = True

        If Directory.Exists(initialDirectory) Then
            oOpenFile.InitialDirectory = initialDirectory
        Else
            oOpenFile.InitialDirectory = ""
        End If

        If oOpenFile.ShowDialog() = DialogResult.OK Then
            Return oOpenFile.FileNames
        Else
            Return Nothing
        End If

    End Function

    Public Function BFF(Optional ByVal startDir As String = "", Optional ByVal title As String = "") As String

        If Len(startDir) And Directory.Exists(startDir) Then
            oBFF.SelectedPath = startDir
        Else
            oBFF.SelectedPath = ""
        End If

        If Len(title) Then
            oBFF.Description = title
        End If

        Dim result As DialogResult = oBFF.ShowDialog()

        If result = DialogResult.OK And Directory.Exists(oBFF.SelectedPath) Then
            Return oBFF.SelectedPath
        Else
            Return ""
        End If

    End Function

    Public Function oDirBFF(Optional ByVal startDir As String = "", Optional ByVal title As String = "") As DirectoryInfo

        If Len(startDir) And Directory.Exists(startDir) Then
            oBFF.SelectedPath = startDir
        Else
            oBFF.SelectedPath = ""
        End If

        If Len(title) Then
            oBFF.Description = title
        End If

        Dim result As DialogResult = oBFF.ShowDialog()

        If result = DialogResult.OK And Directory.Exists(oBFF.SelectedPath) Then
            Return New DirectoryInfo(oBFF.SelectedPath)
        Else
            Return Nothing
        End If

    End Function

    Public Function CopyDir(ByVal startDir As String, ByVal destination As String) As Boolean
        CopyDir(New DirectoryInfo(startDir), destination)
    End Function

    Public Function CopyDir(ByVal startDir As DirectoryInfo, ByVal destination As String) As Boolean

        If startDir.Exists Then

            If destination.Substring(destination.Length - 1, 1) = "\" Then
                destination = Mid(destination, 1, destination.Length - 1)
            End If

            Try

                Dim oDir As DirectoryInfo
                Dim oFile As FileInfo

                For Each oDir In startDir.GetDirectories()
                    CopyDir(oDir, destination & "\" & oDir.Name)
                Next

                Directory.CreateDirectory(destination)

                For Each oFile In startDir.GetFiles()
                    oFile.CopyTo(destination & "\" & oFile.Name)
                Next

            Catch
                Return False
            End Try

        Else
            Return False
        End If

    End Function

    Public Function EmptyDir(ByVal sPath As String) As Boolean

        If Directory.Exists(sPath) Then

            Try

                Dim root As New DirectoryInfo(sPath)
                Dim oDir As DirectoryInfo
                Dim oFile As FileInfo

                For Each oFile In root.GetFiles()
                    oFile.Delete()
                Next

                For Each oDir In root.GetDirectories()
                    EmptyDir(oDir.FullName)
                    oDir.Delete()
                Next

            Catch
                Return False
            End Try

        Else
            Return False
        End If

    End Function

    Public Function StartProcess(ByVal filename As String, Optional ByVal parameters As String = "", Optional ByVal windowStyle As Diagnostics.ProcessWindowStyle = ProcessWindowStyle.Normal, Optional ByVal priority As ProcessPriorityClass = ProcessPriorityClass.Normal) As Integer

        Dim p As New Process
        Dim exitCode As Integer = 0

        If File.Exists(filename) Then

            p.StartInfo.FileName = filename

            Try

                p.StartInfo.UseShellExecute = True
                p.StartInfo.Arguments = parameters
                p.StartInfo.WindowStyle = windowStyle
                'p.PriorityClass = priority
                p.Start()
                exitCode = p.Id

                p.WaitForExit()
                'MsgBox(p.StandardOutput.ReadToEnd)
                p.Close()

                Return exitCode

            Catch ex As Exception
                Return 0
            End Try

        Else
            Return -1
        End If

    End Function

    Public Function qq(ByVal str As String)
        Return Chr(34) & str & Chr(34)
    End Function

    Public Function FirstCapped(ByVal str As String) As String

        Dim curWord As String
        Dim curChar As Integer
        Dim retVal As String = ""

        str = str.Trim()

        For Each curWord In Split(str)

            curChar = 0

            While Not IsAlpha(curWord.Substring(curChar, 1))
                retVal = retVal & curWord.Substring(curChar, 1)
                curChar += 1
            End While

            retVal = retVal & UCase(curWord.Substring(curChar, 1)) & curWord.Substring(curChar + 1) & " "

        Next

        Return Mid(retVal, 1, Len(retVal) - 1)

    End Function

    Public Function IsAlpha(ByVal ch As String) As Boolean

        ch = ch.Substring(0, 1)

        If (ch >= "A" And ch <= "Z") Or (ch >= "a" And ch <= "z") Then
            Return True
        Else
            Return False
        End If

    End Function

    Public Sub RemoveDir(ByVal dirPath As String, Optional ByVal subDirs As Boolean = True)

        If Directory.Exists(dirPath) Then
            Directory.Delete(dirPath, subDirs)
        End If

    End Sub

    Public Function FileSize(ByVal sFile As String, Optional ByVal maxSize As FileSizes = FileSizes.Terabytes) As String

        If File.Exists(sFile) Then
            Return FileSize(New FileInfo(sFile).Length, maxSize)
        Else
            Return Nothing
        End If

    End Function

    Public Function FileLength(ByVal sFile As String) As Long

        If File.Exists(sFile) Then

            Dim oFile As New FileInfo(sFile)

            Return oFile.Length

        Else
            Return 0
        End If

    End Function

    Public Function FileSize(ByVal size As Double, Optional ByVal maxSize As FileSizes = FileSizes.Terabytes, Optional ByVal decimalFormat As String = ".00", Optional ByVal spacer As String = " ") As String

        Dim sizeIndex As Integer = 0

        While sizeIndex < maxSize AndAlso ((Math.Abs(Int(size)) / 1024) >= 1)
            size /= 1024
            sizeIndex += 1
        End While

        Return Format(size, "###,###,###,###,###,##0" & decimalFormat) & spacer & byteSizes.GetValue(sizeIndex)

    End Function

    Public Function GetDirName(ByVal fullPath As String, Optional ByVal delimeter As String = "\") As String

        If fullPath.Contains(delimeter) Then

            Dim arr As String() = fullPath.Split(delimeter)
            Return arr.GetValue(arr.Length - delimeter.Length)

        Else
            Return fullPath
        End If

    End Function

    Public Function GetDirPath(ByVal fullPath As String, Optional ByVal delimeter As String = "\") As String

        If fullPath.Contains(delimeter) Then

            Dim arr As String() = fullPath.Split(delimeter)
            Dim xxx As Integer
            Dim dirPath As String = arr.GetValue(0)

            For xxx = 1 To arr.Length - 2
                dirPath += "\" & arr.GetValue(xxx)
            Next

            Return dirPath

        Else
            Return fullPath
        End If

    End Function

    Public Function GetExtention(ByVal filename As String, Optional ByVal includeDot As Boolean = False) As String

        Dim arr As String() = filename.Split(".")

        If includeDot Then
            Return "." & arr.GetValue(arr.Length - 1)
        Else
            Return arr.GetValue(arr.Length - 1)
        End If

    End Function

    Public Function RemoveExtention(ByVal filename As String) As String

        Try

            If filename.Contains(".") Then

                Dim arr As String() = filename.Split(".")
                Dim xxx As Integer

                filename = ""

                For xxx = 0 To arr.Length - 2
                    filename += arr.GetValue(xxx) & "."
                Next

                Return filename.Substring(0, filename.Length - 1)

            Else
                Return filename
            End If

        Catch
            Return filename
        End Try

    End Function

    Function getControlFromName(ByRef containerObj As Object, ByVal name As String, Optional ByVal recursive As Boolean = False) As Control
        Try
            Dim tempCtrl As Control
            For Each tempCtrl In containerObj.Controls
                If tempCtrl.Name.ToUpper.Trim = name.ToUpper.Trim Then
                    Return tempCtrl
                End If
            Next tempCtrl
            If recursive Then
                For Each tempCtrl In containerObj.Controls
                    Return getControlFromName(containerObj, name, True)
                Next
                Return Nothing
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

#Region "jpegs"
    'Thanks to Neil Crosby.
    'http://www.vb-helper.com/howto_net_optimize_jpg.html
    Public Sub SaveJpeg(ByVal image As Image, ByVal newFileName As String, ByVal compression As Long)
        Dim eps As EncoderParameters = New EncoderParameters(1)
        eps.Param(0) = New EncoderParameter(Encoder.Quality, compression)
        Dim ici As ImageCodecInfo = GetEncoderInfo("image/jpeg")
        image.Save(newFileName, ici, eps)
        image.Dispose()
        eps.Dispose()
    End Sub

    Public Sub SaveJpeg(ByVal imageFile As String, ByVal newFileName As String, ByVal compression As Long)
        If File.Exists(imageFile) Then
            SaveJpeg(New Bitmap(imageFile), newFileName, compression)
        End If
    End Sub

    Public Function GetEncoderInfo(ByVal mimeType As String) As ImageCodecInfo
        Dim j As Integer
        Dim encoders As ImageCodecInfo()
        encoders = ImageCodecInfo.GetImageEncoders()
        For j = 0 To encoders.Length
            If encoders(j).MimeType = mimeType Then
                Return encoders(j)
            End If
        Next j
        Return Nothing
    End Function
#End Region

End Module

Module Notes

    'operators overloads:
    '--------------------
    'Public Shared Operator +(object1, object2) As whatever

    'End Operator

End Module

'Public Class TrueThread

'    Public Delegate Sub EventN(ByVal paramN As Object) 'callback function on some form.
'    'enter all other events here.

'    Private _frm As Form
'    Private _eventN As EventN
'    'enter all other events here.
'    Private _syncObj As System.ComponentModel.ISynchronizeInvoke

'    Public Sub New(ByVal frm As Form, ByVal event1 As Event1) 'enter other events as needed.
'        _frm = frm
'        _event1 = event1
'        _syncObj = frm
'    End Sub

'    Public Sub StartEvent1()

'        _running = True

'        ThreadEvent1()

'        _running = False

'        Dim args(0) As Object
'        args(0) = "param1"
'        _syncObj.Invoke(_event1, args)

'    End Sub

'    Private Sub ThreadEvent1()
'        'do something in the running thread.
'    End Sub

'End Class

Public Class ListViewComparer

    Implements IComparer

    Private m_ColumnNumber As Integer
    Private m_SortOrder As SortOrder
    Private _useTag As Boolean

    Public Sub New(ByVal columnNumber As Integer, ByVal sortOrder As SortOrder, Optional ByVal useTag As Boolean = False)
        m_ColumnNumber = columnNumber
        m_SortOrder = sortOrder
        _useTag = useTag
    End Sub

    ' Compare the items in the appropriate column
    ' for objects x and y.
    Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare

        Dim item_x As ListViewItem = DirectCast(x, ListViewItem)
        Dim item_y As ListViewItem = DirectCast(y, ListViewItem)
        Dim string_x As String
        Dim string_y As String

        If _useTag Then

            If item_x.SubItems.Count <= m_ColumnNumber Then
                string_x = ""
            Else
                string_x = item_x.SubItems(m_ColumnNumber).Tag.ToString
            End If

            If item_y.SubItems.Count <= m_ColumnNumber Then
                string_y = ""
            Else
                string_y = item_y.SubItems(m_ColumnNumber).Tag.ToString
            End If

        Else

            ' Get the sub-item values.
            If item_x.SubItems.Count <= m_ColumnNumber Then
                string_x = ""
            Else
                string_x = item_x.SubItems(m_ColumnNumber).Text
            End If

            If item_y.SubItems.Count <= m_ColumnNumber Then
                string_y = ""
            Else
                string_y = item_y.SubItems(m_ColumnNumber).Text
            End If

        End If

        ' Compare them.
        If m_SortOrder = SortOrder.Ascending Then

            If IsNumeric(string_x) And IsNumeric(string_y) Then
                Return Val(string_x).CompareTo(Val(string_y))
            Else
                Return String.Compare(string_x, string_y)
            End If

        Else

            If IsNumeric(string_x) And IsNumeric(string_y) Then
                Return Val(string_y).CompareTo(Val(string_x))
            Else
                Return String.Compare(string_y, string_x)
            End If

        End If

    End Function

End Class

Public Class Icons

    '=====================================================================================
    '  clsIcon
    '  class to work with icons
    '=====================================================================================
    '  Created By: Marc Cramer
    '  Published Date: 12/31/2002
    '  Legal Copyright: Marc Cramer © 12/31/2002
    '=====================================================================================
    '  Adapted From...
    '  Author: spotchannel (spotchannel@hotmail.com)
    '  Website: forum post at http://www.devcity.net/forums/topic.asp?tid=7422
    '=====================================================================================

    '=====================================================================================
    ' Enumerations
    '=====================================================================================
    <Flags()> Private Enum SHGFI
        SmallIcon = &H1
        LargeIcon = &H0
        Icon = &H100
        DisplayName = &H200
        Typename = &H400
        SysIconIndex = &H4000
        UseFileAttributes = &H10
    End Enum

    Public Enum IconSize
        SmallIcon = 1
        LargeIcon = 0
    End Enum

    '=====================================================================================
    ' Structures
    '=====================================================================================
    <StructLayout(LayoutKind.Sequential)> _
    Private Structure SHFILEINFO
        Public hIcon As IntPtr
        Public iIcon As Integer
        Public dwAttributes As Integer
        <MarshalAs(UnmanagedType.LPStr, SizeConst:=260)> Public szDisplayName As String
        <MarshalAs(UnmanagedType.LPStr, SizeConst:=80)> Public szTypeName As String

        Public Sub New(ByVal B As Boolean)
            hIcon = IntPtr.Zero
            iIcon = 0
            dwAttributes = 0
            szDisplayName = vbNullString
            szTypeName = vbNullString
        End Sub
    End Structure

    '=====================================================================================
    ' API Calls
    '=====================================================================================
    Private Declare Auto Function SHGetFileInfo Lib "shell32" (ByVal pszPath As String, ByVal dwFileAttributes As Integer, ByRef psfi As SHFILEINFO, ByVal cbFileInfo As Integer, ByVal uFlagsn As SHGFI) As Integer

    '=====================================================================================
    ' Functions and Procedures...
    '=====================================================================================
    Public Shared Function GetDefaultIcon(ByVal Path As String, Optional ByVal IconSize As IconSize = IconSize.SmallIcon, Optional ByVal SaveIconPath As String = "") As Icon
        Dim info As New SHFILEINFO(True)
        Dim cbSizeInfo As Integer = Marshal.SizeOf(info)
        Dim flags As SHGFI = SHGFI.Icon Or SHGFI.UseFileAttributes
        flags = flags + IconSize
        SHGetFileInfo(Path, 256, info, cbSizeInfo, flags)
        GetDefaultIcon = Icon.FromHandle(info.hIcon)
        If SaveIconPath <> "" Then
            Dim FileStream As New IO.FileStream(SaveIconPath, IO.FileMode.Create)
            GetDefaultIcon.Save(FileStream)
            FileStream.Close()
        End If
    End Function      'GetDefaultIcon(ByVal Path As String, Optional ByVal IconSize As IconSize = IconSize.SmallIcon, Optional ByVal SaveIconPath As String = "") As Icon
    '=====================================================================================
    Public Shared Function ImageToIcon(ByVal SourceImage As Image) As Icon
        ' converts an image into an icon
        Dim TempBitmap As New Bitmap(SourceImage)
        ImageToIcon = Icon.FromHandle(TempBitmap.GetHicon())
        TempBitmap.Dispose()
    End Function      'ImageToIcon(ByVal SourceImage As Image) As Icon
    '=====================================================================================

End Class

Public Class MyThreads

    'How to handle Delegates:
    '------------------------
    'Public Delegate Sub SyncSub(add parameters)
    'Private/Public _syncSub as SyncSub
    Private _params As New ArrayList()
    Private _syncParams As New ArrayList()
    Private _thread As Thread
    Private _callObj As Object
    Private _startTime As Date
    Private _endTime As Date
    Private _retVal As Object
    Private _syncObj As System.ComponentModel.ISynchronizeInvoke

    Private Sub DoStart(ByVal func As Object)

        Try
            _retVal = _callObj.GetType().GetMethod(func).Invoke(_callObj, _params.ToArray)
        Catch ex As Exception

            If NotNothing(ex.InnerException) Then
                Throw ex.InnerException
            Else
                Throw ex
            End If

        End Try

        _params.Clear()

        _endTime = Now

    End Sub

    Public Property CallObj() As Object
        Get
            Return _callObj
        End Get
        Set(ByVal value As Object)
            _callObj = value
        End Set
    End Property

    Public Sub Start(ByVal functionName As String)

        _thread = New Thread(AddressOf DoStart)
        _startTime = Now
        _endTime = Nothing

        _thread.Start(functionName)

    End Sub

    Public Sub Start(ByVal callObj As Object, ByVal functionName As String)

        _callObj = callObj
        _thread = New Thread(AddressOf DoStart)
        _startTime = Now
        _endTime = Nothing

        _thread.Start(functionName)

    End Sub

    Public Sub Start(ByVal callObj As [Delegate], Optional ByVal syncObj As System.ComponentModel.ISynchronizeInvoke = Nothing)

        If NotNothing(_syncObj) Then
            _retVal = _syncObj.Invoke(callObj, _syncParams.ToArray)
        End If

        _syncParams.Clear()

    End Sub

    Public Sub AddSyncParameter(ByVal parameter As Object)
        _syncParams.Add(parameter)
    End Sub

    Public Sub AddParameter(ByVal parameter As Object)
        _params.Add(parameter)
    End Sub

    Public Sub New(Optional ByVal syncObj As System.ComponentModel.ISynchronizeInvoke = Nothing)
        Control.CheckForIllegalCrossThreadCalls = False
        _thread = Nothing
        _callObj = Nothing
        _retVal = Nothing
        _syncObj = syncObj
    End Sub

    Public ReadOnly Property ReturnValue() As Object
        Get
            Return _retVal
        End Get
    End Property

    Public ReadOnly Property TimeRunning() As TimeSpan
        Get
            If _endTime > Date.MinValue AndAlso _startTime > Date.MinValue Then
                Return (_endTime - _startTime)
            ElseIf _startTime > Date.MinValue Then
                Return (Now - _startTime)
            End If
        End Get
    End Property

    Public ReadOnly Property IsRunning() As Boolean
        Get
            Return (NotNothing(_thread) AndAlso _thread.IsAlive)
        End Get
    End Property

End Class
