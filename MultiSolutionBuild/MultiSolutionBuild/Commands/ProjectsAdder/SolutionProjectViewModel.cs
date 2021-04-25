using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public class SolutionProjectViewModel : ISolutionItemViewModel
    {

        public SolutionProjectViewModel( string projectName,  string projectFilePath)
        {
            Name = projectName ?? throw new ArgumentNullException(nameof(projectName));
            ProjectFilePath = projectFilePath ?? throw new ArgumentNullException(nameof(projectFilePath));
            CreateStatus = SolutionItemCreateStatus.Pending;
        }

        public SolutionItemCreateStatus CreateStatus { get; set; }

        public string ProjectFilePath { get; set; }

        public string Name { get; }
    }
}
