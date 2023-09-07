namespace AuraDDX.Integrity
{
    /// <summary>
    /// Provides functionality for managing the application's file structure integrity.
    /// </summary>
    public interface IStructuration
    {
        /// <summary>
        /// Gets the base path of the application.
        /// </summary>
        string BasePath { get; }

        /// <summary>
        /// Gets the path to the 'bin' directory.
        /// </summary>
        string BinPath { get; }

        /// <summary>
        /// Gets the path to the 'logs' directory.
        /// </summary>
        string LogsPath { get; }

        /// <summary>
        /// Gets the path to the 'texconv.exe' executable.
        /// </summary>
        string TexConvPath { get; }

        /// <summary>
        /// Gets the path to the 'temp' directory.
        /// </summary>
        string TempPath { get; }

        /// <summary>
        /// Event raised when an error occurs during file structure initialization.
        /// </summary>
        event EventHandler<string> ErrorOccurred;
    }

    /// <summary>
    /// Provides methods for ensuring the integrity of the application's file structure.
    /// </summary>
    public static class Structuration
    {
        public static string BasePath { get; private set; }
        public static string BinPath { get; private set; }
        public static string LogsPath { get; private set; }
        public static string TexConvPath { get; private set; }
        public static string TempPath { get; private set; }

        public static event EventHandler<string> ErrorOccurred = delegate { };

        static Structuration()
        {
            BasePath = AppDomain.CurrentDomain.BaseDirectory;
            BinPath = Path.Combine(BasePath, "bin");
            LogsPath = Path.Combine(BasePath, "logs");
            TexConvPath = Path.Combine(BinPath, "texconv.exe");
            TempPath = Path.Combine(BasePath, "temp");

            Initialize();
        }

        /// <summary>
        /// Initializes the application's file structure.
        /// </summary>
        private static void Initialize()
        {
            EnsureDirectoryExists(BinPath);
            EnsureDirectoryExists(LogsPath);
            EnsureDirectoryExists(TempPath);
            EnsureTexConvExists();
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

        private static void EnsureTexConvExists()
        {
            try
            {
                if (!File.Exists(TexConvPath))
                {
                    Environment.Exit(ExitCodes.MissingDependency);
                    ErrorOccurred?.Invoke(null, $"Missing texconv.exe at {TexConvPath}");
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(null, $"Error checking for texconv.exe: {ex.Message}");
            }
        }
    }
}