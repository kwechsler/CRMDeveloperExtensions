using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace JasonLattimer.WebResourceDeployer
{
    public partial class Connection
    {
        public string ConnectionName { get; set; }
        public string ConnectionString { get; set; }
        public string OrgId { get; set; }
        public string Version { get; set; }
        public Connection(string name, string connectionString)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(name))
            {
                Name.Text = name;
                ConnectionType.Visibility = Visibility.Hidden;
            }

            if (!string.IsNullOrEmpty(connectionString))
                ConnString.Text = connectionString;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Name.Text))
            {
                MessageBox.Show("Enter A Name");
                return;
            }

            if (string.IsNullOrEmpty(ConnString.Text))
            {
                MessageBox.Show("Enter A Connection String");
                return;
            }

            try
            {
                CrmConnection connection = CrmConnection.Parse(ConnString.Text);
                using (OrganizationService orgService = new OrganizationService(connection))
                {
                    WhoAmIRequest wRequest = new WhoAmIRequest();
                    WhoAmIResponse wResponse = (WhoAmIResponse)orgService.Execute(wRequest);

                    OrgId = wResponse.OrganizationId.ToString();

                    RetrieveVersionRequest vRequest = new RetrieveVersionRequest();
                    RetrieveVersionResponse vResponse = (RetrieveVersionResponse)orgService.Execute(vRequest);

                    Version = vResponse.Version;
                    ConnectionName = Name.Text;
                    ConnectionString = ConnString.Text;

                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception)
            {
                //TODO: handle error
                MessageBox.Show("Error Connecting to CRM");
            }
        }

        private void ConnectionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem) ConnectionType.SelectedItem;
            if (item == null) return;

            switch (item.Content.ToString())
            {
                case "Online using Windows Live ID":
                    ConnString.Text = "Url=https://[orgname].crm.dynamics.com; Username=[administrator@orgname.onmicrosoft.com]; Password=[password];";
                    return;
                case "On-Premises with provided user credentials":
                    ConnString.Text = "Url=http://[myserver]/[orgname]; Domain=[mydomain]; Username=[administrator]; Password=[password];";
                    return;
                case "On-Premises using Windows integrated security":
                    ConnString.Text = "Url=http://[myserver]/[orgname];";
                    return;
                case "On-Premises (IFD) with claims":
                    ConnString.Text = "Url=https://[orgname].[domain].com; Username=[administrator@domain.com]; Password=[password];";
                    return;
            }
        }
    }
}
