
using System.Runtime.InteropServices;


namespace GrblHalSender
{
    public class DisplayApi
    {
        [DllImport("user32.dll")]
        internal static extern DISP_CHANGE ChangeDisplaySettingsEx
        (string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd,
            DisplaySettingsFlags dwflags, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern bool EnumDisplayDevices(string lpDevice,
            uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice,
            uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        internal static extern int EnumDisplaySettings
            (string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        public const int DMDO_DEFAULT = 0;
        public const int DMDO_90 = 1;
        public const int DMDO_180 = 2;
        public const int DMDO_270 = 3;

        public const int ENUM_CURRENT_SETTINGS = -1;


        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        internal struct DEVMODE
        {
            public const int CCHDEVICENAME = 32;
            public const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
                CCHDEVICENAME)]
            [FieldOffset(0)]
            public string dmDeviceName;

            [FieldOffset(32)] public Int16 dmSpecVersion;
            [FieldOffset(34)] public Int16 dmDriverVersion;
            [FieldOffset(36)] public Int16 dmSize;
            [FieldOffset(38)] public Int16 dmDriverExtra;
            [FieldOffset(40)] public DM dmFields;

            [FieldOffset(44)] Int16 dmOrientation;
            [FieldOffset(46)] Int16 dmPaperSize;
            [FieldOffset(48)] Int16 dmPaperLength;
            [FieldOffset(50)] Int16 dmPaperWidth;
            [FieldOffset(52)] Int16 dmScale;
            [FieldOffset(54)] Int16 dmCopies;
            [FieldOffset(56)] Int16 dmDefaultSource;
            [FieldOffset(58)] Int16 dmPrintQuality;

            [FieldOffset(44)] public POINTL dmPosition;
            [FieldOffset(52)] public Int32 dmDisplayOrientation;
            [FieldOffset(56)] public Int32 dmDisplayFixedOutput;

            [FieldOffset(60)] public short dmColor;
            [FieldOffset(62)] public short dmDuplex;
            [FieldOffset(64)] public short dmYResolution;
            [FieldOffset(66)] public short dmTTOption;
            [FieldOffset(68)] public short dmCollate;

            [FieldOffset(72)] [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;

            [FieldOffset(102)] public Int16 dmLogPixels;
            [FieldOffset(104)] public Int32 dmBitsPerPel;
            [FieldOffset(108)] public Int32 dmPelsWidth;
            [FieldOffset(112)] public Int32 dmPelsHeight;
            [FieldOffset(116)] public Int32 dmDisplayFlags;
            [FieldOffset(116)] public Int32 dmNup;
            [FieldOffset(120)] public Int32 dmDisplayFrequency;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)] public int cb;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;

            [MarshalAs(UnmanagedType.U4)] public DisplayDeviceStateFlags StateFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINTL
        {
            long x;
            long y;
        }

        internal enum DISP_CHANGE : int
        {
            Successful = 0,
            Restart = 1,
            Failed = -1,
            BadMode = -2,
            NotUpdated = -3,
            BadFlags = -4,
            BadParam = -5,
            BadDualView = -6
        }

        [Flags()]
        internal enum DisplayDeviceStateFlags : int
        {
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            PrimaryDevice = 0x4,
            MirroringDriver = 0x8,
            VGACompatible = 0x10,
            Removable = 0x20,
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [Flags()]
        internal enum DisplaySettingsFlags : int
        {
            CDS_NONE = 0,
            CDS_UPDATEREGISTRY = 0x00000001,
            CDS_TEST = 0x00000002,
            CDS_FULLSCREEN = 0x00000004,
            CDS_GLOBAL = 0x00000008,
            CDS_SET_PRIMARY = 0x00000010,
            CDS_VIDEOPARAMETERS = 0x00000020,
            CDS_ENABLE_UNSAFE_MODES = 0x00000100,
            CDS_DISABLE_UNSAFE_MODES = 0x00000200,
            CDS_RESET = 0x40000000,
            CDS_RESET_EX = 0x20000000,
            CDS_NORESET = 0x10000000
        }

        [Flags()]
        internal enum DM : int
        {
            Orientation = 0x00000001,
            PaperSize = 0x00000002,
            PaperLength = 0x00000004,
            PaperWidth = 0x00000008,
            Scale = 0x00000010,
            Position = 0x00000020,
            NUP = 0x00000040,
            DisplayOrientation = 0x00000080,
            Copies = 0x00000100,
            DefaultSource = 0x00000200,
            PrintQuality = 0x00000400,
            Color = 0x00000800,
            Duplex = 0x00001000,
            YResolution = 0x00002000,
            TTOption = 0x00004000,
            Collate = 0x00008000,
            FormName = 0x00010000,
            LogPixels = 0x00020000,
            BitsPerPixel = 0x00040000,
            PelsWidth = 0x00080000,
            PelsHeight = 0x00100000,
            DisplayFlags = 0x00200000,
            DisplayFrequency = 0x00400000,
            ICMMethod = 0x00800000,
            ICMIntent = 0x01000000,
            MediaType = 0x02000000,
            DitherType = 0x04000000,
            PanningWidth = 0x08000000,
            PanningHeight = 0x10000000,
            DisplayFixedOutput = 0x20000000
        }
    }

    public static class clsDisplaySettings
    {
        public enum Orientation
        {
            DEGREES_CW_270 = 1,
            DEGREES_CW_180 = 2,
            DEGREES_CW_90 = 3,
            DEGREES_CW_0 = 0,
            Unknown = -1
        }

        public static bool Rotate(uint uDisplay,
            Orientation oOrientation)
        {

            bool bResult = false;
            DisplayApi.DISPLAY_DEVICE dDevice = new DisplayApi.DISPLAY_DEVICE();
            DisplayApi.DEVMODE dmMode = new DisplayApi.DEVMODE();

            dDevice.cb = Marshal.SizeOf(dDevice);

            if (!DisplayApi.EnumDisplayDevices(null, uDisplay - 1,
                    ref dDevice, 0))
                return false;

            if (0 != DisplayApi.EnumDisplaySettings(
                    dDevice.DeviceName, DisplayApi.ENUM_CURRENT_SETTINGS,
                    ref dmMode))
            {

                if ((dmMode.dmDisplayOrientation + (int)oOrientation)
                    % 2 == 1)
                {

                    int tmp = dmMode.dmPelsHeight;

                    dmMode.dmPelsHeight = dmMode.dmPelsWidth;
                    dmMode.dmPelsWidth = tmp;

                }

                switch (oOrientation)
                {

                    case Orientation.DEGREES_CW_90:

                        dmMode.dmDisplayOrientation = DisplayApi.DMDO_270;
                        break;

                    case Orientation.DEGREES_CW_180:

                        dmMode.dmDisplayOrientation = DisplayApi.DMDO_180;
                        break;

                    case Orientation.DEGREES_CW_270:

                        dmMode.dmDisplayOrientation = DisplayApi.DMDO_90;
                        break;

                    case Orientation.DEGREES_CW_0:

                        dmMode.dmDisplayOrientation = DisplayApi.DMDO_DEFAULT;
                        break;

                    default:
                        break;

                }

                DisplayApi.DISP_CHANGE ret = DisplayApi.ChangeDisplaySettingsEx
                (dDevice.DeviceName, ref dmMode, IntPtr.Zero,
                    DisplayApi.DisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);

                bResult = ret == 0;

            }

            return bResult;
        }

        public static Orientation GetScreenOrientation(uint uDisplay)
        {
            bool bResult = false;
            DisplayApi.DISPLAY_DEVICE dDevice = new DisplayApi.DISPLAY_DEVICE();
            DisplayApi.DEVMODE dmMode = new DisplayApi.DEVMODE();

            dDevice.cb = Marshal.SizeOf(dDevice);

            if (!DisplayApi.EnumDisplayDevices(null, uDisplay - 1,
                    ref dDevice, 0))
                return Orientation.Unknown;

            if (0 != DisplayApi.EnumDisplaySettings(dDevice.DeviceName, DisplayApi.ENUM_CURRENT_SETTINGS, ref dmMode))
            {
                return dmMode.dmDisplayOrientation switch
                {
                    0 => Orientation.DEGREES_CW_0,
                    1 => Orientation.DEGREES_CW_90,
                    2 => Orientation.DEGREES_CW_180,
                    3 => Orientation.DEGREES_CW_270,
                    _ => Orientation.Unknown
                };
            }

            return Orientation.Unknown;
        }


        public static void Reset()
        {
            try
            {
                uint i = 0;

                while (++i <= 64)
                {
                    Rotate(i, Orientation.DEGREES_CW_0);
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {

            }

        }
    }
}