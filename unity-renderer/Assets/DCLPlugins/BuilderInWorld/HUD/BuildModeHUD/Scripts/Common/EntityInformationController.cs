using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using System;
using UnityEngine;

public interface IEntityInformationController
{
    event Action<Vector3> OnPositionChange;
    event Action<Vector3> OnRotationChange;
    event Action<Vector3> OnScaleChange;
    event Action<BIWEntity, string> OnNameChange;
    event Action<BIWEntity> OnSmartItemComponentUpdate;
    event Action OnDisable;

    void Initialize(IEntityInformationView view);
    void Dispose();
    void PositionChanged(Vector3 pos);
    void RotationChanged(Vector3 rot);
    void ScaleChanged(Vector3 scale);
    void NameChanged(BIWEntity entity, string name);
    void ToggleDetailsInfo();
    void ToggleBasicInfo();
    void StartChangingName();
    void EndChangingName();
    void SetEntity(BIWEntity entity, IParcelScene currentScene);
    void Enable();
    void Disable();
    void UpdateInfo(BIWEntity entity);
    void UpdateEntitiesSelection(int numberOfSelectedEntities);
    void SetTransparencyMode(bool isOn);
}

public class EntityInformationController : IEntityInformationController
{
    private const string TRIS_TEXT_FORMAT  = "{0} TRIS";
    private const string MATERIALS_TEXT_FORMAT  = "{0} MATERIALS";
    private const string TEXTURES_TEXT_FORMAT = "{0} TEXTURES";
    private const float TRANSPARENCY_MODE_ALPHA_VALUE = 0.5f;

    public event Action<Vector3> OnPositionChange;
    public event Action<Vector3> OnRotationChange;
    public event Action<Vector3> OnScaleChange;
    public event Action<BIWEntity, string> OnNameChange;
    public event Action<BIWEntity> OnSmartItemComponentUpdate;
    public event Action OnDisable;

    internal IEntityInformationView entityInformationView;
    internal IParcelScene parcelScene;
    internal AssetPromise_Texture loadedThumbnailPromise;
    internal bool isChangingName = false;
    internal BIWEntity currentEntity;

    public void Initialize(IEntityInformationView entityInformationView)
    {
        this.entityInformationView = entityInformationView;

        if (entityInformationView.position != null)
            entityInformationView.position.OnChanged += PositionChanged;

        if (entityInformationView.rotation != null)
            entityInformationView.rotation.OnChanged += RotationChanged;

        if (entityInformationView.scale != null)
            entityInformationView.scale.OnChanged += ScaleChanged;

        entityInformationView.OnNameChange += NameChanged;
        entityInformationView.OnStartChangingName += StartChangingName;
        entityInformationView.OnEndChangingName += EndChangingName;
        entityInformationView.OnDisable += Disable;
        entityInformationView.OnUpdateInfo += UpdateInfo;
    }

    public void Dispose()
    {
        if (entityInformationView == null)
            return;
        
        if (entityInformationView.position != null)
            entityInformationView.position.OnChanged -= PositionChanged;

        if (entityInformationView.rotation != null)
            entityInformationView.rotation.OnChanged -= RotationChanged;

        if (entityInformationView.scale != null)
            entityInformationView.scale.OnChanged -= ScaleChanged;

        entityInformationView.OnNameChange -= NameChanged;
        entityInformationView.OnUpdateInfo -= UpdateInfo;
        entityInformationView.OnStartChangingName -= StartChangingName;
        entityInformationView.OnEndChangingName -= EndChangingName;
        entityInformationView.OnDisable -= Disable;
    }

    public void PositionChanged(Vector3 pos) { OnPositionChange?.Invoke(pos); }

    public void RotationChanged(Vector3 rot)
    {
        currentEntity?.SetRotation(rot);
        OnRotationChange?.Invoke(rot);
    }

    public void ScaleChanged(Vector3 scale) { OnScaleChange?.Invoke(scale); }

    public void NameChanged(BIWEntity entity, string name) { OnNameChange?.Invoke(entity, name); }

    public void ToggleDetailsInfo() { entityInformationView.ToggleDetailsInfo(); }

    public void ToggleBasicInfo() { entityInformationView.ToggleBasicInfo(); }

    public void StartChangingName() { isChangingName = true; }

    public void EndChangingName() { isChangingName = false; }

