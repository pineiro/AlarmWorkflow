using System.Windows.Controls;
using AlarmWorkflow.Tools.AutoUpdater.ViewModels;

namespace AlarmWorkflow.Tools.AutoUpdater.Views
{
    /// <summary>
    /// Interaction logic for PackageListControl.xaml
    /// </summary>
    public partial class PackageListControl : UserControl
    {
        #region Fields

        private PackageListControlViewModel _viewModel;

        #endregion

        #region Constructors

        public PackageListControl()
        {
            InitializeComponent();

            _viewModel = new PackageListControlViewModel();
            this.DataContext = _viewModel;
        }

        #endregion
    }
}
