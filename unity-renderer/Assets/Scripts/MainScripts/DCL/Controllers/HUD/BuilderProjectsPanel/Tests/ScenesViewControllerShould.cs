using NUnit.Framework;
using System.Collections.Generic;
using DCL.Builder;
using UnityEditor;

namespace Tests
{
    public class ScenesViewControllerShould
    {
        private IScenesViewController scenesViewController;
        private Listener_Mock listenerMock;

        [SetUp]
        public void SetUp()
        {
            const string prefabAssetPath =
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/SceneCardView.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<SceneCardView>(prefabAssetPath);

            scenesViewController = new ScenesViewController(prefab);
            listenerMock = new Listener_Mock();

            scenesViewController.OnSceneRemoved += ((ISceneListener)listenerMock).SceneRemoved;
            scenesViewController.OnSceneAdded += ((ISceneListener)listenerMock).SceneAdded;
            scenesViewController.OnScenesSet += ((ISceneListener)listenerMock).SetScenes;
        }

        [TearDown]
        public void TearDown() { scenesViewController.Dispose(); }

        [Test]
        public void CallListenerEventsCorrectly()
        {
            scenesViewController.SetScenes(new ISceneData[] { new SceneData() { id = "1", isDeployed = true } });

            Assert.AreEqual(1, listenerMock.deployedScenes.Count);
            Assert.AreEqual(1, listenerMock.setScenes.Count);
            Assert.AreEqual(0, listenerMock.addedScenes.Count);
            Assert.AreEqual(0, listenerMock.removedScenes.Count);

            listenerMock.Clear();

            scenesViewController.SetScenes(new ISceneData[]
            {
                new SceneData() { id = "1", isDeployed = true },
                new SceneData() { id = "2", isDeployed = true }
            });

            Assert.AreEqual(2, listenerMock.deployedScenes.Count);
            Assert.AreEqual(0, listenerMock.setScenes.Count);
            Assert.AreEqual(1, listenerMock.addedScenes.Count);
            Assert.AreEqual(0, listenerMock.removedScenes.Count);

            listenerMock.Clear();

            scenesViewController.SetScenes(new ISceneData[]
            {
                new SceneData() { id = "1", isDeployed = true },
                new SceneData() { id = "2", isDeployed = false }
            });

            Assert.AreEqual(1, listenerMock.deployedScenes.Count);
            Assert.AreEqual(0, listenerMock.addedScenes.Count);
            Assert.AreEqual(1, listenerMock.removedScenes.Count);

            listenerMock.Clear();

            scenesViewController.SetScenes(new ISceneData[]
            {
                new SceneData() { id = "1", isDeployed = true },
                new SceneData() { id = "2", isDeployed = false }
            });

            Assert.AreEqual(1, listenerMock.deployedScenes.Count);
            Assert.AreEqual(0, listenerMock.addedScenes.Count);
            Assert.AreEqual(0, listenerMock.removedScenes.Count);

            listenerMock.Clear();

            scenesViewController.SetScenes(new ISceneData[]
            {
                new SceneData() { id = "1", isDeployed = false },
                new SceneData() { id = "2", isDeployed = false }
            });

            Assert.AreEqual(0, listenerMock.deployedScenes.Count);
            Assert.AreEqual(1, listenerMock.removedScenes.Count);
        }
    }

    class Listener_Mock : ISceneListener
    {
        public List<string> setScenes = new List<string>();
        public List<string> addedScenes = new List<string>();
        public List<string> removedScenes = new List<string>();

        public List<string> deployedScenes = new List<string>();

        public void Clear()
        {
            setScenes.Clear();
            addedScenes.Clear();
            removedScenes.Clear();
        }

        void ISceneListener.SetScenes(Dictionary<string, ISceneCardView> scenes)
        {
            foreach (var view in scenes.Values)
            {
                setScenes.Add(view.SceneData.id);
                deployedScenes.Add(view.SceneData.id);
            }
        }

        void ISceneListener.SceneAdded(ISceneCardView scene)
        {
            addedScenes.Add(scene.SceneData.id);
            deployedScenes.Add(scene.SceneData.id);
        }

        void ISceneListener.SceneRemoved(ISceneCardView scene)
        {
            removedScenes.Add(scene.SceneData.id);
            deployedScenes.Remove(scene.SceneData.id);
        }
    }
}