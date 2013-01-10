using System.IO;
using System.Xml.Linq;
using AlarmWorkflow.Tools.AutoUpdater.Network;
using AlarmWorkflow.Tools.AutoUpdater.Versioning;

namespace AlarmWorkflow.Tools.AutoUpdater.Models
{
    /// <summary>
    /// Represents the main model for the application.
    /// </summary>
    class Model
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="IServerClient"/> that is used to access the download server.
        /// </summary>
        public IServerClient ServerClient { get; private set; }
        /// <summary>
        /// Gets the package list, server version.
        /// </summary>
        public ServerPackageList PackageListServer { get; private set; }

        #endregion

        #region Constructors

        public Model()
        {
            // Set server client
            var client = new Network.LocalFileSystemServerClient();
            client.RootFolder = @"F:\AlarmWorkflow\Resources\Versioning\master";

            this.ServerClient = client;

            DownloadServerPackageList();
        }

        #endregion

        #region Methods

        private void DownloadServerPackageList()
        {
            PackageListServer = ServerPackageList.Create(ServerClient);
        }

        #endregion
    }
}
