using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public interface IVsSolutionItemVisitor
    {
        void Visit(VsDirectory directory);
        void Visit(VsSolution project);
    }
}
