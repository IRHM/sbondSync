﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    public partial class MainWindow : Window
    {
        public static string BackupDir;
        

        // Colors
        public static byte ErrorA = 255;
        public static byte ErrorR = 125;
        public static byte ErrorG = 39;
        public static byte ErrorB = 39;

        byte SuccessA = 255;
        byte SuccessR = 98;
        byte SuccessG = 255;
        byte SuccessB = 127;

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

        public void ResetProgressBar()
        {
            ProgressBar.Visibility = Visibility.Hidden;
            ProgressBar.Value = 0;
            ProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(SuccessA, SuccessR, SuccessG, SuccessB));
            ProgressBarTextBlock.Text = "";
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            ResetProgressBar();
            SelectingFilesClass.SelectFiles(FileCheckBox, FolderCheckBox, FolderDirTextBox);
        }

        private void AddFilesButton_Click(object sender, RoutedEventArgs e)
        {
            ResetProgressBar();

            if (SelectedFilesListBox.Items.Contains(FolderDirTextBox.Text) || FolderDirTextBox.Text == "")
            {
                Notice("Error: Duplicate File or Nothing Selected.", ErrorA, ErrorR, ErrorG, ErrorB);
            }
            else
            {
                SelectedFilesListBox.Items.Add(FolderDirTextBox.Text);
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

                MessageBoxResult result = CustomMessageBox.ShowYesNoCancel(
                    $"Backup to: '{BackupDir}' ?",
                    "Backup Directory",
                    "Yes",
                    "Change Backup Dir",
                    "Cancel",
                    MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        BackupDirMessageBoxResponse = "Yes";
                        break;
                    case MessageBoxResult.No:
                        BackupDirMessageBoxResponse = "Change Backup Dir";
                        break;
                    case MessageBoxResult.Cancel:
                        BackupDirMessageBoxResponse = "Cancel";
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

        string BackupDirMessageBoxResponse;

        public static string SelectBackupDir()
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

        private void BackupFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileCopyingClass.IsFileCopyingActive)
            {
                CustomMessageBox.Show("Files Are Currently Being Copied. Please Wait.");
            }
            else
            {
                IsDefaultBackupDirAssigned();

                if (BackupDirMessageBoxResponse == "Yes")
                {
                    if (SelectedFilesListBox.Items.Count != 0)
                    {
                        ResetProgressBar();
                        FileCopyingClass.StartCopying(SelectedFilesListBox, ProgressBar, ProgressBarTextBlock, ElapsedTimeTextBlock);
                    }
                    else
                    {
                        FileCopyingClass.FileCopyNotice(ProgressBar, ProgressBarTextBlock, "No Files Selected", ErrorA, ErrorR, ErrorG, ErrorB);
                    }
                }
                else if (BackupDirMessageBoxResponse == "Change Backup Dir")
                {
                    SelectBackupDir();
                }
                else if (BackupDirMessageBoxResponse == "Cancel")
                {
                    FileCopyingClass.FileCopyNotice(ProgressBar, ProgressBarTextBlock, "Operation Cancelled", ErrorA, ErrorR, ErrorG, ErrorB);
                }
            }
        }

        private void CancelProcessButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
