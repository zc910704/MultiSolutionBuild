using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public sealed class FsItemBuildSolutionItemHierarchyVisitor : IVsSolutionItemVisitor
    {
        private readonly IList<IVsSolutionItem> _CurrentContextItems;
        private readonly SolutionItemsCount _ItemsCount;

        public FsItemBuildSolutionItemHierarchyVisitor()
        {
            _CurrentContextItems = new List<IVsSolutionItem>();
            _ItemsCount = new SolutionItemsCount();
        }

        private FsItemBuildSolutionItemHierarchyVisitor(VsDirectoryItem viewModel,
            SolutionItemsCount itemsCount)
        {
            _CurrentContextItems = viewModel.ChildItems;
            _ItemsCount = itemsCount;
        }

        void IVsSolutionItemVisitor.Visit(VsDirectoryItem directory)
        {
            if (directory.IsSelected == false)
            {
                return;
            }

            _ItemsCount.NumberOfSolutionFolders++;
            IVsSolutionItemVisitor childVisitor;

            if (directory.CreateSolutionFolder)
            {
                var solutionFolder = new VsDirectoryItem(directory.Name);
                _CurrentContextItems.Add(solutionFolder);

                childVisitor = new FsItemBuildSolutionItemHierarchyVisitor(solutionFolder, _ItemsCount);
            }
            else
            {
                childVisitor = this;
            }

            foreach (var directoryChild in directory.ChildItems)
            {
                directoryChild.Accept(childVisitor);
            }
        }

        void IVsSolutionItemVisitor.Visit(VsSolutionItem project)
        {
            if (project.IsSelected == false)
            {
                return;
            }

            _ItemsCount.NumberOfProjects++;
            var solutionProject = new VsSolutionItem(project.Name, project.FilePath);
            _CurrentContextItems.Add(solutionProject);
        }

        public SolutionItemHierarchy BuildSolutionItemHierarchy(IEnumerable<IVsSolutionItem> items)
        {
            foreach (var item in items)
            {
                item.Accept(this);
            }

            var hierarchy = new SolutionItemHierarchy();
            hierarchy.NumberOfItemsToCreate = _ItemsCount;
            hierarchy.SolutionItems = _CurrentContextItems;
            return hierarchy;
        }
    }
}
