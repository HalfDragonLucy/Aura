using AuraDDX.Debugging;
using AuraDDX.DirectX;
using AuraDDX.Integrity;

namespace AuraDDX.Viewer
{
    /// <summary>
    /// Represents the main Viewer form for displaying images.
    /// </summary>
    public partial class Viewer : Form
    {
        private const string owner = "HalfDragonLucy";
        private const string repo = "AuraDDX";

        private static readonly ITexConv texConv = new TexConv();
        private readonly ILogging logger = new Logging("AuraDDX", FilePath.LogsPath);
        private readonly IGithub github = new Github(new Octokit.GitHubClient(new Octokit.ProductHeaderValue("AuraDDX")));
        private static Image? loadedImage;

        public Viewer()
        {
            InitializeComponent();

            CurrentVersion.Text = $"Version: {Application.ProductVersion}";

            if (github.IsConnectedToInternet())
            {
                Task.Run(async () =>
                {
                    logger.LogInformation("Checking for updates...");
                    logger.LogInformation($"Current Version: {Application.ProductVersion}");
                    string latestRelease = await github.CheckForNewReleaseAsync(owner, repo);
                    logger.LogInformation($"Latest Version: {latestRelease}");

                    if (github.IsVersionGreaterThan(latestRelease, Application.ProductVersion))
                    {
                        BtnUpdate.Visible = true;
                        logger.LogInformation("Update Available!");
                    }
                    else
                    {
                        BtnUpdate.Visible = false;
                        logger.LogInformation("No Update Available.");
                    }
                }).Wait();
            }

            texConv.ErrorOccurred += (sender, errorMessage) =>
            {
                logger.LogError($"TexConv Error: {errorMessage}");
                throw new Exception(errorMessage);
            };

            github.ErrorOccurred += (sender, errorMessage) =>
            {
                logger.LogError($"GitHub Error: {errorMessage}");
                throw new Exception(errorMessage);
            };

            texConv.Initialize();

            string[] args = Environment.GetCommandLineArgs();
            HandleArguments(args).Wait();
        }

        /// <summary>
        /// Handles command-line arguments passed to the application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        private async Task HandleArguments(string[] args)
        {
            if (args.Length == 2)
            {
                string imageFilePath = args[2];

                try
                {
                    await ProcessImage(imageFilePath);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                }
            }
        }


        /// <summary>
        /// Parses and processes an image file, converting it if it's a supported extension.
        /// </summary>
        /// <param name="imageFilePath">The path to the image file.</param>
        private async Task ProcessImage(string imageFilePath)
        {
            var ext = Path.GetExtension(imageFilePath);

            if (!IsSupportedExtension(ext))
            {
                logger.LogInformation("Image conversion skipped because the extension is not supported.");
                Environment.Exit(ExitCodes.InvalidArgument);
                throw new Exception("Please provide a valid supported picture path argument.");
            }

            logger.LogInformation($"Converting image at path: {imageFilePath}");

            await ConvertImageAsync(imageFilePath, FileFormat.PNG);
            logger.LogInformation("Image conversion completed successfully.");
        }


        /// <summary>
        /// Converts an image file to the specified format and displays it.
        /// </summary>
        /// <param name="imageFilePath">The path to the image file.</param>
        /// <param name="targetExtension">The target file extension to convert to as a string (e.g., "png").</param>
        private async Task ConvertImageAsync(string imageFilePath, FileFormat targetExtension)
        {
            try
            {
                string sourceExtension = Path.GetExtension(imageFilePath).ToLower();

                logger.LogInformation($"Converting and displaying the image: {imageFilePath}");
                logger.LogInformation($"Output directory: {FilePath.TempPath}");
                logger.LogInformation($"Target format: {targetExtension}");

                await Task.Run(async () =>
                {
                    if (!IsSupportedExtension(sourceExtension))
                    {
                        logger.LogError("Unsupported file extension or target format. Code: " + ExitCodes.InvalidArgument);
                        return;
                    }

                    logger.LogInformation($"Image {imageFilePath}");
                    logger.LogInformation(FilePath.TempPath);

                    await texConv.ConvertToAsync(imageFilePath, FilePath.TempPath, targetExtension);

                    string targetFilePath = Path.Combine(FilePath.TempPath, $"{Path.GetFileNameWithoutExtension(imageFilePath)}.{targetExtension}");
                    logger.LogInformation($"Target file path: {targetFilePath}");

                    await LoadAndDisplayImage(targetFilePath);
                });
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while converting the picture: {ex.Message}");
                logger.LogError($"Code: {ExitCodes.ConversionError}");
                Environment.Exit(ExitCodes.ConversionError);
                throw new Exception($"An error occurred while converting the picture: {ex.Message}");
            }
        }


