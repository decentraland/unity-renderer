using System;
using System.Collections;
using DCL.Controllers;
using DCL.Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityGLTF;

namespace DCL.Helpers {
  public class MaterialComponentHelpers {

    public static void UpdateEntityMaterial(DecentralandEntity currentEntity, DecentralandEntity.Material newMaterial) {
      bool usesBasicMaterial = newMaterial.tag != "material";
      var meshRenderer = currentEntity.gameObject.GetComponent<MeshRenderer>();

      if (usesBasicMaterial) {
        LoadBasicMaterial(meshRenderer, newMaterial);
      } else {
        LoadMaterial(meshRenderer, newMaterial);
      }
    }

    static void LoadBasicMaterial(MeshRenderer currentEntityRenderer, DecentralandEntity.Material newMaterial) {
      currentEntityRenderer.material = Resources.Load<Material>("Materials/BasicShapeMaterial");

      if (!string.IsNullOrEmpty(newMaterial.texture)) {
        SceneController.Instance.StartCoroutine(FetchTexture(newMaterial.texture, (fetchedTexture) => {
          currentEntityRenderer.material.mainTexture = fetchedTexture;

          // WRAP MODE CONFIGURATION
          switch (newMaterial.wrap) {
            case 2:
              currentEntityRenderer.material.mainTexture.wrapMode = TextureWrapMode.Repeat;
              break;
            case 3:
              currentEntityRenderer.material.mainTexture.wrapMode = TextureWrapMode.Mirror;
              break;
            default:
              currentEntityRenderer.material.mainTexture.wrapMode = TextureWrapMode.Clamp;
              break;
          }

          // SAMPLING/FILTER MODE CONFIGURATION
          switch (newMaterial.wrap) {
            case 2:
              currentEntityRenderer.material.mainTexture.filterMode = FilterMode.Bilinear;
              break;
            case 3:
              currentEntityRenderer.material.mainTexture.filterMode = FilterMode.Trilinear;
              break;
            default:
              currentEntityRenderer.material.mainTexture.filterMode = FilterMode.Point;
              break;
          }

          // ALPHA CONFIGURATION
          currentEntityRenderer.material.SetFloat("_AlphaClip", newMaterial.alphaTest);
        }));
      }
    }

