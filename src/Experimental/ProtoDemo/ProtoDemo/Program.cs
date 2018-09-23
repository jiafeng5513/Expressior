using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.tensorflow.framework;
using Google.Protobuf;
namespace ProtoDemo
{
    class Program
    {
        private static string InputFile = "E:/VisualStudio/Expressior/src/Experimental/mnist/out/model/saved_model.pb";
        private static string OutputFile = "E:/VisualStudio/Expressior/src/Experimental/mnist/out/model_spectext/csout.txt";
        /// <summary>
        /// Dump GraphDef object into a Json File.
        /// </summary>
        /// <param name="datasource">GraphDef object</param>
        /// <param name="JsonFilePath">json File Path</param>
        void JsonDump(GraphDef datasource,string JsonFilePath)
        {

        }

        static void Main(string[] args)
        {
            using (Stream s = new FileStream(InputFile, FileMode.Open))
            {
                GraphDef st = GraphDef.Parser.ParseFrom(s);
                for (int i = 0; i < st.Node.Count; i++)
                {
                    Console.WriteLine(st.Node[i].Name);
                }
            }


            

            //FileStream fs = new FileStream(OutputFile, FileMode.Create);
            //StreamWriter sw = new StreamWriter(fs);

            ////CodedOutputStream gos=new CodedOutputStream();
            ////byte[] outArray=Mygraph.ToByteArray();
            ////for (int i = 0; i < outArray.Length; i++)
            ////{
            ////    sw.Write(outArray[i]);
            ////}
            //sw.Flush();
            //sw.Close();
            //fs.Close();
        }
    }
}
