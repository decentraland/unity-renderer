using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class LoadingBar : MonoBehaviour
{
    public float currentPercentage { get; set; }

    [SerializeField] internal string textPreviousToPercentage;
    [SerializeField] internal TMP_Text percentageText;
    [SerializeField] internal RectTransform loadingBar;

    private float maxBarWidth;
    private string percentagePreText = "";

    private void Awake()
    {
        maxBarWidth = ((RectTransform)transform).rect.size.x;

        if (!string.IsNullOrEmpty(textPreviousToPercentage))
            percentagePreText = $"{textPreviousToPercentage} ";
    }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public void SetPercentage(float newValue)
    {
        currentPercentage = newValue;
        percentageText.text = $"{percentagePreText}{newValue.ToString("0")}%";
        loadingBar.transform.localScale = new Vector3(newValue / 100f, 1, 1);
    }
}