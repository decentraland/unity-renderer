using DCL;
using NUnit.Framework;
using UnityEngine;

public class SceneMetricsCounterShould
{
    [Test]
    public void CountAnimationClipMemoryWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, 1, Vector2Int.zero, 10);

        dataStore.sceneData.Add(1, new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add(2, new DataStore_WorldObjects.SceneData());
        sceneMetricsCounter.Enable();

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = 0;
        rendereable1.animationClipSize = 256;

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = 0;
        rendereable2.animationClipSize = 512;

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = 0;
        rendereable3.animationClipSize = 1024;

        dataStore.AddRendereable(1, rendereable1);
        dataStore.AddRendereable(1, rendereable2);
        dataStore.AddRendereable(2, rendereable3);

        Assert.That(sceneMetricsCounter.currentCount.animationClipMemoryScore, Is.EqualTo(768));

        dataStore.RemoveRendereable(1, rendereable2);

        Assert.That(sceneMetricsCounter.currentCount.animationClipMemoryScore, Is.EqualTo(256));

        dataStore.RemoveRendereable(1, rendereable1);

        Assert.That(sceneMetricsCounter.currentCount.animationClipMemoryScore, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();
    }

    [Test]
    public void CountAudioClipMemoryWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, 1, Vector2Int.zero, 10);

        dataStore.sceneData.Add(1, new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add(2, new DataStore_WorldObjects.SceneData());
        sceneMetricsCounter.Enable();

        var audioClip1 = AudioClip.Create("Test1", 10000, 2, 44250, false, null, null);
        var audioClip2 = AudioClip.Create("Test2", 10000, 2, 44250, false, null, null);
        var audioClip3 = AudioClip.Create("Test3", 10000, 2, 44250, false, null, null);

        dataStore.AddAudioClip(1, audioClip1);
        dataStore.AddAudioClip(1, audioClip2);
        dataStore.AddAudioClip(2, audioClip3);

        Assert.That(sceneMetricsCounter.currentCount.audioClipMemoryScore, Is.EqualTo(86000));

        dataStore.RemoveAudioClip(1, audioClip1);

        Assert.That(sceneMetricsCounter.currentCount.audioClipMemoryScore, Is.EqualTo(43000));

        dataStore.RemoveAudioClip(1, audioClip2);

        Assert.That(sceneMetricsCounter.currentCount.audioClipMemoryScore, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        Object.Destroy(audioClip1);
        Object.Destroy(audioClip2);
        Object.Destroy(audioClip3);
    }

    [Test]
    public void CountMeshesMemoryWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, 1, Vector2Int.zero, 10);

        dataStore.sceneData.Add(1, new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add(2, new DataStore_WorldObjects.SceneData());
        sceneMetricsCounter.Enable();

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = 0;
        rendereable1.meshDataSize = 1024;

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = 0;
        rendereable2.meshDataSize = 1024;

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = 0;
        rendereable3.meshDataSize = 1024;

        dataStore.AddRendereable(1, rendereable1);
        dataStore.AddRendereable(1, rendereable2);
        dataStore.AddRendereable(2, rendereable3);

        Assert.That(sceneMetricsCounter.currentCount.meshMemoryScore, Is.EqualTo(2048));

        dataStore.RemoveRendereable(1, rendereable2);

        Assert.That(sceneMetricsCounter.currentCount.meshMemoryScore, Is.EqualTo(1024));

        dataStore.RemoveRendereable(1, rendereable1);

        Assert.That(sceneMetricsCounter.currentCount.meshMemoryScore, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();
    }

    [Test]
    public void CountTexturesMemoryWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, 1, Vector2Int.zero, 10);

        dataStore.sceneData.Add(1, new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add(2, new DataStore_WorldObjects.SceneData());
        sceneMetricsCounter.Enable();

        Texture2D tex1 = new Texture2D(128, 128);
        Texture2D tex2 = new Texture2D(64, 64);
        Texture2D tex3 = new Texture2D(32, 32);
        Texture2D tex4 = new Texture2D(16, 16);

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = 0;
        rendereable1.textures.Add(tex1);
        rendereable1.textures.Add(tex2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = 1;
        rendereable2.textures.Add(tex3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = 2;
        rendereable3.textures.Add(tex4);

        dataStore.AddRendereable(1, rendereable1);
        dataStore.AddRendereable(1, rendereable2);
        dataStore.AddRendereable(2, rendereable3);

        Assert.That(sceneMetricsCounter.currentCount.textureMemoryScore, Is.EqualTo(120421));

        dataStore.RemoveRendereable(1, rendereable2);

        Assert.That(sceneMetricsCounter.currentCount.textureMemoryScore, Is.EqualTo(114687));

        dataStore.RemoveRendereable(1, rendereable1);

        Assert.That(sceneMetricsCounter.currentCount.textureMemoryScore, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        UnityEngine.Object.Destroy(tex1);
        UnityEngine.Object.Destroy(tex2);
        UnityEngine.Object.Destroy(tex3);
        UnityEngine.Object.Destroy(tex4);
    }

    [Test]
    public void CountEntitiesWhenAddedAndRemoved()
    {
        var dataStore = new DataStore_WorldObjects();
        dataStore.sceneData.Add(1, new DataStore_WorldObjects.SceneData());

        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, 1, Vector2Int.zero, 10);
        sceneMetricsCounter.Enable();

        var sceneData = dataStore.sceneData[1];

        sceneData.owners.Add(1);

        Assert.That( sceneMetricsCounter.currentCount.entities, Is.EqualTo(1));

        sceneData.owners.Add(2);
        sceneData.owners.Add(3);
        sceneData.owners.Add(4);
        sceneData.owners.Add(5);
        sceneData.owners.Add(6);
        sceneData.owners.Add(7);
        sceneData.owners.Add(8);
        sceneData.owners.Add(9);
        sceneData.owners.Add(10);

        Assert.That( sceneMetricsCounter.currentCount.entities, Is.EqualTo(10));

        sceneData.owners.Remove(9);

        Assert.That( sceneMetricsCounter.currentCount.entities, Is.EqualTo(9));

        sceneMetricsCounter.Dispose();
    }

    [Test]
    public void CountUniqueMeshesWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, 1, Vector2Int.zero, 10);

        dataStore.sceneData.Add(1, new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add(2, new DataStore_WorldObjects.SceneData());
        sceneMetricsCounter.Enable();

        Mesh mesh1 = new Mesh();
        Mesh mesh2 = new Mesh();
        Mesh mesh3 = new Mesh();
        Mesh mesh4 = new Mesh();

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = 0;
        rendereable1.meshes.Add(mesh1);
        rendereable1.meshes.Add(mesh2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = 0;
        rendereable2.meshes.Add(mesh3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = 0;
        rendereable3.meshes.Add(mesh4);

        dataStore.AddRendereable(1, rendereable1);
        dataStore.AddRendereable(1, rendereable2);
        dataStore.AddRendereable(2, rendereable3);

        Assert.That( sceneMetricsCounter.currentCount.meshes, Is.EqualTo(3));

        dataStore.RemoveRendereable(1, rendereable2);

        Assert.That( sceneMetricsCounter.currentCount.meshes, Is.EqualTo(2));

        dataStore.RemoveRendereable(1, rendereable1);

        Assert.That( sceneMetricsCounter.currentCount.meshes, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        UnityEngine.Object.Destroy(mesh1);
        UnityEngine.Object.Destroy(mesh2);
        UnityEngine.Object.Destroy(mesh3);
        UnityEngine.Object.Destroy(mesh4);
    }

    [Test]
    public void CountUniqueMaterialsWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, 1, Vector2Int.zero, 10);

        dataStore.sceneData.Add(1, new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add(2, new DataStore_WorldObjects.SceneData());
        sceneMetricsCounter.Enable();

        Material mat1 = new Material(Shader.Find("Standard"));
        Material mat2 = new Material(Shader.Find("Standard"));
        Material mat3 = new Material(Shader.Find("Standard"));
        Material mat4 = new Material(Shader.Find("Standard"));

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = 0;
        rendereable1.materials.Add(mat1);
        rendereable1.materials.Add(mat2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = 0;
        rendereable2.materials.Add(mat3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = 0;
        rendereable3.materials.Add(mat4);

        dataStore.AddRendereable(1, rendereable1);
        dataStore.AddRendereable(1, rendereable2);
        dataStore.AddRendereable(2, rendereable3);

        Assert.That( sceneMetricsCounter.currentCount.materials, Is.EqualTo(3));

        dataStore.RemoveRendereable(1, rendereable2);

        Assert.That( sceneMetricsCounter.currentCount.materials, Is.EqualTo(2));

        dataStore.RemoveRendereable(1, rendereable1);

        Assert.That( sceneMetricsCounter.currentCount.materials, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        UnityEngine.Object.Destroy(mat1);
        UnityEngine.Object.Destroy(mat2);
        UnityEngine.Object.Destroy(mat3);
        UnityEngine.Object.Destroy(mat4);
    }

    [Test]
    public void CountUniqueTexturesWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, 1, Vector2Int.zero, 10);

        dataStore.sceneData.Add(1, new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add(2, new DataStore_WorldObjects.SceneData());
        sceneMetricsCounter.Enable();

        Texture2D tex1 = new Texture2D(1, 1);
        Texture2D tex2 = new Texture2D(1, 1);
        Texture2D tex3 = new Texture2D(1, 1);
        Texture2D tex4 = new Texture2D(1, 1);

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = 0;
        rendereable1.textures.Add(tex1);
        rendereable1.textures.Add(tex2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = 0;
        rendereable2.textures.Add(tex3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = 0;
        rendereable3.textures.Add(tex4);

        dataStore.AddRendereable(1, rendereable1);
        dataStore.AddRendereable(1, rendereable2);
        dataStore.AddRendereable(2, rendereable3);

        Assert.That( sceneMetricsCounter.currentCount.textures, Is.EqualTo(3));

        dataStore.RemoveRendereable(1, rendereable2);

        Assert.That( sceneMetricsCounter.currentCount.textures, Is.EqualTo(2));

        dataStore.RemoveRendereable(1, rendereable1);

        Assert.That( sceneMetricsCounter.currentCount.textures, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        UnityEngine.Object.Destroy(tex1);
        UnityEngine.Object.Destroy(tex2);
        UnityEngine.Object.Destroy(tex3);
        UnityEngine.Object.Destroy(tex4);
    }

    [Test]
    public void CountTotalTrianglesWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, 1, Vector2Int.zero, 10);

        dataStore.sceneData.Add(1, new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add(2, new DataStore_WorldObjects.SceneData());
        sceneMetricsCounter.Enable();

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = 0;
        rendereable1.totalTriangleCount = 30;

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = 0;
        rendereable2.totalTriangleCount = 60;

        dataStore.AddRendereable(1, rendereable1);
        dataStore.AddRendereable(1, rendereable2);

        Assert.That( sceneMetricsCounter.currentCount.triangles, Is.EqualTo(30));

        dataStore.RemoveRendereable(1, rendereable1);

        Assert.That( sceneMetricsCounter.currentCount.triangles, Is.EqualTo(20));

        sceneMetricsCounter.Dispose();
    }

    [Test]
    public void CountBodiesWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, 1, Vector2Int.zero, 10);
        dataStore.sceneData.Add(1, new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add(2, new DataStore_WorldObjects.SceneData());

        sceneMetricsCounter.Enable();

        Renderer rend1 = new GameObject("Test").AddComponent<MeshRenderer>();
        Renderer rend2 = new GameObject("Test").AddComponent<MeshRenderer>();
        Renderer rend3 = new GameObject("Test").AddComponent<MeshRenderer>();
        Renderer rend4 = new GameObject("Test").AddComponent<MeshRenderer>();

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = 0;
        rendereable1.renderers.Add(rend1);
        rendereable1.renderers.Add(rend2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = 0;
        rendereable2.renderers.Add(rend3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = 0;
        rendereable3.renderers.Add(rend4);

        dataStore.AddRendereable(1, rendereable1);
        dataStore.AddRendereable(1, rendereable2);
        dataStore.AddRendereable(2, rendereable3);

        Assert.That( sceneMetricsCounter.currentCount.bodies, Is.EqualTo(3));

        dataStore.RemoveRendereable(1, rendereable2);

        Assert.That( sceneMetricsCounter.currentCount.bodies, Is.EqualTo(2));

        dataStore.RemoveRendereable(1, rendereable1);

        Assert.That( sceneMetricsCounter.currentCount.bodies, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        UnityEngine.Object.Destroy(rend1.gameObject);
        UnityEngine.Object.Destroy(rend2.gameObject);
        UnityEngine.Object.Destroy(rend3.gameObject);
        UnityEngine.Object.Destroy(rend4.gameObject);
    }
}