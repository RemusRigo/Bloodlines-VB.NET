Namespace Bloodlines

   Public Class Person

      Public Enum SexType
         Unknown
         Male
         Female
      End Enum

      Public Property ID As Integer
      Public Property FirstName As String
      Public Property LastName As String
      Public Property Sex As SexType?            ' Nothing = unknown
      Public Property BirthDate As Date?       ' Nothing = unknown
      Public Property DeathDate As Date?
      Public Property Notes As String
      Public Property Relationships As New List(Of Relationship)

   End Class

End Namespace