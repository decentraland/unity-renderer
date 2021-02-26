using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FavoritesController 
{
    List<CatalogItem> favoritesCatalogItems = new List<CatalogItem>();

    public CatalogGroupListView catalogGroupListView;

    public FavoritesController(CatalogGroupListView catalogGroupListView)
    {
        catalogGroupListView.OnCatalogItemFavorite += ToggleFavoriteState;
    }

    public List<CatalogItem> GetFavorites()
    {
        return favoritesCatalogItems;
    }

    public void ToggleFavoriteState(CatalogItem catalogItem, CatalogItemAdapter adapter)
    {
        if (!favoritesCatalogItems.Contains(catalogItem))
        {
            favoritesCatalogItems.Add(catalogItem);
            catalogItem.SetFavorite(true);
        }
        else
        {
            favoritesCatalogItems.Remove(catalogItem);
            catalogItem.SetFavorite(false);
        }

        adapter?.SetFavorite(catalogItem.IsFavorite());
    }
}
