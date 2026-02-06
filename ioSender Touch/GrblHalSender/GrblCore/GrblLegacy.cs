namespace GrblHalSender.GrblCore;

public static class GrblLegacy
{
    public static byte ConvertRTCommand(byte cmd)
    {
        if (GrblInfo.UseLegacyRTCommands)
            switch (cmd)
            {
                case GrblConstants.CMD_STATUS_REPORT_ALL:
                    cmd = (byte)GrblConstants.CMD_STATUS_REPORT_LEGACY[0];
                    break;

                case GrblConstants.CMD_STATUS_REPORT:
                    cmd = (byte)GrblConstants.CMD_STATUS_REPORT_LEGACY[0];
                    break;

                case GrblConstants.CMD_CYCLE_START:
                    cmd = (byte)GrblConstants.CMD_CYCLE_START_LEGACY[0];
                    break;

                case GrblConstants.CMD_FEED_HOLD:
                    cmd = (byte)GrblConstants.CMD_FEED_HOLD_LEGACY[0];
                    break;
            }

        return cmd;
    }
}