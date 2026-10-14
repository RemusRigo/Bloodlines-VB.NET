# Bloodlines: User's Guide



## Downloads & Runtimes

Download the latest release from the [Releases page](https://github.com/RemusRigo/Bloodlines-VB.NET/releases).

Each release contains one folder per framework and architecture:

|Folder|Target Framework|Architecture|Required Runtime|
|-|-|-|-|
|`net8.0 x64`|.NET 8.0|64-bit|[.NET 8.0 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0)|
|`net8.0 x86`|.NET 8.0|32-bit|[.NET 8.0 Desktop Runtime (x86)](https://dotnet.microsoft.com/download/dotnet/8.0)|
|`net10.0 x64`|.NET 10.0|64-bit|[.NET 10.0 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/10.0)|
|`net10.0 x86`|.NET 10.0|32-bit|[.NET 10.0 Desktop Runtime (x86)](https://dotnet.microsoft.com/download/dotnet/10.0)|

> **Which one do I need?** On 64-bit Windows 10/11, use **x64**. Pick the .NET version whose
> **Desktop Runtime** you already have installed, or install one from the links above.
> The plain ".NET Runtime" is not enough; Bloodlines needs the **Desktop** Runtime.

To check which runtimes are installed, open a command prompt and run:

```
dotnet --list-runtimes
```

Look for a `Microsoft.WindowsDesktop.App 8.x` or `10.x` line.



## Install

Bloodlines is portable: there is no installer.

1. Download and extract the folder that matches your system (see the table above), e.g. `net8.0 x64`.
2. Copy the **whole folder** to a location where you have write access, for example:
   `D:\My Apps\Bloodlines\`
   The folder must contain all 4 files; Bloodlines won't start if any of them is missing:
   * `Bloodlines.exe`
   * `Bloodlines.dll`
   * `Bloodlines.deps.json`
   * `Bloodlines.runtimeconfig.json`
3. Double-click `Bloodlines.exe`.

> Bloodlines saves your family trees, photos and settings next to `Bloodlines.exe`, so don't put it
> in a read-only folder such as `C:\Program Files\`.

> If Windows blocks the file ("Windows protected your PC"), right-click `Bloodlines.exe` →
> **Properties** → check **Unblock** → **OK**, then try again.



## Toolbar

|Button|Description|
|-|-|
|**New Family Tree**|Create a new, empty tree; you are asked for its name|
|**Load Family Tree**|Show the list of trees; double-click one to open it|
|**View Members**|Open the Person form to add, browse and edit the people in the tree|
|**List contacts**|Show all members in a table: ID, name, sex, birth & death date, father, mother, spouse|
|**Tree**|Show the tree chart|
|**Print**|Print the tree chart (only while the chart is shown)|
|**Export**|Save the tree chart as an image (only while the chart is shown)|
|**Settings**|Choose the tree view mode|

The buttons are enabled once a tree is created or loaded. There is no Save button: changes are saved
when you leave a person in the Person form.
Right-click the title bar → **About...** for version information.



## Your first tree

1. Click **New Family Tree** and enter a name, e.g. `Smith`.
   The name can't contain `\ / : * ? " < > |` and can't be the name of an existing tree.
2. Click **View Members** and fill in the first person. Person ID 1 is the person the chart starts from.
3. Click the **Add** button to save this person and start a new one; repeat for each family member.
4. Link the people together (see *Connections* below), or add relatives from the chart (see *Tree chart*).
5. Click **Tree** to see the chart.



## Person form

|Field|Description|
|-|-|
|**ID**|Set automatically|
|**First Name**, **Last Name**|Enter at least a name before you add a photo or a connection|
|**Birth Name**|e.g. maiden name|
|**Sex**|Male, Female, or empty (not specified)|
|**Birth Date**, **Death Date**|Check the box to enter the date; leave it unchecked if unknown. A death date before the birth date is rejected|
|**Birth Place**, **Death Place**|Free text|
|**Tree Link**|The name of another tree (see *Links between trees*)|
|**Notes**|Free text|
|**Photo**|Double-click the picture box to choose an image (JPG, PNG, BMP, GIF)|

Use the **Prev** / **Next** buttons to move between people and **Add** for a new person.
Moving to another person, **Ok**, or closing the form saves what's on screen.

### Connections

1. Choose a **Person ID** and a **Relation**, then click **+**.
   The relation says what the chosen person is to the current one: e.g. pick *John* and *Father* if John is this person's father.
2. To remove a connection, select it in the list and click **-**.

|Relation|Description|
|-|-|
|**Father**, **Mother**|Only one father and one mother per person|
|**Spouse**|Married; drawn with a solid line|
|**Divorced**|Former spouse; drawn with a dashed line|
|**Partner**|Together, not married; drawn with a dotted line|

Spouse, Divorced and Partner are added (and removed) on both people automatically.
Two people can have only one of these 3 relations at a time.



## Tree chart

The chart is centered on one person: ancestors above, descendants below, spouses and partners beside them.

|Action|Description|
|-|-|
|**Drag** with the left mouse button|Move the chart|
|**Mouse wheel** or **+** / **-** keys|Zoom in / out (25% - 300%)|
|**Double-click** a person|Center the chart on that person|
|**Right-click** a person|*Edit*, *Add Mother*, *Add Father*, *Add Child*, *Add Sibling*, *Add Spouse*, *Add Partner*|
|Click the blue **+** on a person|Open the tree linked to that person|



## Settings: Tree View Mode

|Mode|Description|
|-|-|
|**Normal**|Ancestors above the focus person, descendants below|
|**1st Line Siblings**|As Normal, plus the focus person's full and half siblings (in birth order, with their spouses)|
|**All Relatives**|Everyone in the tree; a person drawn twice gets a dashed border on the repeat|

Click **Ok** to save; the open chart is redrawn straight away.
The setting is saved in `Bloodlines.cfg` next to `Bloodlines.exe`.



## Links between trees

A person can point to another family tree, e.g. a spouse's own family:

1. Open the person in the Person form and enter the other tree's name in **Tree Link** (e.g. `Jones`).
   The text turns red if no tree with that name exists in the `Trees` folder.
2. In the chart, that person gets a blue **+** button; click it to open the linked tree.



## Print & Export

* **Print** prints the whole tree, not only the visible part. Choose a printer, then print from the preview window.
  If the tree fits on one page at 50% scale or more, it prints on one page; otherwise it is split across
  several pages at 50%. Wide trees are printed in landscape.
* **Export** saves the whole tree as a PNG or JPG image, at the current zoom level.
  Very large trees are scaled down to fit (max 30,000 px per side).



## Where is my data?

Everything is saved in the folder of `Bloodlines.exe`:

|Path|Content|
|-|-|
|`Trees\<tree name>.json`|The family tree|
|`Trees\<tree name>\<person ID>.png`|Profile pictures|
|`Bloodlines.cfg`|Settings|

To back up your trees, copy the `Trees` folder.



## Uninstall

1. Close Bloodlines.
2. Optional: copy the `Trees` folder somewhere safe to keep your family trees.
3. Delete the folder you copied in step 2 of *Install*.
