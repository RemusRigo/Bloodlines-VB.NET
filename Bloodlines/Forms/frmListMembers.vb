Imports System.ComponentModel
Imports Bloodlines.Bloodlines

Public Class frmListMembers
   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Public Property Tree As FamilyTree

   Private Sub LoadMembers(ft As FamilyTree)
      lvMembers.BeginUpdate()
      lvMembers.Items.Clear()

      For Each p As Person In ft.People
         Dim item As New ListViewItem(p.ID)
         item.SubItems.Add(p.FirstName)
         item.SubItems.Add(p.LastName)
         item.Tag = p
         lvMembers.Items.Add(item)
      Next

      lvMembers.EndUpdate()
   End Sub

   Private Sub frmListMembers_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      lvMembers.View = View.Details
      lvMembers.Columns.Add("ID", 23, HorizontalAlignment.Left)
      lvMembers.Columns.Add("First Name", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Last Name", 100, HorizontalAlignment.Left)
      lvMembers.FullRowSelect = True

      LoadMembers(Tree)
   End Sub
End Class