using System.Collections.Generic;
using System.Diagnostics;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Provides information about one package.
    /// </summary>
    [DebuggerDisplay("Id = '{Identifier}', DisplayName = '{DisplayName}'")]
    class PackageInformation
    {
        #region Properties

        public string Identifier { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public PackageState State { get; set; }
        /// <summary>
        /// Gets/sets the dependencies to other packages that this package has.
        /// The list contains the Identifiers of the packages.
        /// </summary>
        public List<string> Dependencies { get; set; }

        #endregion

        #region Constructors

        internal PackageInformation()
        {
            Dependencies = new List<string>();
        }

        #endregion

        #region Nested types

        public enum PackageState
        {
            /// <summary>
            /// Default package state. The package is in active development.
            /// </summary>
            Active = 0,
            /// <summary>
            /// The package is deprecated and should not be used intentionally.
            /// This package is only included to ensure backwards compatibility.
            /// </summary>
            Deprecated = 10,
        }

        #endregion
    }
}
