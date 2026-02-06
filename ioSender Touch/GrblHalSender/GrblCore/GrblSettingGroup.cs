namespace GrblHalSender.GrblCore;

public class GrblSettingGroup
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string Name { get; set; }
    public IEnumerable<GrblSettingDetails> Settings
    {
        get { return GrblSettings.Settings.Where(x => x.GroupId == Id); }
    }

    public GrblSettingGroup(string data)
    {
        string[] values = data.Split('|');

        Id = int.Parse(values[0]);
        ParentId = int.Parse(values[1]);
        Name = values[2];
    }
}