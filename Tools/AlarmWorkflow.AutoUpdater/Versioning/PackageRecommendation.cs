using System;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Defines the recommendation of a package. This is a guideline for users to decide which packages to download.
    /// </summary>
    public enum PackageRecommendation
    {
        /// <summary>
        /// No recommendation (default).
        /// </summary>
        None = 0,
        /// <summary>
        /// The package is essential for functionality.
        /// </summary>
        Essential,
        /// <summary>
        /// It is recommended to download this package.
        /// </summary>
        Recommended,
    }
}
