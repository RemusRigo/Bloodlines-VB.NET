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
      pnlTop = New Panel()
      lblFocus = New Label()
      cboFocus = New ComboBox()
      pnlTop.SuspendLayout()
      SuspendLayout()
      '
      ' pnlChart
      '
      pnlChart.AutoScroll = True
      pnlChart.BackColor = SystemColors.Window
      pnlChart.Dock = DockStyle.Fill
      pnlChart.Location = New Point(0, 34)
      pnlChart.Name = "pnlChart"
      pnlChart.Size = New Size(800, 416)
      pnlChart.TabIndex = 1
      '
      ' pnlTop
      '
      pnlTop.Controls.Add(cboFocus)
      pnlTop.Controls.Add(lblFocus)
      pnlTop.Dock = DockStyle.Top
      pnlTop.Location = New Point(0, 0)
      pnlTop.Name = "pnlTop"
      pnlTop.Padding = New Padding(8, 6, 8, 6)
      pnlTop.Size = New Size(800, 34)
      pnlTop.TabIndex = 0
      '
      ' lblFocus
      '
      lblFocus.AutoSize = True
      lblFocus.Location = New Point(8, 9)
      lblFocus.Name = "lblFocus"
      lblFocus.Size = New Size(78, 15)
      lblFocus.TabIndex = 0
      lblFocus.Text = "Focus person:"
      '
      ' cboFocus
      '
      cboFocus.DropDownStyle = ComboBoxStyle.DropDownList
      cboFocus.FormattingEnabled = True
      cboFocus.Location = New Point(95, 5)
      cboFocus.Name = "cboFocus"
      cboFocus.Size = New Size(280, 23)
      cboFocus.TabIndex = 1
      '
      ' frmTree
      '
      AutoScaleDimensions = New SizeF(7F, 15F)
      AutoScaleMode = AutoScaleMode.Font
      ClientSize = New Size(800, 450)
      Controls.Add(pnlChart)
      Controls.Add(pnlTop)
      Name = "frmTree"
      Text = "Family Tree"
      pnlTop.ResumeLayout(False)
      pnlTop.PerformLayout()
      ResumeLayout(False)
   End Sub

   Friend WithEvents pnlChart As ChartPanel
   Friend WithEvents pnlTop As Panel
   Friend WithEvents lblFocus As Label
   Friend WithEvents cboFocus As ComboBox
End Class
