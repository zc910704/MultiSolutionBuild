using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Utilities
{
    public interface IFileEnumerationHelper
    {
        Task<IEnumerable<string>> FindFilesAsync(
            string directoryPath,
            string[] searchPatterns,
            CancellationToken cancellationToken,
            IProgress<int> numberOfFoundFilesProgress);
    }

    public class FileEnumerationHelper : IFileEnumerationHelper
    {
        private readonly IDirectoryReader _DirectoryReader;

        public FileEnumerationHelper(IDirectoryReader directoryReader)
        {
            _DirectoryReader = directoryReader ?? throw new ArgumentNullException(nameof(directoryReader));
        }

        public static FileEnumerationHelper Instance { get; }
            = new FileEnumerationHelper(DirectoryReader.Instance);

        public async Task<IEnumerable<string>> FindFilesAsync(
            string directoryPath,
            string[] searchPatterns,
            CancellationToken cancellationToken,
            IProgress<int> numberOfFoundFilesProgress)
        {
            int fileCount = 0;
            var task = Task.Run(() =>
            {
                if (!_DirectoryReader.Exists(directoryPath))
                {
                    return (IEnumerable<string>)new string[0];
                }

                return searchPatterns
                    .AsParallel()
                    .WithCancellation(cancellationToken)
                    .WithMergeOptions(ParallelMergeOptions.AutoBuffered)
                    .SelectMany(p => _DirectoryReader
                        .GetDirectoryFilesRecursive(directoryPath, p)
                        .Aggregate(new List<string>(), (list, file) =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            Interlocked.Increment(ref fileCount);
                            numberOfFoundFilesProgress.Report(fileCount);
                            list.Add(file);
                            return list;
                        })
                    )
                    .ToArray();
            }, cancellationToken);
            var items = await task;
            numberOfFoundFilesProgress.Report(fileCount);
            return items;
        }
    }
}
