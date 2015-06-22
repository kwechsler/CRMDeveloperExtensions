using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TemplateWizards
{
    public partial class SdkProjectPicker
    {
        public string Version { get; set; }
        public string Project { get; set; }
        public bool Nunit { get; set; }
        private readonly string _defaultSdkVersion;

        public SdkProjectPicker(bool isUnitTest, List<ComboBoxItem> projects, string defaultSdkVersion)
        {
            _defaultSdkVersion = defaultSdkVersion;

            InitializeComponent();

            if (isUnitTest)
            {
                ProjectLabel.Visibility = Visibility.Visible;
                ProjectList.Visibility = Visibility.Visible;
                UseNUnit.Visibility = Visibility.Visible;

                foreach (var itemProject in projects)
                {
                    ProjectList.Items.Add(itemProject);
                }
                if (ProjectList.Items.Count > 1)
                    ((ComboBoxItem)ProjectList.Items[1]).IsSelected = true;
            }
            else
            {
                ProjectLabel.Visibility = Visibility.Hidden;
                ProjectList.Visibility = Visibility.Hidden;
            }

            SetSdkVersion();
            ComboBoxItem itemSdk = (ComboBoxItem)SdkVersion.SelectedItem;
            Version = itemSdk.Content.ToString();
        }

        private void SetSdkVersion()
        {
            SdkVersion.IsEnabled = true;

            //If no project is selected use the default SDK version or the first item in the list
            //If a project is selected use the same SDK version
            ComboBoxItem projectItem = (ComboBoxItem)ProjectList.SelectedItem;
            if (projectItem == null || string.IsNullOrEmpty(projectItem.Content.ToString()))
            {
                foreach (ComboBoxItem item in SdkVersion.Items)
                {
                    if (item.Content.ToString() == _defaultSdkVersion)
                        SdkVersion.SelectedItem = item;
                }

                if (SdkVersion.SelectedItem == null)
                    SdkVersion.SelectedIndex = SdkVersion.Items.Count - 1;
            }
            else
            {
                var tag = projectItem.Tag;
                if (tag == null) return;

                string version = projectItem.Tag.ToString();
                if (string.IsNullOrEmpty(version)) return;

                foreach (ComboBoxItem item in SdkVersion.Items)
                {
                    if (item.Content.ToString() != version) continue;

                    SdkVersion.SelectedItem = item;
                    SdkVersion.IsEnabled = false;
                    break;
                }
            }
        }

        private void CreateTemplate_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem itemVersion = (ComboBoxItem)SdkVersion.SelectedItem;
            Version = itemVersion.Content.ToString();

            if (ProjectList.Visibility == Visibility.Visible)
            {
                ComboBoxItem itemProject = (ComboBoxItem)ProjectList.SelectedItem;
                Project = itemProject != null ? itemProject.Content.ToString() : null;
            }
            else
                Project = null;

            Nunit = UseNUnit.IsChecked.HasValue && UseNUnit.IsChecked.Value;

            DialogResult = true;
            Close();
        }

        private void ProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetSdkVersion();
        }
    }
}