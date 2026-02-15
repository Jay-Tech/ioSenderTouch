
using System.IO;

namespace GrblHalSender.GrblCore
{
    public class YModem
    {
        public delegate void DataTransferredHandler(long size, long transferred);

        private const byte SOH = 0x01, STX = 0x02, EOT = 0x04, ACK = 0x06, NAK = 0x15, CAN = 0x18, C = (byte)'C';

        private int packetNum, bytes;
        private byte[] hdr = new byte[3], payload = new byte[1024], crc = new byte[2];
        private int response;

        public event DataTransferredHandler DataTransferred;

        private enum TransferState {
            ACK,
            NAK,
            CAN
        };

        public bool Upload (string path)
        {
            TransferState state = TransferState.NAK;
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            long bytesRemaining = fileStream.Length;

            Comms.com.EventMode = false;
            Comms.com.PurgeQueue();

            ClearPayload();

            if (TransferInitalPacket(path, fileStream) == TransferState.ACK)
            {
                do
                {
                    packetNum++;
                    if (bytesRemaining < 1024)
                        ClearPayload();
                    bytes = fileStream.Read(payload, 0, 1024);
                    bytesRemaining -= bytes;
                    DataTransferred?.Invoke(fileStream.Length, fileStream.Length - bytesRemaining);
                    state = TransferPacket(bytes <= 128 ? 128 : 1024);
                } while (bytesRemaining > 0 && state == TransferState.ACK);

                if(state == TransferState.ACK)
                {
                    hdr[0] = EOT;
                    Comms.com.WriteBytes(hdr, 1);
                }
            }

            Thread.Sleep(100);

            Comms.com.PurgeQueue();
            Comms.com.EventMode = true;

            return state == TransferState.ACK;
        }

        private TransferState TransferInitalPacket (string path, FileStream fileStream)
        {
            int i, j = 0;
            char[] fileName = Path.GetFileName(path).ToCharArray(), fileSize = fileStream.Length.ToString().ToCharArray();

            for (i = 0; i < fileName.Length; i++)
                payload[j++] = (byte)fileName[i];

            j++;

            for (i = 0; i < fileSize.Length; i++)
                payload[j++] = (byte)fileSize[i];

            packetNum = 0;

            return TransferPacket(128);
        }

        private TransferState TransferPacket(int length)
        {
            TransferState state;
            uint errors = 0, crc16 = CRC16.Calculate(payload, length);

            hdr[0] = length == 128 ? SOH : STX;
            hdr[1] = (byte)(packetNum & 0xFF);
            hdr[2] = (byte)(hdr[1] ^ 0xFF);
            crc[0] = (byte)((crc16 >> 8) & 0xFF);
            crc[1] = (byte)(crc16 & 0xFF);

            do
            {
                state = Send(length);
                if (state == TransferState.NAK)
                    errors++;
            } while (state == TransferState.NAK && errors < 10);

            return errors < 10 ? state : TransferState.CAN;
        }

        private void GetByte (int c)
        {
            response = c;
        }

        private TransferState Send(int length)
        {
            TransferState state = TransferState.ACK;
            bool? wait = null;
            CancellationToken cancellationToken = new CancellationToken();

            Comms.com.PurgeQueue();
            Comms.com.WriteBytes(hdr, 3);
            Comms.com.WriteBytes(payload, length);

            response = NAK;

            new Thread(() =>
            {
            wait = WaitFor.SingleEvent<int>(
                cancellationToken,
                s => GetByte(s),
                a => Comms.com.ByteReceived += a,
                a => Comms.com.ByteReceived -= a,
                packetNum == 0 ? 8000 : 2000, () => Comms.com.WriteBytes(crc, 2));
            }).Start();

            while (wait == null)
                EventUtils.DoEvents();

            switch (response)
            {
                case ACK:
                    state = TransferState.ACK;
                    break;

                case NAK:
                    state = TransferState.NAK;
                    break;

                case CAN:
                    state = TransferState.CAN;
                    break;
            }

            if(packetNum == 0) // Read 'C' from input
                Comms.com.ReadByte();

            return state;
        }

        private void ClearPayload ()
        {
            int i = payload.Length;
            do
            {
                payload[--i] = 0;
            } while (i > 0);
        }
    }

    class CRC16
    {
        public static uint Calculate(byte[] buf, int len)
        {
            uint x, i = 0, crc = 0;

            do
            {
                x = (crc >> 8) ^ buf[i++];
                x ^= x >> 4;
                crc = ((crc << 8) ^ (x << 12) ^ (x << 5) ^ x) & 0xFFFF;
                len--;
            } while (len > 0);

            return crc;
        }
    }
}
