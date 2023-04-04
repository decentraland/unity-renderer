using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;

namespace DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces
{
    public enum PointerInputEventType
    {
        NONE,
        CLICK,
        DOWN,
        UP
    }

    public interface IPointerEvent : IMonoBehaviour
    {
        IDCLEntity entity { get; }
        void SetHoverState(bool state);
        bool IsAtHoverDistance(float distance);
        bool IsVisible();
    }

    public interface IPointerInputEvent : IPointerEvent
    {
        void Report(WebInterface.ACTION_BUTTON buttonId, UnityEngine.Ray ray, HitInfo hit);
        PointerInputEventType GetEventType();
        WebInterface.ACTION_BUTTON GetActionButton();
        bool ShouldShowHoverFeedback();
    }

    public interface IAvatarOnPointerDown : IPointerInputEvent { }

    public interface IUnlockedCursorInputEvent : IPointerEvent { }
}
