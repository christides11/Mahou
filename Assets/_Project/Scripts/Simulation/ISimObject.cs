namespace Mahou.Simulation
{
    public interface ISimObject
    {
        public void SimUpdate();
        public void SimLateUpdate();
        public ISimState GetSimState();
        public void ApplySimState(ISimState state);
    }
}