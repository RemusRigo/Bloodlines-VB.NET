'--------------------------------------------------------------------------------------------------
' Bloodlines: FamilyTree.vb: Family tree class
'    © 2026 Remus Rigo
'       v1.0.20260925
'--------------------------------------------------------------------------------------------------

Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Bloodlines.Bloodlines.Relationship


Namespace Bloodlines

   Public Class FamilyTree
      ' Bump when the JSON shape changes, and add a step to Migrate().
      Public Const CurrentVersion As Double = 1.03

      Public Property Version As Double = CurrentVersion
      Public Property People As New List(Of Person)
      <JsonIgnore>
      Public Property SourcePath As String

      ' Folder that holds all tree files: <StartupPath>\Trees
      Public Shared ReadOnly Property TreesFolder As String
         Get
            Return Path.Combine(Application.StartupPath, "Trees")
         End Get
      End Property

      ' Full path of the tree file called `treeName` (bare name, no extension) in TreesFolder.
      ' Any folder part in treeName is dropped, so a hand-edited "..\x" can't escape TreesFolder.
      Public Shared Function PathFor(treeName As String) As String
         Return Path.Combine(TreesFolder, Path.GetFileName(treeName) & ".json")
      End Function

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

      ' Photos live in a folder named after the tree, next to the tree file:
      '   <StartupPath>\Trees\<tree name>\<person ID>.jpg  (SavePhoto always writes .png, so it wins over an older .jpg)
      Private Shared ReadOnly PhotoExtensions() As String = {".png", ".jpg", ".jpeg", ".bmp"}

      <JsonIgnore>
      Public ReadOnly Property PhotoFolder As String
         Get
            If String.IsNullOrEmpty(SourcePath) Then Return Nothing
            Return Path.Combine(Path.GetDirectoryName(SourcePath), Name)
         End Get
      End Property

      ' Full path of the person's photo, or Nothing when there isn't one.
      Public Function FindPhoto(personId As Integer) As String
         Dim folder As String = PhotoFolder
         If folder Is Nothing OrElse Not Directory.Exists(folder) Then Return Nothing
         For Each ext As String In PhotoExtensions
            Dim file As String = Path.Combine(folder, personId & ext)
            If IO.File.Exists(file) Then Return file
         Next
         Return Nothing
      End Function

      ' Copies the chosen image into the photo folder as <person ID>.png (converting any format).
      Public Sub SavePhoto(personId As Integer, sourceFile As String)
         Dim folder As String = PhotoFolder
         If folder Is Nothing Then
            Throw New InvalidOperationException("This family tree has no SourcePath yet, so there is no photo folder.")
         End If
         Directory.CreateDirectory(folder)
         Dim dest As String = Path.Combine(folder, personId & ".png")
         If String.Equals(Path.GetFullPath(sourceFile), Path.GetFullPath(dest), StringComparison.OrdinalIgnoreCase) Then Return

         Using fs As New FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using img As Image = Image.FromStream(fs)
               img.Save(dest, System.Drawing.Imaging.ImageFormat.Png)
            End Using
         End Using
      End Sub

      Public Function NextID() As Integer
         If People.Count = 0 Then Return 1
         Return People.Max(Function(p) p.ID) + 1
      End Function

      ' A child is anyone who lists me as their Father or Mother
      Public Function GetChildren(personId As Integer) As List(Of Person)
         Return People.Where(Function(p) p.Relationships.Any(Function(r) r.RelativeID = personId AndAlso (r.Type = RelationType.Father OrElse r.Type = RelationType.Mother))).ToList()
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
         Version = CurrentVersion
         File.WriteAllText(path, JsonSerializer.Serialize(Me, opts))
         SourcePath = path
      End Sub

      Public Shared Function Load(path As String) As FamilyTree
         Dim tree As FamilyTree
         If Not File.Exists(path) Then
            tree = New FamilyTree()
         Else
            Try
               tree = JsonSerializer.Deserialize(Of FamilyTree)(File.ReadAllText(path))
            Catch ex As JsonException
               Throw New InvalidDataException($"'{IO.Path.GetFileName(path)}' is not a valid family tree file.", ex)
            End Try
            If tree Is Nothing Then tree = New FamilyTree()
            If tree.Version > CurrentVersion Then
               Throw New InvalidDataException($"'{IO.Path.GetFileName(path)}' was saved by a newer version of Bloodlines (file format {tree.Version}, this app supports up to {CurrentVersion}).")
            End If
            tree.Migrate()
         End If
         tree.SourcePath = path
         Return tree
      End Function

      ' Upgrade an older file in memory to the current format; it is re-stamped on the next Save.
      Private Sub Migrate()
         ' 1.00 -> 1.01: Person.Sex lost its "Unknown = 0" member; null now means not specified.
         If Version < 1.01 Then
            For Each p As Person In People
               If p.Sex.HasValue AndAlso Not [Enum].IsDefined(GetType(Person.SexType), p.Sex.Value) Then p.Sex = Nothing
            Next
            Version = 1.01
         End If

         ' 1.01 -> 1.02: Person gained BirthName, BirthPlace and DeathPlace (optional; nothing to convert).
         If Version < 1.02 Then Version = 1.02

         ' 1.02 -> 1.03: Person gained TreeLink (optional; nothing to convert).
         If Version < 1.03 Then Version = 1.03
      End Sub


   End Class

End Namespace
