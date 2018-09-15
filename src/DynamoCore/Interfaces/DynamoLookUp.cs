using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Updates;

namespace Dynamo.Interfaces
{
    /// <summary>
    /// Interface provides methods, that get installed Dynamo paths and the last Dynamo version.
    /// </summary>
    public interface IDynamoLookUp
    {
        /// <summary>
        /// Returns installation path for all version of this Dynamo Product
        /// installed on this system.
        /// </summary>
        IEnumerable<string> GetDynamoInstallLocations();

        /// <summary>
        /// Returns a list of user data folders on this system.
        /// </summary>
        /// <returns>
        /// The implementation of this interface method should return a list of user 
        /// data folders, one for each of Dynamo product installed on the system. When 
        /// there is no Dynamo product installed, this method returns an empty list.
        /// </returns>
        IEnumerable<string> GetDynamoUserDataLocations();

        /// <summary>
        /// Returns the version of latest installed product
        /// </summary>
        BinaryVersion LatestProduct { get; }
    }
    internal abstract class DynamoLookUp : IDynamoLookUp
    {
        /// <summary>
        /// Returns the version of latest product
        /// </summary>
        public BinaryVersion LatestProduct { get { return GetLatestInstallVersion(); } }

        /// <summary>
        /// Locates DynamoCore.dll at given install path and gets file version
        /// </summary>
        /// <param name="installPath">Dynamo install path</param>
        /// <returns>Dynamo version if valid Dynamo exists else null</returns>
        public virtual Version GetDynamoVersion(string installPath)
        {
            if (!Directory.Exists(installPath))//null or empty installPath will return false
                return null;

            var filePath = Directory.GetFiles(installPath, "*DynamoCore.dll").FirstOrDefault();
            return String.IsNullOrEmpty(filePath) ? null : Version.Parse(FileVersionInfo.GetVersionInfo(filePath).FileVersion);
        }

        /// <summary>
        /// Returns all dynamo install path on the system by looking into the Windows registry. 
        /// </summary>
        /// <returns>List of Dynamo install path</returns>
        public abstract IEnumerable<string> GetDynamoInstallLocations();

        /// <summary>
        /// Returns the full path of user data location of all version of this
        /// Dynamo product installed on this system. The default implementation
        /// returns list of all subfolders in %appdata%\Dynamo as well as 
        /// %appdata%\Dynamo\Dynamo Core\ folders.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<string> GetDynamoUserDataLocations()
        {
            var appDatafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dynamoFolder = Path.Combine(appDatafolder, "Dynamo");
            if (!Directory.Exists(dynamoFolder)) return Enumerable.Empty<string>();

            var paths = new List<string>();
            var coreFolder = Path.Combine(dynamoFolder, "Dynamo Core");
            //Dynamo Core folder has to be enumerated first to cater migration from
            //Dynamo 1.0 to Dynamo Core 1.0
            if (Directory.Exists(coreFolder))
            {
                paths.AddRange(Directory.EnumerateDirectories(coreFolder));
            }

            paths.AddRange(Directory.EnumerateDirectories(dynamoFolder));
            return paths;
        }

        private BinaryVersion GetLatestInstallVersion()
        {
            var dynamoInstallations = GetDynamoInstallLocations();
            if (null == dynamoInstallations)
                return null;

            var latestVersion =
                dynamoInstallations.Select(GetDynamoVersion).OrderBy(s => s).LastOrDefault();
            return latestVersion == null ? null : BinaryVersion.FromString(latestVersion.ToString());
        }
    }
}

