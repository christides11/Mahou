namespace Mahou.Simulation
{
    /// <summary>
    /// Does no adjusting of the simulation internal. 
    /// Used on the server only, as it does not have the need to play catch up.
    /// </summary>
    public class NoopAdjuster : ISimulationAdjuster
    {
        public float AdjustedInterval { get; } = 1.0f;
    }
}