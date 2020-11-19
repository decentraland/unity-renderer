using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ProfileHUD sub-view that shows the user MANA balance.
/// </summary>
internal class ManaCounterView : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI balanceText;
    [SerializeField] internal Button_OnPointerDown buttonManaInfo;
    [SerializeField] internal Button buttonManaPurchase;

    /// <summary>
    /// Set the amount of MANA in the HUD.
    /// </summary>
    /// <param name="balance">Amount of MANA.</param>
    public void SetBalance(string balance)
    {
        double manaBalance = 0;
        if (double.TryParse(balance, out manaBalance))
        {
            balanceText.text = FormatBalanceToString(manaBalance);
        }
    }

    private string FormatBalanceToString(double balance)
    {
        if (balance >= 100000000)
        {
            return (balance / 1000000D).ToString("0.#M");
        }
        if (balance >= 1000000)
        {
            return (balance / 1000000D).ToString("0.##M");
        }
        if (balance >= 100000)
        {
            return (balance / 1000D).ToString("0.#K");
        }
        if (balance >= 10000)
        {
            return (balance / 1000D).ToString("0.##K");
        }
        if (balance < 0.001)
        {
            return "0";
        }
        if (balance <= 1)
        {
            return balance.ToString("0.###");
        }
        if (balance < 100)
        {
            return balance.ToString("0.##");
        }

        return balance.ToString("#,0");
    }
}
