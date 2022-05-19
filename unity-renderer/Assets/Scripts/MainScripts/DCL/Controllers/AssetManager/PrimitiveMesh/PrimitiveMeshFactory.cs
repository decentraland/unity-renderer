using System;
using System.Collections;
using System.Collections.Generic;
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
                if (meshModelModel.uvs != null && meshModelModel.uvs.Count > 0)
                {
                    mesh.uv = Utils.FloatArrayToV2List(meshModelModel.uvs);
                }
                break;
            case PrimitiveMeshModel.Type.Sphere:
                mesh = PrimitiveMeshBuilder.BuildSphere(1f);
                break;
        }
        return mesh;
    }
}
