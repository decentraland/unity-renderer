using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using UnityEngine;

public class ABGLTF : MonoBehaviour
{
    public Material gltfMaterial;
    public Material abMaterial;

    private float lastTime = 0;

    private Dictionary<Renderer, Material[]> rendererDict  = new Dictionary<Renderer, Material[]>();


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.A))
        {
            if (lastTime + 0.8f <= Time.timeSinceLevelLoad)
            {
                ChangeGLTFABMaterialsOfCurrentScene();
            }
        }
        
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Q))
        {
            if (lastTime + 0.8f <= Time.timeSinceLevelLoad)
            {
                ChangeGLTFABMaterials();
            }
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.F))
        {
            if (lastTime + 0.8f <= Time.timeSinceLevelLoad)
            {
                RevertChanges();
            }
        }
    }

    void ChangeGLTFABMaterials()
    {
        lastTime = Time.timeSinceLevelLoad;
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
    
    void ChangeGLTFABMaterialsOfCurrentScene()
    {
        lastTime = Time.timeSinceLevelLoad;
        var currentScene = FindSceneForPlayer();
        var sceneTransform = currentScene.GetSceneTransform();

        ChangeMaterials(sceneTransform);
    }

    void ChangeMaterials(Transform transform)
    {
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var childTransform = transform.GetChild(i).transform;
                if (childTransform.gameObject.name.Contains("GLTF Shape"))
                {
                    var childGameObject = childTransform.GetChild(0).gameObject;
                    if (childGameObject.name.Contains("AB: "))
                    {
                        var renderers = childGameObject.GetComponentsInChildren<Renderer>();
                  
                        foreach (Renderer renderer in renderers)
                        {
                            if(!rendererDict.ContainsKey(renderer))
                                rendererDict.Add(renderer,renderer.materials);
                            renderer.material = abMaterial;
                        }
                    }
                    else
                    {
                        var renderers = childGameObject.GetComponentsInChildren<Renderer>();
                        foreach (Renderer renderer in renderers)
                        {
                            if(!rendererDict.ContainsKey(renderer))
                                rendererDict.Add(renderer,renderer.materials);
                            renderer.material = gltfMaterial;
                        }
                    }
                }
                else
                {
                    ChangeMaterials(childTransform);
                }
            }
        }
    }

    void RevertChanges()
    {
        foreach (KeyValuePair<Renderer,Material[]> keyValuePair in rendererDict)
        {
            keyValuePair.Key.materials = keyValuePair.Value;
        }
        
        rendererDict.Clear();
    }
    
    public IParcelScene FindSceneForPlayer()
    {
        foreach (IParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
        {
            if (WorldStateUtils.IsCharacterInsideScene(scene))
                return scene;
        }

        return null;
    }
}
