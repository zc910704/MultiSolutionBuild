using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public interface INVsProjectHierarchy
    {
        IEnumerable<INVsProject> Projects { get; }
        IEnumerable<INVsSolutionFolder> SolutionFolders { get; }
        INVsProject AddExistingProject(string projectFilePath);
        INVsSolutionFolder AddSolutionFolder(string name);
        INVsProject GetProjectByFilePath(string projectFilePath);
        INVsSolutionFolder GetSolutionFolder(string name);
    }
}
