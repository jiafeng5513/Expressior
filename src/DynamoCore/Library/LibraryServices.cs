using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using Dynamo.Interfaces;
using Dynamo.Library;
using Dynamo.Utilities;
using Dynamo.Logging;

using ProtoCore.AST.AssociativeAST;
using ProtoCore.BuildData;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoFFI;


using Operator = ProtoCore.DSASM.Operator;
using ProtoCore;
using ProtoCore.Namespace;
using Dynamo.Exceptions;
using Dynamo.Configuration;
using ProtoCore.CompilerDefinitions;

namespace Dynamo.Engine
{
    /// <summary>
    ///     LibraryServices is a singleton class which manages builtin libraries
    ///     as well as imported libraries. It is across different sessions.
    /// LibraryServices是一个用来管理内置库和导入库的单例类
    /// </summary>
    public class LibraryServices : LogSourceBase, IDisposable
    {
        private readonly Dictionary<string, FunctionGroup> builtinFunctionGroups =
            new Dictionary<string, FunctionGroup>();

        private readonly Dictionary<string, Dictionary<string, FunctionGroup>> importedFunctionGroups =
            new Dictionary<string, Dictionary<string, FunctionGroup>>(new LibraryPathComparer());

        // 这个list中存放所有的预载入的库,从user文件夹package下加载的库和从自定义dll加载的ZERO-touch库
        private readonly List<string> importedLibraries = new List<string>();

        //这个list存放所有在package文件夹下加载上的库
        private readonly List<string> packagedLibraries = new List<string>();

        private readonly IPathManager pathManager;
        private readonly IPreferences preferenceSettings;

        /// <summary>
        /// 返回用于解析代码和加载库的 core
        /// </summary>
        public ProtoCore.Core LibraryManagementCore { get; private set; }
        private ProtoCore.Core liveRunnerCore = null;

        internal void SetLiveCore(ProtoCore.Core core)
        {
            liveRunnerCore = core;
        }

        private class UpgradeHint
        {
            public UpgradeHint()
            {
                UpgradeName = null;
                AdditionalAttributes = new Dictionary<string, string>();
                AdditionalElements = new List<XmlElement>();
            }

            // The new name of the method in Dynamo
            public string UpgradeName { get; set; }
            // A list of additional parameters to append or change on the XML node when migrating
            public Dictionary<string, string> AdditionalAttributes { get; set; } 
            public List<XmlElement> AdditionalElements { get; set; } 
        }

        private readonly Dictionary<string, UpgradeHint> priorNameHints =
            new Dictionary<string, UpgradeHint>();

        /// <summary>
        /// 从活动的 core拷贝properties到LibraryManagementCore
        /// </summary>
        internal void UpdateLibraryCoreData()
        {
            // If a liverunner core is provided, sync the library core data
            if (liveRunnerCore != null)
            {
                LibraryManagementCore.ProcTable = new ProtoCore.DSASM.ProcedureTable(liveRunnerCore.ProcTable);
                LibraryManagementCore.ClassTable = new ProtoCore.DSASM.ClassTable(liveRunnerCore.ClassTable);
            }
        }

        /// <summary>
        /// 初始化(注意这是一个单例类)
        /// </summary>
        /// <param name="libraryManagementCore">Core which is used for parsing code and loading libraries</param>
        /// <param name="pathManager">Instance of IPathManager containing neccessary Dynamo paths</param>
        public LibraryServices(ProtoCore.Core libraryManagementCore, IPathManager pathManager)
            : this(libraryManagementCore, pathManager, null) { }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="libraryManagementCore">用来解析代码和加载libraries的core</param>
        /// <param name="pathManager">IPathManager型对象,用于获取必要的路径</param>
        /// <param name="preferences">IPreferences型对象,用于获取首选项</param>
        public LibraryServices(ProtoCore.Core libraryManagementCore, IPathManager pathManager, IPreferences preferences)
        {
            LibraryManagementCore = libraryManagementCore;
            this.pathManager = pathManager;
            preferenceSettings = preferences;

            PreloadLibraries(pathManager.PreloadedLibraries);
            PopulateBuiltIns();
            PopulateOperators();
            PopulatePreloadLibraries();
            LibraryLoadFailed += new EventHandler<LibraryLoadFailedEventArgs>(LibraryLoadFailureHandler);
        }

        /// <summary>
        /// 释放空间
        /// </summary>
        public void Dispose()
        {
            builtinFunctionGroups.Clear();
            importedFunctionGroups.Clear();
            importedLibraries.Clear();
        }
        
        /// <summary>
        /// 返回已导入库列表
        /// </summary>
        public IEnumerable<string> ImportedLibraries
        {
            get { return importedLibraries; }
        }

        /// <summary>
        /// 返回内置函数group
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FunctionGroup> BuiltinFunctionGroups
        {
            get { return builtinFunctionGroups.Values; }
        }

