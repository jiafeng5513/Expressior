using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DynamoInstallDetective;


namespace DynamoShapeManager
{
    public static class Utilities
    {
        public static readonly string GeometryFactoryAssembly = "LibG.ProtoInterface.dll";
        public static readonly string PreloaderAssembly = "LibG.AsmPreloader.Managed.dll";
        public static readonly string PreloaderClassName = "Autodesk.LibG.AsmPreloader";
        public static readonly string PreloaderMethodName = "PreloadAsmLibraries";

        /// <summary>
        /// 调用此方法以确定安装在用户机器上的ASM版本。
        /// 该方法通过遍历一个写有已知的Autodesk产品的文件夹列表进行扫描
        /// 针对目标版本的ASM二进制文件列表扫描已知的Autodesk产品文件夹列表。
        /// </summary>
        /// <param name="versions">按优先顺序检查的版本号列表。此参数不能为空</param>
        /// <param name="location">ASM二进制文件的目录的完整路径。此参数不能为空</param>
        /// <param name="rootFolder">This method makes use of DynamoInstallDetective
        /// to determine the installation location of various Autodesk products. This 
        /// argument is not optional and must represent the full path to the folder 
        /// which contains DynamoInstallDetective.dll. An exception is thrown if the 
        /// assembly cannot be located.</param>
        /// <returns>如果找到,返回版本号,否则返回空</returns>
        /// 
        public static LibraryVersion GetInstalledAsmVersion(List<LibraryVersion> versions, ref string location, string rootFolder)
        {
            if (string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");
            if (!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException(rootFolder);
            if ((versions == null) || versions.Count <= 0)
                throw new ArgumentNullException("versions");
            if (location == null)
                throw new ArgumentNullException("location");

            location = string.Empty;

            try
            {
                var installations = GetAsmInstallations(rootFolder);

                foreach (var v in versions)
                {
                    foreach (KeyValuePair<string, Tuple<int, int, int, int>> install in installations)
                    {
                        if ((int)v == install.Value.Item1)
                        {
                            location = install.Key;
                            return (LibraryVersion)install.Value.Item1;
                        }
                    }
                }


                //Fallback mechanism, look inside libg folders if any of them
                //contain ASM dlls.
                foreach (var v in versions)
                {
                    var folderName = string.Format("libg_{0}", (int)v);
                    var dir = new DirectoryInfo(Path.Combine(rootFolder, folderName));
                    if (!dir.Exists)
                        continue;

                    var files = dir.GetFiles("ASMAHL*.dll");
                    if (!files.Any())
                        continue;

                    location = dir.FullName;
                    return v; // Found version.
                }                
            }
            catch (Exception)
            {
                return LibraryVersion.None;
            }

            return LibraryVersion.None;
        }

        /// <summary>
        /// Call this method to preload ASM binaries from a specific location. This 
        /// method does not have a return value, any failures in loading ASM binaries
        /// will result in an exception being thrown.
        /// </summary>
        /// <param name="preloaderLocation">Full path of the folder that contains  
        /// PreloaderAssembly assembly. This argument must represent a valid path 
        /// to the loader.</param>
        /// <param name="asmLocation">Full path of the folder that contains ASM 
        /// binaries to load. This argument cannot be null or empty.</param>
        /// 
        public static void PreloadAsmFromPath(string preloaderLocation, string asmLocation)
        {
            if (string.IsNullOrEmpty(preloaderLocation) || !Directory.Exists(preloaderLocation))
                throw new ArgumentException("preloadedPath");
            if (string.IsNullOrEmpty(asmLocation) || !Directory.Exists(asmLocation))
                throw new ArgumentException("asmLocation");

            var preloaderPath = Path.Combine(preloaderLocation, PreloaderAssembly);

            Debug.WriteLine(string.Format("ASM Preloader: {0}", preloaderPath));
            Debug.WriteLine(string.Format("ASM Location: {0}", asmLocation));

            var libG = Assembly.LoadFrom(preloaderPath);
            var preloadType = libG.GetType(PreloaderClassName);

            var preloadMethod = preloadType.GetMethod(PreloaderMethodName,
                BindingFlags.Public | BindingFlags.Static);

            if (preloadMethod == null)
            {
                throw new MissingMethodException(
                    string.Format("Method '{0}' not found", PreloaderMethodName));
            }

            var methodParams = new object[] { asmLocation };
            preloadMethod.Invoke(null, methodParams);

            Debug.WriteLine("Successfully loaded ASM binaries");
        }



        private static IEnumerable GetAsmInstallations(string rootFolder)
        {
            //var assemblyPath = Path.Combine(Path.Combine(rootFolder, "DynamoInstallDetective.dll"));
            //if (!File.Exists(assemblyPath))
            //    throw new FileNotFoundException(assemblyPath);

            //var assembly = Assembly.LoadFrom(assemblyPath);

            //var type = assembly.GetType("DynamoInstallDetective.Utilities");
            ////运行时从程序集中加载方法并调用.
            //var installationsMethod = type.GetMethod(
            //    "FindProductInstallations",
            //    BindingFlags.Public | BindingFlags.Static);

            //if (installationsMethod == null)
            //{
            //    throw new MissingMethodException("Method 'DynamoInstallDetective.Utilities.FindProductInstallations' not found");
            //}

            //var methodParams = new object[] { "Revit", "ASMAHL*.dll" };
            return FindProductInstallations("Revit", "ASMAHL*.dll");
           // return installationsMethod.Invoke(null, methodParams) as IEnumerable;

        }

        public static IEnumerable FindProductInstallations(string productSearchPattern, string fileSearchPattern)
        {
            var installs = new InstalledProducts();
            installs.LookUpAndInitProducts(new InstalledProductLookUp(productSearchPattern, fileSearchPattern));

            return
                installs.Products.Select(
                    p =>
                        new KeyValuePair<string, Tuple<int, int, int, int>>(
                            p.InstallLocation,
                            p.VersionInfo));
        }
    }
}
