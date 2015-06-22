using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TemplateMenuCommands
{
    public partial class ClassNamer
    {
        public string FileName { get; set; }
        public string TestType { get; set; }
        private bool _nameChanged;
        private readonly bool _isNunit;
        private readonly List<string> _fileNames;
        public ClassNamer(string type, bool isUnit, bool isNunit, string wrType, List<string> fileNames)
        {
            _isNunit = isNunit;
            _fileNames = fileNames;

            InitializeComponent();

            if (type == "WEBRESOURCE")
                Extension.Content = (wrType == "JS") ? ".js" : ".html";
            else
                Extension.Content = ".cs";

            if (!isUnit)
            {
                Unit.Visibility = Visibility.Hidden;
                Integration.Visibility = Visibility.Hidden;
                LblTest.Visibility = Visibility.Hidden;

                switch (type)
                {
                    case "PLUGIN":
                        NewFileName.Text = NameFile("PluginClass");
                        break;
                    case "WORKFLOW":
                        NewFileName.Text = NameFile("WorkflowClass");
                        break;
                    case "WEBRESOURCE":
                        NewFileName.Text = (wrType == "JS") ? NameFile("My_Script") : NameFile("My_Html");
                        break;
                }
            }
            else
                NewFileName.Text = !isNunit ? NameFile("UnitTest") : NameFile("NUnitTest");
        }

        private string NameFile(string name)
        {
            int number = 1;
            if (!_fileNames.Contains(name + number + Extension.Content, StringComparer.OrdinalIgnoreCase))
                return name + number;

            while (_fileNames.Contains(name + number + Extension.Content, StringComparer.OrdinalIgnoreCase))
            {
                number++;
            }

            return name + number;
        }

        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            FileName = NewFileName.Text;

            if (Unit.IsChecked.HasValue)
            {
                if (Unit.IsChecked.Value)
                    TestType = "Unit";
            }

            if (Integration.IsChecked.HasValue)
            {
                if (Integration.IsChecked.Value)
                    TestType = "Integration";
            }

            DialogResult = true;
            Close();
        }

        private void Unit_Checked(object sender, RoutedEventArgs e)
        {
            if (_nameChanged) return;

            if (Unit.IsChecked.HasValue)
            {
                if (Unit.IsChecked.Value)
                    NewFileName.Text = (!_isNunit ? NameFile("UnitTest") : NameFile("NUnitTest"));
            }
        }

        private void Integration_Checked(object sender, RoutedEventArgs e)
        {
            if (_nameChanged) return;

            if (Integration.IsChecked.HasValue)
            {
                if (Integration.IsChecked.Value)
                    NewFileName.Text = (!_isNunit ? NameFile("IntegrationTest") : NameFile("NIntegrationTest"));
            }
        }

        private void NewFileName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string name = NewFileName.Text;
            if (name == NameFile("UnitTest") || name == NameFile("NUnitTest") || name == NameFile("IntegrationTest") || name == NameFile("NIntegrationTest"))
                _nameChanged = false;
            else
                _nameChanged = true;
        }
    }
}
