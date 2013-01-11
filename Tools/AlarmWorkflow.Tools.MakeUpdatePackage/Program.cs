using System.Windows.Forms;
using AlarmWorkflow.Tools.MakeUpdatePackage.Forms;

namespace AlarmWorkflow.Tools.MakeUpdatePackage
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}
