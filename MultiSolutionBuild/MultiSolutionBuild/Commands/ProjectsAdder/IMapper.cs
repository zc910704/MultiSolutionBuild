using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    public interface IMapper
    {
        Task<IVsSolutionItem[]> MapFilesToViewModelAsync(string rootDirectoryPath,
            IEnumerable<string> files,
            CancellationToken cancellationToken);
    }
}
