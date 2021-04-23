using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public class SolutionItemsCount
    {
        public int NumberOfProjects { get; set; }
        public int NumberOfSolutionFolders { get; set; }
        public int TotalNumberOfItems => NumberOfProjects + NumberOfSolutionFolders;
    }
}
