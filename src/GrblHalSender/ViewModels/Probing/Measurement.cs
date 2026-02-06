using GrblHalSender.Controls.Probing;
using GrblHalSender.GrblCore;

namespace GrblHalSender.ViewModels.Probing;

public class Measurement : ViewModelBase
{
    public void Add(Position position, AxisFlags axisFlags, ProbingType probingType)
    {
        Position = position;
        AxisFlags = axisFlags;
        ProbingType = probingType;

        OnPropertyChanged();
    }

    public Position Position { get; private set; } = new Position();
    public ProbingType ProbingType { get; private set; } = ProbingType.None;
    public AxisFlags AxisFlags { get; private set; } = AxisFlags.None;
}