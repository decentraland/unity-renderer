using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.FPSDisplay;

public class SceneDebugMetricModule : IDebugMetricModule
{

    private const string TWO_DECIMALS = "##.00";
    
    private string sceneID;
    private string sceneComponents;
    private SceneMetricsModel metrics;
    private SceneMetricsModel limits;
    private IParcelScene activeScene;
    private int totalMessagesCurrent;
    private int totalMessagesGlobal;
    
    
    public void SetUpModule(Dictionary<DebugValueEnum, Func<string>> updateValueDictionary)
    {
        updateValueDictionary.Add(DebugValueEnum.Scene_Name, () => sceneID);
        updateValueDictionary.Add(DebugValueEnum.Scene_ProcessedMessages, () =>  $"{((float)totalMessagesCurrent / totalMessagesGlobal * 100).ToString(TWO_DECIMALS)}%");
        updateValueDictionary.Add(DebugValueEnum.Scene_PendingOnQueue, () =>  $"{totalMessagesGlobal - totalMessagesCurrent}");
        updateValueDictionary.Add(DebugValueEnum.Scene_Poly, () => GetSceneMetric(metrics.triangles, limits.triangles));
        updateValueDictionary.Add(DebugValueEnum.Scene_Textures, () => GetSceneMetric(metrics.textures,limits.textures));
        updateValueDictionary.Add(DebugValueEnum.Scene_Materials, () => GetSceneMetric(metrics.materials,limits.materials));
        updateValueDictionary.Add(DebugValueEnum.Scene_Entities, () =>GetSceneMetric(metrics.entities,limits.entities));
        updateValueDictionary.Add(DebugValueEnum.Scene_Meshes, () => GetSceneMetric(metrics.meshes, limits.meshes));
        updateValueDictionary.Add(DebugValueEnum.Scene_Bodies, () => GetSceneMetric(metrics.bodies,limits.bodies));
        updateValueDictionary.Add(DebugValueEnum.Scene_Components, () => sceneComponents);
    }


    public void UpdateModule()
    {
        activeScene = GetActiveScene();
        if (activeScene != null && activeScene.metricsCounter != null)
        {
            metrics = activeScene.metricsCounter.currentCount;
            limits = activeScene.metricsCounter.maxCount;
            SetSceneData();
        }
    }
    public void EnableModule()
    {
        ProfilingEvents.OnMessageWillQueue += OnMessageWillQueue;
        ProfilingEvents.OnMessageWillDequeue += OnMessageWillDequeue;
    }
    public void DisableModule()
    {
        ProfilingEvents.OnMessageWillQueue -= OnMessageWillQueue;
        ProfilingEvents.OnMessageWillDequeue -= OnMessageWillDequeue;
    }

    private void SetSceneData()
    {
        sceneID = activeScene.sceneData.id;
        if (sceneID.Length >= 11)
        {
            sceneID = $"{sceneID.Substring(0, 5)}...{sceneID.Substring(sceneID.Length - 5, 5)}";
        }
        sceneComponents = (activeScene.componentsManagerLegacy.GetSceneSharedComponentsDictionary().Count + activeScene.componentsManagerLegacy.GetComponentsCount()).ToString();
    }
    
    private string GetSceneMetric(int value, int limit)
    {
        return $"{FPSColoring.GetPercentageColoringString(value, limit)}current: {value} max: {limit}</color>";
    }
    
    private void OnMessageWillDequeue(string obj)
    {
        totalMessagesCurrent = Math.Min(totalMessagesCurrent + 1, totalMessagesGlobal);
    }

    private void OnMessageWillQueue(string obj)
    {
        totalMessagesGlobal++;
    }
    
    private IParcelScene GetActiveScene()
    {
        IWorldState worldState = DCL.Environment.i.world.state;
        int debugSceneNumber = KernelConfig.i.Get().debugConfig.sceneDebugPanelTargetSceneNumber;

        if (debugSceneNumber > 0)
        {
            if (worldState.TryGetScene(debugSceneNumber, out IParcelScene scene))
                return scene;
        }

        var currentPos = DataStore.i.player.playerGridPosition.Get();
        worldState.TryGetScene(worldState.GetSceneNumberByCoords(currentPos), out IParcelScene resultScene);

        return resultScene;
    }
    
    public string GetSceneID()
    {
        return activeScene.sceneData.id;
    }
    
    public void Dispose()
    {
        ProfilingEvents.OnMessageWillQueue -= OnMessageWillQueue;
        ProfilingEvents.OnMessageWillDequeue -= OnMessageWillDequeue;
    }

}
