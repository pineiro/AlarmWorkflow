using System.Linq;
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
        /// <summary>
        /// Gets the package list, local version. Defines all packages that are installed on the client.
        /// </summary>
        public LocalPackageList PackageListLocal { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        public Model()
        {
            // Set server client
            var client = new Network.LocalFileSystemServerClient();
            //client.RootFolder = @"D:\Projects\AlarmWorkflow\Resources\Versioning\master";
            client.RootFolder = @"F:\AlarmWorkflow\Resources\Versioning\master";

            ServerClient = client;

            DownloadServerPackageList();
            ConstructLocalPackageList();
        }

        #endregion

        #region Methods

        private void DownloadServerPackageList()
        {
            PackageListServer = ServerPackageList.Create(ServerClient);
        }

        private void ConstructLocalPackageList()
        {
            PackageListLocal = LocalPackageList.Build();
        }

        internal bool IsUpdateNeeded(string identifier)
        {
            LocalPackageInfo pkgLoc = PackageListLocal.Packages.FirstOrDefault(p => p.Identifier == identifier);
            if (pkgLoc == null)
            {
                // Package not installed
                return false;
            }

            PackageDetail pkgSrv = PackageListServer.PackageDetails.FirstOrDefault(sp => sp.ParentIdentifier == identifier);
            if (pkgSrv == null)
            {
                // TODO: Log, maybe package is too old or server error.
                return false;
            }

            return pkgSrv.GetLatestVersion() > pkgLoc.Version;
        }

        #endregion
    }
}
