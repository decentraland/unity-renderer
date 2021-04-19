using UnityEngine;

namespace UnityGLTF
{
    class SpecGlossMap : SpecGloss2StandardMap
    {
        public SpecGlossMap(int MaxLOD = 1000) : base("DCL/Universal Render Pipeline/Lit", MaxLOD) { }
        public SpecGlossMap(string shaderName, int MaxLOD = 1000) : base(shaderName, MaxLOD) { }
        public SpecGlossMap(Material m, int MaxLOD = 1000) : base(m, MaxLOD) { }

        public override IUniformMap Clone()
        {
            var copy = new SpecGlossMap(new Material(_material));
            base.Copy(copy);
            return copy;
        }
    }
}