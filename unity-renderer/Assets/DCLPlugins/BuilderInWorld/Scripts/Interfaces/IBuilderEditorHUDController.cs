using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using DCL.Builder;
using DCL.Controllers;
using UnityEngine;

public interface IBuilderEditorHUDController
{
    event Action<string, string> OnProjectNameAndDescriptionChanged;
    event Action OnUndoAction;
    event Action OnRedoAction;
    event Action OnResetAction;
    event Action OnChangeModeAction;
    event Action OnChangeSnapModeAction;
    event Action<BIWEntity> OnEntityDelete;
    event Action OnDuplicateSelectedAction;
    event Action OnDeleteSelectedAction;
    event Action<BIWEntity> OnEntityClick;
    event Action<BIWEntity> OnEntityLock;
    event Action<BIWEntity> OnEntityChangeVisibility;
    event Action<BIWEntity> OnEntitySmartItemComponentUpdate;
    event Action OnTutorialAction;
    event Action OnStartExitAction;
    event Action OnLogoutAction;
    event Action OnTranslateSelectedAction;
    event Action OnRotateSelectedAction;
    event Action OnScaleSelectedAction;
    event Action<Vector3> OnSelectedObjectPositionChange;
    event Action<Vector3> OnSelectedObjectRotationChange;
    event Action<Vector3> OnSelectedObjectScaleChange;
    event Action OnResetCameraAction;
    event Action OnPublishAction;
    event Action OnStopInput;
    event Action OnResumeInput;
    event Action<string, string, string> OnConfirmPublishAction;
    void Initialize(IContext context);
    void Dispose();
    void RefreshCatalogAssetPack();
    void RefreshCatalogContent();
    void ExitStart();
    void SetParcelScene(IParcelScene sceneToEdit);
    void SetVisibilityOfCatalog(bool b);
    void SetVisibilityOfInspector(bool b);
    void SetVisibility(bool b);
    void ConfigureConfirmationModal(string exitModalTitle, string exitWithoutPublishModalSubtitle, string exitWithoutPublishModalCancelButton, string exitWithoutPublishModalConfirmButton);
    void ClearEntityList();
    void ActivateGodModeUI();
    void SetActionsButtonsInteractable(bool areInteratable);
    void SetGizmosActive(string gizmos);
    event Action<CatalogItem> OnCatalogItemSelected;
    event Action<CatalogItem> OnCatalogItemDropped;
    BuildModeCatalogSection GetCatalogSectionSelected();
    void HideExtraBtns();
    void ShowSceneLimitsPassed();
    void SceneSaved();
    void SetPublishBtnAvailability(bool canPublish, string feedbackMessage);
    void SetRedoButtonInteractable(bool canRedoAction);
    void SetUndoButtonInteractable(bool canUndoAction);
    void UpdateEntitiesSelection(int selectedEntitiesCount);
    void SetEntityList(List<BIWEntity> getEntitiesInCurrentScene);
    void NewSceneForLand(IBuilderScene scene);
    void HideEntityInformation();
    void UpdateSceneLimitInfo();
    void EntityInformationSetEntity(BIWEntity entityEditable, IParcelScene sceneToEdit);
    void ShowEntityInformation(bool selectedFromCatalog);
    event Action<BIWEntity, string> OnEntityRename;
    void SetSnapModeActive(bool isSnapActiveValue);
    void ActivateFirstPersonModeUI();
}

public enum BuildModeCatalogSection
{
    CATEGORIES,
    ASSET_PACKS,
    FAVOURITES
}