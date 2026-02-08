using CommunityToolkit.Mvvm.Input;
using GrblHalSender.GrblCore;
using GrblHalSender.GrblCore.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GrblHalSender.ViewModels
{
    public class RapidAtcViewModel : ViewModelBase
    {
        public GrblViewModel GrblViewModel { get; set; }
        public ObservableCollection<AtcTool> AtcToolList { get; set; } = [];

       

        public ICommand TlsCommand { get; }
        public ICommand UnloadTool { get; }
        public ICommand LoadTool { get; }
        public RapidAtcViewModel(GrblViewModel mainViewModel)
        {
            GrblViewModel = mainViewModel;
            TlsCommand = new RelayCommand(Tls);
            UnloadTool = new RelayCommand(Unload);
            LoadTool = new RelayCommand<int>(Load);
            var config = GHalSenderConfig.Settings.AccessoryConfig;

            if (!config.RAtcChecked) return;
            for (var i = 0; i < config.SelectedSlot; i++)
            {
                AtcToolList.Add(new AtcTool
                {
                    ToolNumber = i + 1,
                    ToolName = $"Tool {i + 1}",
                });
            }
        }

        private void Tls()
        {
            GrblViewModel.ExecuteCommand("G65 P231");
        }

        private void Unload()
        {
            GrblViewModel.ExecuteCommand("G65 P221");
        }

        private void Load(int tool)
        {
            if (tool == null) return;
            GrblViewModel.ExecuteCommand($"M6T{tool}");
        }
    }
    public class AtcTool
    {
        public int ToolNumber { get; set; }
        public string ToolName { get; set; }

    }
}

