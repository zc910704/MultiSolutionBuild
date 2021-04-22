using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public class VsDirectory : IVsSolutionItem, IEquatable<VsDirectory>
    {
        public VsDirectory(string directoryName)
        {
            Name = directoryName ?? throw new ArgumentNullException(nameof(directoryName));
            ChildItems = new FsItemCollection(this);
        }
        public FsItemCollection ChildItems { get; }

        public bool Equals(VsDirectory other)
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

        public VsDirectory Parent { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((VsDirectory)obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
        }
    }
}
