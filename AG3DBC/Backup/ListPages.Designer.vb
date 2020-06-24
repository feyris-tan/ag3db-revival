<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ListPages
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.GrpPages = New System.Windows.Forms.GroupBox
        Me.SuspendLayout()
        '
        'GrpPages
        '
        Me.GrpPages.Location = New System.Drawing.Point(0, 0)
        Me.GrpPages.Name = "GrpPages"
        Me.GrpPages.Size = New System.Drawing.Size(624, 73)
        Me.GrpPages.TabIndex = 0
        Me.GrpPages.TabStop = False
        Me.GrpPages.Text = "Pages"
        '
        'ListPages
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.GrpPages)
        Me.Name = "ListPages"
        Me.Size = New System.Drawing.Size(624, 73)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GrpPages As System.Windows.Forms.GroupBox

End Class
