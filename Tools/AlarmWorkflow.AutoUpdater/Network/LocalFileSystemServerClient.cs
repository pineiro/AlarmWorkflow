using System.IO;

namespace AlarmWorkflow.Tools.AutoUpdater.Network
{
    /// <summary>
    /// Implements the <see cref="IServerClient"/> to provide a client to a "localhost server",
    /// by retrieving the contents from the local hard drive.
    /// </summary>
    class LocalFileSystemServerClient : IServerClient
    {
        #region Properties

        /// <summary>
        /// Gets/sets the absolute path of the folder that represents the "master" directory.
        /// </summary>
        public string RootFolder { get; set; }

        #endregion

        #region IServerClient Members

        Stream IServerClient.DownloadServerPackageList()
        {
            string filePath = Path.Combine(RootFolder, Properties.Settings.Default.PackagesListFileName);
            if (!File.Exists(filePath))
            {
                return null;
            }
            return File.OpenRead(filePath);
        }

        Stream IServerClient.DownloadPackageDetail(string id)
        {
            string versionFileName = string.Format("{0}.versions.xml", id);
            string filePath = Path.Combine(RootFolder, "pkg", versionFileName);
            if (!File.Exists(filePath))
            {
                return null;
            }
            return File.OpenRead(filePath);
        }

        #endregion
    }
}
