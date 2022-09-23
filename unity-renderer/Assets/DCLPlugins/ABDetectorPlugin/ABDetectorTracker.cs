using System;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    public class ABDetectorTracker : IDisposable
    {
        private const string FromAssetBundleTag = "FromAssetBundle";
        private const string AbDetectorMaterialsPrefabName = "AbDetectorMaterials";
        
        private readonly DebugConfig debugConfig;
        private readonly Dictionary<Renderer, Material[]> rendererDict  = 
            new Dictionary<Renderer, Material[]>();
        private readonly Multimap<IParcelScene, Renderer> parcelToRendererMultimap = 
            new Multimap<IParcelScene, Renderer>();

        private ABDetectorMaterialsHolder abDetectorMaterialsHolder;

        public ABDetectorTracker(DebugConfig debugConfig)
        {
            this.debugConfig = debugConfig;
            
            debugConfig.showGlobalABDetectionLayer.OnChange += OnGlobalABDetectionChanged;
            debugConfig.showSceneABDetectionLayer.OnChange += OnSceneABDetectionChanged;
        }
        
        public void Dispose()
        {
            debugConfig.showGlobalABDetectionLayer.OnChange -= OnGlobalABDetectionChanged;
            debugConfig.showSceneABDetectionLayer.OnChange -= OnSceneABDetectionChanged;
        }
        
        private static IParcelScene FindSceneForPlayer()
        {
            foreach (IParcelScene scene in Environment.i.world.state.GetScenesSortedByDistance())
            {
                if (WorldStateUtils.IsCharacterInsideScene(scene))
                    return scene;
            }

            return null;
        }

        private void LoadMaterialsIfNeeded()
        {
            if (abDetectorMaterialsHolder == null)
            {
                abDetectorMaterialsHolder = Resources.Load<GameObject>
                        (AbDetectorMaterialsPrefabName)
                    .GetComponent<ABDetectorMaterialsHolder>();
            }
        }

        private void OnGlobalABDetectionChanged(bool current, bool previous)
        {
            LoadMaterialsIfNeeded();
            
            if (current)
            {
                RemoveABDetectionPaintingForCurrentScene();
                ApplyGlobalABDetectionPainting();
            }
            else
            {
                RemoveABDetectionPaintingForCurrentScene();
                RemoveGlobalABDetectionPainting();
            }
        }

        private void OnSceneABDetectionChanged(bool current, bool previous)
        {
            LoadMaterialsIfNeeded();
            
            if (current)
            {
                ApplyABDetectionPaintingForCurrentScene();
            }
            else
            {
                RemoveABDetectionPaintingForCurrentScene();
            }
        }

        private void ApplyGlobalABDetectionPainting()
        {
            var gameObjects = GameObject.FindObjectsOfType(typeof(GameObject));
            foreach (var gameObject in gameObjects)
            {
                var converted = (GameObject) gameObject;
                if (converted.transform.parent == null)
                {
                    ApplyMaterials(converted.transform);
                }
            }
        }

        private void RemoveGlobalABDetectionPainting()
        {
            foreach (KeyValuePair<Renderer,Material[]> keyValuePair in rendererDict)
            {
                keyValuePair.Key.materials = keyValuePair.Value;
            }
        
            rendererDict.Clear();
            parcelToRendererMultimap.Clear();
        }
        
        private void ApplyABDetectionPaintingForCurrentScene()
        {
            var currentScene = FindSceneForPlayer();
            ApplyMaterials(currentScene.GetSceneTransform(), currentScene);
        }
        
        private void RemoveABDetectionPaintingForCurrentScene()
        {
            var currentScene = FindSceneForPlayer();
            if (parcelToRendererMultimap.ContainsKey(currentScene))
            {
                foreach (var renderer in parcelToRendererMultimap.GetValues(currentScene))
                {
                    if (rendererDict.TryGetValue(renderer, out var materials))
                    {
                        renderer.materials = materials;
                    }
                }
            }
        }

        private void ApplyMaterials(Transform someTransform, IParcelScene optionalParcelScene = null)
        {
            if (someTransform.childCount > 0)
            {
                for (int i = 0; i < someTransform.childCount; i++)
                {
                    var childTransform = someTransform.GetChild(i).transform;
                    if (childTransform.gameObject.name.Contains("GLTF Shape"))
                    {
                        var childGameObject = childTransform.GetChild(0).gameObject;
                        var renderers = childGameObject.
                            GetComponentsInChildren<Renderer>(true);
                  
                        foreach (Renderer renderer in renderers)
                        {
                            rendererDict[renderer] = renderer.materials;

                            if (optionalParcelScene != null)
                            {
                                parcelToRendererMultimap.Add(optionalParcelScene, renderer);
                            }
                            
                            renderer.material = renderer.tag.Equals(FromAssetBundleTag) ? 
                            abDetectorMaterialsHolder.ABMaterial : abDetectorMaterialsHolder.GLTFMaterial;
                        }
                    }
                    else
                    {
                        ApplyMaterials(childTransform, optionalParcelScene);
                    }
                }
            }
        }
    }
}