namespace GrblHalSender.GrblCore;

public static class GrblCommand
{
    public static string Mist { get; set; } = ((char)GrblConstants.CMD_COOLANT_MIST_OVR_TOGGLE).ToString();
    public static string Flood { get; set; } = ((char)GrblConstants.CMD_COOLANT_FLOOD_OVR_TOGGLE).ToString();
    public static string Fan { get; set; } = ((char)GrblConstants.CMD_OVERRIDE_FAN0_TOGGLE).ToString();
    public static string ToolChange { get; set; } = "T{0}";
}