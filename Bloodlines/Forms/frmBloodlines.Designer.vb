<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmBloodlines
   Inherits System.Windows.Forms.Form

   'Form overrides dispose to clean up the component list.
   <System.Diagnostics.DebuggerNonUserCode()>
   Protected Overrides Sub Dispose(disposing As Boolean)
      Try
         If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
         End If
      Finally
         MyBase.Dispose(disposing)
      End Try
   End Sub

   'Required by the Windows Form Designer
   Private components As System.ComponentModel.IContainer

   'NOTE: The following procedure is required by the Windows Form Designer
   'It can be modified using the Windows Form Designer.
   'Do not modify it using the code editor.
   <System.Diagnostics.DebuggerStepThrough()>
   Private Sub InitializeComponent()
      Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmBloodlines))
      StatusStrip = New StatusStrip()
      ssStatusLabel = New ToolStripStatusLabel()
      ToolStrip = New ToolStrip()
      tsBtnNew = New ToolStripButton()
      tsBtnLoad = New ToolStripButton()
      tsBtnSave = New ToolStripButton()
      tsBtnSep1 = New ToolStripSeparator()
      tsBtnList = New ToolStripButton()
      ToolStripSeparator1 = New ToolStripSeparator()
      tsBtnTree = New ToolStripButton()
      ToolStripSeparator2 = New ToolStripSeparator()
      tsBtnAddMember = New ToolStripButton()
      tsBtnViewMembers = New ToolStripButton()
      pnlPlaceholder = New Panel()
      StatusStrip.SuspendLayout()
      ToolStrip.SuspendLayout()
      SuspendLayout()
      ' 
      ' StatusStrip
      ' 
      StatusStrip.Items.AddRange(New ToolStripItem() {ssStatusLabel})
      StatusStrip.Location = New Point(0, 428)
      StatusStrip.Name = "StatusStrip"
      StatusStrip.Size = New Size(800, 22)
      StatusStrip.TabIndex = 0
      ' 
      ' ssStatusLabel
      ' 
      ssStatusLabel.Name = "ssStatusLabel"
      ssStatusLabel.Size = New Size(0, 17)
      ' 
      ' ToolStrip
      ' 
      ToolStrip.Items.AddRange(New ToolStripItem() {tsBtnNew, tsBtnLoad, tsBtnSave, tsBtnSep1, tsBtnList, ToolStripSeparator1, tsBtnTree, ToolStripSeparator2, tsBtnAddMember, tsBtnViewMembers})
      ToolStrip.Location = New Point(0, 0)
      ToolStrip.Name = "ToolStrip"
      ToolStrip.Size = New Size(800, 25)
      ToolStrip.TabIndex = 1
      ToolStrip.Text = "ToolStrip"
      ' 
      ' tsBtnNew
      ' 
      tsBtnNew.DisplayStyle = ToolStripItemDisplayStyle.Image
      tsBtnNew.Image = CType(resources.GetObject("tsBtnNew.Image"), Image)
      tsBtnNew.ImageTransparentColor = Color.Magenta
      tsBtnNew.Name = "tsBtnNew"
      tsBtnNew.Size = New Size(23, 22)
      tsBtnNew.Text = "New Family Tree"
      ' 
      ' tsBtnLoad
      ' 
      tsBtnLoad.DisplayStyle = ToolStripItemDisplayStyle.Image
      tsBtnLoad.Image = CType(resources.GetObject("tsBtnLoad.Image"), Image)
      tsBtnLoad.ImageTransparentColor = Color.Magenta
      tsBtnLoad.Name = "tsBtnLoad"
      tsBtnLoad.Size = New Size(23, 22)
      tsBtnLoad.Text = "Load Family Tree"
      tsBtnLoad.ToolTipText = "Load Family Tree"
      ' 
      ' tsBtnSave
      ' 
      tsBtnSave.DisplayStyle = ToolStripItemDisplayStyle.Image
      tsBtnSave.Image = CType(resources.GetObject("tsBtnSave.Image"), Image)
      tsBtnSave.ImageTransparentColor = Color.Magenta
      tsBtnSave.Name = "tsBtnSave"
      tsBtnSave.Size = New Size(23, 22)
      tsBtnSave.Text = "Save Family Tree"
      ' 
      ' tsBtnSep1
      ' 
      tsBtnSep1.Name = "tsBtnSep1"
      tsBtnSep1.Size = New Size(6, 25)
      ' 
      ' tsBtnList
      ' 
      tsBtnList.DisplayStyle = ToolStripItemDisplayStyle.Image
      tsBtnList.Image = CType(resources.GetObject("tsBtnList.Image"), Image)
      tsBtnList.ImageTransparentColor = Color.Magenta
      tsBtnList.Name = "tsBtnList"
      tsBtnList.Size = New Size(23, 22)
      tsBtnList.Text = "List contacts"
      ' 
      ' ToolStripSeparator1
      ' 
      ToolStripSeparator1.Name = "ToolStripSeparator1"
      ToolStripSeparator1.Size = New Size(6, 25)
      ' 
      ' tsBtnTree
      ' 
      tsBtnTree.DisplayStyle = ToolStripItemDisplayStyle.Image
      tsBtnTree.Image = CType(resources.GetObject("tsBtnTree.Image"), Image)
      tsBtnTree.ImageTransparentColor = Color.Magenta
      tsBtnTree.Name = "tsBtnTree"
      tsBtnTree.Size = New Size(23, 22)
      tsBtnTree.Text = "ToolStripButton2"
      ' 
      ' ToolStripSeparator2
      ' 
      ToolStripSeparator2.Name = "ToolStripSeparator2"
      ToolStripSeparator2.Size = New Size(6, 25)
      ' 
      ' tsBtnAddMember
      ' 
      tsBtnAddMember.DisplayStyle = ToolStripItemDisplayStyle.Image
      tsBtnAddMember.Image = CType(resources.GetObject("tsBtnAddMember.Image"), Image)
      tsBtnAddMember.ImageTransparentColor = Color.Magenta
      tsBtnAddMember.Name = "tsBtnAddMember"
      tsBtnAddMember.Size = New Size(23, 22)
      tsBtnAddMember.Text = "Add Member"
      ' 
      ' tsBtnViewMembers
      ' 
      tsBtnViewMembers.DisplayStyle = ToolStripItemDisplayStyle.Image
      tsBtnViewMembers.Image = CType(resources.GetObject("tsBtnViewMembers.Image"), Image)
      tsBtnViewMembers.ImageTransparentColor = Color.Magenta
      tsBtnViewMembers.Name = "tsBtnViewMembers"
      tsBtnViewMembers.Size = New Size(23, 22)
      tsBtnViewMembers.Text = "View Members"
      ' 
      ' pnlPlaceholder
      ' 
      pnlPlaceholder.BackColor = SystemColors.InactiveCaption
      pnlPlaceholder.Dock = DockStyle.Fill
      pnlPlaceholder.Location = New Point(0, 25)
      pnlPlaceholder.Name = "pnlPlaceholder"
      pnlPlaceholder.Size = New Size(800, 403)
      pnlPlaceholder.TabIndex = 2
      ' 
      ' frmBloodlines
      ' 
      AutoScaleDimensions = New SizeF(7.0F, 15.0F)
      AutoScaleMode = AutoScaleMode.Font
      ClientSize = New Size(800, 450)
      Controls.Add(pnlPlaceholder)
      Controls.Add(ToolStrip)
      Controls.Add(StatusStrip)
      Name = "frmBloodlines"
      Text = "Bloodlines"
      StatusStrip.ResumeLayout(False)
      StatusStrip.PerformLayout()
      ToolStrip.ResumeLayout(False)
      ToolStrip.PerformLayout()
      ResumeLayout(False)
      PerformLayout()
   End Sub

   Friend WithEvents StatusStrip As StatusStrip
   Friend WithEvents ToolStrip As ToolStrip
   Friend WithEvents tsBtnViewMembers As ToolStripButton
   Friend WithEvents pnlPlaceholder As Panel
   Friend WithEvents tsBtnSep1 As ToolStripSeparator
   Friend WithEvents tsBtnList As ToolStripButton
   Friend WithEvents tsBtnLoad As ToolStripButton
   Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
   Friend WithEvents ToolStripButton1 As ToolStripButton
   Friend WithEvents tsBtnSave As ToolStripButton
   Friend WithEvents tsBtnNew As ToolStripButton
   Friend WithEvents tsBtnTree As ToolStripButton
   Friend WithEvents ssStatusLabel As ToolStripStatusLabel
   Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
   Friend WithEvents tsBtnAddMember As ToolStripButton

End Class
