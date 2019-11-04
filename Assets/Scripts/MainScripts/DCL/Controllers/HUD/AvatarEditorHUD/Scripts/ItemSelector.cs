using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSelector : MonoBehaviour
{
    [SerializeField]
    private ItemToggleFactory itemToggleFactory;

    [SerializeField]
    private RectTransform itemContainer;

    public event System.Action<string> OnItemClicked;

    private Dictionary<string, ItemToggle> itemToggles = new Dictionary<string, ItemToggle>();

    private string currentBodyShape;

    private void Awake()
    {
        Application.quitting += () =>
        {
            OnItemClicked = null;
        };
    }

    public void AddItemToggle(WearableItem item)
    {
        if (item == null) return;
        if (itemToggles.ContainsKey(item.id)) return;

        ItemToggle newToggle;
        if (item.tags.Contains(WearableItem.nftWearableTag))
        {
            newToggle = itemToggleFactory.CreateItemToggleFromType(WearableItem.nftWearableTag, itemContainer); // TODO: use enum
            newToggle.transform.SetAsFirstSibling();
        }
        else
        {
            newToggle = itemToggleFactory.CreateItemToggleFromType(WearableItem.baseWearableTag, itemContainer);
        }

        newToggle.Initialize(item, false);
        newToggle.OnClicked += ToggleClicked;
        itemToggles.Add(item.id, newToggle);

        bool active = string.IsNullOrEmpty(currentBodyShape) || item.SupportsBodyShape(currentBodyShape);
        newToggle.gameObject.SetActive(active);
    }

    public void RemoveItemToggle(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return;

        ItemToggle toggle = GetItemToggleByID(itemID);
        if (toggle == null) return;

        itemToggles.Remove(itemID);
        Destroy(toggle);
    }

    public void SetBodyShape(string bodyShape)
    {
        if (currentBodyShape == bodyShape) return;

        currentBodyShape = bodyShape;
        ShowCompatibleWithBodyShape();
    }

    private void ShowCompatibleWithBodyShape()
    {
        using (Dictionary<string, ItemToggle>.Enumerator iterator = itemToggles.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                ItemToggle current = iterator.Current.Value;
                bool active = current.wearableItem.SupportsBodyShape(currentBodyShape);
                current.gameObject.SetActive(active);
            }
        }
    }

    public void Select(string itemID)
    {
        ItemToggle toggle = GetItemToggleByID(itemID);
        toggle.selected = true;
    }

    public void Unselect(string itemID)
    {
        ItemToggle toggle = GetItemToggleByID(itemID);
        toggle.selected = false;
    }

    public void UnselectAll()
    {
        using (var iterator = itemToggles.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.selected = false;
            }
        }
    }

    private void ToggleClicked(ItemToggle toggle)
    {
        OnItemClicked?.Invoke(toggle.wearableItem.id);
    }

    private ItemToggle GetItemToggleByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return null;
        return itemToggles.ContainsKey(itemID) ? itemToggles[itemID] : null;
    }
}