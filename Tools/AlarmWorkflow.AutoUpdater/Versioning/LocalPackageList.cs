using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
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
            }
            else
            {
                existing.Version = info.Version;
            }
        }

        internal static LocalPackageList Build()
        {
            LocalPackageList list = new LocalPackageList();

            if (Directory.Exists(InstalledPackagesDir))
            {
                foreach (string configFile in Directory.GetFiles(InstalledPackagesDir, "*." + FileExtension, SearchOption.AllDirectories))
                {
                    LocalPackageInfo info = LocalPackageInfo.Load(configFile);
                    list._packages.Add(info);
                }
            }

            return list;
        }

        /// <summary>
        /// Stores all local package infos on the disk.
        /// </summary>
        internal void StoreLocalPackageInfos()
        {
            EnsureAutoUpdaterDirectoriesExists();

            foreach (LocalPackageInfo info in _packages)
            {
                string path = Path.Combine(InstalledPackagesDir, info.Identifier + "." + FileExtension);
                info.Save(path);
            }
        }

        internal void StorePackageInCache(string identifier, Version version, Stream data)
        {
            EnsureAutoUpdaterDirectoriesExists();

            FileInfo cacheFile = new FileInfo(Path.Combine(InstalledPackagesDir, string.Format("{0}.{1}.cache", identifier, version)));
            if (!cacheFile.Directory.Exists)
            {
                cacheFile.Directory.Create();
            }

            using (FileStream fs = File.Create(cacheFile.FullName))
            {
                data.CopyTo(fs);
            }
        }

        private void EnsureAutoUpdaterDirectoriesExists()
        {
            if (!Directory.Exists(AutoUpdaterCacheDir))
            {
                Directory.CreateDirectory(AutoUpdaterCacheDir);
            }
            if (!Directory.Exists(InstalledPackagesDir))
            {
                Directory.CreateDirectory(InstalledPackagesDir);
            }
        }

        #endregion

    }
}
