using AuraDDX.Integrity;
using System.Diagnostics;
using System.Runtime.Serialization;

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
        [EnumMember(Value = ".bmp")]
        BMP,

        [EnumMember(Value = ".dds")]
        DDS,

        [EnumMember(Value = ".ddx")]
        DDX,

        [EnumMember(Value = ".hdr")]
        HDR,

        [EnumMember(Value = ".jpg")]
        JPG,

        [EnumMember(Value = ".jpeg")]
        JPEG,

        [EnumMember(Value = ".pfm")]
        PFM,

        [EnumMember(Value = ".png")]
        PNG,

        [EnumMember(Value = ".ppm")]
        PPM,

        [EnumMember(Value = ".tga")]
        TGA,

        [EnumMember(Value = ".tif")]
        TIF,

        [EnumMember(Value = ".tiff")]
        TIFF,

        [EnumMember(Value = ".wmp")]
        WMP
    }

    public static class FileFormatExtensions
    {
        public static FileFormat AsString(string value)
        {
            foreach (FileFormat format in Enum.GetValues(typeof(FileFormat)))
            {
                if (string.Equals(value, format.GetEnumMemberValue(), StringComparison.OrdinalIgnoreCase))
                {
                    return format;
                }
            }

            throw new ArgumentException($"Invalid FileFormat: {value}");
        }

        private static string GetEnumMemberValue(this FileFormat format)
        {
            var memberInfo = format.GetType().GetMember(format.ToString());
            var enumMemberAttribute = memberInfo.Length > 0 ?
                memberInfo[0].GetCustomAttributes(typeof(EnumMemberAttribute), false).FirstOrDefault() as EnumMemberAttribute : null;

            return enumMemberAttribute == null
                ? throw new InvalidOperationException($"Enum value {format} does not have an EnumMemberAttribute.")
                : enumMemberAttribute.Value ?? throw new InvalidOperationException($"Enum value {format} has a null EnumMemberAttribute value.");
        }
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
