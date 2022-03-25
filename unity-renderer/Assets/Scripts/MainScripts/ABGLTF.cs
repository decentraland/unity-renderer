using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using TMPro;
using UnityEngine;

public class ABGLTF : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Material gltfMaterial;
    public Material abMaterial;

    private float lastTime = 0;

    private Dictionary<Renderer, Material[]> rendererDict  = new Dictionary<Renderer, Material[]>();

    private int abCount = 0;
    private int gltfCount = 0;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.A))
        {
            if (lastTime + 0.8f <= Time.timeSinceLevelLoad)
            {
                text.gameObject.SetActive(true);
                ChangeGLTFABMaterialsOfCurrentScene();
            }
        }
        
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Q))
        {
            if (lastTime + 0.8f <= Time.timeSinceLevelLoad)
            {
                text.gameObject.SetActive(true);
                ChangeGLTFABMaterials();
            }
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.F))
        {
            if (lastTime + 0.8f <= Time.timeSinceLevelLoad)
            {
                RevertChanges();
                text.gameObject.SetActive(false);
            }
        }
    }

    void ChangeGLTFABMaterials()
    {
        abCount = 0;
        gltfCount = 0;
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
        abCount = 0;
        gltfCount = 0;
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
                    if (childGameObject.name.Contains("AB: ") && !childGameObject.name.Contains("GLTF: "))
                    {
                        var renderers = childGameObject.GetComponentsInChildren<Renderer>(true);
                  
                        foreach (Renderer renderer in renderers)
                        {
                            if(!rendererDict.ContainsKey(renderer))
                                rendererDict.Add(renderer,renderer.materials);
                            renderer.material = abMaterial;
                        }
                        abCount++;
                    }
                    else
                    {
                        var renderers = childGameObject.GetComponentsInChildren<Renderer>(true);
                        foreach (Renderer renderer in renderers)
                        {
                            if(!rendererDict.ContainsKey(renderer))
                                rendererDict.Add(renderer,renderer.materials);
                            renderer.material = gltfMaterial;
                        }
                        
                        gltfCount++;
                    }
                }
                else
                {
                    ChangeMaterials(childTransform);
                }
            }
        }

        text.text = "GLTFs: " + gltfCount + "    AB: " + abCount;
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
