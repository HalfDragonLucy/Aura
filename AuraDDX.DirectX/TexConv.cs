using AuraDDX.Integrity;
using System.Diagnostics;

namespace AuraDDX.DirectX
{
    /// <summary>
    /// Provides functionality to convert files using an embedded texconv.exe.
    /// </summary>
    public interface ITexConv
    {
        /// <summary>
        /// Occurs when an error occurs during conversion.
        /// </summary>
        event EventHandler<string> ErrorOccurred;

        /// <summary>
        /// Converts a file asynchronously.
        /// </summary>
        /// <param name="inputFilePath">The input file path.</param>
        /// <param name="outputDirectory">The output directory.</param>
        /// <param name="format">The output file format.</param>
        Task ConvertToAsync(string inputFilePath, string outputDirectory, FileFormat format);

        /// <summary>
        /// Initializes the TexConv component by creating the texconv.exe.
        /// </summary>
        void Initialize();
    }

    public enum FileFormat
    {
        BMP, DDS, DDX, HDR, JPG, JPEG, PFM, PNG, PPM, TGA, TIF, TIFF, WMP
    }

    public class TexConv : ITexConv
    {
        private readonly string texConvExePath;
        private bool isInitialized = false;

        public event EventHandler<string> ErrorOccurred = delegate { };

        public TexConv()
        {
            texConvExePath = Path.Combine(FilePath.TempPath, "texconv.exe");
        }

        public void Initialize()
        {
            if (!isInitialized)
            {
                File.WriteAllBytes(texConvExePath, Properties.Resources.TexConvExec);
                isInitialized = true;
            }
        }


        public async Task ConvertToAsync(string inputFilePath, string outputDirectory, FileFormat format)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            if (!IsSupported(format))
            {
                OnErrorOccurred("Unsupported file format.");
                throw new ArgumentException("Unsupported file format.", nameof(format));
            }

            try
            {
                if (!File.Exists(texConvExePath))
                {
                    OnErrorOccurred($"texconv.exe not found at {texConvExePath}");
                    throw new FileNotFoundException("texconv.exe not found.", texConvExePath);
                }

                using var texConvProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        FileName = texConvExePath,
                        Arguments = $"\"{inputFilePath}\" -ft {format.ToString().ToLower()} -y -o {outputDirectory}"
                    }
                };

                texConvProcess.Start();
                await texConvProcess.WaitForExitAsync();

                if (texConvProcess.ExitCode != 0)
                {
                    string errorMessage = $"Failed to execute texconv.exe. Exit code: {texConvProcess.ExitCode}. Error output: {await texConvProcess.StandardError.ReadToEndAsync()}";
                    OnErrorOccurred(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public static bool IsSupported(FileFormat format)
        {
            return Enum.IsDefined(typeof(FileFormat), format);
        }

        private void OnErrorOccurred(string errorMessage)
        {
            ErrorOccurred?.Invoke(this, errorMessage);
        }
    }
}
