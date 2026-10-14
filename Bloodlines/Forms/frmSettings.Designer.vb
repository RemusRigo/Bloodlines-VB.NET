<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSettings
   Inherits System.Windows.Forms.Form

   'Form overrides dispose to clean up the component list.
   <System.Diagnostics.DebuggerNonUserCode()> _
   Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
   <System.Diagnostics.DebuggerStepThrough()> _
   Private Sub InitializeComponent()
      Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSettings))
      grpTreeViewMode = New GroupBox()
      rBtnFullTree = New RadioButton()
      rBtnSiblings = New RadioButton()
      rBtnNormal = New RadioButton()
      btnOk = New Button()
      grpTreeViewMode.SuspendLayout()
      SuspendLayout()
      ' 
      ' grpTreeViewMode
      ' 
      grpTreeViewMode.Controls.Add(rBtnFullTree)
      grpTreeViewMode.Controls.Add(rBtnSiblings)
      grpTreeViewMode.Controls.Add(rBtnNormal)
      grpTreeViewMode.Location = New Point(3, 3)
      grpTreeViewMode.Name = "grpTreeViewMode"
      grpTreeViewMode.Size = New Size(134, 100)
      grpTreeViewMode.TabIndex = 0
      grpTreeViewMode.TabStop = False
      grpTreeViewMode.Text = "Tree View Mode"
      ' 
      ' rBtnFullTree
      ' 
      rBtnFullTree.AutoSize = True
      rBtnFullTree.Location = New Point(11, 69)
      rBtnFullTree.Name = "rBtnFullTree"
      rBtnFullTree.Size = New Size(88, 19)
      rBtnFullTree.TabIndex = 2
      rBtnFullTree.Text = "All Relatives"
      rBtnFullTree.UseVisualStyleBackColor = True
      ' 
      ' rBtnSiblings
      ' 
      rBtnSiblings.AutoSize = True
      rBtnSiblings.Location = New Point(11, 44)
      rBtnSiblings.Name = "rBtnSiblings"
      rBtnSiblings.Size = New Size(109, 19)
      rBtnSiblings.TabIndex = 1
      rBtnSiblings.Text = "1st Line Siblings"
      rBtnSiblings.UseVisualStyleBackColor = True
      ' 
      ' rBtnNormal
      ' 
      rBtnNormal.AutoSize = True
      rBtnNormal.Location = New Point(11, 19)
      rBtnNormal.Name = "rBtnNormal"
      rBtnNormal.Size = New Size(65, 19)
      rBtnNormal.TabIndex = 0
      rBtnNormal.Text = "Normal"
      rBtnNormal.UseVisualStyleBackColor = True
      ' 
      ' btnOk
      ' 
      btnOk.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
      btnOk.Location = New Point(236, 135)
      btnOk.Name = "btnOk"
      btnOk.Size = New Size(45, 23)
      btnOk.TabIndex = 1
      btnOk.Text = "O&k"
      btnOk.UseVisualStyleBackColor = True
      ' 
      ' frmSettings
      ' 
      AutoScaleDimensions = New SizeF(7F, 15F)
      AutoScaleMode = AutoScaleMode.Font
      BackgroundImageLayout = ImageLayout.Stretch
      ClientSize = New Size(284, 161)
      Controls.Add(btnOk)
      Controls.Add(grpTreeViewMode)
      DoubleBuffered = True
      Icon = CType(resources.GetObject("$this.Icon"), Icon)
      MaximizeBox = False
      MinimizeBox = False
      Name = "frmSettings"
      StartPosition = FormStartPosition.CenterParent
      Text = "Settings"
      grpTreeViewMode.ResumeLayout(False)
      grpTreeViewMode.PerformLayout()
      ResumeLayout(False)
   End Sub

   Friend WithEvents grpTreeViewMode As GroupBox
   Friend WithEvents rBtnFullTree As RadioButton
   Friend WithEvents rBtnSiblings As RadioButton
   Friend WithEvents rBtnNormal As RadioButton
   Friend WithEvents btnOk As Button
End Class
