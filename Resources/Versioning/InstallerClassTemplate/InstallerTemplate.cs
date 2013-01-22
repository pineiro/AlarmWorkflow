/*
 * This file contains the installer logic. It is compiled into its own assembly, and then invoked by the updater.
 * 
 * It is important that you don't have namespaces, that the type is always called "Installer" and that the methods are exactly called 
 * and have the same signatures as they have in this sample. Otherwise the installer will not execute.
 * 
 * Also keep in mind to NOT have any references to classes other than the .Net-Framework!
 * 
 */
 
class Installer
{
    /// <summary>
    /// This method is invoked when installing on the client-side. It is invoked PRIOR to the files being copied.
    /// </summary>
    /// <param name="installDirectory">The absolute path to the directory where the package is installed to.
    /// This is the same directory where the AutoUpdater is placed.</param>
    /// <param name="oldVersion">The old version of the package, if any. If this is a fresh install, then this parameter has the value <c>null</c>.</param>
    /// <param name="newVersion">The new version of the package.</param>
    /// <returns>A collection of all messages that occurred during installation. First character determines type. Use "e" for errors, "w" for warnings, "i" for info.</returns>
    string[] OnInstall(
        string installDirectory, 
        System.Version oldVersion,
        System.Version newVersion)
    {
        return null;
    }
}