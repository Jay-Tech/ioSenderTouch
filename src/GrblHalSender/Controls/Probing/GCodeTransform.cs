
using GrblHalSender.GrblCore;
using GrblHalSender.ViewModels.Probing;
using RP.Math;

namespace GrblHalSender.Controls.Probing
{
    class GCodeTransform
    {

        private ProbingViewModel model;

        public GCodeTransform(ProbingViewModel model)
        {
            this.model = model;
        }

        private Vector3 ToAbsolute(Vector3 orig, double[] values, bool isRelative = false)
        {
            Vector3 p;

            if (isRelative)
                p = orig + new Vector3(values[0], values[1], values[2]);
            else
                p = new Vector3(values[0], values[1], values[2]);

            return p;
        }
   }

    public static class V3Ex
    {
        public static Vector3 RollComponents(this Vector3 value, int turns)
        {
            double[] roll = new double[3];

            for (int i = 0; i < 3; i++)
            {
                roll[i] = value[(i - turns + 300) % 3];
            }

            return new Vector3(roll[0], roll[1], roll[2]);
        }
    }

    abstract class Motion
    {
        public Vector3 Start;
        public Vector3 End;
        public double Feed;

        public Vector3 Delta
        {
            get
            {
                return End - Start;
            }
        }

        /// <summary>
        /// Total travel distance of tool
        /// </summary>
        public abstract double Length { get; }

        /// <summary>
        /// get intermediate point along the path
        /// </summary>
        /// <param name="ratio">ratio between intermediate point and end</param>
        /// <returns>intermediate point</returns>
        public abstract Vector3 Interpolate(double ratio);

        /// <summary>
        /// Split motion into smaller fragments, still following the same path
        /// </summary>
        /// <param name="length">the maximum allowed length per returned segment</param>
        /// <returns>collection of smaller motions that together form this motion</returns>
        public abstract IEnumerable<Motion> Split(double length);
    }
    class Line : Motion
    {
        public bool Rapid = false;
        // PositionValid[i] is true if the corresponding coordinate of the end position was defined in the file.
        // eg. for a file with "G0 Z15" as the first line, X and Y would still be false
        public bool[] PositionValid = new bool[] { false, false, false };

        public Line()
        {
        }

        public Line(AxisFlags axisFlags)
        {
            PositionValid[0] = axisFlags.HasFlag(AxisFlags.X);
            PositionValid[1] = axisFlags.HasFlag(AxisFlags.Y);
            PositionValid[2] = axisFlags.HasFlag(AxisFlags.Z);
        }

        public override double Length
        {
            get
            {
                return Delta.Magnitude;
            }
        }

        public override Vector3 Interpolate(double ratio)
        {
            return Start + Delta * ratio;
        }

        public override IEnumerable<Motion> Split(double length)
        {
            if (Rapid /* || PositionValid.Any(isValid => !isValid)*/)  //don't split up rapid or not fully defined motions
            {
                yield return this;
                yield break;
            }

            int divisions = (int)Math.Ceiling(Length / length);

            if (divisions < 1)
                divisions = 1;

            Vector3 lastEnd = Start;

            for (int i = 1; i <= divisions; i++)
            {
                Vector3 end = Interpolate(((double)i) / divisions);

                Line immediate = new Line();
                immediate.Start = lastEnd;
                immediate.End = end;
                immediate.Feed = Feed;
                immediate.PositionValid = new bool[] { true, true, true };

                yield return immediate;

                lastEnd = end;
            }
        }
    }

    public enum ArcPlane
    {
        XY = 0,
        YZ = 1,
        ZX = 2
    }

    public enum ArcDirection
    {
        CW,
        CCW
    }

    class Arc : Motion
    {
        public ArcPlane Plane;
        public ArcDirection Direction;
        public double U;    //absolute position of center in first axis of plane
        public double V;    //absolute position of center in second axis of plane

        public override double Length
        {
            get
            {
                return Math.Abs(AngleSpan * Radius);
            }
        }

        public double StartAngle
        {
            get
            {
                Vector3 StartInPlane = Start.RollComponents(-(int)Plane);
                double X = StartInPlane.X - U;
                double Y = StartInPlane.Y - V;
                return Math.Atan2(Y, X);
            }
        }

        public double EndAngle
        {
            get
            {
                Vector3 EndInPlane = End.RollComponents(-(int)Plane);
                double X = EndInPlane.X - U;
                double Y = EndInPlane.Y - V;
                return Math.Atan2(Y, X);
            }
        }

        public double AngleSpan
        {
            get
            {
                double span = EndAngle - StartAngle;

                if (Direction == ArcDirection.CW)
                {
                    if (span >= 0)
                        span -= 2 * Math.PI;
                }
                else
                {
                    if (span <= 0)
                        span += 2 * Math.PI;
                }

                return span;
            }
        }

        public double Radius
        {
            get // get average between both radii
            {
                Vector3 startplane = Start.RollComponents(-(int)Plane);
                Vector3 endplane = End.RollComponents(-(int)Plane);

                return (
                    Math.Sqrt(Math.Pow(startplane.X - U, 2) + Math.Pow(startplane.Y - V, 2)) +
                    Math.Sqrt(Math.Pow(endplane.X - U, 2) + Math.Pow(endplane.Y - V, 2))
                    ) / 2;
            }
        }

        public override Vector3 Interpolate(double ratio)
        {
            double angle = StartAngle + AngleSpan * ratio;

            Vector3 onPlane = new Vector3(U + (Radius * Math.Cos(angle)), V + (Radius * Math.Sin(angle)), 0);

            double helix = (Start + (ratio * Delta)).RollComponents(-(int)Plane).Z;

   //         onPlane.Z = helix;

   //         Vector3 interpolation = onPlane.RollComponents((int)Plane);
            Vector3 interpolation = new Vector3(onPlane.X, onPlane.Y, helix).RollComponents((int)Plane);

            return interpolation;
        }

        public override IEnumerable<Motion> Split(double length)
        {
            int divisions = (int)Math.Ceiling(Length / length);

            if (divisions < 1)
                divisions = 1;

            Vector3 lastEnd = Start;

            for (int i = 1; i <= divisions; i++)
            {
                Vector3 end = Interpolate(((double)i) / divisions);

                Arc immediate = new Arc();
                immediate.Start = lastEnd;
                immediate.End = end;
                immediate.Feed = Feed;
                immediate.Direction = Direction;
                immediate.Plane = Plane;
                immediate.U = U;
                immediate.V = V;

                yield return immediate;

                lastEnd = end;
            }
        }
    }
}
