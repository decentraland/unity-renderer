using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSBoxShape
    {
        public float[] uvs;

        public bool withCollisions = true;
        public bool isPointerBlocker = true;
        public bool visible = true;
    }

    public static class ECSBoxShapeSerialization
    {
        public static string Serialize(ECSBoxShape component) { return JsonUtility.ToJson(component); }

        public static ECSBoxShape Deserialize(object data) { return JsonUtility.FromJson<ECSBoxShape>(data as string); }
    }
}