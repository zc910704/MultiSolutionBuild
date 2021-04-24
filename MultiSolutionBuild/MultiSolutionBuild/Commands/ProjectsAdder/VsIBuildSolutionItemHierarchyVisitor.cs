using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public sealed class VsIBuildSolutionItemHierarchyVisitor : IVsSolutionItemVisitor
    {
        private readonly IList<IVsSolutionItem> _CurrentContextItems;
        private readonly SolutionItemsCount _ItemsCount;

        public VsIBuildSolutionItemHierarchyVisitor()
        {
            _CurrentContextItems = new List<IVsSolutionItem>();
            _ItemsCount = new SolutionItemsCount();
        }

        private VsIBuildSolutionItemHierarchyVisitor(VsDirectoryItem viewModel,
            SolutionItemsCount itemsCount)
        {
            _CurrentContextItems = viewModel.ChildItems;
            _ItemsCount = itemsCount;
        }

        void IVsSolutionItemVisitor.Visit(VsDirectoryItem directory)
        {
            _ItemsCount.NumberOfSolutionFolders++;
            IVsSolutionItemVisitor childVisitor;

            var solutionFolder = new VsDirectoryItem(directory.Name);
            _CurrentContextItems.Add(solutionFolder);

            childVisitor = new VsIBuildSolutionItemHierarchyVisitor(solutionFolder, _ItemsCount);

            foreach (var directoryChild in directory.ChildItems)
            {
                directoryChild.Accept(childVisitor);
            }
        }

        void IVsSolutionItemVisitor.Visit(VsSolutionItem project)
        {
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
