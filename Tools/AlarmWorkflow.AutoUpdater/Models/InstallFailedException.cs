using System;

namespace AlarmWorkflow.Tools.AutoUpdater.Models
{
    /// <summary>
    /// Internal exception that occurs if the install process has failed, usually due to missing packages.
    /// </summary>
    class InstallFailedException : Exception
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallFailedException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        public InstallFailedException(Exception innerException)
            : base(Properties.Resources.InstallFailedExceptionMessage, innerException)
        {

        }

        #endregion
    }
}
