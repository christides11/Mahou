namespace Mahou.Simulation
{
    /// <summary>
    /// A snapshot of the state of an object(s) in the simulation.
    /// </summary>
    public class ISimState
    {
        public bool objectEnabled;


        public virtual System.Guid GetGUID()
        {
            return StaticGetGUID();
        }

        public static System.Guid StaticGetGUID()
        {
            return new System.Guid("84560d54-63db-4111-98be-2d77d1d9c58c");
        }
    }
}