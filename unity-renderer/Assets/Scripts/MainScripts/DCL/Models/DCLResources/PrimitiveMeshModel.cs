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
    public object primitiveModel;
    
    protected bool Equals(PrimitiveMeshModel other)
    {
        return type == other.type && Equals(primitiveModel, other.primitiveModel);
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
            return ((int) type * 397) ^ (primitiveModel != null ? primitiveModel.GetHashCode() : 0);
        }
    }
}
