﻿using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using AlarmWorkflow.Shared.Core;

namespace AlarmWorkflow.Windows.Configuration
{
    static class ServiceHelper
    {
        internal static readonly string ServiceName = "AlarmworkflowService";
        internal static readonly string ServiceExecutableName = "AlarmWorkflow.Windows.Service.exe";

        internal static bool IsServiceInstalled()
        {
            try
            {
                return ServiceController.GetServices().Any(s => s.ServiceName == ServiceName);
            }
            catch (Exception)
            {
                // We catch this exception because it tells us that the service is not installed!
            }
            return false;
        }

        internal static void InstallService()
        {
            InstallService(true);
        }

        internal static void UninstallService()
        {
            InstallService(false);
        }

        private static void InstallService(bool install)
        {
            string path = Path.Combine(Utilities.GetWorkingDirectory(), ServiceExecutableName);
            List<string> args = new List<string>();

            if (!install)
            {
                args.Add("/u");
            }
            args.Add(path);

            ManagedInstallerClass.InstallHelper(args.ToArray());
        }

        internal static bool IsServiceRunning()
        {
            try
            {
                if (IsServiceInstalled())
                {
                    return GetServiceState() == ServiceControllerStatus.Running;
                }
            }
            catch (Exception)
            {
                // Swallowed, because this means NO.
            }
            return false;
        }

        internal static void StopService(bool throwOnError)
        {
            ServiceController service = new ServiceController(ServiceName);
            try
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(1d));
            }
            catch (Exception ex)
            {
                if (throwOnError)
                {
                    throw ex;
                }
            }
        }

        internal static void StartService(bool throwOnError)
        {
            ServiceController service = new ServiceController(ServiceName);
            try
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1d));
            }
            catch (Exception ex)
            {
                if (throwOnError)
                {
                    throw ex;
                }
            }
        }

        internal static ServiceControllerStatus GetServiceState()
        {
            ServiceController service = new ServiceController(ServiceName);
            return service.Status;
        }
    }
}
