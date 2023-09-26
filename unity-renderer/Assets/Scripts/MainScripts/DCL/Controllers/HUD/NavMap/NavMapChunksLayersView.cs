using System;
using TMPro;
using UIComponents.Scripts.Components.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class NavMapChunksLayersView : MonoBehaviour
    {
        [SerializeField] private Button satelliteLayerButton;
        [SerializeField] private Button parcelsLayerButton;

        [SerializeField] private TMP_Text satelliteLayerText;
        [SerializeField] private TMP_Text parcelsLayerText;

        [SerializeField] private Color activeButtonColor;
        [SerializeField] private Color inactiveButtonColor = Color.white;

        [SerializeField] private Color activeTextColor = Color.white;
        [SerializeField] private Color inactiveTextColor = Color.black;

        [Space]
        [SerializeField] private TMPTextHyperLink tmpTextHyperLink;

        private Image satelliteLayerBackground;
        private Image parcelsLayerBackground;

        public event Action ParcelsButtonClicked;
        public event Action SatelliteButtonClicked;
        public event Action HyperLinkClicked;

        private void Awake()
        {
            satelliteLayerBackground = satelliteLayerButton.GetComponent<Image>();
            parcelsLayerBackground = parcelsLayerButton.GetComponent<Image>();
        }

        private void OnEnable()
        {
            parcelsLayerButton.onClick.AddListener(() => ParcelsButtonClicked?.Invoke());
            satelliteLayerButton.onClick.AddListener(() => SatelliteButtonClicked?.Invoke());

            tmpTextHyperLink.HyperLinkClicked += OnHyperLinkClicked;
        }

        private void OnDisable()
        {
            parcelsLayerButton.onClick.RemoveAllListeners();
            satelliteLayerButton.onClick.RemoveAllListeners();

            tmpTextHyperLink.HyperLinkClicked -= OnHyperLinkClicked;
        }

        private void OnHyperLinkClicked() =>
            HyperLinkClicked?.Invoke();

        public void SetState(bool satelliteViewActive)
        {
            if (satelliteViewActive)
            {
                tmpTextHyperLink.gameObject.SetActive(true);

                satelliteLayerBackground.color = activeButtonColor;
                parcelsLayerBackground.color = inactiveButtonColor;

                satelliteLayerText.color = activeTextColor;
                parcelsLayerText.color = inactiveTextColor;
            }
            else
            {
                tmpTextHyperLink.gameObject.SetActive(false);

                satelliteLayerBackground.color = inactiveButtonColor;
                parcelsLayerBackground.color = activeButtonColor;

                satelliteLayerText.color = inactiveTextColor;
                parcelsLayerText.color = activeTextColor;
            }
        }
    }
}
