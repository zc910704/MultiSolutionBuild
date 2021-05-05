using EnvDTE;
using MultiSolutionBuild.Extension;
using MultiSolutionBuild.Log;
using MultiSolutionBuild.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
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

        private readonly Solution _Solution;

        private readonly OutputPaneLog _OutputPaneLog;

        public ProjectsAdder(DTE dte)
        {
            DTE = dte;
            _OutputPaneLog = OutputPaneLog.GetInstance(dte);
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

                var item_in_solution = await MapFilesToVsSolutionItemAsync(folder, files, _LoadingCancelationTokenSource.Token);
                await AddProject(item_in_solution, _LoadingCancelationTokenSource.Token);
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


        private Task<IVsSolutionItem[]> MapFilesToVsSolutionItemAsync(
            string rootDirectoryPath,
            IEnumerable<string> files,
            CancellationToken cancellationToken)
        {
            return Task.Run(() => MapFilesToVsSolutionItem(rootDirectoryPath, files, cancellationToken).ToArray(), cancellationToken);
        }

        private IEnumerable<IVsSolutionItem> MapFilesToVsSolutionItem(
            string rootDirectoryPath,
            IEnumerable<string> files,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(rootDirectoryPath))
            {
                throw new ArgumentException($"Value of the {nameof(rootDirectoryPath)} must be not empty string.",
                    nameof(rootDirectoryPath));
            }

            if (files == null) throw new ArgumentNullException(nameof(files));
            return MapFilesToVssolutionItemInner(rootDirectoryPath, files, cancellationToken);
        }

#pragma warning disable S4456 // Parameter validation in yielding methods should be wrapped
        private IEnumerable<IVsSolutionItem> MapFilesToVssolutionItemInner(string rootDirectoryPath,
#pragma warning restore S4456 // Parameter validation in yielding methods should be wrapped
            IEnumerable<string> files,
            CancellationToken cancellationToken)
        {
            var directorySeparatorChars = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            var parentPathParts =
                rootDirectoryPath.Split(directorySeparatorChars, StringSplitOptions.RemoveEmptyEntries);
            VsDirectoryItem parentDirectory = null;
            foreach (var filePath in files)
            {
                if (filePath == null)
                {
                    throw new ArgumentException("One of passed file paths is null.");
                }

                if (!filePath.StartsWith(rootDirectoryPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Path {filePath} is not child of the {rootDirectoryPath}.");
                }

                var filePathParts = filePath
                    .Split(directorySeparatorChars, StringSplitOptions.RemoveEmptyEntries)
                    .Skip(parentPathParts.Length)
                    .ToArray();

                if (filePathParts.Length == 1)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePathParts[0]);
                    yield return new VsProject(fileName, filePath);
                }
                else if (filePathParts.Length > 1)
                {
                    if (parentDirectory == null)
                    {
                        var parentDirectoryName = parentPathParts[parentPathParts.Length - 1];
                        parentDirectory = new VsDirectoryItem(parentDirectoryName);
                    }

                    var processedDirectoryParent = parentDirectory;
                    for (var i = 0; i < filePathParts.Length - 2; i++)
                    {
                        var subDirectoryName = filePathParts[i];
                        var subDirectory = processedDirectoryParent
                            .ChildItems
                            .OfType<VsDirectoryItem>()
                            .SingleOrDefault(d =>
                                string.Equals(d.Name, subDirectoryName, StringComparison.OrdinalIgnoreCase));
                        if (subDirectory == null)
                        {
                            subDirectory = new VsDirectoryItem(subDirectoryName);
                            processedDirectoryParent.ChildItems.Add(subDirectory);
                        }

                        processedDirectoryParent = subDirectory;
                    }

                    var projectFileName = Path.GetFileNameWithoutExtension(filePathParts[filePathParts.Length - 1]);
                    var projectDirectory = new VsProject(projectFileName, filePath);
                    processedDirectoryParent.ChildItems.Add(projectDirectory);
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            if (parentDirectory != null)
            {
                yield return parentDirectory;
            }
        }

        private int NumberOfErrors = 0;
        private int NumberOfCreatedSolutionFolders = 0;
        private int NumberOfCreatedSolutionItems = 0;
        private int NumberOfCreatedProjects = 0;

        private async Task AddProject(IVsSolutionItem[] itemInVs, CancellationToken cancellationToken)
        {
            var itemsToProcess = new Stack<ProcessingContext>();
            FillProcessingStack(itemsToProcess, null, itemInVs);

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
                    case VsProject project:
                        CreateProject(item.Parent, project);
                        break;
                    default:
                        throw new NotSupportedException($"{item.SolutionItem.GetType()} is not supported.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                await Dispatcher.Yield(DispatcherPriority.Background);
            }
        }

        private VsDirectoryItem CreateDirectory(
          VsDirectoryItem parent, VsDirectoryItem directory)
        {
            // TODO: Add logging of the error to the output window
            try
            {
                directory.CreateStatus = SolutionItemCreateStatus.InProgress;
                var solutionFolder =
                    _Solution.GetSolutionFolder(directory.Name) ??
                    _Solution.AddSolutionFolder(directory.Name);
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

        private VsProject CreateProject(VsDirectoryItem parent, VsProject project)
        {
            // TODO: Add logging of the error to the output window
            try
            {
                project.CreateStatus = SolutionItemCreateStatus.InProgress;
                var solutionProject =
                    _Solution.GetProjectByFilePath(project.ProjectFilePath);
                if (solutionProject == null)
                {
                    try
                    {
                        solutionProject = _Solution.AddExistingProject(project.ProjectFilePath);
                    }
                    catch
                    {

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
            VsDirectoryItem parentItem,
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
                VsDirectoryItem parent,
                IVsSolutionItem solutionItem)
            {
                Parent = parent;
                SolutionItem = solutionItem;
            }

            public VsDirectoryItem Parent { get; }
            public IVsSolutionItem SolutionItem { get; }
        }
    }
}
