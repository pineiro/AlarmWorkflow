using System.IO;
using System.Windows.Forms;

namespace AlarmWorkflow.Tools.AutoUpdater
{
    static class Utilities
    {
        internal static void ShowMessageBox(MessageBoxIcon icon, string format, params object[] args)
        {
            string msg = string.Format(format, args);
            MessageBox.Show(msg, "", MessageBoxButtons.OK, icon);
        }

        internal static bool ConfirmMessageBox(string format, params object[] args)
        {
            string msg = string.Format(format, args);
            DialogResult dr = MessageBox.Show(msg, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return dr == DialogResult.Yes;
        }

        internal static string GetWorkingDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Copies the provided stream to a new <see cref="MemoryStream"/>, sets it to position zero and returns it.
        /// </summary>
        /// <param name="source">The stream to copy. The stream is returned open and in position zero.</param>
        /// <returns></returns>
        internal static Stream Copy(this Stream source)
        {
            source.Position = 0L;

            MemoryStream dest = new MemoryStream();
            source.CopyTo(dest);
            dest.Position = 0L;

            source.Position = 0L;

            return dest;
        }
    }
}
