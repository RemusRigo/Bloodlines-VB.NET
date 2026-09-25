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
      tsBtnSep1 = New ToolStripSeparator()
      tsBtnMembers = New ToolStripButton()
      tsBtnList = New ToolStripButton()
      tsBtnTree = New ToolStripButton()
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
      ToolStrip.Items.AddRange(New ToolStripItem() {tsBtnNew, tsBtnLoad, tsBtnSep1, tsBtnMembers, tsBtnList, tsBtnTree})
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
      ' tsBtnSep1
      ' 
      tsBtnSep1.Name = "tsBtnSep1"
      tsBtnSep1.Size = New Size(6, 25)
      ' 
      ' tsBtnMembers
      ' 
      tsBtnMembers.DisplayStyle = ToolStripItemDisplayStyle.Image
      tsBtnMembers.Image = CType(resources.GetObject("tsBtnMembers.Image"), Image)
      tsBtnMembers.ImageTransparentColor = Color.Magenta
      tsBtnMembers.Name = "tsBtnMembers"
      tsBtnMembers.Size = New Size(23, 22)
      tsBtnMembers.Text = "View Members"
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
      ' tsBtnTree
      ' 
      tsBtnTree.DisplayStyle = ToolStripItemDisplayStyle.Image
      tsBtnTree.Image = CType(resources.GetObject("tsBtnTree.Image"), Image)
      tsBtnTree.ImageTransparentColor = Color.Magenta
      tsBtnTree.Name = "tsBtnTree"
      tsBtnTree.Size = New Size(23, 22)
      tsBtnTree.Text = "ToolStripButton2"
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
      AutoScaleDimensions = New SizeF(7F, 15F)
      AutoScaleMode = AutoScaleMode.Font
      ClientSize = New Size(800, 450)
      Controls.Add(pnlPlaceholder)
      Controls.Add(ToolStrip)
      Controls.Add(StatusStrip)
      Name = "frmBloodlines"
      StartPosition = FormStartPosition.CenterScreen
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
   Friend WithEvents tsBtnMembers As ToolStripButton
   Friend WithEvents pnlPlaceholder As Panel
   Friend WithEvents tsBtnSep1 As ToolStripSeparator
   Friend WithEvents tsBtnList As ToolStripButton
   Friend WithEvents tsBtnLoad As ToolStripButton
   Friend WithEvents tsBtnTree As ToolStripButton
   Friend WithEvents tsBtnNew As ToolStripButton
   Friend WithEvents ssStatusLabel As ToolStripStatusLabel

End Class
