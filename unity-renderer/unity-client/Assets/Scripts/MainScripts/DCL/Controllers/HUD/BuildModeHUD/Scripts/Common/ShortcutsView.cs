using System;
using UnityEngine;
using UnityEngine.UI;

public interface IShortcutsView
{
    event Action OnCloseButtonClick;

    void OnCloseClick();
    void SetActive(bool isActive);
}

public class ShortcutsView : MonoBehaviour, IShortcutsView
{
    public event Action OnCloseButtonClick;

    [SerializeField] internal Button closeButton;

    private const string VIEW_PATH = "Common/ShortcutsView";

    internal static ShortcutsView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ShortcutsView>();
        view.gameObject.name = "_ShortcutsView";

        return view;
    }

    private void Awake()
    {
        closeButton.onClick.AddListener(OnCloseClick);
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void OnCloseClick()
    {
        OnCloseButtonClick?.Invoke();
    }
}
