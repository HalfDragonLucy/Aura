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

                logger.LogInformation($"Current Version: {Application.ProductVersion}");
                string latestRelease = await github.CheckForNewReleaseAsync(owner, repo);
                logger.LogInformation($"Latest Version: {latestRelease}");

                if (github.IsVersionGreaterThan(latestRelease, Application.ProductVersion))
                {
                    BtnUpdate.Visible = true;
                    logger.LogInformation($"Update Available!");
                }
                else
                {
                    BtnUpdate.Visible = false;
                    logger.LogInformation($"No Update Available.");
                }


            }).Wait();
            }

            texConv.ErrorOccurred += (sender, errorMessage) =>
        {
            logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        };

            github.ErrorOccurred += (sender, errorMessage) =>
            {
                logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            };

            texConv.Initialize();
            HandleArguments(Environment.GetCommandLineArgs());
        }

        /// <summary>
        /// Handles command-line arguments passed to the application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        private void HandleArguments(string[] args)
        {
            try
            {
                if (args.Length > 1)
                {
                    ProcessAndDisplayImage(args[2]);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                logger.LogError(args.Length.ToString());
            }

        }

        /// <summary>
        /// Parses and processes an image file, converting it if the extension is ".ddx".
        /// </summary>
        /// <param name="imageFilePath">The path to the image file.</param>
        private async void ProcessAndDisplayImage(string imageFilePath)
        {
            if (!IsValidPicturePath(imageFilePath))
            {
                logger.LogError($"Invalid picture path provided. Code: {ExitCodes.InvalidArgument}");
                Environment.Exit(ExitCodes.InvalidArgument);
                throw new Exception("Please provide a valid .ddx or .png picture path argument.");
            }

            logger.LogInformation($"Converting image at path: {imageFilePath}");

            string extension = Path.GetExtension(imageFilePath).ToLower();

            switch (extension)
            {
                case ".ddx":
                    await ConvertAndDisplayImageAsync(imageFilePath);
                    logger.LogInformation("Image conversion completed successfully.");
                    break;
                case ".png":
                    using (Image img = Image.FromFile(imageFilePath))
                    {
                        DisplayImage(img);
                    }

                    logger.LogInformation("Image conversion skipped because the extension is not .ddx.");
                    break;
                default:
                    logger.LogError("Unsupported file extension. Code: " + ExitCodes.InvalidArgument);
                    break;
            }
        }

        /// <summary>
        /// Checks if the provided picture path is valid.
        /// </summary>
        /// <param name="picturePath">The path to the picture.</param>
        /// <returns>True if the path is valid; otherwise, false.</returns>
        private bool IsValidPicturePath(string picturePath)
        {
            bool isValid = !string.IsNullOrWhiteSpace(picturePath);
            string extension = Path.GetExtension(picturePath).ToLower();

            if (extension != ".ddx" && extension != ".png")
            {
                isValid = false;
                logger.LogError("Invalid picture path provided. Code: " + ExitCodes.InvalidArgument);
            }

            return isValid;
        }

        /// <summary>
        /// Converts an image file to a different format and displays it if it's a .ddx file.
        /// </summary>
        /// <param name="imageFilePath">The path to the image file.</param>
        private async Task ConvertAndDisplayImageAsync(string imageFilePath)
        {
            try
            {
                logger.LogInformation($"Converting and displaying the image: {imageFilePath}");
                logger.LogInformation($"Output directory: {FilePath.TempPath}");

                string extension = Path.GetExtension(imageFilePath).ToLower();

                switch (extension)
                {
                    case ".ddx":
                        logger.LogInformation($"Target format: {FileFormat.PNG}");

                        await Task.Run(async () =>
                        {
                            logger.LogInformation($"Image {imageFilePath}");
                            logger.LogInformation(FilePath.TempPath);

                            await texConv.ConvertToAsync(imageFilePath, FilePath.TempPath, FileFormat.PNG);

                            string targetFilePath = Path.Combine(FilePath.TempPath, $"{Path.GetFileNameWithoutExtension(imageFilePath)}.{FileFormat.PNG}");
                            logger.LogInformation($"Target file path: {targetFilePath}");

                            loadedImage = Image.FromFile(targetFilePath);

                            DisplayImage(loadedImage);
                        });
                        break;
                    default:
                        logger.LogError("Invalid picture extension. Code: " + ExitCodes.InvalidArgument);
                        return;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while converting the picture: {ex.Message}");
                logger.LogError($"Code: {ExitCodes.ConversionError}");
                Environment.Exit(ExitCodes.ConversionError);
                throw new Exception($"An error occurred while converting the picture: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays an image on the form.
        /// </summary>
        /// <param name="img">The image to display.</param>
        private void DisplayImage(Image img)
        {
            BackgroundImage = img;
            logger.LogInformation("Image displayed successfully.");
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
        private void OpenFile(object sender, EventArgs e)
        {
            switch (OpenNewFile.ShowDialog())
            {
                case DialogResult.OK:
                    ProcessAndDisplayImage(OpenNewFile.FileName);
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
