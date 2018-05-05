# ==============================================================================
# 天使微积分注释：此处的代码已经经过了修改
# ==============================================================================
"""A simple MNIST classifier which displays summaries in TensorBoard.

This is an unimpressive MNIST model, but it is a good example of using
tf.name_scope to make a graph legible in the TensorBoard graph explorer, and of
naming summary tags so that they are grouped meaningfully in TensorBoard.

It demonstrates the functionality of every TensorBoard dashboard.
"""
from __future__ import absolute_import
from __future__ import division
from __future__ import print_function

import argparse
import sys

import tensorflow as tf

from tensorflow.examples.tutorials.mnist import input_data
import time
FLAGS = None


def train():
  # Import data
  mnist = input_data.read_data_sets(FLAGS.data_dir,
                                    one_hot=True,
                                    fake_data=FLAGS.fake_data)

  sess = tf.InteractiveSession()
  # Create a multilayer model.

  # Input placeholders
  with tf.name_scope('input'):
    x = tf.placeholder(tf.float32, [None, 784], name='x-input')#784=28*28,x维度线性存储一张图片
    y_ = tf.placeholder(tf.float32, [None, 10], name='y-input')#10=0~9,y维度线性存储十个类别,是哪一类,那个位置就是1,其他的9个位置是0,

  with tf.name_scope('input_reshape'):
    image_shaped_input = tf.reshape(x, [-1, 28, 28, 1])#把二维存储的图片和线性存储的向量对应起来
    tf.summary.image('input', image_shaped_input, 10)#采样,用tensorboard可视化

  # We can't initialize these variables to 0 - the network will get stuck.
  def weight_variable(shape):
    """权值初始化子函数"""
    initial = tf.truncated_normal(shape, stddev=0.1)#权重初始化为0.1
    return tf.Variable(initial)

  def bias_variable(shape):
    """bias初始化子函数"""
    initial = tf.constant(0.1, shape=shape)#初始化bias项为0.1
    return tf.Variable(initial)

  def variable_summaries(var):
    """对变量附加摘要信息."""
    with tf.name_scope('summaries'):
      mean = tf.reduce_mean(var)
      tf.summary.scalar('mean', mean)
      with tf.name_scope('stddev'):
        stddev = tf.sqrt(tf.reduce_mean(tf.square(var - mean)))
      tf.summary.scalar('stddev', stddev)
      tf.summary.scalar('max', tf.reduce_max(var))
      tf.summary.scalar('min', tf.reduce_min(var))
      tf.summary.histogram('histogram', var)#直方图

  def nn_layer(input_tensor, input_dim, output_dim, layer_name, act=tf.nn.relu):
    """
       用来定义一个神经网络层的子函数,
       参数:输入张量,输入维度,输出维度,本层名字,本层附加的操作
       这个函数会使用一个with块包围整个操作,这个块的名字就是参数中层的名字
    """
    # 使用层名命名的with块
    with tf.name_scope(layer_name):
      with tf.name_scope('weights'):
        weights = weight_variable([input_dim, output_dim])
        variable_summaries(weights)
      with tf.name_scope('biases'):
        biases = bias_variable([output_dim])
        variable_summaries(biases)
      with tf.name_scope('Wx_plus_b'):
        preactivate = tf.matmul(input_tensor, weights) + biases
        tf.summary.histogram('pre_activations', preactivate)
      activations = act(preactivate, name='activation')
      tf.summary.histogram('activations', activations)
      return activations
  # 定义layer1,前驱为X,输入784,输出500
  hidden1 = nn_layer(x, 784, 500, 'layer1')

  with tf.name_scope('dropout'):
    keep_prob = tf.placeholder(tf.float32)
    tf.summary.scalar('dropout_keep_probability', keep_prob)
    dropped = tf.nn.dropout(hidden1, keep_prob)

  # 定义layer2,前驱为dropped,输入500,输出10
  y = nn_layer(dropped, 500, 10, 'layer2', act=tf.identity)

