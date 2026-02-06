namespace GrblHalSender.GrblCore;

public class Resources
{
    public static string Path { get; set; }
    public static string Locale { get; set; }
    public static string IniName { get; set; }
    public static string IniFile { get { return Path + IniName; } }
    public static string DebugFile { get; set; } = string.Empty;
    public static string ConfigName { get; set; }
    public static bool IsLegacyController { get; set; } = false; // Set true if controller is legacy v1.1

    static Resources()
    {
        Path = @"./";
        Locale = "en-US";
        IniName = "GHallSender.config";
    }
}