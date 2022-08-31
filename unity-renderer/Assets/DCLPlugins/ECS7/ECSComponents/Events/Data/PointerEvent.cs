using DCL.ECSComponents;

namespace DCLPlugins.ECSComponents.Events
{
    // This struct represent a pointer event that will be sent to the scene. We will send this fields so the 
    // content creator can detect that a pointer event has occur
    public struct PointerEvent
    {
        public string sceneId;
        public ActionButton button;
        public RaycastHit hit;
        public PointerEventType type;
        public int timestamp;
        public float analog;
    }
}