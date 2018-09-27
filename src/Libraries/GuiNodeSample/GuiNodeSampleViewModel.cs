using System.Collections.Generic;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

using ModelAnalyzerUI;

namespace Dynamo.Wpf
{
    public class GuiNodeSampleViewModel : NotificationObject 
    {
        private readonly GuiNodeSampleModel _guiNodeSampleModelModel;
        public DelegateCommand ToggleButtonClick { get; set; }
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;

        public int SliderValue
        {
            get { return _guiNodeSampleModelModel.ValueofsliderOfSlider; }
            set { _guiNodeSampleModelModel.ValueofsliderOfSlider = value; }
        }
        //public ConversionUnit SelectedExportedUnit
        //{
        //    get { return _guiNodeSampleModelModel.SelectedExportedUnit; }
        //    set
        //    {
        //        _guiNodeSampleModelModel.SelectedExportedUnit = value;                             
        //    }
        //}

        //public List<ConversionUnit> SelectedExportedUnitsSource
        //{
        //    get { return _guiNodeSampleModelModel.SelectedExportedUnitsSource; }
        //    set
        //    {
        //        _guiNodeSampleModelModel.SelectedExportedUnitsSource = value;               
        //    }
        //}

        public GuiNodeSampleViewModel(GuiNodeSampleModel modelModel, NodeView nodeView)
        {
            _guiNodeSampleModelModel = modelModel;           
            nodeViewModel = nodeView.ViewModel;
            nodeModel = nodeView.ViewModel.NodeModel;
            modelModel.PropertyChanged +=model_PropertyChanged;      
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
