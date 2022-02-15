using UnityEngine;

namespace EmotesDeck
{
    public interface IEmotesDeckComponentView
    {
        IEmoteSlotSelectorComponentView currentEmoteSlotSelector { get; }
        IEmoteSlotViewerComponentView currentEmoteSlotViewer { get; }
        IGridContainerComponentView currentEmotesGrid { get; }
    }

    public class EmotesDeckComponentView : BaseComponentView, IEmotesDeckComponentView
    {
        [Header("Prefab References")]
        [SerializeField] internal EmoteSlotSelectorComponentView emoteSlotSelector;
        [SerializeField] internal EmoteSlotViewerComponentView emoteSlotViewer;
        [SerializeField] internal GridContainerComponentView emotesGrid;

        public IEmoteSlotSelectorComponentView currentEmoteSlotSelector => emoteSlotSelector;
        public IEmoteSlotViewerComponentView currentEmoteSlotViewer => emoteSlotViewer;
        public IGridContainerComponentView currentEmotesGrid => emotesGrid;

        public override void RefreshControl()
        {
            emoteSlotSelector.RefreshControl();
            emoteSlotViewer.RefreshControl();
            emotesGrid.RefreshControl();
        }
    }
}