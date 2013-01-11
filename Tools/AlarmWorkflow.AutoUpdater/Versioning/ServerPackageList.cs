using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using AlarmWorkflow.Tools.AutoUpdater.Network;

namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    class ServerPackageList
    {
        #region Properties

        /// <summary>
        /// Gets the collection containing all server-side available packages.
        /// </summary>
        public ReadOnlyCollection<PackageInformation> Packages { get; private set; }
        public ReadOnlyCollection<PackageDetail> PackageDetails { get; private set; }

        #endregion

        #region Constructors

        private ServerPackageList()
        {

        }

        #endregion

        #region Methods

        internal static ServerPackageList Create(IServerClient serverClient)
        {
            ServerPackageList list = new ServerPackageList();
            CreatePackageInformationList(serverClient, list);
            CreatePackageDetails(serverClient, list);
            return list;
        }

        private static void CreatePackageInformationList(IServerClient serverClient, ServerPackageList list)
        {
            using (Stream stream = serverClient.DownloadServerPackageList())
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

        private static void CreatePackageDetails(IServerClient serverClient, ServerPackageList list)
        {
            List<PackageDetail> details = new List<PackageDetail>();
            foreach (PackageInformation package in list.Packages)
            {
                using (Stream stream = serverClient.DownloadPackageDetail(package.Identifier))
                {
                    if (stream == null)
                    {
                        continue;
                    }
                    XDocument document = XDocument.Load(stream);

                    var d = PackageDetail.FromDocument(document);
                    d.ParentIdentifier = package.Identifier;

                    details.Add(d);
                }
            }
            list.PackageDetails = new ReadOnlyCollection<PackageDetail>(details);
        }

        #endregion
    }
}
