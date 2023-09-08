﻿using Octokit;
using System.Diagnostics;

namespace AuraDDX.Integrity
{
    /// <summary>
    /// Provides methods for ensuring the integrity of the application's file structure.
    /// </summary>
    public interface IStructuration
    {
        /// <summary>
        /// Gets the base path of the application.
        /// </summary>
        string BasePath { get; }

        /// <summary>
        /// Gets the path to the 'logs' directory.
        /// </summary>
        string LogsPath { get; }

        /// <summary>
        /// Gets the path to the 'temp' directory.
        /// </summary>
        string TempPath { get; }

        /// <summary>
        /// Event raised when an error occurs during file structure initialization.
        /// </summary>
        event EventHandler<string> ErrorOccurred;

        /// <summary>
        /// Download and execute the latest release from a GitHub repo.
        /// </summary>
        /// <param name="repoOwner">The owner of the GitHub repository.</param>
        /// <param name="repoName">The name of the GitHub repository.</param>
        /// <param name="executableName">The name of the executable file in the release assets.</param>
        Task DownloadAndExecuteLatestReleaseAsync(string repoOwner, string repoName, string executableName);

        /// <summary>
        /// Asynchronously checks for a new release in a GitHub repository.
        /// </summary>
        /// <param name="repoOwner">The owner of the GitHub repository.</param>
        /// <param name="repoName">The name of the GitHub repository.</param>
        /// <returns>The tag name (version) of the latest release if found; otherwise, an empty string.</returns>
        Task<string> CheckForNewReleaseAsync(string repoOwner, string repoName);
    }
    /// <summary>
    /// Provides methods for ensuring the integrity of the application's file structure.
    /// </summary>
    public static class Structuration
    {
        public static string BasePath { get; private set; }
        public static string LogsPath { get; private set; }
        public static string TempPath { get; private set; }

        private static readonly GitHubClient GitHubClient;

        public static event EventHandler<string> ErrorOccurred = delegate { };

        static Structuration()
        {
            GitHubClient = new GitHubClient(new ProductHeaderValue("AuraDDX"));
            BasePath = AppDomain.CurrentDomain.BaseDirectory;
            LogsPath = Path.Combine(BasePath, "logs");
            TempPath = Path.Combine(BasePath, "temp");

            Initialize();
        }

        /// <summary>
        /// Initializes the application's file structure.
        /// </summary>
        public static void Initialize()
        {
            EnsureDirectoryExists(LogsPath);
            EnsureDirectoryExists(TempPath);
        }

        private static void EnsureDirectoryExists(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(null, $"Error creating directory {directoryPath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Download and execute the latest release from a GitHub repo.
        /// </summary>
        /// <param name="repoOwner">The owner of the GitHub repository.</param>
        /// <param name="repoName">The name of the GitHub repository.</param>
        /// <param name="executableName">The name of the executable file in the release assets.</param>
        public static async Task DownloadAndExecuteLatestReleaseAsync(string repoOwner, string repoName, string executableName)
        {
            string latestRelease = await CheckForNewReleaseAsync(repoOwner, repoName);

            if (latestRelease != null)
            {
                try
                {
                    var releases = await GitHubClient.Repository.Release.GetAll(repoOwner, repoName);

                    foreach (var asset in releases[0].Assets)
                    {
                        if (asset.Name.Equals(executableName, StringComparison.OrdinalIgnoreCase))
                        {
                            string downloadFilePath = Path.Combine(TempPath, asset.Name);

                            using (var httpClient = new HttpClient())
                            {
                                var assetStream = await httpClient.GetStreamAsync(asset.BrowserDownloadUrl);
                                using var fileStream = File.Create(downloadFilePath);
                                await assetStream.CopyToAsync(fileStream);
                            }

                            var process = new Process();
                            process.StartInfo.FileName = downloadFilePath;
                            process.StartInfo.UseShellExecute = true;
                            process.Start();

                            Environment.Exit(ExitCodes.Update);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(null, $"Error downloading and executing latest release: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Asynchronously checks for a new release in a GitHub repository.
        /// </summary>
        /// <param name="repoOwner">The owner of the GitHub repository.</param>
        /// <param name="repoName">The name of the GitHub repository.</param>
        /// <returns>The tag name (version) of the latest release if found; otherwise, an empty string.</returns>
        public static async Task<string> CheckForNewReleaseAsync(string repoOwner, string repoName)
        {
            try
            {
                var releases = await GitHubClient.Repository.Release.GetAll(repoOwner, repoName);

                if (releases.Count > 0)
                {
                    var latestRelease = releases[0];
                    return latestRelease.TagName;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(null, $"Error checking for new release: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Helper function to check if one version is greater than the other.
        /// </summary>
        /// <param name="version1">The first version string.</param>
        /// <param name="version2">The second version string.</param>
        /// <returns>True if version1 is greater than version2; otherwise, false.</returns>
        public static bool IsVersionGreaterThan(string version1, string version2)
        {
            string[] parts1 = version1.Split('.');
            string[] parts2 = version2.Split('.');

            if (parts1.Length != parts2.Length)
            {
                throw new ArgumentException("Version strings have different formats.");
            }

            for (int i = 0; i < parts1.Length; i++)
            {
                int part1 = int.Parse(parts1[i]);
                int part2 = int.Parse(parts2[i]);

                if (part1 != part2)
                {
                    return part1 > part2;
                }
            }

            return false; // Versions are equal
        }
    }
}
