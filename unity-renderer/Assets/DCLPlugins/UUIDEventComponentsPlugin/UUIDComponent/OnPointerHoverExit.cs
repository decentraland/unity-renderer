using DCL.Interface;
using DCL.Models;

namespace DCL.Components
{
    public class OnPointerHoverExit : OnPointerHoverEvent
    {
        public const string NAME = "pointerHoverExit";

        private bool isHovering = true;

        public override void SetHoverState(bool hoverState)
        {
            base.SetHoverState(hoverState);
            if (!hoverState && isHovering)
            {
                Model model = this.model as Model;
                WebInterface.ReportOnPointerHoverExitEvent(scene.sceneData.sceneNumber, model.uuid);
            }

            isHovering = hoverState;
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT;
        }

        protected override string uuidComponentName => "pointerHoverExit";
    }
}
