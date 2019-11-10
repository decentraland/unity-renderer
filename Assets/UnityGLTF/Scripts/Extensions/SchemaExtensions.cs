using GLTF;
using GLTF.Schema;
using UnityEngine;

namespace UnityGLTF.Extensions
{
    public static class SchemaExtensions
    {
        /// <summary>
        /// Define the transformation between Unity coordinate space and glTF.
        /// glTF is a right-handed coordinate system, where the 'right' direction is -X relative to
        /// Unity's coordinate system.
        /// glTF matrix: column vectors, column-major storage, +Y up, +Z forward, -X right, right-handed
        /// unity matrix: column vectors, column-major storage, +Y up, +Z forward, +X right, left-handed
        /// multiply by a negative X scale to convert handedness
        /// </summary>
        public static readonly Vector3 CoordinateSpaceConversionScale = new Vector3(-1, 1, 1);

        /// <summary>
        /// Define whether the coordinate space scale conversion above means we have a change in handedness.
        /// This is used when determining the conventional direction of rotation - the right-hand rule states
        /// that rotations are clockwise in left-handed systems and counter-clockwise in right-handed systems.
        /// Reversing the direction of one or three axes of reverses the handedness.
        /// </summary>
        public static bool CoordinateSpaceConversionRequiresHandednessFlip
        {
            get
            {
                return CoordinateSpaceConversionScale.x * CoordinateSpaceConversionScale.y * CoordinateSpaceConversionScale.z < 0.0f;
            }
        }

        public static readonly Vector4 TangentSpaceConversionScale = new Vector4(-1, 1, 1, -1);

        /// <summary>
        /// Get the converted unity translation, rotation, and scale from a gltf node
        /// </summary>
        /// <param name="node">gltf node</param>
        /// <param name="position">unity translation vector</param>
        /// <param name="rotation">unity rotation quaternion</param>
        /// <param name="scale">unity scale vector</param>
        public static void GetUnityTRSProperties(this Node node, out Vector3 position, out Quaternion rotation,
            out Vector3 scale)
        {
            if (!node.UseTRS)
            {
                Matrix4x4 unityMat = node.Matrix.ToMatrix4x4Convert();
                unityMat.GetTRSProperties(out position, out rotation, out scale);
            }
            else
            {
                position = Vector3.Scale(node.Translation, CoordinateSpaceConversionScale);
                rotation = node.Rotation.ToUnityQuaternionConvert();
                scale = node.Scale;
            }
        }

        /// <summary>
        /// Set a gltf node's converted translation, rotation, and scale from a unity transform
        /// </summary>
        /// <param name="node">gltf node to modify</param>
        /// <param name="transform">unity transform to convert</param>
        public static void SetUnityTransform(this Node node, Transform transform)
        {
            node.Translation = Vector3.Scale(transform.localPosition, CoordinateSpaceConversionScale);
            node.Rotation = transform.localRotation.ToGltfQuaternionConvert();
            node.Scale = transform.localScale;
        }

