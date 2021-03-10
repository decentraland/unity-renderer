using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickBarController 
{
    public event System.Action<CatalogItem> OnCatalogItemSelected;

    private CatalogItem[] quickBarShortcutsCatalogItems = new CatalogItem[AMOUNT_OF_QUICK_SLOTS];

    const int AMOUNT_OF_QUICK_SLOTS = 9;

    public QuickBarController(QuickBarView view)
    { 
        view.OnQuickBarAdd += SetQuickBarShortcut;
    }

    public int GetSlotsCount()
    {
        return AMOUNT_OF_QUICK_SLOTS;
    }

    public CatalogItem QuickBarObjectSelected(int index)
    {
        if (quickBarShortcutsCatalogItems.Length > index && quickBarShortcutsCatalogItems[index] != null)
        {
            OnCatalogItemSelected?.Invoke(quickBarShortcutsCatalogItems[index]);
            return quickBarShortcutsCatalogItems[index];
        }

        return null;
    }

    void SetQuickBarShortcut(CatalogItem catalogItem, int index)
    {
        quickBarShortcutsCatalogItems[index] = catalogItem;
    }

    int FindEmptyShortcutSlot()
    {
        for (int i = 0; i < quickBarShortcutsCatalogItems.Length; i++)
        {
            if (quickBarShortcutsCatalogItems[i] == null)
                return i;
        }
        return -1;
    }
}
