using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MainScripts.DCL.Components.Avatar.VRMExporter
{
    [System.Serializable]
    public struct BonesMapEntry
    {
        public string nameDCL;
        public string nameVRM;
    }

    [CreateAssetMenu(menuName = "VRM/Create BonesMappingSO", fileName = "VRMBonesMapping", order = 0)]
    public class VRMBonesMappingSO : ScriptableObject
    {
        [SerializeField] private List<BonesMapEntry> bonesMapping = new List<BonesMapEntry>();
        private Dictionary<string, string> dclToFbx;

        public bool TryGetFBXBone(string dclBoneName, out string fbxBoneName)
        {
            EnsureDictionary();
            return dclToFbx.TryGetValue(dclBoneName, out fbxBoneName);
        }

        private void EnsureDictionary() => dclToFbx ??= bonesMapping.ToDictionary(x => x.nameDCL, x => x.nameVRM);
    }
}
