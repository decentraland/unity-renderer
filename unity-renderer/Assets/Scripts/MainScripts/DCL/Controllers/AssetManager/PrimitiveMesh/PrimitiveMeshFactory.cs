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
                mesh = PrimitiveMeshBuilder.BuildCube(1f);
                if (meshModelModel.uvs != null && meshModelModel.uvs.Count > 0)
                {
                    mesh.uv = Utils.FloatArrayToV2List(meshModelModel.uvs);
                }
                break;
            case AssetPromise_PrimitiveMesh_Model.PrimitiveType.Sphere:
                mesh = PrimitiveMeshBuilder.BuildSphere(1f);
                break;
            case AssetPromise_PrimitiveMesh_Model.PrimitiveType.Plane:
                mesh = PrimitiveMeshBuilder.BuildPlane(1f);
                if (meshModelModel.uvs != null && meshModelModel.uvs.Count > 0)
                {
                    mesh.uv = Utils.FloatArrayToV2List(meshModelModel.uvs);
                }
                break;
            case AssetPromise_PrimitiveMesh_Model.PrimitiveType.Cylinder:
                mesh = PrimitiveMeshBuilder.BuildCylinder(50, meshModelModel.radiusTop, meshModelModel.radiusBottom, 
                    2f, 0f, true, false);
                break;
        }
        return mesh;
    }
}
