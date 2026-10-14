'--------------------------------------------------------------------------------------------------
' Bloodlines: Relationship.vb: Relationship class
'    © 2026 Remus Rigo
'       v1.0.20260925
'--------------------------------------------------------------------------------------------------

Namespace Bloodlines

   Public Class Relationship
      Public Enum RelationType
         Father
         Mother
         Spouse
         Divorced    ' former spouse
         Partner     ' together, not married
      End Enum

      Public Property RelativeID As Integer    ' who this points to
      Public Property Type As RelationType     ' what RelativeID is to me

      ' Spouse, Divorced and Partner all link a couple: stored on both people and drawn
      ' side by side in the chart. (A Shared function, not a property, so it isn't saved to the JSON.)
      Public Shared Function IsCouple(type As RelationType) As Boolean
         Return type = RelationType.Spouse OrElse type = RelationType.Divorced OrElse type = RelationType.Partner
      End Function

   End Class

End Namespace
