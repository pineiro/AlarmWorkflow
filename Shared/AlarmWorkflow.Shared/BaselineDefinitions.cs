using System;

namespace AlarmWorkflow
{
    /// <summary>
    /// Contains definitions that are used across all AlarmWorkflow-related projects.
    /// </summary>
    public sealed class BaselineDefinitions
    {
        /// <summary>
        /// Returns the full path name of the directory that shall store all user-specific application data.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalAppDataFolderPath()
        {
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "OpenFireSource", "AlarmWorkflow");
        }
    }
}
