using System.Collections.Generic;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using DynamoConversions;

using GeometryUI;

namespace Dynamo.Wpf
{
    public class ExportWithUnitsViewModel : NotificationObject 
    {
        private readonly ExportWithUnits exportWithUnitsModel;
        public DelegateCommand ToggleButtonClick { get; set; }
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;


        public int SliderValue
        {
            get { return exportWithUnitsModel.ValueofsliderOfSlider; }
            set { exportWithUnitsModel.ValueofsliderOfSlider = value; }
        }

        public ExportWithUnitsViewModel(ExportWithUnits model, NodeView nodeView)
        {
            exportWithUnitsModel = model;           
            nodeViewModel = nodeView.ViewModel;
            nodeModel = nodeView.ViewModel.NodeModel;
            model.PropertyChanged +=model_PropertyChanged;      
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ValueofsliderOfSlider":
                    RaisePropertyChanged("ValueofsliderOfSlider");
                    break;
            }
        }
    }
}
