using DCL.Interface;
using UnityEngine;
using System.Collections;

public class ManaHUDController : IHUD
{
    const float FETCH_INTERVAL = 60;
    const string URL_MANA_INFO = "https://docs.decentraland.org/examples/get-a-wallet";
    const string URL_MANA_PURCHASE = "https://market.decentraland.org/settings";

    internal ManaHUDview view;

    Coroutine fetchIntervalRoutine = null;

    public ManaHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ManaHUD")).GetComponent<ManaHUDview>();
        view.name = "_ManaHUD";
        view.SetVisibility(false);

        view.buttonManaInfo.onPointerDown += OnManaInfoPressed;
        view.buttonManaPurchase.onPointerDown += OnManaPurchasePressed;
        CommonScriptableObjects.isProfileHUDOpen.OnChange += IsProfileHUDOpen_OnChange;
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);

        if (visible && fetchIntervalRoutine == null)
        {
            fetchIntervalRoutine = CoroutineStarter.Start(IntervalRoutine());
        }
        else if (!visible && fetchIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchIntervalRoutine);
            fetchIntervalRoutine = null;
        }
    }

    public void SetBalance(string balance)
    {
        double manaBalance = 0;
        if (double.TryParse(balance, out manaBalance))
        {
            view.balanceText.text = FormatBalanceToString(manaBalance);
        }
    }

    public void Dispose()
    {
        if (view != null)
        {
            Object.Destroy(view.gameObject);
        }

        if (fetchIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchIntervalRoutine);
            fetchIntervalRoutine = null;
        }

        view.buttonManaInfo.onPointerDown -= OnManaInfoPressed;
        view.buttonManaPurchase.onPointerDown -= OnManaPurchasePressed;
        CommonScriptableObjects.isProfileHUDOpen.OnChange -= IsProfileHUDOpen_OnChange;
    }

    void OnManaInfoPressed()
    {
        WebInterface.OpenURL(URL_MANA_INFO);
    }

    void OnManaPurchasePressed()
    {
        WebInterface.OpenURL(URL_MANA_PURCHASE);
    }

    void IsProfileHUDOpen_OnChange(bool current, bool previous)
    {
        view.uiHoverTriggerShowHideAnimator.enabled = !current;
    }

    IEnumerator IntervalRoutine()
    {
        while (true)
        {
            WebInterface.FetchBalanceOfMANA();
            yield return WaitForSecondsCache.Get(FETCH_INTERVAL);
        }
    }

    string FormatBalanceToString(double balance)
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
