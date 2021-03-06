﻿using System;

namespace AlarmWorkflow.Shared.Addressing.EntryObjects
{
    /// <summary>
    /// Represents a "Growl" entry in the address book. 
    /// See documentation for further information.
    /// </summary>
    /// <remarks>This is a generalized way to talk any growl-consumer, including (but not limited to): Prowl, Growl etc.</remarks>
    public class GrowlEntryObject
    {
        #region Fields

        /// <summary>
        /// Defines the consumers that are supported by default.
        /// </summary>
        public static readonly string[] DefaultConsumers = { "Growl", "Prowl" };

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the name of the Growl-consumer.
        /// </summary>
        public string Consumer { get; set; }
        /// <summary>
        /// Gets/sets the API-key of the recipient.
        /// </summary>
        public string RecipientApiKey { get; set; }

        #endregion
    }
}
