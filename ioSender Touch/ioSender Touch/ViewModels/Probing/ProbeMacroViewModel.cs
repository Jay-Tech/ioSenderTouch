using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ioSenderTouch.Controls.Probing;
using ioSenderTouch.GrblCore;
using ioSenderTouch.GrblCore.Comands;

namespace ioSenderTouch.ViewModels.Probing;

public class ProbeMacroViewModel : ViewModelBase
{
    private  int _nextId = 0;
    private bool _runOnce;
    private string _postMacroText;
    private string _preMacroText;
    private readonly ProbingMacros _probeMacros;
    private string[] NoCommands { get; } = [];
    private ProbingMacroDialog _dialog;
    private string _macroName;
    private ProbingMacro _macroSelectedItem;
    private bool _macroActive;
    private int _selectedItemIndex;

    public string PreMacroText
    {
        get => _preMacroText;
        set
        {
            if (value == _preMacroText) return;
            _preMacroText = value;
            OnPropertyChanged();
        }
    }
    public string PostMacroText
    {
        get => _postMacroText;
        set
        {
            if (value == _postMacroText) return;
            _postMacroText = value;
            OnPropertyChanged();
        }
    }
    public string MacroName
    {
        get => _macroName;
        set
        {
            if (value == _macroName) return;
            _macroName = value;
            OnPropertyChanged();
        }
    }

    public ProbingMacro MacroSelectedItem
    {
        get => _macroSelectedItem;
        set
        {
            if (Equals(value, _macroSelectedItem)) return;
            _macroSelectedItem = value;
            SelectionChanged();
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ProbingMacro> Macros { get; set; }

    public bool RunOnce
    {
        get { return _runOnce; }
        set
        {
            if (value == _runOnce) return;
            _runOnce = value;
            SingleUseCheckedChanged();
            OnPropertyChanged();
        }
    }

    public int SelectedItemIndex
    {
        get => _selectedItemIndex;
        set
        {
            if (value == _selectedItemIndex) return;
            _selectedItemIndex = value;
            OnPropertyChanged();
        }
    }


    public bool MacroActive
    {
        get => _macroActive;
        set
        {
            if (value == _macroActive) return;
            
            _macroActive = value;
            OnPropertyChanged();
        }
    }
    
    public ICommand OpenDialog { get; }
    public ICommand DeleteCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand CloseCommand { get; }
    public ProbeMacroViewModel()
    {
        _probeMacros = new ProbingMacros();
        _probeMacros.Load();
        Macros = _probeMacros.Macros;
        OpenDialog = new Command(OpenDialogHandler);
        DeleteCommand = new Command(DeleteCommandHandler);
        AddCommand = new Command(AddCommandHandler);
        ClearCommand = new Command(ClearActiveMacro);
        CloseCommand = new Command(Close);
    }

    private void ClearActiveMacro(object x)
    {
        RunOnce = false;
        PostMacroText = string.Empty;
        PreMacroText = string.Empty;
        MacroName = string.Empty;
        MacroSelectedItem = null;
    }

    private void SelectionChanged()
    {
        MacroActive = MacroSelectedItem != null;
        if (MacroSelectedItem == null) return;
        RunOnce = MacroSelectedItem.RunOnce;
        PostMacroText = MacroSelectedItem.PostCommand;
        PreMacroText = MacroSelectedItem.PreCommand;
        MacroName = MacroSelectedItem.Name;
    }
       
    public string[] PreJobCommands =>
        _macroSelectedItem == null ? NoCommands :
            _macroSelectedItem.PreCommand.Split(["\n", "\r\n"], StringSplitOptions.RemoveEmptyEntries);

    public string[] PostJobCommands =>
        _macroSelectedItem == null ? NoCommands :
            _macroSelectedItem.PostCommand.Split(["\n", "\r\n"], StringSplitOptions.RemoveEmptyEntries);

    private void SingleUseCheckedChanged()
    {
        if (MacroSelectedItem != null)
        {
            MacroSelectedItem.RunOnce = RunOnce;
        }
    }


    public void Clear()
    {
        MacroSelectedItem = null;
    }

    public void Close(object x)
    {
        _probeMacros.Save();
        _dialog.Close();
    }

    private void OpenDialogHandler(object x)
    {
        _dialog = new ProbingMacroDialog()
        {
            DataContext = this,
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (_macroSelectedItem == null)
        {
            if (Macros.Count > 0 && SelectedItemIndex >= 0)
            {
                MacroSelectedItem = Macros[SelectedItemIndex];
            }
        }
            

        _dialog.ShowDialog();
    }

    private void DeleteCommandHandler(object x)
    {
        if (MacroSelectedItem != null)
        {
            var found = Macros.First(n => n.Name == MacroSelectedItem.Name);
            if (found != null)
            {
                Macros.Remove(found);
                MacroName = string.Empty;
                _probeMacros.Save();
            }
             
           
            if (Macros.Count > 0 && SelectedItemIndex >=0)
            {
                MacroSelectedItem = Macros[SelectedItemIndex];
            }

        }
    }

    private void AddCommandHandler(object x)
    {
        var macroName = MacroName;

        if (string.IsNullOrEmpty(macroName))
            macroName = $"MC_{new Random().Next(0, 1000)}";
        if (Macros.All(n => n.Name != macroName))
        {
            Macros.Add(MacroSelectedItem = new ProbingMacro(macroName, PreMacroText, PostMacroText, RunOnce,_nextId++));
        }
        else
        {
            var macro = Macros.FirstOrDefault(n => n.Name == macroName);
            {
                if (macro != null)
                {
                    macro.RunOnce = RunOnce;
                    macro.PostCommand = PostMacroText;
                    macro.PreCommand = PreMacroText;
                }

            }
        }
        _probeMacros.Save();
    }
}