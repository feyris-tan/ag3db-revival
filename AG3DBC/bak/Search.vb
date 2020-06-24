Imports System.Threading
Imports System.IO
Imports System.Xml
Imports Microsoft.Win32

Public Enum SearchFieldType
    Null = 0
    Int = 1
    Float = 2
    Bool = 3
    Str = 4
    DateTime = 5
    Tags = 6
End Enum

Public Class SearchField

    Public WILD_START_POS As Integer
    Public WILD_SIZE As New Size(15, 14)

    Private _from As Control
    Private _to As Control
    Private _wildFrom As CheckBox
    Private _wildTo As CheckBox
    Private _toValue As Object
    Private _fromValue As Object
    Private _tagsMatchAny As Boolean
    Private _ignore As Boolean
    Private _now As Date
    Private _type As SearchFieldType
    Private _frmSearch As Form

    Public Sub New(ByVal frmSearch As Form, ByVal fieldIndex As Integer, ByVal fromField As Control, Optional ByVal toField As Control = Nothing)

        _type = fromField.Tag
        _to = toField
        _from = fromField
        _frmSearch = frmSearch
        WILD_START_POS = frmSearch.Width - 50

        If _type = SearchFieldType.DateTime Then
            Dim dt As DateTimePicker = toField
            _now = dt.Value
        End If

        MakeWilds(fieldIndex)

    End Sub

    Private Sub MakeWilds(ByVal index As Integer)

        Dim lblWild As Label = _frmSearch.Controls("lblWild")
        Dim lblFrom As Label = _frmSearch.Controls("lblWildFrom")
        Dim lblTo As Label = _frmSearch.Controls("lblWildTo")

        _wildFrom = New CheckBox()
        AddHandler _wildFrom.CheckedChanged, AddressOf CheckBox_CheckedChanged
        _wildFrom.Name = _from.Name & "Wild"
        _wildFrom.Text = ""
        _wildFrom.Size = WILD_SIZE
        _wildFrom.Tag = _from
        _wildFrom.Checked = True
        _wildFrom.Location = New Point(lblFrom.Location.X + ((lblFrom.Width - _wildFrom.Width) / 2) + 2, _from.Location.Y + ((_from.Height - _wildFrom.Height) / 2))

        _frmSearch.Controls.Add(_wildFrom)
        _wildFrom.Show()

        If NotNothing(_to) Then

            _wildTo = New CheckBox()
            AddHandler _wildTo.CheckedChanged, AddressOf CheckBox_CheckedChanged
            _wildTo.Name = _to.Name & "Wild"
            _wildTo.Text = ""
            _wildTo.Size = WILD_SIZE
            _wildTo.Tag = _to
            _wildTo.Checked = True
            _wildTo.Location = New Point(lblTo.Location.X + ((lblTo.Width - _wildTo.Width) / 2) + 2, _wildFrom.Location.Y)

            _frmSearch.Controls.Add(_wildTo)
            _wildTo.Show()

        Else
            _wildTo = Nothing
        End If

    End Sub

    Private Function GetValue(ByVal field As Control) As Object

        If NotNothing(field) AndAlso Not GetWild(field).Checked Then

            Select Case _type

                Case SearchFieldType.DateTime

                    Dim dt As DateTimePicker = field

                    Return GetSTDDateTime(dt.Value)

                Case SearchFieldType.Bool

                    Dim cbo As ComboBox = field

                    Return cbo.SelectedItem

                Case SearchFieldType.Int

                    Dim num As NumericUpDown = field

                    Return num.Value

                Case SearchFieldType.Float

                    Dim num As NumericUpDown = field

                    Return num.Value

                Case SearchFieldType.Str

                    Return field.Text

                Case SearchFieldType.Tags

                    Return New ArrayList(Split(field.Text, " "))

                Case Else : Return Nothing

            End Select

        Else
            Return Nothing
        End If

    End Function

    Public Sub SetFieldValues()

        _fromValue = GetValue(_from)
        _toValue = GetValue(_to)

        Dim matchAny As RadioButton

        If _type = SearchFieldType.Tags AndAlso NotNothing(_frmSearch.Controls(_from.Name & "MatchAny")) Then

            matchAny = _frmSearch.Controls(_from.Name & "MatchAny")
            _tagsMatchAny = matchAny.Checked

        End If

        matchAny = _frmSearch.Controls("matchTable").Controls("matchAny")

        If matchAny.Checked AndAlso _wildFrom.Checked Then

            If IsNothing(_wildTo) Then
                _ignore = True
            Else
                _ignore = _wildTo.Checked
            End If

        Else
            _ignore = False
        End If

    End Sub

    Private Function GetWild(ByVal field As Control) As CheckBox

        If field.Name.EndsWith("To") Then
            Return _wildTo
        ElseIf field.Name.EndsWith("From") Then
            Return _wildFrom
        Else
            Return Nothing
        End If

    End Function

    Private Sub CheckBox_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        sender.tag.enabled = Not sender.checked
    End Sub

    Public Sub ResetFieldValue()

        Select Case _type
            Case SearchFieldType.Bool
                Dim cbo As ComboBox = _from
                cbo.SelectedIndex = 0
            Case SearchFieldType.DateTime
                Dim dt As DateTimePicker = _from
                dt.Value = _now
                dt = _to
                dt.Value = _now
            Case SearchFieldType.Float
                Dim num As NumericUpDown = _from
                num.Value = 1
                num = _to
                num.Value = 1
            Case SearchFieldType.Int
                Dim num As NumericUpDown = _from
                num.Value = 1
                num = _to
                num.Value = 1
            Case SearchFieldType.Str
                _from.Text = ""
            Case SearchFieldType.Tags
                Dim opt As RadioButton = _frmSearch.Controls(_from.Name & "MatchAny")
                _from.Text = ""
                opt.Checked = True
        End Select

        _wildFrom.Checked = True

        If NotNothing(_wildTo) Then
            _wildTo.Checked = True
        End If

    End Sub

    Public ReadOnly Property Ignore() As Boolean
        Get
            Return _ignore
        End Get
    End Property

    Public ReadOnly Property WildTo() As Boolean
        Get
            Return _wildTo.Checked
        End Get
    End Property

    Public ReadOnly Property WildFrom() As Boolean
        Get
            Return _wildFrom.Checked
        End Get
    End Property

    Public ReadOnly Property TagsMatchAny() As Boolean
        Get
            Return _tagsMatchAny
        End Get
    End Property

    Public ReadOnly Property ToValue() As Object
        Get
            Return _toValue
        End Get
    End Property

    Public ReadOnly Property FromValue() As Object
        Get
            Return _fromValue
        End Get
    End Property

    Public ReadOnly Property Value() As Object
        Get
            Return _fromValue
        End Get
    End Property

    Public ReadOnly Property Ranged() As Boolean
        Get
            Return NotNothing(_to)
        End Get
    End Property

    Public ReadOnly Property FieldType() As SearchFieldType
        Get
            Return _type
        End Get
    End Property

