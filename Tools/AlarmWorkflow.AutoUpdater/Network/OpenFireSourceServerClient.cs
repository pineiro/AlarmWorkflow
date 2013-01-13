using System;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace AlarmWorkflow.Tools.AutoUpdater.Network
{
    /// <summary>
    /// Implements the <see cref="IServerClient"/> to access the master server under http://openfiresource.de/.
    /// </summary>
    class OpenFireSourceServerClient : IServerClient
    {
        #region Methods

        private static string GetUriOnServer(params string[] relativeUriPaths)
        {
            string uri = string.Format("{0}/{1}/{2}",
                Properties.Settings.Default.UpdateServerName,
                Properties.Settings.Default.UpdateFilesDirectory,
                string.Join("/", relativeUriPaths));

            return uri;
        }

        private static Stream DownloadFileSync(string uri)
        {
            using (WebClient client = new WebClient())
            {
                byte[] data = client.DownloadData(uri);
                return new MemoryStream(data);
            }
        }

        private static Stream DownloadFileWithLog(string filePath)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Stream stream = DownloadFileSync(filePath);
            sw.Stop();
            Debug.WriteLine("Downloaded file '{0}' in '{1}' ms.", filePath, sw.ElapsedMilliseconds);

            return stream;
        }

        #endregion

        #region INetworkInterface Members

        Stream IServerClient.DownloadServerPackageList()
        {
            string uri = GetUriOnServer(Properties.Settings.Default.PackagesListFileName);
            return DownloadFileWithLog(uri);
        }

        Stream IServerClient.DownloadPackageDetail(string id)
        {
            string filePath = GetUriOnServer(Properties.Settings.Default.PackagesDirectory,
                id,
                Properties.Settings.Default.UpdateServerVersionListFileName);

            return DownloadFileWithLog(filePath);
        }

        Stream IServerClient.DownloadPackageVersion(string id, Version version)
        {
            string filePath = GetUriOnServer(Properties.Settings.Default.PackagesDirectory,
                id,
                Properties.Settings.Default.UpdateServerVersionListFileName);

            return DownloadFileWithLog(filePath);
        }

        #endregion
    }
}
