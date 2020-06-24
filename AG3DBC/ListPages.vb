Imports System.Reflection

Public Class ListPages

    Private _list As ListView
    Private _buttonSize As Size
    Private _pages As Integer
    Private _perPage As Integer
    Private _page As Integer
    Private _callBack As MethodInfo
    Private _frm As Form

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        _buttonSize = New Size(39, 22)
        _perPage = Nothing
        _pages = Nothing
        _list = Nothing

    End Sub

    Public Sub MakePages(ByVal perPage As Integer)

        ClearPages()

        If IsNothing(_list) Then
            Exit Sub
        End If

        _pages = Math.Ceiling(_list.Items.Count / perPage)

        If _pages > 1 Then

            Dim b As Button

            For i As Integer = 0 To _pages - 1

                b = MakeButton(i + 1)
                b.Left = (i * _buttonSize.Width) + 5

                GrpPages.Controls.Add(b)

            Next

        End If

    End Sub

    Private Function MakeButton(ByVal number As Integer) As Button

        Dim b As New Button()

        b.Name = "CmdPage" & number
        b.Size = _buttonSize
        b.Text = number
        b.TextAlign = ContentAlignment.MiddleCenter
        b.Tag = number
        b.Top = ((Me.Height - b.Height) / 2) + 2

        AddHandler b.Click, AddressOf DoPageAction

        Return b

    End Function

    Public Sub ClearPages()

        _pages = Nothing

        For Each ctrl As Control In GrpPages.Controls
            ctrl.Dispose()
        Next

    End Sub

    Private Sub MakeGoto()

    End Sub

    Private Sub DoPageAction(ByVal sender As Object, ByVal e As EventArgs)
        _page = sender.text
        Dim a As New ArrayList
        a.Add(_page)
        _callBack.Invoke(_frm, a.ToArray)
    End Sub

    Private Sub ListPages_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        GrpPages.Width = Me.Width
        GrpPages.Height = Me.Height
    End Sub

    Public Property ListView() As ListView
        Get
            Return _list
        End Get
        Set(ByVal value As ListView)
            _list = value
        End Set
    End Property

    Public Property PerPage() As Integer
        Get
            Return _perPage
        End Get
        Set(ByVal value As Integer)
            _perPage = value
        End Set
    End Property

    Public Property Page() As Integer
        Get
            Return _page
        End Get
        Set(ByVal value As Integer)
            _page = value
        End Set
    End Property

    Public Property CallBack() As MethodInfo
        Get
            Return _callBack
        End Get
        Set(ByVal value As MethodInfo)
            _callBack = value
        End Set
    End Property

    Public Property MainForm() As Form
        Get
            Return _frm
        End Get
        Set(ByVal value As Form)
            _frm = value
        End Set
    End Property

End Class
