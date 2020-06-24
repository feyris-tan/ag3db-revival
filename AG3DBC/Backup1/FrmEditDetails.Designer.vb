<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmEditDetails
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmEditDetails))
        Me.TxtDesc = New System.Windows.Forms.TextBox
        Me.WebPrev = New System.Windows.Forms.WebBrowser
        Me.CmdPreview = New System.Windows.Forms.Button
        Me.cmd = New System.Windows.Forms.Button
        Me.Label1 = New System.Windows.Forms.Label
        Me.TxtTags = New System.Windows.Forms.TextBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.CmdSave = New System.Windows.Forms.Button
        Me.CmdCancel = New System.Windows.Forms.Button
        Me.CmdSaveClose = New System.Windows.Forms.Button
        Me.WebFrame = New System.Windows.Forms.PictureBox
        Me.GrpQuicks = New System.Windows.Forms.GroupBox
        Me.CmdList = New System.Windows.Forms.Button
        Me.CmdSpoiler = New System.Windows.Forms.Button
        Me.CmdNoParse = New System.Windows.Forms.Button
        Me.CmdEmail = New System.Windows.Forms.Button
        Me.CmdStrike = New System.Windows.Forms.Button
        Me.CmdColor = New System.Windows.Forms.Button
        Me.CmdRight = New System.Windows.Forms.Button
        Me.CmdLeft = New System.Windows.Forms.Button
        Me.CmdCenter = New System.Windows.Forms.Button
        Me.CmdSize = New System.Windows.Forms.Button
        Me.CmdFont = New System.Windows.Forms.Button
        Me.CmdImage = New System.Windows.Forms.Button
        Me.CmdURL = New System.Windows.Forms.Button
        Me.CmdUnderline = New System.Windows.Forms.Button
        Me.CmdItalic = New System.Windows.Forms.Button
        Me.CmdBold = New System.Windows.Forms.Button
        CType(Me.WebFrame, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GrpQuicks.SuspendLayout()
        Me.SuspendLayout()
        '
        'TxtDesc
        '
        Me.TxtDesc.Font = New System.Drawing.Font("Verdana", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtDesc.Location = New System.Drawing.Point(12, 109)
        Me.TxtDesc.Multiline = True
        Me.TxtDesc.Name = "TxtDesc"
        Me.TxtDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TxtDesc.Size = New System.Drawing.Size(619, 113)
        Me.TxtDesc.TabIndex = 0
        Me.TxtDesc.Text = resources.GetString("TxtDesc.Text")
        '
        'WebPrev
        '
        Me.WebPrev.Location = New System.Drawing.Point(14, 230)
        Me.WebPrev.MinimumSize = New System.Drawing.Size(20, 20)
        Me.WebPrev.Name = "WebPrev"
        Me.WebPrev.Size = New System.Drawing.Size(615, 306)
        Me.WebPrev.TabIndex = 1
        '
        'CmdPreview
        '
        Me.CmdPreview.Location = New System.Drawing.Point(10, 544)
        Me.CmdPreview.Name = "CmdPreview"
        Me.CmdPreview.Size = New System.Drawing.Size(114, 29)
        Me.CmdPreview.TabIndex = 2
        Me.CmdPreview.Text = "Preview Description"
        Me.CmdPreview.UseVisualStyleBackColor = True
        '
        'cmd
        '
        Me.cmd.Location = New System.Drawing.Point(495, 544)
        Me.cmd.Name = "cmd"
        Me.cmd.Size = New System.Drawing.Size(134, 29)
        Me.cmd.TabIndex = 3
        Me.cmd.Text = "Button2"
        Me.cmd.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(9, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(34, 13)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Tags:"
        '
        'TxtTags
        '
        Me.TxtTags.Location = New System.Drawing.Point(12, 25)
        Me.TxtTags.Name = "TxtTags"
        Me.TxtTags.Size = New System.Drawing.Size(617, 20)
        Me.TxtTags.TabIndex = 5
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(9, 51)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(63, 13)
        Me.Label2.TabIndex = 6
        Me.Label2.Text = "Description:"
        '
        'CmdSave
        '
        Me.CmdSave.Location = New System.Drawing.Point(124, 544)
        Me.CmdSave.Name = "CmdSave"
        Me.CmdSave.Size = New System.Drawing.Size(114, 29)
        Me.CmdSave.TabIndex = 7
        Me.CmdSave.Text = "Save Changes"
        Me.CmdSave.UseVisualStyleBackColor = True
        '
        'CmdCancel
        '
        Me.CmdCancel.Location = New System.Drawing.Point(352, 544)
        Me.CmdCancel.Name = "CmdCancel"
        Me.CmdCancel.Size = New System.Drawing.Size(114, 29)
        Me.CmdCancel.TabIndex = 8
        Me.CmdCancel.Text = "Cancel"
        Me.CmdCancel.UseVisualStyleBackColor = True
        '
        'CmdSaveClose
        '
        Me.CmdSaveClose.Location = New System.Drawing.Point(238, 544)
        Me.CmdSaveClose.Name = "CmdSaveClose"
        Me.CmdSaveClose.Size = New System.Drawing.Size(114, 29)
        Me.CmdSaveClose.TabIndex = 10
        Me.CmdSaveClose.Text = "Save And Close"
        Me.CmdSaveClose.UseVisualStyleBackColor = True
        '
        'WebFrame
        '
        Me.WebFrame.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.WebFrame.Location = New System.Drawing.Point(12, 228)
        Me.WebFrame.Name = "WebFrame"
        Me.WebFrame.Size = New System.Drawing.Size(619, 310)
        Me.WebFrame.TabIndex = 9
        Me.WebFrame.TabStop = False
        '
        'GrpQuicks
        '
        Me.GrpQuicks.Controls.Add(Me.CmdList)
        Me.GrpQuicks.Controls.Add(Me.CmdSpoiler)
        Me.GrpQuicks.Controls.Add(Me.CmdNoParse)
        Me.GrpQuicks.Controls.Add(Me.CmdEmail)
        Me.GrpQuicks.Controls.Add(Me.CmdStrike)
        Me.GrpQuicks.Controls.Add(Me.CmdColor)
        Me.GrpQuicks.Controls.Add(Me.CmdRight)
        Me.GrpQuicks.Controls.Add(Me.CmdLeft)
        Me.GrpQuicks.Controls.Add(Me.CmdCenter)
        Me.GrpQuicks.Controls.Add(Me.CmdSize)
        Me.GrpQuicks.Controls.Add(Me.CmdFont)
        Me.GrpQuicks.Controls.Add(Me.CmdImage)
        Me.GrpQuicks.Controls.Add(Me.CmdURL)
        Me.GrpQuicks.Controls.Add(Me.CmdUnderline)
        Me.GrpQuicks.Controls.Add(Me.CmdItalic)
        Me.GrpQuicks.Controls.Add(Me.CmdBold)
        Me.GrpQuicks.Location = New System.Drawing.Point(12, 64)
        Me.GrpQuicks.Name = "GrpQuicks"
        Me.GrpQuicks.Size = New System.Drawing.Size(617, 39)
        Me.GrpQuicks.TabIndex = 23
        Me.GrpQuicks.TabStop = False
        '
        'CmdList
        '
        Me.CmdList.Location = New System.Drawing.Point(542, 11)
        Me.CmdList.Name = "CmdList"
        Me.CmdList.Size = New System.Drawing.Size(31, 22)
        Me.CmdList.TabIndex = 38
        Me.CmdList.Text = "List"
        Me.CmdList.UseVisualStyleBackColor = True
        '
        'CmdSpoiler
        '
        Me.CmdSpoiler.Location = New System.Drawing.Point(436, 11)
        Me.CmdSpoiler.Name = "CmdSpoiler"
        Me.CmdSpoiler.Size = New System.Drawing.Size(47, 22)
        Me.CmdSpoiler.TabIndex = 37
        Me.CmdSpoiler.Text = "Spoiler"
        Me.CmdSpoiler.UseVisualStyleBackColor = True
        '
        'CmdNoParse
        '
        Me.CmdNoParse.Location = New System.Drawing.Point(483, 11)
        Me.CmdNoParse.Name = "CmdNoParse"
        Me.CmdNoParse.Size = New System.Drawing.Size(59, 22)
        Me.CmdNoParse.TabIndex = 36
        Me.CmdNoParse.Text = "No Parse"
        Me.CmdNoParse.UseVisualStyleBackColor = True
        '
        'CmdEmail
        '
        Me.CmdEmail.Location = New System.Drawing.Point(167, 11)
        Me.CmdEmail.Name = "CmdEmail"
        Me.CmdEmail.Size = New System.Drawing.Size(40, 22)
        Me.CmdEmail.TabIndex = 35
        Me.CmdEmail.Text = "Email"
        Me.CmdEmail.UseVisualStyleBackColor = True
        '
        'CmdStrike
        '
        Me.CmdStrike.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CmdStrike.Location = New System.Drawing.Point(66, 11)
        Me.CmdStrike.Name = "CmdStrike"
        Me.CmdStrike.Size = New System.Drawing.Size(20, 22)
        Me.CmdStrike.TabIndex = 34
        Me.CmdStrike.Text = "S"
        Me.CmdStrike.UseVisualStyleBackColor = True
        '
        'CmdColor
        '
        Me.CmdColor.Location = New System.Drawing.Point(278, 11)
        Me.CmdColor.Name = "CmdColor"
        Me.CmdColor.Size = New System.Drawing.Size(39, 22)
        Me.CmdColor.TabIndex = 33
        Me.CmdColor.Text = "Color"
        Me.CmdColor.UseVisualStyleBackColor = True
        '
        'CmdRight
        '
        Me.CmdRight.Location = New System.Drawing.Point(396, 11)
        Me.CmdRight.Name = "CmdRight"
        Me.CmdRight.Size = New System.Drawing.Size(40, 22)
        Me.CmdRight.TabIndex = 32
        Me.CmdRight.Text = "Right"
        Me.CmdRight.UseVisualStyleBackColor = True
        '
        'CmdLeft
        '
        Me.CmdLeft.Location = New System.Drawing.Point(317, 11)
        Me.CmdLeft.Name = "CmdLeft"
        Me.CmdLeft.Size = New System.Drawing.Size(33, 22)
        Me.CmdLeft.TabIndex = 31
        Me.CmdLeft.Text = "Left"
        Me.CmdLeft.UseVisualStyleBackColor = True
        '
        'CmdCenter
        '
        Me.CmdCenter.Location = New System.Drawing.Point(350, 11)
        Me.CmdCenter.Name = "CmdCenter"
        Me.CmdCenter.Size = New System.Drawing.Size(46, 22)
        Me.CmdCenter.TabIndex = 30
        Me.CmdCenter.Text = "Center"
        Me.CmdCenter.UseVisualStyleBackColor = True
        '
        'CmdSize
        '
        Me.CmdSize.Location = New System.Drawing.Point(243, 11)
        Me.CmdSize.Name = "CmdSize"
        Me.CmdSize.Size = New System.Drawing.Size(35, 22)
        Me.CmdSize.TabIndex = 29
        Me.CmdSize.Text = "Size"
        Me.CmdSize.UseVisualStyleBackColor = True
        '
        'CmdFont
        '
        Me.CmdFont.Location = New System.Drawing.Point(207, 11)
        Me.CmdFont.Name = "CmdFont"
        Me.CmdFont.Size = New System.Drawing.Size(36, 22)
        Me.CmdFont.TabIndex = 28
        Me.CmdFont.Text = "Font"
        Me.CmdFont.UseVisualStyleBackColor = True
        '
        'CmdImage
        '
        Me.CmdImage.Location = New System.Drawing.Point(123, 11)
        Me.CmdImage.Name = "CmdImage"
        Me.CmdImage.Size = New System.Drawing.Size(44, 22)
        Me.CmdImage.TabIndex = 27
        Me.CmdImage.Text = "Image"
        Me.CmdImage.UseVisualStyleBackColor = True
        '
        'CmdURL
        '
        Me.CmdURL.Location = New System.Drawing.Point(86, 11)
        Me.CmdURL.Name = "CmdURL"
        Me.CmdURL.Size = New System.Drawing.Size(37, 22)
        Me.CmdURL.TabIndex = 26
        Me.CmdURL.Text = "URL"
        Me.CmdURL.UseVisualStyleBackColor = True
        '
        'CmdUnderline
        '
        Me.CmdUnderline.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CmdUnderline.Location = New System.Drawing.Point(46, 11)
        Me.CmdUnderline.Name = "CmdUnderline"
        Me.CmdUnderline.Size = New System.Drawing.Size(20, 22)
        Me.CmdUnderline.TabIndex = 25
        Me.CmdUnderline.Text = "U"
        Me.CmdUnderline.UseVisualStyleBackColor = True
        '
        'CmdItalic
        '
        Me.CmdItalic.Font = New System.Drawing.Font("Courier New", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CmdItalic.Location = New System.Drawing.Point(26, 11)
        Me.CmdItalic.Name = "CmdItalic"
        Me.CmdItalic.Size = New System.Drawing.Size(20, 22)
        Me.CmdItalic.TabIndex = 24
        Me.CmdItalic.Text = "I"
        Me.CmdItalic.UseVisualStyleBackColor = True
        '
        'CmdBold
        '
        Me.CmdBold.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CmdBold.Location = New System.Drawing.Point(6, 11)
        Me.CmdBold.Name = "CmdBold"
        Me.CmdBold.Size = New System.Drawing.Size(20, 22)
        Me.CmdBold.TabIndex = 23
        Me.CmdBold.Text = "B"
        Me.CmdBold.UseVisualStyleBackColor = True
        '
        'FrmEditDetails
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(643, 578)
        Me.Controls.Add(Me.GrpQuicks)
        Me.Controls.Add(Me.CmdSaveClose)
        Me.Controls.Add(Me.CmdCancel)
        Me.Controls.Add(Me.CmdSave)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.TxtTags)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.cmd)
        Me.Controls.Add(Me.CmdPreview)
        Me.Controls.Add(Me.WebPrev)
        Me.Controls.Add(Me.TxtDesc)
        Me.Controls.Add(Me.WebFrame)
        Me.Name = "FrmEditDetails"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "FrmEditDetails"
        CType(Me.WebFrame, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GrpQuicks.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TxtDesc As System.Windows.Forms.TextBox
    Friend WithEvents WebPrev As System.Windows.Forms.WebBrowser
    Friend WithEvents CmdPreview As System.Windows.Forms.Button
    Friend WithEvents cmd As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TxtTags As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents CmdSave As System.Windows.Forms.Button
    Friend WithEvents CmdCancel As System.Windows.Forms.Button
    Friend WithEvents WebFrame As System.Windows.Forms.PictureBox
    Friend WithEvents CmdSaveClose As System.Windows.Forms.Button
    Friend WithEvents GrpQuicks As System.Windows.Forms.GroupBox
    Friend WithEvents CmdColor As System.Windows.Forms.Button
    Friend WithEvents CmdRight As System.Windows.Forms.Button
    Friend WithEvents CmdLeft As System.Windows.Forms.Button
    Friend WithEvents CmdCenter As System.Windows.Forms.Button
    Friend WithEvents CmdSize As System.Windows.Forms.Button
    Friend WithEvents CmdFont As System.Windows.Forms.Button
    Friend WithEvents CmdImage As System.Windows.Forms.Button
    Friend WithEvents CmdURL As System.Windows.Forms.Button
    Friend WithEvents CmdUnderline As System.Windows.Forms.Button
    Friend WithEvents CmdItalic As System.Windows.Forms.Button
    Friend WithEvents CmdBold As System.Windows.Forms.Button
    Friend WithEvents CmdStrike As System.Windows.Forms.Button
    Friend WithEvents CmdEmail As System.Windows.Forms.Button
    Friend WithEvents CmdNoParse As System.Windows.Forms.Button
    Friend WithEvents CmdSpoiler As System.Windows.Forms.Button
    Friend WithEvents CmdList As System.Windows.Forms.Button
End Class