    public void SetEntity(BIWEntity entity, IParcelScene currentScene)
    {
        currentEntity = entity;
        EntityDeselected();
        entityInformationView.SetCurrentEntity(entity);

        if (entityInformationView.currentEntity != null)
        {
            entity.OnStatusUpdate -= UpdateEntityName;
            entityInformationView.currentEntity.OnStatusUpdate += UpdateEntityName;
        }

        parcelScene = currentScene;

        if (entity.HasSmartItemComponent())
        {
            entityInformationView.SetSmartItemListViewActive(false);
            //TODO: Remove this comment when we implement smart items in builder in world
            //if (entity.rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out IEntityComponent baseComponent))
            //   entityInformationView.smartItemList.SetSmartItemParameters(entity.GetSmartItemParameters(), ((SmartItemComponent) baseComponent).GetValues());
        }
        else
        {
            entityInformationView.SetSmartItemListViewActive(false);
        }

        entityInformationView.SetEntityThumbnailEnable(false);
        CatalogItem entitySceneObject = entity.GetCatalogItemAssociated();
        GetThumbnail(entitySceneObject);
        UpdateLimitsInformation(entitySceneObject);
        UpdateEntityName(entityInformationView.currentEntity);
        UpdateInfo(entityInformationView.currentEntity);
    }

    internal void GetThumbnail(CatalogItem catalogItem)
    {
        if (catalogItem == null)
            return;

        var url = catalogItem.thumbnailURL;

        if (string.IsNullOrEmpty(url))
            return;

        var newLoadedThumbnailPromise = new AssetPromise_Texture(url);
        newLoadedThumbnailPromise.OnSuccessEvent += SetThumbnail;
        newLoadedThumbnailPromise.OnFailEvent += (x, error) => { Debug.Log($"Error downloading: {url}, Exception: {error}"); };
        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);
        AssetPromiseKeeper_Texture.i.Forget(loadedThumbnailPromise);
        loadedThumbnailPromise = newLoadedThumbnailPromise;
    }

    internal void SetThumbnail(Asset_Texture texture)
    {
        entityInformationView.SetEntityThumbnailEnable(true);
        entityInformationView.SetEntityThumbnailTexture(texture.texture);
    }

    internal void UpdateEntityName(BIWEntity entity)
    {
        if (entity == null)
            return;

        string currentName = entity.GetDescriptiveName();

        if (!isChangingName)
            entityInformationView.SetNameIFText(currentName);
    }

    internal void UpdateLimitsInformation(CatalogItem catalogItem)
    {
        if (catalogItem == null)
        {
            entityInformationView.SeEntityLimitsText("", "", "");
            return;
        }

        string trisText = string.Format(TRIS_TEXT_FORMAT, catalogItem.metrics.triangles);
        string materialText = string.Format(MATERIALS_TEXT_FORMAT, catalogItem.metrics.materials);
        string textureText = string.Format(TEXTURES_TEXT_FORMAT, catalogItem.metrics.textures);

        entityInformationView.SeEntityLimitsText(trisText, materialText, textureText);
    }

    public void Enable() { entityInformationView.SetActive(true); }

    public void Disable()
    {
        entityInformationView.SetActive(false);
        EntityDeselected();
        entityInformationView.SetCurrentEntity(null);
        OnDisable?.Invoke();
    }

    internal void EntityDeselected()
    {
        if (entityInformationView.currentEntity == null)
            return;

        var scene = entityInformationView.currentEntity.rootEntity.scene;
        if (scene.componentsManagerLegacy.TryGetBaseComponent(entityInformationView.currentEntity.rootEntity, CLASS_ID_COMPONENT.SMART_ITEM, out IEntityComponent component))
        {
            SmartItemComponent smartItemComponent = (SmartItemComponent) component;
            OnSmartItemComponentUpdate?.Invoke(entityInformationView.currentEntity);
        }
    }

    public void UpdateInfo(BIWEntity entity)
    {
        if (entity != null && entity.rootEntity.gameObject != null)
        {
            Vector3 positionConverted = WorldStateUtils.ConvertUnityToScenePosition(entity.rootEntity.gameObject.transform.position, parcelScene);
            Vector3 currentRotation = entity.rootEntity.gameObject.transform.rotation.eulerAngles;
            Vector3 currentScale = entity.rootEntity.gameObject.transform.lossyScale;

            currentRotation = entity.GetEulerRotation();

            entityInformationView.SetPositionAttribute(positionConverted);
            entityInformationView.SetRotationAttribute(currentRotation);
            entityInformationView.SetScaleAttribute(currentScale);
        }
    }

    public void UpdateEntitiesSelection(int numberOfSelectedEntities) { entityInformationView.UpdateEntitiesSelection(numberOfSelectedEntities); }

    public void SetTransparencyMode(bool isOn) { entityInformationView.SetTransparencyMode(isOn ? TRANSPARENCY_MODE_ALPHA_VALUE : 1f, !isOn); }
}