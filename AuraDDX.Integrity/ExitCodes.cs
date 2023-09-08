namespace AuraDDX.Integrity
{
    /// <summary>
    /// Defines exit codes for debugging and error reporting.
    /// </summary>
    public static class ExitCodes
    {
        /// <summary>
        /// Indicates a successful execution without errors.
        /// </summary>
        public const int Success = 0;

        /// <summary>
        /// Indicates that the operation was canceled by the user.
        /// </summary>
        public const int Cancel = 1;

        /// <summary>
        /// Indicates an invalid argument provided to the application.
        /// </summary>
        public const int InvalidArgument = 2;

        /// <summary>
        /// Indicates an error occurred during file conversion.
        /// </summary>
        public const int ConversionError = 3;

        /// <summary>
        /// Indicates a missing dependency or required file.
        /// </summary>
        public const int MissingDependency = 4;

        /// <summary>
        /// Indicates that administrator privileges are required for the operation.
        /// </summary>
        public const int AdminPrivilegesRequired = 5;

        /// <summary>
        /// Indicates an update closed the program.
        /// </summary>
        public const int Update = 6;
    }
}
