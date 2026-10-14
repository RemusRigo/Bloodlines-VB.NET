Imports System.ComponentModel

Public Class frmSettings

   ' Set by the caller before ShowDialog; updated from the radio buttons on OK.
   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Friend Property Settings As AppSettings

   Private Sub frmSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      Select Case Settings.ViewMode
         Case TreeViewMode.Siblings : rBtnSiblings.Checked = True
         Case TreeViewMode.Full : rBtnFullTree.Checked = True
         Case Else : rBtnNormal.Checked = True
      End Select
   End Sub

   Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
      If rBtnFullTree.Checked Then
         Settings.ViewMode = TreeViewMode.Full
      ElseIf rBtnSiblings.Checked Then
         Settings.ViewMode = TreeViewMode.Siblings
      Else
         Settings.ViewMode = TreeViewMode.Normal
      End If
      DialogResult = DialogResult.OK
   End Sub

End Class
