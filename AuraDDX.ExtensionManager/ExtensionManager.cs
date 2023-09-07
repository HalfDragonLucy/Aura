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

        /// <summary>
        /// Event raised when an error occurs.
        /// </summary>
        event EventHandler<string> ErrorOccurred;
    }

    /// <summary>
    /// Provides functionality to manage file extensions on Windows.
    /// </summary>
    public class ExtensionManager : IExtensionManager
    {
        private const uint SHCNE_ASSOCCHANGED = 0x08000000;
        private const uint SHCNF_IDLIST = 0x0000;

        public event EventHandler<string> ErrorOccurred = delegate { };

        public void RegisterForFileExtension(string extension, string applicationPath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            if (!IsUserAdministrator())
            {
                OnErrorOccurred("Administrator privileges required.");
                Environment.Exit(ExitCodes.AdminPrivilegesRequired);
                return;
            }

            try
            {
                using var fileReg = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{extension}");
                fileReg.CreateSubKey("shell\\open\\command").SetValue("", $"\"{applicationPath}\" \"%1\"");
                NotifyShell();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex.Message);
            }
        }

        public void UnregisterFileExtension(string extension)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            if (!IsUserAdministrator())
            {
                OnErrorOccurred("Administrator privileges required.");
                Environment.Exit(ExitCodes.AdminPrivilegesRequired);
                return;
            }

            try
            {
                if (!FileExtensionExistsInRegistry(extension))
                {
                    return;
                }

                Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{extension}", false);
                NotifyShell();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex.Message);
            }
        }

        public bool FileExtensionExistsInRegistry(string extension)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\{extension}");
                bool exists = key != null;
                return exists;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex.Message);
                return false;
            }
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

        private void OnErrorOccurred(string errorMessage)
        {
            ErrorOccurred?.Invoke(this, errorMessage);
        }
    }
}
