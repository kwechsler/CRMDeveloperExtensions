using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Moq;
using NUnit.Framework;
using System;
using System.Activities;
using System.Collections.Generic;

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

            //Input parameters
            var inputs = new Dictionary<string, object> 
            {
                //{ "Input1", new EntityReference("entity", Guid.NewGuid()) },
                //{ "Input2", "test" }
            };

            //Expected value(s)
            const string expected = null;

            //Invoke the workflow
            var output = InvokeWorkflow(_namespaceClassAssembly, ref targetEntity, inputs, TestMethod1Setup);

            //Test(s)
            Assert.AreEqual(expected, null);
        }

        /// <summary>
        /// Modify to mock CRM Organization Service actions
        /// </summary>
        /// <param name="serviceMock">The Organization Service to mock</param>
        /// <returns>Configured Organization Service</returns>
        private static Mock<IOrganizationService> TestMethod1Setup(Mock<IOrganizationService> serviceMock)
        {
            //EntityCollection queryResult = new EntityCollection();
            ////Add created items to EntityCollection

            //serviceMock.Setup(t =>
            //    t.RetrieveMultiple(It.IsAny<QueryExpression>()))
            //    .ReturnsInOrder(queryResult);

            return serviceMock;
        }

        /// <summary>
        /// Invokes the workflow.
        /// </summary>
        /// <param name="name">Namespace.Class, Assembly</param>
        /// <param name="target">The target entity</param>
        /// <param name="inputs">The workflow input parameters</param>
        /// <param name="configuredServiceMock">The function to configure the Organization Service</param>
        /// <returns>The workflow output parameters</returns>
        private static IDictionary<string, object> InvokeWorkflow(string name, ref Entity target, Dictionary<string, object> inputs, 
            Func<Mock<IOrganizationService>, Mock<IOrganizationService>> configuredServiceMock)
        {
            var testClass = Activator.CreateInstance(Type.GetType(name)) as CodeActivity;;

            var serviceMock = new Mock<IOrganizationService>();
            var factoryMock = new Mock<IOrganizationServiceFactory>();
            var tracingServiceMock = new Mock<ITracingService>();
            var workflowContextMock = new Mock<IWorkflowContext>();

            //Apply configured Organization Service Mock
            if (configuredServiceMock != null)
                serviceMock = configuredServiceMock(serviceMock);

            IOrganizationService service = serviceMock.Object;

            //Mock workflow Context
            var workflowUserId = Guid.NewGuid();
            var workflowCorrelationId = Guid.NewGuid();
            var workflowInitiatingUserId = Guid.NewGuid();

            //Workflow Context Mock
            workflowContextMock.Setup(t => t.InitiatingUserId).Returns(workflowInitiatingUserId);
            workflowContextMock.Setup(t => t.CorrelationId).Returns(workflowCorrelationId);
            workflowContextMock.Setup(t => t.UserId).Returns(workflowUserId);
            var workflowContext = workflowContextMock.Object;

            //Organization Service Factory Mock
            factoryMock.Setup(t => t.CreateOrganizationService(It.IsAny<Guid>())).Returns(service);
            var factory = factoryMock.Object;

            //Tracing Service - Content written appears in output
            tracingServiceMock.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>(MoqExtensions.WriteTrace);
            var tracingService = tracingServiceMock.Object;

            //Parameter Collection
            ParameterCollection inputParameters = new ParameterCollection { { "Target", target } };
            workflowContextMock.Setup(t => t.InputParameters).Returns(inputParameters);

            //Workflow Invoker
            var invoker = new WorkflowInvoker(testClass);
            invoker.Extensions.Add(() => tracingService);
            invoker.Extensions.Add(() => workflowContext);
            invoker.Extensions.Add(() => factory);

            return invoker.Invoke(inputs);
        }
    }
}
