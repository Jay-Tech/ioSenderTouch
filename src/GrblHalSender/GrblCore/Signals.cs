namespace GrblHalSender.GrblCore;

[Flags]
public enum Signals : int // Keep in sync with GrblInfo.SignalLetters constant below
{
    Off = 0,
    LimitX = 1 << 0,
    LimitY = 1 << 1,
    LimitZ = 1 << 2,
    LimitA = 1 << 3,
    LimitB = 1 << 4,
    LimitC = 1 << 5,
    LimitU = 1 << 6,
    LimitV = 1 << 7,
    LimitW = 1 << 8,
    EStop = 1 << 9,
    Probe = 1 << 10,
    Reset = 1 << 11,
    SafetyDoor = 1 << 12,
    Hold = 1 << 13,
    CycleStart = 1 << 14,
    BlockDelete = 1 << 15,
    OptionalStop = 1 << 16,
    ProbeDisconnected = 1 << 17,
    MotorWarning = 1 << 18,
    MotorFault = 1 << 19
}