'--------------------------------------------------------------------------------------------------
' Bloodlines: frmBloodlines.vb: Main form
'    © 2026 Remus Rigo
'       v1.0.20260925
'--------------------------------------------------------------------------------------------------

Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.IO
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
   Private Const NodeWidth As Integer = 210     ' width of one person's card
   Private Const NodeHeight As Integer = 84     ' height of one person's card
   Private Const HGap As Integer = 26           ' horizontal gap between sibling subtrees
   Private Const VGap As Integer = 46           ' vertical gap between generations
   Private Const TreeMargin As Integer = 24     ' padding around the whole layout
   Private Const MaxDepth As Integer = 20       ' guard against pathological/cyclic data
   Private Const SpouseGap As Integer = 6       ' gap between a person and their spouse box(es)
   Private Const CardPad As Integer = 6         ' padding inside a person card
   Private Const PhotoWidth As Integer = 62     ' width of the photo on a card
   Private Const BadgeSize As Integer = 18      ' "open linked tree" (+) button in a card's top-right corner
   Private Const BadgeInset As Integer = 4      ' gap between that button and the card's edges

   ' Raised when the user clicks a person's "+" tree-link button (Person.TreeLink);
   ' the host form loads that tree and shows it.
   Public Event OpenTreeRequested(treeName As String)

   ' When this chart was loaded (Environment.TickCount64). "+" clicks within one double-click
   ' interval of that are ignored: if the user double-clicked a "+" out of habit, the first
   ' click already switched trees, and the second must not hit a "+" on the new chart and bounce back.
   Private _loadedAt As Long
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
      Public L As Integer                               ' subtree extent left of PrimaryCx, used only during layout
      Public R As Integer                               ' subtree extent right of PrimaryCx, used only during layout
   End Class

   Private ReadOnly _byId As New Dictionary(Of Integer, Person)   ' fast Person lookup by ID
   Private ReadOnly _allNodes As New List(Of Node)                ' every node currently laid out/drawn
   Private _focus As Node                                          ' the node currently centred/highlighted
   Private _focusTopY As Integer                                   ' Y of the focus row, used while placing ancestors/descendants

   Private _nameFont As Font
   Private _dateFont As Font
   Private ReadOnly _tip As New ToolTip()
   Private ReadOnly _photos As New Dictionary(Of Integer, Image)   ' person ID -> photo (Nothing = none)
   Private ReadOnly _menuIcons As New Dictionary(Of String, Image) From {
      {"Mother", MakeMenuIcon(Color.FromArgb(219, 112, 147), "M")},
      {"Father", MakeMenuIcon(Color.FromArgb(70, 130, 220), "F")},
      {"Child", MakeMenuIcon(Color.FromArgb(150, 150, 150), "C")},
      {"Sibling", MakeMenuIcon(Color.FromArgb(150, 150, 150), "S")},
      {"Spouse", MakeMenuIcon(Color.FromArgb(180, 140, 200), "♥")}
   }

   ' Shared text layout: centred both ways, single line, ellipsis if the name/date
   ' string doesn't fit the box. Reused for every DrawString call to avoid
   ' allocating a new StringFormat per box per paint.
   ' Card text: left-aligned, vertically centred in its line, single line with ellipsis.
   Private ReadOnly _leftFormat As New StringFormat() With {
      .Alignment = StringAlignment.Near,
      .LineAlignment = StringAlignment.Center,
      .Trimming = StringTrimming.EllipsisCharacter,
      .FormatFlags = StringFormatFlags.NoWrap
   }

   Private ReadOnly _centerFormat As New StringFormat() With {
      .Alignment = StringAlignment.Center,
      .LineAlignment = StringAlignment.Center,
      .Trimming = StringTrimming.EllipsisCharacter,
      .FormatFlags = StringFormatFlags.NoWrap
   }

   ' ---------------------------------------------------------------- setup ---

   Private Sub frmUCTree_Load(sender As Object, e As EventArgs) Handles MyBase.Load
      _loadedAt = Environment.TickCount64
      _nameFont = New Font(Font.FontFamily, Font.Size + 2.0F)
      _dateFont = New Font(Font.FontFamily, Font.Size + 0.5F)

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
   ' once the control is disposed. (Not on HandleDestroyed: a handle can be recreated and
   ' the control keeps painting with the same fonts/formats afterwards.)
   Private Sub frmUCTree_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
      If pnlChart.ContextMenuStrip IsNot Nothing Then pnlChart.ContextMenuStrip.Dispose()
      If _nameFont IsNot Nothing Then _nameFont.Dispose()
      If _dateFont IsNot Nothing Then _dateFont.Dispose()
      _tip.Dispose()
      _centerFormat.Dispose()
      _leftFormat.Dispose()
      For Each img As Image In _photos.Values
         If img IsNot Nothing Then img.Dispose()
      Next
      _photos.Clear()
      For Each img As Image In _menuIcons.Values
         img.Dispose()
      Next
   End Sub

   '-----------------------------------------------------------------------------------------------
   ' Rebuilds the node tree around a new focus person, re-lays it out, and re-centres the view on it (keeping the current zoom level).
   Private Sub SetFocus(p As Person)
      If p Is Nothing Then Return
      BuildNodes(p)
      LayoutNodes()
      CenterOnFocus()
      pnlChart.Invalidate()
   End Sub

   '-----------------------------------------------------------------------------------------------
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
      _focusTopY = TreeMargin + maxAnc * (NodeHeight + VGap)

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

      ' Shift everything so the left-most box sits exactly TreeMargin pixels from content X = 0.
      Dim minLeft As Integer = _allNodes.Min(Function(n) n.Cx - CoupleWidth(n) \ 2)
      Dim dx As Integer = TreeMargin - minLeft
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

   ' Inverse of PrimaryCx: the couple block centre that puts n's own box centre at `primary`.
   Private Shared Function CxFromPrimary(n As Node, primary As Integer) As Integer
      Return primary - NodeWidth \ 2 + CoupleWidth(n) \ 2
   End Function

   ' Computes (and caches in node.L/node.R) how far this node's subtree extends to the left
   ' and right of PrimaryCx(node). The node's own box is centred on PrimaryCx with any spouse
   ' boxes hanging off to the right, and its children are centred under PrimaryCx too, so the
   ' extents are asymmetric whenever there's a spouse - a single width centred on the couple
   ' block (node.Cx) would under-reserve on the left and let the kids overlap the neighbours.
   ' `seen` prevents re-measuring/looping on a node reachable two ways.
   Private Function Measure(node As Node, childrenOf As Func(Of Node, List(Of Node)), seen As HashSet(Of Node)) As Integer
      node.L = NodeWidth \ 2
      node.R = CoupleWidth(node) - node.L
      Dim kids As List(Of Node) = childrenOf(node)
      If Not seen.Add(node) OrElse kids.Count = 0 Then Return node.L + node.R

      Dim total As Integer = 0
      For i As Integer = 0 To kids.Count - 1
         total += Measure(kids(i), childrenOf, seen)
         If i < kids.Count - 1 Then total += HGap
      Next

      node.L = Math.Max(node.L, total \ 2)
      node.R = Math.Max(node.R, total - total \ 2)
      Return node.L + node.R
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
   ' out beneath it, each centred over its own subtree width. The kids are centred on
   ' PrimaryCx(node) - node's own box - not on cx/node.Cx, which is the centre of the
   ' whole couple block once a spouse box is attached beside it. DrawDownConnector
   ' converges on PrimaryCx(node) too, so centring the kids on node.Cx instead left the
   ' whole child generation (and everything below it) shifted off to one side whenever
   ' node had a spouse - most visible, but not limited to, the ancestor side (see
   ' PlaceParents) with a lopsided family (e.g. one parent's own ancestors are known and
   ' take up much more width than the other's).
   ' `primary` is where node's own box centre goes (PrimaryCx), not the couple block centre;
   ' each kid is positioned by its own L/R extents around its PrimaryCx (see Measure).
   Private Sub PlaceDown(node As Node, primary As Integer, gen As Integer)
      node.Cx = CxFromPrimary(node, primary)
      node.Y = _focusTopY + gen * (NodeHeight + VGap)
      If node.Kids.Count = 0 Then Return

      Dim total As Integer = node.Kids.Sum(Function(k) k.L + k.R) + HGap * (node.Kids.Count - 1)
      Dim start As Integer = primary - total \ 2
      For Each k As Node In node.Kids
         PlaceDown(k, start + k.L, gen + 1)
         start += k.L + k.R + HGap
      Next
   End Sub

   ' The focus person was already placed by PlaceDown (own box centred at content X = 0);
   ' this just recursively places the ancestors above it.
   Private Sub PlaceUp(focus As Node)
      PlaceParents(focus, 1)
   End Sub

   ' Mirrors PlaceDown, but going upward (ancestors), one generation at a time. See the
   ' comment on PlaceDown: parents are centred on PrimaryCx(node), not node.Cx.
   Private Sub PlaceParents(node As Node, gen As Integer)
      If node.Parents.Count = 0 Then Return

      Dim primary As Integer = PrimaryCx(node)
      Dim total As Integer = node.Parents.Sum(Function(p) p.L + p.R) + HGap * (node.Parents.Count - 1)
      Dim start As Integer = primary - total \ 2
      For Each p As Node In node.Parents
         p.Cx = CxFromPrimary(p, start + p.L)
         p.Y = _focusTopY - gen * (NodeHeight + VGap)
         start += p.L + p.R + HGap
         PlaceParents(p, gen + 1)
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
   ' so +/- zoom keys work right after clicking into the chart. Right button instead
   ' primes pnlChart.ContextMenuStrip with a menu built for whoever's under the cursor,
   ' then lets Windows' own native WM_CONTEXTMENU handling show it. Calling
   ' ToolStripDropDown.Show(control, point) manually here instead reliably threw
   ' ObjectDisposedException from inside WinForms' own auto-close handling as soon as an
   ' item was clicked (reproducible regardless of whether the strip was disposed
   ' ourselves) - assigning ContextMenuStrip and letting Windows show it natively avoids
   ' that code path entirely.
   Private Sub pnlChart_MouseDown(sender As Object, e As MouseEventArgs) Handles pnlChart.MouseDown
      pnlChart.Focus()
      If e.Button = MouseButtons.Right Then
         Dim hit As Person = PersonAt(e.Location)
         ' The previous menu closed long ago, so it's safe to dispose here (unlike from its own
         ' click handler) - otherwise every right-click leaks a window handle.
         Dim oldMenu As ContextMenuStrip = pnlChart.ContextMenuStrip
         pnlChart.ContextMenuStrip = If(hit IsNot Nothing, BuildPersonMenu(hit), Nothing)
         If oldMenu IsNot Nothing Then oldMenu.Dispose()
         Return
      End If
      If e.Button <> MouseButtons.Left Then Return

      ' A click on a person's "+" tree-link button opens that tree instead of starting a pan.
      Dim linked As Person = LinkBadgeAt(e.Location)
      If linked IsNot Nothing Then
         If Environment.TickCount64 - _loadedAt >= SystemInformation.DoubleClickTime Then
            RaiseEvent OpenTreeRequested(linked.TreeLink.Trim())
         End If
         Return
      End If

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
         g.DrawString("Load a family tree, then pick a focus person.", Font, Brushes.Gray, TreeMargin, TreeMargin)
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

   ' A person "card": photo on the left; first name, last name and (born - died) on the right.
   ' The focus person gets a heavier blue outline.
   Private Sub DrawBox(g As Graphics, rect As Rectangle, person As Person, isFocus As Boolean)
      ' background tint by sex (blue = male, pink = female, grey = not specified), fading lighter toward the top
      Dim fill As Color
      Select Case person.Sex
         Case Person.SexType.Male : fill = Color.FromArgb(219, 234, 254)
         Case Person.SexType.Female : fill = Color.FromArgb(252, 228, 236)
         Case Else : fill = Color.FromArgb(238, 238, 238)
      End Select
      Dim fillTop As Color = Color.FromArgb((fill.R + 255) \ 2, (fill.G + 255) \ 2, (fill.B + 255) \ 2)
      Using b As New LinearGradientBrush(rect, fillTop, fill, 90.0F)
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

      ' photo (scales with the chart, so its border pen does too)
      Dim photoRect As New Rectangle(rect.X + CardPad, rect.Y + CardPad, PhotoWidth, rect.Height - 2 * CardPad)
      DrawPhoto(g, photoRect, GetPhoto(person))
      Using pen As New Pen(Color.FromArgb(86, 180, 239), 2)
         g.DrawRectangle(pen, photoRect)
      End Using

      ' text: first name / last name / (born - died)
      ' Drawn with Graphics.DrawString (true GDI+), not TextRenderer.DrawText, so the text follows
      ' the zoom transform along with the boxes (TextRenderer only honours a translation).
      Dim lines As New List(Of (Text As String, Font As Font, Brush As Brush))
      If Not String.IsNullOrWhiteSpace(person.FirstName) Then lines.Add((person.FirstName.Trim(), _nameFont, Brushes.Black))
      If Not String.IsNullOrWhiteSpace(person.LastName) Then lines.Add((person.LastName.Trim(), _nameFont, Brushes.Black))
      If lines.Count = 0 Then lines.Add(("#" & person.ID, _nameFont, Brushes.Black))
      Dim life As String = person.LifeSpan
      If life.Length > 0 Then lines.Add((life, _dateFont, Brushes.DimGray))

      Dim textX As Integer = photoRect.Right + CardPad + 2
      Dim textW As Integer = rect.Right - CardPad - textX
      If HasTreeLink(person) Then textW -= BadgeSize + BadgeInset   ' keep long names clear of the "+" button
      Dim lineH As Integer = _nameFont.Height
      Dim y As Integer = rect.Y + (rect.Height - lineH * lines.Count) \ 2
      For Each ln In lines
         g.DrawString(ln.Text, ln.Font, ln.Brush, New RectangleF(textX, y, textW, lineH), _leftFormat)
         y += lineH
      Next

      If HasTreeLink(person) Then DrawLinkBadge(g, BadgeRect(rect))
   End Sub

   ' A filled blue circle with a white "+", marking a person linked to another tree.
   Private Shared Sub DrawLinkBadge(g As Graphics, r As Rectangle)
      Using b As New SolidBrush(Color.FromArgb(33, 102, 172))
         g.FillEllipse(b, r)
      End Using
      Dim cx As Single = r.X + r.Width / 2.0F, cy As Single = r.Y + r.Height / 2.0F, arm As Single = r.Width * 0.28F
      Using pen As New Pen(Color.White, 2)
         g.DrawLine(pen, cx - arm, cy, cx + arm, cy)
         g.DrawLine(pen, cx, cy - arm, cx, cy + arm)
      End Using
   End Sub

   ' Fills r with the photo (cropped to fit, biased toward the top so faces stay in), or a placeholder.
   Private Shared Sub DrawPhoto(g As Graphics, r As Rectangle, img As Image)
      If img Is Nothing Then
         Using b As New SolidBrush(Color.FromArgb(214, 218, 224))
            g.FillRectangle(b, r)
         End Using
         Dim oldClip As Region = g.Clip
         g.SetClip(r)
         Using b As New SolidBrush(Color.FromArgb(160, 168, 178))
            Dim head As Integer = r.Width \ 2
            g.FillEllipse(b, r.X + (r.Width - head) \ 2, r.Y + r.Height \ 5, head, head)
            g.FillEllipse(b, r.X + r.Width \ 8, r.Y + r.Height \ 5 + head + 4, r.Width * 3 \ 4, r.Height)
         End Using
         g.Clip = oldClip
         Return
      End If

      Dim scale As Double = Math.Max(r.Width / img.Width, r.Height / img.Height)
      Dim sw As Integer = Math.Min(img.Width, CInt(r.Width / scale))
      Dim sh As Integer = Math.Min(img.Height, CInt(r.Height / scale))
      Dim src As New Rectangle((img.Width - sw) \ 2, (img.Height - sh) \ 4, sw, sh)
      Dim oldMode As InterpolationMode = g.InterpolationMode
      g.InterpolationMode = InterpolationMode.HighQualityBilinear
      g.DrawImage(img, r, src, GraphicsUnit.Pixel)
      g.InterpolationMode = oldMode
   End Sub

   ' Loads (once) the person's photo from <tree folder>\<tree name>\<ID>.png/jpg; Nothing if absent or unreadable.
   Private Function GetPhoto(p As Person) As Image
      Dim img As Image = Nothing
      If _photos.TryGetValue(p.ID, img) Then Return img

      Dim file As String = Tree.FindPhoto(p.ID)
      If file IsNot Nothing Then
         Try
            Using fs As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)
               Using src As Image = Image.FromStream(fs)
                  ' keep a downscaled copy: releases the file lock and makes repaints cheaper
                  Dim h As Integer = Math.Min(src.Height, 240)
                  Dim w As Integer = Math.Max(1, CInt(src.Width * (h / src.Height)))
                  Dim bmp As New Bitmap(w, h)
                  Using bg As Graphics = Graphics.FromImage(bmp)
                     bg.InterpolationMode = InterpolationMode.HighQualityBicubic
                     bg.DrawImage(src, 0, 0, w, h)
                  End Using
                  img = bmp
               End Using
            End Using
         Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is OutOfMemoryException
            img = Nothing
         End Try
      End If

      _photos(p.ID) = img
      Return img
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

      ' Not dragging: hand cursor over a "+" tree-link button, and show/update a tooltip
      ' for whichever person's box is under the cursor.
      pnlChart.Cursor = If(LinkBadgeAt(e.Location) IsNot Nothing, Cursors.Hand, Cursors.Default)
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

   Private Shared Function HasTreeLink(p As Person) As Boolean
      Return Not String.IsNullOrWhiteSpace(p.TreeLink)
   End Function

   ' The "+" button's rectangle on a card, in content coordinates.
   Private Shared Function BadgeRect(card As Rectangle) As Rectangle
      Return New Rectangle(card.Right - BadgeInset - BadgeSize, card.Y + BadgeInset, BadgeSize, BadgeSize)
   End Function

   ' Hit-tests a screen/client point against every "+" tree-link button (own boxes and
   ' spouse boxes), returning the Person it belongs to, or Nothing. Same box geometry as PersonAt.
   Private Function LinkBadgeAt(screenPoint As Point) As Person
      Dim content As PointF = ScreenToContent(screenPoint)
      Dim pt As New Point(CInt(content.X), CInt(content.Y))
      For Each n As Node In _allNodes
         Dim rect As New Rectangle(n.Cx - CoupleWidth(n) \ 2, n.Y, NodeWidth, NodeHeight)
         If HasTreeLink(n.Person) AndAlso BadgeRect(rect).Contains(pt) Then Return n.Person

         For Each sp As Person In n.Spouses
            rect = New Rectangle(rect.Right + SpouseGap, n.Y, NodeWidth, NodeHeight)
            If HasTreeLink(sp) AndAlso BadgeRect(rect).Contains(pt) Then Return sp
         Next
      Next
      Return Nothing
   End Function

   ' -------------------------------------------------------- context menu ---

   ' Builds the right-click menu for one person. Not disposed by us: a short-lived,
   ' per-right-click ContextMenuStrip is cheap enough to just leave for the GC/finalizer,
   ' and there's no safe point in the click-handling flow to dispose it from ourselves
   ' (see the note on pnlChart_MouseDown above).
   Private Function BuildPersonMenu(p As Person) As ContextMenuStrip
      Dim hasFather As Boolean = p.Relationships.Any(Function(r) r.Type = Relationship.RelationType.Father)
      Dim hasMother As Boolean = p.Relationships.Any(Function(r) r.Type = Relationship.RelationType.Mother)

      Dim menu As New ContextMenuStrip()
      menu.Items.Add("Add Mother", _menuIcons("Mother"), Sub() RunDeferred(Sub() AddMother(p))).Enabled = Not hasMother
      menu.Items.Add("Add Father", _menuIcons("Father"), Sub() RunDeferred(Sub() AddFather(p))).Enabled = Not hasFather
      menu.Items.Add("Add Child", _menuIcons("Child"), Sub() RunDeferred(Sub() AddChild(p)))
      menu.Items.Add("Add Sibling", _menuIcons("Sibling"), Sub() RunDeferred(Sub() AddSibling(p)))
      menu.Items.Add("Add Spouse", _menuIcons("Spouse"), Sub() RunDeferred(Sub() AddSpouse(p)))
      Return menu
   End Function

   ' Runs `action` on the next message-loop tick instead of directly inside a menu item's
   ' Click handler. AddRelative (used by every action here) opens a modal frmPerson
   ' dialog; deferring keeps that out of the ContextMenuStrip's own click/close handling.
   Private Sub RunDeferred(action As Action)
      pnlChart.BeginInvoke(action)
   End Sub

   ' A small flat circular badge (16x16) for a context-menu item: a solid colour fill
   ' with a single glyph in the middle. Built once and cached in _menuIcons.
   Private Shared Function MakeMenuIcon(fill As Color, glyph As String) As Bitmap
      Dim bmp As New Bitmap(16, 16)
      Using g As Graphics = Graphics.FromImage(bmp)
         g.SmoothingMode = SmoothingMode.AntiAlias
         g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit
         Using b As New SolidBrush(fill)
            g.FillEllipse(b, 0, 0, 15, 15)
         End Using
         Using f As New Font("Segoe UI", 8.0F, FontStyle.Bold)
            Dim sz As SizeF = g.MeasureString(glyph, f)
            g.DrawString(glyph, f, Brushes.White, (16 - sz.Width) / 2, (16 - sz.Height) / 2)
         End Using
      End Using
      Return bmp
   End Function

   Private Sub AddMother(p As Person)
      AddRelative(p, Sub(np) p.Relationships.Add(New Relationship With {.RelativeID = np.ID, .Type = Relationship.RelationType.Mother}))
   End Sub

   Private Sub AddFather(p As Person)
      AddRelative(p, Sub(np) p.Relationships.Add(New Relationship With {.RelativeID = np.ID, .Type = Relationship.RelationType.Father}))
   End Sub

   Private Sub AddSpouse(p As Person)
      AddRelative(p, Sub(np)
                        p.Relationships.Add(New Relationship With {.RelativeID = np.ID, .Type = Relationship.RelationType.Spouse})
                        np.Relationships.Add(New Relationship With {.RelativeID = p.ID, .Type = Relationship.RelationType.Spouse})
                     End Sub)
   End Sub

   Private Sub AddChild(p As Person)
      If Not p.Sex.HasValue Then
         MessageBox.Show("Set this person's sex first, so the new child can be linked as theirs (Father or Mother).",
                          "Add Child", MessageBoxButtons.OK, MessageBoxIcon.Information)
         Return
      End If
      Dim relType As Relationship.RelationType = If(p.Sex.Value = Person.SexType.Male, Relationship.RelationType.Father, Relationship.RelationType.Mother)
      AddRelative(p, Sub(np) np.Relationships.Add(New Relationship With {.RelativeID = p.ID, .Type = relType}))
   End Sub

   Private Sub AddSibling(p As Person)
      Dim parents As List(Of Relationship) = p.Relationships.
         Where(Function(r) r.Type = Relationship.RelationType.Father OrElse r.Type = Relationship.RelationType.Mother).ToList()
      If parents.Count = 0 Then
         MessageBox.Show("Add this person's mother or father first, so the sibling has a parent to share.",
                          "Add Sibling", MessageBoxButtons.OK, MessageBoxIcon.Information)
         Return
      End If
      AddRelative(p, Sub(np)
                        For Each r As Relationship In parents
                           np.Relationships.Add(New Relationship With {.RelativeID = r.RelativeID, .Type = r.Type})
                        Next
                     End Sub)
   End Sub

   ' Creates a blank person, wires up `link` (their relationship to `anchor`), saves,
   ' then opens frmPerson on the new person so its details can be filled in.
   Private Sub AddRelative(anchor As Person, link As Action(Of Person))
      Dim np As New Person With {.ID = Tree.NextID()}
      link(np)
      Tree.People.Add(np)
      Tree.Save()

      frmPerson.Tree = Tree
      frmPerson.FocusID = np.ID
      frmPerson.ShowDialog(Me)

      ' the new person (and anyone added via frmPerson's own Connections list while
      ' it was open) needs to be in _byId before the chart can draw links to them
      _byId.Clear()
      For Each person As Person In Tree.People
         _byId(person.ID) = person
      Next
      SetFocus(anchor)
   End Sub

   ' Builds the multi-line tooltip text shown on hover.
   Private Shared Function DescribePerson(p As Person) As String
      Dim lines As New List(Of String) From {
         $"{p.FirstName} {p.LastName}".Trim(),
         "ID: " & p.ID
      }
      If p.Sex.HasValue Then lines.Add("Sex: " & p.Sex.Value.ToString())
      If p.BirthDate.HasValue Then lines.Add("Born: " & p.BirthDate.Value.ToString("yyyy-MM-dd"))
      If p.DeathDate.HasValue Then lines.Add("Died: " & p.DeathDate.Value.ToString("yyyy-MM-dd"))
      If Not String.IsNullOrWhiteSpace(p.TreeLink) Then lines.Add($"Tree: {p.TreeLink.Trim()} (click + to open)")
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
