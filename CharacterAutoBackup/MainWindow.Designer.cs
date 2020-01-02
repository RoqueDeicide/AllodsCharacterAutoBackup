namespace CharacterAutoBackup
{
	partial class MainWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.RestoreButton = new System.Windows.Forms.Button();
			this.CharactersListView = new System.Windows.Forms.ListView();
			this.CharacterFileColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.CharacterNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.LastPlayedColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label2 = new System.Windows.Forms.Label();
			this.BackupsListView = new System.Windows.Forms.ListView();
			this.BackupFileNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.BackupFileSizeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.BackupFileDateColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ResurrectButton = new System.Windows.Forms.Button();
			this.UnarchiveButton = new System.Windows.Forms.Button();
			this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
			this.CharacterFileSizeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// RestoreButton
			// 
			this.RestoreButton.Enabled = false;
			this.RestoreButton.Location = new System.Drawing.Point(315, 313);
			this.RestoreButton.Name = "RestoreButton";
			this.RestoreButton.Size = new System.Drawing.Size(70, 23);
			this.RestoreButton.TabIndex = 5;
			this.RestoreButton.Text = "Restore";
			this.ToolTip.SetToolTip(this.RestoreButton, "Restore the character from a selected buckup file.");
			this.RestoreButton.UseVisualStyleBackColor = true;
			this.RestoreButton.Click += new System.EventHandler(this.RestoreFromBackup);
			// 
			// CharactersListView
			// 
			this.CharactersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.CharacterFileColumnHeader,
            this.CharacterNameColumnHeader,
            this.CharacterFileSizeColumnHeader,
            this.LastPlayedColumnHeader});
			this.CharactersListView.FullRowSelect = true;
			this.CharactersListView.GridLines = true;
			this.CharactersListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.CharactersListView.Location = new System.Drawing.Point(12, 12);
			this.CharactersListView.MultiSelect = false;
			this.CharactersListView.Name = "CharactersListView";
			this.CharactersListView.Size = new System.Drawing.Size(297, 324);
			this.CharactersListView.TabIndex = 0;
			this.CharactersListView.UseCompatibleStateImageBehavior = false;
			this.CharactersListView.View = System.Windows.Forms.View.Details;
			this.CharactersListView.SelectedIndexChanged += new System.EventHandler(this.ChangeSelection);
			// 
			// CharacterFileColumnHeader
			// 
			this.CharacterFileColumnHeader.Text = "File";
			this.CharacterFileColumnHeader.Width = 76;
			// 
			// CharacterNameColumnHeader
			// 
			this.CharacterNameColumnHeader.Text = "Name";
			// 
			// LastPlayedColumnHeader
			// 
			this.LastPlayedColumnHeader.Text = "Last Played";
			this.LastPlayedColumnHeader.Width = 100;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(315, 12);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(49, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Backups";
			// 
			// BackupsListView
			// 
			this.BackupsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.BackupFileNameColumnHeader,
            this.BackupFileSizeColumnHeader,
            this.BackupFileDateColumnHeader});
			this.BackupsListView.Location = new System.Drawing.Point(315, 29);
			this.BackupsListView.MultiSelect = false;
			this.BackupsListView.Name = "BackupsListView";
			this.BackupsListView.Size = new System.Drawing.Size(300, 278);
			this.BackupsListView.TabIndex = 6;
			this.BackupsListView.UseCompatibleStateImageBehavior = false;
			this.BackupsListView.View = System.Windows.Forms.View.Details;
			this.BackupsListView.SelectedIndexChanged += new System.EventHandler(this.ChangeBackupSelection);
			// 
			// BackupFileNameColumnHeader
			// 
			this.BackupFileNameColumnHeader.Text = "Name";
			this.BackupFileNameColumnHeader.Width = 120;
			// 
			// BackupFileSizeColumnHeader
			// 
			this.BackupFileSizeColumnHeader.Text = "Size";
			this.BackupFileSizeColumnHeader.Width = 50;
			// 
			// BackupFileDateColumnHeader
			// 
			this.BackupFileDateColumnHeader.Text = "Date";
			this.BackupFileDateColumnHeader.Width = 120;
			// 
			// ResurrectButton
			// 
			this.ResurrectButton.Enabled = false;
			this.ResurrectButton.Location = new System.Drawing.Point(391, 313);
			this.ResurrectButton.Name = "ResurrectButton";
			this.ResurrectButton.Size = new System.Drawing.Size(70, 23);
			this.ResurrectButton.TabIndex = 7;
			this.ResurrectButton.Text = "Resurrect";
			this.ToolTip.SetToolTip(this.ResurrectButton, "Restore the character from a special buckup file that was made before character\'s" +
        " death.");
			this.ResurrectButton.UseVisualStyleBackColor = true;
			this.ResurrectButton.Click += new System.EventHandler(this.RestorePreDeathBackup);
			// 
			// UnarchiveButton
			// 
			this.UnarchiveButton.Enabled = false;
			this.UnarchiveButton.Location = new System.Drawing.Point(467, 313);
			this.UnarchiveButton.Name = "UnarchiveButton";
			this.UnarchiveButton.Size = new System.Drawing.Size(70, 23);
			this.UnarchiveButton.TabIndex = 8;
			this.UnarchiveButton.Text = "Unarchive";
			this.ToolTip.SetToolTip(this.UnarchiveButton, "Restore the character if it was previously deleted.");
			this.UnarchiveButton.UseVisualStyleBackColor = true;
			this.UnarchiveButton.Click += new System.EventHandler(this.UnarchiveCharacter);
			// 
			// RefreshTimer
			// 
			this.RefreshTimer.Enabled = true;
			this.RefreshTimer.Interval = 5000;
			this.RefreshTimer.Tick += new System.EventHandler(this.TriggerRefresh);
			// 
			// CharacterFileSizeColumnHeader
			// 
			this.CharacterFileSizeColumnHeader.Text = "Size";
			this.CharacterFileSizeColumnHeader.Width = 50;
			// 
			// MainWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.ClientSize = new System.Drawing.Size(620, 350);
			this.Controls.Add(this.UnarchiveButton);
			this.Controls.Add(this.ResurrectButton);
			this.Controls.Add(this.BackupsListView);
			this.Controls.Add(this.RestoreButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.CharactersListView);
			this.Name = "MainWindow";
			this.Text = "Auto Backup";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView CharactersListView;
		private System.Windows.Forms.ColumnHeader CharacterFileColumnHeader;
		private System.Windows.Forms.ColumnHeader CharacterNameColumnHeader;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListView BackupsListView;
		private System.Windows.Forms.ColumnHeader BackupFileNameColumnHeader;
		private System.Windows.Forms.ColumnHeader BackupFileSizeColumnHeader;
		private System.Windows.Forms.ColumnHeader BackupFileDateColumnHeader;
		private System.Windows.Forms.ColumnHeader LastPlayedColumnHeader;
		private System.Windows.Forms.Button ResurrectButton;
		private System.Windows.Forms.Button UnarchiveButton;
		private System.Windows.Forms.Button RestoreButton;
		private System.Windows.Forms.ToolTip ToolTip;
		private System.Windows.Forms.Timer RefreshTimer;
		private System.Windows.Forms.ColumnHeader CharacterFileSizeColumnHeader;
	}
}

