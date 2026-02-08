using GrblHalSender.ViewModels;

namespace GrblHalSender.GrblCore;

public class PollGrbl
{
    System.Timers.Timer pollTimer = null;

    private byte _rtCommand = GrblConstants.CMD_STATUS_REPORT_ALL;

    internal static bool suspend = false;

    private GrblViewModel _grblViewModel;
    public  bool AutoPollEnabled => _grblViewModel.AutoReportEnabled;
    public PollGrbl(GrblViewModel grblViewModel)
    {
        _grblViewModel = grblViewModel;
    }


    internal static void Suspend()
    {
        suspend = true;
        if(Comms.com != null)
            Comms.com.PurgeQueue();
    }

    internal static void Resume()
    {
        suspend = false;
    }

    public void Run()
    {
            
        pollTimer = new System.Timers.Timer();
        pollTimer.Elapsed += pollTimer_Elapsed;
        //  this.pollTimer.SynchronizingObject = this;
    }

    public bool IsEnabled { get { return pollTimer.Enabled; } }

    public void SetState(int PollInterval)
    {
            
        if (PollInterval != 0)
        {
            suspend = false;
            if (pollTimer.Enabled)
                pollTimer.Stop();
            pollTimer.Interval = PollInterval;
            pollTimer.Start();
            _rtCommand = GrblConstants.CMD_STATUS_REPORT_ALL;
        }
        else
            pollTimer?.Stop();

           
    }

    void pollTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {

        if (!AutoPollEnabled)
        {
            if (!suspend)
            {
                Comms.com.WriteByte(_rtCommand);
            }
        }
        
            
        if (_rtCommand == GrblConstants.CMD_STATUS_REPORT_ALL)
        {
            _rtCommand = GrblLegacy.ConvertRTCommand(GrblConstants.CMD_STATUS_REPORT);
        }

    }
}