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
        Sphere,
        Plane,
        Cylinder
    }

    public PrimitiveMeshModel(Type type)
    {
        this.type = type;
    }

    public Type type;
    public float radiusTop;
    public float radiusBottom;
    public RepeatedField<float> uvs = new RepeatedField<float>();
    
    protected bool Equals(PrimitiveMeshModel other)
    {
        return type == other.type && radiusTop.Equals(other.radiusTop) && radiusBottom.Equals(other.radiusBottom) && Equals(uvs, other.uvs);
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
            int hashCode = (int) type;
            hashCode = (hashCode * 397) ^ radiusTop.GetHashCode();
            hashCode = (hashCode * 397) ^ radiusBottom.GetHashCode();
            hashCode = (hashCode * 397) ^ (uvs != null ? uvs.GetHashCode() : 0);
            return hashCode;
        }
    }
}
