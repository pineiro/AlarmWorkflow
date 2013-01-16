using System;
using System.IO;
using System.Xml.Linq;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Record of a locally installed package.
    /// </summary>
    class LocalPackageInfo
    {
        #region Properties

        /// <summary>
        /// Gets/sets the identifier of the local package.
        /// </summary>
        public string Identifier { get; set; }
        /// <summary>
        /// Gets/sets the version of the local package.
        /// </summary>
        public Version Version { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalPackageInfo"/> class.
        /// </summary>
        public LocalPackageInfo()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalPackageInfo"/> class.
        /// </summary>
        /// <param name="identifier">The identifier of the local package.</param>
        /// <param name="version">The version of the local package.</param>
        public LocalPackageInfo(string identifier, Version version)
            : base()
        {
            Identifier = identifier;
            Version = version;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an XML-document out of this instance and returns a stream containing the contents.
        /// </summary>
        /// <returns></returns>
        internal Stream Save()
        {
            MemoryStream stream = new MemoryStream();

            XDocument doc = new XDocument();
            doc.Add(new XElement("LocalPackageInfo"));
            doc.Root.Add(new XElement("Identifier", this.Identifier));
            doc.Root.Add(new XElement("Version", this.Version));

            doc.Save(stream);

            stream.Position = 0L;
            return stream;
        }

        /// <summary>
        /// Loads the <see cref="LocalPackageInfo"/> from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to load. The stream is left open after this method exits.</param>
        /// <returns></returns>
        internal static LocalPackageInfo Load(Stream stream)
        {
            XDocument doc = XDocument.Load(stream);

            LocalPackageInfo info = new LocalPackageInfo();
            info.Identifier = doc.Root.Element("Identifier").Value;
            info.Version = Version.Parse(doc.Root.Element("Version").Value);

            return info;
        }

        #endregion
    }
}
