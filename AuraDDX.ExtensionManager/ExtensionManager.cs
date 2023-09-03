using AuraDDX.Debugging;
using AuraDDX.Integrity;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace AuraDDX.Extension
{
    /// <summary>
    /// Provides Windows-specific functionality to register and unregister file extensions for associated applications.
    /// </summary>
    public interface IExtensionManager
    {
        /// <summary>
        /// Registers a file extension with the specified application.
        /// </summary>
        /// <param name="extension">The file extension (including the dot).</param>
        /// <param name="applicationPath">The path to the associated application.</param>
        void RegisterForFileExtension(string extension, string applicationPath);

        /// <summary>
        /// Unregisters a file extension.
        /// </summary>
        /// <param name="extension">The file extension (including the dot).</param>
        void UnregisterFileExtension(string extension);

        /// <summary>
        /// Checks if a file extension exists in the Windows Registry.
        /// </summary>
        /// <param name="extension">The file extension (including the dot).</param>
        /// <returns>True if the extension exists in the Registry, false otherwise.</returns>
        bool FileExtensionExistsInRegistry(string extension);
    }

    /// <summary>
    /// Provides functionality to manage file extensions on Windows.
    /// </summary>
    public class ExtensionManager : IExtensionManager
    {
        private const uint SHCNE_ASSOCCHANGED = 0x08000000;
        private const uint SHCNF_IDLIST = 0x0000;
        private readonly ILogging logger = new Logging("Extention", Structuration.LogsPath);

        /// <summary>
        /// Registers a file extension with the specified application.
        /// </summary>
        /// <param name="extension">The file extension (including the dot).</param>
        /// <param name="applicationPath">The path to the associated application.</param>
        public void RegisterForFileExtension(string extension, string applicationPath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                logger.LogWarning("Platform is not Windows. File extension registration is not supported.");
                return;
            }

            if (!IsUserAdministrator())
            {
                logger.LogWarning("Administrator privileges required for file extension registration.");
                Environment.Exit(ExitCodes.AdminPrivilegesRequired);
                return;
            }

            logger.LogInformation($"Registering file extension: {extension} -> {applicationPath}");

            using var fileReg = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{extension}");
            fileReg.CreateSubKey("shell\\open\\command").SetValue("", $"\"{applicationPath}\" \"%1\"");

            NotifyShell();
        }

        /// <summary>
        /// Unregisters a file extension.
        /// </summary>
        /// <param name="extension">The file extension (including the dot).</param>
        public void UnregisterFileExtension(string extension)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                logger.LogWarning("Platform is not Windows. File extension unregistration is not supported.");
                return;
            }

            if (!IsUserAdministrator())
            {
                logger.LogWarning("Administrator privileges required for file extension unregistration.");
                Environment.Exit(ExitCodes.AdminPrivilegesRequired);
                return;
            }

            if (!FileExtensionExistsInRegistry(extension))
            {
                logger.LogWarning($"File extension not found in Registry: {extension}");
                return;
            }

            logger.LogInformation($"Unregistering file extension: {extension}");

            Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{extension}", false);

            NotifyShell();
        }

        /// <summary>
        /// Checks if a file extension exists in the Windows Registry.
        /// </summary>
        /// <param name="extension">The file extension (including the dot).</param>
        /// <returns>True if the extension exists in the Registry, false otherwise.</returns>
        public bool FileExtensionExistsInRegistry(string extension)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                logger.LogWarning("Platform is not Windows. Checking file extension in Registry is not supported.");
                return false;
            }

            using var key = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\{extension}");
            bool exists = key != null;

            if (exists)
            {
                logger.LogInformation($"File extension found in Registry: {extension}");
            }
            else
            {
                logger.LogInformation($"File extension not found in Registry: {extension}");
            }

            return exists;
        }

        private static bool IsUserAdministrator()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void NotifyShell()
        {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
