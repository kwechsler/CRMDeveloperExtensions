using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace $rootnamespace$
{
    [TestFixture]
    public class $fileinputname$
    {
        #region Class Constructor
        private readonly string _namespaceClassAssembly;
        public $fileinputname$()
        {
            //[Namespace.class name, assembly name] for the class/assembly being tested
            //Namespace and class name can be found on the class file being tested
            //Assembly name can be found under the project properties on the Application tab
            _namespaceClassAssembly = "$fullclassname$" + ", " + "$assemblyname$";
        }
        #endregion
        #region Test SetUp and TearDown
        // Use ClassSetUp to run code before running the first test in the class
        [TestFixtureSetUp] public void ClassSetUp() { }

        // Use ClassTearDown to run code after all tests in a class have run
        [TestFixtureTearDown] public void ClassTearDown()  { }

        // Use TestSetUp to run code before running each test 
        [SetUp] public void TestSetUp() { }

        // Use TestTearDown to run code after each test has run
        [TearDown] public void TestTearDown() { }
        #endregion

        [Test]
        public void TestMethod1()
        {
            //Target
            Entity targetEntity = new Entity { LogicalName = "name", Id = Guid.NewGuid() };

            #region Optional Images/Configs
            //Optional Pre/Post Images - pass to InvokePlugin
            Entity preImage = null; //new Entity { LogicalName = "name", Id = Guid.NewGuid() };
            Entity postImage = null; //new Entity { LogicalName = "name", Id = Guid.NewGuid() };

            //Optional Secure/Unsecure Configurations - pass to InvokePlugin
             string unsecureConfig = String.Empty;
            string secureConfig = String.Empty;
            #endregion

            //Expected value(s)
            const string expected = null;

            //Invoke the plug-in
            InvokePlugin(_namespaceClassAssembly, ref targetEntity, preImage, postImage, unsecureConfig, secureConfig);

            //Test(s)
            Assert.AreEqual(expected, null);
        }

        /// <summary>
        /// Invokes the plug-in.
        /// </summary>
        /// <param name="name">Namespace.Class, Assembly</param>
        /// <param name="target">The target entity</param>
        /// <param name="preImage">The pre image</param>
        /// <param name="postImage">The post image</param>
        /// <param name="secureConfig">The secure configuration</param>
        /// <param name="unsecureConfig">The unsecure configuration</param>
        private static void InvokePlugin(string name, ref Entity target, Entity preImage, Entity postImage, string unsecureConfig, string secureConfig)
        {
            var testClass = Activator.CreateInstance(Type.GetType(name), unsecureConfig, secureConfig) as IPlugin;

            var factoryMock = new Mock<IOrganizationServiceFactory>();
            var tracingServiceMock = new Mock<ITracingService>();
            var pluginContextMock = new Mock<IPluginExecutionContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            IOrganizationService service = CreateOrganizationService();

            //Organization Service Factory Mock
            factoryMock.Setup(t => t.CreateOrganizationService(It.IsAny<Guid>())).Returns(service);
            var factory = factoryMock.Object;

            //Tracing Service - Content written appears in output
            tracingServiceMock.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>(MoqExtensions.WriteTrace);
            var tracingService = tracingServiceMock.Object;

            //Parameter Collections
            ParameterCollection inputParameters = new ParameterCollection { { "Target", target } };
            ParameterCollection outputParameters = new ParameterCollection { { "id", Guid.NewGuid() } };

            //Plug-in Context Mock
            pluginContextMock.Setup(t => t.InputParameters).Returns(inputParameters);
            pluginContextMock.Setup(t => t.OutputParameters).Returns(outputParameters);
            pluginContextMock.Setup(t => t.UserId).Returns(Guid.NewGuid());
            pluginContextMock.Setup(t => t.PrimaryEntityName).Returns(target.LogicalName);
            if (preImage != null)
                pluginContextMock.Setup(t => t.PreEntityImages).Returns(new EntityImageCollection() { new KeyValuePair<string, Entity>("preImage", preImage) });
            if (postImage != null)
                pluginContextMock.Setup(t => t.PostEntityImages).Returns(new EntityImageCollection() { new KeyValuePair<string, Entity>("postImage", postImage) });

            var pluginContext = pluginContextMock.Object;

            //Service Provider Mock
            serviceProviderMock.Setup(t => t.GetService(It.Is<Type>(i => i == typeof(ITracingService)))).Returns(tracingService);
            serviceProviderMock.Setup(t => t.GetService(It.Is<Type>(i => i == typeof(IOrganizationServiceFactory)))).Returns(factory);
            serviceProviderMock.Setup(t => t.GetService(It.Is<Type>(i => i == typeof(IPluginExecutionContext)))).Returns(pluginContext);

            var serviceProvider = serviceProviderMock.Object;

            testClass.Execute(serviceProvider);
        }

        /// <summary>
        /// Creates the organization service from credentials in the App.config
        /// </summary>
        /// <returns>IOrganizationService</returns>
        private static IOrganizationService CreateOrganizationService()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString;
            if (connectionString.IndexOf("[orgname]", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new Exception("CRM connection string not set in app.config.");

            CrmConnection connection =
                CrmConnection.Parse(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString);
            
            return new OrganizationService(connection);
        }
    }
}
