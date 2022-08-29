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
        public readonly object properties;

        // Do not use constructor directly, use `Create` method instead
        private AssetPromise_PrimitiveMesh_Model(PrimitiveType type, object meshProperties)
        {
            this.type = type;
            this.properties = meshProperties;
        }

        private static AssetPromise_PrimitiveMesh_Model Create<T>(PrimitiveType type, T properties) where T : struct
        {
            return new AssetPromise_PrimitiveMesh_Model(type, properties);
        }

        public static AssetPromise_PrimitiveMesh_Model CreateBox(IList<float> uvs)
        {
            return Create(PrimitiveType.Box, new PropertyUvs(uvs));
        }

        public static AssetPromise_PrimitiveMesh_Model CreatePlane(IList<float> uvs)
        {
            return Create(PrimitiveType.Plane, new PropertyUvs(uvs));
        }

        public static AssetPromise_PrimitiveMesh_Model CreateSphere()
        {
            return Create(PrimitiveType.Sphere, new PropertyEmpty());
        }

        public static AssetPromise_PrimitiveMesh_Model CreateCylinder(float radiusTop, float radiusBottom)
        {
            return Create(PrimitiveType.Cylinder, new PropertyCylinder(radiusTop, radiusBottom));
        }
    }

    internal readonly struct PropertyEmpty { }

    internal readonly struct PropertyUvs
    {
        public readonly IList<float> uvs;

        public PropertyUvs(IList<float> uvs)
        {
            this.uvs = uvs;
        }
    }

    internal readonly struct PropertyCylinder
    {
        public readonly float radiusTop;
        public readonly float radiusBottom;

        public PropertyCylinder(float radiusTop, float radiusBottom)
        {
            this.radiusTop = radiusTop;
            this.radiusBottom = radiusBottom;
        }
    }
}