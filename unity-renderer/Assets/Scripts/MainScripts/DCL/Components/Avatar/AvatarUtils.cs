using System;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine;

public static class AvatarUtils
{
    public static int _BaseColor = Shader.PropertyToID("_BaseColor");
    public static int _EmissionColor = Shader.PropertyToID("_EmissionColor");
    public static int _BaseMap = Shader.PropertyToID("_BaseMap");
    public static int _EyesTexture = Shader.PropertyToID("_EyesTexture");
    public static int _EyeTint = Shader.PropertyToID("_EyeTint");
    public static int _IrisMask = Shader.PropertyToID("_IrisMask");
    public static int _TintMask = Shader.PropertyToID("_TintMask");

    /// <summary>
    /// This will search all the transform hierachy for sharedMaterials filtered by name, and call a map function on them.
    /// This means each material will be replaced with the function return value.
    /// </summary>
    public static void MapSharedMaterialsRecursively(Transform transformRoot,
        Func<Material, Material> mapFunction,
        string materialsContainingThisName = null)
    {
        Renderer[] renderers = transformRoot.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            Material[] sharedMats = r.sharedMaterials;

            for (int i1 = 0; i1 < sharedMats.Length; i1++)
            {
                Material m = sharedMats[i1];

                if (m == null)
                    continue;

                bool materialNameIsCorrect = true;

                if ( !string.IsNullOrEmpty(materialsContainingThisName) )
                {
                    string materialName = m.name.ToLower();
                    materialNameIsCorrect = materialName.Contains(materialsContainingThisName.ToLower());
                }

                if (!materialNameIsCorrect)
                    continue;

                string newMatName = sharedMats[i1].name;
                Material newMat = mapFunction.Invoke(sharedMats[i1]);
                newMat.name = newMatName;
                sharedMats[i1] = newMat;
            }

            r.sharedMaterials = sharedMats;
        }
    }

    /// <summary>
    /// This will search all the transform hierachy, and change _Color on all materials containing the proper name.
    /// </summary>
    /// <param name="transformRoot">Transform where to start</param>
    /// <param name="materialsContainingThisName">name to filter in materials</param>
    /// <param name="colorToChange">color to change in the renderers</param>
    public static void SetColorInHierarchy(Transform transformRoot,
        string materialsContainingThisName,
        Color colorToChange,
        int propertyId)
    {
        MapSharedMaterialsRecursively(
            transformRoot,
            (mat) =>
            {
                mat.SetColor(propertyId, colorToChange);
                return mat;
            },
            materialsContainingThisName);
    }
}