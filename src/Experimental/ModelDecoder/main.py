import tensorflow as tf
model = '../../../data/optimized_graph.pb'


"""
用TensorBoard显示模型结构
"""
def ShowInTensorBoard():
    graph = tf.get_default_graph()
    graph_def = graph.as_graph_def()
    graph_def.ParseFromString(tf.gfile.FastGFile(model, 'rb').read())
    tf.import_graph_def(graph_def, name='graph')
    summaryWriter = tf.summary.FileWriter('log/', graph)
    """
    tensorboard --logdir log
    """
"""
输出模型信息到文件
"""
def ShowInTextFile():
    with tf.Session() as sess:
        with open(model, 'rb') as model_file:
            graph_def = tf.GraphDef()
            graph_def.ParseFromString(model_file.read())
            f = open(r'out.txt', 'w')
            print(graph_def, file=f)

if __name__ == '__main__':
    #ShowInTensorBoard()
    ShowInTextFile()