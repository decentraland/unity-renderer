using DCL.ECSComponents;

namespace DCLPlugins.ECSComponents.Events
{
    // This struct represent a pointer event that will be sent to the scene. We will send this fields so the 
    // content creator can detect that a pointer event has occur
    public readonly struct PointerEvent
    {
        public readonly string sceneId;
        public readonly ActionButton button;
        public readonly RaycastHit hit;
        public readonly PointerEventType type;
        public readonly int timestamp;
        public readonly float analog;

        public PointerEvent(string sceneId, ActionButton button,
            RaycastHit hit, PointerEventType type, int timestamp, float analog)
        {
            this.sceneId = sceneId;
            this.button = button;
            this.hit = hit;
            this.type = type;
            this.timestamp = timestamp;
            this.analog = analog;
        }
    }
}