        private static bool IsSupportedExtension(string extension)
        {
            string[] supportedExtensions = { ".ddx", ".dds", ".png", ".jpg", ".jpeg", ".bmp", ".tiff" };
            return supportedExtensions.Contains(extension);
        }



        /// <summary>
        /// Loads and displays an image from the specified file path on the form.
        /// </summary>
        /// <param name="imgPath">The file path of the image to display.</param>
        private async Task LoadAndDisplayImage(string imgPath)
        {
            try
            {
                using (var imageStream = File.OpenRead(imgPath))
                {
                    loadedImage = await Task.Run(() => Image.FromStream(imageStream));
                    ImageDisplay.Image = loadedImage;
                }
                logger.LogInformation("Image displayed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error displaying image: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the event when the Viewer form is closed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void Viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            logger.LogInformation("Viewer form closed.");
            PerformGarbageCollection();

            string texconv = Path.Combine(FilePath.TempPath, "texconv.exe");

            try
            {
                File.Delete(texconv);
                logger.LogInformation($"Deleted file: {texconv}");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError($"Unauthorized access when deleting file: {texconv}");
                logger.LogError($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while deleting file: {texconv}");
                logger.LogError($"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Performs garbage collection and deletes temporary files.
        /// </summary>
        public void PerformGarbageCollection()
        {
            logger.LogInformation("Deleting .png files in 'temp' directory...");

            logger.LogInformation("Disposing of Displayed Image...");
            BackgroundImage?.Dispose();
            loadedImage?.Dispose();
            logger.LogInformation("Disposed of Displayed Image.");

            string[] pngFiles = Directory.GetFiles(FilePath.TempPath, "*.png");

            foreach (string filePath in pngFiles)
            {
                if (IsFileInUse(filePath))
                {
                    logger.LogWarning($"Skipped file deletion due to being in use: {filePath}");
                    continue;
                }

                try
                {
                    File.Delete(filePath);
                    logger.LogInformation($"Deleted file: {filePath}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.LogError($"Unauthorized access when deleting file: {filePath}");
                    logger.LogError($"Exception: {ex.Message}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"An error occurred while deleting file: {filePath}");
                    logger.LogError($"Exception: {ex.Message}");
                }
            }

            logger.LogInformation("Deletion of .png files in 'temp' directory completed.");
        }

        /// <summary>
        /// Checks if a file is in use by another process.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>True if the file is in use; otherwise, false.</returns>
        private static bool IsFileInUse(string filePath)
        {
            try
            {
                using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        /// <summary>
        /// Opens a new image file.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private async void OpenFile(object sender, EventArgs e)
        {
            switch (OpenNewFile.ShowDialog())
            {
                case DialogResult.OK:
                    await ProcessImage(OpenNewFile.FileName);
                    PerformGarbageCollection();
                    break;
            }

            OpenNewFile?.Dispose();
        }

        private async void UpdateProgram(object sender, EventArgs e)
        {
            if (github.IsConnectedToInternet())
            {
                await github.DownloadAndExecuteLatestReleaseAsync(owner, repo, "setup.exe");
            }
        }
    }
}
