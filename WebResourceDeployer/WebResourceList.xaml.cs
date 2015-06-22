using EnvDTE;
using EnvDTE80;
using JasonLattimer.WebResourceDeployer.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.Shell;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Xml;

namespace JasonLattimer.WebResourceDeployer
{
    public partial class WebResourceList
    {
        private readonly DTE _dte;
        private readonly Solution _solution;
        private readonly Events _events;
        private readonly Events2 _events2;
        private readonly SolutionEvents _solutionEvents;
        private readonly ProjectItemsEvents _projectItemsEvents;
        private Projects _projects;
        private List<ComboBoxItem> _projectFiles = new List<ComboBoxItem>();
        readonly string[] _extensions = { "HTM", "HTML", "CSS", "JS", "XML", "PNG", "JPG", "GIF", "XAP", "XSL", "XSLT", "ICO" };
        public WebResourceList()
        {
            InitializeComponent();
            _dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            if (_dte == null)
                return;

            _solution = _dte.Solution;
            if (_solution == null)
                return;

            _events = _dte.Events;
            var windowEvents = _events.WindowEvents;
            windowEvents.WindowActivated += WindowEventsOnWindowActivated;
            _solutionEvents = _events.SolutionEvents;
            _solutionEvents.BeforeClosing += BeforeSolutionClosing;
            _solutionEvents.Opened += SolutionOpened;
            _solutionEvents.ProjectAdded += SolutionProjectAdded;
            _solutionEvents.ProjectRemoved += SolutionProjectRemoved;
            _solutionEvents.ProjectRenamed += SolutionProjectRenamed;
            _events2 = (Events2)_dte.Events;
            _projectItemsEvents = _events2.ProjectItemsEvents;
            _projectItemsEvents.ItemAdded += ProjectItemAdded;
            _projectItemsEvents.ItemRemoved += ProjectItemRemoved;
            _projectItemsEvents.ItemRenamed += ProjectItemRenamed;
        }

        //TODO: Handle file move: http://stackoverflow.com/questions/9273633/is-there-a-way-to-detect-file-move-operation-from-a-visual-studio-add-in
        private void ProjectItemRenamed(ProjectItem projectItem, string oldName)
        {
            List<WebResourceItem> webResources = (List<WebResourceItem>)WebResourceGrid.ItemsSource;
            if (webResources == null) return;

            var fullname = projectItem.FileNames[1];
            var projectPath = Path.GetDirectoryName(projectItem.ContainingProject.FullName);
            if (projectPath == null) return;

            var newItemName = fullname.Replace(projectPath, String.Empty).Replace("\\", "/");

            if (projectItem.Name == null) return;

            var oldItemName = newItemName.Replace(Path.GetFileName(projectItem.Name), oldName);
            for (int i = 0; i < _projectFiles.Count; i++)
            {
                if (_projectFiles[i].Content.ToString() != oldItemName) continue;

                _projectFiles[i].Content = newItemName;
                break;
            }

            foreach (var webResourceItem in webResources)
            {
                if (webResourceItem.BoundFile != oldItemName) continue;

                webResourceItem.BoundFile = newItemName;
            }
        }

        private void ProjectItemRemoved(ProjectItem projectItem)
        {
            List<WebResourceItem> webResources = (List<WebResourceItem>)WebResourceGrid.ItemsSource;
            if (webResources == null) return;

            var fullname = projectItem.FileNames[1];
            var projectPath = Path.GetDirectoryName(projectItem.ContainingProject.FullName);
            if (projectPath == null) return;

            var name = fullname.Replace(projectPath, String.Empty).Replace("\\", "/");

            foreach (var webResourceItem in webResources)
            {
                if (webResourceItem.BoundFile != name) continue;

                webResourceItem.BoundFile = String.Empty;
            }

            _projectFiles.RemoveAll(c => c.Content.ToString() == name);
        }

        private void ProjectItemAdded(ProjectItem projectItem)
        {
            List<WebResourceItem> webResources = (List<WebResourceItem>)WebResourceGrid.ItemsSource;
            if (webResources == null) return;

            var fullname = projectItem.FileNames[1];
            var projectPath = Path.GetDirectoryName(projectItem.ContainingProject.FullName);
            if (projectPath == null) return;

            var name = fullname.Replace(projectPath, String.Empty).Replace("\\", "/");
            _projectFiles.Add(new ComboBoxItem() { Content = name });
        }

