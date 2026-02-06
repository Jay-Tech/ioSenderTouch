namespace GrblHalSender.GrblCore;

[Flags]
public enum THCSignals : int
{
    Off = 0,
    ArcOk = 1 << 0,
    THCEnabled = 1 << 1,
    THCActive = 1 << 2,
    TorchOn = 1 << 3,
    OhmicProbe = 1 << 4,
    VelocityLock = 1 << 5,
    VoidLock = 1 << 6,
    Down = 1 << 7,
    Up = 1 << 8
}