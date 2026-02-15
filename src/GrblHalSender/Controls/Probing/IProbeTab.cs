

namespace GrblHalSender.Controls.Probing
{
    public enum ProbingType
    {
        None = 0,
        ToolLength,
        EdgeFinderInternal,
        EdgeFinderExternal,
        CenterFinder,
        Rotation,
        HeightMap
    }

    public interface IProbeTab
    {
        ProbingType ProbingType { get; }
        void Activate(bool activate);
        void Start(bool preview = false);
        void Stop();
    }
}
