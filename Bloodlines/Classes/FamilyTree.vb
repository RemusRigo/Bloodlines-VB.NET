Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Bloodlines.Bloodlines.Relationship


Namespace Bloodlines

   Public Class FamilyTree
      Public Property Version As Integer = 1   ' stamp the format; useful later
      Public Property People As New List(Of Person)
      <JsonIgnore>
      Public Property SourcePath As String

      ' Folder that holds all tree files: <StartupPath>\Trees
      Public Shared ReadOnly Property TreesFolder As String
         Get
            Return Path.Combine(Application.StartupPath, "Trees")
         End Get
      End Property

      <JsonIgnore>
      Public ReadOnly Property Name As String
         Get
            Return If(String.IsNullOrEmpty(SourcePath), Nothing,
                      Path.GetFileNameWithoutExtension(SourcePath))
         End Get
      End Property

      ' Required by JsonSerializer and by Load()'s "new empty tree" fallbacks.
      Public Sub New()
      End Sub

      ' Declare a tree by name; its file lives at <StartupPath>\Trees\<treeName>.json
      Public Sub New(treeName As String)
         If String.IsNullOrWhiteSpace(treeName) Then
            Throw New ArgumentException("A tree name is required.", NameOf(treeName))
         End If
         SourcePath = Path.Combine(TreesFolder, treeName & ".json")
      End Sub

      Public Function NextID() As Integer
         If People.Count = 0 Then Return 1
         Return People.Max(Function(p) p.ID) + 1
      End Function

      ' A child is anyone who lists me as their Father or Mother
      Public Function GetChildren(personId As Integer) As List(Of Person)
         Return People.Where(Function(p) p.Relationships.Any(Function(r) r.OtherId = personId AndAlso (r.Type = RelationType.Father OrElse r.Type = RelationType.Mother))).ToList()
      End Function

      Public Sub Save()
         If String.IsNullOrEmpty(SourcePath) Then
            Throw New InvalidOperationException("This family tree has no SourcePath yet; construct it with New FamilyTree(treeName) or call Save(path) once.")
         End If
         Save(SourcePath)
      End Sub

      Public Sub Save(path As String)
         Dim opts As New JsonSerializerOptions With {.WriteIndented = True}
         Directory.CreateDirectory(IO.Path.GetDirectoryName(path))   ' make sure \Trees exists
         File.WriteAllText(path, JsonSerializer.Serialize(Me, opts))
         SourcePath = path
      End Sub

      Public Shared Function Load(path As String) As FamilyTree
         Dim tree As FamilyTree
         If Not File.Exists(path) Then
            tree = New FamilyTree()
         Else
            tree = JsonSerializer.Deserialize(Of FamilyTree)(File.ReadAllText(path))
            If tree Is Nothing Then tree = New FamilyTree()
         End If
         tree.SourcePath = path
         Return tree
      End Function


   End Class

End Namespace
