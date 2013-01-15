using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlarmWorkflow.Tools.AutoUpdater.Versioning;
using Ionic.Zip;

namespace AlarmWorkflow.Tools.AutoUpdater.Models
{
    /// <summary>
    /// Provides a means to install packages.
    /// </summary>
    sealed class PackageInstaller
    {
        #region Fields

        private Model _model;
        private string _installDirectory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstaller"/> class.
        /// </summary>
        public PackageInstaller()
        {
            _model = App.GetApp().Model;
            _installDirectory = Utilities.GetWorkingDirectory();
        }

        #endregion

        #region Methods

        internal void Execute(IEnumerable<string> packagesToUpdate)
        {
            // 1. Create a backup of the current folder
            BackupCurrentFolder();

            // 2. Install the packages
            IEnumerable<string> packagesInOrder = DetermineInstallOrder(packagesToUpdate);
            foreach (string package in packagesInOrder)
            {
                InstallPackage(package);
            }

            _model.PackageListLocal.StoreLocalPackageInfos();
        }

        private void BackupCurrentFolder()
        {
            Log.Write(Properties.Resources.LogBackupCurrentDirectoryStart);

            string backupZipFileName = Path.Combine(_installDirectory, "_CurrentBackup" + DateTime.Now.ToString("yyyMMddhhmmss") + ".zip.bak");
            using (ZipFile zip = new ZipFile(backupZipFileName))
            {
                foreach (var fileName in Directory.GetFiles(_installDirectory, "*.*", SearchOption.AllDirectories))
                {
                    if (fileName.EndsWith(".bak") ||
                        fileName.Contains("AutoUpdaterCache"))
                    {
                        continue;
                    }

                    string fileNameInZip = Path.GetDirectoryName(fileName);
                    fileNameInZip = fileNameInZip.Replace(_installDirectory, "");
                    zip.AddFile(fileName, fileNameInZip);
                }
                zip.Save();
            }

            Log.Write(Properties.Resources.LogBackupCurrentDirectoryFinished, backupZipFileName);
        }

        /// <summary>
        /// Determines the download and install-order for the given set of packages to be updated.
        /// </summary>
        /// <param name="packagesToUpdate"></param>
        /// <returns></returns>
        private IEnumerable<string> DetermineInstallOrder(IEnumerable<string> packagesToUpdate)
        {
            Dictionary<string, int> depTmp = new Dictionary<string, int>();
            // Add all packages in existence to avoid KeyNotFoundExceptions
            foreach (var pkgSrv in _model.PackageListServer.Packages)
            {
                depTmp.Add(pkgSrv.Identifier, 0);
            }

            foreach (string pkg in packagesToUpdate)
            {
                depTmp[pkg] += 1;

                var deps = _model.PackageListServer.GetDependenciesOfPackage(pkg);
                foreach (string dep in deps)
                {
                    depTmp[dep] += 1;
                }
            }

            // Afterwards, sort out those with > 0 (not needed)
            List<KeyValuePair<string, int>> depOrder = new List<KeyValuePair<string, int>>();
            foreach (var pair in depTmp)
            {
                if (pair.Value > 0)
                {
                    depOrder.Add(pair);
                }
            }

            depOrder.Sort((f, n) => f.Value.CompareTo(n.Value));
            depOrder.Reverse();

            return depOrder.Select(kvp => kvp.Key);
        }

        private void InstallPackage(string packageIdentifier)
        {
            // Fetch available versions (sorted from newest to oldest so we need to revert)
            IEnumerable<Version> versionsAvailableOnServer = _model.PackageListServer.GetVersionsOfPackage(packageIdentifier).Reverse();

            // Determine which versions to install
            List<Version> versionsToInstall = new List<Version>();

            // Get installed package version, if any
            Version localVersion = _model.PackageListLocal.GetLocalVersionOfPackage(packageIdentifier);
            if (localVersion == null)
            {
                // Add all versions
                versionsToInstall.AddRange(versionsAvailableOnServer);
            }
            else
            {
                // Add all versions that are older than the current version.
                versionsToInstall.AddRange(versionsAvailableOnServer.Where(v => v > localVersion));
            }

            foreach (Version version in versionsToInstall)
            {
                InstallPackageUpdate(packageIdentifier, version);
            }
        }

        private void InstallPackageUpdate(string identifier, Version version)
        {
            using (Stream packageStream = DownloadPackage(identifier, version))
            {
                // TODO: Error handling (See comment in DownloadPackage())?!
                if (packageStream == null)
                {
                    return;
                }

                // Install the package (using a copy of the stream which we have to manually dispose of)
                using (Stream zipStreamCopied = packageStream.Copy())
                {
                    using (ZipFile zip = ZipFile.Read(zipStreamCopied))
                    {
                        zip.ExtractAll(_installDirectory, ExtractExistingFileAction.OverwriteSilently);
                    }
                }

                // Add an entry in the local package list
                LocalPackageList localPackageList = _model.PackageListLocal;
                localPackageList.StorePackageInCache(identifier, version, packageStream);

                LocalPackageInfo lpiNew = new LocalPackageInfo();
                lpiNew.Identifier = identifier;
                lpiNew.Version = version;

                localPackageList.SetLocalPackageInfo(lpiNew);
            }
        }

        private Stream DownloadPackage(string identifier, Version version)
        {
            try
            {
                return _model.ServerClient.DownloadPackageVersion(identifier, version);
            }
            catch (Exception)
            {
                // TODO: Rethrow exception? Technically this would be best (no one knows if the following packages are ok at all then)...
            }
            return null;
        }

        #endregion
    }
}
