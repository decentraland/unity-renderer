using System.Collections.Generic;

public class FavoritesController
{
    private List<CatalogItem> favoritesCatalogItems = new List<CatalogItem>();

    public CatalogGroupListView catalogGroupListView;

    public FavoritesController(CatalogGroupListView catalogGroupListView)
    {
        if (catalogGroupListView == null)
            return;

        catalogGroupListView.OnCatalogItemFavorite += ToggleFavoriteState;
    }

    public void Dispose()
    {
        if (catalogGroupListView != null)
            catalogGroupListView.OnCatalogItemFavorite -= ToggleFavoriteState;
    }

    public List<CatalogItem> GetFavorites() { return favoritesCatalogItems; }

    public void ToggleFavoriteState(CatalogItem catalogItem, CatalogItemAdapter adapter)
    {
        if (!favoritesCatalogItems.Contains(catalogItem))
        {
            favoritesCatalogItems.Add(catalogItem);
            catalogItem.SetFavorite(true);
            BIWAnalytics.FavoriteAdded(catalogItem);
        }
        else
        {
            favoritesCatalogItems.Remove(catalogItem);
            catalogItem.SetFavorite(false);
        }

        adapter?.SetFavorite(catalogItem.IsFavorite());
    }
}