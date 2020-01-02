using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CharacterAutoBackup
{
	/// <summary>
	/// Contains information about backup files for characters.
	/// </summary>
	/// <remarks>
	/// The formats for file names are:
	/// <list type="bullet">
	///   <listheader>
	///     <term>File Type</term>
	///     <description>Format</description>
	///   </listheader>
	///   <item>
	///     <term>Main character file</term>
	///     <description>gameXXXX.chr</description>
	///   </item>
	///   <item>
	///     <term>Main backup character file</term>
	///     <description>gameXXXX.chr.bakN</description>
	///   </item>
	///   <item>
	///     <term>Main character file</term>
	///     <description>archiveXXXX.arch</description>
	///   </item>
	///   <item>
	///     <term>Main character file</term>
	///     <description>archiveXXXX.arch.bakN</description>
	///   </item>
	/// </list>
	/// </remarks>
	public class CharacterInfo
	{
		#region Fields
		private string mainFile;
		private string name;
		private DateTime lastPlayed;
		private long size;
		private bool archived;
		private List<string> backups;
		private string preDeathBackup;
		private readonly FileSystemWatcher watcher;
		#endregion
		#region Properties
		/// <summary>
		/// Gets the full path to the file that contains the saved character data.
		/// </summary>
		public string FullPath => this.mainFile;
		/// <summary>
		/// Gets the short name of the file that contains the saved character data.
		/// </summary>
		public string FileName => Path.GetFileName(this.mainFile);
		/// <summary>
		/// Gets the name of the character.
		/// </summary>
		public string Name => this.name;
		/// <summary>
		/// Gets the last time the character was saved.
		/// </summary>
		public DateTime LastPlayed => this.lastPlayed;
		/// <summary>
		/// Gets the length of this file.
		/// </summary>
		public long Size => this.size;
		/// <summary>
		/// Gets the value that indicates whether this character isn't active in the game.
		/// </summary>
		public bool Archived => this.archived;
		/// <summary>
		/// Gets the read-only collection of full names of backups of this character file
		/// (excluding the pre-death backup).
		/// </summary>
		public ReadOnlyCollection<string> Backups => new ReadOnlyCollection<string>(this.backups);
		/// <summary>
		/// Gets the full path to the file that contains a special backup of the last time this
		/// character was saved before dying.
		/// </summary>
		public string PreDeathBackup => this.preDeathBackup;
		#endregion
		#region Events
		/// <summary>
		/// Occurs when changes occur to the files associated with this character.
		/// </summary>
		public event EventHandler Changed;
		#endregion
		#region Construction
		/// <summary>
		/// Creates a new object that represents saved data pertaining to a game character.
		/// </summary>
		/// <param name="file">Path to the character file.</param>
		public CharacterInfo(string file)
		{
			var fileInfo = new FileInfo(file);
			this.size = fileInfo.Length;
			this.mainFile = file;
			this.ReadCharacterName();
			this.lastPlayed = fileInfo.LastWriteTime;
			this.FindBackups();
			this.archived = fileInfo.Extension == CharacterBackups.ArchivedFileExtension;
			if (!this.archived && this.backups.Count == 0 || this.BackupsNeedUpdate())
			{
				// There should always be at least one fresh backup.
				this.Backup(this, new FileSystemEventArgs(WatcherChangeTypes.Changed,
														  CharacterBackups.GamePath,
														  fileInfo.Name));
			}

			this.watcher = new FileSystemWatcher
			{
				Path = CharacterBackups.GamePath,
				Filter = fileInfo.Name,
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
				IncludeSubdirectories = false
			};
			this.watcher.Changed += this.Backup;
			this.watcher.Deleted += this.Archive;

			this.watcher.EnableRaisingEvents = true;
		}
		#endregion
		#region Interface
		/// <summary>
		/// Restores an archived character into the active one.
		/// </summary>
		public void Unpack()
		{
			var nameNoExt = FindViableName("game", CharacterBackups.CharacterFileExtension);

			this.watcher.EnableRaisingEvents = false;
			CharacterBackups.PauseMonitoring();

			this.RenameFiles(nameNoExt, CharacterBackups.CharacterFileExtension,
							 CharacterBackups.ArchivedFileExtension);

			this.watcher.EnableRaisingEvents = true;
			CharacterBackups.ResumeMonitoring();

			this.archived = false;
			this.OnChanged();
		}
		/// <summary>
		/// Restores this character from a pre-death backup.
		/// </summary>
		public void Resurrect()
		{
			if (this.preDeathBackup == null)
			{
				return;
			}

			if (!File.Exists(this.preDeathBackup))
			{
				this.preDeathBackup = null;
				return;
			}

			this.watcher.EnableRaisingEvents = false;
			CharacterBackups.PauseMonitoring();

			if (File.Exists(this.mainFile))
			{
				File.Delete(this.mainFile);
			}
			File.Move(this.preDeathBackup, this.mainFile);
			this.preDeathBackup = null;

			this.watcher.EnableRaisingEvents = true;
			CharacterBackups.ResumeMonitoring();

			this.Backup(this, new FileSystemEventArgs(WatcherChangeTypes.Created,
													  CharacterBackups.GamePath, this.FileName));
		}
		/// <summary>
		/// Restores the character from a specified backup.
		/// </summary>
		/// <param name="backupIndex">Zero-based index of the backup to choose for restoration.</param>
		public void Restore(int backupIndex)
		{
			this.watcher.EnableRaisingEvents = false;
			CharacterBackups.PauseMonitoring();

			// Check just in case.
			if (File.Exists(this.mainFile))
			{
				File.Delete(this.mainFile);
			}
			File.Move(this.backups[backupIndex], this.mainFile);

			this.watcher.EnableRaisingEvents = true;
			CharacterBackups.ResumeMonitoring();

			this.Backup(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, CharacterBackups.GamePath,
													  Path.GetFileName(this.mainFile)));
		}
		#endregion
		#region Utilities
		protected virtual void OnChanged()
		{
			this.Changed?.Invoke(this, EventArgs.Empty);
		}
		private void ReadCharacterName()
		{
			FileStream fs = new FileStream(this.mainFile, FileMode.Open, FileAccess.Read, FileShare.Read);

			var encoding = Encoding.GetEncoding(866);

			using (var reader = new BinaryReader(fs, encoding))
			{
				// The name of the character is a null-terminated string that begins at offset 0C or 12.
				const int charNameOffset = 12;
				reader.BaseStream.Seek(charNameOffset, SeekOrigin.Begin);
				int charCount = 0;
				while (reader.PeekChar() != 0)
				{
					charCount++;
					reader.ReadSByte();
				}

				reader.BaseStream.Seek(charNameOffset, SeekOrigin.Begin);
				var charNameBytes = reader.ReadBytes(charCount);
				this.name = encoding.GetString(charNameBytes);
			}
		}
		private void FindBackups()
		{
			string name = Path.GetFileName(this.mainFile);

			this.backups = new List<string>(Directory.EnumerateFiles(CharacterBackups.BackupsPath,
																	 $"{name}.bak?",
																	 SearchOption.TopDirectoryOnly)
													 .Where(x=>x[x.Length - 1] != '6'));

			string preDeathBackupName = Path.Combine(CharacterBackups.BackupsPath, $"{name}.bak6");
			if (File.Exists(preDeathBackupName))
			{
				this.preDeathBackup = preDeathBackupName;
			}
		}
		private bool BackupsNeedUpdate()
		{
			FileInfo mainFileInfo = new FileInfo(this.mainFile);
			FileInfo firstBackupFileInfo = new FileInfo(this.backups[0]);

			return mainFileInfo.Length != firstBackupFileInfo.Length ||
				   mainFileInfo.LastWriteTime > firstBackupFileInfo.LastWriteTime;
		}
		private void Backup(object sender, FileSystemEventArgs e)
		{
			if (!File.Exists(this.mainFile))
			{
				return;
			}

			var fileName = this.FileName;

			// Find, if there is a hole in a sequence of backup numbers.
			int i = 1;
			for (; i < 5; i++)
			{
				var cb = Path.Combine(CharacterBackups.BackupsPath, $"{fileName}.bak{i}");
				if (!File.Exists(cb))
				{
					break;
				}
			}

			// Shift all backups until the hole to make bak1 extension available.
			for (int j = i - 1; j > 0; j--)
			{
				var currentBackup = Path.Combine(CharacterBackups.BackupsPath, $"{fileName}.bak{j}");
				var nextBackup = Path.Combine(CharacterBackups.BackupsPath, $"{fileName}.bak{j + 1}");
				if (File.Exists(nextBackup))
				{
					File.Delete(nextBackup);
				}
				File.Move(currentBackup, nextBackup);
			}

			FileInfo mainFileInfo = new FileInfo(this.mainFile);
			string backupRaw = Path.Combine(CharacterBackups.BackupsPath, mainFileInfo.Name);
			while (true)
			{
				try
				{
					File.Copy(this.mainFile, $"{backupRaw}.bak1");
					break;
				}
				catch (IOException)
				{
					// Try again.
					Thread.Sleep(20);
				}
			}

			mainFileInfo.Refresh();
			this.lastPlayed = mainFileInfo.LastWriteTime;
			this.size = mainFileInfo.Length;

			this.FindBackups();

			// Check to see, if this character has died.
			if (this.backups.Count > 1)
			{
				var firstBackupInfo = new FileInfo(this.backups[0]);
				var secondBackupInfo = new FileInfo(this.backups[1]);

				if (firstBackupInfo.Length < 1000 && secondBackupInfo.Length > 1000)
				{
					this.preDeathBackup = Path.Combine(CharacterBackups.BackupsPath,
													   $"{mainFileInfo.Name}.bak6");
					if (File.Exists(this.preDeathBackup))
					{
						File.Delete(this.preDeathBackup);
					}

					File.Copy(this.backups[1], this.preDeathBackup);
				}
			}

			this.OnChanged();
		}
		private void Archive(object sender, FileSystemEventArgs e)
		{
			// The main file has been deleted, so let's copy the latest backup to be an archived file.
			var nameNoExt = FindViableName("archive", CharacterBackups.ArchivedFileExtension);

			this.watcher.EnableRaisingEvents = false;
			File.Copy(this.backups.First(), this.mainFile);
			this.watcher.EnableRaisingEvents = true;

			this.RenameFiles(nameNoExt, CharacterBackups.ArchivedFileExtension,
							 CharacterBackups.CharacterFileExtension);

			this.archived = true;
			this.OnChanged();
		}
		private void RenameFiles(string fileNameNoExtension, string extensionNew, string extensionOld)
		{
			string newName = Path.Combine(CharacterBackups.GamePath, $"{fileNameNoExtension}{extensionNew}");
			// Rename the main file.
			this.watcher.EnableRaisingEvents = false;

			File.Move(this.mainFile, newName);
			this.watcher.Filter = $"{fileNameNoExtension}{extensionNew}";

			this.watcher.EnableRaisingEvents = true;

			// Rename the backups.
			string oldName = Path.GetFileNameWithoutExtension(this.mainFile);

			for (int j = 0; j < this.backups.Count; j++)
			{
				string backup = this.backups[j];
				this.backups[j] = backup.Replace(oldName, fileNameNoExtension)
										.Replace(extensionOld, extensionNew);

				File.Move(backup, this.backups[j]);
			}
			if (this.preDeathBackup != null)
			{
				string b = this.preDeathBackup;
				this.preDeathBackup = b.Replace(oldName, fileNameNoExtension)
									   .Replace(extensionOld, extensionNew);
				File.Move(b, this.preDeathBackup);
			}

			this.mainFile = newName;
		}
		private static string FindViableName(string prefix, string fileExtension)
		{
			for (int i = 0; i < 10000; i++)
			{
				string extendedPrefix = $"{prefix}{i:D4}";
				string currentName = $"{extendedPrefix}{fileExtension}";
				if (!Directory.EnumerateFiles(CharacterBackups.GamePath, currentName,
											  SearchOption.TopDirectoryOnly)
							  .Concat(Directory.EnumerateFiles(CharacterBackups.BackupsPath,
															   $"{extendedPrefix}.*",
															   SearchOption.TopDirectoryOnly)).Any())
				{
					return extendedPrefix;
				}
			}

			return null;
		}
		#endregion
	}
	/// <summary>
	/// Defines functionality for management of backups of files that contain saved character information.
	/// </summary>
	public static class CharacterBackups
	{
		#region Fields
		/// <summary>
		/// The extension that is used to identify a file that contains saved data pertaining to a game
		/// character.
		/// </summary>
		/// <remarks>
		/// This extension is presented with a leading dot.
		/// </remarks>
		public const string CharacterFileExtension = ".chr";
		/// <summary>
		/// The extension that is used to identify a file that contains saved data pertaining to a game
		/// character that was deleted.
		/// </summary>
		/// <remarks>
		/// This extension is presented with a leading dot.
		/// </remarks>
		public const string ArchivedFileExtension = ".arch";
		/// <summary>
		/// Full path to the directory that contains the game files.
		/// </summary>
		public static readonly string GamePath = Directory.GetCurrentDirectory();
		/// <summary>
		/// Full path to the directory that contains all backup files.
		/// </summary>
		public static readonly string BackupsPath = Path.Combine(GamePath, "backups");
		
		// This object watches for changes in the character files.
		private static readonly FileSystemWatcher watcher =
			new FileSystemWatcher(GamePath, $"*.{CharacterFileExtension}");
		// The list of objects that contain information about the characters and their backups.
		private static readonly SortedList<string, CharacterInfo> characters =
			new SortedList<string, CharacterInfo>();

		/// <summary>
		/// A collection of character files that monitored by this application.
		/// </summary>
		public static readonly IReadOnlyCollection<CharacterInfo> Characters =
			new ReadOnlyCollection<CharacterInfo>(characters.Values);
		#endregion
		#region Events
		/// <summary>
		/// Occurs whenever there are changes made to the list of characters.
		/// </summary>
		public static event EventHandler CharacterListChanged;
		#endregion
		#region Construction
		static CharacterBackups()
		{
			watcher.Created += AddNewCharacter;
		}
		#endregion
		#region Interface
		/// <summary>
		/// Initiates all operations related to monitoring and backing up of all character files.
		/// </summary>
		public static void CommenceMonitoring()
		{
			var mainCharacterFiles = Directory.EnumerateFiles(GamePath, $"*{CharacterFileExtension}",
															  SearchOption.TopDirectoryOnly);
			var archivedFiles = Directory.EnumerateFiles(GamePath, $"*{ArchivedFileExtension}",
														 SearchOption.TopDirectoryOnly);
			foreach (var file in mainCharacterFiles.Concat(archivedFiles))
			{
				var charInfo = new CharacterInfo(file);
				characters.Add(Path.GetFileName(file) ?? "", charInfo);
			}

			if (characters.Count > 0)
			{
				OnCharacterListChanged();
			}

			watcher.EnableRaisingEvents = true;
		}
		public static void PauseMonitoring()
		{
			watcher.EnableRaisingEvents = false;
		}
		public static void ResumeMonitoring()
		{
			watcher.EnableRaisingEvents = true;
		}
		#endregion
		#region Utilities
		private static void AddNewCharacter(object sender, FileSystemEventArgs e)
		{
			characters.Add(e.Name, new CharacterInfo(e.FullPath));
			OnCharacterListChanged();
		}
		private static void OnCharacterListChanged()
		{
			CharacterListChanged?.Invoke(null, EventArgs.Empty);
		}
		#endregion
	}
}
