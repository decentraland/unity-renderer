using Cysharp.Threading.Tasks;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using VRM;
using VRMShaders;
using Object = UnityEngine.Object;

namespace MainScripts.DCL.Components.Avatar.VRMExporter
{
    public interface IVRMExporter
    {
        UniTask<byte[]> Export(string name, string reference, IEnumerable<SkinnedMeshRenderer> wearables, CancellationToken ct = default);
    }

    public class VRMExporter : IVRMExporter
    {
        internal class ExportingState : IDisposable
        {
            public readonly List<Object> objects = new ();

            public void Dispose()
            {
                foreach (Object obj in objects) { Object.Destroy(obj); }

                objects.Clear();
            }
        }

        private readonly VRMExporterReferences vrmExporterReferences;
        private readonly IReadOnlyDictionary<string, Transform> toExportBones;
        private readonly ExportingState exportingState = new ();
        private readonly UniGLTF.GltfExportSettings settings = new ();
        private readonly RuntimeTextureSerializer textureSerializer = new ();

        public VRMExporter(VRMExporterReferences vrmExporterReferences)
        {
            this.vrmExporterReferences = vrmExporterReferences;
            toExportBones = VRMExporterUtils.CacheFBXBones(vrmExporterReferences.bonesRoot);
            vrmExporterReferences.metaObject.Version = "1.0, UniVRM v0.112.0";
        }

        public UniTask<byte[]> Export(string name, string reference, IEnumerable<SkinnedMeshRenderer> wearables, CancellationToken ct = default)
        {
            exportingState.Dispose();

            using var _ = exportingState;

            // Adding new wearables
            foreach (SkinnedMeshRenderer wearable in wearables)
                AddWearableToExport(wearable);

            vrmExporterReferences.metaObject.Author = name;
            vrmExporterReferences.metaObject.Reference = reference;

            // Normalize Bones
            GameObject toExportNormalized = VRMBoneNormalizer.Execute(vrmExporterReferences.toExport, true);
            exportingState.objects.Add(toExportNormalized);
            var vrmNormalized = VRM.VRMExporter.Export(settings, toExportNormalized, textureSerializer);

            return new UniTask<byte[]>(vrmNormalized.ToGlbBytes());
        }

        private void AddWearableToExport(SkinnedMeshRenderer wearable)
        {
            if (wearable == null)
                return;

            bool TryGetFbxBone(Transform originalBone, out Transform fbxBone)
            {
                fbxBone = null;

                if (!vrmExporterReferences.vrmbonesMapping.TryGetFBXBone(originalBone.name, out string fbxName))
                {
                    Debug.Log($"Couldnt find a mapping for bone {originalBone.name}");
                    return false;
                }

                if (!toExportBones.TryGetValue(fbxName, out Transform fbxTransform))
                {
                    Debug.Log($"Couldnt find a bone with name {fbxName}");
                    return false;
                }

                fbxBone = fbxTransform;
                return true;
            }

            var holder = new GameObject(wearable.name);
            holder.transform.parent = vrmExporterReferences.meshesContainer;
            holder.transform.ResetLocalTRS();
            exportingState.objects.Add(holder);

            var copiedWearable = VRMExporterUtils.CloneSmr(holder, wearable);
            copiedWearable.materials = ConvertMaterials(wearable.materials, vrmExporterReferences.vrmToonMaterial, vrmExporterReferences.vrmUnlitMaterial);

            Transform[] originalBones = wearable.bones;
            Transform[] newBones = new Transform[originalBones.Length];

            for (var i = 0; i < originalBones.Length; i++)
            {
                Transform originalBone = originalBones[i];

                if (TryGetFbxBone(originalBone, out var fbxBone))
                {
                    if (fbxBone == null)
                    {
                        Debug.LogError($"Couldnt find a bone with name {originalBone.name} for mesh {wearable.sharedMesh.name} in object {wearable.transform.GetHierarchyPath()}");
                        continue;
                    }
                    newBones[i] = fbxBone;}
            }

            copiedWearable.bones = newBones;

            if (TryGetFbxBone(wearable.rootBone, out var rootBone))
                copiedWearable.rootBone = rootBone;
        }

        private Material[] ConvertMaterials(Material[] materials, Material toonMaterial, Material unlitMaterial)
        {
            Material[] outputMaterial = new Material[materials.Length];

            for (var i = 0; i < materials.Length; i++)
            {
                var material = materials[i];

                switch (material.shader.name)
                {
                    case "DCL/Eyes Shader":
                    {
                        outputMaterial[i] = new Material(unlitMaterial);
                        exportingState.objects.Add(outputMaterial[i]);
                        var texture = VRMExporterUtils.ExtractComposedEyesTexture(material);
                        exportingState.objects.Add(texture);
                        VRMExporterUtils.ConvertEyesMaterial(material, texture, outputMaterial[i]);
                        break;
                    }
                    case "DCL/Unlit Cutout Tinted":
                    {
                        outputMaterial[i] = new Material(unlitMaterial);
                        exportingState.objects.Add(outputMaterial[i]);
                        var texture = VRMExporterUtils.ExtractComposedBaseMapTexture(material);
                        exportingState.objects.Add(texture);
                        VRMExporterUtils.ConvertEyesMaterial(material, texture, outputMaterial[i]);
                        break;
                    }
                    case "DCL/Universal Render Pipeline/Lit":
                        outputMaterial[i] = new Material(toonMaterial);
                        exportingState.objects.Add(outputMaterial[i]);
                        VRMExporterUtils.ConvertLitMaterial(material, outputMaterial[i]);
                        break;
                    default:
                        throw new Exception($"Material {material.name} with shader {material.shader.name} is not supported");
                }
            }

            return outputMaterial;
        }
    }
}
