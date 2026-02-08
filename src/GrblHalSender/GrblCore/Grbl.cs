

using GrblHalSender.ViewModels;

namespace GrblHalSender.GrblCore
{
    public static class Grbl
    {
        public static void Reset()
        {
            Comms.com.WriteByte((byte)GrblConstants.CMD_RESET);
            System.Threading.Thread.Sleep(20);
        }

        public static GrblViewModel GrblViewModel { get; set; } = null;

        public static bool WaitForResponse(string command)
        {
            bool? res = null;
            CancellationToken cancellationToken = new CancellationToken();

            if (GrblViewModel == null)
                return false;

            if (GrblViewModel.ResponseLogVerbose)
                GrblViewModel.ResponseLog.Add(command);

            var t = new Thread(() =>
            {
                res = WaitFor.AckResponse<string>(
                cancellationToken,
                null,
                a => GrblViewModel.OnResponseReceived += a,
                a => GrblViewModel.OnResponseReceived -= a,
                5000, () => GrblViewModel.ExecuteCommand(command));
            }); t.Start();

            while (res == null)
                EventUtils.DoEvents();

            return res == true;
        }

        public static bool WaitForIdle(string command)
        {
            bool? res = null;
            CancellationToken cancellationToken = new CancellationToken();

            if (GrblViewModel == null)
                return false;

            if (GrblViewModel.ResponseLogVerbose)
                GrblViewModel.ResponseLog.Add(command);

            new Thread(() =>
            {
                res = WaitFor.AckResponse<string>(
                cancellationToken,
                null,
                a => GrblViewModel.OnResponseReceived += a,
                a => GrblViewModel.OnResponseReceived -= a,
                1000, () => GrblViewModel.ExecuteCommand(command));
            }).Start();

            while (res == null)
                EventUtils.DoEvents();

            if (res == true)
                res = null;

            while (res == null)
            {
                new Thread(() =>
                {
                    res = WaitFor.SingleEvent<string>(
                    cancellationToken,
                    null,
                    a => GrblViewModel.OnResponseReceived += a,
                    a => GrblViewModel.OnResponseReceived -= a,
                    5000);
                }).Start();

                while (res == null)
                    EventUtils.DoEvents();

                if (GrblViewModel.GrblState.State != GrblStates.Idle)
                    res = null;
            }

            return res == true;
        }

        public static bool WaitForWcoUpdate()
        {
            bool? res = null;
            CancellationToken cancellationToken = new CancellationToken();

            if (GrblViewModel == null)
                return false;

            // Wait for WCO update to get current work offsets

            if (GrblViewModel.Poller.IsEnabled)
            {
                new Thread(() =>
                {
                    res = WaitFor.SingleEvent<string>(
                    cancellationToken,
                    null,
                    a => GrblViewModel.OnWCOUpdated += a,
                    a => GrblViewModel.OnWCOUpdated -= a,
                    5000);
                }).Start();
            }

            while (res == null)
                EventUtils.DoEvents();

            return res == true;
        }
    }
}
