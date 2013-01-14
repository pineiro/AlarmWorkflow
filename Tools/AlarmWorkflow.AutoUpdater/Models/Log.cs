using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AlarmWorkflow.Tools.AutoUpdater
{
    static class Log
    {
        private static readonly List<string> _entries;
        internal static ReadOnlyCollection<string> Entries { get; private set; }

        static Log()
        {
            _entries = new List<string>();
            Entries = new ReadOnlyCollection<string>(_entries);
        }

        internal delegate void PostTextDelegate(string text);

        internal static event PostTextDelegate PostText;

        internal static void Write(string format, params object[] args)
        {
            string text = string.Format(format, args);

            _entries.Add(text);

            var copy = PostText;
            if (copy != null)
            {
                copy(text);
            }
        }

    }
}
