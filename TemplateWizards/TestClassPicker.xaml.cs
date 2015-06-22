using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TemplateWizards
{
    public partial class TestClassPicker
    {
        public string FullClassname { get; set; }
        public string AssemblyName { get; set; }

        public TestClassPicker(List<ComboBoxItem> classItems)
        {
            InitializeComponent();

            foreach (var itemClass in classItems)
            {
                ProjectClass.Items.Add(itemClass);
            }
            if (ProjectClass.Items.Count > 1)
                ((ComboBoxItem)ProjectClass.Items[1]).IsSelected = true;
        }

        private void CreateItem_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem itemProject = (ComboBoxItem)ProjectClass.SelectedItem;
            FullClassname = itemProject != null ? itemProject.Content.ToString() : null;
            AssemblyName = itemProject != null ? itemProject.Tag.ToString() : null;

            DialogResult = true;
            Close();
        }
    }
}