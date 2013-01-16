using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Provides information about one package.
    /// </summary>
    [DebuggerDisplay("Id = '{Identifier}', DisplayName = '{DisplayName}'")]
    class PackageInformation : IEquatable<PackageInformation>
    {
        #region Properties

        /// <summary>
        /// Gets/sets the unique identifier of the package.
        /// </summary>
        public string Identifier { get; set; }
        /// <summary>
        /// Gets/sets the display name of the package (for the UI).
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Gets/sets the description of the package.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets/sets the category of the package.
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Gets/sets the author of the package.
        /// </summary>        
        public string Author { get; set; }
        /// <summary>
        /// Gets/sets the package state.
        /// </summary>
        public PackageState State { get; set; }
        /// <summary>
        /// Gets/sets the package recommendation.
        /// </summary>
        public PackageRecommendation Recommendation { get; set; }
        /// <summary>
        /// Gets/sets the dependencies to other packages that this package has.
        /// The list contains the Identifiers of the packages.
        /// </summary>
        public List<string> Dependencies { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInformation"/> class.
        /// </summary>
        internal PackageInformation()
        {
            Dependencies = new List<string>();
        }

        #endregion

        #region IEquatable<PackageInformation> Members

        /// <summary>
        /// Returns whether or not the other package and this package are informatically equal.
        /// This is the case if the values of the two <see cref="P:Identifier"/>-properties is the same.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(PackageInformation other)
        {
            return other.Identifier == this.Identifier;
        }

        #endregion
    }
}
