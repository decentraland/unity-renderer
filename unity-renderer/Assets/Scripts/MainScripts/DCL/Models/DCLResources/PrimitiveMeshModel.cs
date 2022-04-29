using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveMeshModel 
{
    public enum Type
    {
        Box,
        Sphere
    }

    public PrimitiveMeshModel(Type type)
    {
        this.type = type;
    }

    public Type type;
    public float[] uvs;
}
