using System;
using TMPro;
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

        private Image satelliteLayerBackground;
        private Image parcelsLayerBackground;

        public event Action ParcelsButtonClicked;
        public event Action SatelliteButtonClicked;

        private void Awake()
        {
            satelliteLayerBackground = satelliteLayerButton.GetComponent<Image>();
            parcelsLayerBackground = parcelsLayerButton.GetComponent<Image>();
        }

        private void OnEnable()
        {
            parcelsLayerButton.onClick.AddListener(() => ParcelsButtonClicked?.Invoke());
            satelliteLayerButton.onClick.AddListener(() => SatelliteButtonClicked?.Invoke());
        }

        private void OnDisable()
        {
            parcelsLayerButton.onClick.RemoveAllListeners();
            satelliteLayerButton.onClick.RemoveAllListeners();
        }

        public void SetState(bool satelliteViewActive)
        {
            if (satelliteViewActive)
            {
                satelliteLayerBackground.color = activeButtonColor;
                parcelsLayerBackground.color = inactiveButtonColor;

                satelliteLayerText.color = activeTextColor;
                parcelsLayerText.color = inactiveTextColor;
            }
            else
            {
                satelliteLayerBackground.color = inactiveButtonColor;
                parcelsLayerBackground.color = activeButtonColor;

                satelliteLayerText.color = inactiveTextColor;
                parcelsLayerText.color = activeTextColor;
            }
        }
    }
}
