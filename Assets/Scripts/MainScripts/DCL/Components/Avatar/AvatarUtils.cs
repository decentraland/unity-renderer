using DCL;
using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AvatarUtils
{
    public static int _Color = Shader.PropertyToID("_Color");
    public static int _EmissionColor = Shader.PropertyToID("_EmissionColor");
    public static int _MainTex = Shader.PropertyToID("_MainTex");
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
                string materialName = m.name.ToLower();

                if (string.IsNullOrEmpty(materialsContainingThisName) || materialName.Contains(materialsContainingThisName.ToLower()))
                {
                    string newMatName = sharedMats[i1].name;
                    Material newMat = mapFunction.Invoke(sharedMats[i1]);
                    newMat.name = newMatName;
                    if (AvatarShape.VERBOSE) Debug.Log("Replacing " + m.name + " by " + newMat.name);
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
                                           string shaderId = "_Color")
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
            (mat) =>
            {
                return replaceThemWith;
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
                copy.SetTexture(_MainTex, mat.GetTexture(_MainTex));
                copy.SetColor(_Color, mat.GetColor(_Color));
                copy.SetColor(_EmissionColor, mat.GetColor(_EmissionColor));
                result.Add(copy);
                return copy;
            },
            materialsContainingThisName);

        return result;
    }



    public static IEnumerator FetchFaceUrl(AvatarShape.Model.Face face, string baseUrl, System.Action<Texture, Texture> OnSuccess)
    {
        string mainTextureUrl = baseUrl + face.texture;
        string maskUrl = baseUrl + face.mask;
        Texture loadedTexture = null;
        Texture loadedMask = null;

        if (!string.IsNullOrEmpty(mainTextureUrl))
        {
            yield return Utils.FetchTexture(mainTextureUrl, (tex) =>
            {
                loadedTexture = tex;
            });
        }

        if (!string.IsNullOrEmpty(maskUrl))
        {
            yield return Utils.FetchTexture(maskUrl, (tex) =>
            {
                loadedMask = tex;
            });
        }

        OnSuccess?.Invoke(loadedTexture, loadedMask);
    }

}
