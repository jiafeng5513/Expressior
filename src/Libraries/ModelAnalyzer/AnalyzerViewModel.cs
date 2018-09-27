using System.Collections.Generic;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

using ModelAnalyzerUI;

namespace Dynamo.Wpf
{
    public partial class AnalyzerViewModel : NotificationObject 
    {
        private readonly AnalyzerModel _analyzerModelModel;
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;




        public AnalyzerViewModel(AnalyzerModel modelModel, NodeView nodeView)
        {
            _analyzerModelModel = modelModel;
            nodeViewModel = nodeView.ViewModel;
            nodeModel = nodeView.ViewModel.NodeModel;
            modelModel.PropertyChanged +=model_PropertyChanged;
            InitializeDelegateCommands();
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
        #region DelegateCommand

        private void Explore(object parameters)
        {
            
            CanSeeProgressBar = !CanSeeProgressBar;
        }
        private void Predict(object parameters)
        {

        }
        #endregion

        private bool canSeeProgressBar=false;
        public bool CanSeeProgressBar
        {
            get { return canSeeProgressBar; }
            set { canSeeProgressBar = value;
                RaisePropertyChanged("CanSeeProgressBar");
            }
        }
    }
}
