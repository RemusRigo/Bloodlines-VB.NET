Imports System.ComponentModel
Imports Bloodlines.Bloodlines

Public Class frmPerson
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Friend Property Tree As FamilyTree
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Friend Property newID As Boolean
   Private currentID As Integer = -1

   Private NotInheritable Class PersonItem
      Public Property Person As Person
      Public Overrides Function ToString() As String
         Dim n As String = $"{Person.FirstName} {Person.LastName}".Trim()
         If n.Length = 0 Then n = "#" & Person.ID
         Return $"{n}  (#{Person.ID})"
      End Function
   End Class

   Private NotInheritable Class ConnectionItem
      Public Property Relationship As Relationship
      Public Property Tree As FamilyTree
      Public Overrides Function ToString() As String
         Dim other As Person = Tree.People.FirstOrDefault(Function(p) p.ID = Relationship.OtherId)
         Dim n As String = If(other Is Nothing, "#" & Relationship.OtherId,
                           $"{other.FirstName} {other.LastName}".Trim())
         Return $"{Relationship.Type}: {n}"
      End Function
   End Class


   Private Sub ShowPerson(index As Integer)
      currentID = index
      If currentID = -1 Then
         txtBoxID.Text = Tree.NextID().ToString()
         txtBoxFirstName.Clear()
         txtBoxLastName.Clear()
         cbSex.SelectedItem = Person.SexType.Unknown
         chkBoxBirthDate.Checked = False
         chkBoxDeathDate.Checked = False
         dtPickerBirthDate.Enabled = False
         dtPickerDeathDate.Enabled = False
         txtBoxNotes.Clear()
      Else
         Dim p As Person = Tree.People(index)

         txtBoxID.Text = p.ID.ToString()
         txtBoxFirstName.Text = p.FirstName
         txtBoxLastName.Text = p.LastName
         cbSex.SelectedItem = If(p.Sex.HasValue, p.Sex.Value, Person.SexType.Unknown)

         chkBoxBirthDate.Checked = p.BirthDate.HasValue
         If p.BirthDate.HasValue Then dtPickerBirthDate.Value = p.BirthDate.Value
         dtPickerBirthDate.Enabled = chkBoxBirthDate.Checked

         chkBoxDeathDate.Checked = p.DeathDate.HasValue
         If p.DeathDate.HasValue Then dtPickerDeathDate.Value = p.DeathDate.Value
         dtPickerDeathDate.Enabled = chkBoxDeathDate.Checked

         txtBoxNotes.Text = p.Notes
      End If

      ' Prev ID / Next ID
      Dim hasPeople As Boolean = Tree.People.Count > 0
      btnPrev.Enabled = hasPeople
      btnNext.Enabled = hasPeople
      Me.Text = If(currentID >= 0, $"Person {currentID + 1} of {Tree.People.Count}", "Person (new)")

      LoadMembers()
      RefreshConnections()
   End Sub

   Private Sub ApplyTo(p As Person)
      p.FirstName = txtBoxFirstName.Text
      p.LastName = txtBoxLastName.Text
      p.Sex = CType(cbSex.SelectedItem, Person.SexType)
      p.BirthDate = If(chkBoxBirthDate.Checked, CType(dtPickerBirthDate.Value, Date?), Nothing)
      p.DeathDate = If(chkBoxDeathDate.Checked, CType(dtPickerDeathDate.Value, Date?), Nothing)
      p.Notes = txtBoxNotes.Text
   End Sub


   ' Save what's on screen. Returns False to cancel navigation (e.g. failed validation).
   Private Function CommitCurrent() As Boolean
      If currentID >= 0 AndAlso currentID < Tree.People.Count Then
         ApplyTo(Tree.People(currentID))
      Else
         ' unsaved new person – only add if something was typed
         If txtBoxFirstName.Text.Trim().Length = 0 AndAlso txtBoxLastName.Text.Trim().Length = 0 Then
            Return True
         End If
         Dim p As New Person With {.ID = Tree.NextID()}
         ApplyTo(p)
         Tree.People.Add(p)
         currentID = Tree.People.Count - 1
      End If
      Tree.Save()
      Return True
   End Function

   Private Sub LoadMembers()
      cbPerson.Items.Clear()
      Dim self As Person = If(currentID >= 0 AndAlso currentID < Tree.People.Count, Tree.People(currentID), Nothing)
      For Each p As Person In Tree.People
         If p Is self Then Continue For          ' can't relate a person to themselves
         cbPerson.Items.Add(New PersonItem With {.Person = p})
      Next
      If cbPerson.Items.Count > 0 Then cbPerson.SelectedIndex = 0
      btnAddConection.Enabled = cbPerson.Items.Count > 0
   End Sub

   Private Sub RefreshConnections()
      lstBoxConnections.Items.Clear()
      If currentID < 0 OrElse currentID >= Tree.People.Count Then Return
      For Each r As Relationship In Tree.People(currentID).Relationships
         lstBoxConnections.Items.Add(New ConnectionItem With {.Relationship = r, .Tree = Tree})
      Next
   End Sub


   Private Sub frmPerson_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      txtBoxID.Text = Tree.NextID().ToString()

      cbSex.DataSource = [Enum].GetValues(GetType(Person.SexType))

      cbRelation.DataSource = [Enum].GetValues(GetType(Relationship.RelationType))
      If cbRelation.Items.Count > 0 Then cbRelation.SelectedIndex = 0

      If newID OrElse Tree.People.Count = 0 Then
         ShowPerson(-1)
      Else
         ShowPerson(0)
      End If
   End Sub


   Private Sub chkBoxBirthDate_CheckedChanged(sender As Object, e As EventArgs) Handles chkBoxBirthDate.CheckedChanged
      dtPickerBirthDate.Enabled = chkBoxBirthDate.Checked
   End Sub

   Private Sub chkBoxDeathDate_CheckStateChanged(sender As Object, e As EventArgs) Handles chkBoxDeathDate.CheckStateChanged
      dtPickerDeathDate.Enabled = chkBoxDeathDate.Checked
   End Sub

   Private Sub btnPrev_Click(sender As Object, e As EventArgs) Handles btnPrev.Click
      If Not CommitCurrent() Then Return
      If Tree.People.Count = 0 Then Return
      Dim i As Integer = If(currentID <= 0, Tree.People.Count - 1, currentID - 1)
      ShowPerson(i)
   End Sub

   Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
      If Not CommitCurrent() Then Return
      If Tree.People.Count = 0 Then Return
      Dim i As Integer = If(currentID >= Tree.People.Count - 1, 0, currentID + 1)
      ShowPerson(i)
   End Sub

   Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
      If Not CommitCurrent() Then Return
      Close()
   End Sub

   Private Sub btnAddConection_Click(sender As Object, e As EventArgs) Handles btnAddConection.Click
      If Not CommitCurrent() Then Return           ' make sure this person exists & is saved
      If currentID < 0 Then
         MessageBox.Show("Enter at least a name first so the person can be saved.")
         Return
      End If

      Dim sel As PersonItem = TryCast(cbPerson.SelectedItem, PersonItem)
      If sel Is Nothing Then
         MessageBox.Show("Pick a person to connect to. (Need at least two people in the tree.)")
         Return
      End If
      If cbRelation.SelectedItem Is Nothing Then
         MessageBox.Show("Pick a relation type.")
         Return
      End If

      Dim relType As Relationship.RelationType = CType(cbRelation.SelectedItem, Relationship.RelationType)
      Dim self As Person = Tree.People(currentID)

      ' no duplicates
      If self.Relationships.Any(Function(r) r.OtherId = sel.Person.ID AndAlso r.Type = relType) Then
         MessageBox.Show("That connection already exists.")
         Return
      End If

      self.Relationships.Add(New Relationship With {.OtherId = sel.Person.ID, .Type = relType})

      ' keep spouse links symmetric
      If relType = Relationship.RelationType.Spouse Then
         If Not sel.Person.Relationships.Any(Function(r) r.OtherId = self.ID AndAlso r.Type = Relationship.RelationType.Spouse) Then
            sel.Person.Relationships.Add(New Relationship With {.OtherId = self.ID, .Type = Relationship.RelationType.Spouse})
         End If
      End If

      Tree.Save()
      RefreshConnections()
   End Sub

   Private Sub btnRemoveConection_Click(sender As Object, e As EventArgs) Handles btnRemoveConection.Click
      Dim item As ConnectionItem = TryCast(lstBoxConnections.SelectedItem, ConnectionItem)
      If item Is Nothing OrElse currentID < 0 Then Return

      Dim self As Person = Tree.People(currentID)
      self.Relationships.Remove(item.Relationship)

      If item.Relationship.Type = Relationship.RelationType.Spouse Then
         Dim other As Person = Tree.People.FirstOrDefault(Function(p) p.ID = item.Relationship.OtherId)
         If other IsNot Nothing Then
            other.Relationships.RemoveAll(Function(r) r.OtherId = self.ID AndAlso r.Type = Relationship.RelationType.Spouse)
         End If
      End If

      Tree.Save()
      RefreshConnections()
   End Sub

End Class
