using System;
using AlarmWorkflow.Windows.UIContracts.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace AlarmWorkflow.Tools.AutoUpdater.ViewModels
{
    class PackageListControlViewModel : ViewModelBase
    {
        #region Properties

        public ObservableCollection<PackageDisplayItemViewModel> Packages { get; private set; }

        #endregion

        #region Constructors

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
                PackageDisplayItemViewModel vm = new PackageDisplayItemViewModel(this);
                vm.Info = item;
                vm.Detail = App.GetApp().Model.PackageListServer.PackageDetails.FirstOrDefault(pd => pd.ParentIdentifier == item.Identifier);
                vm.NeedsUpdate = App.GetApp().Model.IsUpdateNeeded(item.Identifier);

                Packages.Add(vm);
            }
        }

        #endregion
    }
}
