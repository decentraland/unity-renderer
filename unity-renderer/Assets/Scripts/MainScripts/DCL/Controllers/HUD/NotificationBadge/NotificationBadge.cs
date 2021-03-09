using System.Collections.Generic;
using UnityEngine;

public class NotificationBadge : MonoBehaviour
{
    [Tooltip("The value shown on this badge is the sum of the notification variables")]
    [SerializeField] private List<FloatVariable> notificationVariables;
    [SerializeField] private TMPro.TextMeshProUGUI notificationText;
    [SerializeField] private GameObject notificationContainer;

    public int finalValue { get; private set; }
    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (notificationVariables == null || notificationVariables.Count == 0)
            return;

        foreach (var notiVariable in notificationVariables)
        {
            notiVariable.OnChange -= NotificationVariable_OnChange;
            notiVariable.OnChange += NotificationVariable_OnChange;
            NotificationVariable_OnChange(notiVariable.Get(), notiVariable.Get());
        }
    }

    private void OnDestroy()
    {
        if (notificationVariables == null || notificationVariables.Count == 0)
            return;

        foreach (var notiVariable in notificationVariables)
        {
            notiVariable.OnChange -= NotificationVariable_OnChange;
        }
    }

    private void NotificationVariable_OnChange(float current, float previous)
    {
        int finalValue = 0;

        foreach (var notiVariable in notificationVariables)
        {
            finalValue += (int)notiVariable.Get();
        }

        this.finalValue = finalValue;

        if (finalValue > 0)
        {
            notificationContainer.SetActive(true);

            if (finalValue < 99)
            {
                notificationText.text = finalValue.ToString();
            }
            else
            {
                notificationText.text = "99+";
            }
        }
        else
        {
            notificationContainer.SetActive(false);
        }
    }
}
