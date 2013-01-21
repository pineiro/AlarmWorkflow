using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Represents one server version of a package.
    /// </summary>
    [DebuggerDisplay("Version = {Version}, Type = {Type}")]
    class PackageDetailEntry
    {
        public Version Version { get; set; }
        public DateTime Timestamp { get; set; }
        public PackageDetailEntryType Type { get; set; }
        public string Changelog { get; set; }
        /// <summary>
        /// Gets/sets a list declaring the dependencies to other packages in certain versions that are
        /// required for this entry.
        /// </summary>
        // TODO: Reserved for future use!
        public List<PackageDetailEntryDependency> Dependencies { get; set; }

        public PackageDetailEntry()
        {
            Dependencies = new List<PackageDetailEntryDependency>();
        }
    }
}
