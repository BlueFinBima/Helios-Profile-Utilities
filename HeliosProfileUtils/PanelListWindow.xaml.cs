using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace HeliosProfileUtils
{
    /// <summary>
    /// Interaction logic for PanelListWindow.xaml
    /// </summary>
    public partial class PanelListWindow : Window
    {
        private string _selectedProfilePanelName = "";
        public XmlDocument originalProfile;
        private HeliosProfileUtilityWindow _mainWindow;
        public PanelListWindow(ref XmlDocument originalProfile)
        {
            this.originalProfile = originalProfile;
            InitializeComponent();
            BuildTree(treeView, ref originalProfile);
        }
        public PanelListWindow(HeliosProfileUtilityWindow callingWindow)
        {
            _mainWindow = callingWindow;
            originalProfile = callingWindow.OriginalProfile;
            InitializeComponent();
            BuildTree(treeView, ref originalProfile);
        }
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs eventArgs)
        {
            eventArgs.Cancel = false;
        }

        private void BtnExtractPanel_Click(object sender, RoutedEventArgs e)
        {
            // Process the visual components from the specified panel downwards.
            String profilePanelId = _selectedProfilePanelName.Substring(0, _selectedProfilePanelName.Length - 1);
            String profilePanelName = profilePanelId.Substring(profilePanelId.LastIndexOf(".") + 1);
            String controlsXML = "";
            String bindingsXML = "";
            XmlNodeList nodeList;
            XmlNode root;
            controlsXML = "";
            root = originalProfile.DocumentElement.FirstChild.NextSibling;
            while (root.LocalName != "Monitors")
            {
                root = root.NextSibling;
            }
            nodeList = root.SelectNodes(string.Format("//Control[@TypeIdentifier='Helios.Panel' and @Name='{0}']", profilePanelName));
            string newLocation = "<Location>0,0</Location>";
            string oldLocation = newLocation;
            foreach (XmlNode n in nodeList)
            {
                foreach(XmlNode m in n.ChildNodes)
                {
                    if(m.LocalName == "Location" && m.InnerText != "0,0")
                    {
                        // the location of the parent panel is not top left, we need to force it to go there.
                        oldLocation = m.OuterXml;
                        break;
                    }
                }
                string nodeAttributes = "";
                foreach (XmlAttribute a in n.Attributes)
                {
                    nodeAttributes += string.Format(" {0}=\"{1}\"", a.Name, a.Value);
                }
                controlsXML += string.Format("<{0} {1}>", n.Name, nodeAttributes);
                controlsXML += n.InnerXml;
                controlsXML += string.Format("</{0}>", n.Name);
                if(oldLocation!= newLocation)
                {
                    // update the location of the parent panel to top left
                    int locationPosn = controlsXML.IndexOf(oldLocation);
                    controlsXML = controlsXML.Substring(0, locationPosn) + newLocation + controlsXML.Substring(locationPosn + oldLocation.Length);
                }
            }
            _mainWindow.ExportedControlElements = controlsXML;
            // Process the associated bindings.
            bindingsXML = "";
            root = originalProfile.DocumentElement.FirstChild.NextSibling;
            while (root.LocalName != "Bindings")
            {
                root = root.NextSibling;
            }
            nodeList = root.SelectNodes(string.Format("//Trigger[contains(@Source,'{0}')]|//Action[contains(@Target,'{0}')]", profilePanelId));
            foreach (XmlNode n in nodeList)
            {
                string nodeAttributes = "";
                foreach (XmlAttribute a in n.ParentNode.Attributes)
                {
                    nodeAttributes += string.Format(" {0}=\"{1}\"", a.Name, a.Value);
                }
                bindingsXML += string.Format("<{0} {1}>", n.ParentNode.Name, nodeAttributes);
                bindingsXML += n.ParentNode.InnerXml.Replace(profilePanelId.Substring(0,profilePanelId.LastIndexOf(".")), "Monitor 1");
                bindingsXML += string.Format("</{0}>\r\n", n.ParentNode.Name);
            }
            // We only allow visuals to exist on Monitor 1.  These can then be moved using profile editor.
            for (int i=2;i<10;i++)
            {
                bindingsXML = bindingsXML.Replace(String.Format(";Monitor {0}",i),";Monitor 1");
            }
            _mainWindow.ExportedBindingsElements = bindingsXML;
            _mainWindow.imagesEditor.Text = bindingsXML;
            _mainWindow.profileEditor.Text = controlsXML;
            _mainWindow.messageLog.Text += string.Format("Visual Controls and Bindings for panel structure \"{0}\" have been stored so they can be added to another profile.\n", profilePanelName);
            this.Close();
        }

        private void BuildTree(TreeView treeView, ref XmlDocument doc)
        {
            // DOM version
            XmlNode root = doc.FirstChild;
            root = root.NextSibling;
            while(root.NodeType == XmlNodeType.Whitespace)
            {
                root = root.NextSibling;
            }
            TreeViewItem treeNode = new TreeViewItem
            {
                //Should be Root
                Header = String.Format("{0} : {1}",root.Name,doc.BaseURI.ToString()) ,
                IsExpanded = true
            };
            treeView.Items.Add(treeNode);
            if (root.LocalName =="HeliosProfile" && root.HasChildNodes)
            {
                BuildNodes(treeNode, root.FirstChild);
            }
        }
        private void BuildNodes(TreeViewItem treeNode, XmlNode node)
        {
            int monitorNumber = 1;
            TreeViewItem childTreeNode;
            //DOM version
            do
            {
                switch (node.Name)
                {
                    case "Control":
                        if (node.Attributes["TypeIdentifier"].Value == "Helios.Panel")
                        {
                            childTreeNode = new TreeViewItem
                            {
                                //Get First attribute where it is equal to value
                                Header = string.Format("{0} : {1}", node.Attributes["TypeIdentifier"].Value, node.Attributes["Name"].Value),
                                //Automatically expand elements
                                IsExpanded = true
                            };
                            treeNode.Items.Add(childTreeNode);
                            if (node.HasChildNodes) BuildNodes(childTreeNode, node.FirstChild);
                        }
                        else
                        {
                            if (node.HasChildNodes) BuildNodes(treeNode, node.FirstChild);
                        }
                        break;
                    case "Monitor":
                            childTreeNode = new TreeViewItem
                            {
                                //Get First attribute where it is equal to value
                                Header = string.Format("Monitor : Monitor {0}",monitorNumber++),
                                //Automatically expand elements
                                IsExpanded = true
                            };
                            treeNode.Items.Add(childTreeNode);
                            if (node.HasChildNodes) BuildNodes(childTreeNode, node.FirstChild);
                        break;
                    default:
                        if (node.HasChildNodes) BuildNodes(treeNode, node.FirstChild);
                        break;
                }
                node = node.NextSibling;
            } while (node != null);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // create the Helios path from the parents
            string z = "";
            TreeViewItem vti = (TreeViewItem)e.NewValue;
            if (vti.IsSelected)
            {
                // create the Helios path from the parents
                do
                {
                    string t = (string)vti.Header;
                    t = t.Substring(t.IndexOf(":") + 2);
                    z = t + "." + z;
                    vti = (TreeViewItem)vti.Parent;
                } while (!vti.Parent.GetType().Equals(typeof(TreeView)));
                _selectedProfilePanelName = z;
            }
        }
    }
}
