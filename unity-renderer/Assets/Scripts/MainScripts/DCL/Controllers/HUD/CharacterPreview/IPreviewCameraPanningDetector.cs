using System;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface IPreviewCameraPanningDetector
    {
        event Action OnDragStarted;
        event Action OnDragging;
        event Action OnDragFinished;
    }
}
