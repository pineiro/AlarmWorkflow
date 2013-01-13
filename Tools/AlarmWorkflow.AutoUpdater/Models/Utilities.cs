using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace AlarmWorkflow.Tools.AutoUpdater
{
    static class Utilities
    {
        internal static void ShowMessageBox(MessageBoxIcon icon, string format, params object[] args)
        {
            string msg = string.Format(format, args);
            MessageBox.Show(msg, "", MessageBoxButtons.OK, icon);
        }

        internal static bool ConfirmMessageBox(string format, params object[] args)
        {
            string msg = string.Format(format, args);
            DialogResult dr = MessageBox.Show(msg, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return dr == DialogResult.Yes;
        }
    }
}
