
#import sys
#sys.path.append('/home/evjang/thesis/tensor_debugger')
import tdb
from tdb.examples import mnist, viz
import matplotlib.pyplot as plt
import tensorflow as tf
import urllib


#=========================================================================

(train_data_node,
    train_labels_node,
    validation_data_node,
    test_data_node,
    # predictions
    train_prediction,
    validation_prediction,
    test_prediction,
    # weights
    conv1_weights,
    conv2_weights,
    fc1_weights,
    fc2_weights,
    # training
    optimizer,
    loss,
    learning_rate,
    summaries) = mnist.build_model()

#=========================================================================
def viz_activations(ctx, m):
    plt.matshow(m.T,cmap=plt.cm.gray)
    plt.title("LeNet Predictions")
    plt.xlabel("Batch")
    plt.ylabel("Digit Activation")

#=========================================================================

# plotting a user-defined function 'viz_activations'
p0=tdb.plot_op(viz_activations,inputs=[train_prediction])
# weight variables are of type tf.Variable, so we need to find the corresponding tf.Tensor instead
g=tf.get_default_graph()
p1=tdb.plot_op(viz.viz_conv_weights,inputs=[g.as_graph_element(conv1_weights)])
p2=tdb.plot_op(viz.viz_conv_weights,inputs=[g.as_graph_element(conv2_weights)])
p3=tdb.plot_op(viz.viz_fc_weights,inputs=[g.as_graph_element(fc1_weights)])
p4=tdb.plot_op(viz.viz_fc_weights,inputs=[g.as_graph_element(fc2_weights)])
p2=tdb.plot_op(viz.viz_conv_hist,inputs=[g.as_graph_element(conv1_weights)])
ploss=tdb.plot_op(viz.watch_loss,inputs=[loss])

(train_data, 
 train_labels, 
 validation_data, 
 validation_labels, 
 test_data, 
 test_labels) = mnist.get_data("D:/Libraries/Anaconda3/Lib/site-packages/tdb/examples/input_data/")

# start the TensorFlow session that will be used to evaluate the graph
s=tf.InteractiveSession()
tf.global_variables_initializer().run()

BATCH_SIZE = 64
NUM_EPOCHS = 5
TRAIN_SIZE=10000

for step in range(NUM_EPOCHS * TRAIN_SIZE // BATCH_SIZE):
    offset = (step * BATCH_SIZE) % (TRAIN_SIZE - BATCH_SIZE)
    batch_data = train_data[offset:(offset + BATCH_SIZE), :, :, :]
    batch_labels = train_labels[offset:(offset + BATCH_SIZE)]
    feed_dict = {
        train_data_node: batch_data,
        train_labels_node: batch_labels
    }
    # run training node and visualization node
    status,result=tdb.debug([optimizer,p0], feed_dict=feed_dict, session=s)
    if step % 10 == 0:  
        status,result=tdb.debug([loss,p1,p2,p3,p4,ploss], feed_dict=feed_dict, breakpoints=None, break_immediately=False, session=s)
        print('loss: %f' % (result[0]))

#=========================================================================



#=========================================================================