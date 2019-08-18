using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BackupFolders
{
    public class FileCopyingClass
    {
        static string SourceFileName;

        // Colours
        static byte ErrorA = 255;
        static byte ErrorR = 225;
        static byte ErrorG = 0;
        static byte ErrorB = 0;

        static byte SuccessA = 255;
        static byte SuccessR = 98;
        static byte SuccessG = 255;
        static byte SuccessB = 127;

        // Errors
        static string FileNotFoundExceptionError = "File Not Found";
        static string UnauthorizedAccessExceptionError = "Insufficient Permissions";
        static string ArgumentExceptionError = "File Has An Invalid Name";
        static string PathTooLongExceptionError = "File Path Exceed Maximum Length";
        static string DirectoryNotFoundExceptionError = "Path Does Not Exist";
        static string NotSupportedExceptionError = "File Is In Invalid Format";

        public static async void StartCopying(ListBox SelectedFilesListBox, ProgressBar ProgressBar, TextBlock ProgressBarTextBlock)
        {
            int FileCount = 0;

            // Add Warning - Files Will Be Overwritten!

            string[] FilePaths = new string[SelectedFilesListBox.Items.Count];
            FilePaths = SelectedFilesListBox.Items.OfType<string>().ToArray();

            try
            {


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
                ProgressBar.Visibility = Visibility.Visible;
                // Organise file copying
                foreach (string s in FilePaths)
                {
                    SourceFileName = Path.GetFileName(s); // Name of source file

                    FileAttributes attr = File.GetAttributes(s);

                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        // Is Directory...
                        string SourceDir = s; // Directory for source files
                        string SourceDirFolderName = new DirectoryInfo(s).Name; // Folder source files are in
                        MainWindow.BackupDir = $@"{Properties.Settings.Default.DefaultSaveDir}\{SourceDirFolderName}";
                        bool CopySubDirs = true;

                        await FileCopyingClass.DirFileBackup(ProgressBar, ProgressBarTextBlock, SourceDir, MainWindow.BackupDir, CopySubDirs);
                    }
                    else
                    {
                        // Is File...
                        string SourceFileDir = s; // Directory for source file
                        
                        string SourceFileFolderName = Path.GetFileName(Path.GetDirectoryName(SourceFileDir)); // Folder source files are in
                        MainWindow.BackupDir = $@"{Properties.Settings.Default.DefaultSaveDir}\{SourceFileFolderName}\{SourceFileName}";

                        string DirToCreate = MainWindow.BackupDir.Replace($@"\{SourceFileName}", ""); // Remove filename from path of BackupDir
                        Directory.CreateDirectory(DirToCreate); // Create directory, if needed

                        await FileCopyingClass.FileBackup(ProgressBar, ProgressBarTextBlock, SourceFileDir, MainWindow.BackupDir); // Run method for file copying
                    }
                }

                if (ProgressBar.Value == ProgressBar.Maximum)
                {
                    FileCopyStatus(ProgressBar, ProgressBarTextBlock, $"Done Copying All Files", SuccessA, SuccessR, SuccessG, SuccessB);
                }
                else { }
            }
            catch (UnauthorizedAccessException)
            {
                FileCopyNotice(ProgressBar, ProgressBarTextBlock, UnauthorizedAccessExceptionError,
                                ErrorA, ErrorR, ErrorG, ErrorB);
            }
            catch (ArgumentException)
            {
                FileCopyNotice(ProgressBar, ProgressBarTextBlock, ArgumentExceptionError,
                                ErrorA, ErrorR, ErrorG, ErrorB);
            }
            catch (PathTooLongException)
            {
                FileCopyNotice(ProgressBar, ProgressBarTextBlock, PathTooLongExceptionError,
                                ErrorA, ErrorR, ErrorG, ErrorB);
            }
            catch (DirectoryNotFoundException)
            {
                FileCopyNotice(ProgressBar, ProgressBarTextBlock, DirectoryNotFoundExceptionError,
                                ErrorA, ErrorR, ErrorG, ErrorB);
            }
            catch (FileNotFoundException)
            {
                FileCopyNotice(ProgressBar, ProgressBarTextBlock, FileNotFoundExceptionError,
                                ErrorA, ErrorR, ErrorG, ErrorB);
            }
            catch (NotSupportedException)
            {
                FileCopyNotice(ProgressBar, ProgressBarTextBlock, NotSupportedExceptionError,
                                ErrorA, ErrorR, ErrorG, ErrorB);
            }
        }

        public static async Task FileBackup(ProgressBar ProgressBar, TextBlock ProgressBarTextBlock, string SourceFileDir, string BackupDir)
        {
            FileCopyStatus(ProgressBar, ProgressBarTextBlock, $"Copying {SourceFileName}", SuccessA, SuccessR, SuccessG, SuccessB);
            await Task.Run(() => File.Copy(SourceFileDir, BackupDir, true));
            ProgressBar.Value++; // Add 1 to progressbar value once every file copies (when on its own)
        }

        public static async Task DirFileBackup(ProgressBar ProgressBar, TextBlock ProgressBarTextBlock, 
                                                string SourceDir, string BackupDir, bool CopySubDirs)
        {
            DirectoryInfo Dir = new DirectoryInfo(SourceDir);

            if (!Dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    $"{SourceDir} - Could not be found.");
            }

            DirectoryInfo[] Dirs = Dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(BackupDir))
            {
                Directory.CreateDirectory(BackupDir);
            }

            FileInfo[] Files = Dir.GetFiles();
            foreach (FileInfo f in Files)
            {
                FileCopyStatus(ProgressBar, ProgressBarTextBlock, $"Copying {f.Name}", SuccessA, SuccessR, SuccessG, SuccessB);
                string TempPath = Path.Combine(BackupDir, f.Name);
                await Task.Run(() => f.CopyTo(TempPath, true));
                File.SetAttributes(TempPath, FileAttributes.Normal);
                ProgressBar.Value++; // Add 1 to progressbar value once every file copies (when in dir)
            }

            if (CopySubDirs)
            {
                foreach (DirectoryInfo subdir in Dirs)
                {
                    string temppath = Path.Combine(BackupDir, subdir.Name);
                    await DirFileBackup(ProgressBar, ProgressBarTextBlock, subdir.FullName, temppath, CopySubDirs);
                }
            }
        }

        public static void FileCopyNotice(ProgressBar ProgressBar, TextBlock ProgressBarTextBlock, string Error, byte A, byte R, byte G, byte B)
        {
            ProgressBar.Visibility = Visibility.Visible; // Make ProgressBar Visible
            ProgressBar.Value = ProgressBar.Maximum; // Set ProgressBar Value To Max
            ProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(A, R, G, B)); // Set ProgressBar Foreground Colour
            ProgressBarTextBlock.Text = Error; // Add text to ProgressBarTextBlock
        }

        public static void FileCopyStatus(ProgressBar ProgressBar, TextBlock ProgressBarTextBlock, string Status, byte A, byte R, byte G, byte B)
        {
            ProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(A, R, G, B)); // Set ProgressBar Foreground Colour
            ProgressBarTextBlock.Text = Status;
        }
    }
}
