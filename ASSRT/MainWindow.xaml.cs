using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace ASSRT
{
    public partial class MainWindow : Window
    {
        string subtitle = "";
        List<DataClass> AssFiles;
        FileInfo[] files;

        public MainWindow()
        {
            InitializeComponent();
            SetStatus(0);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void AssFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            GetAssFolder();
        }

        private void GetAssFolder()
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Choose .ass folder";
            dlg.IsFolderPicker = true;
            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                AssFolderTextBox.Text = dlg.FileName;

                var path = AssFolderTextBox.Text;
                SelectFiles(path);
            }
        }

        private void SelectFiles(string path)
        {
            SubtitleList.ItemsSource = null;

            if (path == "")
            {
                SetStatus(1);
            }
            else
            {
                if (Directory.Exists(path))
                {
                    files = new DirectoryInfo(path).GetFiles("*.ass");
                    if (files.Length == 0)
                    {
                        SetStatus(2);
                    }
                    else
                    {
                        PopulateDataGrid(files);
                    }

                }
                else
                {
                    StatusBar.Content = "Not a valid directory.";
                }
            }
        }

        private void ConvertAssFiles_Click(object sender, RoutedEventArgs e)
        {
            var path = AssFolderTextBox.Text;
            if (path == "")
            {
                SetStatus(4);
            }
            else if (files.Length != 0)
            {
                ConvertFiles(files);
            }              
        }

        private void PopulateDataGrid(FileInfo[] files)
        {
            AssFiles = new List<DataClass>();
            foreach (var file in files)
            {
                AssFiles.Add(new DataClass(file.ToString().Replace(".ass", ".srt")));
            }

            SubtitleList.ItemsSource = AssFiles;

            SetStatus(3);
        }

        private void ConvertFiles(FileInfo[] files)
        {
            // File conversion
            foreach (var file in files)
            {
                using (StreamReader sr = file.OpenText())
                {
                    string line = "";
                    int lineIndex = 1;

                    // Read line in files
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Extract time and dialogue
                        if (line.StartsWith("Dialogue"))
                        {
                            string time;
                            string dialogue;

                            // Extract time
                            int timeStartIndex = line.IndexOf(',') + 1;
                            int timeEndIndex = line.IndexOf(',', line.IndexOf(',') + 1) - 1;
                            string[] times = line.Substring(timeStartIndex, timeEndIndex).Split(',');

                            time = ConvertTime(times);

                            // Extract dialogue
                            int dialogueIndex = line.IndexOf(",,", line.IndexOf(",,") + 1) + 2;

                            dialogue = ConvertDialogue(line.Substring(dialogueIndex));

                            // Assemle a line with the line index time and dialogue
                            string assembledLine = lineIndex.ToString() + "\n" + time + "\n" + dialogue + "\n\n";
                            lineIndex++;

                            // Assemble the whole subtitle string
                            AssembleSubtitle(assembledLine);
                        }
                    }
                }
                SaveToFile(file.ToString());
            }
            SetStatus(10);

            if (FilesDeleteCheckBox.IsChecked == true)
            {
                DeleteFiles();
            }
        }

        private string ConvertTime(string[] times)
        {
            // Replace dots with commas
            times[0] = times[0].Replace('.', ',');
            times[1] = times[1].Replace('.', ',');

            // Add hours leading zeros
            if (times[0].IndexOf(':') == 1)
                times[0] = "0" + times[0];
            if (times[1].IndexOf(':') == 1)
                times[1] = "0" + times[1];

            // Add miliseconds leading zeros
            string miliseconds;

            if (times[0].Length - times[0].IndexOf(',') - 1 == 2)
            {
                miliseconds = "0" + times[0].Substring(times[0].IndexOf(',') + 1);
                times[0] = times[0].Substring(0, times[0].Length - 2) + miliseconds;
            }

            if (times[1].Length - times[1].IndexOf(',') - 1 == 2)
            {
                miliseconds = "0" + times[1].Substring(times[1].IndexOf(',') + 1);
                times[1] = times[1].Substring(0, times[1].Length - 2) + miliseconds;
            }

            // Return formatted time
            return times[0] + " --> " + times[1];
        }

        private string ConvertDialogue(string dialogue)
        {
            // Remove .ass text formatting
            dialogue = Regex.Replace(dialogue, "{.*?}", string.Empty);

            // Set newline characters
            dialogue = dialogue.Replace("\\N", "\n");

            return dialogue;
        }

        private void AssembleSubtitle(string assembledLine)
        {
            subtitle += assembledLine;
        }

        private void SaveToFile(string fileName)
        {
            fileName = fileName.Replace(".ass", ".srt");

            if (!File.Exists(AssFolderTextBox.Text + "\\" + fileName))
            {
                // Create a file to write to.
                File.WriteAllText(AssFolderTextBox.Text + "\\" + fileName, subtitle);
            }
            subtitle = "";

            // Set converted files to green color
            for (int i = 0; i < AssFiles.Count; i++)
            {
                if (AssFiles[i].Name == fileName)
                {
                    AssFiles[i].ChangeColor();
                }
            }
        }

        private void DeleteFiles()
        {
            foreach (var file in files)
            {
                file.Delete();
            }
            SetStatus(11);
        }

        private void SetStatus(int StatusCode)
        {
            string Status = "";

            switch (StatusCode)
            {
                case 0:
                    Status = "Ready to convert some .ass files";
                    break;
                case 1:
                    Status = "Choose a folder first";
                    break;
                case 2:
                    Status = "Folder does not contain any .ass files";
                    break;
                case 3:
                    Status = "All .ass files loaded";
                    break;
                case 4:
                    Status = "Choose a folder first";
                    break;
                case 10:
                    Status = "All files converted";
                    break;
                case 11:
                    Status = "All files converted, .ass files deleted";
                    break;
            }

            StatusBar.Content = Status;
        }
    }
}


