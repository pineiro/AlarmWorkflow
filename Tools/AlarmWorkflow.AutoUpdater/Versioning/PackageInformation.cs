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
    }
}
