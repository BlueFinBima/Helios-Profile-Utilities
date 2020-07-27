using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace HeliosProfileUtils
{
    /// <summary>
    /// Interaction logic for ExtractedPanelWindow.xaml
    /// </summary>
    public partial class ExtractedPanelWindow : Window
    {
        public XmlDocument originalProfile;
        private string _profilePanelId = "";
        private string _profilePanelName = "";
        private HeliosProfileUtilityWindow _mainWindow;

        public ExtractedPanelWindow(ref XmlDocument originalProfile, String profilePanelName)
        {
            this.originalProfile = originalProfile;
            _profilePanelId = profilePanelName.Substring(0, profilePanelName.Length - 1);
            _profilePanelName = _profilePanelId.Substring(_profilePanelId.LastIndexOf(".") + 1);
            InitializeComponent();
        }
        public ExtractedPanelWindow(HeliosProfileUtilityWindow callingWindow, String profilePanelName)
        {
            _mainWindow = callingWindow;
            originalProfile = callingWindow.OriginalProfile;
            _profilePanelId = profilePanelName.Substring(0, profilePanelName.Length - 1);
            _profilePanelName = _profilePanelId.Substring(_profilePanelId.LastIndexOf(".") + 1);
            InitializeComponent();
        }
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            XmlNodeList nodeList;
            XmlNode root;
            Button but = sender as Button;
            switch (but.Name)
            {
                case "BtnExtractPanel":
                    // Process the visual components from the specified panel downwards.
                    leftEditor.Text = "";
                    root = originalProfile.DocumentElement.FirstChild.NextSibling;
                    while (root.LocalName != "Monitors")
                    {
                        root = root.NextSibling;
                    }
                    nodeList = root.SelectNodes(string.Format("//Control[@TypeIdentifier='Helios.Panel' and @Name='{0}']",_profilePanelName));
                    foreach(XmlNode n in nodeList)
                    {
                        string nodeAttributes = "";
                        foreach (XmlAttribute a in n.Attributes) {
                            nodeAttributes += string.Format(" {0}=\"{1}\"", a.Name, a.Value);
                        }
                        leftEditor.Text += string.Format("<{0} {1}>", n.Name, nodeAttributes);
                        leftEditor.Text += n.InnerXml;
                        leftEditor.Text += string.Format("</{0}>",n.Name);
                    }
                    _mainWindow.ExportedControlElements = leftEditor.Text;
                    // Process the associated bindings.
                    rightEditor.Text = "";
                    root = originalProfile.DocumentElement.FirstChild.NextSibling;
                    while (root.LocalName != "Bindings")
                    {
                        root = root.NextSibling;
                    }
                    nodeList = root.SelectNodes(string.Format("//Trigger[contains(@Source,'{0}')]|//Action[contains(@Target,'{0}')]", _profilePanelId));
                    foreach (XmlNode n in nodeList)
                    {
                        rightEditor.Text += string.Format("<{0}>", n.ParentNode.Name);
                        rightEditor.Text += n.ParentNode.InnerXml.Replace(_profilePanelId, "Monitor 1");
                        rightEditor.Text += string.Format("</{0}>\r\n", n.ParentNode.Name);
                    }
                    _mainWindow.ExportedBindingsElements = rightEditor.Text;
                    break;
                case "Btn2":
                    break;
                case "Btn3":
                    break;
                case "Btn4":
                    break;
                default:
                    break;
            }
        }
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs eventArgs)
        {
            eventArgs.Cancel = false;
        }
    }
}
