using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using Dynamo.Core;
using Dynamo.Logging;
/*
 * 与更新有关的一切
 */
namespace Dynamo.Updates
{
    /// <summary>
    /// Represents the method that will handle <see cref="UpdateManager.UpdateDownloaded"/> events.
    /// </summary>
    /// <param name="sender">The object where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    public delegate void UpdateDownloadedEventHandler(object sender, UpdateDownloadedEventArgs e);
	
    /// <summary>
    /// A delegate used to handle shutdown request
    /// </summary>
    public delegate void ShutdownRequestedEventHandler(IUpdateManager updateManager);

    /// <summary>
    /// Provides data for <see cref="UpdateManager.UpdateDownloaded"/> events.
    /// </summary>
    public class UpdateDownloadedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDownloadedEventArgs"/> class 
        /// with the error and the update location
        /// </summary>
        /// <param name="error">The exception thrown during downloading update. 
        /// Null if update is downloaded successfully.</param>
        /// <param name="fileLocation">Location where the update has been downloaded to.</param>
        public UpdateDownloadedEventArgs(Exception error, string fileLocation)
        {
            Error = error;
            UpdateFileLocation = fileLocation;
            UpdateAvailable = !string.IsNullOrEmpty(fileLocation);
        }

        /// <summary>
        /// Returns flag which indicates if update has been downloaded.
        /// </summary>
        public bool UpdateAvailable { get; private set; }

        /// <summary>
        /// Returns location where the update has been downloaded to.
        /// </summary>
        public string UpdateFileLocation { get; private set; }

