<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmUCTree
   Inherits System.Windows.Forms.UserControl

   'UserControl overrides dispose to clean up the component list.
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
      components = New System.ComponentModel.Container()
      pnlChart = New ChartPanel()
      SuspendLayout()
      '
      ' pnlChart
      ' The single surface the whole hourglass chart is drawn on (see pnlChart_Paint
      ' in the code-behind). It fills the control so the chart always has the full
      ' available area to pan/zoom within.
      '
      pnlChart.BackColor = SystemColors.Window
      pnlChart.Dock = DockStyle.Fill
      pnlChart.Location = New Point(0, 0)
      pnlChart.Name = "pnlChart"
      pnlChart.Size = New Size(800, 450)
      pnlChart.TabIndex = 0
      ' TabStop lets pnlChart receive keyboard focus (via a mouse click) so the
      ' +/- zoom keys have somewhere to be handled - see pnlChart_KeyDown.
      pnlChart.TabStop = True
      '
      ' frmUCTree
      '
      AutoScaleDimensions = New SizeF(7F, 15F)
      AutoScaleMode = AutoScaleMode.Font
      Controls.Add(pnlChart)
      Name = "frmUCTree"
      Size = New Size(800, 450)
      ResumeLayout(False)
   End Sub

   Friend WithEvents pnlChart As ChartPanel
End Class
