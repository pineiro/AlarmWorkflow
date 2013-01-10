using System.Windows;

namespace AlarmWorkflow.Tools.AutoUpdater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Properties

        /// <summary>
        /// Gets access to the main model.
        /// </summary>
        internal Models.Model Model { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Calling this default constructor will cause an exception, because it is not allowed to start the UI manually
        /// (it must be started through the <see cref="UINotifyable"/> type)!
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">More than one instance of the <see cref="T:System.Windows.Application"/> class is created per <see cref="T:System.AppDomain"/>.</exception>
        public App()
        {
            Model = new Models.Model();
        }

        #endregion

        #region Methods

        internal static App GetApp()
        {
            return (App)App.Current;
        }

        #endregion

    }
}
