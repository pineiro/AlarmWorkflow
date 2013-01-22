using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AlarmWorkflow.Tools.AutoUpdater.Versioning;
using AlarmWorkflow.Windows.UIContracts.ViewModels;
using AlarmWorkflow.Tools.AutoUpdater.Models;
using System.ComponentModel;
using System.Windows.Data;

namespace AlarmWorkflow.Tools.AutoUpdater.ViewModels
{
    class PackageListControlViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Gets the collection of all packages.
        /// </summary>
        public ObservableCollection<PackageDisplayItemViewModel> Packages { get; private set; }

        #endregion

        #region Commands

        #region Command "ApplySelectionCommand"

        /// <summary>
        /// The ApplySelectionCommand command.
        /// </summary>
        public ICommand ApplySelectionCommand { get; private set; }

        private bool ApplySelectionCommand_CanExecute(object parameter)
        {
            return Packages.Any(p => p.IsScheduledForInstallOrUpdate);
        }

        private void ApplySelectionCommand_Execute(object parameter)
        {
            if (!Utilities.ConfirmMessageBox(Properties.Resources.ApplySelectionQuestionMessage))
            {
                return;
            }

            var packagesToUpdate = Packages.Where(p => p.IsScheduledForInstallOrUpdate).Select(p => p.Info.Identifier);

            PackageInstaller installer = new PackageInstaller();
            try
            {
                installer.Execute(packagesToUpdate);
            }
            catch (Exception ex)
            {
                Utilities.ShowMessageBox(System.Windows.Forms.MessageBoxIcon.Error, Properties.Resources.InstallationProcessFailedMessage, ex.Message);
            }

            BuildPackagesListForView();
        }

        #endregion

        #region Command "SelectPackagesCommand"

        /// <summary>
        /// The SelectPackagesCommand command.
        /// </summary>
        public ICommand SelectPackagesCommand { get; private set; }

        private void SelectPackagesCommand_Execute(object parameter)
        {
            SelectPackagesCommandParameter par = (SelectPackagesCommandParameter)Enum.Parse(typeof(SelectPackagesCommandParameter), (string)parameter);
            switch (par)
            {
                case SelectPackagesCommandParameter.AvailableUpdates:
                    foreach (var pkg in Packages.Where(p => p.NeedsUpdate))
                    {
                        pkg.SetIsScheduledForInstallOrUpdate(true, false);
                    }
                    break;
                case SelectPackagesCommandParameter.Essential:
                    foreach (var pkg in Packages.Where(p => p.Info.Recommendation == PackageRecommendation.Essential))
                    {
                        pkg.SetIsScheduledForInstallOrUpdate(true, false);
                    }
                    break;
                case SelectPackagesCommandParameter.Recommended:
                    foreach (var pkg in Packages.Where(p => p.Info.Recommendation == PackageRecommendation.Recommended))
                    {
                        pkg.SetIsScheduledForInstallOrUpdate(true, false);
                    }
                    break;
                case SelectPackagesCommandParameter.All:
                    foreach (var pkg in Packages)
                    {
                        pkg.SetIsScheduledForInstallOrUpdate(true, false);
                    }
                    break;
                default:
                case SelectPackagesCommandParameter.None:
                    foreach (var pkg in Packages)
                    {
                        pkg.SetIsScheduledForInstallOrUpdate(false, false);
                    }
                    break;
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageListControlViewModel"/> class.
        /// </summary>
        public PackageListControlViewModel()
        {
            Packages = new ObservableCollection<PackageDisplayItemViewModel>();

            ICollectionView packagesView = CollectionViewSource.GetDefaultView(this.Packages);
            packagesView.GroupDescriptions.Add(new PropertyGroupDescription("Info.Category"));

            BuildPackagesListForView();
        }

        #endregion

        #region Methods

        private void BuildPackagesListForView()
        {
            this.Packages.Clear();

            foreach (var item in App.GetApp().Model.PackageListServer.Packages)
            {
                AddVMForPackage(item);
            }
        }

        private void AddVMForPackage(PackageInformation item)
        {
            PackageDisplayItemViewModel vm = new PackageDisplayItemViewModel(this);
            vm.Info = item;
            vm.Detail = App.GetApp().Model.PackageListServer.PackageDetails.FirstOrDefault(pd => pd.ParentIdentifier == item.Identifier);
            vm.NeedsUpdate = App.GetApp().Model.IsUpdateNeeded(item.Identifier);
            vm.Dependencies = GetDependencies(item.Identifier);

            Packages.Add(vm);
        }

        private IList<string> GetDependencies(string identifier)
        {
            List<string> list = new List<string>();
            list.AddRange(App.GetApp().Model.PackageListServer.GetDependenciesOfPackage(identifier).Distinct().OrderBy(p => p));

            if (list.Count == 0)
            {
                list.Add(Properties.Resources.PackageHasNoDependenciesListItem);
            }
            return list;
        }

        #endregion

        #region Nested types

        enum SelectPackagesCommandParameter
        {
            AvailableUpdates,
            Essential,
            Recommended,
            All,
            None,
        }
        #endregion
    }
}
