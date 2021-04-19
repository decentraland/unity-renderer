using UnityEngine;

[System.Serializable]
public struct NFTShapeMaterial
{
    public enum MaterialType { BACKGROUND, FRAME, IMAGE };

    public Material material;
    public MaterialType type;
}
