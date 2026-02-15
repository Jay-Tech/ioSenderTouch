
using System.Data;
using System.IO;
using System.Windows;
using GrblHalSender.GrblCore;
using GrblHalSender.ViewModels;
using Microsoft.Win32;
using Action = GrblHalSender.GrblCore.Action;

namespace GrblHalSender.Controls;

public class GCode
{

    public event EventHandler<bool> FileLoaded;
    public event GCodeJob.ToolChangedHandler ToolChanged = null;
    
    public const string FileTypes = "nc,ngc,gcode,macro";

    private GCodeJob Program { get; set; } = new GCodeJob();
    private static readonly Lazy<GCode> file = new(() => new GCode());
    
    public static GCode File => file.Value;
    public bool IsLoaded => Program.Loaded;
    public string FileName => Model == null ? string.Empty : Model.FileName;
    public int ToolChanges => Program.Parser.ToolChanges;
    public bool HasGoPredefinedPosition => Program.Parser.HasGoPredefinedPosition;
    public int Decimals => Program.Parser.Decimals;
    public DataTable Data => Program.Data;
    public int Blocks => Program.Data.Rows.Count;
    public List<GCodeToken> Tokens => Program.Tokens;
    public Queue<string> Commands => Program.commands;
    public GCodeParser Parser => Program.Parser;
    public GrblViewModel Model { get; set; }

    private GCode()
    {
        Program.FileChanged += Program_FileChanged;
        Program.ToolChanged += Program_ToolChanged;
    }

    private bool Program_ToolChanged(int toolNumber)
    {
        return ToolChanged?.Invoke(toolNumber) ?? true;
    }

    private void Program_FileChanged(string filename)
    {
        if (Model != null)
        {
            if (filename == "")
                Model.ProgramLimits.Clear();
            else foreach (int i in AxisFlags.All.ToIndices())
            {
                Model.ProgramLimits.MinValues[i] = Model.ConvertMM2Current(Program.BoundingBox.Min[i]);
                Model.ProgramLimits.MaxValues[i] = Model.ConvertMM2Current(Program.BoundingBox.Max[i]);
            }

            Model.FileName = filename;
        }
    }

    public void AddBlock(string block, Action action)
    {
        Program.AddBlock(block, action);

        if (action == Action.End)
            Model.Blocks = Blocks;
    }

    public void AddBlock(string block)
    {
        Program.AddBlock(block);
    }

    public void ClearStatus()
    {
        foreach (DataRow row in Program.Data.Rows)
            if ((string)row["Sent"] != string.Empty)
            {
                row["Sent"] = string.Empty;
            }
    }

    public void Drag(object sender, DragEventArgs e)
    {
        bool allow = Model != null && GrblParserState.IsLoaded && (Model.StreamingState == StreamingState.Idle || Model.StreamingState == StreamingState.NoFile);

        if (allow && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            allow = files.Count() == 1 && FileUtils.IsAllowedFile(files[0].ToLower(), FileTypes  + ",txt");
        }

        e.Handled = true;
        e.Effects = allow ? DragDropEffects.Copy : DragDropEffects.None;
    }

    public void Drop(object sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

        if (files.Count() == 1)
        {
            Load(files[0]);
        }
    }

    public void Close()
    {
        Program.CloseFile();
        Model.Blocks = Blocks;
        FileLoaded?.Invoke(this, false);
    }

    public void Open()
    {
        string filename = string.Empty;
        OpenFileDialog file = new OpenFileDialog();

        string conversionFilter = string.Empty; //conversionTypes == string.Empty ? string.Empty : string.Format("Other files ({0})|{0}|", FileUtils.ExtensionsToFilter(conversionTypes));
        file.Filter = string.Format("GCode files ({0})|{0}|{1}Text files (*.txt)|*.txt|All files (*.*)|*.*", FileUtils.ExtensionsToFilter(FileTypes), conversionFilter);

        if (file.ShowDialog() == true)
        {
            filename = file.FileName;
        }

        if (filename != string.Empty)
            Load(filename);

        Model.Blocks = Blocks;
    }

    public void Load(string filename)
    {

        using (new UIUtils.WaitCursor())
        {

            if (Program.LoadFile(filename))
            {
                FileLoaded?.Invoke(this, true);
            }
        }

        Model.Blocks = Blocks;
    }

    public void SdCardFileLoaded()
    {
        //FileLoaded?.Invoke(this, true);
    }

    public void Save()
    {
        SaveFileDialog saveDialog = new SaveFileDialog()
        {
            Filter = "GCode file (*.nc)|*.nc",
            AddExtension = true,
            DefaultExt = ".nc",
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                //using (new UIUtils.WaitCursor())
                //{
                //    GCodeParser.Save(saveDialog.FileName, GCodeParser.TokensToGCode(File.Tokens));
                //}

                using (StreamWriter stream = new StreamWriter(saveDialog.FileName))
                {
                    using (new UIUtils.WaitCursor())
                    {
                        foreach (DataRow line in Program.Data.Rows)
                            stream.WriteLine((string)line["Data"]);
                    }
                }
            }
            catch (IOException)
            {
            }

            Model.FileName = saveDialog.FileName;
        }
    }
}

public class FileLoadArgs(bool isSdCard, string fileName, bool isOpen) : EventArgs
{
    public  bool IsSdCard { get; set; } = isSdCard;
    public string FileName { get; set; } = fileName;
    public bool IsOpen { get; set; } = isOpen;
}