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
        public DataStore.DataStore_WorldObjects.SceneData sceneData { get; private set; }
        public event Action<Rendereable> OnWillAddRendereable;
        public event Action<Rendereable> OnWillRemoveRendereable;
        public event Action<Mesh, int> OnWillAddMesh;
        public event Action<Mesh, int> OnWillRemoveMesh;

        private string sceneId;
        private DataStore dataStore;

        public WorldSceneObjectsTrackingHelper (DataStore dataStore, string sceneId)
        {
            this.dataStore = dataStore;
            this.sceneId = sceneId;

            if (dataStore.sceneWorldObjects.sceneData.ContainsKey(sceneId))
                sceneData = dataStore.sceneWorldObjects.sceneData[sceneId];

            dataStore.sceneWorldObjects.sceneData.OnAdded += OnSceneAdded;
            dataStore.sceneWorldObjects.sceneData.OnRemoved += OnSceneRemoved;
        }

        private void OnSceneRemoved(string sceneId, DataStore.DataStore_WorldObjects.SceneData arg2)
        {
            if ( sceneId != this.sceneId )
                return;

            // Set dummy scene data so null reference exceptions are avoided.
            SetSceneData( new DataStore.DataStore_WorldObjects.SceneData() );
        }

        private void OnSceneAdded(string sceneId, DataStore.DataStore_WorldObjects.SceneData sceneData)
        {
            if ( sceneId != this.sceneId )
                return;

            SetSceneData( sceneData );
        }

        private void SetSceneData(DataStore.DataStore_WorldObjects.SceneData sceneData)
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
                this.sceneData.renderedObjects.OnAdded -= OnRenderedObjectsAdded;
                this.sceneData.renderedObjects.OnRemoved -= OnRenderedObjectsRemoved;
                this.sceneData.refCountedMeshes.OnAdded -= OnRefCountedMeshesAdded;
                this.sceneData.refCountedMeshes.OnRemoved -= OnRefCountedMeshesRemoved;
            }

            sceneData.renderedObjects.OnAdded += OnRenderedObjectsAdded;
            sceneData.renderedObjects.OnRemoved += OnRenderedObjectsRemoved;
            sceneData.refCountedMeshes.OnAdded += OnRefCountedMeshesAdded;
            sceneData.refCountedMeshes.OnRemoved += OnRefCountedMeshesRemoved;

            this.sceneData = sceneData;
        }

        private void OnRefCountedMeshesRemoved(Mesh mesh, int refCount)
        {
            OnWillRemoveMesh?.Invoke(mesh, refCount);
        }

        private void OnRefCountedMeshesAdded(Mesh mesh, int refCount)
        {
            OnWillAddMesh?.Invoke(mesh, refCount);
        }

        private void OnRenderedObjectsRemoved(Rendereable rendereable)
        {
            OnWillRemoveRendereable?.Invoke(rendereable);
        }

        private void OnRenderedObjectsAdded(Rendereable rendereable)
        {
            OnWillAddRendereable?.Invoke(rendereable);
        }

        public void Dispose()
        {
            if ( sceneData == null )
                return;

            sceneData.renderedObjects.OnAdded -= OnRenderedObjectsAdded;
            sceneData.renderedObjects.OnRemoved -= OnRenderedObjectsRemoved;
            sceneData.refCountedMeshes.OnAdded -= OnRefCountedMeshesAdded;
            sceneData.refCountedMeshes.OnRemoved -= OnRefCountedMeshesRemoved;

            dataStore.sceneWorldObjects.sceneData.OnAdded -= OnSceneAdded;
            dataStore.sceneWorldObjects.sceneData.OnRemoved -= OnSceneRemoved;
        }
    }
}