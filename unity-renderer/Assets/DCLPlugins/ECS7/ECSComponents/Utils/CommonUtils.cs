using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.ECSComponents
{
    public static class CommonUtils 
    {
        public static PBOnPointerResult GetPointerResultModel(string buttonId, string identifier, long entityId, string meshName, Ray ray, HitInfo hit)
        {
            PBOnPointerResult result = new PBOnPointerResult();
            result.Button = buttonId;
            result.Identifier = identifier;
            result.Direction = UnityVectorToPBVector(ray.direction);
            result.Distance = hit.distance;
            result.Normal = UnityVectorToPBVector(hit.normal);
            result.Origin = UnityVectorToPBVector(ray.origin);
            result.Point = UnityVectorToPBVector(hit.point);
            result.EntityId = entityId;
            result.MeshName = meshName;
            return result;
        }
        
        public static Vector3 UnityVectorToPBVector(UnityEngine.Vector3 original)
        {
            Vector3 vector = new Vector3();
            vector.X = original.x;
            vector.Y = original.y;
            vector.Z = original.z;
            return vector;
        }
    }
}
