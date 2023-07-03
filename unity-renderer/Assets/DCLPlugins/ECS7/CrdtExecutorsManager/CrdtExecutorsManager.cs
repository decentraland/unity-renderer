using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using RPC.Context;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CrdtExecutorsManager : IDisposable
{
    private readonly Dictionary<int, ICRDTExecutor> crdtExecutors;
    private readonly ISceneController sceneController;
    private readonly ECSComponentsManager componentsManager;
    private readonly CRDTServiceContext rpcCrdtServiceContext;

    private int cachedSceneNumber;
    private ICRDTExecutor cachedCrdtExecutor;

    public CrdtExecutorsManager(Dictionary<int, ICRDTExecutor> crdtExecutors,
        ECSComponentsManager componentsManager, ISceneController sceneController,
        CRDTServiceContext rpcCrdtServiceContext)
    {
        this.crdtExecutors = crdtExecutors;
        this.sceneController = sceneController;
        this.componentsManager = componentsManager;
        this.rpcCrdtServiceContext = rpcCrdtServiceContext;

        sceneController.OnSceneRemoved += OnSceneRemoved;
        sceneController.OnNewSceneAdded += OnSceneAdded;
        rpcCrdtServiceContext.CrdtMessageReceived += CrdtMessageReceived;
    }

    public void Dispose()
    {
        sceneController.OnSceneRemoved -= OnSceneRemoved;
        sceneController.OnNewSceneAdded -= OnSceneAdded;
        rpcCrdtServiceContext.CrdtMessageReceived -= CrdtMessageReceived;

        foreach (ICRDTExecutor crdtExecutor in crdtExecutors.Values)
        {
            crdtExecutor.Dispose();
        }

        crdtExecutors.Clear();
    }

    private void OnSceneRemoved(IParcelScene scene)
    {
        int sceneNumber = scene.sceneData.sceneNumber;

        if (crdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor crdtExecutor))
        {
            crdtExecutor.Dispose();
            crdtExecutors.Remove(sceneNumber);
        }

        if (cachedSceneNumber == sceneNumber)
        {
            cachedSceneNumber = -1;
        }
    }

    private void OnSceneAdded(IParcelScene scene)
    {
        if (!scene.sceneData.sdk7)
            return;

        CRDTExecutor executor = new CRDTExecutor(scene, componentsManager);
        executor.GenerateInitialEntities();
        crdtExecutors[scene.sceneData.sceneNumber] = executor;
    }

    private ICRDTExecutor GetCachedExecutor(int sceneNumber)
    {
        if (cachedSceneNumber != sceneNumber)
        {
            cachedCrdtExecutor = null;
            cachedSceneNumber = -1;

            if (crdtExecutors.TryGetValue(sceneNumber, out cachedCrdtExecutor))
            {
                cachedSceneNumber = sceneNumber;
            }
        }

        return cachedCrdtExecutor;
    }

    private void CrdtMessageReceived(int sceneNumber, CrdtMessage crdtMessage)
    {
        ICRDTExecutor executor = GetCachedExecutor(sceneNumber);

        if (executor != null)
        {
            executor.Execute(crdtMessage);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogError($"CrdtExecutor not found for sceneNumber {sceneNumber}");
        }
#endif
    }
}
