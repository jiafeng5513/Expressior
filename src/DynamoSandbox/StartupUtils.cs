using System.IO;
using System.Reflection;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using DynamoShapeManager;

namespace Dynamo.Applications
{
    public class StartupUtils
    {

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
