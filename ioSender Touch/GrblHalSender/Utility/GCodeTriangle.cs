namespace GrblHalSender.Utility
{
    public class GCodeTriangle
    {
        private const string AbsolutePos = "G91";
        private const string IncrementalPos = "G90";
        private const string TravelFeedRate = "G1";
        private const string RapidFeedRate = "G0";
        private const string X0 = "X0";
        private const string Y0 = "Y0";
        private const string Metric = "G21";

        private  double _lengthA;
        private  double _lengthB;
        private  double _feedRate;
        private  double _depth;
        private  double _safeZHeight;



        public GCodeTriangle()
        {
           
        }

        public List<string> CreateJob(double lengthA, double lengthB, double feedRate, double depth, double safeZHeight)
        {
            _lengthA = lengthA;
            _lengthB = lengthB;
            _feedRate = feedRate;
            _depth = depth;
            _safeZHeight = safeZHeight;

            var gCodeJob = new List<string>();
            gCodeJob.AddRange(BuildHeader());
            gCodeJob.AddRange(BuildBody());
            return gCodeJob;
        }
        private List<string> BuildHeader()
        {
            var header = new List<string>
            {
                ";Header",
                Metric,
                IncrementalPos,
                $"{RapidFeedRate}{X0}{Y0}Z{_safeZHeight:f4}",
                $"{TravelFeedRate}F{_feedRate}",
            };
            return header;
        }

        private List<string> BuildBody()
        {
            var zFeedRate = _feedRate / 2;
            var gCodeList = new List<string>
            {
                $"G1 Z{_safeZHeight:f4}f{zFeedRate:f4}",
                $"G1 Z-{_depth:f4}f{zFeedRate:f4}",
                $"G1 Z{_safeZHeight:f4}f{zFeedRate:f4}",
                $"G1 X{_lengthA:f4}f{_feedRate:f4}",
                $"G1 Z-{_depth:f4}f{zFeedRate:f4}",
                $"G1 Z{_safeZHeight:f4}f{zFeedRate:f4}",
                $"G1 Y{_lengthB:f4}f{_feedRate:f4}",
                $"G1 Z-{_depth:f4}f{zFeedRate:f4}",
                $"G1 Z{_safeZHeight:f4}f{zFeedRate:f4}"
            };
            return gCodeList;
        }
    }
}
