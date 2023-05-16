using DCL.CameraTool;
using DCL.Configuration;
using DCL.Helpers;
using DCL.UIElements.Structures;
using Google.Protobuf.Collections;
using JetBrains.Annotations;
using Decentraland.Common;
using UnityEngine;
using Vector3 = Decentraland.Common.Vector3;

namespace DCL.ECSComponents
{
    public static class ProtoConvertUtils
    {
        public static RaycastHit ToPBRaycasHit(long entityId, string meshName, Ray ray, HitInfo rawHit)
        {
            var hit = new RaycastHit();
            hit.Length = rawHit.distance;
            hit.GlobalOrigin = UnityVectorToPBVector(ray.origin);
            hit.EntityId = (uint)entityId;
            hit.MeshName = meshName;
            hit.Position = UnityVectorToPBVector(rawHit.point);
            hit.NormalHit = UnityVectorToPBVector(rawHit.normal);
            hit.Direction = UnityVectorToPBVector(ray.direction);

            return hit;
        }

        public static RaycastHit ToPBRaycasHit(long entityId, string meshName, Ray ray,
            float hitDistance, UnityEngine.Vector3 hitPoint, UnityEngine.Vector3 hitNormal)
        {
            var ret = new RaycastHit
            {
                Length = hitDistance,
                GlobalOrigin = UnityVectorToPBVector(ray.origin),
                Position = UnityVectorToPBVector(hitPoint),
                NormalHit = UnityVectorToPBVector(hitNormal),
                Direction = UnityVectorToPBVector(ray.direction),
                EntityId = (uint)entityId
            };

            if (!string.IsNullOrEmpty(meshName))
            {
                ret.MeshName = meshName;
            }

            return ret;
        }

        public static Vector3 UnityVectorToPBVector(UnityEngine.Vector3 original)
        {
            Vector3 vector = new Vector3();
            vector.X = original.x;
            vector.Y = original.y;
            vector.Z = original.z;
            return vector;
        }

        public static UnityEngine.Vector3 PBVectorToUnityVector(Vector3 original)
        {
            UnityEngine.Vector3 vector = new UnityEngine.Vector3();
            vector.x = original.X;
            vector.y = original.Y;
            vector.z = original.Z;
            return vector;
        }

        public static CameraMode.ModeId PBCameraEnumToUnityEnum(CameraType mode)
        {
            switch (mode)
            {
                case CameraType.CtFirstPerson:
                    return CameraMode.ModeId.FirstPerson;
                case CameraType.CtThirdPerson:
                    return CameraMode.ModeId.ThirdPerson;
                default:
                    return CommonScriptableObjects.cameraMode.Get();
            }
        }

        public static CameraType UnityEnumToPBCameraEnum(CameraMode.ModeId mode)
        {
            switch (mode)
            {
                case CameraMode.ModeId.FirstPerson:
                    return CameraType.CtFirstPerson;
                case CameraMode.ModeId.ThirdPerson:
                default:
                    return CameraType.CtThirdPerson;
            }
        }

        public static Color ToUnityColor(this Color3 color)
        {
            return new Color(color.R, color.G, color.B);
        }

        public static Color ToUnityColor(this Color4 color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }

        public static string ToFontName(this Font font)
        {
            // TODO: add support for the rest of the fonts and discuss old font deprecation
            const string SANS_SERIF = "SansSerif";

            switch (font)
            {
                case Font.FSansSerif:
                    return SANS_SERIF;
                default:
                    return SANS_SERIF;
            }
        }

        public static TextAnchor ToUnityTextAlign(this TextAlignMode align)
        {
            switch (align)
            {
                case TextAlignMode.TamTopCenter:
                    return TextAnchor.UpperCenter;
                case TextAlignMode.TamTopLeft:
                    return TextAnchor.UpperLeft;
                case TextAlignMode.TamTopRight:
                    return TextAnchor.UpperRight;

                case TextAlignMode.TamBottomCenter:
                    return TextAnchor.LowerCenter;
                case TextAlignMode.TamBottomLeft:
                    return TextAnchor.LowerLeft;
                case TextAlignMode.TamBottomRight:
                    return TextAnchor.LowerRight;

                case TextAlignMode.TamMiddleCenter:
                    return TextAnchor.MiddleCenter;
                case TextAlignMode.TamMiddleLeft:
                    return TextAnchor.MiddleLeft;
                case TextAlignMode.TamMiddleRight:
                    return TextAnchor.MiddleRight;

                default:
                    return TextAnchor.MiddleCenter;
            }
        }

        public static Vector4 ToUnityBorder([CanBeNull] this BorderRect rect) =>
            rect == null ? Vector4.zero : new Vector4(rect.Left, rect.Top, rect.Right, rect.Bottom);

        public static DCLUVs ToDCLUVs([CanBeNull] this RepeatedField<float> uvs) =>
            uvs is not { Count: 8 }
                ? DCLUVs.Default
                : new DCLUVs(
                    new UnityEngine.Vector2(uvs[0], uvs[1]),
                    new UnityEngine.Vector2(uvs[2], uvs[3]),
                    new UnityEngine.Vector2(uvs[4], uvs[5]),
                    new UnityEngine.Vector2(uvs[6], uvs[7]));

        public static DCLImageScaleMode ToDCLImageScaleMode(this BackgroundTextureMode textureMode)
        {
            return textureMode switch
                   {
                       BackgroundTextureMode.Center => DCLImageScaleMode.CENTER,
                       BackgroundTextureMode.Stretch => DCLImageScaleMode.STRETCH,
                       BackgroundTextureMode.NineSlices => DCLImageScaleMode.NINE_SLICES,
                       _ => DCLImageScaleMode.STRETCH
                   };
        }
    }
}
