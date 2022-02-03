using DCL;
using NUnit.Framework;
using UnityEngine;

public class SceneMetricsCounterShould
{
    [Test]
    public void CountEntitiesWhenAddedAndRemoved()
    {
        var sceneMetricsCounter = new SceneMetricsCounter(new DataStore_WorldObjects(), "A", Vector2Int.zero, 10);
        sceneMetricsCounter.Enable();

        sceneMetricsCounter.AddEntity("1");

        Assert.That( sceneMetricsCounter.model.entities, Is.EqualTo(1));

        sceneMetricsCounter.AddEntity("2");
        sceneMetricsCounter.AddEntity("3");
        sceneMetricsCounter.AddEntity("4");
        sceneMetricsCounter.AddEntity("5");
        sceneMetricsCounter.AddEntity("6");
        sceneMetricsCounter.AddEntity("7");
        sceneMetricsCounter.AddEntity("8");
        sceneMetricsCounter.AddEntity("9");
        sceneMetricsCounter.AddEntity("10");

        Assert.That( sceneMetricsCounter.model.entities, Is.EqualTo(10));

        sceneMetricsCounter.RemoveEntity("5");

        Assert.That( sceneMetricsCounter.model.entities, Is.EqualTo(9));

        sceneMetricsCounter.Dispose();
    }

