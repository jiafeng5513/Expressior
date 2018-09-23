using System;
using System.IO;
using Emgu.TF;
using Buffer = Emgu.TF.Buffer;

namespace DeepLearning
{
    class ModelDeploy
    {
        //private FileDownloadManager _downloadManager;
        private Graph _graph = null;
        private Status _status = null;
        private String _inputName = null;
        private String _modelFile = null;
        private String _labelFile = null;
        private String _outputName = null;

        public ModelDeploy(Status status = null)
        {
            _status = status;
        }

        //进行手动初始化
        public void Init(String[] modelFiles = null, String inputName = null, String outputName = null)
        {
            _inputName = inputName;
            _outputName = outputName;
            String[] fileNames = modelFiles;

            _modelFile = modelFiles[0];
            _labelFile = modelFiles[1];

            if (_graph != null)
                _graph.Dispose();
            _graph = new Graph();
            String localFileName = modelFiles[0];
            byte[] model = File.ReadAllBytes(localFileName);
            Buffer modelBuffer = Buffer.FromString(model);
            using (ImportGraphDefOptions options = new ImportGraphDefOptions())
                _graph.ImportGraphDef(modelBuffer, options, _status);

        }


        public String[] Labels
        {
            get
            {
                return File.ReadAllLines(_labelFile);
            }
        }

        public float[] Recognize(Tensor image)
        {
            Session inceptionSession = new Session(_graph);
            Tensor[] finalTensor = inceptionSession.Run(new Output[] { _graph[_inputName] }, new Tensor[] { image },
                new Output[] { _graph[_outputName] });
            float[] probability = finalTensor[0].GetData(false) as float[];
            return probability;
        }

        public RecognitionResult MostLikely(Tensor image)
        {
            float[] probability = Recognize(image);

            RecognitionResult result = new RecognitionResult();
            result.Label = String.Empty;

            if (probability != null)
            {
                String[] labels = Labels;
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
                result.Label = labels[maxIdx];
                result.Probability = maxVal;
            }
            return result;
        }

        public class RecognitionResult
        {
            public String Label;
            public double Probability;
        }

    }
}