        private void SolutionOpened()
        {
            _projects = _dte.Solution.Projects;
        }

        private void SolutionProjectRenamed(Project project, string oldName)
        {
            string name = Path.GetFileNameWithoutExtension(oldName);
            foreach (ComboBoxItem comboBoxItem in Projects.Items)
            {
                if (string.IsNullOrEmpty(comboBoxItem.Content.ToString())) continue;
                if (name != null && comboBoxItem.Content.ToString().ToUpper() != name.ToUpper()) continue;

                comboBoxItem.Content = project.Name;
            }

            _projects = _dte.Solution.Projects;
        }

        private void SolutionProjectRemoved(Project project)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)Projects.SelectedItem;

            foreach (ComboBoxItem comboBoxItem in Projects.Items)
            {
                if (string.IsNullOrEmpty(comboBoxItem.Content.ToString())) continue;
                if (comboBoxItem.Content.ToString().ToUpper() != project.Name.ToUpper()) continue;

                Projects.Items.Remove(comboBoxItem);
                break;
            }

            if (selectedItem != null)
            {
                var selectedProject = (Project)selectedItem.Tag;
                if (selectedProject.FullName == project.FullName)
                {
                    WebResourceGrid.ItemsSource = null;
                    WebResourceType.ItemsSource = null;
                    WebResourceType.Items.Clear();
                    Connections.ItemsSource = null;
                    Connections.Items.Clear();
                    Connections.IsEnabled = false;
                    AddConnection.IsEnabled = false;
                    Publish.IsEnabled = false;
                }
            }

