using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Provides the details of a package (versions, history etc.).
    /// </summary>
    [DebuggerDisplay("Parent = {ParentIdentifier}, Versions = {Versions.Count}")]
    class PackageDetail
    {
        #region Properties

        /// <summary>
        /// Gets/sets the identifier of the package that this instance belongs to.
        /// </summary>
        public string ParentIdentifier { get; set; }
        /// <summary>
        /// Gets the collection that contains the version entries, with the latest version being at index #0.
        /// </summary>
        public ReadOnlyCollection<PackageDetailEntry> Versions { get; private set; }

        #endregion

        #region Constructors

        private PackageDetail()
        {
        }

        #endregion

        #region Methods

        internal Version GetLatestVersion()
        {
            return Versions.OrderByDescending(ve => ve.Version).First().Version;
        }

        /// <summary>
        /// Parses the <see cref="PackageDetail"/> from the given XML document.
        /// The versions are then contained in sorted order from newest to oldest.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        internal static PackageDetail FromDocument(XDocument document)
        {
            List<PackageDetailEntry> tempList = new List<PackageDetailEntry>();
            foreach (XElement versionElement in document.Root.Elements("Version"))
            {
                PackageDetailEntry entry = ParseVersionElement(versionElement);
                if (entry == null)
                {
                    // Skip these entries, most likely invalid format
                    continue;
                }

                tempList.Add(entry);
            }

            PackageDetail svi = new PackageDetail();
            svi.Versions = new ReadOnlyCollection<PackageDetailEntry>(tempList.OrderByDescending(ve => ve.Version).ToList());

            return svi;
        }

        private static PackageDetailEntry ParseVersionElement(XElement versionElement)
        {
            PackageDetailEntry entry = new PackageDetailEntry();

            Version version = null;
            if (!Version.TryParse(versionElement.Attribute("Version").Value, out version))
            {
                return null;
            }

            string timestampRaw = versionElement.Attribute("Timestamp").Value;
            DateTime timestamp = DateTime.MinValue;
            if (DateTime.TryParse(versionElement.Attribute("Timestamp").Value, out timestamp))
            {
                entry.Timestamp = timestamp;
            }

            PackageDetailEntryType type = PackageDetailEntryType.Regular;
            if (!Enum.TryParse<PackageDetailEntryType>(versionElement.Element("Type").Value, out type))
            {
                return null;
            }

            entry.Version = version;
            entry.Type = type;
            entry.Changelog = versionElement.Element("Changelog").Value;

            foreach (XElement depE in versionElement.Element("Dependencies").Elements("Dependency"))
            {
                PackageDetailEntryDependency dep = new PackageDetailEntryDependency();
                dep.Identifier = depE.Attribute("To").Value;
                dep.Version = Version.Parse(depE.Attribute("Version").Value);
                entry.Dependencies.Add(dep);
            }

            return entry;
        }

        #endregion
    }
}
