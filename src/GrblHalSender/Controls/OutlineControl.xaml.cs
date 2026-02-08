
using System.Windows;
using System.Windows.Controls;
using GrblHalSender.GrblCore;
using GrblHalSender.GrblCore.Config;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    public partial class OutlineControl : UserControl
    {
        public OutlineControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FeedRateProperty = DependencyProperty.Register(nameof(FeedRate), typeof(int), typeof(OutlineControl), new PropertyMetadata(500));
        public int FeedRate
        {
            get => (int)GetValue(FeedRateProperty);
            set => SetValue(FeedRateProperty, value);
        }

        private void button_Go(object sender, RoutedEventArgs e)
        {
            GrblViewModel model = DataContext as GrblViewModel;

            if (GHalSenderConfig.Settings.Base.OutlineFeedRate != FeedRate)
            {
                GHalSenderConfig.Settings.Base.OutlineFeedRate = FeedRate;
                GHalSenderConfig.Settings.Save();
            }

            if (model == null || !model.IsFileLoaded) return;
            if (!model.IsParserStateLive)
                GrblParserState.Get();

            var wasMetric = GrblParserState.IsMetric;
            var gcode = $"G90G{(model.IsMetric ? 21 : 20)}G1F{FeedRate}\r";

            gcode += $"X{model.ProgramLimits.MinX.ToInvariantString()}Y{model.ProgramLimits.MinY.ToInvariantString(model.Format)}\r";
            gcode += $"Y{model.ProgramLimits.MaxY.ToInvariantString(model.Format)}\r";
            gcode += $"X{model.ProgramLimits.MaxX.ToInvariantString(model.Format)}\r";
            gcode += $"Y{model.ProgramLimits.MinY.ToInvariantString(model.Format)}\r";
            gcode += $"X{model.ProgramLimits.MinX.ToInvariantString(model.Format)}\r";
            if(model.IsMetric != wasMetric)
                gcode += $"G{(wasMetric ? 21 : 20)}\r";

            model.ExecuteCommand(gcode);
        }

        private void OutlineControl_Loaded(object sender, RoutedEventArgs e)
        {
            FeedRate = GHalSenderConfig.Settings.Base.OutlineFeedRate;
        }
    }
}
