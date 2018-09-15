using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Updates;

namespace Dynamo.Utilities
{
    class VersionManager
    {
        private static BinaryVersion productVersion;
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
    }
}