        /// <summary>
        /// 返回所有的已导入函数group
        /// </summary>
        public IEnumerable<FunctionGroup> ImportedFunctionGroups
        {
            get { return importedFunctionGroups.SelectMany(d => d.Value).Select(p => p.Value); }
        }

        /// <summary>
        /// 在加载library之前发生
        /// </summary>
        public class LibraryLoadingEventArgs : EventArgs
        {
            public LibraryLoadingEventArgs(string libraryPath)
            {
                LibraryPath = libraryPath;
            }

            public string LibraryPath { get; private set; }
        }
        public event EventHandler<LibraryLoadingEventArgs> LibraryLoading;
        private void OnLibraryLoading(LibraryLoadingEventArgs e)
        {
            string library = e.LibraryPath;

            // Assembly, that is located in package directory, considered as part of package.
            if (pathManager.PackagesDirectories.Any(
                directory => library.StartsWith(directory)))
            {
                packagedLibraries.Add(library);
            }

            EventHandler<LibraryLoadingEventArgs> handler = LibraryLoading;
            if (handler != null)
                handler(this, e);
        }
        /// <summary>
        /// 在无法加载library时发生
        /// </summary>
        public class LibraryLoadFailedEventArgs : EventArgs
        {
            public LibraryLoadFailedEventArgs(string libraryPath, string reason)
            {
                LibraryPath = libraryPath;
                Reason = reason;
            }

            public string LibraryPath { get; private set; }
            public string Reason { get; private set; }
        }
        public event EventHandler<LibraryLoadFailedEventArgs> LibraryLoadFailed;
        private void LibraryLoadFailureHandler(object sender, LibraryLoadFailedEventArgs args)
        {
            LibraryLoadFailedException ex = new LibraryLoadFailedException(args.LibraryPath, args.Reason);
            Log(ex.Message, WarningLevel.Moderate);
            throw ex;
        }
        private void OnLibraryLoadFailed(LibraryLoadFailedEventArgs e)
        {
            EventHandler<LibraryLoadFailedEventArgs> handler = LibraryLoadFailed;
            if (handler != null)
                handler(this, e);
        }
        /// <summary>
        /// 在一个library加载成功的时候发生
        /// </summary>
        public class LibraryLoadedEventArgs : EventArgs
        {
            public LibraryLoadedEventArgs(string libraryPath)
            {
                LibraryPath = libraryPath;
            }

            public string LibraryPath { get; private set; }
        }
        public event EventHandler<LibraryLoadedEventArgs> LibraryLoaded;
        private void OnLibraryLoaded(LibraryLoadedEventArgs e)
        {
            importedLibraries.Add(e.LibraryPath);

            EventHandler<LibraryLoadedEventArgs> handler = LibraryLoaded;
            if (handler != null)
                handler(this, e);
        }
        /// <summary>
        /// 进行libraries的预加载
        /// </summary>
        /// <param name="preloadLibraries">预加载库列表</param>
        private void PreloadLibraries(IEnumerable<string> preloadLibraries)
        {
            importedLibraries.AddRange(preloadLibraries);//这里面是一些dll的名字

            foreach (var library in importedLibraries)
                CompilerUtils.TryLoadAssemblyIntoCore(LibraryManagementCore, library);
        }
        /// <summary>
        /// 查看函数签名是否需要附加Attributes
        /// </summary>
        /// <param name="functionSignature">函数签名</param>
        /// <returns></returns>
        internal bool FunctionSignatureNeedsAdditionalAttributes(string functionSignature)
        {
            if (functionSignature == null)
            {
                return false;
            }
            if (!priorNameHints.ContainsKey(functionSignature))
                return false;

            return priorNameHints[functionSignature].AdditionalAttributes.Count > 0;
        }

        /// <summary>
        /// 给节点添加附加Attributes
        /// </summary>
        /// <param name="functionSignature"></param>
        /// <param name="nodeElement"></param>
        internal void AddAdditionalAttributesToNode(string functionSignature, XmlElement nodeElement)
        {
            var shortKey = GetQualifiedFunction(functionSignature);
            if (!FunctionSignatureNeedsAdditionalAttributes(functionSignature)
                && !FunctionSignatureNeedsAdditionalAttributes(shortKey)) return;

            var upgradeHint = FunctionSignatureNeedsAdditionalAttributes(functionSignature)
                ? priorNameHints[functionSignature]
                : priorNameHints[shortKey];

            foreach (string key in upgradeHint.AdditionalAttributes.Keys)
            {
                var val = nodeElement.Attributes[key];

                if (val != null)
                {
                    nodeElement.Attributes[key].Value = upgradeHint.AdditionalAttributes[key];
                    continue;
                }

                nodeElement.SetAttribute(key, upgradeHint.AdditionalAttributes[key]);
            }
        }

