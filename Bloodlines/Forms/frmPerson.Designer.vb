<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPerson
   Inherits System.Windows.Forms.Form

   'Form overrides dispose to clean up the component list.
   <System.Diagnostics.DebuggerNonUserCode()> _
   Protected Overrides Sub Dispose(ByVal disposing As Boolean)
      Try
         If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
         End If
      Finally
         MyBase.Dispose(disposing)
      End Try
   End Sub

   'Required by the Windows Form Designer
   Private components As System.ComponentModel.IContainer

   'NOTE: The following procedure is required by the Windows Form Designer
   'It can be modified using the Windows Form Designer.  
   'Do not modify it using the code editor.
   <System.Diagnostics.DebuggerStepThrough()> _
   Private Sub InitializeComponent()
      btnOk = New Button()
      lblID = New Label()
      txtBoxID = New TextBox()
      txtBoxFirstName = New TextBox()
      lblFirstName = New Label()
      txtBoxLastName = New TextBox()
      lblLastName = New Label()
      lblSex = New Label()
      cbSex = New ComboBox()
      grpBoxNotes = New GroupBox()
      txtBoxNotes = New TextBox()
      picBoxProfile = New PictureBox()
      dtPickerBirthDate = New DateTimePicker()
      dtPickerDeathDate = New DateTimePicker()
      chkBoxDeathDate = New CheckBox()
      chkBoxBirthDate = New CheckBox()
      grpBoxConnections = New GroupBox()
      lstBoxConnections = New ListBox()
      btnRemoveConection = New Button()
      btnAddConection = New Button()
      cbPerson = New ComboBox()
      lblPersonID = New Label()
      cbRelation = New ComboBox()
      lblRelation = New Label()
      btnPrev = New Button()
      btnNext = New Button()
      grpBoxNotes.SuspendLayout()
      CType(picBoxProfile, ComponentModel.ISupportInitialize).BeginInit()
      grpBoxConnections.SuspendLayout()
      SuspendLayout()
      ' 
      ' btnOk
      ' 
      btnOk.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
      btnOk.Location = New Point(1090, 409)
      btnOk.Name = "btnOk"
      btnOk.Size = New Size(40, 23)
      btnOk.TabIndex = 0
      btnOk.Text = "O&k"
      btnOk.UseVisualStyleBackColor = True
      ' 
      ' lblID
      ' 
      lblID.AutoSize = True
      lblID.Location = New Point(12, 9)
      lblID.Name = "lblID"
      lblID.Size = New Size(21, 15)
      lblID.TabIndex = 1
      lblID.Text = "ID:"
      ' 
      ' txtBoxID
      ' 
      txtBoxID.Location = New Point(98, 6)
      txtBoxID.Name = "txtBoxID"
      txtBoxID.PlaceholderText = "ID"
      txtBoxID.ReadOnly = True
      txtBoxID.Size = New Size(121, 23)
      txtBoxID.TabIndex = 2
      ' 
      ' txtBoxFirstName
      ' 
      txtBoxFirstName.Location = New Point(98, 33)
      txtBoxFirstName.Name = "txtBoxFirstName"
      txtBoxFirstName.PlaceholderText = "First Name"
      txtBoxFirstName.Size = New Size(121, 23)
      txtBoxFirstName.TabIndex = 4
      ' 
      ' lblFirstName
      ' 
      lblFirstName.AutoSize = True
      lblFirstName.Location = New Point(12, 36)
      lblFirstName.Name = "lblFirstName"
      lblFirstName.Size = New Size(67, 15)
      lblFirstName.TabIndex = 3
      lblFirstName.Text = "First Name:"
      ' 
      ' txtBoxLastName
      ' 
      txtBoxLastName.Location = New Point(98, 60)
      txtBoxLastName.Name = "txtBoxLastName"
      txtBoxLastName.PlaceholderText = "Last Name"
      txtBoxLastName.Size = New Size(121, 23)
      txtBoxLastName.TabIndex = 6
      ' 
      ' lblLastName
      ' 
      lblLastName.AutoSize = True
      lblLastName.Location = New Point(12, 63)
      lblLastName.Name = "lblLastName"
      lblLastName.Size = New Size(66, 15)
      lblLastName.TabIndex = 5
      lblLastName.Text = "Last Name:"
      ' 
      ' lblSex
      ' 
      lblSex.AutoSize = True
      lblSex.Location = New Point(12, 90)
      lblSex.Name = "lblSex"
      lblSex.Size = New Size(27, 15)
      lblSex.TabIndex = 7
      lblSex.Text = "Sex:"
      ' 
      ' cbSex
      ' 
      cbSex.FormattingEnabled = True
      cbSex.Location = New Point(98, 87)
      cbSex.Name = "cbSex"
      cbSex.Size = New Size(121, 23)
      cbSex.TabIndex = 9
      ' 
      ' grpBoxNotes
      ' 
      grpBoxNotes.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Right
      grpBoxNotes.Controls.Add(txtBoxNotes)
      grpBoxNotes.Location = New Point(545, 6)
      grpBoxNotes.Name = "grpBoxNotes"
      grpBoxNotes.Size = New Size(279, 400)
      grpBoxNotes.TabIndex = 14
      grpBoxNotes.TabStop = False
      grpBoxNotes.Text = "Notes"
      ' 
      ' txtBoxNotes
      ' 
      txtBoxNotes.Dock = DockStyle.Fill
      txtBoxNotes.Location = New Point(3, 19)
      txtBoxNotes.Multiline = True
      txtBoxNotes.Name = "txtBoxNotes"
      txtBoxNotes.PlaceholderText = "Notes"
      txtBoxNotes.Size = New Size(273, 378)
      txtBoxNotes.TabIndex = 3
      ' 
      ' picBoxProfile
      ' 
      picBoxProfile.Anchor = AnchorStyles.Top Or AnchorStyles.Right
      picBoxProfile.Location = New Point(830, 6)
      picBoxProfile.Name = "picBoxProfile"
      picBoxProfile.Size = New Size(300, 400)
      picBoxProfile.TabIndex = 15
      picBoxProfile.TabStop = False
      ' 
      ' dtPickerBirthDate
      ' 
      dtPickerBirthDate.Format = DateTimePickerFormat.Short
      dtPickerBirthDate.Location = New Point(98, 112)
      dtPickerBirthDate.Name = "dtPickerBirthDate"
      dtPickerBirthDate.Size = New Size(121, 23)
      dtPickerBirthDate.TabIndex = 16
      ' 
      ' dtPickerDeathDate
      ' 
      dtPickerDeathDate.Format = DateTimePickerFormat.Short
      dtPickerDeathDate.Location = New Point(98, 139)
      dtPickerDeathDate.Name = "dtPickerDeathDate"
      dtPickerDeathDate.Size = New Size(121, 23)
      dtPickerDeathDate.TabIndex = 17
      ' 
      ' chkBoxDeathDate
      ' 
      chkBoxDeathDate.AutoSize = True
      chkBoxDeathDate.Location = New Point(12, 143)
      chkBoxDeathDate.Name = "chkBoxDeathDate"
      chkBoxDeathDate.Size = New Size(84, 19)
      chkBoxDeathDate.TabIndex = 18
      chkBoxDeathDate.Text = "Death Date"
      chkBoxDeathDate.UseVisualStyleBackColor = True
      ' 
      ' chkBoxBirthDate
      ' 
      chkBoxBirthDate.AutoSize = True
      chkBoxBirthDate.Location = New Point(12, 117)
      chkBoxBirthDate.Name = "chkBoxBirthDate"
      chkBoxBirthDate.Size = New Size(78, 19)
      chkBoxBirthDate.TabIndex = 19
      chkBoxBirthDate.Text = "Birth Date"
      chkBoxBirthDate.UseVisualStyleBackColor = True
      ' 
      ' grpBoxConnections
      ' 
      grpBoxConnections.Controls.Add(lstBoxConnections)
      grpBoxConnections.Controls.Add(btnRemoveConection)
      grpBoxConnections.Controls.Add(btnAddConection)
      grpBoxConnections.Controls.Add(cbPerson)
      grpBoxConnections.Controls.Add(lblPersonID)
      grpBoxConnections.Controls.Add(cbRelation)
      grpBoxConnections.Controls.Add(lblRelation)
      grpBoxConnections.Location = New Point(239, 6)
      grpBoxConnections.Name = "grpBoxConnections"
      grpBoxConnections.Size = New Size(300, 400)
      grpBoxConnections.TabIndex = 20
      grpBoxConnections.TabStop = False
      grpBoxConnections.Text = "Connections"
      ' 
      ' lstBoxConnections
      ' 
      lstBoxConnections.FormattingEnabled = True
      lstBoxConnections.Location = New Point(6, 113)
      lstBoxConnections.Name = "lstBoxConnections"
      lstBoxConnections.Size = New Size(288, 274)
      lstBoxConnections.TabIndex = 16
      ' 
      ' btnRemoveConection
      ' 
      btnRemoveConection.Location = New Point(35, 84)
      btnRemoveConection.Name = "btnRemoveConection"
      btnRemoveConection.Size = New Size(23, 23)
      btnRemoveConection.TabIndex = 15
      btnRemoveConection.Text = "-"
      btnRemoveConection.UseVisualStyleBackColor = True
      ' 
      ' btnAddConection
      ' 
      btnAddConection.Location = New Point(6, 84)
      btnAddConection.Name = "btnAddConection"
      btnAddConection.Size = New Size(23, 23)
      btnAddConection.TabIndex = 14
      btnAddConection.Text = "+"
      btnAddConection.UseVisualStyleBackColor = True
      ' 
      ' cbPerson
      ' 
      cbPerson.DropDownStyle = ComboBoxStyle.DropDownList
      cbPerson.FormattingEnabled = True
      cbPerson.Location = New Point(86, 51)
      cbPerson.Name = "cbPerson"
      cbPerson.Size = New Size(121, 23)
      cbPerson.TabIndex = 13
      ' 
      ' lblPersonID
      ' 
      lblPersonID.AutoSize = True
      lblPersonID.Location = New Point(6, 54)
      lblPersonID.Name = "lblPersonID"
      lblPersonID.Size = New Size(60, 15)
      lblPersonID.TabIndex = 12
      lblPersonID.Text = "Person ID:"
      ' 
      ' cbRelation
      ' 
      cbRelation.DropDownStyle = ComboBoxStyle.DropDownList
      cbRelation.FormattingEnabled = True
      cbRelation.Location = New Point(86, 22)
      cbRelation.Name = "cbRelation"
      cbRelation.Size = New Size(121, 23)
      cbRelation.TabIndex = 11
      ' 
      ' lblRelation
      ' 
      lblRelation.AutoSize = True
      lblRelation.Location = New Point(6, 25)
      lblRelation.Name = "lblRelation"
      lblRelation.Size = New Size(53, 15)
      lblRelation.TabIndex = 10
      lblRelation.Text = "Relation:"
      ' 
      ' btnPrev
      ' 
      btnPrev.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
      btnPrev.Location = New Point(12, 409)
      btnPrev.Name = "btnPrev"
      btnPrev.Size = New Size(23, 23)
      btnPrev.TabIndex = 21
      btnPrev.Text = "&<"
      btnPrev.UseVisualStyleBackColor = True
      ' 
      ' btnNext
      ' 
      btnNext.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
      btnNext.Location = New Point(41, 409)
      btnNext.Name = "btnNext"
      btnNext.Size = New Size(23, 23)
      btnNext.TabIndex = 22
      btnNext.Text = "&>"
      btnNext.UseVisualStyleBackColor = True
      ' 
      ' frmPerson
      ' 
      AutoScaleDimensions = New SizeF(7F, 15F)
      AutoScaleMode = AutoScaleMode.Font
      ClientSize = New Size(1135, 435)
      Controls.Add(btnNext)
      Controls.Add(btnPrev)
      Controls.Add(grpBoxConnections)
      Controls.Add(chkBoxBirthDate)
      Controls.Add(chkBoxDeathDate)
      Controls.Add(dtPickerDeathDate)
      Controls.Add(dtPickerBirthDate)
      Controls.Add(picBoxProfile)
      Controls.Add(grpBoxNotes)
      Controls.Add(cbSex)
      Controls.Add(lblSex)
      Controls.Add(txtBoxLastName)
      Controls.Add(lblLastName)
      Controls.Add(txtBoxFirstName)
      Controls.Add(lblFirstName)
      Controls.Add(txtBoxID)
      Controls.Add(lblID)
      Controls.Add(btnOk)
      Name = "frmPerson"
      Text = "Person"
      grpBoxNotes.ResumeLayout(False)
      grpBoxNotes.PerformLayout()
      CType(picBoxProfile, ComponentModel.ISupportInitialize).EndInit()
      grpBoxConnections.ResumeLayout(False)
      grpBoxConnections.PerformLayout()
      ResumeLayout(False)
      PerformLayout()
   End Sub

   Friend WithEvents btnOk As Button
   Friend WithEvents lblID As Label
   Friend WithEvents txtBoxID As TextBox
   Friend WithEvents txtBoxFirstName As TextBox
   Friend WithEvents lblFirstName As Label
   Friend WithEvents txtBoxLastName As TextBox
   Friend WithEvents lblLastName As Label
   Friend WithEvents lblSex As Label
   Friend WithEvents cbSex As ComboBox
   Friend WithEvents grpBoxNotes As GroupBox
   Friend WithEvents txtBoxNotes As TextBox
   Friend WithEvents picBoxProfile As PictureBox
   Friend WithEvents dtPickerBirthDate As DateTimePicker
   Friend WithEvents dtPickerDeathDate As DateTimePicker
   Friend WithEvents chkBoxDeathDate As CheckBox
   Friend WithEvents chkBoxBirthDate As CheckBox
   Friend WithEvents grpBoxConnections As GroupBox
   Friend WithEvents cbRelation As ComboBox
   Friend WithEvents lblRelation As Label
   Friend WithEvents cbPerson As ComboBox
   Friend WithEvents lblPersonID As Label
   Friend WithEvents btnPrev As Button
   Friend WithEvents btnNext As Button
   Friend WithEvents lstBoxConnections As ListBox
   Friend WithEvents btnRemoveConection As Button
   Friend WithEvents btnAddConection As Button
End Class
