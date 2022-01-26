using System;
using System.Collections;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;
using Object = UnityEngine.Object;

public class LoadWrapperShould : IntegrationTestSuite
{
    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
    }

    [UnityTest]
    public IEnumerator UnloadWhenEntityDestroyedBeforeFinishing()
    {
        GameObject meshRootGameObject = new GameObject();

        string url = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb";

        IDCLEntity entity = Substitute.For<IDCLEntity>();
        entity.meshRootGameObject.Returns(meshRootGameObject);

        LoadWrapper_GLTF wrapper = Substitute.ForPartsOf<LoadWrapper_GLTF>();
        wrapper.entity = entity;
        wrapper.customContentProvider = new ContentProvider();

        bool loaded = false;
        bool failed = false;
        bool unloaded = false;

        wrapper.WhenForAnyArgs(x => x.Unload()).Do((info) => unloaded = true);

        wrapper.Load(url, loadWrapper => loaded = true, (loadWrapper, error) => failed = true );

        entity.OnCleanupEvent?.Invoke(entity);

        yield return new WaitUntil(() => loaded || failed || unloaded);

        Object.Destroy(meshRootGameObject);

        Assert.IsTrue(unloaded, "Unload should be called if entity is cleaned up while loading mesh");
    }
}