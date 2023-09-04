using AuraDDX.Debugging;
using AuraDDX.DirectX;
using AuraDDX.Extension;
using AuraDDX.Integrity;
using CommandLine;

namespace AuraDDX.Viewer
{
    public partial class Viewer : Form
    {
        private static readonly IExtensionManager extensionManager = new ExtensionManager();
        private static readonly ITexConv texConv = new TexConv(Structuration.TexConvPath);
        private readonly ILogging logger = new Logging("Viewer", Structuration.LogsPath);

        public Viewer(string[] args)
        {
            InitializeComponent();
            InitializeApplication(args);
        }

        /// <summary>
        /// Initializes the application based on command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <summary>
        /// Initializes the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        private void InitializeApplication(string[] args)
        {
            logger.LogInformation("Initializing application.");

            if (args.Length == 1)
            {
                OpenRegisterMenu();
                return;
            }

            ParseAndConvertImage(args[1]);
        }

        /// <summary>
        /// Opens the registration menu for associating DDX files with the application.
        /// </summary>
        private void OpenRegisterMenu()
        {
            logger.LogInformation("Opening registration menu.");

            switch (MessageBox.Show("Would you like to register this program to open DDX files?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
            {
                case DialogResult.Yes:
                    logger.LogInformation("User chose to register and exit.");
                    RegisterAndExit();
                    break;
                case DialogResult.No:
                    logger.LogInformation("User chose to unregister and exit.");
                    UnregisterAndExit();
                    break;
                case DialogResult.Cancel:
                    logger.LogInformation("User chose to cancel and exit.");
                    logger.LogInformation($"Exiting application with code: {ExitCodes.Cancel}");
                    Environment.Exit(ExitCodes.Cancel);
                    return;
            }
        }

        /// <summary>
        /// Registers the application to open DDX files and exits.
        /// </summary>
        private void RegisterAndExit()
        {
            logger.LogInformation("Registering application...");

            logger.LogInformation("Registering file extension '.ddx' with the application.");
            extensionManager.RegisterForFileExtension(".ddx", Path.Combine(Structuration.BasePath, "AuraDDXViewer.exe"));
            logger.LogInformation("Registration completed successfully.");

            logger.LogInformation($"Exiting application with code: {ExitCodes.Success}");
            Environment.Exit(ExitCodes.Success);
        }

        /// <summary>
        /// Unregisters the application from opening DDX files and exits.
        /// </summary>
        private void UnregisterAndExit()
        {
            logger.LogInformation("Unregistering application...");

            logger.LogInformation("Unregistering file extension '.ddx' from the application.");
            extensionManager.UnregisterFileExtension(".ddx");
            logger.LogInformation("Unregistration completed successfully.");

            logger.LogInformation($"Exiting application with code: {ExitCodes.Success}");
            Environment.Exit(ExitCodes.Success);
        }

        /// <summary>
        /// Parses command line arguments and converts the specified image.
        /// </summary>
        /// <param name="argument">The command line argument containing the image path.</param>
        private void ParseAndConvertImage(string argument)
        {
            logger.LogInformation("Parsing and converting image from command line argument.");

            string[] args = { argument };
            var parserResult = Parser.Default.ParseArguments<CommandLineOptions>(args);

            parserResult.WithParsed(options =>
            {
                if (!IsValidPicturePath(options.PicturePath))
                {
                    logger.LogError($"Invalid picture path provided. Code: {ExitCodes.InvalidArgument}");
                    Environment.Exit(ExitCodes.InvalidArgument);
                    throw new Exception("Please provide a valid .ddx picture path argument.");
                }

                logger.LogInformation($"Converting image at path: {options.PicturePath}");
                ConvertAndDisplayImage(options.PicturePath);

                logger.LogInformation("Image conversion completed successfully.");
            });
        }



        /// <summary>
        /// Checks if the provided picture path is valid.
        /// </summary>
        /// <param name="picturePath">The path of the .ddx picture to view.</param>
        /// <returns>True if the path is valid, false otherwise.</returns>
        private bool IsValidPicturePath(string picturePath)
        {
            bool isValid = !string.IsNullOrWhiteSpace(picturePath) && Path.GetExtension(picturePath) == ".ddx";
            if (!isValid)
            {
                logger.LogError("Invalid picture path provided. Code: " + ExitCodes.InvalidArgument);
            }
            return isValid;
        }

        /// <summary>
        /// Converts and displays the specified image.
        /// </summary>
        /// <param name="picturePath">The path of the .ddx picture to convert and display.</param>
        /// <summary>
        /// Converts and displays an image from the specified picture path.
        /// </summary>
        private async void ConvertAndDisplayImage(string picturePath)
        {
            try
            {
                logger.LogInformation($"Converting and displaying the image: {picturePath}");
                logger.LogInformation($"Output directory: {Structuration.TempPath}");
                logger.LogInformation($"Target format: {FileFormat.PNG}");

                await texConv.ConvertToAsync(picturePath, Structuration.TempPath, FileFormat.PNG);

                string targetFilePath = Path.Combine(Structuration.TempPath, $"{Path.GetFileNameWithoutExtension(picturePath)}.png");
                logger.LogInformation($"Target file path: {targetFilePath}");

                BackgroundImage = Image.FromFile(targetFilePath);
                logger.LogInformation("Image displayed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while converting the picture: {ex.Message}");
                logger.LogError($"Code: {ExitCodes.ConversionError}");
                Environment.Exit(ExitCodes.ConversionError);
                throw new Exception($"An error occurred while converting the picture: {ex.Message}");
            }
        }

        private void Viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            logger.LogInformation("Viewer form closed.");
            DeletePngFilesInTempDirectory();
        }


        /// <summary>
        /// Dispose and delete all .png files in the 'temp' directory.
        /// </summary>
        public void DeletePngFilesInTempDirectory()
        {
            logger.LogInformation("Deleting .png files in 'temp' directory...");

            BackgroundImage?.Dispose();

            string[] pngFiles = Directory.GetFiles(Structuration.TempPath, "*.png");

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
        /// Checks if a file is in use by attempting to open it.
        /// </summary>
        /// <param name="filePath">The path of the file to check.</param>
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

        public class CommandLineOptions
        {
            [Value(1, MetaName = "picturePath", Required = true, HelpText = "The path of the .ddx picture to view.")]
            public string PicturePath { get; }

            public CommandLineOptions(string picturePath)
            {
                PicturePath = picturePath;
            }
        }
    }
}