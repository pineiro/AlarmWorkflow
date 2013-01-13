using System;
using System.IO;

namespace AlarmWorkflow.Tools.AutoUpdater.Network
{
    /// <summary>
    /// Defines a means to retrieve the contents of the update server.
    /// Can be used to create mocks, or use internal update servers.
    /// </summary>
    interface IServerClient
    {
        /// <summary>
        /// Downloads the server package list from the server.
        /// </summary>
        /// <returns></returns>
        Stream DownloadServerPackageList();
        /// <summary>
        /// Downloads the version details of the package with the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Stream DownloadPackageDetail(string id);
        /// <summary>
        /// Downloads the specified package's version.
        /// </summary>
        /// <param name="id">The identifier of the package to download.</param>
        /// <param name="version">The version of the package to download.</param>
        /// <returns>The contents of the downloaded package.</returns>
        Stream DownloadPackageVersion(string id, Version version);
    }
}
