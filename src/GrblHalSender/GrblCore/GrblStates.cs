namespace GrblHalSender.GrblCore;

public enum GrblStates
{
    Unknown = 0,
    Idle,
    Run,
    Tool,
    Hold,
    Home,
    Check,
    Jog,
    Alarm,
    Door,
    Sleep
}