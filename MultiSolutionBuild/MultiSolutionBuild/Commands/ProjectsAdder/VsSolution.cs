using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public class VsSolution : IVsSolutionItem, IEquatable<VsSolution>
    {
        public VsSolution(string fileName, string filePath)
        {
            Name = fileName ?? throw new ArgumentNullException(nameof(fileName));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public string FilePath { get; }

        public bool Equals(VsSolution other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(FilePath, other.FilePath, StringComparison.OrdinalIgnoreCase);
        }

        public void Accept(IVsSolutionItemVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            visitor.Visit(this);
        }

        public IEnumerable<IVsSolutionItem> GetAllChildFileSystemItems()
        {
            return Enumerable.Empty<IVsSolutionItem>();
        }

        public string Name { get; }

        public VsDirectory Parent { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((VsSolution)obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
        }
    }
}
