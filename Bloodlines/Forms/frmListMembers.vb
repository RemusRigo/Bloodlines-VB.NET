'--------------------------------------------------------------------------------------------------
' Bloodlines: frmListMembers.vb: List members form
'    © 2026 Remus Rigo
'       v1.0.20260925
'--------------------------------------------------------------------------------------------------

Imports System.ComponentModel
Imports Bloodlines.Bloodlines

Public Class frmListMembers
   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Public Property Tree As FamilyTree

   ' Column indexes, in the order frmListMembers_Load adds them.
   Private Const ColID As Integer = 0
   Private Const ColSex As Integer = 3
   Private Const ColBirth As Integer = 4
   Private Const ColDeath As Integer = 5

   Private ReadOnly _sorter As New MemberComparer()
   Private _titles() As String          ' column titles without the sort arrow

   ' Sorts the member list by one column. IDs and dates compare by value; everything else as
   ' text (culture-aware, so diacritics sort where readers expect). Blank cells always go last,
   ' in either direction, and ties fall back to ID so the order is stable.
   Private NotInheritable Class MemberComparer
      Implements IComparer

      Public Column As Integer = ColID
      Public Descending As Boolean

      Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
         Dim a As ListViewItem = DirectCast(x, ListViewItem), b As ListViewItem = DirectCast(y, ListViewItem)
         Dim pa As Person = DirectCast(a.Tag, Person), pb As Person = DirectCast(b.Tag, Person)

         Dim ea As Boolean = IsBlank(a, pa), eb As Boolean = IsBlank(b, pb)
         If ea <> eb Then Return If(ea, 1, -1)

         Dim result As Integer = 0
         If Not ea Then
            Select Case Column
               Case ColID : result = pa.ID.CompareTo(pb.ID)
               Case ColBirth : result = pa.BirthDate.Value.CompareTo(pb.BirthDate.Value)
               Case ColDeath : result = pa.DeathDate.Value.CompareTo(pb.DeathDate.Value)
               Case Else : result = String.Compare(a.SubItems(Column).Text, b.SubItems(Column).Text, StringComparison.CurrentCultureIgnoreCase)
            End Select
            If Descending Then result = -result
         End If
         If result = 0 Then result = pa.ID.CompareTo(pb.ID)
         Return result
      End Function

      Private Function IsBlank(item As ListViewItem, p As Person) As Boolean
         Select Case Column
            Case ColID : Return False
            Case ColBirth : Return Not p.BirthDate.HasValue
            Case ColDeath : Return Not p.DeathDate.HasValue
            Case ColSex : Return Not p.Sex.HasValue
            Case Else : Return String.IsNullOrWhiteSpace(item.SubItems(Column).Text)
         End Select
      End Function
   End Class

   ' Click a header to sort by it; click it again to reverse the order.
   Private Sub lvMembers_ColumnClick(sender As Object, e As ColumnClickEventArgs) Handles lvMembers.ColumnClick
      If e.Column = _sorter.Column Then
         _sorter.Descending = Not _sorter.Descending
      Else
         _sorter.Column = e.Column
         _sorter.Descending = False
      End If
      lvMembers.Sort()
      ShowSortArrow()
   End Sub

   ' Marks the sorted column's title with ▲ (ascending) or ▼ (descending).
   Private Sub ShowSortArrow()
      For i As Integer = 0 To lvMembers.Columns.Count - 1
         lvMembers.Columns(i).Text = _titles(i) &
            If(i = _sorter.Column, If(_sorter.Descending, " ▼", " ▲"), "")
      Next
   End Sub

   Private Function NameFor(ft As FamilyTree, p As Person, relType As Relationship.RelationType) As String
      Dim names As List(Of String) = NamesFor(ft, p, relType)
      Return If(names.Count > 0, names(0), "")
   End Function

   Private Function NamesFor(ft As FamilyTree, p As Person, relType As Relationship.RelationType) As List(Of String)
      Return p.Relationships.
      Where(Function(r) r.Type = relType).
      Select(Function(r) ft.People.FirstOrDefault(Function(x) x.ID = r.RelativeID)).
      Where(Function(other) other IsNot Nothing).
      Select(Function(other) $"{other.FirstName} {other.LastName}".Trim()).
      ToList()
   End Function

   Private Sub LoadMembers(ft As FamilyTree)
      lvMembers.BeginUpdate()
      lvMembers.Items.Clear()

      For Each p As Person In ft.People
         Dim item As New ListViewItem(p.ID.ToString())
         item.SubItems.Add(p.FirstName)
         item.SubItems.Add(p.LastName)

         Select Case p.Sex
            Case Person.SexType.Male : item.SubItems.Add("M")
            Case Person.SexType.Female : item.SubItems.Add("F")
            Case Else : item.SubItems.Add("?")
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
         item.SubItems.Add(String.Join(", ", NamesFor(ft, p, Relationship.RelationType.Spouse).
                                             Concat(NamesFor(ft, p, Relationship.RelationType.Partner).Select(Function(s) s & " (partner)")).
                                             Concat(NamesFor(ft, p, Relationship.RelationType.Divorced).Select(Function(s) s & " (divorced)"))))

         item.Tag = p
         lvMembers.Items.Add(item)
      Next

      lvMembers.EndUpdate()
   End Sub

   Private Sub frmListMembers_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      lvMembers.View = View.Details
      lvMembers.FullRowSelect = True
      'lvMembers.Font = New Font("Segoe UI", 9, FontStyle.Regular)
      lvMembers.Columns.Add("ID", 45, HorizontalAlignment.Left)     ' room for 3-digit IDs plus the sort arrow
      lvMembers.Columns.Add("First Name", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Last Name", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Sex", 30, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Birth Date", 80, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Death Date", 80, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Father", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Mother", 100, HorizontalAlignment.Left)
      lvMembers.Columns.Add("Spouse", 100, HorizontalAlignment.Left)
      _titles = lvMembers.Columns.Cast(Of ColumnHeader)().Select(Function(c) c.Text).ToArray()

      LoadMembers(Tree)

      ' Sorted by ID to start with; set after loading so the items are sorted once, not per Add.
      lvMembers.ListViewItemSorter = _sorter
      ShowSortArrow()
   End Sub
End Class