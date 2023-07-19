using UnityEngine;

namespace MainScripts.DCL.Components.Avatar.VRMExporter
{
    public class VRMExporterReferences : MonoBehaviour
    {
        public GameObject toExport;
        public Transform bonesRoot;
        public Transform meshesContainer;
        public Material vrmToonMaterial;
        public Material vrmUnlitMaterial;
        public VRMBonesMappingSO vrmbonesMapping;
    }
}
