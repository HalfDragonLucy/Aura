using AuraDDX.Debugging;
using AuraDDX.DirectX;
using AuraDDX.Integrity;
using CommandLine;
using CommandLine.Text;

namespace AuraDDX.Viewer
{
    public partial class Viewer : Form
    {
        private const string owner = "HalfDragonLucy";
        private const string repo = "AuraDDX";

        private static readonly ITexConv texConv = new TexConv();
        private readonly ILogging logger = new Logging("AuraDDX", FilePath.LogsPath);
        private readonly IGithub github = new Github(new Octokit.GitHubClient(new Octokit.ProductHeaderValue("AuraDDX")));
        private static Image? loadedImage;

        public class Options
        {
            [Value(0, MetaName = "imageFilePath", Required = false, HelpText = "Path to the image file to process.")]
            public string? ImageFilePath { get; set; }
        }

        public Viewer()
        {
            InitializeComponent();
            InitializeVersionInfo();
            InitializeGitHub();
            InitializeTexConv();

            CheckAndUpdate();
            HandleArgumentsAsync();
        }

        private void InitializeVersionInfo()
        {
            CurrentVersion.Text = $"Version: {Application.ProductVersion}";
        }

        private void CheckAndUpdate()
        {
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
                        ShowUpdateButton();
                        logger.LogInformation("Update Available!");
                    }
                    else
                    {
                        HideUpdateButton();
                        logger.LogInformation("No Update Available.");
                    }
                }).Wait();
            }
        }

        private void InitializeTexConv()
        {
            texConv.ErrorOccurred += (sender, errorMessage) =>
            {
                logger.LogError($"TexConv Error: {errorMessage}");
                throw new Exception(errorMessage);
            };

            texConv.Initialize();
        }

        private void InitializeGitHub()
        {
            github.ErrorOccurred += (sender, errorMessage) =>
            {
                logger.LogError($"GitHub Error: {errorMessage}");
                throw new Exception(errorMessage);
            };
        }

        private void ShowUpdateButton()
        {
            BtnUpdate.Visible = true;
        }

        private void HideUpdateButton()
        {
            BtnUpdate.Visible = false;
        }

        private async Task HandleArgumentsAsync()
        {
            try
            {
                var parser = new Parser(with => with.HelpWriter = null);
                string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();

                logger.LogInformation($"Command-line arguments ({args.Length}):");
                foreach (var arg in args)
                {
                    logger.LogInformation(arg);
                }

                var parseResult = await parser.ParseArguments<Options>(args).WithParsedAsync(async options =>
                {
                    if (options.ImageFilePath != null)
                    {
                        logger.LogInformation("Command-line argument 'ImageFilePath': " + options.ImageFilePath);
                        await ProcessImageAsync(options.ImageFilePath, FileFormat.PNG);
                    }
                });

                if (parseResult.Tag == ParserResultType.NotParsed)
                {
                    HelpText helpText = HelpText.AutoBuild(parseResult);
                    logger.LogError("Argument parsing error:");
                    logger.LogError(helpText.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while handling command-line arguments: {ex.Message}");
                logger.LogError($"Code: {ExitCode.ArgumentHandlingError}");
                Environment.Exit(ExitCode.ArgumentHandlingError);
                throw new Exception($"An error occurred while handling command-line arguments: {ex.Message}");
            }
        }

        private async Task ProcessImageAsync(string imageFilePath, FileFormat targetExtension)
        {
            try
            {
                string sourceExtension = Path.GetExtension(imageFilePath).ToLower();
                string _targetExtension = $".{targetExtension.ToString().ToLower()}";
                string targetFilePath = Path.Combine(FilePath.TempPath, Path.GetFileNameWithoutExtension(imageFilePath) + _targetExtension);

                logger.LogInformation($"Processing image: {imageFilePath}");
                logger.LogInformation($"Target file: {targetFilePath}");

                if (!IsSupportedExtension(sourceExtension) || !IsSupportedExtension(_targetExtension))
                {
                    logger.LogError("Unsupported file extension or target format.");
                    logger.LogError($"Code: {ExitCode.UnsupportedFormat}");
                    Environment.Exit(ExitCode.UnsupportedFormat);
                    throw new ArgumentException("Please provide a valid supported picture path argument.");
                }

                logger.LogInformation("Starting image conversion...");

                await texConv.ConvertToAsync(imageFilePath, FilePath.TempPath, targetExtension);

                logger.LogInformation($"Image conversion completed. Result saved to: {targetFilePath}");

                LoadAndDisplayImage(targetFilePath);
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while converting the picture: {ex.Message}");
                logger.LogError($"Code: {ExitCode.ConversionError}");
                Environment.Exit(ExitCode.ConversionError);
                throw new Exception($"An error occurred while converting the picture: {ex.Message}");
            }
        }

        private void LoadAndDisplayImage(string imgPath)
        {
            try
            {
                logger.LogInformation($"Loading and displaying image from path: {imgPath}");

                using (var imageStream = File.OpenRead(imgPath))
                {
                    loadedImage = Image.FromStream(imageStream);
                    ImageDisplay.Image = loadedImage;
                }

                logger.LogInformation("Image displayed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error displaying image: {ex.Message}");
            }
        }

        public void PerformGarbageCollection()
        {
            logger.LogInformation("Performing garbage collection...");

            logger.LogInformation("Disposing of displayed image...");
            BackgroundImage?.Dispose();
            loadedImage?.Dispose();
            logger.LogInformation("Disposed of displayed image.");

            string[] pngFiles = Directory.GetFiles(FilePath.TempPath, "*.png");

            foreach (string filePath in pngFiles)
            {
                try
                {
                    if (IsFileInUse(filePath))
                    {
                        logger.LogWarning($"Skipped file deletion due to being in use: {filePath}");
                        continue;
                    }

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

            logger.LogInformation("Garbage collection completed.");
        }

        private static bool IsSupportedExtension(string extension)
        {
            string[] supportedExtensions = { ".ddx", ".dds", ".png", ".jpg", ".jpeg", ".bmp", ".tiff" };
            return supportedExtensions.Contains(extension);
        }

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

        private async void OpenFileAsync(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                logger.LogInformation($"Selected file for opening: {selectedFilePath}");

                try
                {
                    await ProcessImageAsync(selectedFilePath, FileFormat.PNG);
                    logger.LogInformation("Image processing completed successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error processing image: {ex.Message}");
                }
            }
            else
            {
                logger.LogInformation("File selection canceled.");
            }
        }

        private async void UpdateProgramAsync(object sender, EventArgs e)
        {
            try
            {
                if (github.IsConnectedToInternet())
                {
                    logger.LogInformation("Checking for program updates...");
                    await github.DownloadAndExecuteLatestReleaseAsync(owner, repo, "setup.exe");
                    logger.LogInformation("Program update completed successfully.");
                }
                else
                {
                    logger.LogWarning("Unable to check for updates due to no internet connection.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while updating the program: {ex.Message}");
            }
        }

        private void Viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            logger.LogInformation("Viewer form closed.");

            PerformGarbageCollection();

            string texconvFilePath = Path.Combine(FilePath.TempPath, "texconv.exe");

            try
            {
                if (File.Exists(texconvFilePath))
                {
                    File.Delete(texconvFilePath);
                    logger.LogInformation($"Deleted file: {texconvFilePath}");
                }
                else
                {
                    logger.LogInformation($"File not found: {texconvFilePath}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError($"Unauthorized access when deleting file: {texconvFilePath}");
                logger.LogError($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while deleting file: {texconvFilePath}");
                logger.LogError($"Exception: {ex.Message}");
            }
        }

        private async void SaveAsAsync(object sender, EventArgs e)
        {
            try
            {
                if (loadedImage != null)
                {
                    if (SaveNewFile.ShowDialog() == DialogResult.OK)
                    {
                        if (!string.IsNullOrEmpty(SaveNewFile.FileName))
                        {
                            string targetFilePath = SaveNewFile.FileName;
                            string targetExtension = Path.GetExtension(targetFilePath).ToLower();

                            if (!IsSupportedExtension(targetExtension))
                            {
                                logger.LogError("Unsupported file extension or target format.");
                                return;
                            }

                            string selectedDirectory = Path.GetDirectoryName(targetFilePath) ?? throw new InvalidOperationException("No directory provided.");
                            FileFormat format = FileFormatExtensions.AsString(targetExtension);

                            await texConv.ConvertToAsync(targetFilePath, selectedDirectory, format);
                        }
                        else
                        {
                            logger.LogError("No file name provided.");
                        }
                    }
                }
                else
                {
                    logger.LogError("No image loaded to save.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while saving the image: {ex.Message}");
            }
        }

    }
}
