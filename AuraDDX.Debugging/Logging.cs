

namespace AuraDDX.Debugging
{
    /// <summary>
    /// Provides functionality for logging messages to a file.
    /// </summary>
    public interface ILogging
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The informational message to log.</param>
        void LogInformation(string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        void LogWarning(string message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        void LogError(string message);
    }

    /// <summary>
    /// Provides functionality for logging messages to a file.
    /// </summary>
    public class Logging : ILogging
    {
        private readonly string logDirectory;
        private readonly string logFileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logging"/> class.
        /// </summary>
        /// <param name="logDirectory">The directory where log files will be stored.</param>
        /// <param name="logFileName">The name of the log file (excluding the extension).</param>
        public Logging(string logFileName, string logDirectory)
        {
            this.logDirectory = logDirectory;
            this.logFileName = logFileName;
        }

        private void LogMessage(string logLevel, string message)
        {
            Directory.CreateDirectory(logDirectory);

            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string logFilePath = Path.Combine(logDirectory, logFileName + ".log");

            string formattedMessage = $"{timeStamp} [{logLevel}]: {message}";

            try
            {
                File.AppendAllText(logFilePath, formattedMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void LogInformation(string message)
        {
            LogMessage("INFO", message);
        }

        /// <inheritdoc/>
        public void LogWarning(string message)
        {
            LogMessage("WARNING", message);
        }

        /// <inheritdoc/>
        public void LogError(string message)
        {
            LogMessage("ERROR", message);
        }
    }
}
