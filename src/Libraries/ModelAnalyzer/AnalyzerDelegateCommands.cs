using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelegateCommand = Dynamo.UI.Commands.DelegateCommand;
namespace Dynamo.Wpf
{
    partial class AnalyzerViewModel
    {
        private void InitializeDelegateCommands()
        {
            ExploreCommand = new DelegateCommand(Explore, o => true);
            PredictCommand = new DelegateCommand(Predict, o => true);
        }
        public DelegateCommand ExploreCommand { get; set; }
        public DelegateCommand PredictCommand { get; set; }
    }
}
