using EnvDTE;
using Microsoft.VisualStudio.Shell;
using MultiSolutionBuild.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public sealed class VsDirectoryItem : IVsSolutionItem, IEquatable<VsDirectoryItem>
    {
        public SolutionItemCreateStatus CreateStatus { get; set; }
        
        public VsItemCollection ChildItems { get; }

        public VsDirectoryItem(string directoryName)
        {
            Name = directoryName ?? throw new ArgumentNullException(nameof(directoryName));
            ChildItems = new VsItemCollection(this);
        }


        public bool Equals(VsDirectoryItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                   ChildItems.Count == other.ChildItems.Count &&
                   ChildItems.All(other.ChildItems.Contains);
        }

        public void Accept(IVsSolutionItemVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            visitor.Visit(this);
        }

        public IEnumerable<IVsSolutionItem> GetAllChildFileSystemItems()
        {
            return ChildItems.Concat(ChildItems.SelectMany(c => c.GetAllChildFileSystemItems()));
        }

        public string Name { get; }

        public VsDirectoryItem Parent { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((VsDirectoryItem)obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
        }

        public VsDirectoryItem GetSolutionFolder(string name,Solution solution)
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
                .Select(sf => new VsDirectoryItem(sf.FileName))
                .SingleOrDefault();
            return solutionFolder;
        }
    }
}
