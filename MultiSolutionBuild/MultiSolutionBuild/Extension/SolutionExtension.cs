using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Extension
{
    public static class SolutionExtension
    {
        public static IEnumerable<Project> GetDescendantProjects(this Solution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return solution.Projects
                /* OfType is under System.Linq namespace */
                .OfType<Project>().SelectMany(GetProjects)
                .Where(project => { ThreadHelper.ThrowIfNotOnUIThread(); return File.Exists(project.FullName); });
        }

        /// <summary>
        /// 获取子项目
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private static IEnumerable<Project> GetProjects(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (project == null) return Enumerable.Empty<Project>();
            if (project.Kind != ProjectKinds.vsProjectKindSolutionFolder) return new[] { project };
            return project.ProjectItems.OfType<ProjectItem>()
                .SelectMany(p => { ThreadHelper.ThrowIfNotOnUIThread(); return GetProjects(p.SubProject); });
        }
    }
}
