Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports Bloodlines.Bloodlines

' Genealogy "hourglass" chart centred on one person:
'   - the focus person sits in the middle
'   - descendants (children, grandchildren, ...) fan downward
'   - ancestors (parents, grandparents, ...) fan upward
' Boxes are joined by the usual elbow connector lines.
Public Class frmTree
   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Public Property Tree As FamilyTree

   ' --- layout constants (pixels) ---
   Private Const NodeWidth As Integer = 150
   Private Const NodeHeight As Integer = 52
   Private Const HGap As Integer = 26      ' gap between sibling subtrees
   Private Const VGap As Integer = 46      ' gap between generations
   Private Const Margin As Integer = 24
   Private Const MaxDepth As Integer = 20  ' guard against bad data

   ' One box in the chart.
   Private Class Node
      Public Person As Person
      Public ReadOnly Kids As New List(Of Node)      ' drawn below, connected downward
      Public ReadOnly Parents As New List(Of Node)   ' drawn above, connected upward
      Public Cx As Integer                           ' box centre X
      Public Y As Integer                            ' box top Y
      Public W As Integer                            ' subtree width
   End Class

   Private ReadOnly _byId As New Dictionary(Of Integer, Person)
   Private ReadOnly _allNodes As New List(Of Node)
   Private _focus As Node
   Private _focusTopY As Integer

   Private _nameFont As Font
   Private _dateFont As Font
   Private ReadOnly _tip As New ToolTip()

   Private Sub frmTree_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      _nameFont = New Font(Font, FontStyle.Bold)
      _dateFont = New Font(Font.FontFamily, Math.Max(6.0F, Font.Size - 1.5F))

      If Tree Is Nothing OrElse Tree.People Is Nothing OrElse Tree.People.Count = 0 Then
         pnlChart.Invalidate()
         Return
      End If

      For Each p As Person In Tree.People
         _byId(p.ID) = p
      Next

      Dim ordered = Tree.People.
         OrderBy(Function(p) p.LastName).
         ThenBy(Function(p) p.FirstName).
         Select(Function(p) New FocusItem With {.Person = p}).
         ToArray()
      cboFocus.Items.AddRange(ordered)

      ' Prefer someone who actually has both ancestors and descendants to show.
      Dim def As FocusItem = ordered.FirstOrDefault(
         Function(fi) HasKnownParent(fi.Person) AndAlso Tree.GetChildren(fi.Person.ID).Count > 0)
      If def Is Nothing Then def = ordered.FirstOrDefault()
      cboFocus.SelectedItem = def   ' fires SelectedIndexChanged -> build + layout
   End Sub

   Private Sub frmTree_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
      If _nameFont IsNot Nothing Then _nameFont.Dispose()
      If _dateFont IsNot Nothing Then _dateFont.Dispose()
      _tip.Dispose()
   End Sub

   Private Sub cboFocus_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFocus.SelectedIndexChanged
      Dim item As FocusItem = TryCast(cboFocus.SelectedItem, FocusItem)
      If item Is Nothing Then Return
      BuildNodes(item.Person)
      LayoutNodes()
      pnlChart.AutoScrollPosition = New Point(0, 0)
      pnlChart.Invalidate()
   End Sub

   ' ---------------------------------------------------------------- model ---

   Private Function HasKnownParent(p As Person) As Boolean
      Return p.Relationships.Any(
         Function(r) (r.Type = Relationship.RelationType.Father OrElse r.Type = Relationship.RelationType.Mother) _
                     AndAlso _byId.ContainsKey(r.OtherId))
   End Function

   Private Sub BuildNodes(focus As Person)
      _focus = Nothing
      If focus Is Nothing Then Return

      Dim down As Node = BuildDown(focus, 0, New HashSet(Of Integer))
      Dim up As Node = BuildUp(focus, 0, New HashSet(Of Integer))
      down.Parents.AddRange(up.Parents)
      _focus = down
   End Sub

   Private Function BuildDown(p As Person, depth As Integer, path As HashSet(Of Integer)) As Node
      Dim n As New Node With {.Person = p}
      If depth >= MaxDepth OrElse Not path.Add(p.ID) Then Return n

      For Each child As Person In Tree.GetChildren(p.ID).
            OrderBy(Function(c) c.BirthDate).ThenBy(Function(c) c.ID)
         n.Kids.Add(BuildDown(child, depth + 1, path))
      Next

      path.Remove(p.ID)
      Return n
   End Function

   Private Function BuildUp(p As Person, depth As Integer, path As HashSet(Of Integer)) As Node
      Dim n As New Node With {.Person = p}
      If depth >= MaxDepth OrElse Not path.Add(p.ID) Then Return n

      Dim added As New HashSet(Of Integer)
      For Each r As Relationship In p.Relationships.
            Where(Function(x) x.Type = Relationship.RelationType.Father OrElse x.Type = Relationship.RelationType.Mother).
            OrderBy(Function(x) x.Type)
         Dim parent As Person = Nothing
         If _byId.TryGetValue(r.OtherId, parent) AndAlso added.Add(r.OtherId) Then
            n.Parents.Add(BuildUp(parent, depth + 1, path))
         End If
      Next

      path.Remove(p.ID)
      Return n
   End Function

   ' --------------------------------------------------------------- layout ---

   Private Sub LayoutNodes()
      _allNodes.Clear()
      If _focus Is Nothing Then
         pnlChart.AutoScrollMinSize = Size.Empty
         Return
      End If

      Dim kidsOf As Func(Of Node, List(Of Node)) = Function(n) n.Kids
      Dim parentsOf As Func(Of Node, List(Of Node)) = Function(n) n.Parents

      Measure(_focus, kidsOf, New HashSet(Of Node))
      Measure(_focus, parentsOf, New HashSet(Of Node))

      Dim maxAnc As Integer = MaxGen(_focus, parentsOf, 0, New HashSet(Of Node))
      _focusTopY = Margin + maxAnc * (NodeHeight + VGap)

      PlaceDown(_focus, 0, 0)
      PlaceUp(_focus)

      Dim seen As New HashSet(Of Node)
      Collect(_focus, kidsOf, seen, _allNodes)
      Collect(_focus, parentsOf, seen, _allNodes)

      Dim minLeft As Integer = _allNodes.Min(Function(n) n.Cx - NodeWidth \ 2)
      Dim dx As Integer = Margin - minLeft
      For Each n As Node In _allNodes
         n.Cx += dx
      Next

      Dim width As Integer = _allNodes.Max(Function(n) n.Cx + NodeWidth \ 2) + Margin
      Dim height As Integer = _allNodes.Max(Function(n) n.Y + NodeHeight) + Margin
      pnlChart.AutoScrollMinSize = New Size(width, height)
   End Sub

   Private Function Measure(node As Node, childrenOf As Func(Of Node, List(Of Node)), seen As HashSet(Of Node)) As Integer
      Dim kids As List(Of Node) = childrenOf(node)
      If Not seen.Add(node) OrElse kids.Count = 0 Then
         node.W = NodeWidth
         Return node.W
      End If

      Dim total As Integer = 0
      For i As Integer = 0 To kids.Count - 1
         total += Measure(kids(i), childrenOf, seen)
         If i < kids.Count - 1 Then total += HGap
      Next

      node.W = Math.Max(NodeWidth, total)
      Return node.W
   End Function

   Private Function MaxGen(node As Node, childrenOf As Func(Of Node, List(Of Node)), gen As Integer, seen As HashSet(Of Node)) As Integer
      If Not seen.Add(node) Then Return gen
      Dim m As Integer = gen
      For Each k As Node In childrenOf(node)
         m = Math.Max(m, MaxGen(k, childrenOf, gen + 1, seen))
      Next
      Return m
   End Function

   Private Sub Collect(node As Node, childrenOf As Func(Of Node, List(Of Node)), seen As HashSet(Of Node), into As List(Of Node))
      If Not seen.Add(node) Then Return
      into.Add(node)
      For Each k As Node In childrenOf(node)
         Collect(k, childrenOf, seen, into)
      Next
   End Sub

   Private Sub PlaceDown(node As Node, cx As Integer, gen As Integer)
      node.Cx = cx
      node.Y = _focusTopY + gen * (NodeHeight + VGap)
      If node.Kids.Count = 0 Then Return

      Dim total As Integer = node.Kids.Sum(Function(k) k.W) + HGap * (node.Kids.Count - 1)
      Dim start As Integer = cx - total \ 2
      For Each k As Node In node.Kids
         PlaceDown(k, start + k.W \ 2, gen + 1)
         start += k.W + HGap
      Next
   End Sub

   Private Sub PlaceUp(focus As Node)
      focus.Cx = 0
      focus.Y = _focusTopY
      PlaceParents(focus, 0, 1)
   End Sub

   Private Sub PlaceParents(node As Node, cx As Integer, gen As Integer)
      If node.Parents.Count = 0 Then Return

      Dim total As Integer = node.Parents.Sum(Function(p) p.W) + HGap * (node.Parents.Count - 1)
      Dim start As Integer = cx - total \ 2
      For Each p As Node In node.Parents
         p.Cx = start + p.W \ 2
         p.Y = _focusTopY - gen * (NodeHeight + VGap)
         start += p.W + HGap
         PlaceParents(p, p.Cx, gen + 1)
      Next
   End Sub

   ' --------------------------------------------------------------- paint ---

   Private Sub pnlChart_Paint(sender As Object, e As PaintEventArgs) Handles pnlChart.Paint
      Dim g As Graphics = e.Graphics
      g.SmoothingMode = SmoothingMode.AntiAlias
      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit
      g.TranslateTransform(pnlChart.AutoScrollPosition.X, pnlChart.AutoScrollPosition.Y)

      If _focus Is Nothing Then
         g.DrawString("Load a family tree, then pick a focus person.", Font, Brushes.Gray, Margin, Margin)
         Return
      End If

      Using pen As New Pen(Color.FromArgb(150, 150, 150))
         For Each n As Node In _allNodes
            DrawDownConnector(g, pen, n)
            DrawUpConnector(g, pen, n)
         Next
      End Using

      For Each n As Node In _allNodes
         DrawBox(g, n, n Is _focus)
      Next
   End Sub

   Private Sub DrawDownConnector(g As Graphics, pen As Pen, n As Node)
      If n.Kids.Count = 0 Then Return
      Dim fromY As Integer = n.Y + NodeHeight
      Dim busY As Integer = fromY + VGap \ 2
      g.DrawLine(pen, n.Cx, fromY, n.Cx, busY)

      Dim xs = n.Kids.Select(Function(k) k.Cx).ToList()
      g.DrawLine(pen, xs.Min(), busY, xs.Max(), busY)
      For Each k As Node In n.Kids
         g.DrawLine(pen, k.Cx, busY, k.Cx, k.Y)
      Next
   End Sub

   Private Sub DrawUpConnector(g As Graphics, pen As Pen, n As Node)
      If n.Parents.Count = 0 Then Return
      Dim fromY As Integer = n.Y
      Dim busY As Integer = fromY - VGap \ 2
      g.DrawLine(pen, n.Cx, fromY, n.Cx, busY)

      Dim xs = n.Parents.Select(Function(p) p.Cx).ToList()
      g.DrawLine(pen, xs.Min(), busY, xs.Max(), busY)
      For Each p As Node In n.Parents
         g.DrawLine(pen, p.Cx, busY, p.Cx, p.Y + NodeHeight)
      Next
   End Sub

   Private Sub DrawBox(g As Graphics, n As Node, isFocus As Boolean)
      Dim rect As New Rectangle(n.Cx - NodeWidth \ 2, n.Y, NodeWidth, NodeHeight)

      Dim fill As Color
      Select Case If(n.Person.Sex.HasValue, n.Person.Sex.Value, Person.SexType.Unknown)
         Case Person.SexType.Male : fill = Color.FromArgb(219, 234, 254)
         Case Person.SexType.Female : fill = Color.FromArgb(252, 228, 236)
         Case Else : fill = Color.FromArgb(238, 238, 238)
      End Select

      Using b As New SolidBrush(fill)
         g.FillRectangle(b, rect)
      End Using

      If isFocus Then
         Using pen As New Pen(Color.FromArgb(33, 102, 172), 2)
            g.DrawRectangle(pen, rect)
         End Using
      Else
         Using pen As New Pen(Color.FromArgb(120, 120, 120))
            g.DrawRectangle(pen, rect)
         End Using
      End If

      Dim name As String = $"{n.Person.FirstName} {n.Person.LastName}".Trim()
      If name.Length = 0 Then name = "#" & n.Person.ID

      Dim life As String = LifeSpan(n.Person)
      Dim nameArea As New Rectangle(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8 - If(life.Length > 0, 14, 0))
      TextRenderer.DrawText(g, name, _nameFont, nameArea, Color.Black,
                            TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)

      If life.Length > 0 Then
         Dim lifeArea As New Rectangle(rect.X + 4, rect.Bottom - 16, rect.Width - 8, 14)
         TextRenderer.DrawText(g, life, _dateFont, lifeArea, Color.FromArgb(90, 90, 90),
                               TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
      End If
   End Sub

   Private Shared Function LifeSpan(p As Person) As String
      Dim born As String = If(p.BirthDate.HasValue, p.BirthDate.Value.Year.ToString(), "")
      Dim died As String = If(p.DeathDate.HasValue, p.DeathDate.Value.Year.ToString(), "")
      If born.Length = 0 AndAlso died.Length = 0 Then Return ""
      If died.Length = 0 Then Return "b. " & born
      Return $"{born} – {died}"
   End Function

   ' ---------------------------------------------------------- interaction ---

   Private _hoverNode As Node
   Private Sub pnlChart_MouseMove(sender As Object, e As MouseEventArgs) Handles pnlChart.MouseMove
      Dim hit As Node = NodeAt(e.Location)
      If hit Is _hoverNode Then Return
      _hoverNode = hit
      If hit Is Nothing Then
         _tip.Hide(pnlChart)
      Else
         _tip.Show(DescribePerson(hit.Person), pnlChart, e.X + 16, e.Y + 16, 5000)
      End If
   End Sub

   Private Function NodeAt(clientPoint As Point) As Node
      Dim x As Integer = clientPoint.X - pnlChart.AutoScrollPosition.X
      Dim y As Integer = clientPoint.Y - pnlChart.AutoScrollPosition.Y
      For Each n As Node In _allNodes
         If New Rectangle(n.Cx - NodeWidth \ 2, n.Y, NodeWidth, NodeHeight).Contains(x, y) Then Return n
      Next
      Return Nothing
   End Function

   Private Shared Function DescribePerson(p As Person) As String
      Dim lines As New List(Of String) From {
         $"{p.FirstName} {p.LastName}".Trim(),
         "ID: " & p.ID
      }
      If p.Sex.HasValue AndAlso p.Sex.Value <> Person.SexType.Unknown Then lines.Add("Sex: " & p.Sex.Value.ToString())
      If p.BirthDate.HasValue Then lines.Add("Born: " & p.BirthDate.Value.ToString("yyyy-MM-dd"))
      If p.DeathDate.HasValue Then lines.Add("Died: " & p.DeathDate.Value.ToString("yyyy-MM-dd"))
      If Not String.IsNullOrWhiteSpace(p.Notes) Then
         lines.Add("")
         lines.Add(p.Notes.Trim())
      End If
      Return String.Join(Environment.NewLine, lines)
   End Function

   ' Combo entry that shows a readable label but hands back the Person.
   Private NotInheritable Class FocusItem
      Public Property Person As Person
      Public Overrides Function ToString() As String
         Dim name As String = $"{Person.FirstName} {Person.LastName}".Trim()
         If name.Length = 0 Then name = "#" & Person.ID
         Return $"{name}  (#{Person.ID})"
      End Function
   End Class

   ' Panel with double buffering so a large chart repaints without flicker.
   Friend Class ChartPanel
      Inherits Panel

      Public Sub New()
         DoubleBuffered = True
         ResizeRedraw = True
      End Sub
   End Class

End Class
