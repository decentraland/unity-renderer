using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class ABDetectorTracker : IDisposable
    {
        private readonly DebugConfig _debugConfig;
        private int _abCount = 0;
        private int _gltfCount = 0;
        private readonly Dictionary<Renderer, Material[]> _rendererDict  = new Dictionary<Renderer, Material[]>();

        public ABDetectorTracker(DebugConfig debugConfig)
        {
            this._debugConfig = debugConfig;
            
            debugConfig.showGlobalABDetectionLayer.OnChange += OnGlobalABDetectionChanged;
            debugConfig.showSceneABDetectionLayer.OnChange += OnSceneABDetectionChanged;
        }

        private void OnGlobalABDetectionChanged(bool current, bool previous)
        {
            if (current)
            {
                RemoveDetectionPaintingForCurrentScene();
                ApplyGlobalDetectionPainting();
            }
            else
            {
                RemoveDetectionPaintingForCurrentScene();
                RemoveGlobalDetectionPainting();
            }
        }

        private void OnSceneABDetectionChanged(bool current, bool previous)
        {
            if (current)
            {
                ApplyDetectionPaintingForCurrentScene();
            }
            else
            {
                RemoveDetectionPaintingForCurrentScene();
            }
        }

        private void ApplyGlobalDetectionPainting()
        {
            _abCount = 0;
            _gltfCount = 0;
            var gameObjects = GameObject.FindObjectsOfType(typeof(GameObject));
            foreach (var gameObject in gameObjects)
            {
                var converted = (GameObject) gameObject;
                if (converted.transform.parent == null)
                {
                    ChangeMaterials(converted.transform);
                }
            }
        }

        private void ChangeMaterials(Transform someTransform)
        {
            // if (someTransform.childCount > 0)
            // {
            //     for (int i = 0; i < someTransform.childCount; i++)
            //     {
            //         var childTransform = someTransform.GetChild(i).transform;
            //         if (childTransform.gameObject.name.Contains("GLTF Shape"))
            //         {
            //             var childGameObject = childTransform.GetChild(0).gameObject;
            //             if (childGameObject.name.Contains("AB: ") && !childGameObject.name.Contains("GLTF: "))
            //             {
            //                 var renderers = childGameObject.GetComponentsInChildren<Renderer>(true);
            //       
            //                 foreach (Renderer renderer in renderers)
            //                 {
            //                     if(!_rendererDict.ContainsKey(renderer))
            //                         _rendererDict.Add(renderer,renderer.materials);
            //                     
            //                     renderer.material = abMaterial;
            //                 }
            //                 
            //                 _abCount++;
            //             }
            //             else
            //             {
            //                 var renderers = childGameObject.GetComponentsInChildren<Renderer>(true);
            //                 foreach (Renderer renderer in renderers)
            //                 {
            //                     if(!_rendererDict.ContainsKey(renderer))
            //                         _rendererDict.Add(renderer,renderer.materials);
            //                     
            //                     renderer.material = gltfMaterial;
            //                 }
            //             
            //                 _gltfCount++;
            //             }
            //         }
            //         else
            //         {
            //             ChangeMaterials(childTransform);
            //         }
            //     }
            // }
            //
            // text.text = "GLTFs: " + gltfCount + "    AB: " + abCount;
        }

        private void RemoveGlobalDetectionPainting()
        {
            
        }
        
        private void ApplyDetectionPaintingForCurrentScene()
        {
            
        }
        
        private void RemoveDetectionPaintingForCurrentScene()
        {
            
        }

      


        public void Dispose()
        {
            _debugConfig.showGlobalABDetectionLayer.OnChange -= OnGlobalABDetectionChanged;
            _debugConfig.showSceneABDetectionLayer.OnChange -= OnSceneABDetectionChanged;
        }
    }
}