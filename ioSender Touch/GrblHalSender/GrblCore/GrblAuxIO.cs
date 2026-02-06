namespace GrblHalSender.GrblCore;

public static class GrblAuxIO
{
    public static int DigitalInputs { get; internal set; }
    public static int DigitalOutputs { get; internal set; }
    public static int AnalogInputs { get; internal set; }
    public static int AnalogOutputs { get; internal set; }

    public static bool IsEnabled { get { return DigitalInputs + DigitalOutputs + AnalogInputs + AnalogOutputs > 0; } }

    internal static void ParseConfig(string config)
    {
        var values = config.Split(':')[1].TrimEnd(']').Split(',');
        if (values.Length == 4)
        {
            DigitalInputs = int.Parse(values[0]);
            DigitalOutputs = int.Parse(values[1]);
            AnalogInputs = int.Parse(values[2]);
            AnalogOutputs = int.Parse(values[3]);
        }
    }
}