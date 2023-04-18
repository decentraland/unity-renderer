using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarSlotsDefinition", menuName = "Variables/AvatarSlotsDefinition")]
public class AvatarSlotsDefinitionSO : ScriptableObject
{
    [SerializeField] public SerializableKeyValuePair<string, List<string>>[] slotsDefinition;
}
