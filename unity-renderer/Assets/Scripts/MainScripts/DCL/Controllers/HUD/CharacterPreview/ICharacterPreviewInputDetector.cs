using System;
using UnityEngine.EventSystems;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public interface ICharacterPreviewInputDetector : IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        event Action<PointerEventData> OnDragStarted;
        event Action<PointerEventData> OnDragging;
        event Action<PointerEventData> OnDragFinished;
        event Action<PointerEventData> OnPointerFocus;
        event Action<PointerEventData> OnPointerUnFocus;
    }
}
