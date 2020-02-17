using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("AvatarEditorHUDTests")]
public class ItemSelector : MonoBehaviour
{
    [SerializeField]
    internal ItemToggleFactory itemToggleFactory;

    [SerializeField]
    private RectTransform itemContainer;

    public event System.Action<string> OnItemClicked;
    public event System.Action<string> OnSellClicked;

    internal Dictionary<string, ItemToggle> itemToggles = new Dictionary<string, ItemToggle>();

    private string currentBodyShape;

    private void Awake()
    {
        Application.quitting += () =>
        {
            OnItemClicked = null;
        };
    }

    public void AddItemToggle(WearableItem item, int amount)
    {
        if (item == null) return;
        if (itemToggles.ContainsKey(item.id)) return;

        ItemToggle newToggle;
        if (item.IsCollectible())
        {
            newToggle = itemToggleFactory.CreateItemToggleFromRarity(item.rarity, itemContainer);
            newToggle.transform.SetAsFirstSibling();
        }
        else
        {
            newToggle = itemToggleFactory.CreateBaseWearable(itemContainer);
        }

        newToggle.Initialize(item, false, amount);
        newToggle.OnClicked += ToggleClicked;
        newToggle.OnSellClicked += SellClicked;
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
        Destroy(toggle.gameObject);
    }

    public void RemoveAllItemToggle()
    {
        using (var it = itemToggles.GetEnumerator())
        {
            while (it.MoveNext())
            {
                Destroy(it.Current.Value.gameObject);
            }
        }

        itemToggles.Clear();
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
        if (toggle != null)
            toggle.selected = true;
    }

    public void Unselect(string itemID)
    {
        ItemToggle toggle = GetItemToggleByID(itemID);
        if (toggle != null)
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

    private void SellClicked(ItemToggle toggle)
    {
        OnSellClicked?.Invoke(toggle.wearableItem.id);
    }

    private ItemToggle GetItemToggleByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return null;
        return itemToggles.ContainsKey(itemID) ? itemToggles[itemID] : null;
    }
}
