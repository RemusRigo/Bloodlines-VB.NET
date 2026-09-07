Imports System.IO
Imports System.Text.Json
Imports Bloodlines.Bloodlines.Relationship


Namespace Bloodlines

   Public Class FamilyTree
      Public Property Version As Integer = 1   ' stamp the format; useful later
      Public Property People As New List(Of Person)

      Public Shared ReadOnly Property DefaultPath As String
         Get
            Return Path.Combine(Application.StartupPath, "FamilyTree.json")
         End Get
      End Property

      Public Function NextID() As Integer
         If People.Count = 0 Then Return 1
         Return People.Max(Function(p) p.ID) + 1
      End Function

      ' A child is anyone who lists me as their Father or Mother
      Public Function GetChildren(personId As Integer) As List(Of Person)
         Return People.Where(Function(p) p.Relationships.Any(Function(r) r.OtherId = personId AndAlso (r.Type = RelationType.Father OrElse r.Type = RelationType.Mother))).ToList()
      End Function

      Public Sub Save(path As String)
         Dim opts As New JsonSerializerOptions With {.WriteIndented = True}
         File.WriteAllText(path, JsonSerializer.Serialize(Me, opts))
      End Sub

      Public Shared Function Load(path As String) As FamilyTree
         If Not File.Exists(path) Then Return New FamilyTree()
         Return JsonSerializer.Deserialize(Of FamilyTree)(File.ReadAllText(path))
      End Function

   End Class

End Namespace