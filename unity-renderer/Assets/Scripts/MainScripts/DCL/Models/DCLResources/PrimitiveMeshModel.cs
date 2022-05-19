using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using UnityEngine;

public class PrimitiveMeshModel 
{
    public static bool operator ==(PrimitiveMeshModel left, PrimitiveMeshModel right) { return Equals(left, right); }
    public static bool operator !=(PrimitiveMeshModel left, PrimitiveMeshModel right) { return !Equals(left, right); }

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
    public RepeatedField<float> uvs;
    
    protected bool Equals(PrimitiveMeshModel other)
    {
        return type == other.type && Equals(uvs, other.uvs);
    }
    
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((PrimitiveMeshModel) obj);
    }
    
    public override int GetHashCode()
    {
        unchecked
        {
            return ((int) type * 397) ^ (uvs != null ? uvs.GetHashCode() : 0);
        }
    }
}
