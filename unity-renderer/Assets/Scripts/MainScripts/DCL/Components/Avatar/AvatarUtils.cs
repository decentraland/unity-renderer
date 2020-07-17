using System;
using System.Collections.Generic;
using UnityEngine;

public static class AvatarUtils
{
    public static int _BaseColor = Shader.PropertyToID("_BaseColor");
    public static int _EmissionColor = Shader.PropertyToID("_EmissionColor");
    public static int _BaseMap = Shader.PropertyToID("_BaseMap");
    public static int _EyesTexture = Shader.PropertyToID("_EyesTexture");
    public static int _EyeTint = Shader.PropertyToID("_EyeTint");
    public static int _IrisMask = Shader.PropertyToID("_IrisMask");

    /// <summary>
    /// Hack to deactivate unused body parts, so they don't z-fight.
    /// Solve this removing them from the GLB later.
    /// </summary>
    public static void RemoveUnusedBodyParts_Hack(GameObject baseBody)
    {
        SkinnedMeshRenderer[] renderers = baseBody.GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            SkinnedMeshRenderer r = renderers[i];

            if (
                r.transform.parent.name.ToLower().Contains("feet")
                || r.transform.parent.name.ToLower().Contains("lbody")
                || r.transform.parent.name.ToLower().Contains("ubody")
            )
            {
                r.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// This will search all the transform hierachy for sharedMaterials filtered by name, and call a map function on them.
    /// This means each material will be replaced with the function return value.
    /// </summary>
    public static void MapSharedMaterialsRecursively(Transform transformRoot,
        Func<Material, Material> mapFunction,
        string materialsContainingThisName = null)
    {
        Renderer[] renderers = transformRoot.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            Material[] sharedMats = r.sharedMaterials;

            for (int i1 = 0; i1 < sharedMats.Length; i1++)
            {
                Material m = sharedMats[i1];

                if (m == null) continue;

                string materialName = m.name.ToLower();

                if (string.IsNullOrEmpty(materialsContainingThisName) || materialName.Contains(materialsContainingThisName.ToLower()))
                {
                    string newMatName = sharedMats[i1].name;
                    Material newMat = mapFunction.Invoke(sharedMats[i1]);
                    newMat.name = newMatName;
                    sharedMats[i1] = newMat;
                }
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
        string shaderId = "_BaseColor")
    {
        Renderer[] renderers = transformRoot.GetComponentsInChildren<Renderer>();

        int _Color = Shader.PropertyToID(shaderId);

        MapSharedMaterialsRecursively(
            transformRoot,
            (mat) =>
            {
                mat.SetColor(_Color, colorToChange);
                return mat;
            },
            materialsContainingThisName);
    }

    /// <summary>
    /// This will search all the transform hierachy for all renderers,
    /// and replace all of its materials containing the specified name by the new one.
    /// </summary>
    /// <param name="transformRoot">Transform where to start the traversal</param>
    /// <param name="replaceThemWith">material to replace them</param>
    /// <param name="materialsContainingThisName">name to filter in materials</param>
    public static void ReplaceMaterialsWithName(Transform transformRoot,
        Material replaceThemWith,
        string materialsContainingThisName = null)
    {
        MapSharedMaterialsRecursively(
            transformRoot,
            (mat) => { return replaceThemWith; },
            materialsContainingThisName);
    }


    /// <summary>
    /// This will search all the transform hierachy for all renderers,
    /// and replace all of its materials containing the specified name by the new one.
    /// </summary>
    /// <param name="transformRoot">Transform where to start the traversal</param>
    /// <param name="replaceThemWith">material to replace them</param>
    /// <param name="materialsContainingThisName">name to filter in materials</param>
    public static List<Material> ReplaceMaterialsWithCopiesOf(Transform transformRoot,
        Material replaceThemWith,
        string materialsContainingThisName = null)
    {
        List<Material> result = new List<Material>();

        MapSharedMaterialsRecursively(
            transformRoot,
            (mat) =>
            {
                Material copy = new Material(replaceThemWith);

                Texture _MatCap = null;

                if (replaceThemWith.HasProperty("_MatCap"))
                    _MatCap = replaceThemWith.GetTexture("_MatCap");

                //NOTE(Brian): This method has a bug, if the material being copied lacks a property of the source material,
                //             the source material property will get erased. It can't be added back and even the material inspector crashes.
                //             Check the comment in Lit.shader.
                copy.CopyPropertiesFromMaterial(mat);

                if (_MatCap != null)
                    copy.SetTexture("_MatCap", _MatCap);

                result.Add(copy);
                return copy;
            },
            materialsContainingThisName);

        return result;
    }
}