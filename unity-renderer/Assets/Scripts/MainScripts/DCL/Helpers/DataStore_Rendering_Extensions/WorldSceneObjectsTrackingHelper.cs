using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    /// <summary>
    /// This class wraps around the DataStore's DataStore_WorldObjects instance.
    ///
    /// The DataStore_WorldObjects scene data objects must be removed aggressively when the
    /// DataStore_WorldObjects.SceneData is empty, right after the last object is removed.
    ///
    /// The DataStore_WorldObjects.SceneData objects can't be removed just when the scene is
    /// unloaded because the underlying assets are removed lazily (i.e. by ParcelScenesCleaner).
    ///
    /// This helper ensures that any events persist even in the case of the scene being removed and then
    /// added due to the last object being removed and then added again.
    /// </summary>
    public class WorldSceneObjectsTrackingHelper : IDisposable
    {
        private static bool VERBOSE = false;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        public DataStore_WorldObjects.SceneData sceneData { get; private set; }
        public event Action<Rendereable> OnWillAddRendereable;
        public event Action<Rendereable> OnWillRemoveRendereable;

        private string sceneId;
        private DataStore_WorldObjects dataStore;

        public WorldSceneObjectsTrackingHelper (DataStore_WorldObjects dataStore, string sceneId)
        {
            logger.Log("A wild WorldSceneObjectsTrackingHelper appears!");
            this.dataStore = dataStore;
            this.sceneId = sceneId;

            if (dataStore.sceneData.ContainsKey(sceneId))
                SetSceneData(dataStore.sceneData[sceneId]);

            dataStore.sceneData.OnAdded += OnSceneAdded;
            dataStore.sceneData.OnRemoved += OnSceneRemoved;
        }

        private void OnSceneRemoved(string sceneId, DataStore_WorldObjects.SceneData arg2)
        {
            if ( sceneId != this.sceneId )
                return;

            // Set dummy scene data so null reference exceptions are avoided.
            logger.Log($"Scene {sceneId} was removed! Using dummy scene data.");
            SetSceneData( new DataStore_WorldObjects.SceneData() );
        }

        private void OnSceneAdded(string sceneId, DataStore_WorldObjects.SceneData sceneData)
        {
            if ( sceneId != this.sceneId )
                return;

            logger.Log($"Scene {sceneId} was added!");
            SetSceneData( sceneData );
        }

        private void SetSceneData(DataStore_WorldObjects.SceneData sceneData)
        {
            Assert.IsNotNull(sceneData, "sceneData should never be null!");

            if ( sceneData == this.sceneData )
                return;

            if ( this.sceneData != null )
            {
                // This should never happen because of how the flow works.
                //
                // Scenes with the same sceneId shouldn't ever be added twice, and if this happens
                // we have an early exit.
                //this.sceneData.renderedObjects.OnAdded -= OnRenderedObjectsAdded;
                //this.sceneData.renderedObjects.OnRemoved -= OnRenderedObjectsRemoved;
            }

            logger.Log($"Subscribing events for {sceneId}.");
            //sceneData.renderedObjects.OnAdded += OnRenderedObjectsAdded;
            //sceneData.renderedObjects.OnRemoved += OnRenderedObjectsRemoved;

            this.sceneData = sceneData;
        }

        private void OnRenderedObjectsRemoved(Rendereable rendereable)
        {
            logger.Log($"Removing rendereable.");
            OnWillRemoveRendereable?.Invoke(rendereable);
        }

        private void OnRenderedObjectsAdded(Rendereable rendereable)
        {
            logger.Log($"Adding rendereable.");
            OnWillAddRendereable?.Invoke(rendereable);
        }

        public void Dispose()
        {
            if ( sceneData == null )
                return;

            //sceneData.renderedObjects.OnAdded -= OnRenderedObjectsAdded;
            //sceneData.renderedObjects.OnRemoved -= OnRenderedObjectsRemoved;

            dataStore.sceneData.OnAdded -= OnSceneAdded;
            dataStore.sceneData.OnRemoved -= OnSceneRemoved;
            logger.Log($"Disposing.");
        }
    }
}