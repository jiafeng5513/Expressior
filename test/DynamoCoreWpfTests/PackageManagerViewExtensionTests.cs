
using CoreNodeModels.Input;
using Dynamo.Configuration;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
//using Dynamo.PackageManager.UI;
using Dynamo.Scheduler;
using Dynamo.Wpf.Extensions;
using DynamoCoreWpfTests.Utility;
using Microsoft.CSharp;
using Moq;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class PackageManagerViewExtensionTests : DynamoTestUIBase
    {
        private string PackagesDirectory { get { return Path.Combine(GetTestDirectory(this.ExecutingDirectory), "pkgs"); } }

        #region extensionGeneration
        private string extensionPath;
        private string manifestPath;
        private string extensionManifest = 
            @"<ViewExtensionDefinition>
          <AssemblyPath>..\bin\TestViewExtension.dll</AssemblyPath>
          <TypeName>TestViewExtension</TypeName>
        </ViewExtensionDefinition>";
        //this source is compiled at runtime when this test is run, and the resulting extension is loaded then.
        //this extension has a few public properties we can check to make sure dynamo has loaded it correctly.
        private string testViewExtensionSource =
       @"using System;
        using Dynamo.Wpf.Extensions;
        public class TestViewExtension: IViewExtension {
         public int loadedCount = 0;
         public int startupCount = 0;
         public void Dispose() {}

         public void Startup(ViewStartupParams p) {
          startupCount = startupCount + 1;
         }

         public void Loaded(ViewLoadedParams p) {
          loadedCount = loadedCount + 1;
         }

         public void Shutdown() {}

         public string UniqueId {
          get {
           return Guid.NewGuid().ToString();
          }
         }

         public string Name {
          get {
           return ""Test View Extension"";
          }
         }

        }";
        #endregion;

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPathResolver pathResolver)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = new PreferenceSettings()
                {
                    CustomPackageFolders = new List<string>() { Path.Combine(this.PackagesDirectory, "subPackageDirectory") }
                }
            };
        }

        [TestFixtureSetUp]
        /// <summary>
        /// This method compiles an extension at test time and injects it and an extension manifest into the testing package folder
        /// so that the extension gets loaded when the test instance of Dynamo starts.
        /// </summary>
        public virtual void GenerateNewExtension()
        {
            var provider = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = false,
                OutputAssembly = "TestViewExtension.dll",
            };
            options.ReferencedAssemblies.Add("DynamoCore.dll");
            options.ReferencedAssemblies.Add("DynamoCoreWPF.dll");
            string source = this.testViewExtensionSource;

            var results = provider.CompileAssemblyFromSource(options, new[] { source });
            if (results.Errors.Count > 0)
            {
                Console.WriteLine("Compile ERROR");
                foreach (CompilerError error in results.Errors)
                {
                    Console.WriteLine(error.ErrorText);
                }
            }
            else
            {
                Console.WriteLine("Compile OK");
                Console.WriteLine("Assembly Path:" + results.PathToAssembly);
                Console.WriteLine("Assembly Name:" + results.CompiledAssembly.FullName);

                //move the new assembly into the package directory/bin folder.
                extensionPath = Path.Combine(PackagesDirectory, "subPackageDirectory", "runtimeGeneratedExtension", "bin", "TestViewExtension.dll");
                System.IO.File.Copy(results.CompiledAssembly.Location, extensionPath, true);

                //copy the manifest as well.
                manifestPath = Path.Combine(PackagesDirectory, "subPackageDirectory", "runtimeGeneratedExtension",
                    "extra", "TestViewExtension_ViewExtensionDefinition.xml");
                System.IO.File.WriteAllText(manifestPath, this.extensionManifest);
            }
        }

        [Test]
        public void PackageManagerLoadsRuntimeGeneratedExtension()
        {
            Assert.IsTrue(this.View.viewExtensionManager.ViewExtensions.Select(x => x.Name).Contains("Test View Extension"));
        }


        [Test]
        public void StartUpAndLoadedAreCalledOnceOnViewExtensionsInPackges()
        {
            //this extension is compiled at testTime and injected into test package folder.
            //source is above.
            dynamic testExtension = this.View.viewExtensionManager.ViewExtensions.Where(x => x.Name == "Test View Extension").FirstOrDefault();
            Assert.AreEqual(1, testExtension.startupCount);
            Assert.AreEqual(1, testExtension.loadedCount);
        }



        [TestFixtureTearDown]
        /// <summary>
        /// This method cleans up the manifest so the generated extension does not load
        /// during other tests.
        /// </summary>
        public void RemoveExtension()
        {
            //TODO it would be good to cleanup the dll as well but we can't as it is currently loaded.
            System.IO.File.Delete(manifestPath);
            this.FinalTearDown();
        }
    }
}