End Class

Public Class Search2

    Public Const DATE_FORMAT As String = "yyyy-MM-dd @ HH:mm:ss"
    Public Const SPACE_BETWEEN As Integer = 25
    Public Const MARGIN As Integer = 20
    Public DEFAULT_SIZE As New Size(335, 800)
    Public BUTTON_SIZE As New Size((DEFAULT_SIZE.Width / 2) - (MARGIN), 25)
    Public NUMBER_FIELD_SIZE As New Size(55, 20)
    Public STRING_FIELD_SIZE As New Size(150, 20)
    Public DATE_FIELD_SIZE As New Size(138, 20)
    Public BOOL_FIELD_SIZE As New Size(100, 1)

    Private _thread As Thread
    Private _priority As Threading.ThreadPriority
    Private _frmMain As Form
    Private _lblTo As Label
    Private _list As ListView
    Private _searchFields As New ArrayList
    Private _mainFrmPos As Point
    Private _nextStartPos As Integer
    Private _lblResults As Label
    Private _lblWild As Label
    Private _lblWildFrom As Label
    Private _lblWildTo As Label
    Private _matchAll As RadioButton
    Private _matchAny As RadioButton
    Private _matchTable As TableLayoutPanel
    Private WithEvents _frmSearch As Form = Nothing
    Private WithEvents _cmdSearch As Button
    Private WithEvents _cmdClose As Button
    Private WithEvents _cmdResetList As Button
    Private WithEvents _cmdResetSearch As Button

    Public Sub GenerateSearch(ByVal list As ListView)
        _list = list
        MakeForm(list)
    End Sub

    Private Sub MakeForm(ByVal listView As Object)

        Dim list As ListView = listView
        Dim widthDiff As Integer = Screen.PrimaryScreen.WorkingArea.Width - (_frmMain.Location.X + _frmMain.Size.Width + DEFAULT_SIZE.Width) - 5
        Dim i As Integer
        Dim frmHeight As Integer = 0

        _frmSearch = New Form()

        MakeFormLabels()

        For i = 0 To list.Columns.Count - 1
            MakeField(list.Columns(i), i)
        Next

        If widthDiff < 0 Then
            _frmMain.Location = New Point(_frmMain.Location.X + widthDiff, _frmMain.Location.Y)
        End If

        _frmSearch.Icon = _frmMain.Icon
        _frmSearch.Text = "Search " & list.Parent.Text
        _frmSearch.Size = DEFAULT_SIZE
        _frmSearch.FormBorderStyle = FormBorderStyle.FixedSingle
        _frmSearch.MaximizeBox = False
        _frmSearch.Show(_frmMain)
        _frmSearch.Location = New Point(_frmMain.Location.X + _frmMain.Size.Width, _frmMain.Location.Y)
        _matchTable = New TableLayoutPanel
        _matchTable.Name = "matchTable"
        _matchTable.Location = New Point(0, _nextStartPos + SPACE_BETWEEN)
        _matchTable.CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        _matchTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        _matchTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        _matchTable.Size = New Size(_frmSearch.Width, 35)
        _matchAny = New RadioButton()
        _matchAny.Name = "matchAny"
        _matchAny.Anchor = AnchorStyles.Right
        _matchAny.Size = New Size(105, 20)
        _matchAny.Text = "Match Any Field"
        _matchAny.Checked = True
        '       _matchAny.Location = New Point(MARGIN, (SPACE_BETWEEN * 1.5) + _nextStartPos)
        _matchAll = New RadioButton()
        _matchAll.Name = "matchAll"
        _matchAll.Anchor = AnchorStyles.Left
        _matchAll.Size = New Size(100, 20)
        _matchAll.Text = "Match All Fields"
        _matchAll.Checked = False
        '        _matchAll.Location = New Point(_matchAny.Location.X + _matchAny.Width, (SPACE_BETWEEN * 1.5) + _nextStartPos)
        _nextStartPos = _matchTable.Location.Y
        _cmdSearch = MakeCmd("Search", MARGIN, _nextStartPos + (SPACE_BETWEEN * 1.5))
        _cmdResetList = MakeCmd("Reset List", MARGIN + _cmdSearch.Width, _cmdSearch.Location.Y)
        _cmdResetSearch = MakeCmd("Reset Search", MARGIN, _cmdSearch.Location.Y + _cmdResetList.Height)
        _cmdClose = MakeCmd("Close", MARGIN + _cmdResetSearch.Width, _cmdResetSearch.Location.Y)
        _frmSearch.Height = _cmdClose.Location.Y + _cmdClose.Height + (MARGIN * 2)

        _matchTable.Controls.Add(_matchAny, 0, 0)
        _matchTable.Controls.Add(_matchAll, 1, 0)
        _frmSearch.Controls.Add(_matchTable)


    End Sub

    Private Sub MakeField(ByVal field As Windows.Forms.ColumnHeader, ByVal index As Integer)

        Dim obj As Object = Nothing
        Dim obj2 As Object = Nothing
        Dim l As Label = MakeLblSearchField(field.Text & ":", index)
        Dim lblTo As Label

        If field.Tag.tolower = "date" Then

            obj = New DateTimePicker()
            obj.Format = DateTimePickerFormat.Custom
            obj.CustomFormat = DATE_FORMAT
            obj.Size = DATE_FIELD_SIZE
            obj.tag = SearchFieldType.DateTime
            obj.value = Now
            obj.Location = New Point(MARGIN + l.Size.Width, SPACE_BETWEEN + _nextStartPos)
            obj2 = New DateTimePicker
            obj2.Name = "srch" & index & "To"
            obj2.Format = DateTimePickerFormat.Custom
            obj2.CustomFormat = DATE_FORMAT
            obj2.Size = DATE_FIELD_SIZE
            obj2.Tag = SearchFieldType.DateTime
            obj2.Value = Now
            lblTo = MakeLblTo()
            lblTo.Location = New Point(obj.location.x + (obj.width - lblTo.Width) / 2, obj.location.y + obj.height + ((SPACE_BETWEEN - lblTo.Height) / 2) + 3)
            obj2.Location = New Point(obj.location.x, obj.location.Y + obj.height + SPACE_BETWEEN)
            _nextStartPos = obj2.Location.Y

            _frmSearch.Controls.Add(obj2)
            _frmSearch.Controls.Add(lblTo)
            obj2.Show()
            lblTo.Show()

        ElseIf field.Tag.tolower = "int" Or field.Tag.tolower = "float" Then

            obj = New NumericUpDown()
            obj2 = New NumericUpDown()

            Select Case field.Tag.tolower
                Case "int"
                    obj.decimalplaces = 0
                    obj.increment = 1
                    obj.tag = SearchFieldType.Int
                    obj2.decimalplaces = 0
                    obj2.increment = 1
                    obj2.tag = SearchFieldType.Int
                Case "float"
                    obj.decimalplaces = 2
                    obj.increment = 0.01
                    obj.tag = SearchFieldType.Float
                    obj2.decimalplaces = 2
                    obj2.increment = 0.01
                    obj2.tag = SearchFieldType.Float
            End Select

            obj.Size = NUMBER_FIELD_SIZE
            obj.maximum = Integer.MaxValue
            obj.value = 1
            obj.Location = New Point(MARGIN + l.Size.Width, SPACE_BETWEEN + _nextStartPos)
            lblTo = MakeLblTo()
            lblTo.Location = New Point(obj.location.x + obj.size.width + 8, l.Location.Y)
            obj2.Name = "srch" & index & "To"
            obj2.Size = NUMBER_FIELD_SIZE
            obj2.Location = New Point(lblTo.Location.X + lblTo.Size.Width + 8, obj.location.Y)
            obj2.maximum = Integer.MaxValue
            obj2.value = 1
            _nextStartPos = obj2.Location.Y

            _frmSearch.Controls.Add(obj2)
            _frmSearch.Controls.Add(lblTo)
            obj2.Show()
            lblTo.Show()

        ElseIf field.Tag.tolower = "string" Then

            obj = New TextBox()
            obj.Size = STRING_FIELD_SIZE
            obj.tag = SearchFieldType.Str
            obj.Location = New Point(MARGIN + l.Size.Width, SPACE_BETWEEN + _nextStartPos)
            _nextStartPos = obj.Location.Y

        ElseIf field.Tag.tolower = "bool" Then

            obj = New ComboBox
            obj.DropDownStyle = ComboBoxStyle.DropDownList
            obj.Items.Add("Yes")
            obj.Items.Add("No")
            obj.selectedindex = 0
            obj.Size = BOOL_FIELD_SIZE
            obj.Tag = SearchFieldType.Bool
            obj.Location = New Point(MARGIN + l.Size.Width, SPACE_BETWEEN + _nextStartPos)
            _nextStartPos = obj.Location.Y

        ElseIf field.Tag.tolower = "tags" Then

            Dim matchAll As New RadioButton()
            Dim matchAny As New RadioButton()

            obj = New TextBox()
            obj.Size = STRING_FIELD_SIZE
            obj.tag = SearchFieldType.Tags
            obj.Location = New Point(MARGIN + l.Size.Width, SPACE_BETWEEN + _nextStartPos)
            matchAny.Name = "srch" & index & "FromMatchAny"
            matchAny.Size = New Size(80, 20)
            matchAny.Text = "Match Any"
            matchAny.Checked = True
            matchAny.Location = New Point(obj.location.x, obj.location.y + obj.height)
            matchAll.Name = "srch" & index & "MatchAll"
            matchAll.Size = New Size(75, 20)
            matchAll.Text = "Match All"
            matchAll.Checked = False
            matchAll.Location = New Point(matchAny.Location.X + matchAny.Width, obj.location.y + obj.height)
            _nextStartPos = matchAny.Location.Y

            _frmSearch.Controls.Add(matchAny)
            _frmSearch.Controls.Add(matchAll)
            matchAny.Show()
            matchAll.Show()

        End If

        obj.Name = "srch" & index & "From"

        _frmSearch.Controls.Add(obj)
        obj.Show()
        _searchFields.Add(New SearchField(_frmSearch, index, obj, obj2))

    End Sub

    Private Sub MakeFormLabels()

        _lblResults = New Label()
        _lblResults.Size = New Size(200, 13)
        _lblResults.TextAlign = ContentAlignment.TopLeft
        _lblResults.Font = New Font(_lblResults.Font, FontStyle.Bold)
        _lblResults.Text = "Displaying " & _list.Items.Count & " of " & _list.Items.Count
        _lblWild = New Label()
        _lblWild.Name = "lblWild"
        _lblWild.Text = "Wild"
        _lblWild.Size = New Size(28, 13)
        _lblWildTo = New Label()
        _lblWildTo.Name = "lblWildTo"
        _lblWildTo.Text = "To"
        _lblWildTo.Size = New Size(20, 13)
        _lblWildTo.Location = New Point(DEFAULT_SIZE.Width - _lblWildTo.Width - MARGIN, 10 + _lblWild.Size.Height)
        _lblWildFrom = New Label()
        _lblWildFrom.Name = "lblWildFrom"
        _lblWildFrom.Text = "From"
        _lblWildFrom.Size = New Size(30, 13)
        _lblWildFrom.Location = New Point(_lblWildTo.Location.X - _lblWildFrom.Width, _lblWildTo.Location.Y)
        _lblWild.Location = New Point(_lblWildFrom.Location.X + (((_lblWildFrom.Width + _lblWildTo.Width) - _lblWild.Width) / 2), 10)
        _lblResults.Location = New Point(5, ((_lblWild.Location.Y + _lblWildFrom.Location.Y) - _lblResults.Location.Y) / 2)
        _nextStartPos = _lblWildFrom.Location.Y

        _frmSearch.Controls.Add(_lblResults)
        _frmSearch.Controls.Add(_lblWild)
        _frmSearch.Controls.Add(_lblWildFrom)
        _frmSearch.Controls.Add(_lblWildTo)

    End Sub

    Private Sub CmdResetSearch_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _cmdResetSearch.Click

        If Not threads.IsRunning Then

            For Each field As SearchField In _searchFields
                field.ResetFieldValue()
            Next

            _matchAny.Checked = True

        End If

    End Sub

    Private Sub CmdClose_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _cmdClose.Click
        If NotNothing(_frmSearch) AndAlso Not threads.IsRunning Then
            _searchFields.Clear()
            CmdResetList_Click(Nothing, Nothing)
            _frmSearch.Dispose()
            _frmSearch = Nothing
            _list = Nothing
            _frmMain.Location = _mainFrmPos
            _nextStartPos = 0
            _frmMain.Show()
        End If
    End Sub

    Private Sub CmdSearch_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _cmdSearch.Click

        If Not threads.IsRunning Then

            For Each field As SearchField In _searchFields
                field.SetFieldValues()
            Next

            Dim t As New Thread(AddressOf DoSearch)
            t.Priority = Threading.ThreadPriority.Highest
            t.Start()

        End If

    End Sub

    Private Sub CmdResetList_Click(ByVal sender As Object, ByVal e As EventArgs) Handles _cmdResetList.Click

        If NotNothing(_list) Then

            _list.Visible = False

            If Not threads.IsRunning Then

                _list.Items.Clear()

                For Each item As ListViewItem In _list.Tag.items
                    _list.Items.Add(item.Clone)
                Next

            End If

            _list.Visible = True
            _lblResults.Text = "Displaying " & _list.Items.Count & " of " & _list.Tag.items.count

        End If

    End Sub

    Public Sub DoSearch()

        Dim i As Integer
        Dim add As Boolean

        Mes("Searching " & _list.Parent.Text & "...")
        _list.Items.Clear()

        _list.Visible = False

        If _matchAll.Checked Then

            For Each item As ListViewItem In _list.Tag.items

                add = True

                For i = 0 To _list.Columns.Count - 1

                    If Not CheckField(_searchFields(i), item.SubItems(i).Text) Then
                        add = False : Exit For
                    End If

                Next

                If add Then
                    _list.Items.Add(item.Clone)
                End If

            Next

        Else

            For Each item As ListViewItem In _list.Tag.items

                add = False

                For i = 0 To _list.Columns.Count - 1

                    If CheckField(_searchFields(i), item.SubItems(i).Text) Then
                        add = True : Exit For
                    End If

                Next

                If add Then
                    _list.Items.Add(item.Clone)
                End If

            Next

        End If

        Mes("Search finished with " & Plural("result", _list.Items.Count) & " from " & Plural("list item", _list.Tag.items.count) & ".")

        _list.Visible = True
        _lblResults.Text = "Displaying " & _list.Items.Count & " of " & _list.Tag.items.count

    End Sub

    Private Function CheckField(ByVal field As SearchField, ByVal value As Object) As Boolean

        If Not field.Ignore Then

            If field.FieldType = SearchFieldType.Int Or field.FieldType = SearchFieldType.Float Then

                If NotNothing(field.FromValue) AndAlso Val(value) < field.FromValue Then
                    Return False
                ElseIf NotNothing(field.ToValue) AndAlso Val(value) > field.ToValue Then
                    Return False
                End If

            ElseIf field.FieldType = SearchFieldType.Bool AndAlso NotNothing(field.Value) AndAlso Not value.ToLower.Contains(field.Value.tolower) Then
                Return False
            ElseIf field.FieldType = SearchFieldType.DateTime Then

                If NotNothing(field.FromValue) AndAlso value < field.FromValue Then
                    Return False
                ElseIf NotNothing(field.ToValue) AndAlso value > field.ToValue Then
                    Return False
                End If

            ElseIf field.FieldType = SearchFieldType.Tags AndAlso NotNothing(field.Value) AndAlso field.Value.count > 0 Then

                If field.TagsMatchAny Then

                    For Each tag As String In field.Value

                        If value.ToLower.Contains(tag.ToLower) Then
                            Return True
                        End If

                    Next

                    Return False

                Else

                    For Each tag As String In field.Value

                        If Not value.ToLower.Contains(tag.ToLower) Then
                            Return False
                        End If

                    Next

                    Return True

                End If

            Else

                If NotNothing(field.Value) AndAlso Not value.ToLower.Contains(field.Value.tolower) Then
                    Return False
                End If

            End If

        Else
            Return False
        End If

        Return True

    End Function

    Public Property ThreadPriority() As ThreadPriority
        Get
            Return _priority
        End Get
        Set(ByVal value As ThreadPriority)
            _priority = value
        End Set
    End Property

    Public ReadOnly Property SearchOpen() As Boolean
        Get
            Return NotNothing(_frmSearch)
        End Get
    End Property

    Public Sub New(ByVal mainForm As Form)
        _thread = Nothing
        _priority = Threading.ThreadPriority.Normal
        _frmMain = mainForm
        _mainFrmPos = mainForm.Location
    End Sub

    Public Sub Focus()

        If NotNothing(_frmSearch) Then
            _frmSearch.Show()
            _frmSearch.Focus()
        End If

    End Sub

    Public Sub CloseSearch()
        CmdClose_Click(Nothing, Nothing)
    End Sub

    Private Function MakeLblTo() As Label

        Dim lbl As New Label()

        lbl.Text = "To"
        lbl.Size = New Size(25, 20)

        Return lbl

    End Function

    Private Function MakeLblSearchField(ByVal text As String, ByVal index As Integer) As Label

        Dim lbl As New Label()

        lbl.Text = text
        lbl.Size = New Size(90, 20)
        lbl.Name = "lbl" & index
        lbl.Location = New Point(MARGIN, SPACE_BETWEEN + _nextStartPos + 3)

        _frmSearch.Controls.Add(lbl)
        lbl.Show()

        Return lbl

    End Function

    Private Function MakeCmd(ByVal text As String, ByVal vPos As Integer, ByVal hPos As Integer) As Button

        Dim cmd As New Button()

        cmd.Text = text
        cmd.Location = New Point(vPos, hPos)
        cmd.Size = BUTTON_SIZE
        _frmSearch.Controls.Add(cmd)
        cmd.Show()

        Return cmd

    End Function

    'Private Sub AddUniqueCbo(ByVal cbo As ComboBox, ByVal value As String)

    '    If Not cbo.Items.Contains(value) Then

    '        If value.ToLower.Substring(0) >= "m" Then

    '            For i As Integer = cbo.Items.Count - 1 To 0 Step -1

    '                If value.ToLower >= cbo.Items(i).ToLower Then
    '                    cbo.Items.Insert(i, value) : Exit For
    '                End If

    '            Next

    '        Else

    '            For i As Integer = 0 To cbo.Items.Count - 1

    '                If value.ToLower <= cbo.Items(i).ToLower Then
    '                    cbo.Items.Insert(i, value) : Exit For
    '                End If

    '            Next

    '        End If

    '        cbo.Items.Add(value)

    '    End If

    'End Sub
End Class