        // todo: move to utility class
        /// <summary>
        /// Get unity translation, rotation, and scale from a unity matrix
        /// </summary>
        /// <param name="mat">unity matrix to get properties from</param>
        /// <param name="position">unity translation vector</param>
        /// <param name="rotation">unity rotation quaternion</param>
        /// <param name="scale">unity scale vector</param>
        public static void GetTRSProperties(this Matrix4x4 mat, out Vector3 position, out Quaternion rotation,
            out Vector3 scale)
        {
            position = mat.GetColumn(3);

            Vector3 x = mat.GetColumn(0);
            Vector3 y = mat.GetColumn(1);
            Vector3 z = mat.GetColumn(2);
            Vector3 calculatedZ = Vector3.Cross(x, y);
            bool mirrored = Vector3.Dot(calculatedZ, z) < 0.0f;

            scale = new Vector3(x.magnitude * (mirrored ? -1.0f : 1.0f), y.magnitude, z.magnitude);

            rotation = Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));
        }

        /// <summary>
        /// Get converted unity translation, rotation, and scale from a gltf matrix
        /// </summary>
        /// <param name="gltfMat">gltf matrix to get and convert properties from</param>
        /// <param name="position">unity translation vector</param>
        /// <param name="rotation">unity rotation quaternion</param>
        /// <param name="scale">unity scale vector</param>
        public static void ConvertAndGetTRSProperties(this Matrix4x4 gltfMat, out Vector3 position, out Quaternion rotation,
            out Vector3 scale)
        {
            gltfMat.ToMatrix4x4Convert().GetTRSProperties(out position, out rotation, out scale);
        }

        /// <summary>
        /// Get a gltf column vector from a gltf matrix
        /// </summary>
        /// <param name="mat">gltf matrix</param>
        /// <param name="columnNum">the specified column vector from the matrix</param>
        /// <returns></returns>
        public static Vector4 GetColumn(this Matrix4x4 mat, uint columnNum)
        {
            switch (columnNum)
            {
                case 0:
                    {
                        return new Vector4(mat.m00, mat.m21, mat.m31, mat.m30);
                    }
                case 1:
                    {
                        return new Vector4(mat.m01, mat.m22, mat.m32, mat.m31);
                    }
                case 2:
                    {
                        return new Vector4(mat.m02, mat.m12, mat.m22, mat.m32);
                    }
                case 3:
                    {
                        return new Vector4(mat.m03, mat.m13, mat.m23, mat.m33);
                    }
                default:
                    throw new System.Exception("column num is out of bounds");
            }
        }

        /// <summary>
        /// Convert gltf quaternion to a unity quaternion
        /// </summary>
        /// <param name="gltfQuat">gltf quaternion</param>
        /// <returns>unity quaternion</returns>
        public static Quaternion ToUnityQuaternionConvert(this Quaternion gltfQuat)
        {
            Vector3 fromAxisOfRotation = new Vector3(gltfQuat.x, gltfQuat.y, gltfQuat.z);
            float axisFlipScale = CoordinateSpaceConversionRequiresHandednessFlip ? -1.0f : 1.0f;
            Vector3 toAxisOfRotation = axisFlipScale * Vector3.Scale(fromAxisOfRotation, CoordinateSpaceConversionScale);

            return new Quaternion(toAxisOfRotation.x, toAxisOfRotation.y, toAxisOfRotation.z, gltfQuat.w);
        }

        /// <summary>
        /// Convert gltf Vector3 to unity Vector3
        /// </summary>
        /// <param name="gltfVec3">gltf vector3</param>
        /// <returns>unity vector3</returns>
        public static Vector3 ToUnityVector3Convert(this Vector3 gltfVec3)
        {
            Vector3 coordinateSpaceConversionScale = CoordinateSpaceConversionScale;
            Vector3 unityVec3 = Vector3.Scale(gltfVec3, coordinateSpaceConversionScale);
            return unityVec3;
        }

        /// <summary>
        /// Convert unity quaternion to a gltf quaternion
        /// </summary>
        /// <param name="unityQuat">unity quaternion</param>
        /// <returns>gltf quaternion</returns>
        public static Quaternion ToGltfQuaternionConvert(this Quaternion unityQuat)
        {
            Vector3 fromAxisOfRotation = new Vector3(unityQuat.x, unityQuat.y, unityQuat.z);
            fromAxisOfRotation.x *= -1;

            float axisFlipScale = CoordinateSpaceConversionRequiresHandednessFlip ? -1.0f : 1.0f;
            Vector3 toAxisOfRotation = axisFlipScale * fromAxisOfRotation;

            return new Quaternion(toAxisOfRotation.x, toAxisOfRotation.y, toAxisOfRotation.z, unityQuat.w);
        }

        /// <summary>
        /// Convert gltf matrix to a unity matrix
        /// </summary>
        /// <param name="gltfMat">gltf matrix</param>
        /// <returns>unity matrix</returns>
        public static Matrix4x4 ToMatrix4x4Convert(this Matrix4x4 rawUnityMat)
        {
            Vector3 coordinateSpaceConversionScale = CoordinateSpaceConversionScale;
            Matrix4x4 convert = Matrix4x4.Scale(coordinateSpaceConversionScale);
            Matrix4x4 unityMat = convert * rawUnityMat * convert;
            return unityMat;
        }








        /// <summary>
        /// Flips the V component of the UV (1-V) to put from glTF into Unity space
        /// </summary>
        /// <param name="attributeAccessor">The attribute accessor to modify</param>
        public static void FlipTexCoordArrayV(ref AttributeAccessor attributeAccessor)
        {
            int arrayLength = attributeAccessor.AccessorContent.AsVec2s.Length;
            var array = attributeAccessor.AccessorContent.AsVec2s;
            for (var i = 0; i < arrayLength; i++)
            {
                array[i].y = 1.0f - array[i].y;
            }
        }

        /// <summary>
        /// Flip the V component of the UV (1-V)
        /// </summary>
        /// <param name="array">The array to copy from and modify</param>
        /// <returns>Copied Vector2 with coordinates in glTF space</returns>
        public static UnityEngine.Vector2[] FlipTexCoordArrayVAndCopy(UnityEngine.Vector2[] array)
        {
            var returnArray = new UnityEngine.Vector2[array.Length];

            for (var i = 0; i < array.Length; i++)
            {
                returnArray[i].x = array[i].x;
                returnArray[i].y = 1.0f - array[i].y;
            }

            return returnArray;
        }

        /// <summary>
        /// Converts vector3 to specified coordinate space
        /// </summary>
        /// <param name="attributeAccessor">The attribute accessor to modify</param>
        /// <param name="coordinateSpaceCoordinateScale">The coordinate space to move into</param>
        public static void ConvertVector3CoordinateSpace(ref AttributeAccessor attributeAccessor, Vector3 coordinateSpaceCoordinateScale)
        {
            int arrayLength = attributeAccessor.AccessorContent.AsVertices.Length;
            Vector3[] v3Array = attributeAccessor.AccessorContent.AsVertices;

            for (int i = 0; i < arrayLength; i++)
            {
                v3Array[i] = Vector3.Scale(v3Array[i], coordinateSpaceCoordinateScale);
            }
        }

        public static void ConvertVector3CoordinateSpaceFast(ref AttributeAccessor attributeAccessor)
        {
            int arrayLength = attributeAccessor.AccessorContent.AsVertices.Length;
            Vector3[] v3Array = attributeAccessor.AccessorContent.AsVertices;

            for (int i = 0; i < arrayLength; i++)
            {
                v3Array[i].x *= -1;
            }
        }


        public static Vector3 ToVector3Convert(this Vector3 unityVec3)
        {
            Vector3 gltfVec3 = Vector3.Scale(unityVec3, CoordinateSpaceConversionScale);
            return gltfVec3;
        }

        /// <summary>
        /// Converts and copies based on the specified coordinate scale
        /// </summary>
        /// <param name="array">The array to convert and copy</param>
        /// <param name="coordinateSpaceCoordinateScale">The specified coordinate space</param>
        /// <returns>The copied and converted coordinate space</returns>
        public static UnityEngine.Vector3[] ConvertVector3CoordinateSpaceAndCopy(Vector3[] array, Vector3 coordinateSpaceCoordinateScale)
        {
            var returnArray = new UnityEngine.Vector3[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                returnArray[i].x = array[i].x * coordinateSpaceCoordinateScale.x;
                returnArray[i].y = array[i].y * coordinateSpaceCoordinateScale.y;
                returnArray[i].z = array[i].z * coordinateSpaceCoordinateScale.z;
            }

            return returnArray;
        }

        /// <summary>
        /// Converts vector4 to specified coordinate space
        /// </summary>
        /// <param name="attributeAccessor">The attribute accessor to modify</param>
        /// <param name="coordinateSpaceCoordinateScale">The coordinate space to move into</param>
        public static void ConvertVector4CoordinateSpace(ref AttributeAccessor attributeAccessor, Vector4 coordinateSpaceCoordinateScale)
        {
            int arrayLength = attributeAccessor.AccessorContent.AsVec4s.Length;
            var array = attributeAccessor.AccessorContent.AsVec4s;
            for (int i = 0; i < arrayLength; i++)
            {
                array[i].x *= coordinateSpaceCoordinateScale.x;
                array[i].y *= coordinateSpaceCoordinateScale.y;
                array[i].z *= coordinateSpaceCoordinateScale.z;
                array[i].w *= coordinateSpaceCoordinateScale.w;
            }
        }

        public static void ConvertVector4CoordinateSpaceFast(ref AttributeAccessor attributeAccessor)
        {
            int arrayLength = attributeAccessor.AccessorContent.AsVec4s.Length;
            var array = attributeAccessor.AccessorContent.AsVec4s;
            for (int i = 0; i < arrayLength; i++)
            {
                array[i].x *= -1;
                array[i].w *= -1;
            }
        }
        /// <summary>
        /// Converts and copies based on the specified coordinate scale
        /// </summary>
        /// <param name="array">The array to convert and copy</param>
        /// <param name="coordinateSpaceCoordinateScale">The specified coordinate space</param>
        /// <returns>The copied and converted coordinate space</returns>
        public static Vector4[] ConvertVector4CoordinateSpaceAndCopy(Vector4[] array, Vector4 coordinateSpaceCoordinateScale)
        {
            var returnArray = new Vector4[array.Length];

            for (var i = 0; i < array.Length; i++)
            {
                returnArray[i].x = array[i].x * coordinateSpaceCoordinateScale.x;
                returnArray[i].y = array[i].y * coordinateSpaceCoordinateScale.y;
                returnArray[i].z = array[i].z * coordinateSpaceCoordinateScale.z;
                returnArray[i].w = array[i].w * coordinateSpaceCoordinateScale.w;
            }

            return returnArray;
        }

        /// <summary>
        /// Rewinds the indicies into Unity coordinate space from glTF space
        /// </summary>
        /// <param name="attributeAccessor">The attribute accessor to modify</param>
        public static void FlipFaces(ref AttributeAccessor attributeAccessor)
        {
            int arrayLength = attributeAccessor.AccessorContent.AsTriangles.Length;
            var array = attributeAccessor.AccessorContent.AsUInts;

            for (int i = 0; i < arrayLength; i += 3)
            {
                uint temp = array[i];
                array[i] = array[i + 2];
                array[i + 2] = temp;
            }
        }

        /// <summary>
        /// Rewinds the indices from glTF space to Unity space
        /// </summary>
        /// <param name="triangles">The indices to copy and modify</param>
        /// <returns>Indices in glTF space that are copied</returns>
        public static int[] FlipFacesAndCopy(int[] triangles)
        {
            int[] returnArr = new int[triangles.Length];
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i];
                returnArr[i] = triangles[i + 2];
                returnArr[i + 1] = triangles[i + 1];
                returnArr[i + 2] = temp;
            }

            return returnArr;
        }
    }
}
