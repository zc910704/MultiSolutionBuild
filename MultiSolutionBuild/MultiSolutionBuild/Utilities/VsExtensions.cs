using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Utilities
{
    public static class VsExtensions
    {
        public static IEnumerable<Project> FilterToProjects(this IEnumerable<Project> projects)
        {
            return projects.Where(p => !CheckIfSolutionFolder(p));
        }

        public static IEnumerable<Project> FilterToSolutionFolders(this IEnumerable<Project> projects)
        {
            return projects.Where(CheckIfSolutionFolder);
        }

        private static bool CheckIfSolutionFolder(Project p)
        {
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            return string.Equals(p.Kind, ProjectKinds.vsProjectKindSolutionFolder, StringComparison.Ordinal);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }
    }
}
