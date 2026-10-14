'--------------------------------------------------------------------------------------------------
' AppSetting.vb: Load/Save settings to JSON file
'    © 2026 Remus Rigo
'       v1.0.20260929
'--------------------------------------------------------------------------------------------------

Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes

' What the tree chart shows around the focus person. Saved to the settings file as its number.
Public Enum TreeViewMode
   Normal = 0     ' ancestors above, descendants below
   Siblings = 1   ' as Normal, plus the focus person's siblings beside them
   Full = 2       ' everyone in the tree
End Enum

Public Class AppSettings

   Public Property ViewMode As TreeViewMode = TreeViewMode.Normal
   Private ReadOnly Property SettingsFile As String
      Get
         Dim exePath = Application.ExecutablePath
         Return Path.ChangeExtension(exePath, ".cfg")
      End Get
   End Property

   '-----------------------------------------------------------------------------------------------
   ' Save Settings to JSON file
   Public Sub SaveSettings()
      Dim jsonRoot As New JsonObject From {
          {"ViewMode", JsonValue.Create(CInt(ViewMode))}
      }

      Dim options As New JsonSerializerOptions With {.WriteIndented = True}
      Try
         File.WriteAllText(SettingsFile, jsonRoot.ToJsonString(options))
      Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
         ' Failed to save settings
      End Try
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' Load Settings from JSON file
   Public Sub LoadSettings()
      If Not File.Exists(SettingsFile) Then Return
      Try
         Dim root As JsonNode = JsonNode.Parse(File.ReadAllText(SettingsFile))
         Dim node = TryCast(root?("ViewMode"), JsonValue)
         Dim value As Integer
         If node IsNot Nothing AndAlso node.TryGetValue(value) AndAlso [Enum].IsDefined(GetType(TreeViewMode), value) Then
            ViewMode = CType(value, TreeViewMode)
         End If
      Catch ex As Exception
         ' Corrupt/unreadable settings file: keep defaults
      End Try
   End Sub

End Class