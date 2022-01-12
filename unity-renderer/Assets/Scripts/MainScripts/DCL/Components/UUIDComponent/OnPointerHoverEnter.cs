using DCL.Interface;
using DCL.Models;

namespace DCL.Components
{
    public class OnPointerHoverEnter : OnPointerHoverEvent
    {
        public const string NAME = "pointerHoverEnter";

        private bool isHovering = false;

        public override void SetHoverState(bool hoverState)
        {
            if (hoverState && !isHovering)
            {
                Model model = this.model as Model;
                WebInterface.ReportOnPointerHoverEnterEvent(scene.sceneData.id, model.uuid);
            }
            isHovering = hoverState;
        }

        public override int GetClassId()
        {
            return (int)CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER;
        }
    }
}