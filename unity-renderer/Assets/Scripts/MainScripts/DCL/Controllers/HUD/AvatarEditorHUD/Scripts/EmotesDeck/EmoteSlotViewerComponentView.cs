using TMPro;
using UnityEngine;

namespace EmotesDeck
{
    public interface IEmoteSlotViewerComponentView
    {
        /// <summary>
        /// Get the current selected slot number.
        /// </summary>
        int selectedSlot { get; }

        /// <summary>
        /// Set the selected slot.
        /// </summary>
        /// <param name="slotNumber">Number of the selected slot.</param>
        void SetSelectedSlot(int slotNumber);
    }

    public class EmoteSlotViewerComponentView : BaseComponentView, IEmoteSlotViewerComponentView, IComponentModelConfig
    {
        [Header("Prefab References")]
        [SerializeField] internal TMP_Text selectedSlotText;

        [Header("Configuration")]
        [SerializeField] internal EmoteSlotViewerComponentModel model;

        public int selectedSlot => model.selectedSlot;

        public void Configure(BaseComponentModel newModel)
        {
            model = (EmoteSlotViewerComponentModel)newModel;
            RefreshControl();
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetSelectedSlot(model.selectedSlot);
        }

        public void SetSelectedSlot(int slotNumber)
        {
            model.selectedSlot = slotNumber;

            if (selectedSlotText == null)
                return;

            selectedSlotText.text = slotNumber.ToString();
            selectedSlotText.gameObject.SetActive(slotNumber >= 0);
        }
    }
}