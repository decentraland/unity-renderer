using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalogAssetPackListView : ListView<SceneAssetPack>
{

    public CatalogAssetPackAdapter catalopgAssetPackItemAdapterPrefab;
    public System.Action<SceneAssetPack> OnSceneAssetPackClick;



    public override void AddAdapters()
    {
        base.AddAdapters();
        if (contentPanelTransform == null)
            return;

        foreach (SceneAssetPack sceneAssetPack in contentList)
        {
            CatalogAssetPackAdapter adapter = Instantiate(catalopgAssetPackItemAdapterPrefab, contentPanelTransform).GetComponent<CatalogAssetPackAdapter>();
            adapter.SetContent(sceneAssetPack);
            adapter.OnSceneAssetPackClick += SceneAssetPackClick;
        }
    }

    void SceneAssetPackClick(SceneAssetPack sceneAssetPack)
    {
        OnSceneAssetPackClick?.Invoke(sceneAssetPack);
    }
}
