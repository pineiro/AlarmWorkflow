using System;

namespace AlarmWorkflow.Tools.AutoUpdater.Models
{
    /// <summary>
    /// Internal exception that occurs when a package could not be downloaded.
    /// </summary>
    class PackageDownloadException : Exception
    {
        #region Properties

        /// <summary>
        /// Gets the identifier of the package that could not be downloaded.
        /// </summary>
        public string Package { get; private set; }
        /// <summary>
        /// Gets the version of the package that could not be downloaded.
        /// </summary>
        public Version Version { get; private set; }
        /// <summary>
        /// Gets/sets whether or not this fail is fatal and the whole process needs to stop.
        /// </summary>
        public bool IsFatalFail { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageDownloadException"/> class.
        /// </summary>
        /// <param name="package">The identifier of the package that could not be downloaded.</param>
        /// <param name="version">The version of the package that could not be downloaded.</param>
        /// <param name="innerException">The inner exception, if any.</param>
        public PackageDownloadException(string package, Version version, Exception innerException)
            : base(string.Format(Properties.Resources.PackageDownloadExceptionMessageFormat, package, version), innerException)
        {
            Package = package;
            Version = version;
        }

        #endregion
    }
}
