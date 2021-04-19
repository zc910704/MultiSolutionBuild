using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Utilities
{
    public interface IDirectoryReader
    {
        bool Exists(string directoryPath);
        IEnumerable<string> GetDirectoryFilesRecursive(string directoryPath, string searchPattern);
    }

    public sealed class DirectoryReader : IDirectoryReader
    {
        private DirectoryReader()
        {
        }

        public static DirectoryReader Instance { get; } = new DirectoryReader();

        public bool Exists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public IEnumerable<string> GetDirectoryFilesRecursive(string directoryPath, string searchPattern)
        {
            return Directory.EnumerateFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
        }
    }
}
