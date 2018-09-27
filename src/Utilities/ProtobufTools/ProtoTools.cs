using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.tensorflow.framework;

namespace ProtobufTools
{
    public class ProtoTools
    {
        /// <summary>
        /// dump GraphDef into Json File
        /// </summary>
        /// <param name="data">GraphDef object</param>
        /// <param name="jsonpath">File Path for json</param>
        public static void DumpJson(GraphDef data, string jsonpath)
        {

        }
        public static string TestFunc(int a, int b, int c)
        {
            return (a + b + c).ToString();
        }
        public static string TestFunc(IEnumerable<string> a, string b, string c)
        {
            return (Int16.Parse(b) + Int16.Parse(b) + Int16.Parse(c)).ToString();
        }
    }
}
