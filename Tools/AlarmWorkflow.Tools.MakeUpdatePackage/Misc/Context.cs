using System;
using System.IO;

namespace AlarmWorkflow.Tools.MakeUpdatePackage.Misc
{
    class Context
    {
        internal DirectoryInfo ProjectRootDirectory { get; set; }
        internal DirectoryInfo InstallerTempDirectory { get; set; }
        internal Version NewVersion { get; set; }
    }
}
