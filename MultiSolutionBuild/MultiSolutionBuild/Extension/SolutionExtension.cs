using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using MultiSolutionBuild.Commands.ProjectsAdder;
using MultiSolutionBuild.Utilities;
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

        public static VsProject GetProjectByFilePath(this Solution solution, string projectFilePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var project = solution
                .Projects
                .Cast<Project>()
                .FilterToProjects()
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
                .Where(p => string.Equals(p.FileName, projectFilePath, StringComparison.OrdinalIgnoreCase))
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
                .Select(p => new VsProject(p.Name, p.FullName))
                .SingleOrDefault();

            return project;
        }

        public static VsDirectoryItem GetSolutionFolder(this Solution solution,string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            ThreadHelper.ThrowIfNotOnUIThread();
            var solutionFolder = solution
                .Projects
                .Cast<Project>()
                .FilterToSolutionFolders()
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
                .Where(sf => string.Equals(sf.Name, name, StringComparison.OrdinalIgnoreCase))
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
                .Select(sf => new VsDirectoryItem(name))
                .SingleOrDefault();
            return solutionFolder;
        }

        public static VsDirectoryItem AddSolutionFolder(this Solution solution, string name)
        {
            var solutionFolder = solution.AddSolutionFolder(name);
            return new VsDirectoryItem(name);
        }

        public static VsProject AddExistingProject(this Solution solution, string projectFilePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var project = solution.AddFromFile(projectFilePath);
            return new VsProject(project.Name, projectFilePath);
        }
    }
}
