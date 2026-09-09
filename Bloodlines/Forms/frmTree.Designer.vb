<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTree
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
      pnlChart = New ChartPanel()
      btnSetFocus = New Button()
      SuspendLayout()
      '
      ' pnlChart
      '
      pnlChart.AutoScroll = True
      pnlChart.BackColor = SystemColors.Window
      pnlChart.Dock = DockStyle.Fill
      pnlChart.Location = New Point(0, 0)
      pnlChart.Name = "pnlChart"
      pnlChart.Size = New Size(800, 450)
      pnlChart.TabIndex = 0
      '
      ' btnSetFocus
      '
      btnSetFocus.Anchor = AnchorStyles.Top Or AnchorStyles.Left
      btnSetFocus.Cursor = Cursors.Hand
      btnSetFocus.Font = New Font("Segoe UI Emoji", 12.0F)
      btnSetFocus.Location = New Point(8, 8)
      btnSetFocus.Name = "btnSetFocus"
      btnSetFocus.Size = New Size(32, 32)
      btnSetFocus.TabIndex = 1
      btnSetFocus.Text = "🎯"
      btnSetFocus.UseVisualStyleBackColor = True
      '
      ' frmTree
      '
      AutoScaleDimensions = New SizeF(7F, 15F)
      AutoScaleMode = AutoScaleMode.Font
      ClientSize = New Size(800, 450)
      Controls.Add(pnlChart)
      Controls.Add(btnSetFocus)
      btnSetFocus.BringToFront()
      Name = "frmTree"
      Text = "Family Tree"
      ResumeLayout(False)
   End Sub

   Friend WithEvents pnlChart As ChartPanel
   Friend WithEvents btnSetFocus As Button
End Class
