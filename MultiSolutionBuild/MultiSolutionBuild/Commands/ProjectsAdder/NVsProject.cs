using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public class NVsProject : INVsProject
    {
        private readonly Solution2 _Solution;
        private readonly Project _Project;

        public NVsProject(
            Solution2 solution,
            Project project)
        {
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                throw new ArgumentException("Invalid project type. Project can not be solution folder.");
            _Solution = solution;
            _Project = project;

            Name = project.Name;
            FilePath = project.FileName;
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }

        public string FilePath { get; }

        public string Name { get; }

        public void Remove()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _Solution.Remove(_Project);
        }
    }
}
