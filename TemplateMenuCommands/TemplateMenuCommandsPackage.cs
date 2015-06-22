using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;

namespace TemplateMenuCommands
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidMenuCommandsPkgString)]
    [ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]
    public sealed class TemplateMenuCommandsPackage : Package
    {
        private string _projectType;
        private string _testType;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public TemplateMenuCommandsPackage()
        {
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu items.
                CommandID pluginMenuCommandId1 = new CommandID(GuidList.guidMenuCommandsCmdSet, (int)PkgCmdIDList.cmdidAddItem1);
                OleMenuCommand pluginMenuItem1 = new OleMenuCommand(MenuItem1Callback, pluginMenuCommandId1);
                pluginMenuItem1.BeforeQueryStatus += menuItem1_BeforeQueryStatus;
                mcs.AddCommand(pluginMenuItem1);

                CommandID pluginMenuCommandId2 = new CommandID(GuidList.guidMenuCommandsCmdSet, (int)PkgCmdIDList.cmdidAddItem2);
                OleMenuCommand pluginMenuItem2 = new OleMenuCommand(MenuItem2Callback, pluginMenuCommandId2);
                pluginMenuItem2.BeforeQueryStatus += menuItem2_BeforeQueryStatus;
                mcs.AddCommand(pluginMenuItem2);

                CommandID pluginMenuCommandId3 = new CommandID(GuidList.guidMenuCommandsCmdSet, (int)PkgCmdIDList.cmdidAddItem3);
                OleMenuCommand pluginMenuItem3 = new OleMenuCommand(MenuItem3Callback, pluginMenuCommandId3);
                pluginMenuItem3.BeforeQueryStatus += menuItem3_BeforeQueryStatus;
                mcs.AddCommand(pluginMenuItem3);
            }
        }
        #endregion

        private void menuItem1_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand == null) return;

            //Determine if the Project -> Add Item should be displayed and which what text
            GetCrmProject();
            if (_projectType == "PLUGIN" || _projectType == "WORKFLOW")
                menuCommand.Visible = true;
            else
            {
                menuCommand.Visible = false;
                return;
            }

            if (_projectType == "PLUGIN")
            {
                if (string.IsNullOrEmpty(_testType))
                {
                    menuCommand.Text = "CRM Plug-in Class...";
                    return;
                }

                if (_testType == "UNIT")
                    menuCommand.Text = "CRM Plug-in Unit Test...";
                if (_testType == "NUNIT")
                    menuCommand.Text = "CRM Plug-in NUnit Test...";

                return;
            }

            if (_projectType == "WORKFLOW")
            {
                if (string.IsNullOrEmpty(_testType))
                {
                    menuCommand.Text = "CRM Workflow Class...";
                    return;
                }

                if (_testType == "UNIT")
                    menuCommand.Text = "CRM Workflow Unit Test...";
                if (_testType == "NUNIT")
                    menuCommand.Text = "CRM Workflow NUnit Test...";
            }
        }

        private void menuItem2_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand == null) return;

            //Determine if the Project -> Add Item should be displayed and which what text
            GetCrmProject();
            menuCommand.Visible = _projectType == "WEBRESOURCE";
        }

        private void menuItem3_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand == null) return;

            //Determine if the Project -> Add Item should be displayed and which what text
            GetCrmProject();
            menuCommand.Visible = _projectType == "WEBRESOURCE";
        }

        /// <summary>
        /// Reads the CRM project settings.
        /// </summary>
        private void GetCrmProject()
        {
            var dte = GetGlobalService(typeof(DTE)) as DTE;
            if (dte == null) return;

            Array activeSolutionProjects = (Array)dte.ActiveSolutionProjects;
            if (activeSolutionProjects == null || activeSolutionProjects.Length <= 0) return;

            var project = (Project)((Array)(dte.ActiveSolutionProjects)).GetValue(0);

            var path = Path.GetDirectoryName(project.FullName);
            if (!File.Exists(path + "\\Properties\\settings.settings")) return;

            XmlDocument doc = new XmlDocument();
            doc.Load(path + "\\Properties\\settings.settings");

            XmlNodeList settings = doc.GetElementsByTagName("Settings");
            if (settings.Count == 0) return;

            XmlNodeList appSettings = settings[0].ChildNodes;
            foreach (XmlNode node in appSettings)
            {
                if (node.Attributes != null && node.Attributes["Name"] != null)
                {
                    if (node.Attributes["Name"].Value == "CRMProjectType")
                    {
                        XmlNode value = node.FirstChild;
                        _projectType = value.InnerText;
                    }
                }

                if (node.Attributes != null && node.Attributes["Name"] != null)
                {
                    if (node.Attributes["Name"].Value == "CRMTestType")
                    {
                        XmlNode value = node.FirstChild;
                        _testType = value.InnerText;
                    }
                }
            }
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItem1Callback(object sender, EventArgs e)
        {
            bool isUnit = (_testType == "UNIT" || _testType == "NUNIT");
            bool isNunit = _testType == "NUNIT";

            var dte = GetGlobalService(typeof(DTE)) as DTE;
            if (dte == null) return;

            ProjectItem selectedProjectItem = dte.SelectedItems.Item(1).ProjectItem;
            Solution2 solution = (Solution2)dte.Application.Solution;
            Project project = (Project)((Array)(dte.ActiveSolutionProjects)).GetValue(0);

            //Get the exsting class file names so it won't get duplicated when creating a new item
            List<string> fileNames = new List<string>();
            string projectPath = (selectedProjectItem == null)
                ? Path.GetDirectoryName(project.FullName)
                : Path.GetDirectoryName(selectedProjectItem.FileNames[1]);

            if (projectPath != null)
            {
                foreach (string fullname in Directory.GetFiles(projectPath))
                {
                    string filename = Path.GetFileName(fullname);
                    if (!string.IsNullOrEmpty(filename) && filename.ToUpper().EndsWith(".CS"))
                        fileNames.Add(filename);
                }
            }

            //Display the form prompting for the class file name and unit test type
            ClassNamer namer = new ClassNamer(_projectType, isUnit, isNunit, null, fileNames);
            bool? fileNamed = namer.ShowDialog();
            if (!fileNamed.HasValue || !fileNamed.Value) return;
            string testStyle = namer.TestType;
            string templateName = String.Empty;

            //Based on the results, load the proper template in the project
            if (_projectType == "PLUGIN")
            {
                if (!isUnit)
                    templateName = "Plugin Class.csharp.zip";
                else
                {
                    if (!isNunit)
                    {
                        switch (testStyle)
                        {
                            case "Unit":
                                templateName = "Plugin Unit Test.csharp.zip";
                                break;
                            case "Integration":
                                templateName = "Plugin Integration Test.csharp.zip";
                                break;
                            default:
                                return;
                        }
                    }
                    else
                    {
                        switch (testStyle)
                        {
                            case "Unit":
                                templateName = "Plugin NUnit Test.csharp.zip";
                                break;
                            case "Integration":
                                templateName = "Plugin NUnit Integration Test.csharp.zip";
                                break;
                            default:
                                return;
                        }
                    }
                }
            }

            if (_projectType == "WORKFLOW")
            {
                if (!isUnit)
                    templateName = "Workflow Class.csharp.zip";
                else
                {
                    if (!isNunit)
                    {
                        switch (testStyle)
                        {
                            case "Unit":
                                templateName = "Workflow Unit Test.csharp.zip";
                                break;
                            case "Integration":
                                templateName = "Workflow Integration Test.csharp.zip";
                                break;
                            default:
                                return;
                        }
                    }
                    else
                    {
                        switch (testStyle)
                        {
                            case "Unit":
                                templateName = "Workflow NUnit Test.csharp.zip";
                                break;
                            case "Integration":
                                templateName = "Workflow NUnit Integration Test.csharp.zip";
                                break;
                            default:
                                return;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(templateName)) return;

            var item = solution.GetProjectItemTemplate(templateName, "CSharp");
            dte.StatusBar.Text = @"Adding class from template...";

            if (selectedProjectItem == null)
                project.ProjectItems.AddFromTemplate(item, namer.FileName);
            else
                selectedProjectItem.ProjectItems.AddFromTemplate(item, namer.FileName);
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItem2Callback(object sender, EventArgs e)
        {
            var dte = GetGlobalService(typeof(DTE)) as DTE;
            if (dte == null) return;

            ProjectItem selectedProjectItem = dte.SelectedItems.Item(1).ProjectItem;
            Solution2 solution = (Solution2)dte.Application.Solution;
            Project project = (Project)((Array)(dte.ActiveSolutionProjects)).GetValue(0);

            //Get the exsting file names so it won't get duplicated when creating a new item
            List<string> fileNames = new List<string>();
            string projectPath = (selectedProjectItem == null)
                ? Path.GetDirectoryName(project.FullName)
                : Path.GetDirectoryName(selectedProjectItem.FileNames[1]);

            if (projectPath != null)
            {
                foreach (string fullname in Directory.GetFiles(projectPath))
                {
                    string filename = Path.GetFileName(fullname);
                    if (!string.IsNullOrEmpty(filename) && (filename.ToUpper().EndsWith(".HTML") || filename.ToUpper().EndsWith(".HTM")))
                        fileNames.Add(filename);
                }
            }

            //Display the form prompting for the file name
            ClassNamer namer = new ClassNamer(_projectType, false, false, "HTML", fileNames);
            bool? fileNamed = namer.ShowDialog();
            if (!fileNamed.HasValue || !fileNamed.Value) return;
            string templateName = "HTML Web Resource.csharp.zip";

            var item = solution.GetProjectItemTemplate(templateName, "CSharp");
            dte.StatusBar.Text = @"Adding file from template...";

            if (selectedProjectItem == null)
                project.ProjectItems.AddFromTemplate(item, namer.FileName);
            else
                selectedProjectItem.ProjectItems.AddFromTemplate(item, namer.FileName);
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItem3Callback(object sender, EventArgs e)
        {
            var dte = GetGlobalService(typeof(DTE)) as DTE;
            if (dte == null) return;

            ProjectItem selectedProjectItem = dte.SelectedItems.Item(1).ProjectItem;
            Solution2 solution = (Solution2)dte.Application.Solution;
            Project project = (Project)((Array)(dte.ActiveSolutionProjects)).GetValue(0);

            //Get the exsting file names so it won't get duplicated when creating a new item
            List<string> fileNames = new List<string>();

            string projectPath = (selectedProjectItem == null)
                ? Path.GetDirectoryName(project.FullName)
                : Path.GetDirectoryName(selectedProjectItem.FileNames[1]);

            if (projectPath != null)
            {
                foreach (string fullname in Directory.GetFiles(projectPath))
                {
                    string filename = Path.GetFileName(fullname);
                    if (!string.IsNullOrEmpty(filename) && filename.ToUpper().EndsWith(".JS"))
                        fileNames.Add(filename);
                }
            }

            //Display the form prompting for the file name
            ClassNamer namer = new ClassNamer(_projectType, false, false, "JS", fileNames);
            bool? fileNamed = namer.ShowDialog();
            if (!fileNamed.HasValue || !fileNamed.Value) return;
            string templateName = "JavaScript (Module) Web Resource.csharp.zip";

            var item = solution.GetProjectItemTemplate(templateName, "CSharp");
            dte.StatusBar.Text = @"Adding file from template...";

            if (selectedProjectItem == null)
                project.ProjectItems.AddFromTemplate(item, namer.FileName);
            else
                selectedProjectItem.ProjectItems.AddFromTemplate(item, namer.FileName);
        }
    }
}
