Imports System.Text.Json.Serialization

Namespace Bloodlines

   Public Class Person

      Public Enum SexType
         Male = 1
         Female = 2
      End Enum

      Public Property ID As Integer
      Public Property FirstName As String
      Public Property LastName As String
      Public Property BirthName As String
      Public Property Sex As SexType?           ' Nothing = not specified
      Public Property BirthDate As Date?        ' Nothing = unknown
      Public Property BirthPlace As String
      Public Property DeathDate As Date?
      Public Property DeathPlace As String
      Public Property Notes As String
      Public Property Relationships As New List(Of Relationship)

      ' "(1979 - )" or "(1940 - 2010)"; empty when neither date is known.
      <JsonIgnore>
      Public ReadOnly Property LifeSpan As String
         Get
            If Not BirthDate.HasValue AndAlso Not DeathDate.HasValue Then Return ""
            Dim born As String = If(BirthDate.HasValue, BirthDate.Value.Year.ToString(), "")
            Dim died As String = If(DeathDate.HasValue, DeathDate.Value.Year.ToString(), "")
            Return $"({born} - {died})"
         End Get
      End Property

   End Class

End Namespace