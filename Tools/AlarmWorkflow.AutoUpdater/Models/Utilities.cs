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

        internal static bool ConfirmMessageBox(string text)
        {
            DialogResult dr = MessageBox.Show(text, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return dr == DialogResult.Yes;
        }

        /// <summary>
        /// Constructs the Uri to any path that is relative to the server url and is inside the "updates" directory on the server.
        /// </summary>
        /// <param name="relativeUriPaths"></param>
        /// <returns></returns>
        internal static Uri GetUriOnServer(params string[] relativeUriPaths)
        {
            string uri = string.Format("{0}/{1}/{2}",
                Properties.Settings.Default.UpdateServerName,
                Properties.Settings.Default.UpdateFilesDirectory,
                string.Join("/", relativeUriPaths));

            return new Uri(uri, UriKind.Absolute);
        }

        internal static Stream DownloadFileSync(string uri)
        {
            using (WebClient client = new WebClient())
            {
                byte[] data = client.DownloadData(GetUriOnServer(Properties.Settings.Default.UpdateServerVersionOldFileName));
                return new MemoryStream(data);
            }
        }
    }
}
