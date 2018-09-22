using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Updates;
using DynamoShapeManager;
using Microsoft.Win32;
using NDesk.Options;

namespace Dynamo.Applications
{
    public class StartupUtils
    {
        /// <summary>
        /// 在注册表中查找软件的安装位置
        /// </summary>
        internal class SandboxLookUp : DynamoLookUp
        {
            public override IEnumerable<string> GetDynamoInstallLocations()
            {
                const string regKey64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
                //Open HKLM for 64bit registry
                var regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                //Open Windows/CurrentVersion/Uninstall registry key
                regKey = regKey.OpenSubKey(regKey64);

                //Get "InstallLocation" value as string for all the subkey that starts with "Dynamo"
                return regKey.GetSubKeyNames().Where(s => s.StartsWith("Dynamo")).Select(
                    (s) => regKey.OpenSubKey(s).GetValue("InstallLocation") as string);
            }
        }

        public static void PreloadShapeManager(ref string geometryFactoryPath, ref string preloaderLocation)
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var rootFolder = Path.GetDirectoryName(exePath);

            var versions = new[]
            {
                LibraryVersion.Version223, 
                LibraryVersion.Version222,
                LibraryVersion.Version221
            };

            var preloader = new Preloader(rootFolder, versions);
            preloader.Preload();
            geometryFactoryPath = preloader.GeometryFactoryPath;
            preloaderLocation = preloader.PreloaderLocation;
        }

        public static DynamoModel MakeModel()
        {

            var geometryFactoryPath = string.Empty;
            var preloaderLocation = string.Empty;
            PreloadShapeManager(ref geometryFactoryPath, ref preloaderLocation);

            var config = new DynamoModel.DefaultStartConfiguration()
                  {
                      GeometryFactoryPath = geometryFactoryPath,
                      ProcessMode = TaskProcessMode.Asynchronous
                  };

            //config.StartInTestMode = false;
            config.PathResolver = new SandboxPathResolver(preloaderLocation) as IPathResolver ;

            var model = DynamoModel.Start(config);
            return model;
        }

    }
}