    [Test]
    public void CountUniqueMeshesWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, "1", Vector2Int.zero, 10);
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add("1", new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add("2", new DataStore_WorldObjects.SceneData());

        Mesh mesh1 = new Mesh();
        Mesh mesh2 = new Mesh();
        Mesh mesh3 = new Mesh();
        Mesh mesh4 = new Mesh();

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = "A";
        rendereable1.meshes.Add(mesh1);
        rendereable1.meshes.Add(mesh2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = "B";
        rendereable2.meshes.Add(mesh3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = "C";
        rendereable3.meshes.Add(mesh4);

        dataStore.AddRendereable("1", rendereable1);
        dataStore.AddRendereable("1", rendereable2);
        dataStore.AddRendereable("2", rendereable3);

        Assert.That( sceneMetricsCounter.model.meshes, Is.EqualTo(3));

        dataStore.RemoveRendereable("1", rendereable2);

        Assert.That( sceneMetricsCounter.model.meshes, Is.EqualTo(2));

        dataStore.RemoveRendereable("1", rendereable1);

        Assert.That( sceneMetricsCounter.model.meshes, Is.EqualTo(0));

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
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, "1", Vector2Int.zero, 10);
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add("1", new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add("2", new DataStore_WorldObjects.SceneData());

        Material mat1 = new Material(Shader.Find("Standard"));
        Material mat2 = new Material(Shader.Find("Standard"));
        Material mat3 = new Material(Shader.Find("Standard"));
        Material mat4 = new Material(Shader.Find("Standard"));

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = "A";
        rendereable1.materials.Add(mat1);
        rendereable1.materials.Add(mat2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = "B";
        rendereable2.materials.Add(mat3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = "C";
        rendereable3.materials.Add(mat4);

        dataStore.AddRendereable("1", rendereable1);
        dataStore.AddRendereable("1", rendereable2);
        dataStore.AddRendereable("2", rendereable3);

        Assert.That( sceneMetricsCounter.model.materials, Is.EqualTo(3));

        dataStore.RemoveRendereable("1", rendereable2);

        Assert.That( sceneMetricsCounter.model.materials, Is.EqualTo(2));

        dataStore.RemoveRendereable("1", rendereable1);

        Assert.That( sceneMetricsCounter.model.materials, Is.EqualTo(0));

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
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, "1", Vector2Int.zero, 10);
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add("1", new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add("2", new DataStore_WorldObjects.SceneData());

        Texture2D tex1 = new Texture2D(1, 1);
        Texture2D tex2 = new Texture2D(1, 1);
        Texture2D tex3 = new Texture2D(1, 1);
        Texture2D tex4 = new Texture2D(1, 1);

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = "A";
        rendereable1.textures.Add(tex1);
        rendereable1.textures.Add(tex2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = "B";
        rendereable2.textures.Add(tex3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = "C";
        rendereable3.textures.Add(tex4);

        dataStore.AddRendereable("1", rendereable1);
        dataStore.AddRendereable("1", rendereable2);
        dataStore.AddRendereable("2", rendereable3);

        Assert.That( sceneMetricsCounter.model.textures, Is.EqualTo(3));

        dataStore.RemoveRendereable("1", rendereable2);

        Assert.That( sceneMetricsCounter.model.textures, Is.EqualTo(2));

        dataStore.RemoveRendereable("1", rendereable1);

        Assert.That( sceneMetricsCounter.model.textures, Is.EqualTo(0));

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
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, "1", Vector2Int.zero, 10);
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add("1", new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add("2", new DataStore_WorldObjects.SceneData());

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = "A";
        rendereable1.totalTriangleCount = 30;

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = "B";
        rendereable2.totalTriangleCount = 60;

        dataStore.AddRendereable("1", rendereable1);
        dataStore.AddRendereable("1", rendereable2);

        Assert.That( sceneMetricsCounter.model.triangles, Is.EqualTo(30));

        dataStore.RemoveRendereable("1", rendereable1);

        Assert.That( sceneMetricsCounter.model.triangles, Is.EqualTo(20));

        sceneMetricsCounter.Dispose();
    }

    [Test]
    public void CountBodiesWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, "1", Vector2Int.zero, 10);
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add("1", new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add("2", new DataStore_WorldObjects.SceneData());

        Renderer rend1 = new GameObject("Test").AddComponent<MeshRenderer>();
        Renderer rend2 = new GameObject("Test").AddComponent<MeshRenderer>();
        Renderer rend3 = new GameObject("Test").AddComponent<MeshRenderer>();
        Renderer rend4 = new GameObject("Test").AddComponent<MeshRenderer>();

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = "A";
        rendereable1.renderers.Add(rend1);
        rendereable1.renderers.Add(rend2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = "B";
        rendereable2.renderers.Add(rend3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = "C";
        rendereable3.renderers.Add(rend4);

        dataStore.AddRendereable("1", rendereable1);
        dataStore.AddRendereable("1", rendereable2);
        dataStore.AddRendereable("2", rendereable3);

        Assert.That( sceneMetricsCounter.model.bodies, Is.EqualTo(3));

        dataStore.RemoveRendereable("1", rendereable2);

        Assert.That( sceneMetricsCounter.model.bodies, Is.EqualTo(2));

        dataStore.RemoveRendereable("1", rendereable1);

        Assert.That( sceneMetricsCounter.model.bodies, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        UnityEngine.Object.Destroy(rend1.gameObject);
        UnityEngine.Object.Destroy(rend2.gameObject);
        UnityEngine.Object.Destroy(rend3.gameObject);
        UnityEngine.Object.Destroy(rend4.gameObject);
    }

    [Test]
    public void NotCountWhenEntityIsExcluded()
    {
        const string OWNER_1 = "A";
        const string OWNER_2 = "B";
        const string SCENE_ID = "1";

        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, SCENE_ID, Vector2Int.zero, 10);
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add(SCENE_ID, new DataStore_WorldObjects.SceneData());

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = OWNER_1;

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = OWNER_2;

        dataStore.AddRendereable(SCENE_ID, rendereable1);

        Assert.That(sceneMetricsCounter.model.entities, Is.EqualTo(1), $"Entity {OWNER_1} shouldn't be excluded!");

        dataStore.RemoveRendereable(SCENE_ID, rendereable1);

        Assert.That(sceneMetricsCounter.model.entities, Is.EqualTo(0), $"Entity {OWNER_1} should be always removed!");

        sceneMetricsCounter.AddExcludedEntity(OWNER_1);

        dataStore.AddRendereable(SCENE_ID, rendereable1);
        dataStore.AddRendereable(SCENE_ID, rendereable2);

        Assert.That(sceneMetricsCounter.model.entities, Is.EqualTo(1), "AddExcludedEntity is not working!");

        sceneMetricsCounter.RemoveExcludedEntity(OWNER_1);

        Assert.That(sceneMetricsCounter.model.entities, Is.EqualTo(2), "RemoveExcludedEntity is not working!");

        sceneMetricsCounter.AddExcludedEntity(OWNER_1);
        sceneMetricsCounter.AddExcludedEntity(OWNER_2);

        Assert.That(sceneMetricsCounter.model.entities, Is.EqualTo(0), "AddExcludedEntity should remove existing entities!");

        sceneMetricsCounter.Dispose();
    }
}