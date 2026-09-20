Namespace Bloodlines

   Public Class Relationship
      Public Enum RelationType
         Father
         Mother
         Spouse
      End Enum

      Public Property RelativeID As Integer    ' who this points to
      Public Property Type As RelationType     ' what RelativeID is to me

   End Class

End Namespace
