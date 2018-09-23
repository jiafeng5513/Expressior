using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using TensorFlow;
using Google.Protobuf;
using org.tensorflow.framework;
using System.Runtime.Serialization.Formatters.Binary;


namespace DecodePbByCS
{
    class Program
    {
        static string inFile = "E:\\VisualStudio\\Expressior\\src\\Experimental\\mnist\\out\\model\\saved_model.pb";
        static string outFile = "E:\\VisualStudio\\Expressior\\src\\Experimental\\mnist\\out\\model_spectext\\csout.txt";

        static void Main(string[] args)
        {
            //图加载
            using (var graph = new TFGraph())
            {
                //graph.Import(File.ReadAllBytes("saved_model.pb"));
            }
            //TFBuff编解码
            var hello = Encoding.UTF8.GetBytes("Hello, world!");
            var buffer = new TFBuffer(hello);
            var bytes = buffer.ToArray();
            Console.WriteLine(Encoding.UTF8.GetString(bytes));
            
            //方案1
            //string stringdata = File.ReadAllText(FileName);
            //string [] s=stringdata.Split('\n');
            //FileStream fs = new FileStream("E:\\VisualStudio\\Expressior\\src\\Experimental\\mnist\\out\\model_spectext\\csout.txt", FileMode.Create);
            //StreamWriter sw =new StreamWriter(fs,Encoding.UTF8);
            //for (int i = 0; i <s.Length; i++)
            //{
            //    sw.WriteLine(s[i]);
            //}

            //方案2
            //byte[] data = File.ReadAllBytes(inFile);
            //var datastring = "";
            FileStream fs = new FileStream(outFile, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //for (int i = 0; i < 1000; i++)
            //{
            //    byte[] str = new byte[2];
            //    str[0] = data[i];
            //    datastring = Encoding.Unicode.GetString(str);
            //    sw.Write(datastring);
            //}
            //方案3:proto编解码
            byte[] data = File.ReadAllBytes(inFile);
            MemoryStream ms1 = new MemoryStream(data);
            GraphDef Mygraph = Deserialize<GraphDef>(ms1);
            
            







            sw.Flush();
            sw.Close();
            fs.Close();

            //String temp = graph.ToString();
            ////Console.WriteLine(graph);

            ////获得字节数组
            //TFBuffer outputGraphDef=new TFBuffer();
            //graph.ToGraphDef(outputGraphDef);


            ////String str = System.Text.Encoding.ASCII.GetString(outputGraphDef.ToArray());
            //byte[] data = outputGraphDef.ToArray();
            ////string result = Encoding.GetEncoding("ascii").GetString(data);
            ////Google.Protobuf.ByteString byteString = Google.Protobuf.ByteString.CopyFrom(data, 0, data.Length);

            ////byteString.WriteTo(fs);

            ////开始写入
            //String tmp2 = Encoding.UTF8.GetString(data);
            //sw.Write(stringdata);




            ////开始写入
            //fs.Write(data, 0, data.Length);
            ////清空缓冲区、关闭流
            //fs.Flush();
            //fs.Close();






        }
    }
}
