Imports System.ComponentModel
Imports System.IO
Imports Bloodlines.Bloodlines

Public Class frmBloodlines
   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Public Property Tree As FamilyTree
   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Public Property jsonTreePath As String
   Dim lstTrees As ListBox = New ListBox()

   Private Sub frmBloodlines_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      lstTrees.Left = 3
      lstTrees.Top = 3
      lstTrees.Width = 200
      lstTrees.Height = pnlPlaceholder.Height - 6
      lstTrees.Dock = DockStyle.Left And DockStyle.Top And DockStyle.Bottom
      AddHandler lstTrees.DoubleClick, AddressOf lstTrees_DoubleClick
      pnlPlaceholder.Controls.Add(lstTrees)
      lstTrees.Visible = False
   End Sub

   Private Sub tsBtnNew_Click(sender As Object, e As EventArgs) Handles tsBtnNew.Click
      '
   End Sub

   Private Sub tsBtnLoad_Click(sender As Object, e As EventArgs) Handles tsBtnLoad.Click
      lstTrees.Visible = True

      Dim treePath As String = Application.StartupPath & "Trees"
      If Directory.Exists(treePath) Then
         For Each file As String In Directory.GetFiles(treePath, "*.json")
            lstTrees.Items.Add(Path.GetFileNameWithoutExtension(file))
         Next
      End If
   End Sub

   Private Sub tsBtnSave_Click(sender As Object, e As EventArgs) Handles tsBtnSave.Click
      Tree.Save()
   End Sub

   Private Sub tsBtnList_Click(sender As Object, e As EventArgs) Handles tsBtnList.Click
      If Tree Is Nothing Then
         MessageBox.Show("No family tree loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
      Else
         pnlPlaceholder.Controls.Clear()
         Dim frmChild As New frmListMembers
         If frmChild IsNot Nothing Then
            frmChild.TopLevel = False
            frmChild.FormBorderStyle = FormBorderStyle.None
            frmChild.Dock = DockStyle.Fill
            frmChild.Tree = Me.Tree
            pnlPlaceholder.Controls.Add(frmChild)
            frmChild.Show()
         End If
      End If
   End Sub

   Private Sub tsBtnTree_Click(sender As Object, e As EventArgs) Handles tsBtnTree.Click
      If Tree Is Nothing Then
         MessageBox.Show("No family tree loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
      Else
         pnlPlaceholder.Controls.Clear()
         Dim frmChild As New frmTree
         If frmChild IsNot Nothing Then
            frmChild.TopLevel = False
            frmChild.FormBorderStyle = FormBorderStyle.None
            frmChild.Dock = DockStyle.Fill
            frmChild.Tree = Me.Tree
            pnlPlaceholder.Controls.Add(frmChild)
            frmChild.Show()
         End If
      End If
   End Sub

   Private Sub tsBtnAddMember_Click(sender As Object, e As EventArgs) Handles tsBtnAddMember.Click
      frmPerson.Tree = Tree
      frmPerson.newID = True
      frmPerson.ShowDialog()
   End Sub

   Private Sub tsBtnViewMembers_Click(sender As Object, e As EventArgs) Handles tsBtnViewMembers.Click
      frmPerson.Tree = Tree
      frmPerson.newID = False
      frmPerson.ShowDialog()
   End Sub

   Private Sub lstTrees_DoubleClick(sender As Object, e As EventArgs)
      Dim lst As ListBox = CType(sender, ListBox)
      jsonTreePath = Path.Combine(Application.StartupPath, "Trees", lst.SelectedItem.ToString() & ".json")
      Tree = FamilyTree.Load(jsonTreePath)
      lstTrees.Visible = False
      lstTrees.Items.Clear()
      ssStatusLabel.Text = "Loaded " & Tree.People.Count & " people."
   End Sub

End Class
