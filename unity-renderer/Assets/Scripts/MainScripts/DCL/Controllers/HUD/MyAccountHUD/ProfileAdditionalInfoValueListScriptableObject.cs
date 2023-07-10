using UnityEngine;

namespace DCL.MyAccount
{
    [CreateAssetMenu(fileName = "ProfileAdditionalInfoValueList", menuName = "DCL/Profiles/AdditionalInfoValueList")]
    public class ProfileAdditionalInfoValueListScriptableObject : ScriptableObject, IProfileAdditionalInfoValueListProvider
    {
        [SerializeField] private string[] values;

        public string[] Provide() =>
            values;
    }
}
