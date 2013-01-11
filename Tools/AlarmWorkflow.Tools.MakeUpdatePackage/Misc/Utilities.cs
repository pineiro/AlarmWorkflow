﻿using System;
using System.IO;
using System.Windows.Forms;

namespace AlarmWorkflow.Tools.MakeUpdatePackage.Misc
{
    static class Utilities
    {
        internal static DirectoryInfo GetProjectRootDirectory()
        {
            return new DirectoryInfo(Path.Combine(Application.StartupPath, Properties.Settings.Default.ProjectRootDirectory));
        }
    }
}
