using Dynamo.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Dynamo.Applications
{
    /// <summary>
    /// 为系统提供目录服务,查找启动点,用户文件夹,公用文件夹(程序文档位置)
    /// 动态链接库列表
    /// </summary>
    internal class SandboxPathResolver : IPathResolver
    {
        private readonly List<string> additionalResolutionPaths;
        private readonly List<string> additionalNodeDirectories;
        private readonly List<string> preloadedLibraryPaths;

        public SandboxPathResolver(string preloaderLocation)
        {
            // If a suitable preloader cannot be found on the system, then do 
            // not add invalid path into additional resolution. The default 
            // implementation of IPathManager in Dynamo insists on having valid 
            // paths specified through "IPathResolver" implementation.
            // 
            additionalResolutionPaths = new List<string>();
            if (Directory.Exists(preloaderLocation))
                additionalResolutionPaths.Add(preloaderLocation);

            additionalNodeDirectories = new List<string>();
            preloadedLibraryPaths = new List<string>
            {
                "VMDataBridge.dll",
                "DSCoreNodes.dll",
                "DesignScriptBuiltin.dll",
                "DSIronPython.dll",
                "FunctionObject.ds",
                "BuiltIn.ds",
                "ImageProcess.dll",//注意这个dll的名字
                "DicomTools.dll",
                "DeepLearning.dll"
            };

        }

        public IEnumerable<string> AdditionalResolutionPaths
        {
            get { return additionalResolutionPaths; }
        }

        public IEnumerable<string> AdditionalNodeDirectories
        {
            get { return additionalNodeDirectories; }
        }

        public IEnumerable<string> PreloadedLibraryPaths
        {
            get { return preloadedLibraryPaths; }
        }

        public string UserDataRootFolder
        {
            get { return string.Empty; }
        }

        public string CommonDataRootFolder
        {
            get { return string.Empty; }
        }
    }
}
