using System.Linq;
using AlarmWorkflow.Tools.AutoUpdater.Versioning;
using AlarmWorkflow.Windows.UIContracts.ViewModels;
using System;

namespace AlarmWorkflow.Tools.AutoUpdater.ViewModels
{
    /// <summary>
    /// Represents the VM that is used to display a package which may be installed.
    /// </summary>
    class PackageDisplayItemViewModel : ViewModelBase
    {
        #region Fields

        private bool _isScheduledForInstallOrUpdate;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the parent VM that we are attached to.
        /// </summary>
        public PackageListControlViewModel Parent { get; private set; }

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
        public bool IsScheduledForInstallOrUpdate
        {
            get { return _isScheduledForInstallOrUpdate; }
            set { SetIsScheduledForInstallOrUpdate(value, true); }
        }

        public Version LocalPackageVersion
        {
            get { return App.GetApp().Model.PackageListLocal.GetLocalVersionOfPackage(Info.Identifier); }
        }
        /// <summary>
        /// Gets/sets whether or not this item is already installed.
        /// </summary>
        public bool IsInstalled
        {
            get { return LocalPackageVersion != null; }
        }
        public bool NeedsUpdate { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">The parent VM that we are attached to.</param>
        public PackageDisplayItemViewModel(PackageListControlViewModel parent)
        {
            this.Parent = parent;
        }

        #endregion

        #region Methods

        private void SetIsScheduledForInstallOrUpdate(bool value, bool interactiveMode)
        {
            if (value == _isScheduledForInstallOrUpdate)
            {
                return;
            }

            if (value)
            {
                if (interactiveMode)
                {
                    switch (this.Info.State)
                    {
                        case PackageInformation.PackageState.WIP:
                            if (!Utilities.ConfirmMessageBox(Properties.Resources.PackageIsWIPWarningMessage))
                            {
                                return;
                            }
                            break;
                        case PackageInformation.PackageState.Deprecated:
                            if (!Utilities.ConfirmMessageBox(Properties.Resources.PackageIsDeprecatedWarningMessage))
                            {
                                return;
                            }
                            break;
                        case PackageInformation.PackageState.Active:
                        default:
                            break;
                    }
                }

                /* If we schedule this item for install/update,
                 * we also have to schedule the dependencies!
                 */
                var requiredDependencies = this.Info.Dependencies.SelectMany(d => Parent.Packages.Where(p => p.Info.Identifier == d));
                foreach (var item in requiredDependencies)
                {
                    item.IsScheduledForInstallOrUpdate = true;
                }
            }
            else
            {
                // Don't allow removal if there are dependencies!
                var dependenciesOfMe = Parent.Packages.Where(p => p.IsScheduledForInstallOrUpdate && p.Info.Dependencies.Contains(this.Info.Identifier));
                if (dependenciesOfMe.Any())
                {
                    if (interactiveMode)
                    {
                        string msg = string.Join("\n-", dependenciesOfMe.Select(d => d.Info.DisplayName));
                        if (!Utilities.ConfirmMessageBox(Properties.Resources.CannotUnscheduleBecauseOfExistingDependenciesMessage, msg))
                        {
                            return;
                        }
                    }

                    // Disable all packages without notice
                    foreach (var item in dependenciesOfMe)
                    {
                        item._isScheduledForInstallOrUpdate = false;
                        item.OnPropertyChanged("IsScheduledForInstallOrUpdate");
                    }
                }
            }

            _isScheduledForInstallOrUpdate = value;
            OnPropertyChanged("IsScheduledForInstallOrUpdate");
        }

        #endregion

    }
}
