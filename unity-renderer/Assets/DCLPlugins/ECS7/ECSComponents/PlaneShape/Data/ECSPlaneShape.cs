using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSPlaneShape
    {
        public RepeatedField<float> uvs;

        public bool withCollisions = true;
        public bool isPointerBlocker = true;
        public bool visible = true;
    }

    public static class ECSPlaneShapeeSerialization
    {
        public static string Serialize(ECSPlaneShape component) { return JsonUtility.ToJson(component); }

        public static ECSPlaneShape Deserialize(object data) { return JsonUtility.FromJson<ECSPlaneShape>(data as string); }
    }
}