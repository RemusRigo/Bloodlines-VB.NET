Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports Bloodlines.Bloodlines

' Genealogy "hourglass" chart centred on one person, as an embeddable UserControl
' (as opposed to frmTree, which is the same chart hosted in its own Form):
'   - the focus person sits in the middle, descendants fan downward, ancestors fan upward
'   - drag with the mouse to pan the chart around
'   - mouse wheel, or the +/- keys, zoom in/out (centred on the cursor / viewport centre)
'   - double-click a person to rebuild the chart focused on them, re-centring the view
'   - the view re-centres on the focus person automatically whenever the control is resized
'     (e.g. the host form being maximised), so the chart never "drifts" out of view
Public Class frmUCTree
   ' Set by the caller (see frmBloodlines.tsBtnTreeNew_Click) before the control is
   ' shown/embedded. Hidden from the designer since it's only ever wired up in code.
   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Public Property Tree As FamilyTree

   ' --- layout constants (pixels, unscaled - i.e. measured at zoom = 1.0) ---
   Private Const NodeWidth As Integer = 150     ' width of one person's box
   Private Const NodeHeight As Integer = 52     ' height of one person's box
   Private Const HGap As Integer = 26           ' horizontal gap between sibling subtrees
   Private Const VGap As Integer = 46           ' vertical gap between generations
   Private Const Margin As Integer = 24         ' padding around the whole layout
   Private Const MaxDepth As Integer = 20       ' guard against pathological/cyclic data
   Private Const SpouseGap As Integer = 6       ' gap between a person and their spouse box(es)

   ' --- zoom/pan state ---
   ' The chart is drawn in its own unscaled "content" coordinate space (see LayoutNodes),
   ' then mapped onto the screen each paint via a translate-then-scale transform:
   '     screenPoint = contentPoint * _zoom + (_panX, _panY)
   ' _panX/_panY is therefore the screen position of content point (0, 0), and _zoom is
   ' how many screen pixels one content pixel currently occupies.
   Private Const MinZoom As Single = 0.25F      ' furthest you can zoom out (25%)
   Private Const MaxZoom As Single = 3.0F       ' furthest you can zoom in (300%)
   Private Const ZoomStep As Single = 0.15F     ' zoom change per wheel notch / key press
   Private _zoom As Single = 1.0F
   Private _panX As Single = 0
   Private _panY As Single = 0
   Private _dragging As Boolean                 ' True while the left mouse button is held for panning
   Private _dragLast As Point                    ' last mouse position seen during a drag, to compute deltas

   ' One box in the chart - a thin wrapper around a Person plus its computed layout position.
   Private Class Node
      Public Person As Person
      Public ReadOnly Kids As New List(Of Node)         ' drawn below, connected downward
      Public ReadOnly Parents As New List(Of Node)      ' drawn above, connected upward
      Public ReadOnly Spouses As New List(Of Person)    ' drawn beside, same generation
      Public Cx As Integer                              ' couple block centre X (content coords)
      Public Y As Integer                               ' box top Y (content coords)
      Public W As Integer                               ' subtree width, used only during layout
   End Class

   Private ReadOnly _byId As New Dictionary(Of Integer, Person)   ' fast Person lookup by ID
   Private ReadOnly _allNodes As New List(Of Node)                ' every node currently laid out/drawn
   Private _focus As Node                                          ' the node currently centred/highlighted
   Private _focusTopY As Integer                                   ' Y of the focus row, used while placing ancestors/descendants

   Private _nameFont As Font
   Private _dateFont As Font
   Private ReadOnly _tip As New ToolTip()

   ' Shared text layout: centred both ways, single line, ellipsis if the name/date
   ' string doesn't fit the box. Reused for every DrawString call to avoid
   ' allocating a new StringFormat per box per paint.
   Private ReadOnly _centerFormat As New StringFormat() With {
      .Alignment = StringAlignment.Center,
      .LineAlignment = StringAlignment.Center,
      .Trimming = StringTrimming.EllipsisCharacter,
      .FormatFlags = StringFormatFlags.NoWrap
   }

   ' ---------------------------------------------------------------- setup ---

   Private Sub frmUCTree_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      _nameFont = New Font(Font, FontStyle.Bold)
      _dateFont = New Font(Font.FontFamily, Math.Max(6.0F, Font.Size - 1.5F))

      If Tree Is Nothing OrElse Tree.People Is Nothing OrElse Tree.People.Count = 0 Then
         pnlChart.Invalidate()
         Return
      End If

      For Each p As Person In Tree.People
         _byId(p.ID) = p
      Next

      ' Default to person #1 (or just the first person if there's no #1) until the
      ' user double-clicks someone else to change focus.
      Dim def As Person = Tree.People.FirstOrDefault(Function(p) p.ID = 1)
      If def Is Nothing Then def = Tree.People.First()

      SetFocus(def)
      pnlChart.Focus() ' so the +/- zoom keys work immediately without an extra click
   End Sub

   ' GDI/GDI+ resources aren't garbage-collected promptly, so free them explicitly
   ' once the control's window handle goes away (control removed/host form closing).
   Private Sub frmUCTree_HandleDestroyed(sender As Object, e As EventArgs) Handles Me.HandleDestroyed
      If _nameFont IsNot Nothing Then _nameFont.Dispose()
      If _dateFont IsNot Nothing Then _dateFont.Dispose()
      _tip.Dispose()
      _centerFormat.Dispose()
   End Sub

   ' ---------------------------------------------------------------- model ---
   ' Rebuilds the node tree around a new focus person, re-lays it out, and
   ' re-centres the view on it (keeping the current zoom level).

   Private Sub SetFocus(p As Person)
      If p Is Nothing Then Return
      BuildNodes(p)
      LayoutNodes()
      CenterOnFocus()
      pnlChart.Invalidate()
   End Sub

   ' Builds the Node graph: descendants of p downward, ancestors of p upward,
   ' merged into one tree hanging off a single focus Node.
   Private Sub BuildNodes(focus As Person)
      _focus = Nothing
      If focus Is Nothing Then Return

      Dim down As Node = BuildDown(focus, 0, New HashSet(Of Integer))
      Dim up As Node = BuildUp(focus, 0, New HashSet(Of Integer))
      down.Parents.AddRange(up.Parents)
      _focus = down
   End Sub

   ' Recursively builds the descendant side (p and everyone below them), plus p's spouses.
   ' `path` tracks ancestors currently being recursed through, to guard against a bad/cyclic
   ' relationship (e.g. someone accidentally listed as their own descendant) infinite-looping.
   Private Function BuildDown(p As Person, depth As Integer, path As HashSet(Of Integer)) As Node
      Dim n As New Node With {.Person = p}

      For Each r As Relationship In p.Relationships.Where(Function(x) x.Type = Relationship.RelationType.Spouse)
         Dim spouse As Person = Nothing
         If _byId.TryGetValue(r.RelativeID, spouse) Then n.Spouses.Add(spouse)
      Next

      If depth >= MaxDepth OrElse Not path.Add(p.ID) Then Return n

      For Each child As Person In Tree.GetChildren(p.ID).
            OrderBy(Function(c) c.BirthDate).ThenBy(Function(c) c.ID)
         n.Kids.Add(BuildDown(child, depth + 1, path))
      Next

      path.Remove(p.ID)
      Return n
   End Function

   ' Recursively builds the ancestor side (p's parents, grandparents, ...).
   ' Same cycle-guard as BuildDown.
   Private Function BuildUp(p As Person, depth As Integer, path As HashSet(Of Integer)) As Node
      Dim n As New Node With {.Person = p}
      If depth >= MaxDepth OrElse Not path.Add(p.ID) Then Return n

      Dim added As New HashSet(Of Integer)
      For Each r As Relationship In p.Relationships.
            Where(Function(x) x.Type = Relationship.RelationType.Father OrElse x.Type = Relationship.RelationType.Mother).
            OrderBy(Function(x) x.Type)
         Dim parent As Person = Nothing
         If _byId.TryGetValue(r.RelativeID, parent) AndAlso added.Add(r.RelativeID) Then
            n.Parents.Add(BuildUp(parent, depth + 1, path))
         End If
      Next

      path.Remove(p.ID)
      Return n
   End Function

   ' --------------------------------------------------------------- layout ---
   ' Positions every node in unscaled "content" coordinates (Node.Cx/Node.Y).
   ' Zoom/pan are applied later, only at paint time - the layout itself never changes
   ' with zoom, which keeps all the box/line geometry math simple.

   Private Sub LayoutNodes()
      _allNodes.Clear()
      If _focus Is Nothing Then Return

      Dim kidsOf As Func(Of Node, List(Of Node)) = Function(n) n.Kids
      Dim parentsOf As Func(Of Node, List(Of Node)) = Function(n) n.Parents

      ' First pass: measure how wide each subtree needs to be, so siblings can be
      ' spaced without overlapping.
      Measure(_focus, kidsOf, New HashSet(Of Node))
      Measure(_focus, parentsOf, New HashSet(Of Node))

      ' The focus row's Y depends on how many ancestor generations sit above it.
      Dim maxAnc As Integer = MaxGen(_focus, parentsOf, 0, New HashSet(Of Node))
      _focusTopY = Margin + maxAnc * (NodeHeight + VGap)

      ' Second pass: assign actual X/Y to every node.
      PlaceDown(_focus, 0, 0)
      PlaceUp(_focus)

      ' Flatten the tree into _allNodes for drawing/hit-testing.
      _allNodes.Add(_focus)
      Dim seen As New HashSet(Of Node) From {_focus}
      For Each k As Node In _focus.Kids
         Collect(k, kidsOf, seen, _allNodes)
      Next
      For Each p As Node In _focus.Parents
         Collect(p, parentsOf, seen, _allNodes)
      Next

      ' Shift everything so the left-most box sits exactly Margin pixels from content X = 0.
      Dim minLeft As Integer = _allNodes.Min(Function(n) n.Cx - CoupleWidth(n) \ 2)
      Dim dx As Integer = Margin - minLeft
      For Each n As Node In _allNodes
         n.Cx += dx
      Next
   End Sub

   ' Width, in pixels, of a person's box plus any spouse boxes drawn beside it.
   Private Shared Function CoupleWidth(n As Node) As Integer
      Return NodeWidth + n.Spouses.Count * (NodeWidth + SpouseGap)
   End Function

   ' Centre X of the person's own box (n.Cx is the whole couple block's centre, which
   ' shifts right when spouse boxes are attached) - parent/child connector lines must
   ' always land here, not on the couple midpoint, so kids from a different partner
   ' than the one shown still hang from the right box.
   Private Shared Function PrimaryCx(n As Node) As Integer
      Return n.Cx - CoupleWidth(n) \ 2 + NodeWidth \ 2
   End Function

   ' Computes (and caches in node.W) how wide this node's subtree needs to be:
   ' either its own couple-width, or the sum of its children's subtree widths, whichever
   ' is larger. `seen` prevents re-measuring/looping on a node reachable two ways.
   Private Function Measure(node As Node, childrenOf As Func(Of Node, List(Of Node)), seen As HashSet(Of Node)) As Integer
      Dim ownWidth As Integer = CoupleWidth(node)
      Dim kids As List(Of Node) = childrenOf(node)
      If Not seen.Add(node) OrElse kids.Count = 0 Then
         node.W = ownWidth
         Return node.W
      End If

      Dim total As Integer = 0
      For i As Integer = 0 To kids.Count - 1
         total += Measure(kids(i), childrenOf, seen)
         If i < kids.Count - 1 Then total += HGap
      Next

      node.W = Math.Max(ownWidth, total)
      Return node.W
   End Function

   ' Deepest generation reached below `node` (0 = node itself), used to size the
   ' ancestor side of the layout before placement.
   Private Function MaxGen(node As Node, childrenOf As Func(Of Node, List(Of Node)), gen As Integer, seen As HashSet(Of Node)) As Integer
      If Not seen.Add(node) Then Return gen
      Dim m As Integer = gen
      For Each k As Node In childrenOf(node)
         m = Math.Max(m, MaxGen(k, childrenOf, gen + 1, seen))
      Next
      Return m
   End Function

   ' Flattens the subtree rooted at `node` into `into`, depth-first.
   Private Sub Collect(node As Node, childrenOf As Func(Of Node, List(Of Node)), seen As HashSet(Of Node), into As List(Of Node))
      If Not seen.Add(node) Then Return
      into.Add(node)
      For Each k As Node In childrenOf(node)
         Collect(k, childrenOf, seen, into)
      Next
   End Sub

   ' Places descendants: node itself at (cx, generation row), then its kids spread
   ' out beneath it, each centred over its own subtree width.
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

   ' Places the focus person at content X = 0, then recursively places ancestors above it.
   Private Sub PlaceUp(focus As Node)
      focus.Cx = 0
      focus.Y = _focusTopY
      PlaceParents(focus, 0, 1)
   End Sub

   ' Mirrors PlaceDown, but going upward (ancestors), one generation at a time.
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

   ' ------------------------------------------------------------ pan / zoom ---

   ' Converts a point in screen/client pixels (e.g. a mouse position) into content
   ' coordinates, i.e. undoes the pan+zoom transform applied at paint time.
   Private Function ScreenToContent(pt As Point) As PointF
      Return New PointF((pt.X - _panX) / _zoom, (pt.Y - _panY) / _zoom)
   End Function

   ' Changes the zoom level by `delta`, keeping the content point currently under
   ' `screenPt` fixed on screen (so zooming with the mouse wheel zooms "into" the
   ' cursor instead of the top-left corner).
   Private Sub ZoomAt(screenPt As Point, delta As Single)
      Dim newZoom As Single = Math.Max(MinZoom, Math.Min(MaxZoom, _zoom + delta))
      If newZoom = _zoom Then Return ' already at the min/max clamp

      Dim worldPt As PointF = ScreenToContent(screenPt)
      _zoom = newZoom
      _panX = screenPt.X - worldPt.X * _zoom
      _panY = screenPt.Y - worldPt.Y * _zoom
      pnlChart.Invalidate()
   End Sub

   ' Recomputes _panX/_panY so the focus person's box sits in the middle of the
   ' visible chart area, at the current zoom level. Called after every SetFocus
   ' and whenever pnlChart is resized (see pnlChart_Resize), so the chart never
   ' drifts off-screen when the host window is resized/maximised.
   Private Sub CenterOnFocus()
      If _focus Is Nothing Then Return
      Dim cx As Integer = PrimaryCx(_focus)
      Dim cy As Integer = _focus.Y + NodeHeight \ 2
      _panX = pnlChart.ClientSize.Width / 2.0F - cx * _zoom
      _panY = pnlChart.ClientSize.Height / 2.0F - cy * _zoom
   End Sub

   ' Re-centres the chart on the focus person whenever the panel's size changes -
   ' covers the host form being resized, maximised, or restored.
   Private Sub pnlChart_Resize(sender As Object, e As EventArgs) Handles pnlChart.Resize
      CenterOnFocus()
      pnlChart.Invalidate()
   End Sub

   ' Mouse wheel: one notch = one ZoomStep, zooming toward whatever the cursor is over.
   Private Sub pnlChart_MouseWheel(sender As Object, e As MouseEventArgs) Handles pnlChart.MouseWheel
      Dim steps As Single = e.Delta / 120.0F ' WinForms reports 120 "detents" per notch
      ZoomAt(e.Location, steps * ZoomStep)
   End Sub

   ' Keyboard zoom: + / - (either the top-row or numpad variants), centred on the
   ' middle of the visible chart area since there's no cursor position to anchor to.
   Private Sub pnlChart_KeyDown(sender As Object, e As KeyEventArgs) Handles pnlChart.KeyDown
      Dim center As New Point(pnlChart.ClientSize.Width \ 2, pnlChart.ClientSize.Height \ 2)
      Select Case e.KeyCode
         Case Keys.Add, Keys.Oemplus
            ZoomAt(center, ZoomStep)
            e.Handled = True
         Case Keys.Subtract, Keys.OemMinus
            ZoomAt(center, -ZoomStep)
            e.Handled = True
      End Select
   End Sub

   ' Left mouse button down starts a pan drag; also grabs keyboard focus for pnlChart
   ' so +/- zoom keys work right after clicking into the chart.
   Private Sub pnlChart_MouseDown(sender As Object, e As MouseEventArgs) Handles pnlChart.MouseDown
      pnlChart.Focus()
      If e.Button <> MouseButtons.Left Then Return
      _dragging = True
      _dragLast = e.Location
      pnlChart.Cursor = Cursors.SizeAll
   End Sub

   Private Sub pnlChart_MouseUp(sender As Object, e As MouseEventArgs) Handles pnlChart.MouseUp
      _dragging = False
      pnlChart.Cursor = Cursors.Default
   End Sub

   ' Double-clicking a person's box rebuilds/re-centres the chart around them.
   Private Sub pnlChart_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles pnlChart.MouseDoubleClick
      Dim hit As Person = PersonAt(e.Location)
      If hit IsNot Nothing Then SetFocus(hit)
   End Sub

   ' --------------------------------------------------------------- paint ---

   Private Sub pnlChart_Paint(sender As Object, e As PaintEventArgs) Handles pnlChart.Paint
      Dim g As Graphics = e.Graphics
      g.SmoothingMode = SmoothingMode.AntiAlias
      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit

      ' Everything below is drawn in unscaled content coordinates; this transform maps
      ' it onto the screen. Order matters: translate first, then scale, so _panX/_panY
      ' stay in screen pixels regardless of the current zoom level.
      g.TranslateTransform(_panX, _panY)
      g.ScaleTransform(_zoom, _zoom)

      If _focus Is Nothing Then
         g.ResetTransform() ' draw this message in plain screen coordinates, unscaled
         g.DrawString("Load a family tree, then pick a focus person.", Font, Brushes.Gray, Margin, Margin)
         Return
      End If

      ' Connector lines first, so they sit behind the boxes.
      Using pen As New Pen(Color.FromArgb(150, 150, 150), 0)
         For Each n As Node In _allNodes
            DrawDownConnector(g, pen, n)
            DrawUpConnector(g, pen, n)
         Next
      End Using

      For Each n As Node In _allNodes
         DrawCouple(g, n, n Is _focus)
      Next
   End Sub

   ' Draws the elbow connector from `n` down to each of its children, via a shared
   ' horizontal "bus" line halfway down the generation gap.
   Private Sub DrawDownConnector(g As Graphics, pen As Pen, n As Node)
      If n.Kids.Count = 0 Then Return
      Dim myCx As Integer = PrimaryCx(n)
      Dim fromY As Integer = n.Y + NodeHeight
      Dim busY As Integer = fromY + VGap \ 2
      g.DrawLine(pen, myCx, fromY, myCx, busY)

      ' The bus line must span myCx too, not just the kids' own x-positions: an only
      ' child who has a spouse gets their own box pushed off-centre within their
      ' couple block (their spouse's box sits beside it), so xs.Min()/Max() alone
      ' can land entirely to one side of myCx and leave a gap back to the parent.
      Dim xs = n.Kids.Select(Function(k) PrimaryCx(k)).ToList()
      xs.Add(myCx)
      g.DrawLine(pen, xs.Min(), busY, xs.Max(), busY)
      For Each k As Node In n.Kids
         Dim kCx As Integer = PrimaryCx(k)
         g.DrawLine(pen, kCx, busY, kCx, k.Y)
      Next
   End Sub

   ' Mirrors DrawDownConnector, but upward to `n`'s parents.
   Private Sub DrawUpConnector(g As Graphics, pen As Pen, n As Node)
      If n.Parents.Count = 0 Then Return
      Dim myCx As Integer = PrimaryCx(n)
      Dim fromY As Integer = n.Y
      Dim busY As Integer = fromY - VGap \ 2
      g.DrawLine(pen, myCx, fromY, myCx, busY)

      ' Same fix as DrawDownConnector: include myCx so a single parent whose own
      ' box is off-centre (because they have a spouse) still connects back to n.
      Dim xs = n.Parents.Select(Function(p) PrimaryCx(p)).ToList()
      xs.Add(myCx)
      g.DrawLine(pen, xs.Min(), busY, xs.Max(), busY)
      For Each p As Node In n.Parents
         Dim pCx As Integer = PrimaryCx(p)
         g.DrawLine(pen, pCx, busY, pCx, p.Y + NodeHeight)
      Next
   End Sub

   ' Draws the person's own box, then any spouse box(es) beside it, joined by a
   ' short horizontal "marriage" line.
   Private Sub DrawCouple(g As Graphics, n As Node, isFocus As Boolean)
      Dim left As Integer = n.Cx - CoupleWidth(n) \ 2
      Dim rect As New Rectangle(left, n.Y, NodeWidth, NodeHeight)
      DrawBox(g, rect, n.Person, isFocus)

      Dim prevRect As Rectangle = rect
      For Each sp As Person In n.Spouses
         Dim spRect As New Rectangle(prevRect.Right + SpouseGap, n.Y, NodeWidth, NodeHeight)
         DrawBox(g, spRect, sp, False)

         Using pen As New Pen(Color.FromArgb(150, 150, 150), 0)
            Dim midY As Integer = n.Y + NodeHeight \ 2
            g.DrawLine(pen, prevRect.Right, midY, spRect.Left, midY)
         End Using
         prevRect = spRect
      Next
   End Sub

   ' Draws one person's box: fill colour by sex, border (bold+blue if this is the
   ' focus person), name, and life span (birth-death years).
   Private Sub DrawBox(g As Graphics, rect As Rectangle, person As Person, isFocus As Boolean)
      Dim fill As Color
      Select Case If(person.Sex.HasValue, person.Sex.Value, Person.SexType.Unknown)
         Case Person.SexType.Male : fill = Color.FromArgb(219, 234, 254)
         Case Person.SexType.Female : fill = Color.FromArgb(252, 228, 236)
         Case Else : fill = Color.FromArgb(238, 238, 238)
      End Select

      Using b As New SolidBrush(fill)
         g.FillRectangle(b, rect)
      End Using

      If isFocus Then
         ' Pen width is divided by _zoom so the focus outline stays a constant
         ' ~2 screen pixels thick regardless of zoom level (the Graphics transform
         ' otherwise scales pen widths along with everything else).
         Using pen As New Pen(Color.FromArgb(33, 102, 172), 2 / _zoom)
            g.DrawRectangle(pen, rect)
         End Using
      Else
         ' Width 0 = a GDI+ "cosmetic" pen: always exactly 1 device pixel wide,
         ' unaffected by the world transform, so thin borders don't vanish when
         ' zoomed out or turn blurry when zoomed in.
         Using pen As New Pen(Color.FromArgb(120, 120, 120), 0)
            g.DrawRectangle(pen, rect)
         End Using
      End If

      Dim name As String = $"{person.FirstName} {person.LastName}".Trim()
      If name.Length = 0 Then name = "#" & person.ID

      ' Text is drawn with Graphics.DrawString (true GDI+), not TextRenderer.DrawText
      ' (which goes through native GDI and only honours a translation on the Graphics
      ' object, not the ScaleTransform used for zoom - using it here would leave the
      ' text fixed in place while the boxes pan/zoom underneath it).
      Dim life As String = LifeSpan(person)
      Dim nameArea As New RectangleF(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8 - If(life.Length > 0, 14, 0))
      g.DrawString(name, _nameFont, Brushes.Black, nameArea, _centerFormat)

      If life.Length > 0 Then
         Dim lifeArea As New RectangleF(rect.X + 4, rect.Bottom - 16, rect.Width - 8, 14)
         Using lifeBrush As New SolidBrush(Color.FromArgb(90, 90, 90))
            g.DrawString(life, _dateFont, lifeBrush, lifeArea, _centerFormat)
         End Using
      End If
   End Sub

   ' "b. 1900" if only a birth year is known, "1900 - 1980" if both are, "" if neither.
   Private Shared Function LifeSpan(p As Person) As String
      Dim born As String = If(p.BirthDate.HasValue, p.BirthDate.Value.Year.ToString(), "")
      Dim died As String = If(p.DeathDate.HasValue, p.DeathDate.Value.Year.ToString(), "")
      If born.Length = 0 AndAlso died.Length = 0 Then Return ""
      If died.Length = 0 Then Return "b. " & born
      Return $"{born} – {died}"
   End Function

   ' ---------------------------------------------------------- interaction ---

   Private _hoverPerson As Person
   Private Sub pnlChart_MouseMove(sender As Object, e As MouseEventArgs) Handles pnlChart.MouseMove
      If _dragging Then
         ' Panning: shift the pan offset by however far the mouse moved since the
         ' last MouseMove, in screen pixels (pan is always in screen space).
         Dim dx As Integer = e.X - _dragLast.X
         Dim dy As Integer = e.Y - _dragLast.Y
         _panX += dx
         _panY += dy
         _dragLast = e.Location
         pnlChart.Invalidate()
         Return
      End If

      ' Not dragging: show/update a tooltip for whichever person's box is under the cursor.
      Dim hit As Person = PersonAt(e.Location)
      If hit Is _hoverPerson Then Return
      _hoverPerson = hit
      If hit Is Nothing Then
         _tip.Hide(pnlChart)
      Else
         _tip.Show(DescribePerson(hit), pnlChart, e.X + 16, e.Y + 16, 5000)
      End If
   End Sub

   ' Hit-tests a screen/client point against every drawn box, returning the Person
   ' whose box contains it (or Nothing). Converts through ScreenToContent first since
   ' the boxes' own coordinates are unscaled content coordinates.
   Private Function PersonAt(screenPoint As Point) As Person
      Dim content As PointF = ScreenToContent(screenPoint)
      Dim x As Integer = CInt(content.X)
      Dim y As Integer = CInt(content.Y)
      For Each n As Node In _allNodes
         Dim left As Integer = n.Cx - CoupleWidth(n) \ 2
         Dim rect As New Rectangle(left, n.Y, NodeWidth, NodeHeight)
         If rect.Contains(x, y) Then Return n.Person

         For Each sp As Person In n.Spouses
            rect = New Rectangle(rect.Right + SpouseGap, n.Y, NodeWidth, NodeHeight)
            If rect.Contains(x, y) Then Return sp
         Next
      Next
      Return Nothing
   End Function

   ' Builds the multi-line tooltip text shown on hover.
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

   ' Panel with double buffering so a large chart repaints without flicker while
   ' panning/zooming/resizing.
   Friend Class ChartPanel
      Inherits Panel

      Public Sub New()
         DoubleBuffered = True
         ResizeRedraw = True ' repaint immediately on resize, instead of showing stale content
         SetStyle(ControlStyles.Selectable, True) ' allow this panel to receive keyboard focus
      End Sub
   End Class

End Class
