using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Decentraland.Ecs;
using UnityEngine;

public class ECSBoxShape 
{
    public ECSBoxShape()
    {
    
    }
    
    public ECSBoxShape(PBBoxShape pb) 
    {
        uvs = pb.Uvs.ToArray();
        withCollisions = pb.WithCollisions;
        isPointerBlocker = pb.IsPointerBlocker;
        visible = pb.Visible;
    }
    
    public float[] uvs;
    
    public bool withCollisions = true;
    public bool isPointerBlocker = true;
    public bool visible = true;
}

public static class ECSBoxShapeSerialization
{
    public static string Serialize(ECSBoxShape component)
    {
        return JsonUtility.ToJson(component);
    }

    public static ECSBoxShape Deserialize(object data)
    {
        return JsonUtility.FromJson<ECSBoxShape>(data as string);
    }
}