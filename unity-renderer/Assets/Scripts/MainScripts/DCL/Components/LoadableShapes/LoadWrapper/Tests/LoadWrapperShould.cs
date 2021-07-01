using System;
using System.Collections;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;
using Object = UnityEngine.Object;

public class LoadWrapperShould : IntegrationTestSuite
{
    protected override PlatformContext CreatePlatformContext() { return DCL.Tests.PlatformContextFactory.CreateWithGenericMocks(WebRequestController.Create()); }

    [UnityTest]
    public IEnumerator UnloadWhenEntityDestroyedBeforeFinishing()
    {
        GameObject meshRootGameObject = new GameObject();

        string url = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";

        IDCLEntity entity = Substitute.For<IDCLEntity>();
        entity.meshRootGameObject.Returns(meshRootGameObject);

        LoadWrapper_GLFT_Overload wrapper = new LoadWrapper_GLFT_Overload();
        wrapper.entity = entity;
        wrapper.customContentProvider = new ContentProvider();

        bool loaded = false;
        bool failed = false;
        bool unloaded = false;

        wrapper.OnUnload += () => unloaded = true;

        wrapper.Load(url, loadWrapper => loaded = true, loadWrapper => failed = true );

        entity.OnCleanupEvent?.Invoke(entity);

        yield return new WaitUntil(() => loaded || failed || unloaded);

        Object.Destroy(meshRootGameObject);

        Assert.IsTrue(unloaded, "Unload should be called if entity is cleaned up while loading mesh");
    }

    class LoadWrapper_GLFT_Overload : LoadWrapper_GLTF
    {
        public event Action OnUnload;

        public override void Unload()
        {
            base.Unload();
            OnUnload?.Invoke();
        }
    }
}