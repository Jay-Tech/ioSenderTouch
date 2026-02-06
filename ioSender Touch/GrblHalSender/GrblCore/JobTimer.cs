using System.Diagnostics;

namespace GrblHalSender.GrblCore;

public static class JobTimer
{
    private static bool paused = false;
    private static Stopwatch stopWatch = new Stopwatch();

    public static bool IsRunning { get { return stopWatch.IsRunning || paused; } }

    public static bool IsPaused { get { return paused; } }

    public static bool Pause
    {
        get
        {
            return paused;
        }
        set
        {
            if (IsRunning)
            {
                if ((paused = value))
                    stopWatch.Stop();
                else
                    stopWatch.Start();
            }
        }
    }

    public static string RunTime =>
        IsRunning ? $"{stopWatch.Elapsed.Hours:00}:{stopWatch.Elapsed.Minutes:00}:{stopWatch.Elapsed.Seconds:00}"
            : "00:00:00";

    public static void Start()
    {
        paused = false;
        stopWatch.Reset();
        stopWatch.Start();
    }

    public static void Stop()
    {
        paused = false;
        stopWatch.Stop();
    }
}