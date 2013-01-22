using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AlarmWorkflow.Tools.AutoUpdater.Versioning;
using Ionic.Zip;
using System.Reflection;

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

        /// <summary>
        /// Installs/updates the given packages.
        /// </summary>
        /// <param name="packagesToUpdate">The names of the packages to install/update.</param>
        /// <exception cref="InstallFailedException">Thrown if the installation process has failed, usually due to missing packages.</exception>
        public void Execute(IEnumerable<string> packagesToUpdate)
        {
            // 1. Create a backup of the current folder
            BackupCurrentFolder();

            // 2. Install the packages
            IEnumerable<CalculatedPackageOrderInfo> packagesInOrder = DetermineInstallOrder(packagesToUpdate);
            foreach (CalculatedPackageOrderInfo package in packagesInOrder)
            {
                try
                {
                    InstallPackage(package);
                }
                catch (PackageDownloadException ex)
                {
                    throw new InstallFailedException(ex);
                }
            }
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
        private IEnumerable<CalculatedPackageOrderInfo> DetermineInstallOrder(IEnumerable<string> packagesToUpdate)
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

            // When setting the dependency count, subtract 1 because the count is one-based and we need it zero-based for convenience.
            return depOrder.Select(kvp => new CalculatedPackageOrderInfo(kvp.Key, kvp.Value - 1));
        }

        private void InstallPackage(CalculatedPackageOrderInfo info)
        {
            string packageIdentifier = info.Package;

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

            if (versionsToInstall.Count == 0)
            {
                Log.Write(Properties.Resources.PackageToInstallNoVersionsMessage, packageIdentifier);
            }
            else
            {
                foreach (Version version in versionsToInstall)
                {
                    try
                    {
                        InstallPackageUpdate(info, version);
                    }
                    catch (PackageDownloadException ex)
                    {
                        // If this fail is fatal, cancel the whole process. Otherwise continue.
                        if (ex.IsFatalFail)
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

        private void InstallPackageUpdate(CalculatedPackageOrderInfo info, Version version)
        {
            using (Stream packageStream = DownloadPackage(info, version))
            {
                // Install the package (using a copy of the stream which we have to manually dispose of)
                using (Stream zipStreamCopied = packageStream.Copy())
                {
                    using (ZipFile zip = ZipFile.Read(zipStreamCopied))
                    {
                        ExtractZipFileAndRunInstallScript(zip, version);
                    }
                }

                Log.Write(Properties.Resources.PackageInstallAllFilesExtractedMessage);

                // Add an entry in the local package list
                string package = info.Package;
                LocalPackageList localPackageList = _model.PackageListLocal;
                localPackageList.StorePackageInCache(package, version, packageStream);

                LocalPackageInfo lpiNew = new LocalPackageInfo(package, version);
                localPackageList.SetLocalPackageInfo(lpiNew);

                Log.Write(Properties.Resources.PackageInstallUpdatedLocalVersionsConfig);
            }
        }

        private void ExtractZipFileAndRunInstallScript(ZipFile zip, Version version)
        {
            foreach (ZipEntry entry in zip.EntriesSorted)
            {
                if (entry.FileName == "$Installer$.dll")
                {
                    RunInstallScript(entry, version);
                    continue;
                }

                entry.Extract(_installDirectory, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        private void RunInstallScript(ZipEntry entry, Version version)
        {
            try
            {
                RunInstallScriptWithFail(entry, version);
            }
            catch (TypeLoadException)
            {
                // Could not find the installer type.
                // TODO: Fail package install?
            }
            catch (Exception)
            {
                // TODO

                throw;
            }
        }

        private void RunInstallScriptWithFail(ZipEntry entry, Version version)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                entry.Extract(stream);

                // Load assembly, and find type
                Assembly installerAssembly = Assembly.Load(stream.ToArray());

                Type type = installerAssembly.GetType("Installer", true);
                MethodInfo onInstallMethod = type.GetMethod("OnInstall", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                // Create arguments
                string inInstallDirectory = _installDirectory;
                Version inOldVersion = null;    // < TODO
                Version inNewVersion = version;
                string[] returnValue = null;

                // Instantiate the class and invoke method
                object installerClass = Activator.CreateInstance(type, true);

                returnValue = (string[])onInstallMethod.Invoke(installerClass, new object[3] { inInstallDirectory, inOldVersion, inNewVersion });
                if (returnValue != null && returnValue.Length > 0)
                {
                    foreach (string logEntry in returnValue)
                    {
                        if (logEntry.Length <= 1)
                        {
                            continue;
                        }

                        char typeId = logEntry.First();
                        switch (typeId)
                        {
                            case 'i':
                                break;
                            case 'w':
                                break;
                            case 'e':
                                break;
                            default:
                                break;
                        }

                        Log.Write("Nachricht von Installer-Modul [{0}]: {1}", typeId, logEntry.Remove(0, 1));
                    }
                }
            }
        }

        /// <summary>
        /// Downloads a file using the server client.
        /// </summary>
        /// <param name="identifier">The package.</param>
        /// <param name="version">The concrete version of the package to download.</param>
        /// <returns>A stream containing the downloaded file.</returns>
        /// <exception cref="PackageDownloadException">Download of the package has failed.</exception>
        private Stream DownloadPackage(CalculatedPackageOrderInfo info, Version version)
        {
            string package = info.Package;
            try
            {
                Log.Write(Properties.Resources.PackageVersionDownloadingMessage, package, version);

                Stream stream = _model.ServerClient.DownloadPackageVersion(package, version);

                Log.Write(Properties.Resources.PackageVersionDownloadSucceededMessage, stream.Length);

                return stream;
            }
            catch (Exception ex)
            {
                Log.Write(Properties.Resources.PackageVersionDownloadFailedMessage, ex.Message);

                // Determine whether a fail in downloading this package is fatal.
                // A fail is considered non-fatal if no other package has dependencies to this package.
                // Otherwise a fail is always fatal, because other packages rely on this one and when it is missing they may not work.
                bool isFatalFail = info.HasDependencies;

                PackageDownloadException exception = new PackageDownloadException(package, version, ex);
                exception.IsFatalFail = isFatalFail;
                throw exception;
            }
        }

        #endregion

        #region Nested types

        /// <summary>
        /// Helper class that stores whether or not a package has dependencies to other packages.
        /// </summary>
        [DebuggerDisplay("{Package}, Dependencies = {DependencyCount}")]
        class CalculatedPackageOrderInfo
        {
            internal string Package { get; set; }
            internal int DependencyCount { get; set; }
            internal bool HasDependencies
            {
                get { return DependencyCount > 0; }
            }

            internal CalculatedPackageOrderInfo(string package, int dependencyCount)
            {
                Package = package;
                DependencyCount = dependencyCount;
            }
        }

        #endregion
    }
}
