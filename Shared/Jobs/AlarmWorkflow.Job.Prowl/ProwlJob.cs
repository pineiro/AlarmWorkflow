﻿using System;
using AlarmWorkflow.Shared.Core;
using AlarmWorkflow.Shared.Diagnostics;
using AlarmWorkflow.Shared.Engine;
using AlarmWorkflow.Shared.Extensibility;
using AlarmWorkflow.Shared.Settings;
using Prowl;

namespace AlarmWorkflow.Job.Prowl
{
    [Export("ProwlJob", typeof(IJob))]
    class ProwlJob : IJob
    {
        #region Fields

        private ProwlClientConfiguration _configuration;
        private ProwlClient _client;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ProwlJob class.
        /// </summary>
        public ProwlJob()
        {
        }

        #endregion

        #region IJob Members

        bool IJob.Initialize()
        {
            try
            {
                _configuration = new ProwlClientConfiguration();

                _configuration.ApplicationName = SettingsManager.Instance.GetSetting("ProwlJob", "ApplicationName").GetString();
                _configuration.ProviderKey = SettingsManager.Instance.GetSetting("ProwlJob", "ProviderKey").GetString();

                string apiKeychain = string.Join(",", SettingsManager.Instance.GetSetting("ProwlJob", "API").GetStringArray());
                _configuration.ApiKeychain = apiKeychain;

                _client = new ProwlClient(_configuration);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFormat(LogType.Error, this, "Error while initializing Prowl.", ex);
                return false;
            }
        }

        void IJob.Execute(IJobContext context, Operation operation)
        {
            // Construct Notification text
            string body = "Einsatz:\r\n";
            body += "Zeitstempel: " + operation.Timestamp.ToString() + "\r\n";
            body += "Stichwort: " + operation.Keywords.Keyword + "\r\n";
            body += "Meldebild: " + operation.Picture + "\r\n";
            body += "Einsatznr: " + operation.OperationNumber + "\r\n";
            body += "Hinweis: " + operation.Comment + "\r\n";
            body += "Mitteiler: " + operation.Messenger + "\r\n";
            body += "Einsatzort: " + operation.Einsatzort.Location + "\r\n";
            body += "Straße: " + operation.Einsatzort.Street + " " + operation.Einsatzort.StreetNumber + "\r\n";
            body += "Ort: " + operation.Einsatzort.ZipCode + " " + operation.Einsatzort.City + "\r\n";
            body += "Objekt: " + operation.Einsatzort.Property + "\r\n";

            ProwlNotification notifi = new ProwlNotification();
            notifi.Priority = ProwlNotificationPriority.Emergency;
            notifi.Event = "Feuerwehr Einsatz";
            notifi.Description = body;

            //Send the Message
            try
            {
                _client.PostNotification(notifi);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFormat(LogType.Error, this, "An error occurred while sending the Prowl Messages.", ex);
            }
        }

        bool IJob.IsAsync
        {
            get { return false; }
        }

        #endregion

        #region IDisposable Members

        void System.IDisposable.Dispose()
        {

        }

        #endregion
    }
}
