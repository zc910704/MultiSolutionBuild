﻿using EnvDTE;
using MultiSolutionBuild.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiSolutionBuild.Commands.ProjectsAdder
{
    internal class ProjectsAdder
    {
        private static readonly string[] DefaultProjectTypesExtensions =
        {
            "*.csproj",
            "*.fsproj",
            "*.vbproj",
            "*.shproj",
            "*.sqlproj"
        };

        private bool IsLoading { get; set; }

        public string LoadingStatus { get; set; }

        public string FolderPath { get; set; }

        private CancellationTokenSource _LoadingCancelationTokenSource;

        private readonly IFileEnumerationHelper _FileEnumerationHelper = FileEnumerationHelper.Instance;

        public static DTE DTE { get; private set; }

#pragma warning disable S1118 // Utility classes should not have public constructors
        public ProjectsAdder(DTE dte)
#pragma warning restore S1118 // Utility classes should not have public constructors
        {
            DTE = dte;
        }


        private async IEnumerable<string> LoadProjects(string folder)
        {
            if (IsLoading)
            {
                throw new InvalidOperationException("Loading already in progress.");
            }

            _LoadingCancelationTokenSource = new CancellationTokenSource();
            try
            {
                LoadingStatus = "Searching for project files started.";
                IsLoading = true;
                var progressUpdater = new Progress<int>(numberOfFoundProjects =>
                {
                    LoadingStatus = $"Searching for project files. Already found {numberOfFoundProjects} projects.";
                });
                var files = await _FileEnumerationHelper.FindFilesAsync(
                    folder, DefaultProjectTypesExtensions, _LoadingCancelationTokenSource.Token,
                    progressUpdater);
                LoadingStatus = "Searching completed. Preparing data for display.";
                return files;
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                IsLoading = false;
                _LoadingCancelationTokenSource.Dispose();
                _LoadingCancelationTokenSource = null;
            }
            return null;
        }

    }
}