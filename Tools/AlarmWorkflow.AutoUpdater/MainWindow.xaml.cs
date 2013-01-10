using System.Windows;
using AlarmWorkflow.Tools.AutoUpdater.ViewModels;

namespace AlarmWorkflow.Tools.AutoUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private MainWindowViewModel _viewModel;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel();
            this.DataContext = _viewModel;
        }

        #endregion
    }
}
