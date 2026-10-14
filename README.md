# Bloodlines
Bloodlines: a Windows family tree app



## Author

* [@remusrigo](https://github.com/RemusRigo)
* [@remusrigo](https://sourceforge.net/u/remusrigo/profile)



## Features

* Person details: first/last/birth name, sex, birth & death date and place, notes, profile picture
* Relationships: Father, Mother, Spouse, Divorced, Partner
* Members list with parents and spouses
* Multiple family trees, each saved as a JSON file in the `Trees` folder
* Links between trees: a person can open another family tree (e.g. a spouse's own tree)
* 3 view modes: Normal, 1st Line Siblings, All Relatives
* Interactive tree chart ("hourglass": ancestors above, descendants below)
  * pan with the mouse, zoom with the mouse wheel or the +/- keys
  * double-click a person to center the chart on them
  * right-click a person to edit them, or to add a mother, father, child, sibling, spouse or partner
* Print the whole tree (on one page, or tiled across several pages)
* Export the whole tree as a PNG or JPG image



## User's Guide

See the [User's Guide](UsersGuide.md) for downloads, runtimes, installation and usage.



## Statistics

![VB.NET](https://img.shields.io/badge/Language-VB.NET-512BD4?labelColor=555555)
![VS](https://img.shields.io/badge/IDE-Visual%20Studio-5C2D91?labelColor=555555)
![JSON](https://img.shields.io/badge/File-JSON-5C2D91?labelColor=555555)
<br>
![status](https://img.shields.io/badge/status-in%20progress-orange) 
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/RemusRigo/Bloodlines-VB.NET/total)



## Roadmap

* 2026-09-30: Add: Print and Export (PNG/JPG) the whole tree chart
* 2026-09-30: Add: Settings: tree view modes (Normal, 1st Line Siblings, All Relatives)
* 2026-09-30: Add: Divorced and Partner relationships
* 2026-09-30: Add: About box
* 2026-09-30: Upd: removed the Save button (the Person form saves the edits)
* 2026-09-30: Fix: a tree file can't be left half-written if the app crashes while saving
* 2026-09-27: Add: "Edit" on right-click on person panel
* 2026-09-24: Add: JSON v1.03: links between trees (Tree Link)
* 2026-09-24: Fix: overlapping person cards in the tree chart
* 2026-09-24: Fix: closing the Person form with X/Esc/Alt+F4 now saves the edits
* 2026-09-22: Upd: removed old form that display the tree chart
* 2026-09-21: Add: image for person
* 2026-09-21: Add: JSON v1.02: add Birth Name. Birth Place and Death Place
* 2026-09-21: Upd: JSON v1.01: remove unknown from Person.Sex (if sex is unknown, then it becomes null)
* 2026-09-21: Fix: minor naming
* 2026-09-20: Fix: some connections not drawn
* 2026-09-20: Fix: text not inside person panel
* 2026-09-20: Add: New User Control form to display tree chart
* 2026-09-07: Add: User/Add User by [Md Tanvirul Haque](https://www.flaticon.com/authors/md-tanvirul-haque)
* 2026-09-07: Add: [Save icon](https://www.flaticon.com/free-icon/save_5234222) by [Flat Icons](https://www.flaticon.com/authors/flat-icons)
* 2026-09-07: Add: UI
* 2026-09-07: Add: FamilyTree class
* 2026-09-06: Add: Person class
* 2026-09-06: Project started
