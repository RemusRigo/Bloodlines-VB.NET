'--------------------------------------------------------------------------------------------------
' Bloodlines: frmBloodlines.vb: Main form
'    © 2026 Remus Rigo
'       v1.0.20260925
'--------------------------------------------------------------------------------------------------

Imports System.ComponentModel
Imports System.IO
Imports Bloodlines.Bloodlines
Imports Bloodlines.API
Public Class frmBloodlines

   Private Const SYSMENU_ABOUT_ID As UInteger = 1000

   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Public Property Tree As FamilyTree
   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Public Property jsonTreePath As String

   Private lastForm As Control

   Dim lstTrees As ListBox = New ListBox()

   '-----------------------------------------------------------------------------------------------
   ' OnHandleCreated: add About menu item
   Protected Overrides Sub OnHandleCreated(e As EventArgs)
      MyBase.OnHandleCreated(e)
      Dim hSysMenu As IntPtr = GetSystemMenu(Me.Handle, False)
      ' Add a separator and then your custom item
      AppendMenu(hSysMenu, MF_SEPARATOR, 0, String.Empty)
      AppendMenu(hSysMenu, MF_STRING, SYSMENU_ABOUT_ID, "About...")
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' WndProc: open About DialogBox
   Protected Overrides Sub WndProc(ByRef m As Message)
      MyBase.WndProc(m)
      If m.Msg = WM_SYSCOMMAND Then
         If CUInt(m.WParam) = SYSMENU_ABOUT_ID Then
            frmAbout.ShowDialog()
         End If
      End If
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' ClearPlaceholder: Remove and dispose the last form from the placeholder panel
   Private Sub ClearPlaceholder()
      If lastForm IsNot Nothing Then
         pnlPlaceholder.Controls.Remove(lastForm)
         lastForm.Dispose()
         lastForm = Nothing
      End If
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' frmBloodlines: OnLoad
   Private Sub frmBloodlines_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      Me.Text = AppData.appTitle

      ' disable buttons
      tsBtnSave.Enabled = False
      tsBtnMembers.Enabled = False
      tsBtnList.Enabled = False
      tsBtnTree.Enabled = False

      ' Initialize list trees
      lstTrees.Left = 3
      lstTrees.Top = 3
      lstTrees.Width = 200
      lstTrees.Height = pnlPlaceholder.Height - 6
      lstTrees.Dock = DockStyle.Left And DockStyle.Top And DockStyle.Bottom
      AddHandler lstTrees.DoubleClick, AddressOf lstTrees_DoubleClick
      pnlPlaceholder.Controls.Add(lstTrees)
      lstTrees.Visible = False

      ' search and load trees
      tsBtnLoad_Click(Nothing, Nothing)
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' Shows the standard "no tree" error and returns False when nothing is loaded yet.
   Private Function EnsureTree() As Boolean
      If Tree IsNot Nothing Then Return True
      MessageBox.Show("No family tree loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
      Return False
   End Function

   '-----------------------------------------------------------------------------------------------
   ' tsBtnNew: OnClick: Create a new tree
   Private Sub tsBtnNew_Click(sender As Object, e As EventArgs) Handles tsBtnNew.Click
      ' ClearPlaceholder, not pnlPlaceholder.Controls.Clear(): that would also drop lstTrees
      ' out of the panel (so Load could never show it again) and never dispose lastForm.
      ClearPlaceholder()
      lstTrees.Visible = False

      Dim treeName As String = InputBox("Enter a name for the new family tree:", "Name").Trim()
      If treeName.Length = 0 Then Return
      If File.Exists(FamilyTree.PathFor(treeName)) Then
         MessageBox.Show($"A tree named '{treeName}' already exists.", "New tree", MessageBoxButtons.OK, MessageBoxIcon.Warning)
         Return
      End If

      Tree = New FamilyTree(treeName)
      Tree.Save()      ' create the file now, so it shows up in Load and can be linked to
      jsonTreePath = Tree.SourcePath
      ssStatusLabel.Text = $"Created {Tree.Name}."

      ' enable buttons
      tsBtnSave.Enabled = True
      tsBtnMembers.Enabled = True
      tsBtnList.Enabled = True
      tsBtnTree.Enabled = True
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' tsBtnLoad: OnClick: Scan trees from the Trees folder
   Private Sub tsBtnLoad_Click(sender As Object, e As EventArgs) Handles tsBtnLoad.Click
      ClearPlaceholder()
      lstTrees.Items.Clear()
      lstTrees.Visible = True

      If Directory.Exists(FamilyTree.TreesFolder) Then
         For Each f As String In Directory.GetFiles(FamilyTree.TreesFolder, "*.json")
            lstTrees.Items.Add(Path.GetFileNameWithoutExtension(f))
         Next
      End If
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' tsBtnSave: OnClick: Save the current tree
   Private Sub tsBtnSave_Click(sender As Object, e As EventArgs) Handles tsBtnSave.Click
      If Not EnsureTree() Then Return
      Tree.Save()
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' tsBtnList: OnClick: Show the list of members
   Private Sub tsBtnList_Click(sender As Object, e As EventArgs) Handles tsBtnList.Click
      Me.Text = AppData.appTitle & " [Members of " & Tree.Name & "]"
      ClearPlaceholder()
      If Tree Is Nothing Then
         MessageBox.Show("No family tree loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
      Else
         Dim frmChild As New frmListMembers
         If frmChild IsNot Nothing Then
            frmChild.TopLevel = False
            frmChild.FormBorderStyle = FormBorderStyle.None
            frmChild.Dock = DockStyle.Fill
            frmChild.Tree = Me.Tree
            pnlPlaceholder.Controls.Add(frmChild)
            frmChild.Show()
         End If
         lastForm = frmChild
      End If
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' tsBtnTree: OnClick: Show the tree chart
   Private Sub tsBtnTree_Click(sender As Object, e As EventArgs) Handles tsBtnTree.Click
      Me.Text = AppData.appTitle & " [" & Tree.Name & " Tree]"
      ShowTree()
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' ShowTree: Show the tree chart in the placeholder panel
   Private Sub ShowTree()
      ClearPlaceholder()
      If Not EnsureTree() Then Return
      Dim frmChild As New frmUCTree With {.Dock = DockStyle.Fill, .Tree = Me.Tree}
      AddHandler frmChild.OpenTreeRequested, AddressOf frmUCTree_OpenTreeRequested
      pnlPlaceholder.Controls.Add(frmChild)
      frmChild.Show()
      lastForm = frmChild
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' Raised from inside the chart's own mouse handler, and ShowTree disposes that
   ' chart - so defer to the next message-loop tick and let the handler unwind first.
   Private Sub frmUCTree_OpenTreeRequested(treeName As String)
      BeginInvoke(Sub() OpenLinkedTree(treeName))
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' OpenLinkedTree: Open a tree linked from the chart
   Private Sub OpenLinkedTree(treeName As String)
      If Tree IsNot Nothing AndAlso String.Equals(treeName, Tree.Name, StringComparison.OrdinalIgnoreCase) Then Return ' already showing it

      Dim treeFile As String = FamilyTree.PathFor(treeName)
      ' FamilyTree.Load returns an empty tree for a missing file, so check first
      If Not File.Exists(treeFile) Then
         MessageBox.Show($"The linked tree '{treeName}' was not found in{Environment.NewLine}{FamilyTree.TreesFolder}",
                         "Open linked tree", MessageBoxButtons.OK, MessageBoxIcon.Warning)
         Return
      End If
      If LoadTree(treeFile) Then ShowTree()
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' LoadTree: Load a tree from a file, and update the status bar
   Private Function LoadTree(treeFile As String) As Boolean
      Try
         Tree = FamilyTree.Load(treeFile)
      Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
         ' IOException also covers InvalidDataException (corrupt/newer-format file)
         MessageBox.Show(ex.Message, "Load tree", MessageBoxButtons.OK, MessageBoxIcon.Error)
         Return False
      End Try
      jsonTreePath = treeFile
      ssStatusLabel.Text = $"Loaded {Tree.Name}: {Tree.People.Count} people."
      Return True
   End Function

   '-----------------------------------------------------------------------------------------------
   ' View Members
   Private Sub tsBtnMembers_Click(sender As Object, e As EventArgs) Handles tsBtnMembers.Click
      If Not EnsureTree() Then Return
      frmPerson.Tree = Tree
      frmPerson.newID = False
      frmPerson.ShowDialog()
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' lstTrees: OnDoubleClick: Load the selected tree from the list
   Private Sub lstTrees_DoubleClick(sender As Object, e As EventArgs)
      Dim lst As ListBox = CType(sender, ListBox)
      If lst.SelectedItem Is Nothing Then Return
      If Not LoadTree(FamilyTree.PathFor(lst.SelectedItem.ToString())) Then Return
      lstTrees.Visible = False
      lstTrees.Items.Clear()

      ' enable buttons
      tsBtnSave.Enabled = True
      tsBtnMembers.Enabled = True
      tsBtnList.Enabled = True
      tsBtnTree.Enabled = True
   End Sub

End Class
