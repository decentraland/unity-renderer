using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;

namespace Tests
{
    public class ScenesViewControllerShould
    {
        private IPlacesViewController placesViewController;
        private Listener_Mock listenerMock;

        [SetUp]
        public void SetUp()
        {
            const string prefabAssetPath =
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/SceneCardView.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<PlaceCardView>(prefabAssetPath);

            placesViewController = new PlacesViewController(prefab);
            listenerMock = new Listener_Mock();

            placesViewController.OnPlaceRemoved += ((IPlaceListener)listenerMock).PlaceRemoved;
            placesViewController.OnPlaceAdded += ((IPlaceListener)listenerMock).PlaceAdded;
            placesViewController.OnPlacesSet += ((IPlaceListener)listenerMock).SetPlaces;

            placesViewController.OnProjectSceneRemoved += ((IProjectListener)listenerMock).PlaceRemoved;
            placesViewController.OnProjectSceneAdded += ((IProjectListener)listenerMock).PlaceAdded;
            placesViewController.OnProjectScenesSet += ((IProjectListener)listenerMock).SetPlaces;
        }

        [TearDown]
        public void TearDown() { placesViewController.Dispose(); }

        [Test]
        public void CallListenerEventsCorrectly()
        {
            placesViewController.SetPlaces(new IPlaceData[] { new PlaceData() { id = "1", isDeployed = true } });

            Assert.AreEqual(1, listenerMock.deployedScenes.Count);
            Assert.AreEqual(1, listenerMock.setScenes.Count);
            Assert.AreEqual(0, listenerMock.addedScenes.Count);
            Assert.AreEqual(0, listenerMock.removedScenes.Count);
            Assert.AreEqual(0, listenerMock.projectScenes.Count);

            listenerMock.Clear();

            placesViewController.SetPlaces(new IPlaceData[]
            {
                new PlaceData() { id = "1", isDeployed = true },
                new PlaceData() { id = "2", isDeployed = true }
            });

            Assert.AreEqual(2, listenerMock.deployedScenes.Count);
            Assert.AreEqual(0, listenerMock.setScenes.Count);
            Assert.AreEqual(1, listenerMock.addedScenes.Count);
            Assert.AreEqual(0, listenerMock.removedScenes.Count);
            Assert.AreEqual(0, listenerMock.projectScenes.Count);

            listenerMock.Clear();

            placesViewController.SetPlaces(new IPlaceData[]
            {
                new PlaceData() { id = "1", isDeployed = true },
                new PlaceData() { id = "2", isDeployed = false }
            });

            Assert.AreEqual(1, listenerMock.deployedScenes.Count);
            Assert.AreEqual(1, listenerMock.setScenes.Count);
            Assert.AreEqual(0, listenerMock.addedScenes.Count);
            Assert.AreEqual(1, listenerMock.removedScenes.Count);
            Assert.AreEqual(1, listenerMock.projectScenes.Count);

            listenerMock.Clear();

            placesViewController.SetPlaces(new IPlaceData[]
            {
                new PlaceData() { id = "1", isDeployed = true },
                new PlaceData() { id = "2", isDeployed = false }
            });

            Assert.AreEqual(1, listenerMock.deployedScenes.Count);
            Assert.AreEqual(0, listenerMock.setScenes.Count);
            Assert.AreEqual(0, listenerMock.addedScenes.Count);
            Assert.AreEqual(0, listenerMock.removedScenes.Count);
            Assert.AreEqual(1, listenerMock.projectScenes.Count);

            listenerMock.Clear();

            placesViewController.SetPlaces(new IPlaceData[]
            {
                new PlaceData() { id = "1", isDeployed = false },
                new PlaceData() { id = "2", isDeployed = false }
            });

            Assert.AreEqual(0, listenerMock.deployedScenes.Count);
            Assert.AreEqual(0, listenerMock.setScenes.Count);
            Assert.AreEqual(1, listenerMock.addedScenes.Count);
            Assert.AreEqual(1, listenerMock.removedScenes.Count);
            Assert.AreEqual(2, listenerMock.projectScenes.Count);
        }
    }

    class Listener_Mock : IPlaceListener, IProjectListener
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

        void IPlaceListener.SetPlaces(Dictionary<string, IPlaceCardView> scenes)
        {
            foreach (var view in scenes.Values)
            {
                setScenes.Add(view.PlaceData.id);
                deployedScenes.Add(view.PlaceData.id);
            }
        }

        void IPlaceListener.PlaceAdded(IPlaceCardView place)
        {
            addedScenes.Add(place.PlaceData.id);
            deployedScenes.Add(place.PlaceData.id);
        }

        void IPlaceListener.PlaceRemoved(IPlaceCardView place)
        {
            removedScenes.Add(place.PlaceData.id);
            deployedScenes.Remove(place.PlaceData.id);
        }

        void IProjectListener.SetPlaces(Dictionary<string, IPlaceCardView> scenes)
        {
            foreach (var view in scenes.Values)
            {
                setScenes.Add(view.PlaceData.id);
                projectScenes.Add(view.PlaceData.id);
            }
        }

        void IProjectListener.PlaceAdded(IPlaceCardView place)
        {
            addedScenes.Add(place.PlaceData.id);
            projectScenes.Add(place.PlaceData.id);
        }

        void IProjectListener.PlaceRemoved(IPlaceCardView place)
        {
            removedScenes.Add(place.PlaceData.id);
            projectScenes.Remove(place.PlaceData.id);
        }
    }
}