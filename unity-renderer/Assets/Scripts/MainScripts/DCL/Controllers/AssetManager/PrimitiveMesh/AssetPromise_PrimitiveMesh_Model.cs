using System.Collections.Generic;

namespace DCL
{
    public readonly struct AssetPromise_PrimitiveMesh_Model
    {
        public enum PrimitiveType
        {
            Box = 0,
            Sphere = 1,
            Plane = 2,
            Cylinder = 3
        }

        public readonly PrimitiveType type;
        public readonly float radiusTop;
        public readonly float radiusBottom;
        public readonly IList<float> uvs;

        public AssetPromise_PrimitiveMesh_Model(PrimitiveType type, IList<float> uvs, float radiusTop, float radiusBottom)
        {
            this.type = type;
            this.uvs = uvs;
            this.radiusTop = radiusTop;
            this.radiusBottom = radiusBottom;
        }

        public static AssetPromise_PrimitiveMesh_Model CreateBox(IList<float> uvs)
        {
            return new AssetPromise_PrimitiveMesh_Model(PrimitiveType.Box, uvs, 0, 0);
        }

        public static AssetPromise_PrimitiveMesh_Model CreatePlane(IList<float> uvs)
        {
            return new AssetPromise_PrimitiveMesh_Model(PrimitiveType.Plane, uvs, 0, 0);
        }

        public static AssetPromise_PrimitiveMesh_Model CreateSphere()
        {
            return new AssetPromise_PrimitiveMesh_Model(PrimitiveType.Sphere, null, 0, 0);
        }

        public static AssetPromise_PrimitiveMesh_Model CreateCylinder(float radiusTop, float radiusBottom)
        {
            return new AssetPromise_PrimitiveMesh_Model(PrimitiveType.Cylinder, null, radiusTop, radiusBottom);
        }
    }
}