Imports System.ComponentModel
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports Bloodlines.Bloodlines

Public Class frmListMembers
   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Public Property Tree As FamilyTree

   Private Function NameFor(ft As FamilyTree, p As Person, relType As Relationship.RelationType) As String
      Dim names As List(Of String) = NamesFor(ft, p, relType)
      Return If(names.Count > 0, names(0), "")
   End Function

   Private Function NamesFor(ft As FamilyTree, p As Person, relType As Relationship.RelationType) As List(Of String)
      Return p.Relationships.
      Where(Function(r) r.Type = relType).
      Select(Function(r) ft.People.FirstOrDefault(Function(x) x.ID = r.OtherId)).
      Where(Function(other) other IsNot Nothing).
      Select(Function(other) $"{other.FirstName} {other.LastName}".Trim()).
      ToList()
   End Function

   Private Sub LoadMembers(ft As FamilyTree)
      lvMembers.BeginUpdate()
      lvMembers.Items.Clear()

      For Each p As Person In ft.People
         Dim item As New ListViewItem(p.ID)
         item.SubItems.Add(p.FirstName)
         item.SubItems.Add(p.LastName)

         Select Case p.Sex
            Case 0 : item.SubItems.Add("?")
            Case 1 : item.SubItems.Add("M")
            Case 2 : item.SubItems.Add("F")
         End Select

         If p.BirthDate.HasValue Then
            item.SubItems.Add(p.BirthDate.Value.ToString("yyyy-MM-dd"))
         Else
            item.SubItems.Add("")
         End If

         If p.DeathDate.HasValue Then
            item.SubItems.Add(p.DeathDate.Value.ToString("yyyy-MM-dd"))
         Else
            item.SubItems.Add("")
         End If

         item.SubItems.Add(NameFor(ft, p, Relationship.RelationType.Father))
         item.SubItems.Add(NameFor(ft, p, Relationship.RelationType.Mother))
         item.SubItems.Add(String.Join(", ", NamesFor(ft, p, Relationship.RelationType.Spouse)))

         item.Tag = p
         lvMembers.Items.Add(item)
      Next

      lvMembers.EndUpdate()
   End Sub

   Private Sub frmListMembers_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      lvMembers.View = View.Details
      lvMembers.FullRowSelect = True
      lvMembers.Font = New Font("Segoe UI", 9, FontStyle.Regular)
      lvMembers.Columns.Add("ID", 23, HorizontalAlignment.Left)
      lvMembers.Columns.Add("First Name", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Last Name", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Sex", 30, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Birth Date", 80, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Death Date", 80, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Father", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Mother", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Spouse", 100, HorizontalAlignment.Left)

      LoadMembers(Tree)
   End Sub
End Class