using System;
using System.Diagnostics;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Declares a dependency form a package to another package in a certain version.
    /// </summary>
    [DebuggerDisplay("Identifier = {Identifier}, Version = {Version}")]
    class PackageDetailEntryDependency
    {
        /// <summary>
        /// Gets/sets the identifier of this dependency.
        /// </summary>
        public string Identifier { get; set; }
        /// <summary>
        /// Gets/sets the minimum required version of this dependency.
        /// </summary>
        public Version Version { get; set; }
    }
}
