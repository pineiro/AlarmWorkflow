using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Provides the details of a package (versions, history etc.).
    /// </summary>
    class PackageDetail
    {
        #region Properties

        public string ParentIdentifier { get; set; }
        /// <summary>
        /// Gets the collection that contains the version entries, with the latest version being at index #0.
        /// </summary>
        public ReadOnlyCollection<Entry> Versions { get; private set; }

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
            List<Entry> tempList = new List<Entry>();
            foreach (XElement versionElement in document.Root.Elements("Version"))
            {
                Entry entry = ParseVersionElement(versionElement);
                if (entry == null)
                {
                    // Skip these entries, most likely invalid format
                    continue;
                }

                tempList.Add(entry);
            }

            PackageDetail svi = new PackageDetail();
            svi.Versions = new ReadOnlyCollection<Entry>(tempList.OrderByDescending(ve => ve.Version).ToList());

            return svi;
        }

        private static Entry ParseVersionElement(XElement versionElement)
        {
            Entry entry = new Entry();

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

            Type type = Type.Regular;
            if (!Enum.TryParse<Type>(versionElement.Element("Type").Value, out type))
            {
                return null;
            }

            entry.Version = version;
            entry.Type = type;
            entry.Changelog = versionElement.Element("Changelog").Value;

            return entry;
        }

        #endregion

        #region Nested types

        internal class Entry
        {
            public Version Version { get; set; }
            public DateTime Timestamp { get; set; }
            public Type Type { get; set; }
            public string Changelog { get; set; }
        }

        internal enum Type
        {
            Regular = 0,
            Minor,
            Bugfix,
            Critical,
            Major,
        }

        #endregion
    }
}
