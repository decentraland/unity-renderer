﻿using NUnit.Framework;
using System.Collections.Generic;
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

            scenesViewController.OnDeployedSceneRemoved += ((IDeployedSceneListener)listenerMock).OnSceneRemoved;
            scenesViewController.OnDeployedSceneAdded += ((IDeployedSceneListener)listenerMock).OnSceneAdded;
            scenesViewController.OnDeployedScenesSet += ((IDeployedSceneListener)listenerMock).OnSetScenes;

            scenesViewController.OnProjectSceneRemoved += ((IScenesListener)listenerMock).OnSceneRemoved;
            scenesViewController.OnProjectSceneAdded += ((IScenesListener)listenerMock).OnSceneAdded;
            scenesViewController.OnProjectScenesSet += ((IScenesListener)listenerMock).OnSetScenes;
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
            Assert.AreEqual(0, listenerMock.projectScenes.Count);

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
            Assert.AreEqual(0, listenerMock.projectScenes.Count);

            listenerMock.Clear();

            scenesViewController.SetScenes(new ISceneData[]
            {
                new SceneData() { id = "1", isDeployed = true },
                new SceneData() { id = "2", isDeployed = false }
            });

            Assert.AreEqual(1, listenerMock.deployedScenes.Count);
            Assert.AreEqual(1, listenerMock.setScenes.Count);
            Assert.AreEqual(0, listenerMock.addedScenes.Count);
            Assert.AreEqual(1, listenerMock.removedScenes.Count);
            Assert.AreEqual(1, listenerMock.projectScenes.Count);

            listenerMock.Clear();

            scenesViewController.SetScenes(new ISceneData[]
            {
                new SceneData() { id = "1", isDeployed = true },
                new SceneData() { id = "2", isDeployed = false }
            });

            Assert.AreEqual(1, listenerMock.deployedScenes.Count);
            Assert.AreEqual(0, listenerMock.setScenes.Count);
            Assert.AreEqual(0, listenerMock.addedScenes.Count);
            Assert.AreEqual(0, listenerMock.removedScenes.Count);
            Assert.AreEqual(1, listenerMock.projectScenes.Count);

            listenerMock.Clear();

            scenesViewController.SetScenes(new ISceneData[]
            {
                new SceneData() { id = "1", isDeployed = false },
                new SceneData() { id = "2", isDeployed = false }
            });

            Assert.AreEqual(0, listenerMock.deployedScenes.Count);
            Assert.AreEqual(0, listenerMock.setScenes.Count);
            Assert.AreEqual(1, listenerMock.addedScenes.Count);
            Assert.AreEqual(1, listenerMock.removedScenes.Count);
            Assert.AreEqual(2, listenerMock.projectScenes.Count);
        }
    }

    class Listener_Mock : IDeployedSceneListener, IScenesListener
    {
        public List<string> setScenes = new List<string>();
        public List<string> addedScenes = new List<string>();
        public List<string> removedScenes = new List<string>();

        public List<string> deployedScenes = new List<string>();
        public List<string> projectScenes = new List<string>();

        public void Clear()
        {
            setScenes.Clear();
            addedScenes.Clear();
            removedScenes.Clear();
        }

        void IDeployedSceneListener.OnSetScenes(Dictionary<string, ISceneCardView> scenes)
        {
            foreach (var view in scenes.Values)
            {
                setScenes.Add(view.sceneData.id);
                deployedScenes.Add(view.sceneData.id);
            }
        }

        void IDeployedSceneListener.OnSceneAdded(ISceneCardView scene)
        {
            addedScenes.Add(scene.sceneData.id);
            deployedScenes.Add(scene.sceneData.id);
        }

        void IDeployedSceneListener.OnSceneRemoved(ISceneCardView scene)
        {
            removedScenes.Add(scene.sceneData.id);
            deployedScenes.Remove(scene.sceneData.id);
        }

        void IScenesListener.OnSetScenes(Dictionary<string, ISceneCardView> scenes)
        {
            foreach (var view in scenes.Values)
            {
                setScenes.Add(view.sceneData.id);
                projectScenes.Add(view.sceneData.id);
            }
        }

        void IScenesListener.OnSceneAdded(ISceneCardView scene)
        {
            addedScenes.Add(scene.sceneData.id);
            projectScenes.Add(scene.sceneData.id);
        }

        void IScenesListener.OnSceneRemoved(ISceneCardView scene)
        {
            removedScenes.Add(scene.sceneData.id);
            projectScenes.Remove(scene.sceneData.id);
        }
    }
}