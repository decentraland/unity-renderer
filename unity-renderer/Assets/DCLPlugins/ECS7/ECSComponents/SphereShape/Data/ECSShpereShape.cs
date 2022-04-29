using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ECSShpereShape 
{
    public bool withCollisions = true;
    public bool isPointerBlocker = true;
    public bool visible = true;
}

public static class ECSShpereShapeSerialization
{
    public static string Serialize(ECSShpereShape component)
    {
        return JsonUtility.ToJson(component);
    }

    public static ECSShpereShape Deserialize(object data)
    {
        return JsonUtility.FromJson<ECSShpereShape>(data as string);
    }
}
