using System.Collections.Generic;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

using ModelAnalyzerUI;

namespace Dynamo.Wpf
{
    public class AnalyzerViewModel : NotificationObject 
    {
        private readonly AnalyzerModel _analyzerModelModel;
        public DelegateCommand ToggleButtonClick { get; set; }
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;

        public int SliderValue
        {
            get { return _analyzerModelModel.ValueofsliderOfSlider; }
            set { _analyzerModelModel.ValueofsliderOfSlider = value; }
        }
        //public ConversionUnit SelectedExportedUnit
        //{
        //    get { return _analyzerModelModel.SelectedExportedUnit; }
        //    set
        //    {
        //        _analyzerModelModel.SelectedExportedUnit = value;                             
        //    }
        //}

        //public List<ConversionUnit> SelectedExportedUnitsSource
        //{
        //    get { return _analyzerModelModel.SelectedExportedUnitsSource; }
        //    set
        //    {
        //        _analyzerModelModel.SelectedExportedUnitsSource = value;               
        //    }
        //}

        public AnalyzerViewModel(AnalyzerModel modelModel, NodeView nodeView)
        {
            _analyzerModelModel = modelModel;           
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
