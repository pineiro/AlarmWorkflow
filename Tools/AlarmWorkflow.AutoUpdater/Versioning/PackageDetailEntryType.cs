
namespace AlarmWorkflow.Tools.AutoUpdater.Versioning
{
    /// <summary>
    /// Specifies the entry classification.
    /// </summary>
    enum PackageDetailEntryType
    {
        /// <summary>
        /// This is a regular update.
        /// </summary>
        Regular = 0,
        /// <summary>
        /// This update contains minor improvements or small fixes.
        /// </summary>
        Minor,
        /// <summary>
        /// This update contains bug fixes and installation is recommended.
        /// </summary>
        Bugfix,
        /// <summary>
        /// This update is classified as critical, meaning that installation is explicitely recommended.
        /// </summary>
        Critical,
        /// <summary>
        /// This update contains major feature changes or improvements.
        /// </summary>
        Major,
    }
}
