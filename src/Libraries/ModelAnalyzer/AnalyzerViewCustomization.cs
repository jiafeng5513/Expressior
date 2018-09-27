using System;
using System.Collections.Generic;
using System.Text;

using Dynamo.Controls;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Controls;

using ModelAnalyzerUI;

namespace Dynamo.Wpf.NodeViewCustomizations
{
    public class AnalyzerViewCustomization : INodeViewCustomization<AnalyzerModel>
    {
        private NodeModel nodeModel;
        private AnalyzerView exporterControl;
        private NodeViewModel nodeViewModel;
        private AnalyzerModel convertModel;
        private AnalyzerViewModel exporterViewModel;

        public void CustomizeView(AnalyzerModel model, NodeView nodeView)
        {
            nodeModel = nodeView.ViewModel.NodeModel;
            nodeViewModel = nodeView.ViewModel;
            convertModel = model;
            
            exporterControl = new AnalyzerView(model, nodeView)
            {
                DataContext = new AnalyzerViewModel(model, nodeView),
            };

            exporterViewModel = exporterControl.DataContext as AnalyzerViewModel;
            nodeView.inputGrid.Children.Add(exporterControl);
            exporterControl.Loaded += converterControl_Loaded;
            //exporterControl.SelectExportedUnit.PreviewMouseUp += SelectExportedUnit_PreviewMouseUp;
        }

        //private void SelectExportedUnit_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
        //    var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
        //    WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        //}

        private void converterControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        public void Dispose()
        {
            //exporterControl.SelectExportedUnit.PreviewMouseUp -= SelectExportedUnit_PreviewMouseUp;
        }
    }
}
