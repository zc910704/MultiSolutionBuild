using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using VSLangProj;
using MultiSolutionBuild.Extension;
using Task = System.Threading.Tasks.Task;
using Microsoft;
using MultiSolutionBuild.OptionPage;
using MultiSolutionBuild.Commands.ProjectsAdder;

namespace MultiSolutionBuild
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class BuiltDependencyCommand
    {

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("215a5d13-dbae-431e-a6fb-bc88958c77d6");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltDependencyCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private BuiltDependencyCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static BuiltDependencyCommand Instance
        {
            get;
            private set;
        }

        public static DTE DTE { get; private set; }



        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }


        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in BuiltDependencyCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            Instance = new BuiltDependencyCommand(package, commandService);

            DTE = await package.GetServiceAsync(typeof(DTE)) as DTE;
           /* var projects = DTE.Solution.GetDescendantProjects();

            foreach (var prj in projects)
            {
                var vsProject = ((VSProject)prj.Object);
                foreach (Reference reference in vsProject.References)
                {
                    string path = reference.Path;
                    string s = reference.Name;
                    string l = reference.SourceProject?.Name;
                    var seq = reference.BuildNumber;
                };
            }*/
        }


        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "BuiltDependencyCommand";


            // Call the Instance singleton from the UI thread is easy
            string[] solutionLocations = GeneralOptions.Instance.SolutionLocations;

            ProjectsAdder adder = new ProjectsAdder(DTE);

            adder.LoadProjects(solutionLocations[0]);
                /*System.Threading.Tasks.Task.Run(async () =>
                {
                    // Make the call to GetLiveInstanceAsync from a background thread to avoid blocking the UI thread
                    GeneralOptions options = await GeneralOptions.GetLiveInstanceAsync();
                    string message = options.Message;
                    // Do something with message
                });*/

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

    }
}
