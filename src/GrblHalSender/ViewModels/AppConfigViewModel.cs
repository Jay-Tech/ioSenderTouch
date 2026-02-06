using System.Windows.Input;
using GrblHalSender.GrblCore;
using GrblHalSender.GrblCore.Comands;
using GrblHalSender.GrblCore.Config;

namespace GrblHalSender.ViewModels;

public class AppConfigViewModel : ViewModelBase, IActiveViewModel
{
    public bool Active { get; set; }
    public string Name { get; }

    public Config ConfigBase => GHalSenderConfig.Settings.Base;

    public ICommand SaveCommand { get; set; }

    public ICommand SaveKeyMapCommand { get; set; }
    public AppConfigViewModel()
    {
        Name = nameof(AppConfigViewModel);
        SaveCommand = new Command(Save);
        SaveKeyMapCommand = new Command(SaveKeyMap);
    }

    private void SaveKeyMap(object obj)
    {
        if (GHalSenderConfig.Settings.Save())
            Grbl.GrblViewModel.Message = "SettingsSaved";
    }

    private void Save(object obj)
    {
        string filename = Resources.Path + "KeyMap.xml";
        if (Grbl.GrblViewModel.Keyboard.SaveMappings(filename))
            Grbl.GrblViewModel.Message = $"Keymappings saved to {filename}";
    }

    public void Activated(){}
    
    public void Deactivated(){}
    
}