        /// <summary>
        /// 查看函数签名是否需要附加Elements
        /// </summary>
        /// <param name="functionSignature"></param>
        /// <returns></returns>
        internal bool FunctionSignatureNeedsAdditionalElements(string functionSignature)
        {
            if (functionSignature == null)
            {
                return false;
            }
            if (!priorNameHints.ContainsKey(functionSignature))
                return false;

            return priorNameHints[functionSignature].AdditionalElements.Count > 0;
        }

        /// <summary>
        /// 给节点添加附加Elements
        /// </summary>
        /// <param name="functionSignature"></param>
        /// <param name="nodeElement"></param>
        internal void AddAdditionalElementsToNode(string functionSignature, XmlElement nodeElement)
        {
            var shortKey = GetQualifiedFunction(functionSignature);
            if (!FunctionSignatureNeedsAdditionalElements(functionSignature)
                && !FunctionSignatureNeedsAdditionalElements(shortKey)) return;

            var upgradeHint = FunctionSignatureNeedsAdditionalElements(functionSignature)
                ? priorNameHints[functionSignature]
                : priorNameHints[shortKey];

            foreach (XmlElement elem in upgradeHint.AdditionalElements)
            {
                XmlNode newNode = nodeElement.OwnerDocument.ImportNode(elem, true);
                nodeElement.AppendChild(newNode);
            }
        }

        /// <summary>
        /// 从函数签名中获取 Name(ZERO-touch library)
        /// </summary>
        /// <param name="functionSignature"></param>
        /// <returns></returns>
        internal string NameFromFunctionSignature(string functionSignature)
        {
            string[] splitted = null;
            string newName = null;

            if (priorNameHints.ContainsKey(functionSignature))
            {
                var mappedSignature = priorNameHints[functionSignature].UpgradeName;

                splitted = mappedSignature.Split('@');

                if (splitted.Length < 1 || String.IsNullOrEmpty(splitted[0]))
                    return null;

                newName = splitted[0];
            }
            else
            {
                splitted = functionSignature.Split('@');

                if (splitted.Length < 1 || String.IsNullOrEmpty(splitted[0]))
                    return null;

                string qualifiedFunction = splitted[0];

                if (!priorNameHints.ContainsKey(qualifiedFunction))
                    return null;

                newName = priorNameHints[qualifiedFunction].UpgradeName;
            }

            splitted = newName.Split('.');

            // Case for BuitIn nodes, because they don't have namespace or class.
            if (splitted.Length == 1)
                return newName;

            // Other nodes should have at least 2 parameters.
            if (splitted.Length < 2)
                return null;

            return splitted[splitted.Length - 2] + "." + splitted[splitted.Length - 1];
        }

        /// <summary>
        /// 从函数签名Hint中获取函数签名
        /// </summary>
        /// <param name="functionSignature"></param>
        /// <returns></returns>
        internal string FunctionSignatureFromFunctionSignatureHint(string functionSignature)
        {
            // if the hint is explicit, we can simply return the mapped function
            if (priorNameHints.ContainsKey(functionSignature))
                return priorNameHints[functionSignature].UpgradeName;

            // if the hint is not explicit, we try the function name without parameters
            string[] splitted = functionSignature.Split('@');

            if (splitted.Length < 2 || String.IsNullOrEmpty(splitted[0]) || String.IsNullOrEmpty(splitted[1]))
                return null;

            string qualifiedFunction = splitted[0];

            if (!priorNameHints.ContainsKey(qualifiedFunction))
                return null;

            string newName = priorNameHints[qualifiedFunction].UpgradeName;

            return newName + "@" + splitted[1];
        }

        /// <summary>
        /// 获取函数的短名,去掉参数
        /// </summary>
        /// <param name="functionSignature"></param>
        /// <returns></returns>
        private string GetQualifiedFunction(string functionSignature)
        {
            // get a short name representation of the function without parameters
            string[] splitted = functionSignature.Split('@');
           
            if (splitted.Length < 1 || String.IsNullOrEmpty(splitted[0]))
                return null;

            string qualifiedFunction = splitted[0];

            if (!priorNameHints.ContainsKey(qualifiedFunction))
                return null;

            return qualifiedFunction;
        }

        /// <summary>
        /// 获取library中的函数列表
        /// </summary>
        /// <param name="library">Library path</param>
        /// <returns></returns>
        internal IEnumerable<FunctionGroup> GetFunctionGroups(string library)
        {
            if (null == library)
                throw new ArgumentNullException();

            Dictionary<string, FunctionGroup> functionGroups;
            if (!importedFunctionGroups.TryGetValue(library, out functionGroups))
            {
                // Return an empty list instead of 'null' as some of the caller may
                // not have the opportunity to check against 'null' enumerator (for
                // example, an inner iterator in a nested LINQ statement).
                return new List<FunctionGroup>();
            }

            IEnumerable<FunctionGroup> result = functionGroups.Values;

            // Skip namespaces specified in the preference settings
            var settings = preferenceSettings as PreferenceSettings;
            if (settings != null)
            {
                foreach (var nsp in settings.NamespacesToExcludeFromLibrary
                    .Where(x => x.StartsWith(library + ':')).Select(x => x.Split(':').LastOrDefault()))
                {
                    result = result.Where(funcGroup => !funcGroup.QualifiedName.StartsWith(nsp));
                }
            }
            return result;
        }

