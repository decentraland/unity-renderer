using DCL;
using DCL.Helpers;
using UnityEngine;

internal class PrimitiveMeshFactory
{
    public static Mesh CreateMesh(AssetPromise_PrimitiveMesh_Model meshModelModel)
    {
        Mesh mesh = null;
        switch (meshModelModel.type)
        {
            case AssetPromise_PrimitiveMesh_Model.PrimitiveType.Box:
                {
                    if (meshModelModel.properties is PropertyUvs modelWithUvs)
                    {
                        mesh = PrimitiveMeshBuilder.BuildCube(1f);
                        if (modelWithUvs.uvs != null && modelWithUvs.uvs.Count > 0)
                        {
                            mesh.uv = Utils.FloatArrayToV2List(modelWithUvs.uvs);
                        }
                    }
                }
                break;
            case AssetPromise_PrimitiveMesh_Model.PrimitiveType.Sphere:
            {
                if (meshModelModel.properties is PropertySphere sphereProps)
                {
                    mesh = PrimitiveMeshBuilder.BuildSphere(sphereProps.Radius, sphereProps.Longitude, sphereProps.Latitude);
                }
                else
                {
                    mesh = PrimitiveMeshBuilder.BuildSphere(1f);
                }
                break;
            }
            case AssetPromise_PrimitiveMesh_Model.PrimitiveType.Plane:
                {
                    if (meshModelModel.properties is PropertyUvs modelWithUvs)
                    {
                        mesh = PrimitiveMeshBuilder.BuildPlaneV2(1f);
                        if (modelWithUvs.uvs != null && modelWithUvs.uvs.Count > 0)
                        {
                            mesh.uv = Utils.FloatArrayToV2List(modelWithUvs.uvs);
                        }
                    }
                }
                break;
            case AssetPromise_PrimitiveMesh_Model.PrimitiveType.Cylinder:
                {
                    if (meshModelModel.properties is PropertyCylinder cylinder)
                    {
                        mesh = PrimitiveMeshBuilder.BuildCylinder(50, cylinder.radiusTop, cylinder.radiusBottom,
                            2f, 0f, true, false);
                    }
                }
                break;
        }
        return mesh;
    }
}
