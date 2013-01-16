using System;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Defines the possible package states.
    /// </summary>
    public enum PackageState
    {
        /// <summary>
        /// Default package state. The package is in active development.
        /// </summary>
        Active = 0,
        /// <summary>
        /// The package is work in progress (WIP). This means that it is used at one's own risk.
        /// </summary>
        WIP = 9,
        /// <summary>
        /// The package is deprecated and should not be used intentionally.
        /// This package is only included to ensure backwards compatibility.
        /// </summary>
        Deprecated = 10,
        /// <summary>
        /// The package is ignored and not available for automated download.
        /// This attributes to packages that should not be downloaded because of intermittent problems or such.
        /// </summary>
        Ignored = 11,
    }
}
