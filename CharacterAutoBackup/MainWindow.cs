using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CharacterAutoBackup
{
	/// <inheritdoc />
	/// <summary>
	/// Represents the main window of the application.
	/// </summary>
	/// <remarks>
	/// This window currently handles all of the functionality.
	/// </remarks>
	public partial class MainWindow : Form
	{
		#region Fields
		private ListViewItem selectedCharacter;
		#endregion
		#region Properties

		#endregion
		#region Events

		#endregion
		#region Construction
		/// <inheritdoc />
		/// <summary>
		/// Creates a new window.
		/// </summary>
		public MainWindow()
		{
			this.InitializeComponent();

			CharacterBackups.CharacterListChanged += this.CharacterBackupsOnCharacterListChanged;
			
			CharacterBackups.CommenceMonitoring();
		}
		#endregion
		#region Interface

		#endregion
		#region Utilities
		private void RefreshCharacterList()
		{
			string selectedFile = null;
			if (this.CharactersListView.SelectedItems.Count > 0)
			{
				selectedFile = this.CharactersListView.SelectedItems[0].Text;
			}

			this.CharactersListView.BeginUpdate();

			this.CharactersListView.Items.Clear();

			foreach (var character in CharacterBackups.Characters)
			{
				this.CharactersListView.Items.Add(new ListViewItem
				{
					Text = character.FileName,
					SubItems =
					{
						character.Name, character.Size.ToString(),
						character.LastPlayed.ToString("g")
					},
					Tag = character
				});
			}

			// Reselect a character that was selected before this.
			var listViewItems = this.CharactersListView.Items.Cast<ListViewItem>();
			var previouslySelected = listViewItems.FirstOrDefault(x => x.Text == selectedFile);
			if (previouslySelected != null)
			{
				previouslySelected.Selected = true;
			}

			this.CharactersListView.EndUpdate();
		}
		private void CharacterBackupsOnCharacterListChanged(object sender, EventArgs e)
		{
			if (this.CharactersListView.InvokeRequired)
			{
				this.Invoke((Action)this.RefreshCharacterList);
			}
			else
			{
				this.RefreshCharacterList();
			}
		}

		private void UpdateBackupList()
		{
			this.BackupsListView.Items.Clear();
			this.ResurrectButton.Enabled = false;
			if (this.CharactersListView.SelectedIndices.Count == 0)
			{
				return;
			}

			var character = (CharacterInfo)this.CharactersListView.SelectedItems[0].Tag;

			this.BackupsListView.BeginUpdate();

			// Display the backups.
			foreach (var backup in character.Backups)
			{
				var info = new FileInfo(backup);
				if (info.Exists)
				{
					this.BackupsListView.Items.Add(new ListViewItem
					{
						Text = info.Name,
						SubItems = {info.Length.ToString(), info.LastWriteTime.ToString("g")},
						Tag = backup
					});
				}
			}

			this.BackupsListView.EndUpdate();
			
			// Check existence of pre-death backup file.
			this.ResurrectButton.Enabled = character.PreDeathBackup != null;
			this.UnarchiveButton.Enabled = character.Archived;
		}

		private void ChangeSelection(object sender, EventArgs e)
		{
			if (this.CharactersListView.SelectedItems.Count == 0)
			{
				if (this.selectedCharacter != null)
				{
					((CharacterInfo)this.selectedCharacter.Tag).Changed -= this.CharacterChanged;
				}
			}
			else
			{
				if (this.selectedCharacter == null ||
					this.selectedCharacter.Text != this.CharactersListView.SelectedItems[0].Text)
				{
					if (this.selectedCharacter != null)
					{
						((CharacterInfo)this.selectedCharacter.Tag).Changed -= this.CharacterChanged;
					}
					this.selectedCharacter = this.CharactersListView.SelectedItems[0];
					((CharacterInfo)this.selectedCharacter.Tag).Changed += this.CharacterChanged;
				}
			}
			
			this.UpdateBackupList();
		}
		private void CharacterChanged(object sender, EventArgs e)
		{
			if (this.BackupsListView.InvokeRequired)
			{
				this.Invoke((Action)this.UpdateBackupList);
			}
			else
			{
				this.UpdateBackupList();
			}
		}

		private void RestoreFromBackup(object sender, EventArgs e)
		{
			var character = (CharacterInfo)this.CharactersListView.SelectedItems[0].Tag;

			character.Restore(this.BackupsListView.SelectedIndices[0]);
		}
		private void RestorePreDeathBackup(object sender, EventArgs e)
		{
			var character = (CharacterInfo)this.CharactersListView.SelectedItems[0].Tag;

			character.Resurrect();

			this.UpdateBackupList();
		}

		private void ChangeBackupSelection(object sender, EventArgs e)
		{
			var selectedItem = this.BackupsListView.SelectedItems.Cast<ListViewItem>().FirstOrDefault();

			this.RestoreButton.Enabled = selectedItem != null;
		}

		private void TriggerRefresh(object sender, EventArgs e)
		{
			this.RefreshCharacterList();
		}

		private void UnarchiveCharacter(object sender, EventArgs e)
		{
			var character = (CharacterInfo)this.CharactersListView.SelectedItems[0].Tag;

			character.Unpack();

			this.RefreshCharacterList();
		}
		#endregion
	}
}
