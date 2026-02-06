namespace GrblHalSender.GrblCore;

public class GrblConstants
{
    public const byte
        CMD_EXIT = 0x03, // ctrl-C
        CMD_RESET = 0x18, // ctrl-X
        CMD_STOP = 0x19, // ctrl-Y
        CMD_STATUS_REPORT = 0x80,
        CMD_CYCLE_START = 0x81,
        CMD_FEED_HOLD = 0x82,
        CMD_GCODE_REPORT = 0x83,
        CMD_SAFETY_DOOR = 0x84,
        CMD_JOG_CANCEL = 0x85,
        CMD_STATUS_REPORT_ALL = 0x87,
        CMD_OPTIONAL_STOP_TOGGLE = 0x88,
        CMD_SINGLE_BLOCK_TOGGLE = 0x89,
        CMD_OVERRIDE_FAN0_TOGGLE = 0x8A,
        CMD_MPG_MODE_TOGGLE = 0x8B,
        CMD_AUTO_REPORTING_TOGGLE = 0x8C,
        CMD_FEED_OVR_RESET = 0x90,
        CMD_FEED_OVR_COARSE_PLUS = 0x91,
        CMD_FEED_OVR_COARSE_MINUS = 0x92,
        CMD_FEED_OVR_FINE_PLUS = 0x93,
        CMD_FEED_OVR_FINE_MINUS = 0x94,
        CMD_RAPID_OVR_RESET = 0x95,
        CMD_RAPID_OVR_MEDIUM = 0x96,
        CMD_RAPID_OVR_LOW = 0x97,
        CMD_SPINDLE_OVR_RESET = 0x99,
        CMD_SPINDLE_OVR_COARSE_PLUS = 0x9A,
        CMD_SPINDLE_OVR_COARSE_MINUS = 0x9B,
        CMD_SPINDLE_OVR_FINE_PLUS = 0x9C,
        CMD_SPINDLE_OVR_FINE_MINUS = 0x9D,
        CMD_SPINDLE_OVR_STOP = 0x9E,
        CMD_COOLANT_FLOOD_OVR_TOGGLE = 0xA0,
        CMD_COOLANT_MIST_OVR_TOGGLE = 0xA1,
        CMD_PID_REPORT = 0xA2,
        CMD_TOOL_ACK = 0xA3,
        CMD_PROBE_CONNECTED_TOGGLE = 0xA4;

    public const string
        CMD_STATUS_REPORT_LEGACY = "?",
        CMD_CYCLE_START_LEGACY = "~",
        CMD_FEED_HOLD_LEGACY = "!",
        CMD_UNLOCK = "$X",
        CMD_HOMING = "$H",
        CMD_CHECK = "$C",
        CMD_GETSETTINGS = "$$",
        CMD_GETSETTINGS_ALL = "$+",
        CMD_GETPARSERSTATE = "$G",
        CMD_GETINFO = "$I",
        CMD_GETINFO_EXTENDED = "$I+",
        CMD_GETNGCPARAMETERS = "$#",
        CMD_GETSTARTUPLINES = "$N",
        CMD_GETSETTINGSDETAILS = "$ES",
        CMD_GETSETTINGSGROUPS = "$EG",
        CMD_GETALARMCODES = "$EA",
        CMD_GETERRORCODES = "$EE",
        CMD_PROGRAM_DEMARCATION = "%",
        CMD_SDCARD_MOUNT = "$FM",
        CMD_SDCARD_DIR = "$F",
        CMD_SDCARD_DIR_ALL = "$F+",
        CMD_SDCARD_REWIND = "$FR",
        CMD_SDCARD_RUN = "$F=",
        CMD_SDCARD_UNLINK = "$FD=",
        CMD_SDCARD_DUMP = "$F<=",
        FORMAT_METRIC = "###0.000",
        FORMAT_IMPERIAL = "##0.0000",
        NO_TOOL = "None",
        THCSIGNALS = "AERTOVHDU"; // Keep in sync with THCSignals enum below!!

    public const int
        X_AXIS = 0,
        Y_AXIS = 1,
        Z_AXIS = 2,
        A_AXIS = 3,
        B_AXIS = 4,
        C_AXIS = 5;
}