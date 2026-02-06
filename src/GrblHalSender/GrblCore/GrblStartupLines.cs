using GrblHalSender.ViewModels;

namespace GrblHalSender.GrblCore;

public static class GrblStartupLines
{
    public static List<string> Lines { get; private set; } = new List<string>();

    public static bool Get()
    {
        return Grbl.GrblViewModel != null && Get(Grbl.GrblViewModel);
    }

    public static bool Get(GrblViewModel model)
    {
        bool? res = null;

        Lines.Clear();

        PollGrbl.Suspend();
        CancellationToken cancellationToken = new CancellationToken();

        new Thread(() =>
        {
            res = WaitFor.AckResponse<string>(
                cancellationToken,
                response => Process(response),
                a => model.OnResponseReceived += a,
                a => model.OnResponseReceived -= a,
                400, () => Comms.com.WriteCommand(GrblConstants.CMD_GETSTARTUPLINES));
        }).Start();

        while (res == null)
            EventUtils.DoEvents();

        PollGrbl.Resume();

        return Lines.Count > 0;
    }

    private static void Process(string data)
    {
        if (data.StartsWith(GrblConstants.CMD_GETSTARTUPLINES))
        {
            string[] valuepair = data.Split('=');
            if (valuepair.Length == 2)
            {
                Lines.Add(valuepair[1]);
            }
        }
    }
}