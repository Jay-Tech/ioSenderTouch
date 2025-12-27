namespace GrblHalSender.ViewModels;

public class ToolData
{
    public string Name { get; set; }
    public string Index { get; set; }
    public double PosX { get; set; }
    public double PosY { get; set; }
    public double PosZ { get; set; }
    public ToolData(string name, string index, double posX, double posY, double posZ)
    {
        Name = name;
        Index = index;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
    }
}