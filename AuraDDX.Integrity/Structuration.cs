using AuraDDX.Debugging;

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
    }

    /// <summary>
    /// Provides methods for ensuring the integrity of the application's file structure.
    /// </summary>
    public static class Structuration
    {
        private static readonly ILogging logger;

        public static string BasePath { get; private set; }
        public static string BinPath { get; private set; }
        public static string LogsPath { get; private set; }
        public static string TexConvPath { get; private set; }
        public static string TempPath { get; private set; }

        static Structuration()
        {
            BasePath = AppDomain.CurrentDomain.BaseDirectory;
            BinPath = Path.Combine(BasePath, "bin");
            LogsPath = Path.Combine(BasePath, "logs");
            TexConvPath = Path.Combine(BinPath, "texconv.exe");
            TempPath = Path.Combine(BasePath, "temp");

            logger = new Logging("Integrity", LogsPath);

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
                Directory.CreateDirectory(directoryPath);
                logger.LogInformation($"Created directory: {directoryPath}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error creating directory {directoryPath}: {ex.Message}");
            }
        }

        private static void EnsureTexConvExists()
        {
            if (!File.Exists(TexConvPath))
            {
                logger.LogError($"Missing texconv.exe at path: {TexConvPath}");
                Environment.Exit(ExitCodes.MissingDependency);
                throw new Exception("Missing texconv.exe");
            }
        }
    }
}
