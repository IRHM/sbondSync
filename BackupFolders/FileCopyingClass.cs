using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BackupFolders
{
    public class FileCopyingClass
    {
        public static async void StartCopying(ListBox SelectedFilesListBox, ProgressBar ProgressBar)
        {
            int FileCount = 0;

            // Add Warning - Files Will Be Overwritten!

            string[] FilePaths = new string[SelectedFilesListBox.Items.Count];
            FilePaths = SelectedFilesListBox.Items.OfType<string>().ToArray();

            MainWindow.IsDefaultBackupDirAssigned();

            // Sum of how many files are going to be copied
            foreach (string s in FilePaths)
            {
                FileAttributes attr = File.GetAttributes(s);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    FileCount = Directory.GetFiles(s, "*", SearchOption.AllDirectories).Length; // FileCount = How many files in directories
                }
                else
                {
                    FileCount++; // FileCount = How many files are on their own
                }
                // Now, FileCount = How many files are in directories + on their own
            }
            ProgressBar.Maximum = FileCount;

            // Organise file copying
            foreach (string s in FilePaths)
            {
                FileAttributes attr = File.GetAttributes(s);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    // Is Directory...
                    string SourceDir = s; // Directory for source files
                    string SourceDirFolderName = new DirectoryInfo(s).Name; // Folder source files are in
                    MainWindow.BackupDir = $@"{Properties.Settings.Default.DefaultSaveDir}\{SourceDirFolderName}"; //Properties.Settings.Default.DefaultSaveDir
                    bool CopySubDirs = true;

                    await FileCopyingClass.DirFileBackup(ProgressBar, SourceDir, MainWindow.BackupDir, CopySubDirs);
                }
                else
                {
                    // Is File...
                    string SourceFileDir = s; // Directory for source file
                    string SourceFileName = Path.GetFileName(s); // Name of source file
                    string SourceFileFolderName = Path.GetFileName(Path.GetDirectoryName(SourceFileDir)); // Folder source files are in
                    MainWindow.BackupDir = $@"{Properties.Settings.Default.DefaultSaveDir}\{SourceFileFolderName}\{SourceFileName}";

                    string DirToCreate = MainWindow.BackupDir.Replace($@"\{SourceFileName}", ""); // Remove filename from path
                    Directory.CreateDirectory(DirToCreate); // Create directory, if needed

                    await FileCopyingClass.FileBackup(ProgressBar, SourceFileDir, MainWindow.BackupDir); // Run method for file copying
                }
            }
        }

        public static async Task FileBackup(ProgressBar ProgBar, string SourceFileDir, string BackupDir)
        {
            await Task.Run(() => File.Copy(SourceFileDir, BackupDir, true));
            ProgBar.Value++; // Add 1 to progressbar value once every file copies (when on its own)
        }

        public static async Task DirFileBackup(ProgressBar ProgBar, string SourceDir, string BackupDir, bool CopySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(SourceDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    $"{SourceDir} - Could not be found.");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(BackupDir))
            {
                Directory.CreateDirectory(BackupDir);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string TempPath = Path.Combine(BackupDir, file.Name);
                await Task.Run(() => file.CopyTo(TempPath, true));
                File.SetAttributes(TempPath, FileAttributes.Normal);
                ProgBar.Value++; // Add 1 to progressbar value once every file copies (when in dir)
            }

            if (CopySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(BackupDir, subdir.Name);
                    await DirFileBackup(ProgBar, subdir.FullName, temppath, CopySubDirs);
                }
            }
        }
    }
}
