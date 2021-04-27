using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public interface IVsSolutionItemVisitor
    {
        void Visit(VsDirectoryItem directory);
        void Visit(VsProject project);
    }
}
