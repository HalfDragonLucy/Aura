using Octokit;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace AuraDDX.Integrity
{
    public interface IGithub
    {
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

        /// <summary>
        /// Helper function to check if one version is greater than the other.
        /// </summary>
        /// <param name="version1">The first version string.</param>
        /// <param name="version2">The second version string.</param>
        /// <returns>True if version1 is greater than version2; otherwise, false.</returns>
        bool IsVersionGreaterThan(string version1, string version2);

        /// <summary>
        /// Checks if the user is connected to the internet.
        /// </summary>
        /// <returns>True if the user is connected to the internet; otherwise, false.</returns>
        bool IsConnectedToInternet();
    }

    public class Github : IGithub
    {

        private readonly GitHubClient GitHubClient;

        public event EventHandler<string> ErrorOccurred = delegate { };

        public Github(GitHubClient gitHubClient)
        {
            GitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
        }

        public async Task<string> CheckForNewReleaseAsync(string repoOwner, string repoName)
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

        public async Task DownloadAndExecuteLatestReleaseAsync(string repoOwner, string repoName, string executableName)
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
                            string downloadFilePath = Path.Combine(FilePath.TempPath, asset.Name);

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
                            process.WaitForExit();
                            File.Delete(downloadFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(null, $"Error downloading and executing latest release: {ex.Message}");
                }
            }
        }

        public bool IsVersionGreaterThan(string version1, string version2)
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

            return false;
        }

        public bool IsConnectedToInternet()
        {
            try
            {
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    if (networkInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
