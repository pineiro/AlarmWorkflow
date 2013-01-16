using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Contains a list of all locally installed packages.
    /// </summary>
    class LocalPackageList
    {
        #region Constants

        internal static readonly string AutoUpdaterCacheDir = Path.Combine(Utilities.GetWorkingDirectory(), "AutoUpdaterCache");
        internal static readonly string InstalledPackagesDir = Path.Combine(AutoUpdaterCacheDir, "packages");
        private static readonly string FileExtension = "config";

        #endregion

        #region Fields

        private List<LocalPackageInfo> _packages;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a read-only collection of all <see cref="LocalPackageInfo"/> objects.
        /// </summary>
        public ReadOnlyCollection<LocalPackageInfo> Packages
        {
            get { return _packages.AsReadOnly(); }
        }

        #endregion

        #region Constructors

        private LocalPackageList()
        {
            _packages = new List<LocalPackageInfo>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the <see cref="LocalPackageInfo"/>-files from the cache directory and constructs the <see cref="LocalPackageList"/> accordingly.
        /// </summary>
        /// <returns></returns>
        internal static LocalPackageList Build()
        {
            LocalPackageList list = new LocalPackageList();

            if (Directory.Exists(InstalledPackagesDir))
            {
                foreach (string configFile in Directory.GetFiles(InstalledPackagesDir, "*." + FileExtension, SearchOption.AllDirectories))
                {
                    using (FileStream stream = File.OpenRead(configFile))
                    {
                        LocalPackageInfo info = LocalPackageInfo.Load(stream);
                        list._packages.Add(info);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Convenience method that returns the local version of a given package.
        /// </summary>
        /// <param name="identifier">The identifier of the package to return its local version.</param>
        /// <returns>The local version of the given package. -or- null, if the package was not installed.</returns>
        internal Version GetLocalVersionOfPackage(string identifier)
        {
            LocalPackageInfo info = Packages.FirstOrDefault(p => p.Identifier == identifier);
            if (info != null)
            {
                return info.Version;
            }
            return null;
        }

        /// <summary>
        /// Creates a new local package info file or updates the existing one.
        /// Call <see cref="StoreLocalPackageInfos()"/> to persist the files to the disk.
        /// </summary>
        /// <param name="info"></param>
        internal void SetLocalPackageInfo(LocalPackageInfo info)
        {
            LocalPackageInfo existing = Packages.FirstOrDefault(p => p.Identifier == info.Identifier);
            if (existing == null)
            {
                _packages.Add(info);
                existing = info;
            }

            existing.Version = info.Version;

            StoreLocalPackageInfo(existing);
        }

        private void StoreLocalPackageInfo(LocalPackageInfo info)
        {
            FileInfo infoFile = new FileInfo(Path.Combine(InstalledPackagesDir, info.Identifier + "." + FileExtension));
            if (!infoFile.Directory.Exists)
            {
                infoFile.Directory.Create();
            }

            using (FileStream stream = infoFile.OpenWrite())
            {
                using (Stream source = info.Save())
                {
                    source.CopyTo(stream);
                }
            }
        }

        /// <summary>
        /// Stores the package data in the local cache.
        /// </summary>
        /// <param name="identifier">The identifier of the package to store.</param>
        /// <param name="version">The version of the package to store.</param>
        /// <param name="data">The data which represents the package to store. The stream is left open after this method exits.</param>
        internal void StorePackageInCache(string identifier, Version version, Stream data)
        {
            FileInfo cacheFile = new FileInfo(Path.Combine(InstalledPackagesDir, string.Format("{0}.{1}.cache", identifier, version)));
            if (!cacheFile.Directory.Exists)
            {
                cacheFile.Directory.Create();
            }

            using (FileStream fs = cacheFile.OpenWrite())
            {
                data.CopyTo(fs);
            }
        }

        #endregion

    }
}
