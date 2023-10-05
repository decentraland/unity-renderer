using System;
using UIComponents.Scripts.Components.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class NavMapChunksLayersView : MonoBehaviour
    {
        private const int SATELLITE_SECTION = 0;
        private const int PARCELS_SECTION = 1;

        [SerializeField] private SectionSelectorComponentView atlasLayerSectionSelector;

        [Space]
        [SerializeField] private TMPTextHyperLink tmpTextHyperLink;

        private Image satelliteLayerBackground;
        private Image parcelsLayerBackground;

        public event Action ParcelsButtonClicked;
        public event Action SatelliteButtonClicked;
        public event Action HyperLinkClicked;

        private void Start()
        {
            atlasLayerSectionSelector.GetSection(SATELLITE_SECTION)
                                     .onSelect.AddListener(isActive =>
                                      {
                                          if (isActive)
                                              SatelliteButtonClicked?.Invoke();
                                      });

            atlasLayerSectionSelector.GetSection(PARCELS_SECTION)
                                     .onSelect.AddListener(isActive =>
                                      {
                                          if (isActive)
                                              ParcelsButtonClicked?.Invoke();
                                      });

            tmpTextHyperLink.HyperLinkClicked += OnHyperLinkClicked;
        }

        private void OnDestroy()
        {
            atlasLayerSectionSelector.GetSection(SATELLITE_SECTION).onSelect.RemoveAllListeners();
            atlasLayerSectionSelector.GetSection(PARCELS_SECTION).onSelect.RemoveAllListeners();

            tmpTextHyperLink.HyperLinkClicked -= OnHyperLinkClicked;
        }

        public void Hide()
        {
            tmpTextHyperLink.gameObject.SetActive(false);
            atlasLayerSectionSelector.gameObject.SetActive(false);
        }

        private void OnHyperLinkClicked() =>
            HyperLinkClicked?.Invoke();

        public void SetState(bool satelliteViewActive) =>
            tmpTextHyperLink.gameObject.SetActive(satelliteViewActive);
    }
}