#交叉熵损失函数
  with tf.name_scope('cross_entropy'):
    # 原始的交叉熵损失函数如下,由于稳定性不好
    # tf.reduce_mean(-tf.reduce_sum(y_ * tf.log(tf.softmax(y)),reduction_indices=[1]))
    # 所以在这里使用的是tf.nn.softmax_cross_entropy_with_logits函数
    # 网络层的原始输出经过这个函数后,每个batch进行一次平均

    diff = tf.nn.softmax_cross_entropy_with_logits(labels=y_, logits=y)
    with tf.name_scope('total'):
      cross_entropy = tf.reduce_mean(diff)
  tf.summary.scalar('cross_entropy', cross_entropy)

  with tf.name_scope('train'):
    train_step = tf.train.AdamOptimizer(FLAGS.learning_rate).minimize(
        cross_entropy)

  with tf.name_scope('accuracy'):
    with tf.name_scope('correct_prediction'):
      correct_prediction = tf.equal(tf.argmax(y, 1), tf.argmax(y_, 1))
    with tf.name_scope('accuracy'):
      accuracy = tf.reduce_mean(tf.cast(correct_prediction, tf.float32))
  tf.summary.scalar('accuracy', accuracy)

  # 合并所有的summaries,写入到指定目录,默认目录如下
  # /tmp/tensorflow/mnist/logs/mnist_with_summaries
  merged = tf.summary.merge_all()
  train_writer = tf.summary.FileWriter(FLAGS.log_dir + '/train', sess.graph)
  test_writer = tf.summary.FileWriter(FLAGS.log_dir + '/test')
  tf.global_variables_initializer().run()

  # 每逢10步,载测试集上测试准确度,并记录到summaries
  # 其他情况,执行训练,

  def feed_dict(train):
    """数据供给子函数"""
    if train or FLAGS.fake_data:
      xs, ys = mnist.train.next_batch(100, fake_data=FLAGS.fake_data)
      k = FLAGS.dropout
    else:
      xs, ys = mnist.test.images, mnist.test.labels
      k = 1.0
    return {x: xs, y_: ys, keep_prob: k}

  for i in range(FLAGS.max_steps):
    if i % 10 == 0:  # Record summaries and test-set accuracy
      summary, acc = sess.run([merged, accuracy], feed_dict=feed_dict(False))
      test_writer.add_summary(summary, i)
      print('迭代次数:%s 准确率:%s ' % (i, acc))
    else:  # Record train set summaries, and train
      if i % 100 == 99:  # Record execution stats
        run_options = tf.RunOptions(trace_level=tf.RunOptions.FULL_TRACE)
        run_metadata = tf.RunMetadata()
        summary, _ = sess.run([merged, train_step],
                              feed_dict=feed_dict(True),
                              options=run_options,
                              run_metadata=run_metadata)
        train_writer.add_run_metadata(run_metadata, 'step%03d' % i)
        train_writer.add_summary(summary, i)
        print('Adding run metadata for', i)
      else:  # Record a summary
        summary, _ = sess.run([merged, train_step], feed_dict=feed_dict(True))
        train_writer.add_summary(summary, i)
  train_writer.close()
  test_writer.close()


def main(_):
  #主函数
  start=time.clock()
  if tf.gfile.Exists(FLAGS.log_dir):
    tf.gfile.DeleteRecursively(FLAGS.log_dir)
  tf.gfile.MakeDirs(FLAGS.log_dir)
  train()
  end=time.clock()
  print("总共用时:%f 秒"%(end-start))

if __name__ == '__main__':
  #如果从此函数启动,先检视所有的程序参数,再启动主函数
  parser = argparse.ArgumentParser()
  parser.add_argument('--fake_data', nargs='?', const=True, type=bool,
                      default=False,
                      help='If true, uses fake data for unit testing.')
  parser.add_argument('--max_steps', type=int, default=1000,
                      help='Number of steps to run trainer.')
  parser.add_argument('--learning_rate', type=float, default=0.001,
                      help='Initial learning rate')
  parser.add_argument('--dropout', type=float, default=0.9,
                      help='Keep probability for training dropout.')
  parser.add_argument(
      '--data_dir',
      type=str,
      default='./tmp/tensorflow/mnist/input_data',
      help='Directory for storing input data')
  parser.add_argument(
      '--log_dir',
      type=str,
      default='./tmp/tensorflow/mnist/logs/mnist_with_summaries',
      help='Summaries log directory')
  FLAGS, unparsed = parser.parse_known_args()
  tf.app.run(main=main, argv=[sys.argv[0]] + unparsed)