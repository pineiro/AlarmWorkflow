/*
 * See "(Root)/Resources/Versioning/InstallerClassTemplate/InstallerTemplate.cs" for documentation.
 */

using System.Collections.Generic;
class Installer
{
    string[] OnInstall(
        string installDirectory,
        System.Version oldVersion,
        System.Version newVersion)
    {
        List<string> log = new List<string>();
        log.Add("iSuccess!");

        return log.ToArray();
    }
}