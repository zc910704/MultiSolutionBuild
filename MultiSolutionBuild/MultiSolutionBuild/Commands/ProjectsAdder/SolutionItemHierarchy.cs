using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public class SolutionItemHierarchy
    {
        public SolutionItemsCount NumberOfItemsToCreate { get; set; }
        public IList<IVsSolutionItem> SolutionItems { get; set; }
    }
}
