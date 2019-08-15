﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using WPFCustomMessageBox;

namespace BackupFolders
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string SelectedFileDir;
        string[] files;
        string[] IsFile;
        string[] IsFolder;
        string BackupDir;

        // Colors
        byte ErrorA = 255;
        byte ErrorR = 125;
        byte ErrorG = 39;
        byte ErrorB = 39;

        byte SuccessA = 255;
        byte SuccessR = 95;
        byte SuccessG = 186;
        byte SuccessB = 125;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BackupWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void FileCheckBox_Click(object sender, RoutedEventArgs e)
        {
            FolderCheckBox.IsChecked = false;
        }

        private void FolderCheckBox_Click(object sender, RoutedEventArgs e)
        {
            FileCheckBox.IsChecked = false;
        }

        private void NoticeTextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NoticeTextBlock.Text = "";
            NoticeTextBlock.Opacity = 0;
            NoticeTextBlock.Cursor = Cursors.Arrow;
        }

        private void Notice(string Error, byte A, byte R, byte G, byte B)
        {
            NoticeTextBlock.Text = $"{Error}";
            NoticeTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(A, R, G, B));
            NoticeTextBlock.Cursor = Cursors.Hand;
            NoticeTextBlock.Opacity = 100;
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressBarGrid.Visibility = Visibility.Hidden;
            if (FileCheckBox.IsChecked == true)
            {
                OpenFileDialog SelectFile = new OpenFileDialog();

                SelectFile.Filter = "All files (*.*)|*.*";
                SelectFile.FilterIndex = 1;
                SelectFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer).ToString();
                SelectFile.RestoreDirectory = true;

                if (SelectFile.ShowDialog() == true)
                {
                    SelectedFileDir = SelectFile.FileName; // Get selected files Directory

                    if (SelectedFileDir != null)
                    {
                        FolderDirTextBox.Text = SelectedFileDir;
                    }
                    else
                    {
                        MessageBox.Show("You need to select a file or folder!");
                    }
                }
            }
            else if (FolderCheckBox.IsChecked == true)
            {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        files = Directory.GetFiles(fbd.SelectedPath);
                        FolderDirTextBox.Text = fbd.SelectedPath;
                    }
                }
            }
        }

        private void AddFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileCheckBox.IsChecked == true)
            {
                if (!SelectedFilesListBox.Items.Contains(FolderDirTextBox.Text))
                {
                    SelectedFilesListBox.Items.Add(FolderDirTextBox.Text);
                }
                else
                {
                    Notice("Error: File(s) already added.", ErrorA, ErrorR, ErrorG, ErrorB);
                }
            }
            else if (FolderCheckBox.IsChecked == true)
            {
                if (!SelectedFilesListBox.Items.Contains(FolderDirTextBox.Text))
                {
                    SelectedFilesListBox.Items.Add(FolderDirTextBox.Text);
                }
                else
                {
                    Notice("Error: File(s) already added.", ErrorA, ErrorR, ErrorG, ErrorB);
                }
            }
            ShouldBackupFilesButtonBePlural();
        }

        private void ShouldBackupFilesButtonBePlural()
        {
            if (SelectedFilesListBox.Items.Count > 1)
            {
                BackupFilesButton.Content = "Backup Files";
            }
            else
            {
                BackupFilesButton.Content = "Backup File";
            }
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedFilesListBox.Items.Clear();
            ShouldBackupFilesButtonBePlural();
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (string s in SelectedFilesListBox.SelectedItems.OfType<string>().ToList())
            {
                SelectedFilesListBox.Items.Remove(s);
            }
            ShouldBackupFilesButtonBePlural();
        }

        public string IsDefaultBackupDirAssigned()
        {
            if (Properties.Settings.Default.DefaultSaveDir != "")
            {
                BackupDir = Properties.Settings.Default.DefaultSaveDir;
                MessageBoxResult result = CustomMessageBox.Show($"Backup to: '{BackupDir}' ?", "Backup Directory",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // CustomMessageBox.Show("Yes");
                        break;
                    case MessageBoxResult.No:
                        SelectBackupDir();
                        break;
                }
            }
            else
            {
                CustomMessageBox.Show("Please select a default backup directory." +
                "\nThis can be changed later in the settings.", "Backup Dir Not Set");
                SelectBackupDir();
            }
            return BackupDir;
        }

        public string SelectBackupDir()
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Properties.Settings.Default.DefaultSaveDir = fbd.SelectedPath;
                    BackupDir = Properties.Settings.Default.DefaultSaveDir;
                }
            }
            Properties.Settings.Default.Save();
            return BackupDir;
        }

        private async void BackupFilesButton_Click(object sender, RoutedEventArgs e)
        {
            // Add Warning - Files Will Be Overwritten!

            string[] FilePaths = new string[SelectedFilesListBox.Items.Count];
            FilePaths = SelectedFilesListBox.Items.OfType<string>().ToArray();

            IsDefaultBackupDirAssigned();

            foreach (string s in FilePaths)
            {
                FileAttributes attr = File.GetAttributes(s);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    // Is Directory...
                    string SourceDir = s; // Directory for source files
                    string SourceDirFolderName = new DirectoryInfo(s).Name; // Folder source files are in
                    BackupDir = $@"{Properties.Settings.Default.DefaultSaveDir}\{SourceDirFolderName}"; //Properties.Settings.Default.DefaultSaveDir
                    bool CopySubDirs = true;

                    await DirFileBackup(SourceDir, BackupDir, CopySubDirs);
                }
                else
                {
                    // Is File...
                    string SourceFileDir = s;
                    string SourceFileName = Path.GetFileName(s);
                    string SourceFileFolderName = Path.GetFileName(Path.GetDirectoryName(SourceFileDir));
                    BackupDir = $@"{Properties.Settings.Default.DefaultSaveDir}\{SourceFileFolderName}\{SourceFileName}";

                    string DirToCreate = BackupDir.Replace($@"\{SourceFileName}", "");
                    Directory.CreateDirectory(DirToCreate);

                    await FileBackup(SourceFileDir, BackupDir);
                }
            }
        }

        private async Task FileBackup(string SourceFileDir, string BackupDir)
        {
            await Task.Run(() => File.Copy(SourceFileDir, BackupDir, true));
        }

        private async Task DirFileBackup(string SourceDir, string BackupDir, bool CopySubDirs)
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
            }

            if (CopySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(BackupDir, subdir.Name);
                    await DirFileBackup(subdir.FullName, temppath, CopySubDirs);
                }
            }
        }
    }
}
