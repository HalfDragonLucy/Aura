using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AuraDDX.Debugging;
using AuraDDX.DirectX;
using AuraDDX.Integrity;
using CommandLine;
using CommandLine.Text;
using ImageMagick;

namespace AuraDDX.Viewer
{
    /// <summary>
    /// The main viewer application class.
    /// </summary>
    public partial class Viewer : Form
    {
        private const string owner = "HalfDragonLucy";
        private const string repo = "AuraDDX";

        private static readonly ITexConv texConv = new TexConv();
        private readonly ILogging logger = new Logging("AuraDDX", FilePath.LogsPath);
        private readonly IGithub github = new Github(new Octokit.GitHubClient(new Octokit.ProductHeaderValue("AuraDDX")));
        private static string? loadedImage;

        /// <summary>
        /// Command-line options for the viewer.
        /// </summary>
        public class Options
        {
            [Value(0, MetaName = "imageFilePath", Required = false, HelpText = "Path to the image file to process.")]
            public string? ImageFilePath { get; set; }
        }

        /// <summary>
        /// Initializes the viewer.
        /// </summary>
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
                    HandleUnsupportedFormatError();
                    return;
                }

                logger.LogInformation("Starting image conversion...");

                await texConv.ConvertToAsync(imageFilePath, FilePath.TempPath, targetExtension);

                logger.LogInformation($"Image conversion completed. Result saved to: {targetFilePath}");

                LoadAndDisplayImage(targetFilePath);
            }
            catch (Exception ex)
            {
                HandleConversionError(ex);
            }
        }

        private void HandleUnsupportedFormatError()
        {
            logger.LogError("Unsupported file extension or target format.");
            logger.LogError($"Code: {ExitCode.UnsupportedFormat}");
            Environment.Exit(ExitCode.UnsupportedFormat);
            throw new ArgumentException("Please provide a valid supported picture path argument.");
        }

        private void HandleConversionError(Exception ex)
        {
            logger.LogError($"An error occurred while converting the picture: {ex.Message}");
            logger.LogError($"Code: {ExitCode.ConversionError}");
            Environment.Exit(ExitCode.ConversionError);
            throw new Exception($"An error occurred while converting the picture: {ex.Message}");
        }

        private void LoadAndDisplayImage(string imgPath)
        {
            try
            {
                logger.LogInformation($"Loading and displaying image from path: {imgPath}");

                using (var imageStream = File.OpenRead(imgPath))
                {
                    loadedImage = imgPath;
                    ImageDisplay.Image = Image.FromStream(imageStream);
                }

                logger.LogInformation("Image displayed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error displaying image: {ex.Message}");
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
                    HandleNoInternetConnection();
                }
            }
            catch (Exception ex)
            {
                HandleUpdateError(ex);
            }
        }

        private void HandleNoInternetConnection()
        {
            logger.LogWarning("Unable to check for updates due to no internet connection.");
        }

        private void HandleUpdateError(Exception ex)
        {
            logger.LogError($"An error occurred while updating the program: {ex.Message}");
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

        public void PerformGarbageCollection()
        {
            logger.LogInformation("Performing garbage collection...");

            logger.LogInformation("Disposing of displayed image...");
            ImageDisplay.Image?.Dispose();
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

        private void MenuTimer_Tick(object sender, EventArgs e)
        {
            SaveAsMenu.Enabled = loadedImage != null;
        }

        private void SaveAsPNG(object sender, EventArgs e)
        {
            try
            {
                using SaveFileDialog saveFileDialog = new();
                saveFileDialog.Filter = "PNG Files|*.png";
                saveFileDialog.Title = "Save As PNG";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (MagickImage image = new(loadedImage))
                    {
                        image.Quality = 50;
                        image.Write(saveFileDialog.FileName);
                    }

                    logger.LogInformation("Image saved as PNG successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
            }
        }

        private void SaveAsDDX(object sender, EventArgs e)
        {
            try
            {
                using SaveFileDialog saveFileDialog = new();
                saveFileDialog.Filter = "DDX Files|*.ddx";
                saveFileDialog.Title = "Save As DDX";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using MagickImage image = new(loadedImage);
                    image.Format = MagickFormat.Dds;
                    image.Write(Path.ChangeExtension(saveFileDialog.FileName, ".ddx"));
                }

                Console.WriteLine("DDS file saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
