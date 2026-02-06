namespace GrblHalSender.GrblCore;

public enum StreamingState
{
    NoFile = 0,
    Idle,
    Send,
    SendMDI,
    Home,
    Halted,
    FeedHold,
    ToolChange,
    Start,
    Stop,
    Paused,
    JobFinished,
    Reset,
    AwaitResetAck,
    Disabled,
    Error
}