            _projects = _dte.Solution.Projects;
        }

        private void SolutionProjectAdded(Project project)
        {
            bool addProject = true;
            foreach (ComboBoxItem projectItem in Projects.Items)
            {
                if (projectItem.Content.ToString().ToUpper() != project.Name.ToUpper()) continue;

                addProject = false;
                break;
            }

            if (addProject)
            {
                ComboBoxItem item = new ComboBoxItem() { Content = project.Name, Tag = project };
                Projects.Items.Add(item);
            }

            _projects = _dte.Solution.Projects;
        }

        private void BeforeSolutionClosing()
        {
            ResetForm();
        }

        private void ResetForm()
        {
            WebResourceGrid.ItemsSource = null;
            WebResourceType.ItemsSource = null;
            WebResourceType.Items.Clear();
            Connections.ItemsSource = null;
            Connections.Items.Clear();
            Connections.IsEnabled = false;
            Projects.ItemsSource = null;
            Projects.Items.Clear();
            Projects.IsEnabled = false;
            AddConnection.IsEnabled = false;
            Publish.IsEnabled = false;
        }

        private void AddConnection_Click(object sender, RoutedEventArgs e)
        {
            Connection connection = new Connection(null, null);
            bool? result = connection.ShowDialog();

            if (!result.HasValue || !result.Value) return;

            var project = (Project)((ComboBoxItem)Projects.SelectedItem).Tag;
            var configExists = ConfigFileExists(project);
            if (!configExists)
                CreateConfigFile(project);

            bool change = AddOrUpdateConnection(project, connection.ConnectionName, connection.ConnectionString, connection.OrgId, connection.Version, true);
            if (!change) return;

            GetConnections(project);
            foreach (CrmConn conn in Connections.Items)
            {
                if (conn.Name != connection.ConnectionName) continue;

                Connections.SelectedItem = conn;
                GetWebResources(connection.ConnectionString);
                break;
            }
        }

        private bool AddOrUpdateConnection(Project vsProject, string connectionName, string connString, string orgId, string versionNum, bool showPrompt)
        {
            var path = Path.GetDirectoryName(vsProject.FullName);
            XmlDocument doc = new XmlDocument();
            doc.Load(path + "\\WebResourceDeployer.config");

            //Check if connection alredy exists for project
            XmlNodeList connectionStrings = doc.GetElementsByTagName("ConnectionString");
            if (connectionStrings.Count > 0)
            {
                foreach (XmlNode node in connectionStrings)
                {
                    string decodedString = DecodeString(node.InnerText);
                    if (decodedString != connString) continue;

                    if (showPrompt)
                    {
                        MessageBoxResult result = MessageBox.Show("Update Connection?", "Connection Already Added",
                            MessageBoxButton.YesNo);

                        //Update existing connection
                        if (result != MessageBoxResult.Yes)
                            return false;
                    }

                    XmlNode connectionU = node.ParentNode;
                    if (connectionU != null)
                    {
                        XmlNode nameU = connectionU["Name"];
                        if (nameU != null)
                            nameU.InnerText = connectionName;
                        XmlNode versionU = connectionU["Version"];
                        if (versionU != null)
                            versionU.InnerText = versionNum;
                    }

                    doc.Save(path + "\\WebResourceDeployer.config");
                    return true;
                }
            }

            //Add the connection elements
            XmlNodeList connections = doc.GetElementsByTagName("Connections");
            XmlElement connection = doc.CreateElement("Connection");
            XmlElement name = doc.CreateElement("Name");
            name.InnerText = connectionName;
            connection.AppendChild(name);
            XmlElement org = doc.CreateElement("OrgId");
            org.InnerText = orgId;
            connection.AppendChild(org);
            XmlElement connectionString = doc.CreateElement("ConnectionString");
            connectionString.InnerText = EncodeString(connString);
            connection.AppendChild(connectionString);
            XmlElement version = doc.CreateElement("Version");
            version.InnerText = versionNum;
            connection.AppendChild(version);
            connections[0].AppendChild(connection);

            doc.Save(path + "\\WebResourceDeployer.config");
            return true;
        }

        private void CreateConfigFile(Project vsProject)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement webResourceDeployer = doc.CreateElement("WebResourceDeployer");
            XmlElement connections = doc.CreateElement("Connections");
            XmlElement files = doc.CreateElement("Files");
            webResourceDeployer.AppendChild(connections);
            webResourceDeployer.AppendChild(files);
            doc.AppendChild(webResourceDeployer);

            var path = Path.GetDirectoryName(vsProject.FullName);
            doc.Save(path + "/WebResourceDeployer.config");
        }

        private bool ConfigFileExists(Project project)
        {
            var path = Path.GetDirectoryName(project.FullName);
            return File.Exists(path + "/WebResourceDeployer.config");
        }

        private Project GetProjectByName(string projectName)
        {
            foreach (Project project in _projects)
            {
                if (project.Name != projectName) continue;

                return project;
            }

            return null;
        }

        private List<ComboBoxItem> GetProjectFiles(string projectName)
        {
            List<ComboBoxItem> projectFiles = new List<ComboBoxItem>();
            Project project = GetProjectByName(projectName);
            if (project == null)
                return projectFiles;

            var projectItems = project.ProjectItems;
            for (int i = 1; i < projectItems.Count; i++)
            {
                projectFiles.AddRange(GetFiles(projectItems.Item(i), String.Empty));
            }

            return projectFiles;
        }

        private List<ComboBoxItem> GetFiles(ProjectItem projectItem, string path)
        {
            List<ComboBoxItem> projectFiles = new List<ComboBoxItem>();
            if (projectItem.Kind != "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}") // VS Folder 
            {
                string ex = Path.GetExtension(projectItem.Name);
                if (string.IsNullOrEmpty(ex))
                    return projectFiles;

                if (_extensions.Contains(ex.Replace(".", String.Empty).ToUpper()))
                    projectFiles.Add(new ComboBoxItem() { Content = path + "/" + projectItem.Name, Tag = projectItem });
            }
            else
            {
                for (int i = 1; i <= projectItem.ProjectItems.Count; i++)
                {
                    projectFiles.AddRange(GetFiles(projectItem.ProjectItems.Item(i), path + "/" + projectItem.Name));
                }
            }
            return projectFiles;
        }

        private void GetConnections(Project vsProject)
        {
            Connections.Items.Clear();

            var path = Path.GetDirectoryName(vsProject.FullName);
            XmlDocument doc = new XmlDocument();

            if (!File.Exists(path + "\\WebResourceDeployer.config"))
                return;

            doc.Load(path + "\\WebResourceDeployer.config");
            XmlNodeList connections = doc.GetElementsByTagName("Connection");
            if (connections.Count == 0) return;

            List<CrmConn> crmConnections = new List<CrmConn>();

            foreach (XmlNode node in connections)
            {
                CrmConn conn = new CrmConn();
                XmlNode nameNode = node["Name"];
                if (nameNode != null)
                    conn.Name = nameNode.InnerText;
                XmlNode connectionStringNode = node["ConnectionString"];
                if (connectionStringNode != null)
                    conn.ConnectionString = DecodeString(connectionStringNode.InnerText);
                XmlNode orgIdNode = node["OrgId"];
                if (orgIdNode != null)
                    conn.OrgId = orgIdNode.InnerText;
                XmlNode versionNode = node["Version"];
                if (versionNode != null)
                    conn.Version = versionNode.InnerText;

                crmConnections.Add(conn);
            }

            Connections.ItemsSource = crmConnections;
        }

        private void WindowEventsOnWindowActivated(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus)
        {
            if (_projects == null)
                _projects = _dte.Solution.Projects;

            //No solution loaded
            if (_solution.Count == 0)
            {
                ResetForm();
                return;
            }

            //Lost focus
            if (gotFocus.Caption != "WebResourceDeployerWindow")
                return;

            Projects.IsEnabled = true;
            AddConnection.IsEnabled = true;
            Connections.IsEnabled = true;
        }

        private string EncodeString(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        private string DecodeString(string value)
        {
            byte[] data = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(data);
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (Connections.SelectedItem == null) return;

            string connString = ((CrmConn)Connections.SelectedItem).ConnectionString;
            if (string.IsNullOrEmpty(connString)) return;

            GetWebResources(connString);
        }

        private void GetWebResources(string connString)
        {
            DisplayStatusMessage("Connecting to CRM and getting web resources");
            string projectName = ((ComboBoxItem)Projects.SelectedItem).Content.ToString();

            _projectFiles = new List<ComboBoxItem>();
            _projectFiles = GetProjectFiles(projectName);


            _projectFiles.Insert(0, new ComboBoxItem() { Content = String.Empty });

            CrmConnection connection = CrmConnection.Parse(connString);
            using (OrganizationService orgService = new OrganizationService(connection))
            {
                QueryExpression query = new QueryExpression
                {
                    EntityName = "webresource",
                    ColumnSet = new ColumnSet("name", "webresourcetype"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression
                            {
                                AttributeName = "ismanaged",
                                Operator = ConditionOperator.Equal,
                                Values = { false }
                            }
                        }
                    }
                };

                EntityCollection results = orgService.RetrieveMultiple(query);
                List<WebResourceItem> wrItems = new List<WebResourceItem>();
                foreach (var entity in results.Entities)
                {
                    WebResourceItem wrItem = new WebResourceItem
                    {
                        Publish = false,
                        WebResourceId = entity.Id,
                        Name = entity.GetAttributeValue<string>("name"),
                        Image = GetWebResourceImageByNumber(entity.GetAttributeValue<OptionSetValue>("webresourcetype").Value.ToString()),
                        Type = entity.GetAttributeValue<OptionSetValue>("webresourcetype").Value,
                        ProjectFiles = _projectFiles
                    };
                    wrItem.PropertyChanged += WebResourceItem_PropertyChanged;
                    wrItems.Add(wrItem);
                }

                CreateFilterList(results);
                wrItems = HandleMappings(wrItems);
                WebResourceGrid.ItemsSource = wrItems;
                WebResourceGrid.IsEnabled = true;
                WebResourceType.IsEnabled = true;
                DisplayStatusMessage(String.Empty);
            }
        }

        private void CreateFilterList(EntityCollection results)
        {
            List<string> types = new List<string>();
            foreach (var entity in results.Entities)
            {
                if (!types.Contains(entity.GetAttributeValue<OptionSetValue>("webresourcetype").Value.ToString()))
                    types.Add(entity.GetAttributeValue<OptionSetValue>("webresourcetype").Value.ToString());
            }

            List<ComboBoxItem> items = new List<ComboBoxItem>
            {
                new ComboBoxItem() {Content = string.Empty, Tag = string.Empty}
            };
            foreach (var type in types)
            {
                ComboBoxItem item = new ComboBoxItem() { Content = GetWebResourceTypeByNumber(type), Tag = type };
                items.Add(item);
            }

            WebResourceType.ItemsSource = items;
        }

        private string GetWebResourceTypeByNumber(string type)
        {
            switch (type)
            {
                case "1":
                    return "HTM/HTML";
                case "2":
                    return "CSS";
                case "3":
                    return "JS";
                case "4":
                    return "XML";
                case "5":
                    return "PNG";
                case "6":
                    return "JPG";
                case "7":
                    return "GIF";
                case "8":
                    return "XAP";
                case "9":
                    return "XSL/XSLT";
                case "10":
                    return "ICO";
            }

            return String.Empty;
        }

        private string GetWebResourceImageByNumber(string type)
        {
            switch (type)
            {
                case "1":
                    return "Resources/html_16.png";
                case "2":
                    return "Resources/css_16.png";
                case "3":
                    return "Resources/script_16.png";
                case "4":
                    return "Resources/xml_16.png";
                case "5":
                case "6":
                case "7":
                case "10":
                    return "Resources/image_16.png";
                case "8":
                    return "Resources/silverlight_16.png";
                case "9":
                    return "Resources/xslt_16.png"; ;

            }

            return String.Empty;
        }

        private List<WebResourceItem> HandleMappings(List<WebResourceItem> wrItems)
        {
            CrmConn conn = (CrmConn)Connections.SelectedItem;
            string projectName = ((ComboBoxItem)Projects.SelectedItem).Content.ToString();
            Project project = GetProjectByName(projectName);
            if (project == null)
                return new List<WebResourceItem>();

            var path = Path.GetDirectoryName(project.FullName);
            XmlDocument doc = new XmlDocument();
            doc.Load(path + "\\WebResourceDeployer.config");

            XmlNodeList mappedFiles = doc.GetElementsByTagName("File");
            List<XmlNode> nodesToRemove = new List<XmlNode>();

            foreach (WebResourceItem wrItem in wrItems)
            {
                foreach (XmlNode file in mappedFiles)
                {
                    XmlNode orgIdNode = file["OrgId"];
                    if (orgIdNode == null) continue;
                    if (orgIdNode.InnerText.ToUpper() != conn.OrgId.ToUpper()) continue;

                    XmlNode webResourceId = file["WebResourceId"];
                    if (webResourceId == null) continue;
                    if (webResourceId.InnerText.ToUpper() != wrItem.WebResourceId.ToString().ToUpper()) continue;

                    XmlNode filePartialPath = file["Path"];
                    if (filePartialPath == null) continue;

                    string filePath = Path.GetDirectoryName(project.FullName) + filePartialPath.InnerText.Replace("/", "\\");
                    if (!File.Exists(filePath))
                        nodesToRemove.Add(file);
                    else
                        wrItem.BoundFile = filePartialPath.InnerText;
                }
            }

            if (nodesToRemove.Count <= 0)
                return wrItems;

            XmlNode files = nodesToRemove[0].ParentNode;
            foreach (XmlNode xmlNode in nodesToRemove)
            {
                if (files != null && files.ParentNode != null)
                    files.RemoveChild(xmlNode);
            }
            doc.Save(path + "\\WebResourceDeployer.config");

            return wrItems;
        }

        private void WebResourceItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BoundFile")
            {
                WebResourceItem item = (WebResourceItem)sender;
                AddOrUpdateMapping(item);
            }

            if (e.PropertyName == "Publish")
            {
                List<WebResourceItem> webResources = (List<WebResourceItem>)WebResourceGrid.ItemsSource;
                if (webResources == null) return;

                Publish.IsEnabled = webResources.Count(w => w.Publish) > 0;
            }
        }

        private void AddOrUpdateMapping(WebResourceItem item)
        {
            CrmConn conn = (CrmConn)Connections.SelectedItem;
            var project = (Project)((ComboBoxItem)Projects.SelectedItem).Tag;
            var projectPath = Path.GetDirectoryName(project.FullName);
            XmlDocument doc = new XmlDocument();
            doc.Load(projectPath + "\\WebResourceDeployer.config");

            //Update or delete existing mapping
            XmlNodeList fileNodes = doc.GetElementsByTagName("File");
            if (fileNodes.Count > 0)
            {
                foreach (XmlNode node in fileNodes)
                {
                    XmlNode orgId = node["OrgId"];
                    if (orgId != null && orgId.InnerText.ToUpper() != conn.OrgId.ToUpper()) continue;

                    XmlNode webResourceId = node["WebResourceId"];
                    if (webResourceId != null && webResourceId.InnerText.ToUpper() !=
                        item.WebResourceId.ToString().ToUpper().Replace("{", String.Empty).Replace("}", String.Empty))
                        continue;

                    if (string.IsNullOrEmpty(item.BoundFile))
                    {
                        //Delete
                        var parentNode = node.ParentNode;
                        if (parentNode != null)
                            parentNode.RemoveChild(node);
                    }
                    else
                    {
                        //Update
                        XmlNode path = node["Path"];
                        if (path != null)
                            path.InnerText = item.BoundFile;
                    }

                    doc.Save(projectPath + "\\WebResourceDeployer.config");
                    return;
                }
            }

            //Create new mapping
            XmlNodeList files = doc.GetElementsByTagName("Files");
            if (files.Count > 0)
            {
                XmlNode file = doc.CreateElement("File");
                XmlNode org = doc.CreateElement("OrgId");
                org.InnerText = conn.OrgId;
                file.AppendChild(org);
                XmlNode path = doc.CreateElement("Path");
                path.InnerText = item.BoundFile;
                file.AppendChild(path);
                XmlNode webResourceId = doc.CreateElement("WebResourceId");
                webResourceId.InnerText = item.WebResourceId.ToString();
                file.AppendChild(webResourceId);
                XmlNode webResourceName = doc.CreateElement("WebResourceName");
                webResourceName.InnerText = item.Name;
                file.AppendChild(webResourceName);
                files[0].AppendChild(file);

                doc.Save(projectPath + "\\WebResourceDeployer.config");
            }
        }

        private void GetWebResource_OnClick(object sender, RoutedEventArgs e)
        {
            Guid webResourceId = new Guid(((Button)sender).CommandParameter.ToString());
            DownloadWebResource(webResourceId);
        }

        private void DownloadWebResource(Guid webResourceId)
        {
            DisplayStatusMessage("Downloading file");
            string connString = ((CrmConn)Connections.SelectedItem).ConnectionString;
            CrmConnection connection = CrmConnection.Parse(connString);
            using (OrganizationService orgService = new OrganizationService(connection))
            {
                Entity webResource = orgService.Retrieve("webresource", webResourceId, new ColumnSet("content", "name"));

                var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
                if (dte == null)
                    return;

                string projectName = ((ComboBoxItem)Projects.SelectedItem).Content.ToString();
                foreach (Project project in dte.Solution.Projects)
                {
                    if (project.Name != projectName) continue;

                    string[] name = webResource.GetAttributeValue<string>("name").Split('/');
                    var path = Path.GetDirectoryName(project.FullName) + "\\" + name[name.Length - 1];
                    FileInfo file = new FileInfo(path);
                    StreamWriter sw = file.CreateText();
                    sw.Write(DecodeString(webResource.GetAttributeValue<string>("content")));
                    sw.Close();

                    project.ProjectItems.AddFromFile(path);
                }
            }
            DisplayStatusMessage(String.Empty);
        }

        private void DisplayStatusMessage(string message)
        {
            _dte.StatusBar.Text = message + ((string.IsNullOrEmpty(message)) ? String.Empty : "...");
        }

        private void Publish_Click(object sender, RoutedEventArgs e)
        {
            List<WebResourceItem> items = (List<WebResourceItem>)WebResourceGrid.ItemsSource;
            List<WebResourceItem> selectedItems = new List<WebResourceItem>();

            List<ProjectItem> dirtyItems = new List<ProjectItem>();
            foreach (var selectedItem in items.Where(w => w.Publish))
            {
                selectedItems.Add(selectedItem);
                ComboBoxItem item = selectedItem.ProjectFiles.FirstOrDefault(c => c.Content.ToString() == selectedItem.BoundFile);
                if (item == null) continue;

                ProjectItem projectItem = (ProjectItem)item.Tag;
                if (projectItem.IsDirty)
                    dirtyItems.Add(projectItem);
            }

            if (dirtyItems.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("Save items and publish?", "Unsaved Items",
                            MessageBoxButton.YesNo);

                //Update existing connection
                if (result != MessageBoxResult.Yes)
                    return;

                foreach (var projectItem in dirtyItems)
                {
                    projectItem.Save();
                }
            }

            UpdateWebResources(selectedItems);
        }

        private void UpdateWebResources(List<WebResourceItem> items)
        {
            //TODO: Handle CRM 2011 w/o execute multiple
            ExecuteMultipleRequest emRequest = new ExecuteMultipleRequest
            {
                Requests = new OrganizationRequestCollection(),
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = false,
                    ReturnResponses = true
                }
            };

            OrganizationRequestCollection requests = new OrganizationRequestCollection();

            string projectName = ((ComboBoxItem)Projects.SelectedItem).Content.ToString();
            Project project = GetProjectByName(projectName);
            if (project == null)
                return;

            string publishXml = "<importexportxml><webresources>";
            foreach (var webResourceItem in items)
            {
                Entity webResource = new Entity("webresource") { Id = webResourceItem.WebResourceId };

                string filePath = Path.GetDirectoryName(project.FullName) + webResourceItem.BoundFile.Replace("/", "\\");
                if (!File.Exists(filePath)) continue;

                string content = File.ReadAllText(filePath);
                webResource["content"] = EncodeString(content);

                UpdateRequest request = new UpdateRequest { Target = webResource };
                requests.Add(request);

                publishXml += "<webresource>{" + webResource.Id + "}</webresource>";
            }
            publishXml += "</webresources></importexportxml>";

            PublishXmlRequest pubRequest = new PublishXmlRequest { ParameterXml = publishXml };
            requests.Add(pubRequest);
            emRequest.Requests = requests;

            string connString = ((CrmConn)Connections.SelectedItem).ConnectionString;
            CrmConnection connection = CrmConnection.Parse(connString);
            using (OrganizationService orgService = new OrganizationService(connection))
            {
                DisplayStatusMessage("Updating & publishing web resources");
                ExecuteMultipleResponse emResponse = (ExecuteMultipleResponse)orgService.Execute(emRequest);

                foreach (var responseItem in emResponse.Responses)
                {
                    if (responseItem.Fault == null) continue;

                    //Some error - do something
                    //TODO: handle error
                    DisplayStatusMessage(String.Empty);
                    return;
                }

                MessageBox.Show("Published");//change to status message that goes away itself
                DisplayStatusMessage(String.Empty);
            }
        }

        private void Connections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CrmConn conn = (CrmConn)Connections.SelectedItem;
            if (conn != null)
            {
                Connect.IsEnabled = !string.IsNullOrEmpty(conn.Name);
                ModifyConnection.IsEnabled = !string.IsNullOrEmpty(conn.Name);
            }
            else
            {
                Connect.IsEnabled = false;
                ModifyConnection.IsEnabled = false;
            }

            WebResourceGrid.ItemsSource = null;
            WebResourceType.ItemsSource = null;
            WebResourceType.Items.Clear();
            WebResourceType.IsEnabled = false;
            Publish.IsEnabled = false;
            WebResourceGrid.IsEnabled = false;
        }

        private void WebResourceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)WebResourceType.SelectedItem;
            if (selectedItem == null) return;

            ICollectionView cv = CollectionViewSource.GetDefaultView(WebResourceGrid.ItemsSource);
            if (string.IsNullOrEmpty(selectedItem.Tag.ToString()))
                cv.Filter = null;
            else
            {
                cv.Filter = o =>
                {
                    WebResourceItem w = o as WebResourceItem;
                    return w != null && (w.Type.ToString() == selectedItem.Tag.ToString());
                };
            }
        }

        private void Projects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //No solution loaded
            if (_solution.Count == 0)
                return;

            WebResourceGrid.ItemsSource = null;
            WebResourceType.ItemsSource = null;
            WebResourceType.Items.Clear();
            WebResourceType.IsEnabled = false;
            Publish.IsEnabled = false;
            WebResourceGrid.IsEnabled = false;

            ComboBoxItem item = (ComboBoxItem)Projects.SelectedItem;
            if (item == null) return;
            if (string.IsNullOrEmpty(item.Content.ToString())) return;

            Project project = (Project)((ComboBoxItem)Projects.SelectedItem).Tag;
            GetConnections(project);
        }

        private void ModifyConnection_Click(object sender, RoutedEventArgs e)
        {
            if (Connections.SelectedItem == null) return;

            string connString = ((CrmConn)Connections.SelectedItem).ConnectionString;
            if (string.IsNullOrEmpty(connString)) return;

            string name = ((CrmConn)Connections.SelectedItem).Name;
            Connection connection = new Connection(name, connString);
            bool? result = connection.ShowDialog();

            if (!result.HasValue || !result.Value) return;

            var project = (Project)((ComboBoxItem)Projects.SelectedItem).Tag;
            var configExists = ConfigFileExists(project);
            if (!configExists)
                CreateConfigFile(project);

            AddOrUpdateConnection(project, connection.ConnectionName, connection.ConnectionString, connection.OrgId, connection.Version, false);

            GetConnections(project);
            foreach (CrmConn conn in Connections.Items)
            {
                if (conn.Name != connection.ConnectionName) continue;

                Connections.SelectedItem = conn;
                GetWebResources(connection.ConnectionString);
                break;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
