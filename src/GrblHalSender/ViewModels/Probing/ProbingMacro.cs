namespace GrblHalSender.ViewModels.Probing;

[Serializable]
public class ProbingMacro
{
    
    public ProbingMacro()
    {

    }
    public ProbingMacro(string name, string preCommand, string postCommand, bool isChecked, int id = -1)
    {
        Id = id;
        Name = name;
        PreCommand = preCommand;
        PostCommand = postCommand;
        RunOnce = isChecked;
    }

    public int Id { get; set; }
    public string Name { get; set; }

    public string PreCommand { get; set; }

    public string PostCommand { get; set; }
    public bool RunOnce { get; set; }
}