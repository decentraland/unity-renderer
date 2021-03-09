using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FavoritesController 
{
    List<SceneObject> favoritesSceneObjects = new List<SceneObject>();

    public CatalogGroupListView catalogGroupListView;

    public FavoritesController(CatalogGroupListView catalogGroupListView)
    {
        catalogGroupListView.OnSceneObjectFavorite += ToggleFavoriteState;
    }

    public List<SceneObject> GetFavorites()
    {
        return favoritesSceneObjects;
    }

    public void ToggleFavoriteState(SceneObject sceneObject, CatalogItemAdapter adapter)
    {

        if (!favoritesSceneObjects.Contains(sceneObject))
        {
            favoritesSceneObjects.Add(sceneObject);
            sceneObject.isFavorite = true;
        }
        else
        {
            favoritesSceneObjects.Remove(sceneObject);
            sceneObject.isFavorite = false;
        }

        adapter.SetFavorite(sceneObject.isFavorite);
    }
}
