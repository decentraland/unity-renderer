using System;
using DCL.ECSRuntime.Models;
using DCL.Interface;
using UnityEngine;

namespace DCL.ECSRuntime.Deserializers
{
    public static class TransformDeserializer
    {
        public static ECSTransform Deserialize(string data)
        {
            var model = new ECSTransform();
            byte[] bytes = Convert.FromBase64String(data);
            PB_Transform pbTransform = PB_Transform.Parser.ParseFrom(bytes);
            model.position = new Vector3(pbTransform.Position.X, pbTransform.Position.Y, pbTransform.Position.Z);
            model.scale = new Vector3(pbTransform.Scale.X, pbTransform.Scale.Y, pbTransform.Scale.Z);
            model.rotation = new Quaternion((float)pbTransform.Rotation.X, (float)pbTransform.Rotation.Y, (float)pbTransform.Rotation.Z, (float)pbTransform.Rotation.W);
            return model;
        }
    }
}