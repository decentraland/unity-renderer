using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSelector : MonoBehaviour
{
    [SerializeField]
    private ItemToggleFactory itemToggleFactory;

    [SerializeField]
    private RectTransform itemContainer;

    public bool mustHaveSelection;

    public event System.Action<WearableItem> OnItemEquipped;

    public event System.Action<string> OnItemUnequipped;

    private Dictionary<string, ItemToggle> itemToggles = new Dictionary<string, ItemToggle>();
    private List<ItemToggle> activeItemToggles = new List<ItemToggle>();

    private ItemToggle currentToggle;
    private string currentBodyShape;

    private void Awake()
    {
        Application.quitting += () =>
        {
            OnItemEquipped = null;
            OnItemUnequipped = null;
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
        if (active)
        {
            activeItemToggles.Add(newToggle);
        }
        else
        {
            newToggle.gameObject.SetActive(false);
        }
    }

    public void RemoveItemToggle(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return;

        ItemToggle toggle = GetItemToggleByID(itemID);
        if (toggle == null) return;

        bool wasSelected = toggle.selected;
        activeItemToggles.Remove(toggle);
        itemToggles.Remove(itemID);
        Destroy(toggle);

        if (wasSelected)
        {
            OnItemUnequipped?.Invoke(itemID);
            if (mustHaveSelection)
            {
                SelectFirstActive();
            }
        }
    }

    public void SetBodyShape(string bodyShape)
    {
        if (currentBodyShape == bodyShape) return;

        currentBodyShape = bodyShape;
        ShowCompatibleWithBodyShape();
    }

    private void ShowCompatibleWithBodyShape()
    {
        activeItemToggles.Clear();

        using (Dictionary<string, ItemToggle>.Enumerator iterator = itemToggles.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                ItemToggle current = iterator.Current.Value;
                bool active = current.wearableItem.SupportsBodyShape(currentBodyShape);
                current.gameObject.SetActive(active);
                if (active)
                {
                    activeItemToggles.Add(current);
                }
            }
        }

        if (currentToggle != null && !currentToggle.wearableItem.SupportsBodyShape(currentBodyShape))
        {
            OnItemUnequipped?.Invoke(currentToggle.wearableItem.id);
            currentToggle.selected = false;
            currentToggle = null;
        }

        if (mustHaveSelection && currentToggle == null)
        {
            SelectFirstActive();
        }
    }

    public void Select(string itemID)
    {
        ItemToggle toggle = GetItemToggleByID(itemID);
        Select(toggle);
    }

    public void SelectFirstActive()
    {
        if (activeItemToggles.Count == 0) return;

        ItemToggle toggle = activeItemToggles[0];
        Select(toggle);
    }

    public void SelectRandom()
    {
        if (activeItemToggles.Count == 0) return;

        ItemToggle toggle = activeItemToggles[Random.Range(0, activeItemToggles.Count)];
        Select(toggle);
    }

    private void Select(ItemToggle itemToggle)
    {
        if (itemToggle == null) return;
        if (currentToggle == itemToggle) return;

        if (currentToggle != null)
        {
            OnItemUnequipped?.Invoke(currentToggle.wearableItem.id);
            currentToggle.selected = false;
        }

        currentToggle = itemToggle;
        currentToggle.selected = true;
        OnItemEquipped?.Invoke(currentToggle.wearableItem);
    }

    private void ToggleClicked(ItemToggle toggle)
    {
        if (toggle == null) return;

        if (toggle == currentToggle)
        {
            if (mustHaveSelection) return;

            OnItemUnequipped?.Invoke(currentToggle.wearableItem.id);
            currentToggle.selected = false;
            currentToggle = null;
        }
        else
        {
            if (currentToggle != null)
            {
                currentToggle.selected = false;
                OnItemUnequipped?.Invoke(currentToggle.wearableItem.id);
            }

            currentToggle = toggle;

            if (currentToggle != null)
            {
                currentToggle.selected = true;
                OnItemEquipped?.Invoke(currentToggle.wearableItem);
            }
        }
    }

    private ItemToggle GetItemToggleByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return null;
        return itemToggles.ContainsKey(itemID) ? itemToggles[itemID] : null;
    }

    public void Unselect()
    {
        if (currentToggle == null) return;

        currentToggle.selected = false;
        currentToggle = null;
    }
}