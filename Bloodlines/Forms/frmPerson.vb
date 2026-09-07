Imports System.ComponentModel
Imports Bloodlines.Bloodlines

Public Class frmPerson
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Friend Property Tree As FamilyTree

   Private Sub frmPerson_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      txtBoxID.Text = Tree.NextID().ToString()
      cbSex.DataSource = [Enum].GetValues(GetType(Person.SexType))
   End Sub

   Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
      Dim id As Integer
      If Not Integer.TryParse(txtBoxID.Text, id) Then
         id = Tree.NextID()
      End If

      Dim sex As Person.SexType = Person.SexType.Unknown
      If cbSex.SelectedItem IsNot Nothing Then
         sex = CType(cbSex.SelectedItem, Person.SexType)
      End If

      Dim birthDate As Date? = Nothing
      If chkBoxBirthDate.Checked Then
         birthDate = dtPickerBirthDate.Value
      End If
      'Dim strBirthDate As String = If(birthDate.HasValue, birthDate.Value.ToString("yyyy-MM-dd"), "")
      'Dim strBirthDate As String = If(birthDate.HasValue, birthDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), "")

      Dim deathDate As Date? = Nothing
      If chkBoxDeathDate.Checked Then
         deathDate = dtPickerDeathDate.Value
      End If
      'Dim strDeathDate As String = If(deathDate.HasValue, deathDate.Value.ToString("yyyy-MM-dd"), "")
      'Dim strDeathDate As String = If(deathDate.HasValue, deathDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), "")

      Tree.People.Add(New Person With {
         .ID = id,
         .FirstName = txtBoxFirstName.Text,
         .LastName = txtBoxLastName.Text,
         .Sex = sex,
         .BirthDate = birthDate,
         .DeathDate = deathDate
      })
      Tree.Save(FamilyTree.DefaultPath)

      txtBoxFirstName.Clear()
      txtBoxLastName.Clear()
      txtBoxID.Text = Tree.NextID().ToString()

      Close()
   End Sub

   Private Sub chkBoxBirthDate_CheckedChanged(sender As Object, e As EventArgs) Handles chkBoxBirthDate.CheckedChanged
      dtPickerBirthDate.Enabled = chkBoxBirthDate.Checked
   End Sub

   Private Sub chkBoxDeathDate_CheckStateChanged(sender As Object, e As EventArgs) Handles chkBoxDeathDate.CheckStateChanged
      dtPickerDeathDate.Enabled = chkBoxDeathDate.Checked
   End Sub

End Class
