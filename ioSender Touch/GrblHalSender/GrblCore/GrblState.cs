using System.Windows.Media;

namespace GrblHalSender.GrblCore;

public struct GrblState
{
    public GrblStates State;
    public int Substate;
    public int LastAlarm;
    public int Error;
    public Color Color;
    public bool MPG;
}