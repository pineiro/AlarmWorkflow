using AlarmWorkflow.Tools.AutoUpdater.Versioning;
using AlarmWorkflow.Windows.UIContracts.ViewModels;

namespace AlarmWorkflow.Tools.AutoUpdater.ViewModels
{
    /// <summary>
    /// Represents the VM that is used to display a package which may be installed.
    /// </summary>
    class PackageDisplayItemViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Gets/sets the generic package information from the server.
        /// </summary>
        public PackageInformation Info { get; set; }
        /// <summary>
        /// Gets/sets the package details.
        /// </summary>
        public PackageDetail Detail { get; set; }
        /// <summary>
        /// Gets/sets whether or not this item has been selected for install or update.
        /// </summary>
        public bool IsInstallOrUpdate { get; set; }
        /// <summary>
        /// Gets/sets whether or not this item is already installed.
        /// </summary>
        public bool IsInstalled { get; set; }

        #endregion
    }
}
