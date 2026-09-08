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
         item.SubItems.Add(p.Sex)
         item.SubItems.Add(p.BirthDate.ToString("yyyy-MM-dd"))
         item.SubItems.Add(p.DeathDate.ToString("yyyy-MM-dd"))
         'item.SubItems.Add(p.Father)
         'item.SubItems.Add(p.Mother)
         'item.SubItems.Add(p.Spouse)
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
      lvMembers.Columns.Add("Sex", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Birth Date", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Death Date", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Father", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Mother", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Spouse", 100, HorizontalAlignment.Left)
      lvMembers.FullRowSelect = True

      LoadMembers(Tree)
   End Sub
End Class