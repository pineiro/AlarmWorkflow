using System;
using System.IO;

namespace AlarmWorkflow.Tools.AutoUpdater.Network
{
    /// <summary>
    /// Implements the <see cref="IServerClient"/> to access the master server under http://openfiresource.de/.
    /// </summary>
    class OpenFireSourceServerClient : IServerClient
    {
        #region INetworkInterface Members

        Stream IServerClient.DownloadServerPackageList()
        {
            Uri uri = Utilities.GetUriOnServer(Properties.Settings.Default.PackagesListFileName);
            return Utilities.DownloadFileSync(uri.ToString());
        }

        Stream IServerClient.DownloadPackageDetail(string id)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
