using System.IO;
using System.Diagnostics;

namespace AlarmWorkflow.Tools.AutoUpdater.Network
{
    /// <summary>
    /// Implements the <see cref="IServerClient"/> to provide a client to a "localhost server",
    /// by retrieving the contents from the local hard drive.
    /// </summary>
    class LocalFileSystemServerClient : IServerClient
    {
        #region Constants

        private const string VersionsXmlFileName = "versions.xml";
        private static readonly string PackagesListFileName = Debugger.IsAttached ? "packages.dev.xml" : Properties.Settings.Default.PackagesListFileName;

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the absolute path of the folder that represents the "master" directory.
        /// </summary>
        public string RootFolder { get; set; }

        #endregion

        #region IServerClient Members

        Stream IServerClient.DownloadServerPackageList()
        {
            string filePath = Path.Combine(RootFolder, PackagesListFileName);
            return File.OpenRead(filePath);
        }

        Stream IServerClient.DownloadPackageDetail(string id)
        {
            string filePath = Path.Combine(RootFolder, "pkg", id, VersionsXmlFileName);
            return File.OpenRead(filePath);
        }

        Stream IServerClient.DownloadPackageVersion(string id, System.Version version)
        {
            string packageFileName = string.Format("{0}.zip", version.ToString());

            string filePath = Path.Combine(RootFolder, "pkg", id, packageFileName);
            return File.OpenRead(filePath);
        }

        #endregion
    }
}