        /// <summary>
        /// 返回所有的function groups.
        /// </summary>
        internal IEnumerable<FunctionGroup> GetAllFunctionGroups()
        {
            return BuiltinFunctionGroups.Union(ImportedLibraries.SelectMany(GetFunctionGroups));
        }

        /// <summary>
        /// 获取函数描述符(zero-touch)
        /// </summary>
        /// <param name="library">Library path</param>
        /// <param name="mangledName">Mangled function name</param>
        /// <returns></returns>
        public FunctionDescriptor GetFunctionDescriptor(string library, string mangledName)
        {
            if (null == library || null == mangledName)
                throw new ArgumentNullException();

            Dictionary<string, FunctionGroup> groups;
            if (importedFunctionGroups.TryGetValue(library, out groups))
            {
                FunctionGroup functionGroup;
                string qualifiedName = mangledName.Split(new[] { '@' })[0];

                if (TryGetFunctionGroup(groups, qualifiedName, out functionGroup))
                    return functionGroup.GetFunctionDescriptor(mangledName);
            }
            return null;
        }
        /// <summary>
        /// 获取函数描述符
        /// </summary>
        /// <param name="managledName"></param>
        /// <returns></returns>
        public FunctionDescriptor GetFunctionDescriptor(string managledName)
        {
            if (string.IsNullOrEmpty(managledName))
                throw new ArgumentException("Invalid arguments");

            string qualifiedName = managledName.Split(new[] { '@' })[0];
            FunctionGroup functionGroup;

            if (builtinFunctionGroups.TryGetValue(qualifiedName, out functionGroup))
                return functionGroup.GetFunctionDescriptor(managledName);

            return
                importedFunctionGroups.Values.Any(
                    groupMap => TryGetFunctionGroup(groupMap, qualifiedName, out functionGroup))
                    ? functionGroup.GetFunctionDescriptor(managledName)
                    : null;
        }

        /// <summary>
        /// 获取一个字典,其中存储 <old names,new names>
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetPriorNames()
        {
            var priorNames = new Dictionary<string, string>();
            foreach (var kvp in priorNameHints)
            {
                priorNames[kvp.Key] = kvp.Value.UpgradeName;
            }
            return priorNames;
        }

        /// <summary>
        /// 获取所有相关的函数描述符
        /// </summary>
        /// <param name="qualifiedName"></param>
        /// <returns></returns>
        public IEnumerable<FunctionDescriptor> GetAllFunctionDescriptors(string qualifiedName)
        {
            IEnumerable<FunctionDescriptor> descriptors = null;
            FunctionGroup functionGroup;

            // Check through both builtinFunctionGroups and importedFunctionGroups to find the function descriptors
            if (builtinFunctionGroups.TryGetValue(qualifiedName, out functionGroup))
            {
                descriptors = functionGroup.Functions;
                return descriptors;
            }

            foreach (var fg in importedFunctionGroups)
            {
                if (fg.Value.TryGetValue(qualifiedName, out functionGroup))
                {
                    descriptors = functionGroup.Functions;
                    return descriptors;
                }
            }

            return null; // If no function descriptors are found
        }

        /// <summary>
        /// 检查该library是否已经被导入,不允许重复导入组件
        /// </summary>
        /// <param name="library"> can be either the full path or the assembly name </param>
        /// <returns> true even if the same library name is loaded from different paths </returns>
        internal bool IsLibraryLoaded(string library)
        {
            return importedFunctionGroups.ContainsKey(library);
        }

