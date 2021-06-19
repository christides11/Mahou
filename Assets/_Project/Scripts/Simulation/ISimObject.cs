namespace Mahou.Simulation
{
    public interface ISimObject
    {
        public bool ObjectEnabled { get; }
        public void Enable();
        public void Disable();
        public void SimUpdate();
        public void SimLateUpdate();
        public ISimState GetSimState();
        public void ApplySimState(ISimState state);
    }
}