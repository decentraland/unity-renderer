﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public class CombineMeshData
    {
        // Mesh stuff
        public List<CombineInstance> combineInstances = new List<CombineInstance>();
        public Matrix4x4[] bindPoses;
        public List<SubMeshDescriptor> subMeshes = new List<SubMeshDescriptor>();

        // Combined vertex attributes
        // TODO(Brian): use interleaved buffers with SetVertexBufferData
        public List<BoneWeight> boneWeights = new List<BoneWeight>();
        public List<Vector3> texturePointers = new List<Vector3>();
        public List<Color> colors = new List<Color>();
        public List<Vector4> emissionColors = new List<Vector4>();

        // Renderers
        public List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
        public List<Material> materials = new List<Material>();
    }
}