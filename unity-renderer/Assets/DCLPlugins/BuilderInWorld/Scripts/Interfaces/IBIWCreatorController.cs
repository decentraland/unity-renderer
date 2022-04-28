using System;
using DCL.Models;
using UnityEngine;

public interface IBIWCreatorController : IBIWController
{
    event Action OnCatalogItemPlaced;
    event Action OnInputDone;
    void CreateCatalogItem(CatalogItem catalogItem, bool autoSelect = true, bool isFloor = false);
    BIWEntity CreateCatalogItem(CatalogItem catalogItem, Vector3 startPosition, bool autoSelect = true, bool isFloor = false, Action<IDCLEntity> onFloorLoadedAction = null);
    void CreateErrorOnEntity(BIWEntity entity);
    void RemoveLoadingObjectInmediate(long entityId);
    bool IsAnyErrorOnEntities();
    void CreateLoadingObject(BIWEntity entity);
    void CleanUp();
}