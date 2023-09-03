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
        private void InitializeApplication(string[] args)
        {
            logger.LogInformation("Initializing application.");

            if (args.Length == 1)
            {
                OpenRegisterMenu();
            }
            else
            {
                ParseAndConvertImage(args[1]);
            }
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
                    RegisterAndExit();
                    break;
                case DialogResult.No:
                    UnregisterAndExit();
                    break;
                case DialogResult.Cancel:
                    Environment.Exit(ExitCodes.Cancel);
                    return;
            }
        }

        /// <summary>
        /// Registers the application to open DDX files and exits.
        /// </summary>
        private void RegisterAndExit()
        {
            logger.LogInformation("Registering application.");

            extensionManager.RegisterForFileExtension(".ddx", Path.Combine(Structuration.BasePath, "AuraDDXViewer.exe"));
            Environment.Exit(ExitCodes.Success);
        }

        /// <summary>
        /// Unregisters the application from opening DDX files and exits.
        /// </summary>
        private void UnregisterAndExit()
        {
            logger.LogInformation("Unregistering application.");

            extensionManager.UnregisterFileExtension(".ddx");
            Environment.Exit(ExitCodes.Success);
        }

        /// <summary>
        /// Parses command line arguments and converts the specified image.
        /// </summary>
        /// <param name="argument">The command line argument containing the image path.</param>
        private void ParseAndConvertImage(string argument)
        {
            string[] args = { argument };
            var parserResult = Parser.Default.ParseArguments<CommandLineOptions>(args);

            parserResult.WithParsed(options =>
            {
                if (!IsValidPicturePath(options.PicturePath))
                {
                    logger.LogError("Invalid picture path provided.");
                    Environment.Exit(ExitCodes.InvalidArgument);
                    throw new Exception("Please provide a valid .ddx picture path argument.");
                }

                ConvertAndDisplayImage(options.PicturePath);
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
                logger.LogError("Invalid picture path provided.");
            }
            return isValid;
        }

        /// <summary>
        /// Converts and displays the specified image.
        /// </summary>
        /// <param name="picturePath">The path of the .ddx picture to convert and display.</param>
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
                logger.LogInformation($"Image displayed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
                Environment.Exit(ExitCodes.ConversionError);
                throw new Exception($"An error occurred while converting the picture: {ex.Message}");
            }
        }


        private void Viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            logger.LogInformation("Viewer form closed.");
            DeletePngFilesInBinDirectory();
        }

        /// <summary>
        /// Dispose and delete all .png files in the 'bin' directory.
        /// </summary>
        private void DeletePngFilesInBinDirectory()
        {
            logger.LogInformation("Deleting .png files in 'temp' directory.");

            BackgroundImage?.Dispose();

            string[] files = Directory.GetFiles(Structuration.TempPath, "*.png");
            for (int i = 0; i < files.Length; i++)
            {
                string filePath = files[i];
                File.Delete(filePath);
            }
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