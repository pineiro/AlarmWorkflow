using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AlarmWorkflow.Tools.AutoUpdater.Network;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Represents the package list that is downloaded from the server.
    /// </summary>
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
                    // Yield warning and skip if package with duplicate id detected
                    if (packages.Contains(package))
                    {
                        Log.Write(Properties.Resources.LogDuplicateServerPackageDetected, package.Identifier);
                        continue;
                    }
                    if (package.State == PackageState.Ignored)
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

        private static PackageState ReadPackageState(XElement packageE)
        {
            XAttribute stateA = packageE.Attribute("State");
            if (stateA != null)
            {
                return (PackageState)Enum.Parse(typeof(PackageState), stateA.Value, false);
            }
            return PackageState.Active;
        }

        private static PackageRecommendation ReadPackageRecommendation(XElement packageE)
        {
            XAttribute recA = packageE.Attribute("Recommendation");
            if (recA != null)
            {
                return (PackageRecommendation)Enum.Parse(typeof(PackageRecommendation), recA.Value, false);
            }
            return PackageRecommendation.None;
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
            // TODO: This method doesn't do a duplicate check, meaning that it gives an endless cycle if the package list is poorly constructed!
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
