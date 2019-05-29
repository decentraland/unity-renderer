using DCL.Controllers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    [System.Serializable]
    public class SceneMetricsController
    {
        private static bool VERBOSE = false;
        public ParcelScene scene;

        public static class LimitsConfig
        {
            // number of entities
            public const int entities = 200;

            // Number of faces (per parcel)
            public const int triangles = 10000;
            public const int bodies = 300;
            public const int textures = 10;
            public const int materials = 20;
            public const int meshes = 200;

            public const float visibleRadius = 10;
        }

        [System.Serializable]
        public class Model
        {
            public int meshes;
            public int bodies;
            public int materials;
            public int textures;
            public int triangles;
            public int entities;

            public Model Clone()
            {
                return (Model)MemberwiseClone();
            }
        }

        [SerializeField]
        private Model model;

        private HashSet<Mesh> uniqueMeshes;
        private HashSet<Material> uniqueMaterials;
        public bool isDirty { get; private set; }

        public Model GetModel() { return model.Clone(); }

        public SceneMetricsController(ParcelScene sceneOwner)
        {
            this.scene = sceneOwner;

            uniqueMeshes = new HashSet<Mesh>();
            uniqueMaterials = new HashSet<Material>();
            model = new Model();

            scene.OnEntityAdded += OnEntityAdded;
            scene.OnEntityRemoved += OnEntityRemoved;

            if (VERBOSE) { Debug.Log("Start ScenePerformanceLimitsController..."); }
        }

        public Model GetLimits()
        {
            int parcelCount = scene.sceneData.parcels.Length;
            float log = Mathf.Log(parcelCount + 1, 2);
            float lineal = parcelCount;

            Model result = new Model()
            {
                triangles = (int)(lineal * LimitsConfig.triangles),
                bodies = (int)(lineal * LimitsConfig.bodies),
                entities = (int)(lineal * LimitsConfig.entities),
                materials = (int)(log * LimitsConfig.materials),
                textures = (int)(log * LimitsConfig.textures),
                meshes = (int)(log * LimitsConfig.meshes)
            };

            return result;
        }

        public void SendEvent()
        {
            if (isDirty)
            {
                isDirty = false;
                Interface.WebInterface.ReportOnMetricsUpdate(scene.sceneData.id, model, GetLimits());
            }
        }

        void OnEntityAdded(DecentralandEntity e)
        {
            e.OnShapeUpdated += Entity_OnShapeUpdated;
            model.entities++;
            isDirty = true;
        }

        void OnEntityRemoved(DecentralandEntity e)
        {
            RemoveGameObject(e.meshGameObject);
            e.OnShapeUpdated -= Entity_OnShapeUpdated;
            model.entities--;
            isDirty = true;
        }

        void Entity_OnShapeUpdated(DecentralandEntity e)
        {
            AddGameObject(e.meshGameObject);
        }

        void RemoveGameObject(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            //NOTE(Brian): If this proves to be too slow we can spread it with a Coroutine spooler.
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                MeshFilter mf = r.gameObject.GetComponent<MeshFilter>();

                var transitionController = r.gameObject.GetComponentInParent<MaterialTransitionController>();
                if (transitionController != null && transitionController.placeholder == r.gameObject)
                {
                    continue;
                }

                if (mf != null && mf.sharedMesh != null)
                {
                    model.bodies--;
                    isDirty = true;

                    if (uniqueMeshes.Contains(mf.sharedMesh))
                    {
                        if (VERBOSE) { Debug.Log("Removing mesh... " + go.name); }

                        uniqueMeshes.Remove(mf.sharedMesh);
                        RemoveMesh(mf.sharedMesh);
                    }
                }

                for (int j = 0; j < r.sharedMaterials.Length; j++)
                {
                    Material m = r.sharedMaterials[j];

                    if (uniqueMaterials.Contains(m))
                    {
                        uniqueMaterials.Remove(m);
                        model.materials = uniqueMaterials.Count;
                    }
                }
            }
        }

        void AddGameObject(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            //NOTE(Brian): If this proves to be too slow we can spread it with a Coroutine spooler.
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];

                var transitionController = r.gameObject.GetComponentInParent<MaterialTransitionController>();
                if (transitionController != null && transitionController.placeholder == r.gameObject)
                {
                    continue;
                }

                MeshFilter mf = r.gameObject.GetComponent<MeshFilter>();

                if (mf != null && mf.sharedMesh != null)
                {
                    model.bodies++;
                    isDirty = true;

                    if (!uniqueMeshes.Contains(mf.sharedMesh))
                    {
                        if (VERBOSE) { Debug.Log("Adding mesh... " + go.name, r.gameObject); }

                        uniqueMeshes.Add(mf.sharedMesh);
                        AddMesh(mf.sharedMesh);
                    }
                }

                for (int j = 0; j < r.sharedMaterials.Length; j++)
                {
                    Material m = r.sharedMaterials[j];

                    if (!uniqueMaterials.Contains(m))
                    {
                        uniqueMaterials.Add(m);
                        model.materials = uniqueMaterials.Count;
                    }
                }
            }
        }

        public void AddMesh(Mesh mesh)
        {
            model.meshes++;
            model.triangles += mesh.triangles.Length / 3;
            isDirty = true;

            if (VERBOSE) { Debug.Log("Mesh name = " + mesh.name + " ... tri count = " + (mesh.triangles.Length / 3)); }

            ;
        }

        public void RemoveMesh(Mesh mesh)
        {
            model.meshes--;
            model.triangles -= mesh.triangles.Length / 3;
            isDirty = true;
        }
    }
}