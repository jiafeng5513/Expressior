using System;
using System.Diagnostics;
using Emgu.TF;
using Emgu.TF.Models;

namespace DeepLearning
{
    public class Example
    {
        private Example() { }

        public static string Predict(string ModelFile = " ", string LabelFile=" ", string inputFile=" ")
        {
            ModelDeploy modelDecoderGraph = new ModelDeploy(  );
            modelDecoderGraph.Init(new string[] { ModelFile, LabelFile }, "Mul", "final_result");

            Tensor imageTensor = ImageIO.ReadTensorFromImageFile(inputFile, 299, 299, 128.0f, 1.0f / 128.0f);
            //modelDecoderGraph.ImportGraph();
            Stopwatch sw = Stopwatch.StartNew();
            float[] probability = modelDecoderGraph.Recognize(imageTensor);
            sw.Stop();

            String resStr = String.Empty;
            if (probability != null)
            {
                String[] labels = modelDecoderGraph.Labels;
                float maxVal = 0;
                int maxIdx = 0;
                for (int i = 0; i < probability.Length; i++)
                {
                    if (probability[i] > maxVal)
                    {
                        maxVal = probability[i];
                        maxIdx = i;
                    }
                }
                resStr = String.Format("Object is {0} with {1}% probability. \n Recognition done in {2} milliseconds.", labels[maxIdx], maxVal * 100, sw.ElapsedMilliseconds);
            }

            return resStr;
        }
    }
}
