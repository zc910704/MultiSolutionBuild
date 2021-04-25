using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public interface INVsProject
    {
        string FilePath { get; }
        string Name { get; }
        void Remove();
    }
}
