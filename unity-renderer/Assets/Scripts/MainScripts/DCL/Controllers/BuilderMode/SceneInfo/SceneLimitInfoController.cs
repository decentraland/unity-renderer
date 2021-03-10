using DCL;
using DCL.Configuration;
using DCL.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneLimitInfoController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite openMenuSprite;
    public Sprite closeMenuSprite;

    [Header("Design")]
    public Color lowFillColor;
    public Color mediumFillColor;
    public Color highFillColor;

    [Header("Scene references")]

    public Image detailsToggleBtn;
    public GameObject sceneLimitsBodyGO;
    public TextMeshProUGUI titleTxt;
    public TextMeshProUGUI leftDescTxt, rightDescTxt;
    public Image[] limitUsageFillsImgs;

    ParcelScene currentParcelScene;

    const int FRAMES_BETWEEN_UPDATES = 15;

    int infoDelayCont = 0;

    private void Update()
    {
        if(infoDelayCont >= FRAMES_BETWEEN_UPDATES)
        {
            infoDelayCont = 0;
            UpdateInfo();
        }
        else
        {
            infoDelayCont++;
        }
    }

    public void SetParcelScene(ParcelScene parcelScene)
    {
        currentParcelScene = parcelScene;
        UpdateInfo();
    }

    public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    public void ToggleSceneLimitsInfo()
    {
        if (!sceneLimitsBodyGO.activeSelf)
            Enable();
        else
            Disable();
    }

    public void Enable()
    {
        sceneLimitsBodyGO.SetActive(true);
        detailsToggleBtn.sprite = openMenuSprite;
        UpdateInfo();
    }

    public void Disable()
    {
        sceneLimitsBodyGO.SetActive(false);

        detailsToggleBtn.sprite = closeMenuSprite;
    }

    public void UpdateInfo()
    {
        if (IsParcelSceneSquare(currentParcelScene))
        {
            int size = (int)Math.Sqrt(currentParcelScene.sceneData.parcels.Length);
            int meters = size * 16;
            titleTxt.text = $"{size}x{size} LAND <color=#959696>{meters}x{meters}m";
        }
        else
        {
            titleTxt.text = BuilderInWorldSettings.CUSTOM_LAND;
        }


        SceneMetricsController.Model limits = currentParcelScene.metricsController.GetLimits();
        SceneMetricsController.Model usage = currentParcelScene.metricsController.GetModel();

        string leftDesc = AppendUsageAndLimit("ENTITIES", usage.entities, limits.entities);
        leftDesc += "\n" + AppendUsageAndLimit("BODIES", usage.bodies, limits.bodies);
        leftDesc += "\n" + AppendUsageAndLimit("TRIS", usage.triangles, limits.triangles);
        string rightDesc = AppendUsageAndLimit("TEXTURES", usage.textures, limits.textures);
        rightDesc += "\n" + AppendUsageAndLimit("MATERIALS", usage.materials, limits.materials);
        rightDesc += "\n" + AppendUsageAndLimit("GEOMETRIES", usage.meshes, limits.meshes);
      
        
       
        leftDescTxt.text = leftDesc;
        rightDescTxt.text = rightDesc;
        SetFillInfo();
    }

    void SetFillInfo()
    {
        float percentAmount = GetHigherLimitPercentInfo();
        Color colorToUse = lowFillColor;
        if (percentAmount > 66)
            colorToUse = mediumFillColor; 
        if (percentAmount > 85)
            colorToUse = highFillColor; 
        foreach(Image img in limitUsageFillsImgs)
        {
            if (img == null)
                continue;
            img.fillAmount = percentAmount/100f;
            img.color = colorToUse;
        }
    }

    string AppendUsageAndLimit(string name, int usage, int limit)
    {
        string currentString = $"{name}:   {usage} / <color=#959696>{limit}</color>";
        if (usage >= limit)
            currentString = "<color=red>" + currentString + "</color>";
        return currentString;
    }

    float GetHigherLimitPercentInfo()
    {
        SceneMetricsController.Model limits = currentParcelScene.metricsController.GetLimits();
        SceneMetricsController.Model usage = currentParcelScene.metricsController.GetModel();


        float percentEntities = usage.entities*100/ limits.entities;
        float percentBodies = usage.bodies*100/ limits.bodies;
        float percentTris = usage.triangles*100/ limits.triangles;
        float percentTexture = usage.textures*100/ limits.textures;
        float percentmats = usage.materials*100/ limits.materials;
        float percentMeshes = usage.meshes*100/ limits.meshes;

        float result = percentEntities;
        if (percentBodies > result)
            result = percentBodies;
        if (percentTris > result)
            result = percentTris;
        if (percentTexture > result)
            result = percentTexture;
        if (percentmats > result)
            result = percentmats;
        if (percentMeshes > result)
            result = percentMeshes;

        return result;
    }

    bool IsParcelSceneSquare(ParcelScene scene)
    {
        Vector2Int[] parcelsPoints = scene.sceneData.parcels;
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach(Vector2Int vector in parcelsPoints)
        {
            if (vector.x < minX) minX = vector.x;
            if (vector.y < minY) minY = vector.y;
            if (vector.x > maxX) maxX = vector.x;
            if (vector.y > maxY) maxY = vector.y;
        }

        if(maxX - minX != maxY - minY)
            return false;

        int lateralLengh = Math.Abs((maxX - minX) + 1);
        if (parcelsPoints.Length != lateralLengh * lateralLengh)
            return false;
        
        return true;
    }
}
