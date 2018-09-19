import tensorflow as tf
from tensorflow.python.framework import graph_util
from tensorflow.python.framework import tensor_util
model = '../../../data/optimized_graph.pb'
model2='../mnist/saved_model.pb'

"""
用TensorBoard显示模型结构
"""
def ShowInTensorBoard():
    graph = tf.get_default_graph()
    graph_def = graph.as_graph_def()
    graph_def.ParseFromString(tf.gfile.FastGFile(model2, 'rb').read())
    tf.import_graph_def(graph_def, name='graph')
    summaryWriter = tf.summary.FileWriter('TensorBoardLog/', graph)
    """
    tensorboard --logdir TensorBoardLog
    """
"""
输出模型信息到文件
"""
def ShowInTextFile():
    with tf.Session() as sess:
        with open(model2, 'rb') as model_file:
            graph_def = tf.GraphDef()
            graph_def.ParseFromString(model_file.read())
            f = open(r'TensorBoardLog/out.txt', 'w')
            print(graph_def, file=f)
            for n in graph_def.node:
                print (tensor_util.MakeNdarray(n.attr['value'].tensor),file=f)

if __name__ == '__main__':
    #ShowInTensorBoard()
    ShowInTextFile()