using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using AlarmWorkflow.Tools.MakeUpdatePackage.Misc;
using Ionic.Zip;

namespace AlarmWorkflow.Tools.MakeUpdatePackage.Forms
{
    public partial class MainForm : Form
    {
        #region Fields

        private List<PackageDefinition> _packages;

        #endregion

        #region Constructors

        public MainForm()
        {
            _packages = new List<PackageDefinition>();

            InitializeComponent();
            ListPackages();

            txtOutputDirectory.Text = Path.Combine(Application.StartupPath, "GeneratedPackages");
        }

        #endregion

        #region Event handlers

        private void btnCreate_Click(object sender, System.EventArgs e)
        {
            //Context context = new Context();
            //context.NewVersion = Version.Parse(txtVersion.Text);
            //context.ProjectRootDirectory = Utilities.GetProjectRootDirectory();
            //context.InstallerTempDirectory = new DirectoryInfo(txtOutputDirectory.Text);
            //if (!context.InstallerTempDirectory.Exists)
            //{
            //    context.InstallerTempDirectory.Create();
            //}

            //ITask task = new ZipFolderTask();
            //task.Execute(context);

            PackageDefinition package = _packages.First(p => p.Identifier == cboPackageList.Text);
            string buildDir = Path.Combine(Utilities.GetProjectRootDirectory().FullName, "Build");
            string outDir = Path.Combine(txtOutputDirectory.Text, package.Identifier);
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            using (ZipFile zip = new ZipFile(Path.Combine(outDir, txtVersion.Text + ".zip")))
            {
                foreach (var file in package.IncludedFiles)
                {
                    string fileAbs = Path.Combine(buildDir, file.FileName);
                    Stream fs = File.OpenRead(fileAbs);
                    zip.AddEntry(file.FileName, fs);
                }

                zip.Save();
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
                    _packages.Add(pd);
                }
            }

            _packages = new List<PackageDefinition>(_packages.OrderBy(pd => pd.Identifier));
            cboPackageList.Items.AddRange(_packages.Select(e => e.Identifier).ToArray());

            cboPackageList.SelectedIndex = 0;
        }

        #endregion

    }
}
