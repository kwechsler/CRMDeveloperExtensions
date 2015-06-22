using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UserOptions
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
    [PackageRegistration(UseManagedResourcesOnly = true), InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400),
        ProvideMenuResource("Menus.ctmenu", 1), Guid(GuidList.guidUserOptionsPkgString)]
    [ProvideOptionPage(typeof(OptionPageCustom), "CRM Developer Extensions", "Settings", 0, 0, true)]
    public sealed class UserOptionsPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public UserOptionsPackage()
        {
            //Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
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
            // Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

        }
        #endregion
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("1D9ECCF3-5D2F-4112-9B25-264596873DC9")]
    public class OptionPageCustom : DialogPage
    {
        private string _defaultCrmSdkVersion = "CRM 2015 (7.1.X)";
        private string _defaultProjectKeyFileName = "MyKey";

        public string DefaultCrmSdkVersion
        {
            get { return _defaultCrmSdkVersion; }
            set { _defaultCrmSdkVersion = value; }
        }

        public string DefaultProjectKeyFileName
        {
            get { return _defaultProjectKeyFileName; }
            set { _defaultProjectKeyFileName = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                OptionsControl page = new OptionsControl {DefaultCrmSdkVersion = this, DefaultProjectKeyFileName = this};
                page.Initialize();
                return page;
            }
        }
    }
}
