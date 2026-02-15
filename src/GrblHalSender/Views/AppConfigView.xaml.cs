using System.Collections.ObjectModel;
using System.Windows.Controls;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Views
{
    public partial class AppConfigView : UserControl
    {
       
        private GrblViewModel _grblModel;
        private  AppConfigViewModel _vModel;

        public AppConfigView(GrblViewModel grblViewModel, ContentManager contentManager)
        {

            InitializeComponent();
            _grblModel = grblViewModel;
             _vModel = new AppConfigViewModel();
             this.DataContext = _vModel;
            contentManager.RegisterViewAndModel("appSettingsView", _vModel);
        }

        public AppConfigView()
        {
            InitializeComponent();
        }

        public void Setup(ObservableCollection<UserControl> controls)
        {
            AppConfigControls.DataContext = _vModel.ConfigBase;
            AppConfigControls.ItemsSource = controls;
        }


    }
}