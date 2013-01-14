using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AlarmWorkflow.Tools.AutoUpdater.Network;
using System.Diagnostics;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    class ServerPackageList
    {
        #region Properties

        /// <summary>
        /// Gets the collection containing all server-side available packages.
        /// </summary>
        public ReadOnlyCollection<PackageInformation> Packages { get; private set; }
        /// <summary>
        /// Gets the collection containing the details of all packages.
        /// </summary>
        public ReadOnlyCollection<PackageDetail> PackageDetails { get; private set; }

        #endregion

        #region Constructors

        private ServerPackageList()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the server package list by accessing the given server.
        /// </summary>
        /// <param name="serverClient"></param>
        /// <returns></returns>
        internal static ServerPackageList Create(IServerClient serverClient)
        {
            ServerPackageList list = new ServerPackageList();
            CreatePackageInformationList(serverClient, list);
            CreatePackageDetails(serverClient, list);
            return list;
        }

        private static void CreatePackageInformationList(IServerClient serverClient, ServerPackageList list)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Stream stream = serverClient.DownloadServerPackageList();

            if (stream == null)
            {
                throw new InvalidOperationException("Could not retrieve packages list from server!");
            }

            using (stream)
            {
                XDocument document = XDocument.Load(stream);

                List<PackageInformation> packages = new List<PackageInformation>();
                foreach (XElement packageE in document.Root.Elements("Package"))
                {
                    PackageInformation package = ParsePackageInformationElement(packageE);
                    if (package == null)
                    {
                        continue;
                    }
                    packages.Add(package);
                }
                list.Packages = new ReadOnlyCollection<PackageInformation>(packages);
            }

            sw.Stop();
            Log.Write(Properties.Resources.LogPackageListDownloadedInMs, sw.ElapsedMilliseconds);
        }

        private static PackageInformation ParsePackageInformationElement(XElement packageE)
        {
            PackageInformation package = new PackageInformation();

            package.Identifier = packageE.Attribute("Id").Value;
            package.DisplayName = packageE.Attribute("DisplayName").Value;
            package.Category = packageE.Attribute("Category").Value;
            package.Author = packageE.Attribute("Author").Value;
            package.Description = packageE.Element("Description").Value;
            package.State = ReadPackageState(packageE);
            package.Recommendation = ReadPackageRecommendation(packageE);

            foreach (XElement dependencyE in packageE.Element("Dependencies").Elements("Id"))
            {
                package.Dependencies.Add(dependencyE.Value);
            }

            return package;
        }

        private static PackageInformation.PackageState ReadPackageState(XElement packageE)
        {
            XAttribute stateA = packageE.Attribute("State");
            if (stateA != null)
            {
                return (PackageInformation.PackageState)Enum.Parse(typeof(PackageInformation.PackageState), stateA.Value, false);
            }
            return PackageInformation.PackageState.Active;
        }

        private static PackageInformation.RecommendationType ReadPackageRecommendation(XElement packageE)
        {
            XAttribute recA = packageE.Attribute("Recommendation");
            if (recA != null)
            {
                return (PackageInformation.RecommendationType)Enum.Parse(typeof(PackageInformation.RecommendationType), recA.Value, false);
            }
            return PackageInformation.RecommendationType.None;
        }

        private static void CreatePackageDetails(IServerClient serverClient, ServerPackageList list)
        {
            List<PackageDetail> details = new List<PackageDetail>();
            foreach (PackageInformation package in list.Packages)
            {
                Stopwatch sw = Stopwatch.StartNew();
                Stream stream = serverClient.DownloadPackageDetail(package.Identifier);
                if (stream == null)
                {
                    continue;
                }

                using (stream)
                {
                    XDocument document = XDocument.Load(stream);

                    var d = PackageDetail.FromDocument(document);
                    d.ParentIdentifier = package.Identifier;

                    details.Add(d);
                }

                sw.Stop();
                Log.Write(Properties.Resources.LogPackageDetailDownloadedInMs, package.Identifier, sw.ElapsedMilliseconds);

            }
            list.PackageDetails = new ReadOnlyCollection<PackageDetail>(details);
        }

        private PackageInformation GetPackageFromIdentifier(string identifier)
        {
            return Packages.FirstOrDefault(p => p.Identifier == identifier);
        }

        private PackageDetail GetPackageDetailFromIdentifier(string identifier)
        {
            return PackageDetails.FirstOrDefault(p => p.ParentIdentifier == identifier);
        }

        /// <summary>
        /// Returns all direct and indirect dependencies of the package with the given identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        internal IEnumerable<string> GetDependenciesOfPackage(string identifier)
        {
            PackageInformation package = GetPackageFromIdentifier(identifier);
            if (package == null)
            {
                yield break;
            }

            foreach (string dep in package.Dependencies)
            {
                yield return dep;
                foreach (string pkgSrvDep in GetDependenciesOfPackage(dep))
                {
                    yield return pkgSrvDep;
                }
            }
        }

        /// <summary>
        /// Returns all versions of the package, sorted from newest to oldest.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        internal IEnumerable<Version> GetVersionsOfPackage(string identifier)
        {
            PackageDetail detail = GetPackageDetailFromIdentifier(identifier);
            if (detail == null)
            {
                return null;
            }

            return detail.Versions.Select(v => v.Version);
        }

        #endregion
    }
}
