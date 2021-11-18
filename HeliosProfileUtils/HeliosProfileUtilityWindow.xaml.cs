using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace HeliosProfileUtils
{
    /// <summary>
    /// Interaction logic for HeliosProfileUtilityWindow.xaml
    /// </summary>
    public partial class HeliosProfileUtilityWindow : Window
    {
        private string _originalFileName = "";
        private string _windowTitle = "";
        private string _newFileName = "";
        public static XmlDocument originalProfile;
        private bool _unsavedChanges = false;
        private string _selectedProfilePanelName = "";
        private String _exportedBindingsElements = "";
        private String _exportedControlElements = "";

        public HeliosProfileUtilityWindow()
        {
            InitializeComponent();
            _windowTitle = this.Title;
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

        private bool UnsavedChanges
        {
            get => _unsavedChanges;
            set
            {
                this.Title = string.Format("{0} : {1} {2}", _windowTitle, value ? "*" : "", _originalFileName);
                _unsavedChanges = value;
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            switch (((System.Windows.Controls.Button)e.Source).Name) { 
                case "BtnOpenProfile":
                    OpenProfile();
                    break;
                case "BtnSaveProfile":
                    SaveProfile();
                    break;
                case "BtnPanelExtractor":
                    ExtractPanel();
                    break;
                case "BtnElementInserter":
                    InsertElements();
                    break;
                case "BtnPackageImages":
                    PackageImages();
                    break;
                default:
                    break;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (((System.Windows.Controls.MenuItem)e.Source).Header)
            {
                case "_Open":
                    OpenProfile();
                    break;
                case "_New":
                    NewProfile();
                    break;
                case "_Save":
                case "S_ave As":
                    SaveProfile();
                    break;
                case "_Close":
                case "_Exit":
                    this.Close();
                    break;
                case "_Extract Panel":
                    ExtractPanel();
                    break;
                case "E_xtract Interface":
                    ExtractInterface();
                    break;
                case "_Insert Elements":
                    InsertElements();
                    break;
                case "_Package Images":
                    PackageImages();
                    break;
                case "_Check Bindings":
                    ProcessBindings(false);
                    break;
                case "_Remove Duplicate Bindings":
                    ProcessBindings(true);
                    break;
                case "_About":
                    new AboutBox1().ShowDialog();
                    break;
                default:
                    break;
            }
        }

        private void Editor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            switch (((System.Windows.Controls.TextBox)e.Source).Name) {
                case "profileEditor":
                    _exportedControlElements = ((System.Windows.Controls.TextBox)e.Source).Text;
                    break;
                case "imagesEditor":
                    _exportedBindingsElements = ((System.Windows.Controls.TextBox)e.Source).Text;
                    break;
                default:
                    break;
            }
        }

        private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            switch (((System.Windows.Input.RoutedCommand)e.Command).Name)
            {
                case "Close":
                    this.Close();
                    break;
                case "Save":
                    SaveProfile();
                    break;
                case "SaveAs":
                    SaveProfile();
                    break;
                case "Open":
                    OpenProfile();
                    break;
                case "Quit":
                    this.Close();
                    break;
                default:
                    break;
            }
        }


            private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs eventArgs)
        {
            if (UnsavedChanges)
            {
                UnsavedChanges = !PromptBox.ShowDialog("Unsaved Changes", "There are unsaved changes to the loaded Helios profile. If you CONTINUE, you will lose the changes. " +
    "  Press CANCEL to go back and save these changes.", "Cancel", "Continue");
            }
            eventArgs.Cancel = UnsavedChanges;
        }

        private void OpenProfile()
        {
            if (UnsavedChanges)
            {
                if (PromptBox.ShowDialog("Unsaved Changes", "There are unsaved changes in the currently loaded Helios profile.  You should consider saving these before loading another profile.", "Discard Changes", "Save Changes"))
                {
                    SaveProfile();
                }
                else
                {
                    // Discard changes
                    UnsavedChanges = false;
                }
            }
            if (!UnsavedChanges)
            {
                OpenFileDialog openProfileDialog = new OpenFileDialog()
                {
                    InitialDirectory = Environment.GetEnvironmentVariable("userprofile") + "\\Documents\\Helios\\Profiles\\",
                    Filter = "Helios Profiles (*.hpf)|*.hpf;*.hpf.bak|All files (*.*)|*.*",
                    RestoreDirectory = true
                };
                if (openProfileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    originalProfile = new XmlDocument()
                    {
                        PreserveWhitespace = true
                    };
                    _originalFileName = openProfileDialog.FileName;
                    if (_originalFileName != "")
                    {
                        try
                        {
                            originalProfile.Load(_originalFileName);
                            UnsavedChanges = false;
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
        private void NewProfile()
        {
            if (UnsavedChanges)
            {
                if (PromptBox.ShowDialog("Unsaved Changes", "There are unsaved changes in the currently loaded Helios profile.  You should consider saving these before loading another profile.", "Discard Changes", "Save Changes"))
                {
                    SaveProfile();
                }
                else
                {
                    _originalFileName = "untitled.hpf";
                    UnsavedChanges = false;
                }
            }
            if (!UnsavedChanges)
            {
                _selectedProfilePanelName = "";
                _exportedBindingsElements = "";
                _exportedControlElements = "";
                messageLog.Text = "";
                imagesEditor.Text = "";
                profileEditor.Text = "";
                _originalFileName = "untitled.hpf";
                UnsavedChanges = false;
            }
        }
        private void SaveProfile()
        {
            SaveFileDialog saveProfileDialog = new SaveFileDialog
            {
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
                    UnsavedChanges = false;
                    messageLog.Text += String.Format("Successfully saved Helios profile to file: {0}\n", _newFileName);
                }
                catch (Exception ex)
                {
                    messageLog.Text += String.Format("Error saving Helios profile to file: {0}\n", _newFileName);
                    //UnsavedChanges = true;
                    throw (ex);
                }
            }
        }

        private void PackageImages()
        {
            String projectImageDir = "FA-18C_Images";
            projectImageDir = InputBox.ShowDialog("Image Directory", "Enter the Directory Name For Saving Images", projectImageDir, "Cancel", "Ok");
            if (!Directory.Exists(string.Format("{0}\\Documents\\Helios\\Images\\{1}\\", Environment.GetEnvironmentVariable("userprofile"), projectImageDir)))
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
                    if (imageNode != null && imageNode.InnerText != "" && !imageNode.InnerText.StartsWith("{") && imageNode.InnerText.IndexOf(".png", StringComparison.InvariantCultureIgnoreCase) > 0)
                    {
                        UnsavedChanges = true;
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
            }
            else
            {
                messageLog.Text += string.Format("Copy failed due to no target directory.\n");
            }
        }

        private void ExtractPanel()
        {
            // This passes the in memory DOM version of the profile
            PanelListWindow plw = new PanelListWindow(this);
            plw.Show();
        }

        private void ExtractInterface()
        {
            // This passes the in memory DOM version of the profile
            InterfaceListWindow ilw = new InterfaceListWindow(this);
            ilw.Show();
        }

        private void InsertElements()
        {
            // The XML to be inserted will be either visual controls or interfaces, and these get inserted into different places.  The binding insertion goes in the same place regardless.

            // This inserts the extracted visual components and their bindings into the currently loaded profile
            if (_exportedControlElements != "")
            {
                string insertionPosn = "//Children";  // this is where the controls live.
                if(_exportedControlElements.IndexOf("Interface ")>0) insertionPosn = "//Interfaces";
                XmlDocument tempDoc = new XmlDocument();
                try
                {
                    tempDoc.LoadXml(_exportedControlElements);
                    XmlNode targetNode = originalProfile.SelectSingleNode(insertionPosn);
                    XmlNode insertionNode = originalProfile.ImportNode(tempDoc.FirstChild, true);
                    targetNode.AppendChild(insertionNode);
                    UnsavedChanges = true;
                    messageLog.Text += "Elements inserted.\n";
                }
                catch (XmlException ex)
                {
                    messageLog.Text += string.Format("XML Error processing Elements at line {1}, Position{2}.\n{0}.\nInsertion stopped & possibly incomplete.\n",ex.Message,ex.LineNumber,ex.LinePosition);
                }
                catch (InvalidOperationException ex)
                {
                    messageLog.Text += string.Format("XML Error Inserting Elements.  {0}.  Insertion stopped & possibly incomplete.\n", ex.Message);
                }
            }
            if (_exportedBindingsElements != "")
            {
                XmlDocument tempDoc = new XmlDocument();
                try
                {
                    tempDoc.LoadXml("<X>" + _exportedBindingsElements + "</X>");
                    XmlNode targetNode = originalProfile.SelectSingleNode("//Bindings");
                    XmlNode insertionNode = originalProfile.ImportNode(tempDoc.FirstChild, true);
                    targetNode.AppendChild(insertionNode);
                    targetNode.InnerXml = targetNode.InnerXml.Replace("<X>", "").Replace("</X>", "");
                    UnsavedChanges = true;
                    messageLog.Text += "Bindings inserted .\n";
                }
                catch (XmlException ex)
                {
                    messageLog.Text += string.Format("XML Error processing Binding Elements at line {1}, Position{2}.\n{0}.\nInsertion stopped & possibly incomplete.\n", ex.Message, ex.LineNumber, ex.LinePosition);
                }
                catch (InvalidOperationException ex)
                {
                    messageLog.Text += string.Format("XML Error Inserting Binding Elements.  {0}.  Insertion stopped & possibly incomplete.\n", ex.Message);
                }
            }
        }
        private void ProcessBindings(bool resolve = false)
        {
            messageLog.Text += string.Format("Checking Bindings for Duplicates.\n");

            XmlNodeList bindingsList;
            // find all of the nodes for bindings
            bindingsList = originalProfile.SelectNodes("descendant::Binding");
            int index = 0;
            int duplicateCount = 0;
            Dictionary<string, XmlNode> bindingDictionary = new Dictionary<string, XmlNode>();
            if (bindingsList.Count > 0)
            {
                foreach (XmlNode bindingNode in bindingsList)
                {
                    if (index == 0)
                    {
                        index++;
                        bindingDictionary.Add(bindingNode.OuterXml, bindingNode);
                    }
                    else
                    {
                        if (!bindingDictionary.ContainsKey(bindingNode.OuterXml))
                        {
                            index++;
                            bindingDictionary.Add(bindingNode.OuterXml, bindingNode);
                        }
                        else
                        {
                            imagesEditor.Text += string.Format("    {0}\n", bindingNode.OuterXml);
                            duplicateCount++;
                        }
                    }
                }
                messageLog.Text += string.Format("Found {0} unique bindings.\nFound {1} duplicate bindings\n", bindingDictionary.Count, duplicateCount);
                foreach(XmlNode uniqueBindingNode in bindingDictionary.Values)
                {
                    profileEditor.Text += string.Format("    {0}\n", uniqueBindingNode.OuterXml);

                }
            }
            if (resolve)
            {
               try
                {
                    //tempDoc.LoadXml(_exportedControlElements);
                    XmlNode targetNode = originalProfile.SelectSingleNode("//Bindings");
                    targetNode.RemoveAll();
                    messageLog.Text += "All Binding Elements removed.\n";

                    foreach (XmlNode uniqueBindingNode in bindingDictionary.Values)
                    {
                        targetNode.AppendChild(uniqueBindingNode);

                    }
                    UnsavedChanges = true;
                    messageLog.Text += $"{bindingDictionary.Count} Binding Elements inserted.\n";
                }
                catch (XmlException ex)
                {
                    messageLog.Text += string.Format("XML Error processing Elements at line {1}, Position{2}.\n{0}.\nInsertion stopped & possibly incomplete.\n", ex.Message, ex.LineNumber, ex.LinePosition);
                }
                catch (InvalidOperationException ex)
                {
                    messageLog.Text += string.Format("XML Error Inserting Elements.  {0}.  Insertion stopped & possibly incomplete.\n", ex.Message);
                }

            }
        }

        private bool BindingNodeEquals(XmlNode n1, XmlNode n2)
        {
            return n1.OuterXml == n2.OuterXml?true:false;
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
