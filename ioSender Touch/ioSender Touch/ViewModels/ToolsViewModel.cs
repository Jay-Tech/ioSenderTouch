using ioSenderTouch.GrblCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ioSenderTouch.GrblCore.Comands;
using ioSenderTouch.Utility;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;

namespace ioSenderTouch.ViewModels
{
    // G10 L1 P- axes <R- I- J- Q-> Set Tool Table
    // L10 - ref G5x + G92 - useful for probe (G38)
    // L11 - ref g59.3 only
    // Q: 1 - 8: 1: 135, 2: 45, 3: 315, 4: 225, 5: 180, 6: 90, 7: 0, 8: 270

    public class ToolsViewModel : ViewModelBase, IActiveViewModel
    {
        private readonly GrblViewModel _grblVM;
        private ToolData _selectedTool;
        private ObservableCollection<ToolData> _toolCollection;

        public bool Active { get; set; }
        public string Name { get; set; }
        public ToolData Offset  { get; private set; } = new ToolData
        {
            X = 0.000,
            Y = 0.000,
            Z = 0.000
        };

        public ObservableCollection<ToolData> ToolCollection
        {
            get => _toolCollection;
            set
            {
                if (Equals(value, _toolCollection)) return;
                _toolCollection = value;
                OnPropertyChanged();
            }
        }

        public ICommand GetCurrPosCommand { get; }
        public ICommand SetOffsetCommand { get; }
        public ICommand SetAllCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand SaveToolOffsetsCommand { get; }
        public ICommand RestoreToolOffsetsCommand { get; }
        public ToolData SelectedTool
        {
            get => _selectedTool;
            set
            {
                if (Equals(value, _selectedTool)) return;
                _selectedTool = value;
                SelectedToolChanged();
                OnPropertyChanged();
            }
        }

        private void SelectedToolChanged()
        {
            if (SelectedTool == null) return;
            Offset.X = 0.000;
            Offset.Y = 0.000;
            Offset.Z = 0.000;
            Offset.ToolNumber = SelectedTool.ToolNumber;
        }

        public ToolsViewModel(GrblViewModel grblVM)
        {
            _grblVM = grblVM;
            Name = nameof(ToolsViewModel);
            GetCurrPosCommand = new Command(GetCurrPos);
            SetAllCommand = new Command(SetAll);
            ClearAllCommand = new Command(ClearAll);
            SetOffsetCommand = new Command(SetOffset);
            SaveToolOffsetsCommand = new Command(SaveOffsets);
            RestoreToolOffsetsCommand = new Command(RestoreOffsets);
            BuildToolCollection();
        }

        private void GetCurrPos(object obj)
        {
            var t = RequestExtension.SendSettings(_grblVM, GrblLegacy.ConvertRTCommand(GrblConstants.CMD_STATUS_REPORT), "Mpos", InfoReceived);
        }

        private void InfoReceived()
        {
            if (!double.IsNaN(_grblVM.MachinePosition.Values[0]))
            {
                Offset.Set(_grblVM.MachinePosition);
            }
        }
        private void BuildToolCollection()
        {
            var toolList = (from tool in _grblVM.Tools where tool.Code != "None" select new 
                ToolData { ToolNumber = int.Parse(tool.Code), X = tool.X, Y = tool.Y, Z = tool.Z }).ToList();
            ToolCollection = new ObservableCollection<ToolData>(toolList.OrderBy(t=>t.ToolNumber));
            var setUnit = _grblVM.IsMetric ? "G21" : "G20";

        }

       private void SaveOffset(string axis)
        {
            string axes;
            var pos = new Position(Offset);

            switch (axis)
            {
                
                case "All":
                    axes = pos.ToString(GrblInfo.AxisFlags);
                    break;

                default:
                    axes = pos.ToString(GrblInfo.AxisLetterToFlag(axis));
                    break;
            }

            Comms.com.WriteCommand($"G10L1P{SelectedTool.ToolNumber}{axes}");
        }

        private void SetOffset(object x)
        {
            if (SelectedTool == null) return;
            var axisLetter = x.ToString();
            var axis = GrblInfo.AxisLetterToIndex(axisLetter);
            SelectedTool.Values[axis] = Offset.Values[axis];
            SaveOffset(axisLetter);
        }

        private void SetAll(object e)
        {
            if (SelectedTool != null)
            {
                for (var i = 0; i < Offset.Values.Length; i++)
                    SelectedTool.Values[i] = Offset.Values[i];
            }

            SaveOffset("All");
        }

        private void ClearAll(object  e)
        {
            if (SelectedTool != null)
            {
                for (var i = 0; i < Offset.Values.Length; i++)
                    Offset.Values[i]  = 0d;
                SelectedTool.X = 0.000;
                SelectedTool.Y = 0.000;
                SelectedTool.Z = 0.000;
                SaveOffset("All");
            }
        }

        private void SaveOffsets(object x)
        {
            List<ToolOffsets> settings = ToolCollection.Select(offset => new ToolOffsets(offset.ToolNumber, offset.X,
                offset.Y, offset.Z)).ToList(); 
            using StreamWriter file = File.CreateText(Path.Combine(Resources.Path, "ToolTableSettings.json"));
            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            serializer.Serialize(file, settings);
        }

        private void RestoreOffsets(object x)
        {
            if (!File.Exists(Path.Combine(Resources.Path, "ToolTableSettings.json"))) return;
            var setUnit = _grblVM.IsMetric ? "G21" : "G20";
            Comms.com.WriteCommand(setUnit);

            using StreamReader file = File.OpenText(Path.Combine(Resources.Path, "ToolTableSettings.json"));
            var json = file.ReadToEnd();
            var offsets = (List<ToolOffsets>)JsonConvert.DeserializeObject(json, typeof(List<ToolOffsets>));
            if (offsets == null) return;
            foreach (var command in from offset in offsets where offset.ToolNumber != 0 select $"G10L1P{offset.ToolNumber}X{offset.X}Y{offset.Y}Z{offset.Z}")
            {
                Comms.com.WriteCommand(command);
            }
            ToolCollection.Clear();
            var t = RequestExtension.SendSettings(_grblVM, GrblConstants.CMD_GETNGCPARAMETERS, "PRB:", ReBindCollection);
        }

        public void ReBindCollection()
        {
            BuildToolCollection();
        }
        public void Activated()
        {
            _grblVM.PropertyChanged += _grblVM_PropertyChanged;
        }

        private void _grblVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GrblViewModel.IsMetric))
            {
                var setUnit = _grblVM.IsMetric ? "G21" : "G20";
                Comms.com.WriteCommand(setUnit);
            }
        }

        public void Deactivated()
        {
            _grblVM.PropertyChanged -= _grblVM_PropertyChanged;
        }
    }
}

public class ToolData : Position
{
    public int ToolNumber { get; set; }
}

public class ToolOffsets
{
    public ToolOffsets(int toolNumber, double offsetX, double offsetY, double offsetZ)
    {
        ToolNumber = toolNumber;
        X = offsetX; 
        Y = offsetY; 
        Z = offsetZ;
    }

    public int ToolNumber { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}
