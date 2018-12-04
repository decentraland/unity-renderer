using System;
using UnityEngine;

namespace DCL.Models {
  [Serializable]
  public class DecentralandEntity {
    // TODO: interfaces do not have logic or game objects
    // TODO: Remove the "Entity" from "entityId", EntityComponents", "EntityTransform" and "EntityShape" names
    public GameObject gameObject;
    public string entityId;
    public string parentId;
    public EntityComponents components = null;

    [Serializable]
    public class EntityComponents {
      public EntityShape shape = null;
      public Material material = null;
      public EntityTransform transform = null;
    }

    [Serializable]
    public class EntityShape {
      public string tag;
      public string src;                  // GLTF/OBJ
      public float[] uvs;                 // Plane
      public float width = 1f;            // Plane/TextShape
      public float height = 1f;           // Plane/TextShape
      public float radiusTop = 0f;        // Cone/Cylinder
      public float radiusBottom = 1f;     // Cone/Cylinder
      public float segmentsHeight = 1f;   // Cone/Cylinder
      public float segmentsRadial = 36f;  // Cone/Cylinder
      public bool openEnded = false;      // Cone/Cylinder
      public float? radius;               // Cone/Cylinder
      public float arc = 360f;            // Cone/Cylinder
      public float outlineWidth;          // TextShape
      public Color outlineColor;          // TextShape
      public Color color;                 // TextShape
      public string fontFamily;           // TextShape
      public float fontSize;              // TextShape
      public string fontWeight;           // TextShape
      public float opacity;               // TextShape
      public string value;                // TextShape
      public string lineSpacing;          // TextShape
      public float lineCount;             // TextShape
      public bool resizeToFit;            // TextShape
      public bool textWrapping;           // TextShape
      public float shadowBlur;            // TextShape
      public float shadowOffsetX;         // TextShape
      public float shadowOffsetY;         // TextShape
      public Color shadowColor;           // TextShape
      public float zIndex;                // TextShape
      public string hAlign;               // TextShape
      public string vAlign;               // TextShape
      public float paddingTop;            // TextShape
      public float paddingRight;          // TextShape
      public float paddingBottom;         // TextShape
      public float paddingLeft;           // TextShape

      public bool Equals(EntityShape b) {
        if (b == this) return true;
        if (b == null) return false;
        return this.tag == b.tag && this.src == b.src;
      }
    }

    [Serializable]
    public class Material {
      public string tag;

      // -BASIC MATERIAL- (always unlit)
      public string texture;

      [Range(1, 3)]
      public int samplingMode;  // 1: NEAREST; 2: BILINEAR; 3: TRILINEAR

      [Range(1, 3)]
      public int wrap;          // 1: CLAMP; 2: WRAP; 3: MIRROR

      [Range(0f, 1f)]
      public float alphaTest = 1f; // value that defines if a pixel is visible or invisible (no transparency gradients)

      // -MATERIAL-
      [Range(0f, 1f)]
      public float alpha = 1f;

      public string albedoColor = "#fff";
      public string albedoTexture;
      public string ambientColor = "#fff";
      public float metallic = 0.5f;
      public float roughness = 0.5f;
      public float microSurface = 1f; // Glossiness
      public float specularIntensity = 1f;
      public bool hasAlpha = false;
      public string alphaTexture;
      public string emissiveTexture;
      public string emissiveColor = "#000";
      public float emissiveIntensity = 1f;
      public string reflectionColor = "#fff"; // Specular color
      public string reflectivityColor = "#fff";
      public float directIntensity = 1f;
      public float environmentIntensity = 1f;
      public string bumpTexture;
      public string refractionTexture;
      public bool disableLighting = false;

      [Range(0, 3)]
      public int transparencyMode; // 0: OPAQUE; 1: ALPHATEST; 2: ALPHBLEND; 3: ALPHATESTANDBLEND
    }

    [Serializable]
    public class EntityTransform {
      public Vector3 position;
      public Quaternion rotation;
      public Vector3 scale;

      public void ApplyTo(GameObject gameObject) {
        if (gameObject != null) {
          var t = gameObject.transform;

          if (t.localPosition != position) {
            t.localPosition = position;
          }

          if (t.localRotation != rotation) {
            t.localRotation = rotation;
          }

          if (t.localScale != scale) {
            t.localScale = scale;
          }
        }
      }
    }
  }
}
