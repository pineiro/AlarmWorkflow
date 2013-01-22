using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace AlarmWorkflow.Tools.MakeUpdatePackage.Misc
{
    class PackageDefinition
    {
        #region Properties

        /// <summary>
        /// Gets/sets the file from which the package definition was loaded.
        /// </summary>
        internal System.IO.FileInfo SourceFile { get; set; }

        internal string Identifier { get; set; }
        internal string InstallerClassFile { get; set; }
        internal List<File> IncludedFiles { get; set; }

        #endregion

        #region Constructors

        public PackageDefinition()
        {
            IncludedFiles = new List<File>();
        }

        #endregion

        #region Methods

        internal static PackageDefinition Parse(XDocument doc)
        {
            PackageDefinition def = new PackageDefinition();
            def.Identifier = doc.Root.Element("Identifier").Value;
            def.InstallerClassFile = TryGetInstallerClassFile(doc.Root);

            foreach (XElement fileE in doc.Root.Element("PackageFiles").Elements("File"))
            {
                File file = new File();
                file.FileName = fileE.Value;

                if (file.FileName.StartsWith("$"))
                {
                    Debug.WriteLine("File names must not start with '$' (reserved).");
                    return null;
                }

                XAttribute keepA = fileE.Attribute("Keep");
                if (keepA != null)
                {
                    file.Keep = bool.Parse(keepA.Value);
                }

                if (!def.IncludedFiles.Contains(file))
                {
                    def.IncludedFiles.Add(file);
                }
            }

            return def;
        }

        private static string TryGetInstallerClassFile(XElement rootE)
        {
            XElement installerClassFileE = rootE.Element("InstallerClassFile");
            if (installerClassFileE == null)
            {
                return null;
            }
            return installerClassFileE.Value;
        }

        #endregion

        #region Nested types

        internal class File : IEquatable<File>
        {
            /// <summary>
            /// Gets/sets the file that is included in the package. This file is relative to the output directory.
            /// </summary>
            internal string FileName { get; set; }
            /// <summary>
            /// Gets/sets whether or not to keep the file if uninstalling.
            /// </summary>
            internal bool Keep { get; set; }

            #region IEquatable<File> Members

            bool IEquatable<File>.Equals(File other)
            {
                return other.FileName.Equals(this.FileName, StringComparison.OrdinalIgnoreCase);
            }

            #endregion
        }

        #endregion

    }
}
