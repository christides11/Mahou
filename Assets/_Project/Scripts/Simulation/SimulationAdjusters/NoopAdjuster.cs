namespace Mahou.Simulation
{
    public class NoopAdjuster : ISimulationAdjuster
    {
        public float AdjustedInterval { get; } = 1.0f;
    }
}