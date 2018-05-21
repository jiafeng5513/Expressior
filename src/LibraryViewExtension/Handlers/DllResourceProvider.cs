using System;
using System.IO;
using System.Reflection;
using CefSharp;
#pragma warning disable CS3001 // Argument type is not CLS-compliant
#pragma warning disable CS3002 // Return type is not CLS-compliant

namespace Dynamo.LibraryUI.Handlers
{
    /// <summary>
    /// 一个简单的DllResourceProvider，它提供来自DLL的资源。
    /// </summary>
    public class DllResourceProvider : ResourceProviderBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="baseUrl">请求提供服务的base url.</param>
        /// <param name="rootNamespace">Root namespace for this resource provider.</param>
        /// <param name="assembly">Assembly from where the resources are to be obtained.</param>
        public DllResourceProvider(string baseUrl, string rootNamespace, Assembly assembly = null)
        {
            RootNamespace = rootNamespace;
            BaseUrl = baseUrl;
            Assembly = assembly;
            if (Assembly == null)
                Assembly = Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// 资源提供者的根命名空间
        /// 例如,如果资源提供者是"Dynamo.LibraryUI.Web.Xxxx"("Xxxx" 是资源的实际名字),
        /// 那么根命名空间就是"Dynamo.LibraryUI.Web".
        /// </summary>
        public string RootNamespace { get; private set; }

        /// <summary>
        /// 提供服务的根URL
        /// 例如服务提供者是http://localhost/dist/v0.0.1/Xxxx
        /// 则跟URL是http://localhost/dist/v0.0.1
        /// </summary>
        public string BaseUrl { get; private set; }

        /// <summary>
        /// Assembly from where the resources are to be obtained.
        /// </summary>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// 通过此方法获取要请求的资源的流
        /// </summary>
        /// <param name="request">要请求的对象</param>
        /// <param name="extension">输出参数，其值是请求资源的扩展名.此扩展名不包含"."</param>
        /// <returns>Returns the stream if the requested resource can be found, or null otherwise.</returns>
        public override Stream GetResource(IRequest request, out string extension)
        {
            extension = "txt";
            var uri = new Uri(request.Url);
            string resourceName;
            var assembly = GetResourceAssembly(uri, out resourceName);
            if (null == assembly) return null;

            var stream = assembly.GetManifestResourceStream(resourceName);
            var idx = resourceName.LastIndexOf('.');
            if (idx > 0)
            {
                extension = resourceName.Substring(idx+1);
            }
            
            return stream;
        }

        protected virtual Assembly GetResourceAssembly(Uri url, out string resourceName)
        {
            var path = url.AbsoluteUri.Replace(BaseUrl, "").Replace('/', '.');
            resourceName = RootNamespace + path;
            return this.Assembly;
        }
    }
}