    // TODO: Figure out how to deal with parsedEntity's alphaTexture (Make custom LWRP shader?), ambientColor, reflectionColor and refractionTexture
    static void LoadMaterial(MeshRenderer currentEntityRenderer, DecentralandEntity.Material newMaterial) {
      Color auxColor = new Color();
      var disableLighting = newMaterial.disableLighting;
      var albedoColor = newMaterial.albedoColor;
      var alpha = newMaterial.alpha;
      var albedoTexture = newMaterial.albedoTexture;
      var hasAlpha = newMaterial.hasAlpha;
      var alphaTexture = newMaterial.alphaTexture;
      var bumpTexture = newMaterial.bumpTexture;
      var transparencyMode = newMaterial.transparencyMode;

      if (!disableLighting) {
        currentEntityRenderer.material = Resources.Load<Material>("Materials/ShapeMaterial");

        var emissiveTexture = newMaterial.emissiveTexture;
        var emissiveColor = newMaterial.emissiveColor;
        var emissiveIntensity = newMaterial.emissiveIntensity;
        var metallic = newMaterial.metallic;
        var roughness = newMaterial.roughness;
        var microSurface = newMaterial.microSurface;
        var specularIntensity = newMaterial.specularIntensity;
        var reflectivityColor = newMaterial.reflectivityColor;
        var directIntensity = newMaterial.directIntensity;

        // FETCH AND LOAD EMISSIVE TEXTURE
        if (!string.IsNullOrEmpty(emissiveTexture)) {
          SceneController.Instance.StartCoroutine(FetchTexture(emissiveTexture, (fetchedEmissiveTexture) => {
            currentEntityRenderer.material.SetTexture("_EmissionMap", fetchedEmissiveTexture);
          }));
        }

        // METALLIC/SPECULAR CONFIGURATIONS
        ColorUtility.TryParseHtmlString(emissiveColor, out auxColor);
        currentEntityRenderer.material.SetColor("_EmissionColor", auxColor * emissiveIntensity);

        ColorUtility.TryParseHtmlString(reflectivityColor, out auxColor);
        currentEntityRenderer.material.SetColor("_SpecColor", auxColor);

        currentEntityRenderer.material.SetFloat("_Metallic", metallic);
        currentEntityRenderer.material.SetFloat("_Glossiness", 1 - roughness);
        currentEntityRenderer.material.SetFloat("_GlossyReflections", microSurface);
        currentEntityRenderer.material.SetFloat("_SpecularHighlights", specularIntensity * directIntensity);
      } else {
        currentEntityRenderer.material = Resources.Load<Material>("Materials/BasicShapeMaterial");
      }

      ColorUtility.TryParseHtmlString(albedoColor, out auxColor);
      currentEntityRenderer.material.SetColor("_Color", auxColor);

      // FETCH AND LOAD TEXTURES
      if (!string.IsNullOrEmpty(albedoTexture)) {
        SceneController.Instance.StartCoroutine(FetchTexture(albedoTexture, (fetchedAlbedoTexture) => {
          currentEntityRenderer.material.mainTexture = fetchedAlbedoTexture;
        }));
      }

      if (!string.IsNullOrEmpty(bumpTexture)) {
        SceneController.Instance.StartCoroutine(FetchTexture(bumpTexture, (fetchedBumpTexture) => {
          currentEntityRenderer.material.SetTexture("_BumpMap", fetchedBumpTexture);
        }));
      }

      // ALPHA CONFIGURATION
      currentEntityRenderer.material.SetFloat("_AlphaClip", alpha);

      if (hasAlpha || !string.IsNullOrEmpty(alphaTexture)) {
        // Reset shader keywords
        currentEntityRenderer.material.DisableKeyword("_ALPHATEST_ON"); // Cut Out Transparency
        currentEntityRenderer.material.DisableKeyword("_ALPHABLEND_ON"); // Fade Transparency
        currentEntityRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON"); // Transparent

        switch (transparencyMode) {
          case 2: // ALPHABLEND
            currentEntityRenderer.material.EnableKeyword("_ALPHABLEND_ON");

            currentEntityRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            currentEntityRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            currentEntityRenderer.material.SetInt("_ZWrite", 0);
            currentEntityRenderer.material.renderQueue = 3000;
            break;
          case 3: // ALPHATESTANDBLEND
            currentEntityRenderer.material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

            currentEntityRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            currentEntityRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            currentEntityRenderer.material.SetInt("_ZWrite", 0);
            currentEntityRenderer.material.renderQueue = 3000;
            break;
          default: // ALPHATEST
            currentEntityRenderer.material.EnableKeyword("_ALPHATEST_ON");

            currentEntityRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            currentEntityRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            currentEntityRenderer.material.SetInt("_ZWrite", 1);
            currentEntityRenderer.material.renderQueue = 2450;
            break;
        }
      }
    }

    static IEnumerator FetchTexture(string textureURL, Action<Texture> callback) {
      if (!string.IsNullOrEmpty(textureURL) && callback != null) {
        using (var webRequest = UnityWebRequestTexture.GetTexture(textureURL)) {
          yield return webRequest.SendWebRequest();

          if (webRequest.isNetworkError || webRequest.isHttpError) {
            Debug.Log("Fetching texture failed: " + webRequest.error);
          } else {
            callback(DownloadHandlerTexture.GetContent(webRequest));
          }
        }
      } else {
        Debug.Log("Can't fetch texture as the url or callback is empty");
      }
    }

    public static void InstantiateEntityWithMaterial(ParcelScene scene, string entityId, Vector3 position, string materialComponentJSON) {
      scene.CreateEntity(entityId);

      scene.UpdateEntityComponent(JsonConvert.SerializeObject(new {
        entityId = entityId,
        name = "shape",
        json = JsonConvert.SerializeObject(new {
          tag = "plane"
        })
      }));

      scene.UpdateEntityComponent(JsonConvert.SerializeObject(new {
        entityId = entityId,
        name = "transform",
        json = JsonConvert.SerializeObject(new {
          position = position,
          scale = new Vector3(1, 1, 1)
        })
      }));

      scene.UpdateEntityComponent(materialComponentJSON);
    }
  }
}