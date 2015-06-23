Dynamics CRM Developer Extensions

**This is still a work in progress while I work out a few issues and finish the documentation**

The goal of this project is to be an alternative to the CRM Developer Toolkit that shipped with the Dynamics CRM 2011 & 2013 SDK. Initially it will contain templates to help jump start the development process but eventually will expand to include other tooling to help streamline the process of getting and deploying web resources as well as plug-ins and custom workflow activities. Supported versions of Visual Studio will include 2012, 2013, & 2015 and will be distributed via the Visual Studio Gallery.

The project and item templates are based on the SideWaffle project

**Templates**

Plug-in - Project Template    
Plug-in Class - Item Template   
Plug-in Test - Project Template   
Plug-in Unit Test (MS Test) - Item Template   
Plug-in Integration Test (MS Test) - Item Template   
Plug-in Unit Test (NUnit) - Item Template   
Plug-in Integration Test (NUnit) - Item Template   
Custom Workflow Activity - Project Template   
Custom Workflow Activity - Item Template   
Custom Workflow Activity Test - Project Template   
Custom Workflow Activity Unit Test (MS Test) - Item Template   
Custom Workflow Activity Integration Test (MS Test) - Item Template   
Custom Workflow Activity Unit Test (NUnit) - Item Template   
Custom Workflow Activity Integration Test (NUnit) - Item Template   
Web Resource - Project Template   
JavaScript - Item Template   
HTML - Item Template   

You can take a look at how the templates will be implemented here:

https://www.youtube.com/watch?t=47&v=NV9T_0F6HM0

**Code Snippets**

Currently there are JavaScript snippets the majority of the 2011, 2013, and 2015 Dynamics CRM Client SDK. On the .NET side there are some snippets to add input and out parameters to custom workflow assemblies along with some snippets to assist in creating unit tests.

The first piece of additional tooling that is being developed is a cross between the functionality in the old Developer Toolkit which allowed you to download web resource files from CRM into your project and the Web Resource Linker/Publisher from CodePlex which allowed deploying web resources from Visual Studio.

That is still a work in progress but you can have a look at it in action here:

https://www.youtube.com/watch?v=9zZiyf1ylGE

Eventually there will be some functionality to help deploy custom assemblies from Visual Studio. I haven't thought this all the way through yet but currently I'm leaning toward still using the plug-in registration tool to do the initial deployment and creation of steps, images, etc... and then build something to update the assemblies when needed. The plug-in registration tool could be recreated inside Visual Studio but I'm not sure the effort to so would realistically save and significant amount of time.

If you have ideas for new templates or tools please post them in the issues area.
