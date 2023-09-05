using AuraDDX.Debugging;
using AuraDDX.DirectX;
using AuraDDX.Extension;
using AuraDDX.Integrity;

namespace AuraDDX.Viewer
{
    /// <summary>
    /// Represents the main Viewer form for displaying images.
    /// </summary>
    public partial class Viewer : Form
    {
        private static readonly IExtensionManager extensionManager = new ExtensionManager();
        private static readonly ITexConv texConv = new TexConv(Structuration.TexConvPath);
        private readonly ILogging logger = new Logging("Viewer", Structuration.LogsPath);
        private static Image? loadedImage;

        public Viewer(string[] args)
        {
            InitializeComponent();
            HandleArguments(args);
        }

        /// <summary>
        /// Handles command-line arguments passed to the application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        private void HandleArguments(string[] args)
        {
            if (args.Length > 1)
            {
                ParseImage(args[2]);
                return;
            }
        }

        /// <summary>
        /// Registers the application for a file extension.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void RegisterAndExit(object sender, EventArgs e)
        {
            logger.LogInformation("Registering application...");

            logger.LogInformation("Registering file extension '.ddx' with the application.");
            extensionManager.RegisterForFileExtension(".ddx", Path.Combine(Structuration.BasePath, "AuraDDXViewer.exe"));
            logger.LogInformation("Registration completed successfully.");

            MessageBox.Show("Registration completed successfully.", "Register AuraDDX", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Unregisters the application for a file extension.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void UnregisterAndExit(object sender, EventArgs e)
        {
            logger.LogInformation("Unregistering application...");

            logger.LogInformation("Unregistering file extension '.ddx' from the application.");
            extensionManager.UnregisterFileExtension(".ddx");
            logger.LogInformation("Unregistration completed successfully.");

            MessageBox.Show("Unregistration completed successfully.", "Unregister AuraDDX", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Parses and processes an image file.
        /// </summary>
        /// <param name="path">The path to the image file.</param>
        private void ParseImage(string path)
        {
            if (!IsValidPicturePath(path))
            {
                logger.LogError($"Invalid picture path provided. Code: {ExitCodes.InvalidArgument}");
                Environment.Exit(ExitCodes.InvalidArgument);
                throw new Exception("Please provide a valid .ddx or .png picture path argument.");
            }

            logger.LogInformation($"Converting image at path: {path}");

            string extension = Path.GetExtension(path).ToLower();

            switch (extension)
            {
                case ".ddx":
                    ConvertImage(path);
                    logger.LogInformation("Image conversion completed successfully.");
                    break;
                case ".png":
                    Image img = Image.FromFile(path);
                    DisplayImage(img);
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
        /// Converts an image file to a different format and displays it.
        /// </summary>
        /// <param name="picturePath">The path to the image file.</param>
        private async void ConvertImage(string picturePath)
        {
            try
            {
                logger.LogInformation($"Converting and displaying the image: {picturePath}");
                logger.LogInformation($"Output directory: {Structuration.TempPath}");

                string extension = Path.GetExtension(picturePath).ToLower();

                switch (extension)
                {
                    case ".ddx":
                        logger.LogInformation($"Target format: {FileFormat.PNG}");
                        await texConv.ConvertToAsync(picturePath, Structuration.TempPath, FileFormat.PNG);

                        string targetFilePath = Path.Combine(Structuration.TempPath, $"{Path.GetFileNameWithoutExtension(picturePath)}.{FileFormat.PNG}");
                        logger.LogInformation($"Target file path: {targetFilePath}");

                        loadedImage = Image.FromFile(targetFilePath);

                        DisplayImage(loadedImage);
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
            GarbageCollection();
        }

        /// <summary>
        /// Performs garbage collection and deletes temporary files.
        /// </summary>
        public void GarbageCollection()
        {
            logger.LogInformation("Deleting .png files in 'temp' directory...");

            logger.LogInformation("Disposing of Displayed Image...");
            BackgroundImage?.Dispose();
            loadedImage?.Dispose();
            logger.LogInformation("Disposed of Displayed Image.");

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
                    GarbageCollection();
                    ParseImage(OpenNewFile.FileName);
                    break;
            }

            OpenNewFile?.Dispose();
        }
    }
}
