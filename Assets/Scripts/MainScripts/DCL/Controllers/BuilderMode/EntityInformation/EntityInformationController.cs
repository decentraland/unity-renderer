using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityInformationController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite openMenuSprite;
    public Sprite closeMenuSprite;

    [Header("Prefab references")]
    public TextMeshProUGUI titleTxt;
    public TextMeshProUGUI entityLimitsLeftTxt;
    public TextMeshProUGUI entityLimitsRightTxt;
    public TMP_InputField nameIF;
    public RawImage entitytTumbailImg; 
    public AttributeXYZ positionAttribute;
    public AttributeXYZ rotationAttribute;
    public AttributeXYZ scaleAttribute;
    public GameObject detailsGO;
    public GameObject basicsGO;
    public Image detailsToggleBtn;
    public Image basicToggleBtn;
    public SmartItemListView smartItemListView;


    public event Action<Vector3> OnPositionChange;
    public event Action<Vector3> OnRotationChange;
    public event Action<Vector3> OnScaleChange;

    public event Action<DCLBuilderInWorldEntity, string> OnNameChange;

    DCLBuilderInWorldEntity currentEntity;
    ParcelScene parcelScene;

    bool isEnable = false;
    bool isChangingName = false;

    int framesBetweenUpdate = 5;
    int framesCount = 0;

    string loadedThumbnailURL;

    AssetPromise_Texture loadedThumbnailPromise;

    private void Start()
    {
        positionAttribute.OnChanged += (x) => OnPositionChange?.Invoke(x);
        rotationAttribute.OnChanged += (x) => OnRotationChange?.Invoke(x);
        scaleAttribute.OnChanged += (x) => OnScaleChange?.Invoke(x);
    }

    private void LateUpdate()
    {
        if (!isEnable)
            return;

        if (currentEntity == null)
            return;

        if (framesCount >= framesBetweenUpdate)
        {
            UpdateInfo(currentEntity);
            framesCount = 0;
        }
        else
        {
            framesCount++;
        }
    }

    public void ToggleDetailsInfo()
    {
        detailsGO.SetActive(!detailsGO.activeSelf);

        detailsToggleBtn.sprite = detailsGO.activeSelf ? openMenuSprite : closeMenuSprite;
    }

    public void ToggleBasicInfo()
    {
        basicsGO.SetActive(!basicsGO.activeSelf);

        basicToggleBtn.sprite = basicsGO.activeSelf ? openMenuSprite : closeMenuSprite;
    }

    public void StartChangingName()
    {
        isChangingName = true;
    }

    public void EndChangingName()
    {
        isChangingName = false;
    }

    public void ChangeEntityName(string newName)
    {      
        OnNameChange?.Invoke(currentEntity,newName);
    }

    public void SetEntity(DCLBuilderInWorldEntity entity, ParcelScene currentScene)
    {
        if (currentEntity != null)
            entity.onStatusUpdate -= UpdateEntityName;

        currentEntity = entity;
        currentEntity.onStatusUpdate += UpdateEntityName;

        parcelScene = currentScene;

        if (entity.HasSmartItemComponent())
        {
            entity.rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out BaseComponent baseComponent);
            smartItemListView.SetSmartItemParameters((SmartItemComponent) baseComponent);
        }
        else
        {
            smartItemListView.gameObject.SetActive(false);
        }

        entitytTumbailImg.enabled = false;

        SceneObject entitySceneObject = entity.GetSceneObjectAssociated();

        GetThumbnail(entitySceneObject);

        UpdateLimitsInformation(entitySceneObject);
        UpdateEntityName(currentEntity);
        UpdateInfo(currentEntity);
    }

    public void Enable()
    {
        gameObject.SetActive(true);
        isEnable = true;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        isEnable = false;
    }

    public void UpdateEntityName(DCLBuilderInWorldEntity entity)
    {
        string currentName = entity.GetDescriptiveName();
        titleTxt.text = currentName;

        if (!isChangingName)
            nameIF.SetTextWithoutNotify(currentName);
    }

    public void UpdateInfo(DCLBuilderInWorldEntity entity)
    {
        if (entity.gameObject != null)
        {
            Vector3 positionConverted = DCL.Environment.i.world.state.ConvertUnityToScenePosition(entity.gameObject.transform.position, parcelScene);
            Vector3 currentRotation = entity.gameObject.transform.rotation.eulerAngles;
            Vector3 currentScale = entity.gameObject.transform.localScale;

            var newEuler = currentRotation;

            newEuler.x = RepeatWorking(newEuler.x - currentRotation.x + 180.0F, 360.0F) + currentRotation.x - 180.0F;
            newEuler.y = RepeatWorking(newEuler.y - currentRotation.y + 180.0F, 360.0F) + currentRotation.y - 180.0F;
            newEuler.z = RepeatWorking(newEuler.z - currentRotation.z + 180.0F, 360.0F) + currentRotation.z - 180.0F;

            currentRotation = newEuler;

            positionAttribute.SetValues(positionConverted);
            rotationAttribute.SetValues(currentRotation);
            scaleAttribute.SetValues(currentScale);
            
        }
    }

    void UpdateLimitsInformation(SceneObject sceneObject)
    {
        if (sceneObject == null)
        {
            entityLimitsLeftTxt.text = "";
            entityLimitsRightTxt.text = "";
            return;
        }

        string leftText = $"ENTITIES: {sceneObject.metrics.entities}\n"+ 
                          $"BODIES: {sceneObject.metrics.bodies}\n" + 
                          $"TRIS: {sceneObject.metrics.triangles}";

        string rightText = $"TEXTURES: {sceneObject.metrics.textures}\n" +
                           $"MATERIALS: {sceneObject.metrics.materials}\n" +
                           $"GEOMETRIES: {sceneObject.metrics.meshes}";

         entityLimitsLeftTxt.text = leftText;
         entityLimitsRightTxt.text = rightText;
    }


    private float RepeatWorking(float t, float length)
    {
        return (t - (Mathf.Floor(t / length) * length));
    }

    private void GetThumbnail(SceneObject sceneObject)
    {
        var url = sceneObject?.GetComposedThumbnailUrl();

        if (sceneObject == null || string.IsNullOrEmpty(url))
            return;

        if (string.Equals(sceneObject.asset_pack_id, BuilderInWorldSettings.ASSETS_COLLECTIBLES))
            url = sceneObject.thumbnail;

        string newLoadedThumbnailURL = url;
        var newLoadedThumbnailPromise = new AssetPromise_Texture(url);


        newLoadedThumbnailPromise.OnSuccessEvent += SetThumbnail;
        newLoadedThumbnailPromise.OnFailEvent += x => { Debug.Log($"Error downloading: {url}"); };

        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);


        AssetPromiseKeeper_Texture.i.Forget(loadedThumbnailPromise);

        loadedThumbnailPromise = newLoadedThumbnailPromise;
        loadedThumbnailURL = newLoadedThumbnailURL;
    }

    public void SetThumbnail(Asset_Texture texture)
    {
        if (entitytTumbailImg != null)
        {
            entitytTumbailImg.enabled = true;
            entitytTumbailImg.texture = texture.texture;
        }
    }
}