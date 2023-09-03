using AuraDDX.Debugging;
using AuraDDX.Integrity;
using System.Diagnostics;

namespace AuraDDX.DirectX
{
    /// <summary>
    /// Provides functionality to convert texture files using the texconv.exe utility.
    /// </summary>
    public interface ITexConv
    {
        /// <summary>
        /// Converts a texture file to the specified format asynchronously.
        /// </summary>
        /// <param name="inputFilePath">The path to the input texture file.</param>
        /// <param name="outputDirectory">The directory where the converted file will be saved.</param>
        /// <param name="format">The desired output file format.</param>
        Task ConvertToAsync(string inputFilePath, string outputDirectory, FileFormat format);
    }

    /// <summary>
    /// Represents the supported file formats for texture conversion.
    /// </summary>
    public enum FileFormat
    {
        BMP, DDS, DDX, HDR, JPG, JPEG, PFM, PNG, PPM, TGA, TIF, TIFF, WMP
    }

    public class TexConv : ITexConv
    {
        private readonly string texConvPath;
        private readonly ILogging logger = new Logging("DirectX", Structuration.LogsPath);

        /// <summary>
        /// Initializes a new instance of the TexConv class with the path to texconv.exe.
        /// </summary>
        /// <param name="texConvPath">The path to the texconv.exe utility.</param>
        public TexConv(string texConvPath)
        {
            if (string.IsNullOrEmpty(texConvPath))
            {
                throw new ArgumentException("The texConvPath cannot be null or empty.", nameof(texConvPath));
            }

            if (!File.Exists(texConvPath))
            {
                throw new FileNotFoundException("texconv.exe not found.", texConvPath);
            }

            this.texConvPath = texConvPath;
        }

        /// <summary>
        /// Converts a texture file to the specified format asynchronously.
        /// </summary>
        /// <param name="inputFilePath">The path to the input texture file.</param>
        /// <param name="outputDirectory">The directory where the converted file will be saved.</param>
        /// <param name="format">The desired output file format.</param>
        public async Task ConvertToAsync(string inputFilePath, string outputDirectory, FileFormat format)
        {
            if (!IsSupported(format))
            {
                logger.LogWarning($"Unsupported file format: {format}");
                throw new ArgumentException("Unsupported file format.", nameof(format));
            }

            logger.LogInformation($"Converting {inputFilePath} to {format} format.");

            await Task.Run(() =>
            {
                using var texConv = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        FileName = texConvPath,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        Arguments = $"\"{inputFilePath}\" -ft {format.ToString().ToLower()} -y -o \"{outputDirectory}\""
                    }
                };

                texConv.Start();
                texConv.WaitForExit();

                if (texConv.ExitCode != 0)
                {
                    string errorOutput = texConv.StandardError.ReadToEnd();
                    logger.LogError($"Failed to execute texconv.exe. Exit code: {texConv.ExitCode}. Error output: {errorOutput}");
                    throw new InvalidOperationException($"Failed to execute texconv.exe. Exit code: {texConv.ExitCode}. Error output: {errorOutput}");
                }

                if (!Directory.Exists(outputDirectory))
                {
                    logger.LogError($"Output directory not created: {outputDirectory}");
                    throw new FileNotFoundException("Output file not created.", outputDirectory);
                }

                logger.LogInformation($"Conversion completed. Output saved to {outputDirectory}");
            });
        }

        /// <summary>
        /// Checks if the specified file format is supported for conversion.
        /// </summary>
        /// <param name="format">The file format to check.</param>
        /// <returns>True if the format is supported; otherwise, false.</returns>
        public static bool IsSupported(FileFormat format)
        {
            return format switch
            {
                FileFormat.BMP or FileFormat.DDS or FileFormat.DDX or FileFormat.HDR or FileFormat.JPG or FileFormat.JPEG or FileFormat.PFM or FileFormat.PNG or FileFormat.PPM or FileFormat.TGA or FileFormat.TIF or FileFormat.TIFF or FileFormat.WMP => true,
                _ => false,
            };
        }
    }
}
