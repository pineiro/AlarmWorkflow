using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AlarmWorkflow.Tools.AutoUpdater.Versioning;
using AlarmWorkflow.Windows.UIContracts.ViewModels;

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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageListControlViewModel"/> class.
        /// </summary>
        public PackageListControlViewModel()
        {
            Packages = new ObservableCollection<PackageDisplayItemViewModel>();
            BuildPackagesListForView();
        }

        #endregion

        #region Methods

        private void BuildPackagesListForView()
        {
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
    }
}
