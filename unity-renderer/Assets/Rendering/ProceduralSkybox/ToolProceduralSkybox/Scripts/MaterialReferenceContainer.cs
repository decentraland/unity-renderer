
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkyboxMaterialData", menuName = "ScriptableObjects/SkyboxMaterialData", order = 1)]
public class MaterialReferenceContainer : ScriptableObject
{
    [System.Serializable]
    public class Mat_Layer
    {
        public int numberOfLayers;
        public Material material;
    }

    private static MaterialReferenceContainer instance;
    public static MaterialReferenceContainer i => GetOrLoad(ref instance, "Skybox Materials/SkyboxMaterialData");

    private static T GetOrLoad<T>(ref T variable, string path) where T : Object
    {
        if (variable == null)
        {
            variable = Resources.Load<T>(path);
        }

        return variable;
    }

    public Mat_Layer[] materials;

    public Material GetMaterialForLayers(int numOfLayer)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (numOfLayer <= materials[i].numberOfLayers)
            {
                return materials[i].material;
            }
        }
        return null;
    }
}
