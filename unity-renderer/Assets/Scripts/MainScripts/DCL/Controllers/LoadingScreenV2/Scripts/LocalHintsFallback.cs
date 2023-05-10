using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    [CreateAssetMenu(fileName = "LocalHintsFallback", menuName = "Variables/Local Hints Fallback")]
    public class LocalHintsFallback : ScriptableObject
    {
        [SerializeField] public SerializableKeyValuePair<SourceTag, List<BaseHint>>[] slotsDefinition;
    }
}
