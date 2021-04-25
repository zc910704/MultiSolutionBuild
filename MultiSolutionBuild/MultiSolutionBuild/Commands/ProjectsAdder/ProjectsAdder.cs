using EnvDTE;
using MultiSolutionBuild.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    internal class ProjectsAdder
    {
        private static readonly string[] DefaultProjectTypesExtensions =
        {
            "*.csproj",
            "*.fsproj",
            "*.vbproj",
            "*.shproj",
            "*.sqlproj"
        };

        private bool IsLoading { get; set; }

        public string LoadingStatus { get; set; }

        public string FolderPath { get; set; }

        private CancellationTokenSource _LoadingCancelationTokenSource;

        private readonly IFileEnumerationHelper _FileEnumerationHelper = FileEnumerationHelper.Instance;

        public static DTE DTE { get; private set; }

        private readonly INVsSolution _Solution;

        public IList<IVsSolutionItem> SolutionItemHierarchy { get; }

        private readonly IMapper _ViewModelMapper = Mapper.Instance;

#pragma warning disable S1118 // Utility classes should not have public constructors
        public ProjectsAdder(DTE dte)
#pragma warning restore S1118 // Utility classes should not have public constructors
        {
            DTE = dte;
        }

        public async Task LoadProjects(string folder)
        {
            if (IsLoading)
            {
                throw new InvalidOperationException("Loading already in progress.");
            }
            _LoadingCancelationTokenSource = new CancellationTokenSource();
            try
            {
                LoadingStatus = "Searching for project files started.";
                IsLoading = true;
                var progressUpdater = new Progress<int>(numberOfFoundProjects =>
                {
                    LoadingStatus = $"Searching for project files. Already found {numberOfFoundProjects} projects.";
                });
                var files = await _FileEnumerationHelper.FindFilesAsync(
                    folder, DefaultProjectTypesExtensions, _LoadingCancelationTokenSource.Token,
                    progressUpdater);
                LoadingStatus = "Searching completed. Preparing data for display.";

                var fsItemViewModels = await _ViewModelMapper
                    .MapFilesToViewModelAsync(folder, files, _LoadingCancelationTokenSource.Token);
                await AddProject(fsItemViewModels, _LoadingCancelationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                IsLoading = false;
                _LoadingCancelationTokenSource.Dispose();
                _LoadingCancelationTokenSource = null;
            }
        }

        private int NumberOfErrors = 0;
        private int NumberOfCreatedSolutionFolders = 0;
        private int NumberOfCreatedSolutionItems = 0;
        private int NumberOfCreatedProjects = 0;

        private async Task AddProject(IVsSolutionItem[] fsItemViewModels, CancellationToken cancellationToken)
        {
            var solutionItemHierarchyBuilder = new VsIBuildSolutionItemHierarchyVisitor();
            var solutionItemHierarchy = solutionItemHierarchyBuilder.BuildSolutionItemHierarchy(fsItemViewModels);
            /*            var addProjectsProgress =
                            new AddMultipleProjectsProgressViewModel(_WindowService, _Solution, this, solutionItemHierarchy);*/
            var itemsToProcess = new Stack<ProcessingContext>();
            FillProcessingStack(itemsToProcess, _Solution, SolutionItemHierarchy);

            while (itemsToProcess.Any())
            {
                var item = itemsToProcess.Pop();
                switch (item.SolutionItem)
                {
                    case VsDirectoryItem directory:
                        var createdSolutionFolder = CreateDirectory(item.Parent, directory);
                        if (createdSolutionFolder != null)
                        {
                            FillProcessingStack(itemsToProcess, createdSolutionFolder, directory.ChildItems);
                        }

                        break;
                    case VsSolutionItem project:
                        CreateProject(item.Parent, project);
                        break;
                    default:
                        throw new NotSupportedException($"{item.SolutionItem.GetType()} is not supported.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                await Dispatcher.Yield(DispatcherPriority.Background);
            }
        }

        private INVsProjectHierarchy CreateDirectory(
          INVsProjectHierarchy parent, VsDirectoryItem directory)
        {
            // TODO: Add logging of the error to the output window
            try
            {
                directory.CreateStatus = SolutionItemCreateStatus.InProgress;
                var solutionFolder =
                    parent.GetSolutionFolder(directory.Name) ??
                    parent.AddSolutionFolder(directory.Name);
                directory.CreateStatus = SolutionItemCreateStatus.Added;
                return solutionFolder;
            }
            catch (Exception e)
            {
                directory.CreateStatus = SolutionItemCreateStatus.Failed;
                NumberOfErrors++;
                return null;
            }
            finally
            {
                NumberOfCreatedSolutionFolders++;
                NumberOfCreatedSolutionItems++;
            }
        }

        private INVsProject CreateProject(INVsProjectHierarchy parent, VsSolutionItem project)
        {
            // TODO: Add logging of the error to the output window
            try
            {
                project.CreateStatus = SolutionItemCreateStatus.InProgress;
                var solutionProject =
                    parent.GetProjectByFilePath(project.ProjectFilePath);
                if (solutionProject == null)
                {
                    try
                    {
                        solutionProject = parent.AddExistingProject(project.ProjectFilePath);
                    }
                    catch
                    {
                        var projectInDifferentLocation =
                            _Solution.GetProjectFromAnywhereInSolution(project.ProjectFilePath);
                        projectInDifferentLocation?.Remove();
                        solutionProject = parent.AddExistingProject(project.ProjectFilePath);
                    }
                }

                project.CreateStatus = SolutionItemCreateStatus.Added;
                return solutionProject;
            }
            catch (Exception e)
            {
                project.CreateStatus = SolutionItemCreateStatus.Failed;
                NumberOfErrors++;
                return null;
            }
            finally
            {
                NumberOfCreatedProjects++;
                NumberOfCreatedSolutionItems++;
            }
        }

        private void FillProcessingStack(
            Stack<ProcessingContext> itemsToProcess,
            INVsProjectHierarchy parentItem,
            IEnumerable<IVsSolutionItem> solutionItems)
        {
            foreach (var solutionItem in solutionItems)
            {
                itemsToProcess.Push(new ProcessingContext(parentItem, solutionItem));
            }
        }

        public class ProcessingContext
        {
            public ProcessingContext(
                INVsProjectHierarchy parent,
                IVsSolutionItem solutionItem)
            {
                Parent = parent;
                SolutionItem = solutionItem;
            }

            public INVsProjectHierarchy Parent { get; }
            public IVsSolutionItem SolutionItem { get; }
        }
    }
}
