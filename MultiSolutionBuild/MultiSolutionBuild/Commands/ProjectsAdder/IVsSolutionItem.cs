using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public interface IVsSolutionItem
    {
        string Name { get; }

        VsDirectory Parent { get; set; }

        void Accept(IVsSolutionItemVisitor visitor);

        IEnumerable<IVsSolutionItem> GetAllChildFileSystemItems();
    }
}
