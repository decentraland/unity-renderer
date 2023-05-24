using UnityEngine;

namespace MainScripts.DCL.Controllers.ShaderPrewarm
{
    [CreateAssetMenu(menuName = "Create ShaderVariantsData", fileName = "ShaderVariantsData", order = 0)]
    public class ShaderVariantsData : ScriptableObject
    {
        public ShaderVariantCollection[] collections;
    }
}
