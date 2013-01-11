using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AlarmWorkflow.Tools.MakeUpdatePackage.Misc
{
    class PackageDefinition
    {
        #region Properties

        internal string Identifier { get; set; }
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

            foreach (XElement fileE in doc.Root.Element("PackageFiles").Elements("File"))
            {
                File file = new File();
                file.FileName = fileE.Value;

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
