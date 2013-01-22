using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using AlarmWorkflow.Tools.MakeUpdatePackage.Misc;
using Ionic.Zip;
using Microsoft.CSharp;

namespace AlarmWorkflow.Tools.MakeUpdatePackage.Forms
{
    /// <summary>
    /// Interaction logic for the MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        #region Fields

        private List<PackageDefinition> _packages;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            _packages = new List<PackageDefinition>();

            InitializeComponent();
            ListPackages();

            txtOutputDirectory.Text = Path.Combine(Application.StartupPath, "GeneratedPackages");
        }

        #endregion

        #region Event handlers

        private void tsmSelectAll_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < clbPackages.Items.Count; i++)
            {
                clbPackages.SetItemChecked(i, true);
            }
        }

        private void tsmDeselectAll_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < clbPackages.Items.Count; i++)
            {
                clbPackages.SetItemChecked(i, false);
            }
        }

        private void btnCreate_Click(object sender, System.EventArgs e)
        {
            if (clbPackages.CheckedItems.Count == 0)
            {
                MessageBox.Show(Properties.Resources.NoPackagesSelectedMessage);
                return;
            }

            foreach (object item in clbPackages.CheckedItems.Cast<object>().ToList())
            {
                string packageIdentifier = (string)item;
                CreatePackage(packageIdentifier);
            }

            MessageBox.Show("Package(s) created!");
        }

        private void CreatePackage(string packageIdentifier)
        {
            PackageDefinition package = _packages.First(p => p.Identifier == packageIdentifier);

            string buildDir = Path.Combine(Utilities.GetProjectRootDirectory().FullName, "Build");
            string outDir = Path.Combine(txtOutputDirectory.Text, package.Identifier);
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            string zipFileName = Path.Combine(outDir, txtVersion.Text + ".zip");
            if (File.Exists(zipFileName))
            {
                File.Delete(zipFileName);
            }

            using (ZipFile zip = new ZipFile(zipFileName))
            {
                foreach (var fileName in package.IncludedFiles)
                {
                    FileInfo file = new FileInfo(Path.Combine(buildDir, fileName.FileName));
                    if (!file.Exists)
                    {
                        Utilities.ShowMessageBox(MessageBoxIcon.Error, Properties.Resources.FileNotFoundMessage, file.FullName);
                        return;
                    }

                    Stream fs = file.OpenRead();
                    zip.AddEntry(fileName.FileName, fs);
                }

                // When done, create the installer assembly
                CreateInstallerAssembly(package, zip);

                zip.Save();
            }
        }

        private void CreateInstallerAssembly(PackageDefinition package, ZipFile zip)
        {
            if (string.IsNullOrWhiteSpace(package.InstallerClassFile))
            {
                return;
            }

            string classFile = Path.Combine(package.SourceFile.DirectoryName, package.InstallerClassFile);
            if (!File.Exists(classFile))
            {
                return;
            }

            using (CSharpCodeProvider compiler = new CSharpCodeProvider())
            {
                CompilerParameters options = new CompilerParameters();
                options.OutputAssembly = Path.GetTempFileName();
                options.ReferencedAssemblies.Add("System.dll");
                options.ReferencedAssemblies.Add("System.Data.dll");
                options.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                options.ReferencedAssemblies.Add("System.Xml.dll");
                options.ReferencedAssemblies.Add("System.Xml.Linq.dll");

                string source = File.ReadAllText(classFile);

                var results = compiler.CompileAssemblyFromSource(options, source);
                if (results.Errors.HasErrors)
                {

                }
                else if (results.Errors.HasWarnings)
                {

                }

                using (FileStream stream = File.OpenRead(results.PathToAssembly))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        zip.AddEntry("$Installer$.dll", ms.ToArray());
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void ListPackages()
        {
            foreach (FileInfo file in Utilities.GetProjectRootDirectory().GetFiles("*.xml", SearchOption.AllDirectories).Where(f => f.Name == "PackageDefinition.xml"))
            {
                using (Stream stream = file.OpenRead())
                {
                    XDocument doc = XDocument.Load(stream);

                    PackageDefinition pd = PackageDefinition.Parse(doc);
                    if (pd == null)
                    {
                        Debug.WriteLine("Could not load package definition {0}. Please see log for further information.", file.FullName);
                        continue;
                    }

                    pd.SourceFile = file;
                    _packages.Add(pd);
                }
            }

            _packages = new List<PackageDefinition>(_packages.OrderBy(pd => pd.Identifier));
            clbPackages.Items.AddRange(_packages.Select(e => e.Identifier).ToArray());
        }

        #endregion

    }
}