        /// <summary>
        /// 检查给定函数是否在builtinFunctionGroups中，不必根据它的Assembly标签查找它的库(zero-touch)
        /// </summary>
        /// <param name="library">assembly name</param>
        /// <param name="name">name, used for searching as key with default value ""</param>
        /// <returns></returns>
        internal bool IsFunctionBuiltIn(string library, string name = "")
        {
            // For Nodes with not .dll specific Assembly tag
            if (library == Categories.BuiltIn || library == Categories.Operators)
            {
                return builtinFunctionGroups.ContainsKey(name);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查该部分名称能否被解析成该全名
        /// </summary>
        /// <param name="partialName">部分名称</param>
        /// <param name="fullName">全名</param>
        /// <returns></returns>
        private static bool CanbeResolvedTo(ICollection<string> partialName, ICollection<string> fullName)
        {
            return null != partialName && null != fullName && partialName.Count <= fullName.Count
                && fullName.Reverse().Take(partialName.Count).SequenceEqual(partialName.Reverse());
        }

        /// <summary>
        /// 尝试获取Function Group
        /// </summary>
        /// <param name="funcGroupMap"></param>
        /// <param name="qualifiedName"></param>
        /// <param name="funcGroup"></param>
        /// <returns></returns>
        private static bool TryGetFunctionGroup(
            Dictionary<string, FunctionGroup> funcGroupMap, string qualifiedName, out FunctionGroup funcGroup)
        {
            if (funcGroupMap.TryGetValue(qualifiedName, out funcGroup))
                return true;

            string[] partialName = qualifiedName.Split('.');
            string key = funcGroupMap.Keys.FirstOrDefault(k => CanbeResolvedTo(partialName, k.Split('.')));

            if (key != null)
            {
                funcGroup = funcGroupMap[key];
                return true;
            }

            return false;
        }

        /// <summary>
        /// 如果尚未导入,则导入这个库
        /// </summary>
        /// <param name="library"></param>
        internal bool ImportLibrary(string library)
        {
            if (null == library)
                throw new ArgumentNullException();

            if (!library.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)
                && !library.EndsWith(".ds", StringComparison.InvariantCultureIgnoreCase))
            {
                string errorMessage = Properties.Resources.InvalidLibraryFormat;
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                return false;
            }

            if (importedFunctionGroups.ContainsKey(library))
            {
                string errorMessage = string.Format(Properties.Resources.LibraryHasBeenLoaded, library);
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                return false;
            }

            if (!pathManager.ResolveLibraryPath(ref library))
            {
                string errorMessage = string.Format(Properties.Resources.LibraryPathCannotBeFound, library);
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, errorMessage));
                return false;
            }

            OnLibraryLoading(new LibraryLoadingEventArgs(library));

            try
            {
                DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());

                var functionTable = LibraryManagementCore.CodeBlockList[0].procedureTable;
                var classTable = LibraryManagementCore.ClassTable;

                int functionNumber = functionTable.Procedures.Count;
                int classNumber = classTable.ClassNodes.Count;

                CompilerUtils.TryLoadAssemblyIntoCore(LibraryManagementCore, library);


                if (LibraryManagementCore.BuildStatus.ErrorCount > 0)
                {
                    string errorMessage = string.Format(Properties.Resources.LibraryBuildError, library);
                    Log(errorMessage, WarningLevel.Moderate);
                    foreach (ErrorEntry error in LibraryManagementCore.BuildStatus.Errors)
                    {
                        Log(error.Message, WarningLevel.Moderate);
                        errorMessage += error.Message + "\n";
                    }

                    throw new Exception(errorMessage);
                }

                LoadLibraryMigrations(library);

                var loadedClasses = classTable.ClassNodes.Skip(classNumber);
                foreach (var classNode in loadedClasses)
                {
                    ImportClass(library, classNode);
                }

                var loadedFunctions = functionTable.Procedures.Skip(functionNumber);
                foreach (var globalFunction in loadedFunctions)
                {
                    ImportProcedure(library, globalFunction);
                }
            }
            catch (Exception e)
            {
                OnLibraryLoadFailed(new LibraryLoadFailedEventArgs(library, e.Message));
                return false;
            }

            OnLibraryLoaded(new LibraryLoadedEventArgs(library));

