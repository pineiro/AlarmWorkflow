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
            if (clbPackages.CheckedItems.Count == 0)
            {
                MessageBox.Show(Properties.Resources.NoPackagesSelectedMessage);
                return;
            }

            foreach (var item in clbPackages.CheckedItems.Cast<object>().ToList())
            {
                string text = (string)item;

                PackageDefinition package = _packages.First(p => p.Identifier == text);
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

                    zip.Save();
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
                    _packages.Add(pd);
                }
            }

            _packages = new List<PackageDefinition>(_packages.OrderBy(pd => pd.Identifier));
            clbPackages.Items.AddRange(_packages.Select(e => e.Identifier).ToArray());
        }

        #endregion

    }
}
