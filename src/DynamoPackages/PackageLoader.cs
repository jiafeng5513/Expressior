using System;
using System.Collections.Generic;
using System.Reflection;
using Dynamo.Interfaces;
using Dynamo.Logging;
using DynamoUtilities;
using Dynamo.Core;
using Dynamo.Extensions;

namespace Dynamo.PackageManager
{
    public struct LoadPackageParams
    {
        public IPreferences Preferences { get; set; }
        public IPathManager PathManager { get; set; }
    }

    public class PackageLoader : LogSourceBase
    {
        internal event Action<Assembly> RequestLoadNodeLibrary;
        internal event Func<string, IEnumerable<CustomNodeInfo>> RequestLoadCustomNodeDirectory;
        internal event Func<string, IExtension> RequestLoadExtension;
        internal event Action<IExtension> RequestAddExtension;

        private readonly List<IExtension> requestedExtensions = new List<IExtension>();
        /// <summary>
        /// Collection of ViewExtensions the ViewExtensionSource requested be loaded.
        /// </summary>
        public IEnumerable<IExtension> RequestedExtensions
        {
            get
            {
                return requestedExtensions;
            }
        }

        private readonly List<string> packagesDirectories = new List<string>();
        public string DefaultPackagesDirectory
        {
            get { return packagesDirectories[0]; }
        }

        public PackageLoader(IEnumerable<string> packagesDirectories)
        {
            if (packagesDirectories == null)
                throw new ArgumentNullException("packagesDirectories");

            this.packagesDirectories.AddRange(packagesDirectories);
            var error = PathHelper.CreateFolderIfNotExist(DefaultPackagesDirectory);

            if (error != null)
                Log(error);
        }

        public void LoadCustomNodesAndPackages(LoadPackageParams loadPackageParams, CustomNodeManager customNodeManager)
        {
            foreach (var path in loadPackageParams.Preferences.CustomPackageFolders)
            {
                customNodeManager.AddUninitializedCustomNodesInPath(path, false);
                if (!this.packagesDirectories.Contains(path))
                {
                    this.packagesDirectories.Add(path);
                }
            }
        }

        private static bool hasAttemptedUninstall;

    }
}
