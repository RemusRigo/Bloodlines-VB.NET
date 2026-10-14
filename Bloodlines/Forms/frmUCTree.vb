'--------------------------------------------------------------------------------------------------
' Bloodlines: frmBloodlines.vb: Main form
'    © 2026 Remus Rigo
'       v1.0.20260925
'--------------------------------------------------------------------------------------------------

Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Drawing.Printing
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

   ' What the chart shows around the focus person (see TreeViewMode). Changing it
   ' rebuilds the chart around the current focus person.
   Private _viewMode As TreeViewMode = TreeViewMode.Normal
   <Browsable(False)>
   <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
   Public Property ViewMode As TreeViewMode
      Get
         Return _viewMode
      End Get
      Set(value As TreeViewMode)
         If value = _viewMode Then Return
         _viewMode = value
         If _focusPerson IsNot Nothing Then SetFocus(_focusPerson)
      End Set
   End Property

   ' --- layout constants (pixels, unscaled - i.e. measured at zoom = 1.0) ---
   Private Const NodeWidth As Integer = 210     ' width of one person's card
   Private Const NodeHeight As Integer = 84     ' height of one person's card
   Private Const HGap As Integer = 26           ' horizontal gap between sibling subtrees
   Private Const VGap As Integer = 46           ' vertical gap between generations
   Private Const TreeMargin As Integer = 24     ' padding around the whole layout
   Private Const MaxDepth As Integer = 20       ' guard against pathological/cyclic data
   Private Const SpouseGap As Integer = 24      ' gap between a person and their spouse box(es) - wide enough to show a dashed/dotted couple line
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
      Public ReadOnly SpouseTypes As New List(Of Relationship.RelationType)  ' Spouse/Divorced/Partner, parallel to Spouses
      Public ReadOnly SpouseRepeats As New List(Of Boolean)  ' Full mode: that spouse is also drawn elsewhere, parallel to Spouses
      Public IsRepeat As Boolean                        ' Full mode: this person is also drawn elsewhere
      Public Cx As Integer                              ' couple block centre X (content coords)
      Public Y As Integer                               ' box top Y (content coords)
      Public L As Integer                               ' subtree extent left of PrimaryCx, used only during layout
      Public R As Integer                               ' subtree extent right of PrimaryCx, used only during layout
   End Class

   Private ReadOnly _byId As New Dictionary(Of Integer, Person)   ' fast Person lookup by ID
   Private ReadOnly _allNodes As New List(Of Node)                ' every node currently laid out/drawn
   Private _focus As Node                                          ' root of the hourglass layout (Nothing in Full mode)
   Private _focusPerson As Person                                  ' the person currently centred/highlighted
   Private ReadOnly _siblings As New List(Of Node)                 ' Siblings mode: the focus person's siblings, in the focus row
   Private ReadOnly _roots As New List(Of Node)                    ' Full mode: top of each family line, laid out side by side
   Private ReadOnly _forestNodes As New HashSet(Of Integer)        ' Full mode, while building: IDs that already have their own node
   Private ReadOnly _links As New List(Of (A As Person, B As Person, Type As Relationship.RelationType))  ' Full mode: couples drawn in separate branches, joined by a long line
   Private _focusTopY As Integer                                   ' Y of the focus row, used while placing ancestors/descendants

   Private _nameFont As Font
   Private _dateFont As Font
   Private ReadOnly _tip As New ToolTip()
   Private ReadOnly _photos As New Dictionary(Of Integer, Image)   ' person ID -> photo (Nothing = none)
   Private ReadOnly _menuIcons As New Dictionary(Of String, Image) From {
      {"Edit", MakeMenuIcon(Color.FromArgb(90, 160, 90), "E")},
      {"Mother", MakeMenuIcon(Color.FromArgb(219, 112, 147), "M")},
      {"Father", MakeMenuIcon(Color.FromArgb(70, 130, 220), "F")},
      {"Child", MakeMenuIcon(Color.FromArgb(150, 150, 150), "C")},
      {"Sibling", MakeMenuIcon(Color.FromArgb(150, 150, 150), "S")},
      {"Spouse", MakeMenuIcon(Color.FromArgb(180, 140, 200), "♥")},
      {"Partner", MakeMenuIcon(Color.FromArgb(200, 170, 220), "P")}
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
      _focusPerson = focus
      _siblings.Clear()
      _roots.Clear()
      _links.Clear()
      If focus Is Nothing Then Return

      If _viewMode = TreeViewMode.Full Then
         BuildForest(focus)
         Return
      End If

      Dim down As Node = BuildDown(focus, 0, New HashSet(Of Integer))
      Dim up As Node = BuildUp(focus, 0, New HashSet(Of Integer))
      down.Parents.AddRange(up.Parents)
      _focus = down

      If _viewMode = TreeViewMode.Siblings Then BuildSiblings()
   End Sub

   ' A node for p with p's partners (Spouse/Divorced/Partner) beside them, except anyone in
   ' `skip`. With `placed` (Full mode), everyone drawn is recorded, and a person who was
   ' already drawn elsewhere is still drawn here but flagged as a repeat.
   Private Function NewNode(p As Person, Optional placed As HashSet(Of Integer) = Nothing,
                            Optional skip As HashSet(Of Integer) = Nothing) As Node
      Dim n As New Node With {.Person = p}
      If placed IsNot Nothing Then n.IsRepeat = Not placed.Add(p.ID)

      For Each r As Relationship In p.Relationships.Where(Function(x) Relationship.IsCouple(x.Type))
         Dim spouse As Person = Nothing
         If Not _byId.TryGetValue(r.RelativeID, spouse) Then Continue For
         If n.Spouses.Contains(spouse) Then Continue For   ' e.g. listed as both Spouse and Divorced
         If skip IsNot Nothing AndAlso skip.Contains(spouse.ID) Then Continue For
         If placed IsNot Nothing AndAlso HasParents(spouse) Then
            ' Full mode: a partner with their own family here is drawn in that family's
            ' branch instead, and the two cards are joined by a long line (see DrawLinks).
            If Not _links.Any(Function(l) (l.A Is p AndAlso l.B Is spouse) OrElse (l.A Is spouse AndAlso l.B Is p)) Then
               _links.Add((p, spouse, r.Type))
            End If
            Continue For
         End If
         n.Spouses.Add(spouse)
         n.SpouseTypes.Add(r.Type)
         n.SpouseRepeats.Add(placed IsNot Nothing AndAlso Not placed.Add(spouse.ID))
      Next
      Return n
   End Function

   ' Recursively builds the descendant side (p and everyone below them), plus p's spouses.
   ' `path` tracks ancestors currently being recursed through, to guard against a bad/cyclic
   ' relationship (e.g. someone accidentally listed as their own descendant) infinite-looping.
   ' `placed` (Full mode only): see BuildForest.
   Private Function BuildDown(p As Person, depth As Integer, path As HashSet(Of Integer),
                              Optional placed As HashSet(Of Integer) = Nothing) As Node
      Dim n As Node = NewNode(p, placed)
      If placed IsNot Nothing Then _forestNodes.Add(p.ID)

      If depth >= MaxDepth OrElse Not path.Add(p.ID) Then Return n

      For Each child As Person In Tree.GetChildren(p.ID).
            OrderBy(Function(c) c.BirthDate).ThenBy(Function(c) c.ID)
         If placed IsNot Nothing AndAlso _forestNodes.Contains(child.ID) Then Continue For
         n.Kids.Add(BuildDown(child, depth + 1, path, placed))
      Next

      path.Remove(p.ID)
      Return n
   End Function

   ' Siblings mode: everyone else who is a child of one of the focus person's parents
   ' (full or half siblings). Shown with their spouses but not their descendants, and
   ' linked to whichever of the focus's parents they share.
   Private Sub BuildSiblings()
      Dim seen As New HashSet(Of Integer) From {_focus.Person.ID}
      For Each parentNode As Node In _focus.Parents
         For Each child As Person In Tree.GetChildren(parentNode.Person.ID)
            If Not seen.Add(child.ID) Then Continue For
            Dim sib As Node = NewNode(child)
            sib.Parents.AddRange(_focus.Parents.Where(Function(pn) IsChildOf(child, pn.Person.ID)))
            _siblings.Add(sib)
         Next
      Next
   End Sub

   Private Shared Function IsChildOf(child As Person, parentId As Integer) As Boolean
      Return child.Relationships.Any(Function(r) r.RelativeID = parentId AndAlso
         (r.Type = Relationship.RelationType.Father OrElse r.Type = Relationship.RelationType.Mother))
   End Function

   ' Full mode: every person in the tree, as a row of descendant trees side by side.
   ' The first one starts from the focus person's top-most ancestor, so their own line
   ' comes first; then one per remaining person with no parents in the tree; then anyone
   ' still not shown (e.g. a child whose only listed parent appears as someone's spouse).
   ' A partner with their own parents in the tree stays in their own family's branch and
   ' is joined to their partner by a long line (_links, see NewNode/DrawLinks); only a
   ' partner with no family here is drawn beside them. A couple's children appear once,
   ' under whichever parent's line is built first. Everyone gets their own box (a node) at
   ' most once; a partner box for someone already drawn elsewhere gets a dashed border
   ' (see Node.IsRepeat). LayoutForest shifts lines up/down to put linked couples in the same row.
   Private Sub BuildForest(focus As Person)
      Dim placed As New HashSet(Of Integer)       ' everyone drawn so far, as a node or as a partner
      _forestNodes.Clear()
      Dim addRoot = Sub(p As Person)
                       If Not placed.Contains(p.ID) Then _roots.Add(BuildDown(p, 0, New HashSet(Of Integer), placed))
                    End Sub

      addRoot(TopAncestor(focus))

      ' A parentless person married to someone who does have parents here is left for
      ' that spouse's line to pick up, rather than starting a line of their own.
      For Each p As Person In Tree.People.Where(Function(x) Not HasParents(x)).OrderBy(Function(x) x.ID)
         Dim marriedIn As Boolean = p.Relationships.Any(Function(r)
                                                           Dim sp As Person = Nothing
                                                           Return Relationship.IsCouple(r.Type) AndAlso
                                                                  _byId.TryGetValue(r.RelativeID, sp) AndAlso HasParents(sp)
                                                        End Function)
         If Not marriedIn Then addRoot(p)
      Next

      For Each p As Person In Tree.People.OrderBy(Function(x) x.ID)
         addRoot(p)
      Next
   End Sub

   Private Function HasParents(p As Person) As Boolean
      Return p.Relationships.Any(Function(r) (r.Type = Relationship.RelationType.Father OrElse r.Type = Relationship.RelationType.Mother) AndAlso
                                             _byId.ContainsKey(r.RelativeID))
   End Function

   ' Follows the first listed parent (father before mother, as in BuildUp) up to someone
   ' with no parents in the tree. Stops early on a cycle.
   Private Function TopAncestor(p As Person) As Person
      Dim seen As New HashSet(Of Integer)
      Do While seen.Add(p.ID)
         Dim r As Relationship = p.Relationships.
            Where(Function(x) (x.Type = Relationship.RelationType.Father OrElse x.Type = Relationship.RelationType.Mother) AndAlso
                              _byId.ContainsKey(x.RelativeID)).
            OrderBy(Function(x) x.Type).FirstOrDefault()
         If r Is Nothing Then Exit Do
         p = _byId(r.RelativeID)
      Loop
      Return p
   End Function

   ' Recursively builds the ancestor side (p's parents, grandparents, ...).
   ' Same cycle-guard as BuildDown. Each ancestor is shown with their partners, except
   ' the child's other parent (`coParents`), who's already drawn beside them as a parent -
   ' so e.g. a grandmother's second husband appears, but not her first one twice.
   Private Function BuildUp(p As Person, depth As Integer, path As HashSet(Of Integer),
                            Optional coParents As HashSet(Of Integer) = Nothing) As Node
      Dim n As Node = NewNode(p, skip:=coParents)
      If depth >= MaxDepth OrElse Not path.Add(p.ID) Then Return n

      Dim parentRels As List(Of Relationship) = p.Relationships.
            Where(Function(x) x.Type = Relationship.RelationType.Father OrElse x.Type = Relationship.RelationType.Mother).
            OrderBy(Function(x) x.Type).ToList()
      Dim parentIds As New HashSet(Of Integer)(parentRels.Select(Function(x) x.RelativeID))
      Dim added As New HashSet(Of Integer)
      For Each r As Relationship In parentRels
         Dim parent As Person = Nothing
         If _byId.TryGetValue(r.RelativeID, parent) AndAlso added.Add(r.RelativeID) Then
            n.Parents.Add(BuildUp(parent, depth + 1, path, parentIds))
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

      Dim kidsOf As Func(Of Node, List(Of Node)) = Function(n) n.Kids
      Dim parentsOf As Func(Of Node, List(Of Node)) = Function(n) n.Parents

      If _roots.Count > 0 Then
         LayoutForest(kidsOf)
      ElseIf _focus IsNot Nothing Then
         LayoutHourglass(kidsOf, parentsOf)
      End If
      If _allNodes.Count = 0 Then Return

      ' Shift everything so the left-most box sits exactly TreeMargin pixels from content X = 0.
      Dim minLeft As Integer = _allNodes.Min(Function(n) n.Cx - CoupleWidth(n) \ 2)
      Dim dx As Integer = TreeMargin - minLeft
      For Each n As Node In _allNodes
         n.Cx += dx
      Next
   End Sub

   ' Full mode: each root's descendant tree, left to right, all starting on the top row.
   ' Each line starts on whatever row puts couples joined by a long line (_links) side by
   ' side in the same generation row, where possible.
   Private Sub LayoutForest(kidsOf As Func(Of Node, List(Of Node)))
      _focusTopY = TreeMargin

      ' which line each person's own node is in, and its generation within that line
      Dim where As New Dictionary(Of Integer, (Root As Integer, Gen As Integer))
      For i As Integer = 0 To _roots.Count - 1
         MapGens(_roots(i), i, 0, where)
      Next

      ' Starting row per line: walk outward from each not-yet-placed line through the
      ' links, shifting each newly reached line to line its partner up with the one already
      ' placed. (With conflicting links, the first one found wins.)
      Dim startGen As New Dictionary(Of Integer, Integer)
      For seed As Integer = 0 To _roots.Count - 1
         If startGen.ContainsKey(seed) Then Continue For
         startGen(seed) = 0
         Dim queue As New Queue(Of Integer)
         queue.Enqueue(seed)
         Do While queue.Count > 0
            Dim cur As Integer = queue.Dequeue()
            For Each lk In _links
               Dim a, b As (Root As Integer, Gen As Integer)
               If Not where.TryGetValue(lk.A.ID, a) OrElse Not where.TryGetValue(lk.B.ID, b) Then Continue For
               If b.Root = cur Then
                  Dim t = a : a = b : b = t
               End If
               If a.Root <> cur OrElse startGen.ContainsKey(b.Root) Then Continue For
               startGen(b.Root) = startGen(cur) + a.Gen - b.Gen
               queue.Enqueue(b.Root)
            Next
         Loop
      Next
      Dim topGen As Integer = startGen.Values.Min()

      Dim x As Integer = 0
      Dim seen As New HashSet(Of Node)
      For i As Integer = 0 To _roots.Count - 1
         Dim root As Node = _roots(i)
         Measure(root, kidsOf, New HashSet(Of Node))
         PlaceDown(root, x + root.L, startGen(i) - topGen)
         x += root.L + root.R + HGap * 2   ' a wider gap between separate family lines
         Collect(root, kidsOf, seen, _allNodes)
      Next
   End Sub

   ' Records the generation (within its own line) of every node below `n`; first one wins.
   Private Shared Sub MapGens(n As Node, root As Integer, gen As Integer, where As Dictionary(Of Integer, (Root As Integer, Gen As Integer)))
      If where.ContainsKey(n.Person.ID) Then Return
      where(n.Person.ID) = (root, gen)
      For Each k As Node In n.Kids
         MapGens(k, root, gen + 1, where)
      Next
   End Sub

   Private Sub LayoutHourglass(kidsOf As Func(Of Node, List(Of Node)), parentsOf As Func(Of Node, List(Of Node)))

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

      PlaceSiblings()
      _allNodes.AddRange(_siblings)
   End Sub

   ' Siblings mode: lines the siblings up in the focus row in birth order - older ones to
   ' the left of the focus person, younger ones to the right of the focus couple block.
   ' Only the focus row is shared (siblings have no descendants or ancestors of their
   ' own drawn), so they just need to clear the focus's own couple block.
   Private Sub PlaceSiblings()
      If _siblings.Count = 0 Then Return
      Dim byAge As List(Of Node) = _siblings.Append(_focus).
         OrderBy(Function(n) n.Person.BirthDate).ThenBy(Function(n) n.Person.ID).ToList()
      Dim at As Integer = byAge.IndexOf(_focus)

      Dim edge As Integer = _focus.Cx - CoupleWidth(_focus) \ 2
      For i As Integer = at - 1 To 0 Step -1
         Dim n As Node = byAge(i)
         edge -= HGap + CoupleWidth(n)
         n.Cx = edge + CoupleWidth(n) \ 2
         n.Y = _focus.Y
      Next

      edge = _focus.Cx + CoupleWidth(_focus) - CoupleWidth(_focus) \ 2
      For i As Integer = at + 1 To byAge.Count - 1
         Dim n As Node = byAge(i)
         n.Cx = edge + HGap + CoupleWidth(n) \ 2
         n.Y = _focus.Y
         edge += HGap + CoupleWidth(n)
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
      Dim box As Rectangle? = FocusBox()
      If Not box.HasValue Then Return
      Dim cx As Integer = box.Value.X + NodeWidth \ 2
      Dim cy As Integer = box.Value.Y + NodeHeight \ 2
      _panX = pnlChart.ClientSize.Width / 2.0F - cx * _zoom
      _panY = pnlChart.ClientSize.Height / 2.0F - cy * _zoom
   End Sub

   ' The focus person's card, in content coordinates. In Full mode they may be drawn as
   ' someone's spouse rather than as their own node, so search every card.
   Private Function FocusBox() As Rectangle?
      ' Prefer their first appearance over a repeat (Full mode draws some people twice).
      If _focusPerson Is Nothing Then Return Nothing
      Dim repeat As Rectangle? = Nothing
      For Each n As Node In _allNodes
         Dim rect As New Rectangle(n.Cx - CoupleWidth(n) \ 2, n.Y, NodeWidth, NodeHeight)
         If n.Person Is _focusPerson Then
            If Not n.IsRepeat Then Return rect
            If Not repeat.HasValue Then repeat = rect
         End If
         For i As Integer = 0 To n.Spouses.Count - 1
            rect = New Rectangle(rect.Right + SpouseGap, n.Y, NodeWidth, NodeHeight)
            If n.Spouses(i) Is _focusPerson Then
               If Not n.SpouseRepeats(i) Then Return rect
               If Not repeat.HasValue Then repeat = rect
            End If
         Next
      Next
      Return repeat
   End Function

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

      If _allNodes.Count = 0 Then
         ' drawn in plain screen coordinates, unscaled
         g.DrawString("Load a family tree, then pick a focus person.", Font, Brushes.Gray, TreeMargin, TreeMargin)
         Return
      End If

      ' Everything in DrawChart is drawn in unscaled content coordinates; this transform maps
      ' it onto the screen. Order matters: translate first, then scale, so _panX/_panY
      ' stay in screen pixels regardless of the current zoom level.
      g.TranslateTransform(_panX, _panY)
      g.ScaleTransform(_zoom, _zoom)
      DrawChart(g)
   End Sub

   ' Draws the whole chart in content coordinates - the caller sets up the transform
   ' (screen pan/zoom, exported bitmap, or printed page).
   Private Sub DrawChart(g As Graphics)
      ' Connector lines first, so they sit behind the boxes.
      Using pen As New Pen(Color.FromArgb(150, 150, 150), 0)
         For Each n As Node In _allNodes
            DrawDownConnector(g, pen, n)
            DrawUpConnector(g, pen, n)
         Next
      End Using
      DrawLinks(g)

      For Each n As Node In _allNodes
         DrawCouple(g, n)
      Next
   End Sub

   ' Full mode: the long lines joining couples drawn in separate branches, in the same
   ' solid/dashed/dotted style as a couple drawn side by side (see CouplePen).
   Private Sub DrawLinks(g As Graphics)
      If _links.Count = 0 Then Return
      Dim cards As New Dictionary(Of Integer, Rectangle)   ' person ID -> their own (first) card
      For Each n As Node In _allNodes
         If n.IsRepeat OrElse cards.ContainsKey(n.Person.ID) Then Continue For
         cards(n.Person.ID) = New Rectangle(n.Cx - CoupleWidth(n) \ 2, n.Y, NodeWidth, NodeHeight)
      Next

      For Each lk In _links
         Dim a, b As Rectangle
         If Not cards.TryGetValue(lk.A.ID, a) OrElse Not cards.TryGetValue(lk.B.ID, b) Then Continue For
         Using pen As Pen = CouplePen(lk.Type)
            g.DrawLines(pen, LinkPath(a, b))
         End Using
      Next
   End Sub

   ' Route for a long couple line: out of the top of each card (or the bottom, for the
   ' higher card when they're in different rows), along a lane just above the lower card's
   ' row - below the parents' connector bus - so it doesn't run through the cards in between.
   Private Shared Function LinkPath(a As Rectangle, b As Rectangle) As Point()
      If b.X < a.X Then
         Dim t As Rectangle = a : a = b : b = t
      End If
      Dim laneY As Integer = Math.Max(a.Y, b.Y) - VGap \ 4
      Dim xa As Integer = a.Right - NodeWidth \ 4
      Dim xb As Integer = b.Left + NodeWidth \ 4
      Dim ya As Integer = If(a.Y < b.Y, a.Bottom, a.Y)
      Dim yb As Integer = If(b.Y < a.Y, b.Bottom, b.Y)
      Return {New Point(xa, ya), New Point(xa, laneY), New Point(xb, laneY), New Point(xb, yb)}
   End Function

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
   Private Sub DrawCouple(g As Graphics, n As Node)
      Dim left As Integer = n.Cx - CoupleWidth(n) \ 2
      Dim rect As New Rectangle(left, n.Y, NodeWidth, NodeHeight)
      DrawBox(g, rect, n.Person, n.Person Is _focusPerson, n.IsRepeat)

      Dim prevRect As Rectangle = rect
      For i As Integer = 0 To n.Spouses.Count - 1
         Dim sp As Person = n.Spouses(i)
         Dim spRect As New Rectangle(prevRect.Right + SpouseGap, n.Y, NodeWidth, NodeHeight)
         DrawBox(g, spRect, sp, sp Is _focusPerson, n.SpouseRepeats(i))

         Using pen As Pen = CouplePen(n.SpouseTypes(i))
            Dim midY As Integer = n.Y + NodeHeight \ 2
            g.DrawLine(pen, prevRect.Right, midY, spRect.Left, midY)
         End Using
         prevRect = spRect
      Next
   End Sub

   ' The line joining a couple: solid for Spouse, dashed (- - -) for Divorced, dotted (. . .)
   ' for Partner. The patterned pens are 1.5 wide rather than cosmetic (width 0), so the
   ' dashes/dots scale with the zoom like the boxes do and stay readable.
   Private Shared Function CouplePen(type As Relationship.RelationType) As Pen
      Dim color As Color = Color.FromArgb(150, 150, 150)
      Select Case type
         Case Relationship.RelationType.Divorced
            Return New Pen(color, 1.5F) With {.DashPattern = {3.0F, 2.0F}}   ' in pen widths: 4.5px dash, 3px gap
         Case Relationship.RelationType.Partner
            Return New Pen(color, 1.5F) With {.DashPattern = {1.0F, 2.0F}}   ' 1.5px dot, 3px gap
         Case Else
            Return New Pen(color, 0)
      End Select
   End Function

   ' A person "card": photo on the left; first name, last name and (born - died) on the right.
   ' The focus person gets a heavier blue outline.
   ' A repeat (Full mode: this person is also drawn elsewhere) gets a dashed border.
   Private Sub DrawBox(g As Graphics, rect As Rectangle, person As Person, isFocus As Boolean, Optional isRepeat As Boolean = False)
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
            If isRepeat Then pen.DashStyle = DashStyle.Dash
            g.DrawRectangle(pen, rect)
         End Using
      Else
         ' Width 0 = a GDI+ "cosmetic" pen: always exactly 1 device pixel wide,
         ' unaffected by the world transform, so thin borders don't vanish when
         ' zoomed out or turn blurry when zoomed in. A repeat's dashed border is
         ' 1.5 wide instead, so its dashes scale with the zoom like the couple lines.
         Using pen As New Pen(Color.FromArgb(120, 120, 120), If(isRepeat, 1.5F, 0))
            If isRepeat Then pen.DashPattern = {3.0F, 2.0F}
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
      menu.Items.Add("Edit", _menuIcons("Edit"), Sub() RunDeferred(Sub() EditPerson(p)))
      menu.Items.Add(New ToolStripSeparator())
      menu.Items.Add("Add Mother", _menuIcons("Mother"), Sub() RunDeferred(Sub() AddMother(p))).Enabled = Not hasMother
      menu.Items.Add("Add Father", _menuIcons("Father"), Sub() RunDeferred(Sub() AddFather(p))).Enabled = Not hasFather
      menu.Items.Add("Add Child", _menuIcons("Child"), Sub() RunDeferred(Sub() AddChild(p)))
      menu.Items.Add("Add Sibling", _menuIcons("Sibling"), Sub() RunDeferred(Sub() AddSibling(p)))
      menu.Items.Add("Add Spouse", _menuIcons("Spouse"), Sub() RunDeferred(Sub() AddSpouse(p, Relationship.RelationType.Spouse)))
      menu.Items.Add("Add Partner", _menuIcons("Partner"), Sub() RunDeferred(Sub() AddSpouse(p, Relationship.RelationType.Partner)))
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

   ' Adds a new person linked to p as a couple - `type` is Spouse or Partner.
   Private Sub AddSpouse(p As Person, type As Relationship.RelationType)
      AddRelative(p, Sub(np)
                        p.Relationships.Add(New Relationship With {.RelativeID = np.ID, .Type = type})
                        np.Relationships.Add(New Relationship With {.RelativeID = p.ID, .Type = type})
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

   ' Opens frmPerson on an existing person, then redraws the chart so name/sex/date
   ' edits show up. The view stays centred on the current focus, not on whoever was edited.
   Private Sub EditPerson(p As Person)
      frmPerson.Tree = Tree
      frmPerson.FocusID = p.ID
      frmPerson.ShowDialog(Me)

      RebuildIndex()
      SetFocus(If(_focusPerson, p))
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

      RebuildIndex()
      SetFocus(anchor)
   End Sub

   ' Anyone added via frmPerson's own Connections list while it was open needs to be
   ' in _byId before the chart can draw links to them.
   Private Sub RebuildIndex()
      _byId.Clear()
      For Each person As Person In Tree.People
         _byId(person.ID) = person
      Next
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

   ' ------------------------------------------------------- print / export ---

   ' Printing never shrinks the chart below this; a tree that would need a smaller scale
   ' to fit one page is printed at this scale instead, tiled across several pages.
   Private Const MinPrintScale As Single = 0.5F

   ' True once there is something on the chart to print/export.
   <Browsable(False)>
   Public ReadOnly Property HasChart As Boolean
      Get
         Return _allNodes.Count > 0
      End Get
   End Property

   ' Bounding box of every card, in content coordinates, plus the usual margin.
   Private Function ContentBounds() As Rectangle
      Dim r As Rectangle = Rectangle.Empty
      For Each n As Node In _allNodes
         Dim box As New Rectangle(n.Cx - CoupleWidth(n) \ 2, n.Y, CoupleWidth(n), NodeHeight)
         r = If(r.IsEmpty, box, Rectangle.Union(r, box))
      Next
      r.Inflate(TreeMargin, TreeMargin)
      Return r
   End Function

   ' Export: the whole chart (not just the visible part) at the current zoom level,
   ' so the image is exactly as big as the chart would be on screen if it all fit.
   ' Capped so a huge tree zoomed in can't exceed what GDI+ / memory can handle.
   Private Const MaxExportSide As Integer = 30000      ' pixels, per side
   Private Const MaxExportPixels As Long = 200_000_000 ' ~800 MB at 32 bpp

   Public Function RenderChart() As Bitmap
      Dim bounds As Rectangle = ContentBounds()
      Dim scale As Single = _zoom
      scale = Math.Min(scale, MaxExportSide / CSng(bounds.Width))
      scale = Math.Min(scale, MaxExportSide / CSng(bounds.Height))
      scale = Math.Min(scale, CSng(Math.Sqrt(MaxExportPixels / (CDbl(bounds.Width) * bounds.Height))))

      Dim bmp As New Bitmap(Math.Max(1, CInt(Math.Ceiling(bounds.Width * scale))),
                            Math.Max(1, CInt(Math.Ceiling(bounds.Height * scale))))
      Using g As Graphics = Graphics.FromImage(bmp)
         g.Clear(pnlChart.BackColor)
         g.SmoothingMode = SmoothingMode.AntiAlias
         g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit ' ClearType looks fringed in a saved image
         g.ScaleTransform(scale, scale)
         g.TranslateTransform(-bounds.Left, -bounds.Top)
         DrawChart(g)
      End Using
      Return bmp
   End Function

   ' Print: the whole chart (not just the visible part). Fits one page when it can do so
   ' at MinPrintScale or larger (never enlarged past 100%); otherwise it is tiled across
   ' pages, left to right then top to bottom. The caller owns (and disposes) the document.
   Public Function CreatePrintDocument(title As String) As PrintDocument
      Dim doc As New PrintDocument With {.DocumentName = title}
      Dim bounds As Rectangle = ContentBounds()
      doc.DefaultPageSettings.Landscape = bounds.Width > bounds.Height

      ' pagination, worked out on the first page (the page size isn't known before then)
      Dim page As Integer, cols As Integer, rows As Integer, scale As Single

      AddHandler doc.BeginPrint, Sub(sender As Object, e As PrintEventArgs) page = 0 ' preview and print each run the pages from the start

      AddHandler doc.PrintPage,
         Sub(sender As Object, e As PrintPageEventArgs)
            Dim g As Graphics = e.Graphics
            Dim mb As Rectangle = e.MarginBounds ' 1/100 inch; 1 content pixel = 1/100 inch at 100%
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit

            ' header line: title on the left, page number on the right
            Dim headerHeight As Integer
            Using f As New Font(Font.FontFamily, 12, FontStyle.Bold)
               headerHeight = CInt(g.MeasureString("Ag", f).Height) + 8
               g.DrawString(title, f, Brushes.Black, mb.Left, mb.Top)
               If page = 0 Then
                  Dim areaW As Single = mb.Width, areaH As Single = mb.Height - headerHeight
                  scale = Math.Min(1.0F, Math.Min(areaW / bounds.Width, areaH / bounds.Height))
                  If scale < MinPrintScale Then scale = MinPrintScale
                  cols = CInt(Math.Ceiling(bounds.Width * scale / areaW))
                  rows = CInt(Math.Ceiling(bounds.Height * scale / areaH))
               End If
               If cols * rows > 1 Then
                  Dim pageText As String = $"Page {page + 1} of {cols * rows}"
                  Dim w As Single = g.MeasureString(pageText, f).Width
                  g.DrawString(pageText, f, Brushes.Black, mb.Right - w, mb.Top)
               End If
            End Using
            Dim area As New Rectangle(mb.Left, mb.Top + headerHeight, mb.Width, mb.Height - headerHeight)

            ' which slice of the chart this page shows (content coords)
            Dim tileW As Single = area.Width / scale, tileH As Single = area.Height / scale
            Dim col As Integer = page Mod cols, row As Integer = page \ cols
            Dim originX As Single = bounds.Left + col * tileW
            Dim originY As Single = bounds.Top + row * tileH
            Dim offX As Single = area.Left, offY As Single = area.Top
            If cols = 1 Then offX += (area.Width - bounds.Width * scale) / 2 ' centre a chart that fits
            If rows = 1 Then offY += (area.Height - bounds.Height * scale) / 2

            g.SetClip(area) ' keep neighbouring tiles' content out of the header/margins
            g.TranslateTransform(offX, offY)
            g.ScaleTransform(scale, scale)
            g.TranslateTransform(-originX, -originY)
            DrawChart(g)

            page += 1
            e.HasMorePages = page < cols * rows
         End Sub
      Return doc
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