        /// <summary>
        /// Returns exception thrown during downloading update. 
        /// Null if update is downloaded successfully.
        /// </summary>
        public Exception Error { get; private set; }
    }

    /// <summary>
    /// An interface which describes properties and methods for
    /// updating the application.
    /// </summary>
    public interface IUpdateManager
    {
        /// <summary>
        /// Returns current product version.
        /// </summary>
        BinaryVersion ProductVersion { get; }

        /// <summary>
        /// Returns available product version.
        /// </summary>
        BinaryVersion AvailableVersion { get; }

        /// <summary>
        /// Returns information, where version can be updated.
        /// </summary>
        IAppVersionInfo UpdateInfo { get; set; }

        /// <summary>
        /// Bool value indicates if new version is available.
        /// </summary>
        bool IsUpdateAvailable { get; }

        /// <summary>
        /// Event is fired when an update is downloaded.
        /// </summary>
        event UpdateDownloadedEventHandler UpdateDownloaded;

        /// <summary>
        /// Event is fired when Dynamo needs to be restarted.
        /// </summary>
        event ShutdownRequestedEventHandler ShutdownRequested;

        /// <summary>
        /// Checks for product updates in background thread.
        /// </summary>
        /// <param name="request">Asynchronous web request for update data</param>
        void CheckForProductUpdate(IAsynchronousRequest request);

        /// <summary>
        /// Quits and installs new version.
        /// </summary>
        void QuitAndInstallUpdate();

        /// <summary>
        /// This function is called when a Dynamo Model is shutting down.
        /// </summary>
        void HostApplicationBeginQuit();

        /// <summary>
        /// Reads the request's data, and parses for available versions. 
        /// If a more recent version is available, the UpdateInfo object 
        /// will be set.
        /// </summary>
        /// <param name="request">Asynchronous request</param>
        void UpdateDataAvailable(IAsynchronousRequest request);

        /// <summary>
        /// This flag is available via the debug menu to
        /// allow the update manager to check for newer daily builds.
        /// </summary>
        bool CheckNewerDailyBuilds { get; set; }

        /// <summary>
        /// Specifies whether to force update.
        /// </summary>
        bool ForceUpdate { get; set; }

        /// <summary>
        /// Returns a reference to Update Manager Configuration settings.
        /// </summary>
        IUpdateManagerConfiguration Configuration { get; }

        /// <summary>
        /// Event fires, when something should be logged.
        /// </summary>
        event LogEventHandler Log;

        /// <summary>
        /// This function logs a message.
        /// </summary>
        /// <param name="args">LogEventArgs</param>
        void OnLog(LogEventArgs args);

        /// <summary>
        /// Sets application process id. It's used for logging.
        /// </summary>
        /// <param name="id">int</param>
        void RegisterExternalApplicationProcessId(int id);

        /// <summary>
        /// Get the current version of the Host
        /// </summary>
        Version HostVersion { get; set; }

        /// <summary>
        /// Get the current name of the Host
        /// </summary>
        String HostName { get; set; }
    }

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

    /// <summary>
    /// This interface represents configuration properties for Update manager.
    /// </summary>
    public interface IUpdateManagerConfiguration
    {
        /// <summary>
        /// Specifies download location for new installer
        /// </summary>
        string DownloadSourcePath { get; set; }

        /// <summary>
        /// Specifies location for signature file to validate the new installer.
        /// </summary>
        string SignatureSourcePath { get; set; }

        /// <summary>
        /// Specifies whether to consider daily builds for update, default is false.
        /// </summary>
        bool CheckNewerDailyBuild { get; set; }

        /// <summary>
        /// Specifies whether to force update, default value is false.
        /// </summary>
        bool ForceUpdate { get; set; }

        /// <summary>
        /// Returns the base name of the installer to be used for upgrade.
        /// </summary>
        string InstallerNameBase { get; set; }

        /// <summary>
        /// Returns IDynamoLookUp interface to search Dynamo installations on the system.
        /// </summary>
        IDynamoLookUp DynamoLookUp { get; set; }
    }

    /// <summary>
    /// An interface to describe available
    /// application update info.
    /// </summary>
    public interface IAppVersionInfo
    {
        BinaryVersion Version { get; set; }
        string VersionInfoURL { get; set; }
        string InstallerURL { get; set; }
        string SignatureURL { get; set; }
    }

    /// <summary>
    /// An interface to describe an asynchronous web
    /// request for updating data.
    /// </summary>
    public interface IAsynchronousRequest
    {
        /// <summary>
        /// The data returned from the request.
        /// </summary>
        string Data { get; set; }

        /// <summary>
        /// Any error information returned from the request.
        /// </summary>
        string Error { get; set; }

        /// <summary>
        /// Represents the send request link.
        /// </summary>
        Uri Path { get; set; }

        /// <summary>
        /// An action to be invoked upon completion of the request.
        /// This action is invoked regardless of the success of the request.
        /// </summary>
        Action<IAsynchronousRequest> OnRequestCompleted { get; set; }
    }

    /// <summary>
    /// This class returns <see cref="BinaryVersion"/> of Dynamo
    /// </summary>
    public class AppVersionInfo : IAppVersionInfo
    {
        /// <summary>
        /// Returns current Dynamo version
        /// </summary>
        public BinaryVersion Version { get; set; }

        /// <summary>
        /// Returns URL where one can get information about 
        /// current Dynamo version
        /// </summary>
        public string VersionInfoURL { get; set; }
        
        /// <summary>
        /// Returns URL where Dynamo installer can be downloaded from
        /// </summary>
        public string InstallerURL { get; set; }

        /// <summary>
        /// Returns URL where signature file to validate the new installer can be downloaded from
        /// </summary>
        public string SignatureURL { get; set; }
    }

    /// <summary>
    /// The UpdateRequest class encapsulates a request for 
    /// getting update information from the web.
    /// </summary>
    internal class UpdateRequest : IAsynchronousRequest
    {
        /// <summary>
        /// An action to be invoked upon completion of the request.
        /// This action is invoked regardless of the success of the request.
        /// </summary>
        public Action<IAsynchronousRequest> OnRequestCompleted { get; set; }

        /// <summary>
        /// The data returned from the request.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Any error information returned from the request.
        /// </summary>
        public string Error { get; set; }

        public Uri Path { get; set; }

        /// <summary>
        /// UpdateManager instance that created this request.
        /// </summary>
        private readonly IUpdateManager manager = null;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="path">Uri that needs to be read to get the update information.</param>
        /// <param name="manager">The update manager which is making this request.</param>
        public UpdateRequest(Uri path, IUpdateManager manager)
        {
            OnRequestCompleted = manager.UpdateDataAvailable;
            this.manager = manager;

            Error = string.Empty;
            Data = string.Empty;
            Path = path;

            var client = new WebClient();
            client.OpenReadAsync(path);
            client.OpenReadCompleted += ReadResult;
        }

        /// <summary>
        /// Event handler for the web client's requestion completed event. Reads
        /// the request's result information and subsequently triggers
        /// the UpdateDataAvailable event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadResult(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                if (null == e || e.Error != null)
                {
                    Error = "Unspecified error";
                    if (null != e && (null != e.Error))
                        Error = e.Error.Message;
                }

                using (var sr = new StreamReader(e.Result))
                {
                    Data = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Error = string.Empty;
                Data = string.Empty;

                manager.OnLog(new LogEventArgs("The update request could not be completed.", LogLevel.File));
                manager.OnLog(new LogEventArgs(ex, LogLevel.File));
            }

            //regardless of the success of the above logic
            //invoke the completion callback
            OnRequestCompleted.Invoke(this);
        }
    }

    /// <summary>
    /// Specifies Update Manager Configuration settings.
    /// </summary>
    public class UpdateManagerConfiguration : IUpdateManagerConfiguration
    {
        private const string PRODUCTION_SOURCE_PATH_S = "http://dyn-builds-data.s3.amazonaws.com/";
        private const string PRODUCTION_SIG_SOURCE_PATH_S = "http://dyn-builds-data-sig.s3.amazonaws.com/";
        private const string DEFAULT_CONFIG_FILE_S = "UpdateManagerConfig.xml";
        private const string INSTALL_NAME_BASE = "DynamoInstall";

        /// <summary>
        /// Specifies download location for new installer
        /// </summary>
        public string DownloadSourcePath { get; set; }
        
        /// <summary>
        /// Specifies location for signature file to validate the new installer.
        /// </summary>
        public string SignatureSourcePath { get; set; }

        /// <summary>
        /// Specifies whether to consider daily builds for update, default is false.
        /// </summary>
        public bool CheckNewerDailyBuild { get; set; }

        /// <summary>
        /// Specifies whether to force update, default value is false.
        /// </summary>
        public bool ForceUpdate { get; set; }

        /// <summary>
        /// Returns the base name of the installer to be used for upgrade.
        /// </summary>
        public string InstallerNameBase { get; set; }

        /// <summary>
        /// Return file path for the overriding config file.
        /// </summary>
        [XmlIgnore]
        public string ConfigFilePath { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UpdateManagerConfiguration()
        {
            DownloadSourcePath = PRODUCTION_SOURCE_PATH_S;
            SignatureSourcePath = PRODUCTION_SIG_SOURCE_PATH_S;
            CheckNewerDailyBuild = false;
            ForceUpdate = false;
            InstallerNameBase = INSTALL_NAME_BASE;
        }

        /// <summary>
        /// Loads the configurations from given xml file.
        /// </summary>
        /// <param name="filePath">Xml file path that contains configuration details.</param>
        /// <param name="updateManager">IUpdateManager object which can log errors during loading.</param>
        /// <returns>Loaded UpdateManagerConfiguration.</returns>
        public static UpdateManagerConfiguration Load(string filePath, IUpdateManager updateManager)
        {
            if(string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var serializer = new XmlSerializer(typeof(UpdateManagerConfiguration));
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var config = serializer.Deserialize(fs) as UpdateManagerConfiguration;
                    if(null != config)
                        config.ConfigFilePath = filePath;
                    return config;
                }
            }
            catch (Exception ex)
            {
                if (null != updateManager)
                    updateManager.OnLog(
                        new LogEventArgs(
                            string.Format(
                                Properties.Resources.FailedToLoad,
                                filePath,
                                ex.Message),
                            LogLevel.Console));
                else throw;
            }
            return null;
        }

        /// <summary>
        /// Saves this configuration to a given file in xml format.
        /// </summary>
        /// <param name="filePath">File path to save this configuration.</param>
        /// <param name="updateManager">IUpdateManager object which can log errors during saving.</param>
        public void Save(string filePath, IUpdateManager updateManager)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(UpdateManagerConfiguration));
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    serializer.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                if (null != updateManager)
                    updateManager.OnLog(
                        new LogEventArgs(
                            string.Format(
                                Properties.Resources.FailedToSave,
                                filePath,
                                ex.Message),
                            LogLevel.Console));
                else throw;
            }
        }

        /// <summary>
        /// Utility method to get the settings
        /// </summary>
        /// <param name="lookUp">IDynamoLookUp instance</param>
        /// <param name="updateManager">IUpdateManager object which can log errors during saving.</param>
        /// <returns>Update Manager Configuration settings object</returns>
        public static UpdateManagerConfiguration GetSettings(IDynamoLookUp lookUp, IUpdateManager updateManager = null)
        {
            string filePath;
            var exists = TryGetConfigFilePath(out filePath);
#if DEBUG
            //This code is just to create the default config file to
            //save the default settings, which later on can be modified
            //to re-direct it to other download target for testing.
            if (!exists)
            {
                var umConfig = new UpdateManagerConfiguration();
                umConfig.Save(filePath, updateManager);
            }
#endif
            if (!exists) 
                return new UpdateManagerConfiguration() { DynamoLookUp = lookUp };

            var config = Load(filePath, updateManager);
            if (null != config)
                config.DynamoLookUp = lookUp;

            return config;
        }

        /// <summary>
        /// Returns the update manager config file path.
        /// </summary>
        /// <param name="filePath">Full path for the config file</param>
        /// <returns>True if file exists.</returns>
        public static bool TryGetConfigFilePath(out string filePath)
        {
            string location = Assembly.GetExecutingAssembly().Location;
            // ReSharper disable once AssignNullToNotNullAttribute, location is always available
            filePath = Path.Combine(Path.GetDirectoryName(location), DEFAULT_CONFIG_FILE_S);
            return File.Exists(filePath);
        }

        /// <summary>
        /// IDynamoLookUp object to get installed Dynamo versions
        /// </summary>
        [XmlIgnore]
        public IDynamoLookUp DynamoLookUp { get; set; }
    }

    /// <summary>
    /// 该类为产品更新管理提供服务。
    /// </summary>
    internal sealed class UpdateManager : NotificationObject, IUpdateManager
    {
        #region Private Class Data Members

        private bool versionCheckInProgress;
        private static BinaryVersion productVersion;
        private IAppVersionInfo updateInfo;
        private const string OLD_DAILY_INSTALL_NAME_BASE = "DynamoDailyInstall";
        private const string INSTALLUPDATE_EXE = "InstallUpdate.exe";
        private string updateFileLocation;
        private int currentDownloadProgress = -1;
        private IAppVersionInfo downloadedUpdateInfo;
        private IUpdateManagerConfiguration configuration = null;
        private int hostApplicationProcessId = -1;

        #endregion

        #region Public Event Handlers

        /// <summary>
        /// 当RequestUpdateDownload操作完成时发生。
        /// </summary>
        public event UpdateDownloadedEventHandler UpdateDownloaded;
        public event ShutdownRequestedEventHandler ShutdownRequested;
        public event LogEventHandler Log;

        #endregion

        #region Public Class Properties

        /// <summary>
        /// Obtains product version string
        /// </summary>
        public BinaryVersion ProductVersion
        {
            get
            {
                return GetProductVersion();
            }
        }

        public static BinaryVersion GetProductVersion()
        {
            if (null != productVersion) return productVersion;

            var executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
            productVersion = BinaryVersion.FromString(executingAssemblyName.Version.ToString());

            return productVersion;
        }

        public Version HostVersion { get; set; }

        public string HostName { get; set; }

        /// <summary>
        /// BaseVersion is a method which compares the current Dynamo Core Version and the HostVersion
        /// (DynamoRevit/DynamoStudio etc.) and returns the earlier (lower) Version.
        /// This allows subsequent methods to do a single check and if there is an updated version (to either Core/Host
        /// versions), the subsequent methods will poll the server for an update.
        /// </summary>
        private BinaryVersion BaseVersion()
        {
            if (HostVersion == null) return ProductVersion;

            var binaryHostVersion = BinaryVersion.FromString(HostVersion.ToString());

            if (ProductVersion < binaryHostVersion) return ProductVersion;
            else return binaryHostVersion;
        }

        /// <summary>
        ///     Obtains available update version string 
        /// </summary>
        public BinaryVersion AvailableVersion
        {
            get
            {
                // Dirty patch: A version is available only when the update has been downloaded.
                // This causes the UI to display the update button only after the download has
                // completed.
                return downloadedUpdateInfo == null
                    ? BaseVersion() : updateInfo.Version;
            }
        }

        /// <summary>
        /// Obtains downloaded update file location.
        /// </summary>
        public string UpdateFileLocation
        {
            get { return updateFileLocation; }
            private set
            {
                updateFileLocation = value;
                RaisePropertyChanged("UpdateFileLocation");
            }
        }

        public IAppVersionInfo UpdateInfo
        {
            get { return updateInfo; }
            set
            {
                if (value != null)
                {
                    //在控制台上输出:Update available! [版本号]
                    OnLog(new LogEventArgs(string.Format(Properties.Resources.UpdateAvailable, value.Version), LogLevel.Console));
                }

                updateInfo = value;
                RaisePropertyChanged("UpdateInfo");
            }
        }

        /// <summary>
        ///     Dirty patch: Set to the value of UpdateInfo once the new update installer has been
        ///     downloaded.
        /// </summary>
        public IAppVersionInfo DownloadedUpdateInfo
        {
            get { return downloadedUpdateInfo; }
            set
            {
                downloadedUpdateInfo = value;
                RaisePropertyChanged("DownloadedUpdateInfo");
            }
        }

        /// <summary>
        /// 查询是否进行更新
        /// 如果启动了强制更新,则直接进行更新
        /// 如果当前最新版本高于运行版本,进行更新
        /// 在最新本版下载完毕之前,查询将返回false
        /// </summary>
        public bool IsUpdateAvailable
        {
            get
            {
                return false;//自废武功
                //Update is not available unitl it's downloaded
                if (DownloadedUpdateInfo == null)
                    return false;

                return ForceUpdate || AvailableVersion > BaseVersion();
            }
        }

        /// <summary>
        /// This flag is available via the debug menu to
        /// allow the update manager to check for newer daily 
        /// builds as well.
        /// </summary>
        public bool CheckNewerDailyBuilds
        {
            get { return Configuration.CheckNewerDailyBuild; }
            set
            {
                if (!Configuration.CheckNewerDailyBuild && value)
                {
                    CheckForProductUpdate(new UpdateRequest(new Uri(Configuration.DownloadSourcePath), this));
                }
                Configuration.CheckNewerDailyBuild = value;
                RaisePropertyChanged("CheckNewerDailyBuilds");
            }
        }

        /// <summary>
        /// Apply the most recent update, regardless
        /// of whether it is newer than the current version.
        /// </summary>
        public bool ForceUpdate
        {
            get { return Configuration.ForceUpdate; }
            set
            {
                if (!Configuration.ForceUpdate && value)
                {
                    // do a check
                    CheckForProductUpdate(new UpdateRequest(new Uri(Configuration.DownloadSourcePath), this));
                }
                Configuration.ForceUpdate = value;
                RaisePropertyChanged("ForceUpdate");
            }
        }

        /// <summary>
        /// Returns the configuration settings.
        /// </summary>
        public IUpdateManagerConfiguration Configuration
        {
            get 
            {
                return configuration ?? (configuration = UpdateManagerConfiguration.GetSettings(null, this));
            }
        }

        #endregion

        public UpdateManager(IUpdateManagerConfiguration configuration)
        {
            this.configuration = configuration;
            PropertyChanged += UpdateManager_PropertyChanged;
            HostVersion = null;
            HostName = string.Empty;
        }
        //向控制台输出下载开始,并启动同步下载
        void UpdateManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "UpdateInfo":
                    if (updateInfo != null)
                    {
                        //When the UpdateInfo property changes, this will be reflected in the UI
                        //by the vsisibility of the download cloud. The most up to date version will
                        //be downloaded asynchronously.
                        OnLog(new LogEventArgs(Properties.Resources.UpdateDownloadStarted, LogLevel.Console));

                        var tempPath = Path.GetTempPath();
                        //DownloadUpdatePackageAsynchronously(updateInfo.InstallerURL, updateInfo.Version, tempPath);
                        //DownloadSignatureFileAsynchronously(updateInfo.SignatureURL, tempPath);
                    }
                    break;
            }
        }

        #region Public Class Operational Methods

        /// <summary>
        /// Async call to request the update version info from the web. 
        /// This call raises UpdateFound event notification, if an update is
        /// found.
        /// </summary>
        public void CheckForProductUpdate(IAsynchronousRequest request)
        {
            OnLog(new LogEventArgs("RequestUpdateVersionInfo", LogLevel.File));
            OnLog(new LogEventArgs(Properties.Resources.RequestingVersionUpdate, LogLevel.Console));

            if (versionCheckInProgress)
                return;

            versionCheckInProgress = true;
        }

        /// <summary>
        /// Callback for the UpdateRequest's UpdateDataAvailable event.
        /// Reads the request's data, and parses for available versions. 
        /// If a more recent version is available, the UpdateInfo object 
        /// will be set.
        /// 自废武功
        /// </summary>
        /// <param name="request">An instance of an update request.</param>
        public void UpdateDataAvailable(IAsynchronousRequest request)
        {
            UpdateInfo = null;

            //OnLog(new LogEventArgs(String.Format(Properties.Resources.CouldNotGetUpdateData, request.Path), LogLevel.Console));
            OnLog(new LogEventArgs("更新个香蕉船哟", LogLevel.Console));

            versionCheckInProgress = false;
            return;
        }

        public void QuitAndInstallUpdate()
        {
            OnLog(new LogEventArgs("UpdateManager.QuitAndInstallUpdate-Invoked", LogLevel.File));

            if (ShutdownRequested != null)
                ShutdownRequested(this);
        }
        /// <summary>
        /// 更新下载完毕之后,要关闭程序进行更新
        /// </summary>
        public void HostApplicationBeginQuit()
        {
            // Double check that the updater path is not null and that there
            // exists a file at that location on disk.
            // Although this updater is stored in a temp directory,
            // and the user wouldn't have come across it, there's the
            // outside chance that it was deleted. Update cannot
            // continue without this file.

            if (string.IsNullOrEmpty(UpdateFileLocation) || !File.Exists(UpdateFileLocation))
                return;

            var currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var updater = Path.Combine(currDir, INSTALLUPDATE_EXE);
            
            // Double check that that the updater program exists.
            // This program lives in the users's base Dynamo directory. If 
            // it doesn't exist, we can't run the update.

            if (!File.Exists(updater)) 
                return;

            var p = new Process
            {
                StartInfo =
                {
                    FileName = updater,
                    Arguments = UpdateFileLocation,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (hostApplicationProcessId != -1)
            {
                p.StartInfo.Arguments += " " + hostApplicationProcessId;
            }
            p.Start();
            Dynamo.Logging.Analytics.TrackEvent(Actions.Installed, Categories.Upgrade, AvailableVersion.ToString());
        }

        public void RegisterExternalApplicationProcessId(int id)
        {
            hostApplicationProcessId = id;
        }

        #endregion

        #region Private Event Handlers


        public void OnLog(LogEventArgs args)
        {
            Log?.Invoke(args);
        }

        #endregion



        /// <summary>
        /// Checks for the product update by requesting for update version info 
        /// from configured download source path. This method will skip the 
        /// update check if a newer version of the product is already installed.
        /// </summary>
        /// <param name="manager">Update manager instance using which product
        /// update check needs to be done.</param>
        internal static void CheckForProductUpdate(IUpdateManager manager)
        {
            //If we already have higher version installed, don't look for product update.
            if(manager.Configuration.DynamoLookUp != null && manager.Configuration.DynamoLookUp.LatestProduct > manager.ProductVersion)
                return;

            var downloadUri = new Uri(manager.Configuration.DownloadSourcePath);
            manager.CheckForProductUpdate(new UpdateRequest(downloadUri, manager));
        }
    }

    /// <summary>
    /// Lookup for installed products
    /// </summary>
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
            if(!Directory.Exists(installPath))//null or empty installPath will return false
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
            if(null == dynamoInstallations)
                return null;

            var latestVersion =
                dynamoInstallations.Select(GetDynamoVersion).OrderBy(s => s).LastOrDefault();
            return latestVersion == null ? null : BinaryVersion.FromString(latestVersion.ToString());
        }
    }
}
