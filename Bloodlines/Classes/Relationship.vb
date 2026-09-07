Namespace Bloodlines

   Public Class Relationship
      Public Enum RelationType
         Father
         Mother
         Spouse
      End Enum

      Public Property OtherId As Integer       ' who this points to
      Public Property Type As RelationType     ' what OtherId is to me

   End Class

End Namespace
