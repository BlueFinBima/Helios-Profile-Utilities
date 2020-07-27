using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Xml;
using Microsoft.Win32;

namespace HeliosProfileUtils
{
    /// <summary>
    /// Interaction logic for HeliosProfileUtilityWindow.xaml
    /// </summary>
    public partial class HeliosProfileUtilityWindow : Window
    {
        private string _originalFileName = "";
        private string _newFileName = "";
        public static XmlDocument originalProfile;
        private bool _unsavedChanges = false;
        private string _selectedProfilePanelName = "";
        private String _exportedBindingsElements = "";
        private String _exportedControlElements = "";

        public HeliosProfileUtilityWindow()
        {
            InitializeComponent();
            new AboutBox1().ShowDialog();
        }
        public string SelectedProfilePanelName
        {
            get => _selectedProfilePanelName;
            set => _selectedProfilePanelName = value;
        }
        public string ExportedBindingsElements
        {
            get => _exportedBindingsElements;
            set => _exportedBindingsElements = value;
        }
        public string ExportedControlElements
        {
            get => _exportedControlElements;
            set => _exportedControlElements = value;
        }
        public XmlDocument OriginalProfile
        {
            get => originalProfile;
            set => originalProfile = value;
        }

        private void BtnOpenProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_unsavedChanges)
            {
                if (PromptBox.ShowDialog("Unsaved Changes", "There are unsaved changes in the currently loaded Helios profile.  You should consider saving these before loading another profile.", "Discard Changes", "Save Changes"))
                {
                    BtnSaveProfile_Click(sender, e);
                }
                else {
                    // Discard changes
                    _unsavedChanges = false;
                }
            } 
            if (!_unsavedChanges)
            {
                OpenFileDialog openProfileDialog = new OpenFileDialog
                {
                    InitialDirectory = Environment.GetEnvironmentVariable("userprofile") + "\\Documents\\Helios\\Profiles\\",
                    Filter = "Helios Profiles (*.hpf)|*.hpf;*.hpf.bak|All files (*.*)|*.*"
                };
                if (openProfileDialog.ShowDialog() == true)
                {
                    originalProfile = new XmlDocument();
                    originalProfile.PreserveWhitespace = true;
                    _originalFileName = openProfileDialog.FileName;
                    if (_originalFileName != "")
                    {
                        try
                        {
                            originalProfile.Load(_originalFileName);
                            _unsavedChanges = false;
                        }
                        catch (Exception ex)
                        {
                            messageLog.Text += "Error Loading Helios Profile.\n";
                            throw (ex);
                        }
                        XmlNode n = originalProfile.ChildNodes[0];
                        while (n.Name != "HeliosProfile" && n != null)
                        {
                            n = n.NextSibling;
                        }
                        if (n.Name == "HeliosProfile")
                        {
                            messageLog.Text += String.Format("Successfully loaded Helios Profile {0}.\n", _originalFileName);
                            imagesEditor.Text = _exportedBindingsElements;
                            profileEditor.Text = _exportedControlElements;
                        }
                    }
                    else
                    {
                        messageLog.Text += String.Format("{0} is not a Helios Profile.\n", _originalFileName);
                        _originalFileName = "";
                    }
                }
            }
        }

        private void BtnPackageImages_Click(object sender, RoutedEventArgs e)
        {
            String projectImageDir = "FA-18C_Linx";
            projectImageDir = InputBox.ShowDialog("Image Directory","Enter the Directory Name For Saving Images",projectImageDir,"Cancel","Ok");
            if(!Directory.Exists(string.Format("{0}\\Documents\\Helios\\Images\\{1}\\", Environment.GetEnvironmentVariable("userprofile"), projectImageDir)))
            {
                try
                {
                    Directory.CreateDirectory(string.Format("{0}\\Documents\\Helios\\Images\\{1}\\", Environment.GetEnvironmentVariable("userprofile"), projectImageDir));
                }
                catch (Exception ex)
                {
                    messageLog.Text += String.Format("Creation of destination directory failed: {0}\n", ex.Message);
                }
            }
            if (projectImageDir != "")
            {
                XmlNodeList imageList;
                // find all of the nodes which have Image in their names - note ImageAlignment will show up in the list.  There are likely to be elements which do not contain any text.
                imageList = originalProfile.SelectNodes("//*[contains(local-name(),'Image')]");
                Dictionary<string, string> imageDictionary = new Dictionary<string, string>();
                messageLog.Text += string.Format("Starting to process images to {0}\n", projectImageDir);
                foreach (XmlNode imageNode in imageList)
                {
                    if (imageNode != null && imageNode.InnerText != "" && !imageNode.InnerText.StartsWith("{") && imageNode.InnerText.IndexOf(".png",StringComparison.InvariantCultureIgnoreCase)>0)
                    {
                        _unsavedChanges = true;
                        if (!imageDictionary.ContainsKey(imageNode.InnerText.Replace("/", "\\")))
                        {
                            imageDictionary.Add(imageNode.InnerText.Replace("/", "\\"), "1");  // Save a list of unique image filenames
                        }
                        imageNode.InnerText = String.Format("{0}\\{1}", projectImageDir, CleanImageName(imageNode.InnerText.Replace("/", "\\").Substring(imageNode.InnerText.Replace("/", "\\").LastIndexOf("\\") + 1))); // replace the file name in the profile
                        imagesEditor.Text += "\n" + imageNode.InnerText;
                    }
                }
                messageLog.Text += "Coralling Image Files used by this Helios Profile\n";
                foreach (string k in imageDictionary.Keys)
                {
                    profileEditor.Text += string.Format("\n{0}  ->  {1}\\{2}", k, projectImageDir, CleanImageName(k.Substring(k.LastIndexOf("\\") + 1)));
                    try
                    {
                        File.Copy(string.Format("{0}\\Documents\\Helios\\Images\\{1}", Environment.GetEnvironmentVariable("userprofile"), k), string.Format("{0}\\Documents\\Helios\\Images\\{1}\\{2}", Environment.GetEnvironmentVariable("userprofile"), projectImageDir, CleanImageName(k.Substring(k.LastIndexOf("\\") + 1))));
                    }
                    catch (IOException copyError)
                    {
                        messageLog.Text += string.Format("Copy failed for {0}\n", copyError.Message);
                    }
                    catch (Exception ex)
                    {
                        messageLog.Text += string.Format("Copy failed: 0}\n", ex.Message);
                    }
                }
            } else
            {
                messageLog.Text += string.Format("Copy failed due to no target directory.\n");
            }
        }

        private void BtnPanelExtractor_Click(object sender, RoutedEventArgs e)
        {
            // This passes the in memory DOM version of the profile
            PanelListWindow plw = new PanelListWindow(this);
            plw.Show();
        }
        private void BtnPanelInserter_Click(object sender, RoutedEventArgs e)
        {
            // This inserts the extracted visual components and their bindings into the currently loaded profile
            if (_exportedControlElements != "")
            {
                XmlDocument tempDoc = new XmlDocument();
                tempDoc.LoadXml(_exportedControlElements);
                XmlNode targetNode = originalProfile.SelectSingleNode("//Children");
                XmlNode insertionNode = originalProfile.ImportNode(tempDoc.FirstChild, true);
                targetNode.AppendChild(insertionNode);
                _unsavedChanges = true;
                messageLog.Text += "Inserted Controls.\n";
            }
            if (_exportedBindingsElements != "")
            {
                XmlDocument tempDoc = new XmlDocument();
                tempDoc.LoadXml("<X>" + _exportedBindingsElements + "</X>");
                XmlNode targetNode = originalProfile.SelectSingleNode("//Bindings");
                XmlNode insertionNode = originalProfile.ImportNode(tempDoc.FirstChild, true);
                targetNode.AppendChild(insertionNode);
                targetNode.InnerXml = targetNode.InnerXml.Replace("<X>", "").Replace("</X>", "");
                _unsavedChanges = true;
                messageLog.Text += "Inserted Bindings.\n";
            }
        }
        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveProfileDialog = new SaveFileDialog {
                InitialDirectory = Environment.GetEnvironmentVariable("userprofile") + "\\Documents\\Helios\\Profiles\\",
                Filter = "Helios Profiles (*.hpf)|*.hpf|Helios Profile Backups (*.hpf.bak)|*.hpf.bak|All files (*.*)|*.*",
                Title = "Save a Helios Profile"
            };
            saveProfileDialog.ShowDialog();
            if (saveProfileDialog.FileName != "")
            {
                _newFileName = saveProfileDialog.FileName;
                try
                {
                    originalProfile.Save(_newFileName);
                    _unsavedChanges = false;
                    messageLog.Text += String.Format("Successfully saved Helios profile to file: {0}\n", _newFileName );
                }
                catch (Exception ex)
                {
                    messageLog.Text += String.Format("Error saving Helios profile to file: {0}\n", _newFileName );
                    //_unsavedChanges = true;
                    throw (ex);
                }
            }
        }
        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs eventArgs)
        {
            if (_unsavedChanges)
            {
                _unsavedChanges = !PromptBox.ShowDialog("Unsaved Changes","There are unsaved changes to the loaded Helios profile. If you CONTINUE, you will lose the changes. " +
    "  Press CANCEL to go back and save these changes.", "Cancel", "Continue");
            }
                eventArgs.Cancel = _unsavedChanges;
        }
        private string CleanImageName(string sb)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            sb = ti.ToTitleCase(sb)
            .Replace(" ", "_")
            .Replace("_ON", "_On")
            .Replace("_OFF", "_Off")
            .Replace(".Png", ".png")
            .Replace("BTN", "_button")
            .Replace("Btn", "button")
            .Replace("Sw_", "switch_")
            .Replace("Panel_", "panel_")
            .Replace("_PUSHED", "_Pushed")
            .Replace("_PULLED", "_Pulled")
            .Replace("_PRESSED", "_Pressed")
            .Replace("_RELEASED", "_Released")
            .Replace("_CLOSED", "_Closed")
            .Replace("_OPENED", "_Opened")
            .Replace("_NORMAL", "_Normal")
            .Replace("_NRM", "_Normal")
            .Replace("_UP", "_Up")
            .Replace("_DOWN", "_Down")
            .Replace("_LEFT", "_Left")
            .Replace("_RIGHT", "_Right")
            .Replace("_MIDDLE", "_Middle")
            .Replace("_MID", "_Middle")
            .Replace("_Mid.", "_Middle.")
            .Replace("_Mid_", "_Middle_")
            .Replace("ndicador", "ndicator");

            return sb;
        }
     }
}
