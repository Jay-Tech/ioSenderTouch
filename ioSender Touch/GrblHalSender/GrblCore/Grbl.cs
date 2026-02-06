/*
 * Grbl.cs - part of CNC Controls library
 *
 * v0.43 / 2023-07-05 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2018-2023, Io Engineering (Terje Io)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

· Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

· Redistributions in binary form must reproduce the above copyright notice, this
list of conditions and the following disclaimer in the documentation and/or
other materials provided with the distribution.

· Neither the name of the copyright holder nor the names of its contributors may
be used to endorse or promote products derived from this software without
specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

//#define USE_ASYNC

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
