using UnityEngine;

namespace DCL.Components
{
    public static class DCLTransformUtils
    {
        public static void DecodeTransform(string payload, ref DCLTransform.Model model)
        {
            byte[] bytes = System.Convert.FromBase64String(payload);
            DCL.Interface.PB_Transform pbTransform = DCL.Interface.PB_Transform.Parser.ParseFrom(bytes);
            model.position = new Vector3(pbTransform.Position.X, pbTransform.Position.Y, pbTransform.Position.Z);
            model.scale = new Vector3(pbTransform.Scale.X, pbTransform.Scale.Y, pbTransform.Scale.Z);
            model.rotation = new Quaternion((float) pbTransform.Rotation.X, (float) pbTransform.Rotation.Y,
                (float) pbTransform.Rotation.Z, (float) pbTransform.Rotation.W);
        }
    }
}