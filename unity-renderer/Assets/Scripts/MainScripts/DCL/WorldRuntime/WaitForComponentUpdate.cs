namespace DCL.Components
{
    /// <summary>
    /// Unity is unable to yield a coroutine while is already being yielded by another one.
    /// To fix that we wrap the routine in a CustomYieldInstruction.
    /// </summary>
    public class WaitForComponentUpdate : CleanableYieldInstruction
    {
        public IDelayedComponent component;

        public WaitForComponentUpdate(IDelayedComponent component) { this.component = component; }

        public override bool keepWaiting { get { return component.isRoutineRunning; } }

        public override void Cleanup() { component.Cleanup(); }
    }
}