using System;
using System.Collections;
using System.Collections.Generic;
using DCL.ECSComponents;
using DCL.Helpers;
using UnityEngine;

public interface IPrimitiveMeshFactory
{
    Mesh CreateMesh(PrimitiveMeshModel meshModelModel);
}

public class PrimitiveMeshFactory : IPrimitiveMeshFactory
{
    public Mesh CreateMesh(PrimitiveMeshModel meshModelModel)
    {
        Mesh mesh = null;
        switch (meshModelModel.type)
        {
            case PrimitiveMeshModel.Type.Box:
                mesh = PrimitiveMeshBuilder.BuildCube(1f);
                var boxModel = (PBBoxShape) meshModelModel.primitiveModel;
                if (boxModel.Uvs != null && boxModel.Uvs.Count > 0)
                {
                    mesh.uv = Utils.FloatArrayToV2List(boxModel.Uvs);
                }
                break;
            case PrimitiveMeshModel.Type.Sphere:
                mesh = PrimitiveMeshBuilder.BuildSphere(1f);
                break;
            case PrimitiveMeshModel.Type.Plane:
                mesh = PrimitiveMeshBuilder.BuildPlane(1f);
                var model = (ECSPlaneShape) meshModelModel.primitiveModel;
                if (model.uvs != null && model.uvs.Count > 0)
                {
                    mesh.uv = Utils.FloatArrayToV2List(model.uvs);
                }
                break;
            case PrimitiveMeshModel.Type.Cylinder:
                var cylinderModel = (PBCylinderShape) meshModelModel.primitiveModel;
                mesh = PrimitiveMeshBuilder.BuildCylinder(50, cylinderModel.RadiusTop, cylinderModel.RadiusBottom, 2f, 0f, true, false);
                break;
        }
        return mesh;
    }
}
