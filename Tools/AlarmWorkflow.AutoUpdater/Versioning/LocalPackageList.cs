using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using AlarmWorkflow.Tools.AutoUpdater.Models;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    class LocalPackageList
    {

        #region Constants

        internal static readonly string PathInAppData = "InstalledPackages";
        internal static readonly string FileExtension = "config";

        #endregion

        #region Properties

        public ReadOnlyCollection<LocalPackageInfo> Packages { get; private set; }

        #endregion

        #region Constructors

        private LocalPackageList()
        {

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

        internal static string GetInstalledPackagesDirectory()
        {
            // Since we can have multiple installation directories (especially when developing) we hash the assembly dir
            string asmDirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dirName = HashHelper.GetHashOfString(asmDirName);
            return Path.Combine(BaselineDefinitions.GetLocalAppDataFolderPath(), LocalPackageList.PathInAppData, dirName);
        }

        internal static LocalPackageList Build()
        {
            List<LocalPackageInfo> tempList = new List<LocalPackageInfo>();

            if (Directory.Exists(LocalPackageList.GetInstalledPackagesDirectory()))
            {
                foreach (string configFile in Directory.GetFiles(LocalPackageList.GetInstalledPackagesDirectory(), "*." + FileExtension, SearchOption.AllDirectories))
                {
                    LocalPackageInfo info = LocalPackageInfo.Load(configFile);
                    tempList.Add(info);
                }
            }

            LocalPackageList list = new LocalPackageList();
            list.Packages = new ReadOnlyCollection<LocalPackageInfo>(tempList);
            return list;
        }

        #endregion

    }
}
