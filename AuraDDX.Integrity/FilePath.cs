namespace AuraDDX.Integrity
{
    /// <summary>
    /// Provides methods for ensuring the integrity of the application's file structure.
    /// </summary>
    public interface IStructuration
    {
        /// <summary>
        /// Gets the base path of the application.
        /// </summary>
        string BasePath { get; }

        /// <summary>
        /// Gets the path to the 'logs' directory.
        /// </summary>
        string LogsPath { get; }

        /// <summary>
        /// Gets the path to the 'temp' directory.
        /// </summary>
        string TempPath { get; }
    }
    /// <summary>
    /// Provides methods for ensuring the integrity of the application's file structure.
    /// </summary>
    public static class FilePath
    {
        public static string BasePath { get; private set; }
        public static string LogsPath { get; private set; }
        public static string TempPath { get; private set; }


        static FilePath()
        {

            BasePath = AppDomain.CurrentDomain.BaseDirectory;
            LogsPath = Path.Combine(BasePath, "logs");
            TempPath = Path.GetTempPath();
        }
    }
}
