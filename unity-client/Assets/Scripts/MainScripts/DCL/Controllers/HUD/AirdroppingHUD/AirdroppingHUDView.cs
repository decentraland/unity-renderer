using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AirdroppingHUDView : MonoBehaviour
{
    private const string VIEW_PATH = "AirdroppingHUD";

    [SerializeField] internal GenericFactory collectiblesFactory;
    [SerializeField] internal GenericFactory erc20Factory;

    [SerializeField] internal GameObject content;

    [Header("Initial Screen")]
    [SerializeField] internal GameObject initialScreen;
    [SerializeField] internal TextMeshProUGUI initialScreenTitle;
    [SerializeField] internal TextMeshProUGUI initialScreenSubtitle;
    [SerializeField] internal Button initialScreenDoneButton;

    [Header("Single Item Screen")]
    [SerializeField] internal GameObject singleItemScreen;
    [SerializeField] internal GameObject singleItemContainer;
    [SerializeField] internal Button singleItemDoneButton;
    [SerializeField] internal TextMeshProUGUI itemsLeft;

    [Header("Summary Screen")]
    [SerializeField] internal GameObject summaryScreen;
    [SerializeField] internal GameObject summaryItemsContainer;
    [SerializeField] internal Button summaryDoneButton;

    [Header("Summary No Items Screen")]
    [SerializeField] internal GameObject summaryNoItemsScreen;
    [SerializeField] internal Button summaryNoItemsDoneButton;

    internal static AirdroppingHUDView Create()
    {
        return Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<AirdroppingHUDView>();
    }

    public void Initialize(UnityAction nextStateCallback)
    {
        initialScreenDoneButton.onClick.RemoveAllListeners();
        initialScreenDoneButton.onClick.AddListener(nextStateCallback);

        singleItemDoneButton.onClick.RemoveAllListeners();
        singleItemDoneButton.onClick.AddListener(nextStateCallback);

        summaryDoneButton.onClick.RemoveAllListeners();
        summaryDoneButton.onClick.AddListener(nextStateCallback);

        summaryNoItemsDoneButton.onClick.RemoveAllListeners();
        summaryNoItemsDoneButton.onClick.AddListener(nextStateCallback);

        CleanState();
    }

    public void ShowInitialScreen(string title, string subtitle)
    {
        CleanState();
        initialScreen.SetActive(true);
        initialScreenTitle.text = title;
        initialScreenSubtitle.text = subtitle;
    }

    public void ShowItemScreen(AirdroppingHUDController.ItemModel model, int itemsleft)
    {
        CleanState();
        singleItemScreen.SetActive(true);
        itemsLeft.text = itemsleft.ToString();
        CreateItemPanel(singleItemContainer.transform, model).SetData(model.name, model.subtitle, model.thumbnailURL);
    }

    public void ShowSummaryScreen(AirdroppingHUDController.ItemModel[] items)
    {
        CleanState();
        summaryScreen.SetActive(true);
        for (int index = 0; index < items.Length; index++)
        {
            var item = items[index];
            CreateItemPanel(summaryItemsContainer.transform, items[index]).SetData(item.name, item.subtitle, item.thumbnailURL);
        }
    }

    public void ShowSummaryNoItemsScreen()
    {
        CleanState();
        summaryNoItemsScreen.SetActive(true);
    }

    public void CleanState()
    {
        initialScreen.SetActive(false);

        singleItemScreen.SetActive(false);
        singleItemContainer.transform.DestroyAllChild();

        summaryScreen.SetActive(false);
        summaryItemsContainer.transform.DestroyAllChild();

        summaryNoItemsScreen.SetActive(false);
    }

    public void SetContentActive(bool active)
    {
        content.SetActive(active);
    }

    public void SetVisibility(bool active)
    {
        gameObject.SetActive(active);
    }

    private AirdroppingItemPanel CreateItemPanel(Transform parent, AirdroppingHUDController.ItemModel model)
    {
        AirdroppingItemPanel item = null;
        if (model.type == "collectible")
        {
            item = collectiblesFactory.Instantiate<AirdroppingItemPanel>(model.rarity, parent);
        }

        if (model.type == "erc20")
        {
            item = erc20Factory.Instantiate<AirdroppingItemPanel>(model.rarity, parent);
        }

        return item;
    }
}