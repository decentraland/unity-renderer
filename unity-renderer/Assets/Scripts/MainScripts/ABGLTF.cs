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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.A))
        {
            if (lastTime + 2 <= Time.timeSinceLevelLoad)
            {
                ChangeGLTFABMaterials();
            }
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.B))
        {
            if (lastTime + 2 <= Time.timeSinceLevelLoad)
            {
                RevertChanges();
            }
        }
    }

    void ChangeGLTFABMaterials()
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
                            renderer.material = abMaterial;
                        }
                    }
                    else
                    {
                        var renderers = childGameObject.GetComponentsInChildren<Renderer>();
                        foreach (Renderer renderer in renderers)
                        {
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
