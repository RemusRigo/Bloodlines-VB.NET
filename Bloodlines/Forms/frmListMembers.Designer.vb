<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmListMembers
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
      lvMembers = New ListView()
      SuspendLayout()
      ' 
      ' lvMembers
      ' 
      lvMembers.Dock = DockStyle.Fill
      lvMembers.Location = New Point(0, 0)
      lvMembers.Name = "lvMembers"
      lvMembers.Size = New Size(800, 450)
      lvMembers.TabIndex = 0
      lvMembers.UseCompatibleStateImageBehavior = False
      ' 
      ' frmListMembers
      ' 
      AutoScaleDimensions = New SizeF(7F, 15F)
      AutoScaleMode = AutoScaleMode.Font
      ClientSize = New Size(800, 450)
      Controls.Add(lvMembers)
      Name = "frmListMembers"
      Text = "List Members"
      ResumeLayout(False)
   End Sub

   Friend WithEvents lvMembers As ListView
End Class
