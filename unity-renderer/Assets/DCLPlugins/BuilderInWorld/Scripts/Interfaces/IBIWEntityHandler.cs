using DCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DCL.Builder;
using DCL.Controllers;
using UnityEngine;

public interface IBIWEntityHandler : IBIWController
{
    event Action<BIWEntity> OnEntityDeselected;
    event Action OnEntitySelected;
    event Action<List<BIWEntity>> OnDeleteSelectedEntities;
    event Action<BIWEntity> OnEntityDeleted;
    BIWEntity GetConvertedEntity(long entityId);
    BIWEntity GetConvertedEntity(IDCLEntity entity);
    void DeleteEntity(BIWEntity entityToDelete);
    void DeleteEntity(long entityId);
    void DeleteFloorEntities();
    void DeleteSelectedEntities();
    IDCLEntity CreateEntityFromJSON(string entityJson);
    BIWEntity CreateEmptyEntity(IParcelScene parcelScene, Vector3 entryPoint, Vector3 editionGOPosition, bool notifyEntityList = true);
    List<BIWEntity> GetAllEntitiesFromCurrentScene();
    void DeselectEntities();
    List<BIWEntity> GetSelectedEntityList();
    bool IsAnyEntitySelected();
    void SetActiveMode(IBIWMode buildMode);
    void SetMultiSelectionActive(bool isActive);
    void ChangeLockStateSelectedEntities();
    void DeleteEntitiesOutsideSceneBoundaries();
    bool AreAllEntitiesInsideBoundaries();
    void EntityListChanged();
    void NotifyEntityIsCreated(IDCLEntity entity);
    void SetEntityName(BIWEntity entityToApply, string newName, bool sendUpdateToKernel = true);
    void EntityClicked(BIWEntity entityToSelect);
    void ReportTransform(bool forceReport = false);
    void CancelSelection();
    bool IsPointerInSelectedEntity();
    void DestroyLastCreatedEntities();
    void Select(IDCLEntity entity);
    bool SelectEntity(BIWEntity entityEditable, bool selectedFromCatalog = false);
    void DeselectEntity(BIWEntity entity);
    int GetCurrentSceneEntityCount();
}