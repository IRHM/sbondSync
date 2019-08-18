using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WPFCustomMessageBox;

namespace BackupFolders
{
    public class SelectingFilesClass
    {
        static string[] files;
        static string SelectedFileDir;

        // Colours
        static byte ErrorA = 255;
        static byte ErrorR = 225;
        static byte ErrorG = 0;
        static byte ErrorB = 0;

        public static void SelectFiles(CheckBox FileCheckBox, CheckBox FolderCheckBox, TextBox FolderDirTextBox)
        {
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
            else
            {
                CustomMessageBox.Show("Please check either the folder or file check box.");
            }
        }
    }
}
