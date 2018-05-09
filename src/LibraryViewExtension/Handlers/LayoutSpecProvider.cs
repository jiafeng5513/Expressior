using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CefSharp;
using Dynamo.Search.SearchElements;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.LibraryUI.Handlers
{
    /// <summary>
    /// Implements LayoutSepc resource provider, by default it reads the spec
    /// from a given json resource. It also allows to update certain specific
    /// sections from the layout spec for a given set of NodeSearchElements
    ///
    ///实现LayoutSepc资源提供者，默认情况下它从给定的json资源中读取规范。
    /// 它还允许从给定的一组NodeSearchElement的布局规范中更新某些特定部分
    ///
    /// 
    /// </summary>
    class LayoutSpecProvider : ResourceProviderBase
    {
        private Stream resourceStream;
        private readonly ILibraryViewCustomization customization;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resource">The resource name of json resource in the given assembly.</param>
        /// <param name="assembly">Assembly which contains the specified resource</param>
        public LayoutSpecProvider(ILibraryViewCustomization customization, string resource, Assembly assembly = null) : base(false)
        {
            assembly = assembly == null ? Assembly.GetExecutingAssembly() : assembly;
            var stream = assembly.GetManifestResourceStream(resource);

            //Get the spec from the stream
            var spec = LayoutSpecification.FromJSONStream(stream);
            customization.AddSections(spec.sections);
            this.customization = customization;
            this.customization.SpecificationUpdated += OnSpecificationUpdate;
        }

        private void OnSpecificationUpdate(object sender, EventArgs e)
        {
            DisposeResourceStream();
        }

        /// <summary>
        /// 获得请求的资源
        /// </summary>
        public override Stream GetResource(IRequest request, out string extension)
        {
            extension = "json";
            if (resourceStream == null)
            {
                resourceStream = customization.ToJSONStream();
            }

            return resourceStream;
        }

        private void DisposeResourceStream()
        {
            if (resourceStream != null)
            {
                resourceStream.Dispose();
                resourceStream = null;
            }
        }
    }
}
