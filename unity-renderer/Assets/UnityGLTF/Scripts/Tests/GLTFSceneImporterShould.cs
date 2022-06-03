using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GLTF;
using GLTF.Schema;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;
using UnityGLTF.Loader;

public class GLTFSceneImporterShould
{
    private const string FILE_NAME = "Trunk.gltf";
    private readonly string rootDirectoryPath = Application.dataPath + "/../TestResources/GLTF/Trunk/";
    
    private GLTFSceneImporter gltfLoader;
    private FileStream stream;
    private CancellationTokenSource cancellationTokenSource;
    private ILoader fileLoader;

    [SetUp]
    public void SetUp()
    {
        PerformanceAnalytics.ResetAll();

        cancellationTokenSource = new CancellationTokenSource();
        string fullPath = Path.Combine(rootDirectoryPath, FILE_NAME);
        fileLoader = new GLTFFileLoader(Path.GetDirectoryName(rootDirectoryPath));
        stream = File.OpenRead(fullPath);
        GLTFParser.ParseJson(stream, out GLTFRoot gLTFRoot);
        gltfLoader = new GLTFSceneImporter("id", gLTFRoot, fileLoader, new GLTFThrottlingCounter(), stream);
        gltfLoader.addImagesToPersistentCaching = false;
        gltfLoader.addMaterialsToPersistentCaching = false;
        gltfLoader.initialVisibility = true;
        gltfLoader.useMaterialTransition = false;
        gltfLoader.forceGPUOnlyMesh = false;
        gltfLoader.forceGPUOnlyTex = false;
        gltfLoader.forceSyncCoroutines = true;
    }

    [TearDown]
    public void TearDown()
    {
        stream.Dispose();
        Object.DestroyImmediate(gltfLoader.CreatedObject);
    }
    
    [UnityTest]
    public IEnumerator TrackLoadingGLTFCorrectly()
    {
        Task task = gltfLoader.LoadScene(cancellationTokenSource.Token);

        yield return null;

        Assert.AreEqual(1, PerformanceAnalytics.GLTFTracker.GetData().loading, "Loading GLTFs");

        cancellationTokenSource.Cancel();
        yield return new WaitUntil(() => task.IsCompleted);
        
        (int loading, int failed, int cancelled, int loaded) = PerformanceAnalytics.GLTFTracker.GetData();
        Assert.AreEqual(0, loaded, "Loaded GLTFs");
        Assert.AreEqual(0, loading, "Loading GLTFs");
        Assert.AreEqual(0, failed, "Failed GLTFs");
        Assert.AreEqual(1, cancelled, "Cancelled GLTFs");
    }
    
    [UnityTest]
    public IEnumerator TrackLoadedGLTFCorrectly()
    {
        Task task = gltfLoader.LoadScene(cancellationTokenSource.Token);

        yield return new WaitUntil(() => task.IsCompleted);

        (int loading, int failed, int cancelled, int loaded) = PerformanceAnalytics.GLTFTracker.GetData();
        Assert.AreEqual(1, loaded, "Loaded GLTFs");
        Assert.AreEqual(0, loading, "Loading GLTFs");
        Assert.AreEqual(0, failed, "Failed GLTFs");
        Assert.AreEqual(0, cancelled, "Cancelled GLTFs");
    }
    
    [UnityTest]
    public IEnumerator TrackCancelledGLTFCorrectly()
    {
        Task task = gltfLoader.LoadScene(cancellationTokenSource.Token);

        yield return null;

        Assert.AreEqual(1, PerformanceAnalytics.GLTFTracker.GetData().loading, "Loading GLTFs");

        cancellationTokenSource.Cancel();
        
        yield return new WaitUntil(() => task.IsCompleted);

        (int loading, int failed, int cancelled, int loaded) = PerformanceAnalytics.GLTFTracker.GetData();
        Assert.AreEqual(0, loaded, "Loaded GLTFs");
        Assert.AreEqual(0, loading, "Loading GLTFs");
        Assert.AreEqual(0, failed, "Failed GLTFs");
        Assert.AreEqual(1, cancelled, "Cancelled GLTFs");
    }
    
    [UnityTest]
    public IEnumerator TrackFailedGLTFCorrectly()
    {
        gltfLoader = new GLTFSceneImporter("id", null, fileLoader, new GLTFThrottlingCounter(), stream);
        stream.Dispose(); // this should cause an exception
        
        Task task = gltfLoader.LoadScene(cancellationTokenSource.Token);
        
        yield return new WaitUntil(() => task.IsCompleted);

        (int loading, int failed, int cancelled, int loaded) = PerformanceAnalytics.GLTFTracker.GetData();
        Assert.AreEqual(0, loaded, "Loaded GLTFs");
        Assert.AreEqual(0, loading, "Loading GLTFs");
        Assert.AreEqual(1, failed, "Failed GLTFs");
        Assert.AreEqual(0, cancelled, "Cancelled GLTFs");
    }
}