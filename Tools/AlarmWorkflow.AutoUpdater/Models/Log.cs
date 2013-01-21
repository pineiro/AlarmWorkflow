using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AlarmWorkflow.Tools.AutoUpdater
{
    /// <summary>
    /// Provides an interface for logging.
    /// </summary>
    static class Log
    {
        #region Fields

        private static readonly List<string> _entries;
        /// <summary>
        /// Occurs if new text has been logged.
        /// </summary>
        internal static event PostTextDelegate PostText;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a collection that contains all entries logged so far.
        /// </summary>
        internal static ReadOnlyCollection<string> Entries { get; private set; }

        #endregion

        #region Constructors

        static Log()
        {
            _entries = new List<string>();
            Entries = new ReadOnlyCollection<string>(_entries);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Logs some text.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
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

        #endregion

        #region Nested types

        internal delegate void PostTextDelegate(string text);

        #endregion

    }
}
