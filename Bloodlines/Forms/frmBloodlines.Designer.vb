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
      StatusStrip1 = New StatusStrip()
      ToolStrip1 = New ToolStrip()
      tsBtnNew = New ToolStripButton()
      tsBtnLoad = New ToolStripButton()
      tsBtnSave = New ToolStripButton()
      tsBtnSep1 = New ToolStripSeparator()
      tsBtnList = New ToolStripButton()
      ToolStripSeparator1 = New ToolStripSeparator()
      tsBtnAdd = New ToolStripButton()
      ToolStripButton2 = New ToolStripButton()
      pnlPlaceholder = New Panel()
      ToolStrip1.SuspendLayout()
      SuspendLayout()
      ' 
      ' StatusStrip1
      ' 
      StatusStrip1.Location = New Point(0, 428)
      StatusStrip1.Name = "StatusStrip1"
      StatusStrip1.Size = New Size(800, 22)
      StatusStrip1.TabIndex = 0
      StatusStrip1.Text = "StatusStrip"
      ' 
      ' ToolStrip1
      ' 
      ToolStrip1.Items.AddRange(New ToolStripItem() {tsBtnNew, tsBtnLoad, tsBtnSave, tsBtnSep1, tsBtnList, ToolStripSeparator1, tsBtnAdd, ToolStripButton2})
      ToolStrip1.Location = New Point(0, 0)
      ToolStrip1.Name = "ToolStrip1"
      ToolStrip1.Size = New Size(800, 25)
      ToolStrip1.TabIndex = 1
      ToolStrip1.Text = "ToolStrip"
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
      ' tsBtnAdd
      ' 
      tsBtnAdd.DisplayStyle = ToolStripItemDisplayStyle.Image
      tsBtnAdd.Image = CType(resources.GetObject("tsBtnAdd.Image"), Image)
      tsBtnAdd.ImageTransparentColor = Color.Magenta
      tsBtnAdd.Name = "tsBtnAdd"
      tsBtnAdd.Size = New Size(23, 22)
      tsBtnAdd.Text = "Add Person"
      ' 
      ' ToolStripButton2
      ' 
      ToolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image
      ToolStripButton2.Image = CType(resources.GetObject("ToolStripButton2.Image"), Image)
      ToolStripButton2.ImageTransparentColor = Color.Magenta
      ToolStripButton2.Name = "ToolStripButton2"
      ToolStripButton2.Size = New Size(23, 22)
      ToolStripButton2.Text = "ToolStripButton2"
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
      Controls.Add(ToolStrip1)
      Controls.Add(StatusStrip1)
      Name = "frmBloodlines"
      Text = "Bloodlines"
      ToolStrip1.ResumeLayout(False)
      ToolStrip1.PerformLayout()
      ResumeLayout(False)
      PerformLayout()
   End Sub

   Friend WithEvents StatusStrip1 As StatusStrip
   Friend WithEvents ToolStrip1 As ToolStrip
   Friend WithEvents tsBtnAdd As ToolStripButton
   Friend WithEvents pnlPlaceholder As Panel
   Friend WithEvents tsBtnSep1 As ToolStripSeparator
   Friend WithEvents tsBtnList As ToolStripButton
   Friend WithEvents tsBtnLoad As ToolStripButton
   Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
   Friend WithEvents ToolStripButton1 As ToolStripButton
   Friend WithEvents tsBtnSave As ToolStripButton
   Friend WithEvents tsBtnNew As ToolStripButton
   Friend WithEvents ToolStripButton2 As ToolStripButton

End Class
