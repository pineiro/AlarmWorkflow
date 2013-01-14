using System;
using System.Windows.Controls;

namespace AlarmWorkflow.Tools.AutoUpdater.Views
{
    /// <summary>
    /// Interaction logic for LogControl.xaml
    /// </summary>
    public partial class LogControl : UserControl
    {
        #region Constructors

        public LogControl()
        {
            InitializeComponent();

            foreach (string text in Log.Entries)
            {
                AddToLog(text);
            }
            Log.PostText += Log_PostText;
        }

        #endregion

        #region Methods

        private void AddToLog(string text)
        {
            txtLog.AppendText(text + Environment.NewLine);
        }

        void Log_PostText(string text)
        {
            txtLog.Dispatcher.BeginInvoke((Action)(() => AddToLog(text)));
        }

        #endregion
    }
}
