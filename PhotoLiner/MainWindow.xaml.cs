using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;

namespace PictureOrder
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SourceFolder = "G:\\Output\\TODO\\2";
            OutputFolder = "G:\\Output";
        }

        private void StartRenaming(object sender, RoutedEventArgs e)
        {
            if (SourceFolder.Length == 0)
            {
                System.Windows.MessageBox.Show("Please enter source folder");
                return;
            }

            if (OutputFolder.Length == 0)
            {
                System.Windows.MessageBox.Show("Please enter output folder");
                return;
            }

            if (Directory.Exists(OutputFolder + "\\Recognized") == false)
                Directory.CreateDirectory(OutputFolder + "\\Recognized");

            if (Directory.Exists(OutputFolder + "\\NoRecognized") == false)
                Directory.CreateDirectory(OutputFolder + "\\NoRecognized");

            ProcessingProgress.Minimum = 0;

            ProcessImageFolder(SourceFolder);

            System.Windows.MessageBox.Show("Processing Finishes");
        }

        private void ProcessImageFolder(String folderPath)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                if (directoryInfo == null)
                    return;

                FileInfo[] fileInfos = directoryInfo.GetFiles();
                ProcessingProgress.Maximum += fileInfos.Count();

                foreach (FileInfo fileInfo in fileInfos)
                {
                    ProcessImages(fileInfo.FullName);
                }

                DirectoryInfo[] subDirectoryInfos = directoryInfo.GetDirectories();
                foreach (DirectoryInfo subDirectoryInfo in subDirectoryInfos)
                {
                    ProcessImageFolder(subDirectoryInfo.FullName);
                }
            }
            catch (Exception e)
            {
                ProcessingException.Text += String.Format("Error on Folder {0}", folderPath);
                System.Console.Out.WriteLine(e.Message);
            }
        }

        private String GetUniqueFileName(String filePath)
        {
            String uniquePath = filePath;
            int counter = 0;

            while (true)
            {
                if (File.Exists(uniquePath))
                {
                    String extension = System.IO.Path.GetExtension(filePath);
                    String fileWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    String fileDirectory = System.IO.Path.GetDirectoryName(filePath);

                    uniquePath = fileDirectory + "\\" +fileWithoutExtension + "__" + counter.ToString() + extension;
                    counter++;
                }
                else
                {
                    return uniquePath;
                }
            }
        }

        private void ProcessImages(String imagePath)
        {
            try
            {
                String takenDate = GetTakenDate(imagePath, true);
                String extension = System.IO.Path.GetExtension(imagePath);
                String newImagepath = imagePath;

                if (takenDate == null)
                {
                    String fileIdentity = imagePath.Replace("\\", "_").Replace(":", "=");
                    newImagepath = OutputFolder + "\\NoRecognized\\" + fileIdentity;
                }
                else
                {
                    newImagepath = OutputFolder + "\\Recognized\\" + takenDate + extension;
                }

                File.Copy(imagePath, GetUniqueFileName(newImagepath));
            }
            catch (Exception e)
            {
                ProcessingException.Text += String.Format("Error on File {0}", imagePath);
                System.Console.Out.WriteLine("Error:%s file=" + imagePath, e.Message);
            }

            ProcessingProgress.Value += 1;
        }

        public static String GetTakenDate(string imagePath, Boolean withTime)
        {
            try
            {
                Uri imgUri = new Uri(imagePath);
                BitmapSource img = BitmapFrame.Create(imgUri);
                BitmapMetadata meta = (BitmapMetadata)img.Metadata;
                DateTime dateTime = DateTime.Parse(meta.DateTaken);
                if (withTime)
                    return dateTime.ToString("yyyyMMdd_hhmmss");
                else
                    return dateTime.ToString("yyyyMMdd");
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        private void StartGrouping(object sender, RoutedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(OutputFolder + "\\Recognized\\");
            if (directoryInfo == null)
            {
                System.Windows.MessageBox.Show("Fail to start grouping task");
                return;
            }

            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
            {
                String subFolderName = OutputFolder + "\\Recognized\\" + fileInfo.Name.Substring(0, 8);

                try
                {
                    if (!System.IO.Directory.Exists(subFolderName))
                    {
                        System.IO.Directory.CreateDirectory(subFolderName);
                    }

                    System.IO.File.Move(fileInfo.FullName, subFolderName + "\\" + fileInfo.Name);
                }
                catch (Exception e1)
                {
                    System.Console.Out.WriteLine("Error:%s", e1.Message);
                }
            }

            System.Windows.MessageBox.Show("Processing Finishes");
        }

        private String SourceFolder
        {
            get
            {
                return ImageSourceFolderTextEdit.Text;
            }
            set
            {
                ImageSourceFolderTextEdit.Text = value;
            }
        }

        private String OutputFolder
        {
            get
            {
                return ImageOutputFolderTextEdit.Text;
            }
            set
            {
                ImageOutputFolderTextEdit.Text = value;
            }
        }

        private String ImagePrefix
        {
            get
            {
                return ImagePrefixEditBox.Text;
            }
            set
            {
                ImagePrefixEditBox.Text = value;
            }
        }

        private String SelectLocalFolder(String currentPath, String description, Boolean showNewFolderButton)
        {
            String selectedPath = currentPath;

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = description;
            folderBrowserDialog.ShowNewFolderButton = showNewFolderButton;

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedPath = folderBrowserDialog.SelectedPath;
            }
            return selectedPath;
        }

        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            SourceFolder = SelectLocalFolder(SourceFolder, "Select folder of your images", false);
        }

        private void BrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            OutputFolder = SelectLocalFolder(OutputFolder, "Select folder to save your relined images", true);
        }

        private void ImagePrefixEditBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FileNameFormatSampleLabel.Content = String.Format("e.g. {0}20130626__121159.png", ImagePrefixEditBox.Text.Length == 0 ? "" : ImagePrefixEditBox.Text + "__");
        }
    }
}