            // After a library is loaded, update the library core data with the liveRunner core data
            UpdateLibraryCoreData();
            return true;
        }

        /// <summary>
        /// 导入库的迁移文件(.Migrations.xml)
        /// </summary>
        /// <param name="library"></param>
        internal void LoadLibraryMigrations(string library)
        {
            string fullLibraryName = library;

            // If library is not found, that doesn't mean there is no migration file.
            // E.g. built in nodes don't have assembly, but they do have migration file.
            if (!pathManager.ResolveLibraryPath(ref fullLibraryName))
            {
                fullLibraryName = library;
            }

            string migrationsXMLFile = Path.Combine(Path.GetDirectoryName(fullLibraryName),
                Path.GetFileNameWithoutExtension(fullLibraryName) + ".Migrations.xml");

            if (!pathManager.ResolveDocumentPath(ref migrationsXMLFile))
                return;

            var foundPriorNameHints = new Dictionary<string, UpgradeHint>();

            try
            {
                using (var reader = XmlReader.Create(migrationsXMLFile))
                {

                    var doc = new XmlDocument();
                    doc.Load(reader);
                    XmlElement migrationsElement = doc.DocumentElement;

                    var names = new List<string>();

                    foreach (XmlNode subNode in migrationsElement.ChildNodes)
                    {
                        if (subNode.Name != "priorNameHint")
                            throw new Exception("Invalid XML");

                        names.Add(subNode.Name);

                        var upgradeHint = new UpgradeHint();

                        string oldName = null;

                        foreach (XmlNode hintSubNode in subNode.ChildNodes)
                        {
                            names.Add(hintSubNode.Name);

                            switch (hintSubNode.Name)
                            {
                                case "oldName":
                                    oldName = hintSubNode.InnerText;
                                    break;
                                case "newName":
                                    upgradeHint.UpgradeName = hintSubNode.InnerText;
                                    break;
                                case "additionalAttributes":
                                    foreach (XmlNode attributesSubNode in hintSubNode.ChildNodes)
                                    {
                                        string attributeName = null;
                                        string attributeValue = null;

                                        switch (attributesSubNode.Name)
                                        {
                                            case "attribute":
                                                foreach (XmlNode attributeSubNode in attributesSubNode.ChildNodes)
                                                {
                                                    switch (attributeSubNode.Name)
                                                    {
                                                        case "name":
                                                            attributeName = attributeSubNode.InnerText;
                                                            break;
                                                        case "value":
                                                            attributeValue = attributeSubNode.InnerText;
                                                            break;
                                                    }
                                                }
                                                break;
                                        }
                                        upgradeHint.AdditionalAttributes[attributeName] = attributeValue;
                                    }
                                    break;
                                case "additionalElements":
                                    foreach (XmlNode elementsSubnode in hintSubNode.ChildNodes)
                                    {
                                        XmlElement elem = elementsSubnode as XmlElement;

                                        if (elem != null)
                                            upgradeHint.AdditionalElements.Add(elem);
                                    }
                                    break;
                            }
                        }

                        foundPriorNameHints[oldName] = upgradeHint;
                    }
                }
            }
            catch (Exception exception)
            {
                return; // if the XML file is badly formatted, return like it doesn't exist
            }

            // if everything parsed correctly, then add these names to the priorNameHints

            foreach (string key in foundPriorNameHints.Keys)
            {
                priorNameHints[key] = foundPriorNameHints[key];
            }
        }

        /// <summary>
        /// 添加导入函数
        /// </summary>
        /// <param name="library"></param>
        /// <param name="functions"></param>
        private void AddImportedFunctions(string library, IEnumerable<FunctionDescriptor> functions)
        {
            if (null == library || null == functions)
                throw new ArgumentNullException();


            Dictionary<string, FunctionGroup> fptrs;
            if (!importedFunctionGroups.TryGetValue(library, out fptrs))
            {
                fptrs = new Dictionary<string, FunctionGroup>();
                importedFunctionGroups[library] = fptrs;
            }

            foreach (FunctionDescriptor function in functions)
            {
                string qualifiedName = function.QualifiedName;
                FunctionGroup functionGroup;
                if (!fptrs.TryGetValue(qualifiedName, out functionGroup))
                {
                    functionGroup = new FunctionGroup(qualifiedName);
                    fptrs[qualifiedName] = functionGroup;
                }
                functionGroup.AddFunctionDescriptor(function);
            }
        }

        /// <summary>
        /// 添加内置函数
        /// </summary>
        /// <param name="functions"></param>
        private void AddBuiltinFunctions(IEnumerable<FunctionDescriptor> functions)
        {
            if (null == functions)
                throw new ArgumentNullException();

            foreach (FunctionDescriptor function in functions)
            {
                string qualifiedName = function.QualifiedName;

                if (CoreUtils.StartsWithDoubleUnderscores(qualifiedName))
                    continue;

                FunctionGroup functionGroup;
                if (!builtinFunctionGroups.TryGetValue(qualifiedName, out functionGroup))
                {
                    functionGroup = new FunctionGroup(qualifiedName);
                    builtinFunctionGroups[qualifiedName] = functionGroup;
                }
                functionGroup.AddFunctionDescriptor(function);
            }
        }

        /// <summary>
        /// 添加DesignScript的内置函数
        /// </summary>
        private void PopulateBuiltIns()
        {
            if (LibraryManagementCore == null)
                return;
            if (LibraryManagementCore.CodeBlockList.Count <= 0)
                return;

            var builtins = LibraryManagementCore.CodeBlockList[0]
                                                .procedureTable
                                                .Procedures
                                                .Where(p =>
                    !p.Name.StartsWith(Constants.kInternalNamePrefix) &&
                    !p.Name.Equals("Break"));

            IEnumerable<FunctionDescriptor> functions = from method in builtins
                                                        let arguments =
                                                            method.ArgumentInfos.Zip(
                                                                method.ArgumentTypes,
                                                                (arg, argType) =>
                                                                    new TypedParameter(
                                                                    arg.Name,
                                                                    argType))
                                                        let visibleInLibrary =
                                                            (method.MethodAttribute == null || !method.MethodAttribute.HiddenInLibrary)
                                                        let description = 
                                                            (method.MethodAttribute != null ? method.MethodAttribute.Description :String.Empty)
                                                        select
                                                            new FunctionDescriptor(new FunctionDescriptorParams
                                                            {
                                                                FunctionName = method.Name,
                                                                Summary = description,
                                                                Parameters = arguments,
                                                                PathManager = pathManager,
                                                                ReturnType = method.ReturnType,
                                                                FunctionType = FunctionType.GenericFunction,
                                                                IsVisibleInLibrary = visibleInLibrary,
                                                                IsBuiltIn = true,
                                                                IsPackageMember = false,
                                                                Assembly = Categories.BuiltIn
                                                            });

            AddBuiltinFunctions(functions);
            LoadLibraryMigrations(Categories.BuiltIn);
        }

        /// <summary>
        /// 获取二元函数参数
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<TypedParameter> GetBinaryFuncArgs()
        {
            yield return new TypedParameter("x", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var));
            yield return new TypedParameter("y", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var));
        }

        /// <summary>
        /// 获取一元函数参数
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<TypedParameter> GetUnaryFuncArgs()
        {
            return new List<TypedParameter> { new TypedParameter("x", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var)), };
        }

        /// <summary>
        /// 添加操作符
        /// </summary>
        private void PopulateOperators()
        {
            var args = GetBinaryFuncArgs();

            var ops = new[]
            {
                Op.GetOpFunction(Operator.add), Op.GetOpFunction(Operator.sub), Op.GetOpFunction(Operator.mul),
                Op.GetOpFunction(Operator.div), Op.GetOpFunction(Operator.eq), Op.GetOpFunction(Operator.ge),
                Op.GetOpFunction(Operator.gt), Op.GetOpFunction(Operator.mod), Op.GetOpFunction(Operator.le),
                Op.GetOpFunction(Operator.lt), Op.GetOpFunction(Operator.and), Op.GetOpFunction(Operator.or),
                Op.GetOpFunction(Operator.nq),
            };

            var functions =
                ops.Select(op => new FunctionDescriptor(new FunctionDescriptorParams
                {
                    FunctionName = op,
                    Parameters = args,
                    PathManager = pathManager,
                    FunctionType = FunctionType.GenericFunction,
                    IsBuiltIn = true,
                    IsPackageMember = false,
                    Assembly = Categories.Operators
                }))
                .Concat(new FunctionDescriptor(new FunctionDescriptorParams
                {
                    FunctionName = Op.GetUnaryOpFunction(UnaryOperator.Not),
                    Parameters = GetUnaryFuncArgs(),
                    PathManager = pathManager,
                    FunctionType = FunctionType.GenericFunction,
                    IsBuiltIn = true,
                    IsPackageMember = false,
                    Assembly = Categories.Operators
                }).AsSingleton());

            AddBuiltinFunctions(functions);
        }

        /// <summary>
        /// 填充预加载libraries
        /// </summary>
        private void PopulatePreloadLibraries()
        {
            HashSet<String> librariesThatNeedMigrationLoading = new HashSet<string>();

            foreach (ClassNode classNode in LibraryManagementCore.ClassTable.ClassNodes)
            {
                if (classNode.IsImportedClass && !string.IsNullOrEmpty(classNode.ExternLib))
                {
                    string library = Path.GetFileName(classNode.ExternLib);
                    ImportClass(library, classNode);
                    librariesThatNeedMigrationLoading.Add(library);
                }
            }

            foreach (String library in librariesThatNeedMigrationLoading)
            {
                LoadLibraryMigrations(library);
            }

        }

        /// <summary>
        /// 尝试从Default Argument Attribute获取默认参数表达式，然后解析为AST节点。
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="defaultArgumentNode"></param>
        /// <returns></returns>
        private bool TryGetDefaultArgumentFromAttribute(ArgumentInfo arg, out AssociativeNode defaultArgumentNode)
        {
            defaultArgumentNode = null;

            if (arg.Attributes == null)
                return false;

            object o;
            if (!arg.Attributes.TryGetAttribute("DefaultArgumentAttribute", out o))
                return false;

            var defaultExpression = o as string;
            if (string.IsNullOrEmpty(defaultExpression))
                return false;

            defaultArgumentNode = ParserUtils.ParseRHSExpression(defaultExpression, LibraryManagementCore);
           
            return defaultArgumentNode != null;
        }

        /// <summary>
        /// 导入Procedure
        /// </summary>
        /// <param name="library"></param>
        /// <param name="proc"></param>
        private void ImportProcedure(string library, ProcedureNode proc)
        {
            string procName = proc.Name;
            if (proc.IsAutoGeneratedThisProc ||
                // There could be DS functions that have private access 
                // that shouldn't be imported into the Library
                proc.AccessModifier == AccessModifier.Private ||
                CoreUtils.IsSetter(procName) ||
                CoreUtils.IsDisposeMethod(procName) ||
                CoreUtils.StartsWithDoubleUnderscores(procName))
            {
                return;
            }

            string obsoleteMessage = "";
            int classScope = proc.ClassID;
            string className = string.Empty;
            MethodAttributes methodAttribute = proc.MethodAttribute;
            ClassAttributes classAttribute = null;

            if (classScope != Constants.kGlobalScope)
            {
                var classNode = LibraryManagementCore.ClassTable.ClassNodes[classScope];

                classAttribute = classNode.ClassAttributes;
                className = classNode.Name;
            }

            // MethodAttribute's HiddenInLibrary has higher priority than
            // ClassAttribute's HiddenInLibrary
            var isVisible = true;
            var canUpdatePeriodically = false;
            if (methodAttribute != null)
            {
                isVisible = !methodAttribute.HiddenInLibrary;
                canUpdatePeriodically = methodAttribute.CanUpdatePeriodically;
            }
            else
            {
                if (classAttribute != null)
                    isVisible = !classAttribute.HiddenInLibrary;
            }

            FunctionType type;

            if (classScope == Constants.kGlobalScope)
            {
                type = FunctionType.GenericFunction;
            }
            else
            {
                if (CoreUtils.IsGetter(procName))
                {
                    type = proc.IsStatic
                        ? FunctionType.StaticProperty
                        : FunctionType.InstanceProperty;

                    string property;
                    if (CoreUtils.TryGetPropertyName(procName, out property))
                        procName = property;
                }
                else
                {
                    if (proc.IsConstructor)
                        type = FunctionType.Constructor;
                    else if (proc.IsStatic)
                        type = FunctionType.StaticMethod;
                    else
                        type = FunctionType.InstanceMethod;
                }
            }

            List<TypedParameter> arguments = proc.ArgumentInfos.Zip(
                proc.ArgumentTypes,
                (arg, argType) =>
                {
                    AssociativeNode defaultArgumentNode;
                    // Default argument specified by DefaultArgumentAttribute
                    // takes higher priority
                    if (!TryGetDefaultArgumentFromAttribute(arg, out defaultArgumentNode) 
                        && arg.IsDefault)
                    {
                        var binaryExpr = arg.DefaultExpression as BinaryExpressionNode;
                        if (binaryExpr != null)
                        {
                            defaultArgumentNode = binaryExpr.RightNode;
                        }
                    }
                    string shortName = null;
                    if (defaultArgumentNode != null)
                    {
                        shortName = defaultArgumentNode.ToString();
                        var rewriter = new ElementRewriter(LibraryManagementCore.ClassTable, LibraryManagementCore.BuildStatus.LogSymbolConflictWarning);
                        defaultArgumentNode = defaultArgumentNode.Accept(rewriter);
                    }
                    return new TypedParameter(arg.Name, argType, defaultArgumentNode, shortName);
                }).ToList();

            bool isLacingDisabled = false;
            IEnumerable<string> returnKeys = null;
            if (proc.MethodAttribute != null)
            {
                if (proc.MethodAttribute.ReturnKeys != null)
                    returnKeys = proc.MethodAttribute.ReturnKeys;
                if (proc.MethodAttribute.IsObsolete)
                    obsoleteMessage = proc.MethodAttribute.ObsoleteMessage;
                isLacingDisabled = proc.MethodAttribute.IsLacingDisabled;
            }

            var function = new FunctionDescriptor(new FunctionDescriptorParams
            {
                Assembly = library,
                ClassName = className,
                FunctionName = procName,
                Parameters = arguments,
                ReturnType = proc.ReturnType,
                FunctionType = type,
                IsVisibleInLibrary = isVisible,
                ReturnKeys = returnKeys,
                PathManager = pathManager,
                IsVarArg = proc.IsVarArg,
                ObsoleteMsg = obsoleteMessage,
                CanUpdatePeriodically = canUpdatePeriodically,
                IsBuiltIn = pathManager.PreloadedLibraries.Contains(library),
                IsPackageMember = packagedLibraries.Contains(library),
                IsLacingDisabled = isLacingDisabled
            });

            AddImportedFunctions(library, new[] { function });
        }

        /// <summary>
        /// 导入类
        /// </summary>
        /// <param name="library"></param>
        /// <param name="classNode"></param>
        private void ImportClass(string library, ClassNode classNode)
        {
            foreach (ProcedureNode proc in classNode.ProcTable.Procedures)
                ImportProcedure(library, proc);
                
        }

        /// <summary>
        /// library的类别
        /// </summary>
        public static class Categories
        {
            public const string BuiltIn = "BuiltIn";
            public const string Operators = "Operators";
            public const string Constructors = "Create";
            public const string MemberFunctions = "Actions";
            public const string Properties = "Query";
        }

        /// <summary>
        /// 用于比较library的路径
        /// </summary>
        private class LibraryPathComparer : IEqualityComparer<string>
        {
            public bool Equals(string path1, string path2)
            {
                string file1 = Path.GetFileName(path1);
                string file2 = Path.GetFileName(path2);
                return string.Compare(file1, file2, StringComparison.InvariantCultureIgnoreCase) == 0;
            }

            public int GetHashCode(string path)
            {
                string file = Path.GetFileName(path);
                return file.ToUpper().GetHashCode();
            }
        }
    }
}
