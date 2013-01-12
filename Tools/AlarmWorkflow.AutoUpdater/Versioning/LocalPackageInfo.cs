using System;
using System.Xml.Linq;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Record of a locally installed package.
    /// </summary>
    class LocalPackageInfo
    {
        #region Properties

        public string Identifier { get; set; }
        public Version Version { get; set; }

        #endregion

        #region Methods

        internal void Save(string path)
        {
            XDocument doc = new XDocument();
            doc.Add(new XElement("LocalPackageInfo"));
            doc.Root.Add(new XElement("Identifier", this.Identifier));
            doc.Root.Add(new XElement("Version", this.Version));

            doc.Save(path);
        }

        internal static LocalPackageInfo Load(string path)
        {
            XDocument doc = XDocument.Load(path);

            LocalPackageInfo info = new LocalPackageInfo();
            info.Identifier = doc.Root.Element("Identifier").Value;
            info.Version = Version.Parse(doc.Root.Element("Version").Value);

            return info;
        }

        #endregion
    }
}
