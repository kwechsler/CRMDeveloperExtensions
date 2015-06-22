using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMSimpleConsole
{
    class Program
    {
        private static OrganizationService _orgService;
        static void Main(string[] args)
        {
            CrmConnection connection = CrmConnection.Parse(
                ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString);

            using (_orgService = new OrganizationService(connection))
            {
                //Do stuff
            }
        }
    }
}
