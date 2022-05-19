using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSSphereShape
    {
        public bool withCollisions = true;
        public bool isPointerBlocker = true;
        public bool visible = true;
    }

    public static class ECSShpereShapeSerialization
    {
        public static string Serialize(ECSSphereShape component) { return JsonUtility.ToJson(component); }

        public static ECSSphereShape Deserialize(object data) { return JsonUtility.FromJson<ECSSphereShape>(data as string); }
    }
}
