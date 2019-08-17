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
