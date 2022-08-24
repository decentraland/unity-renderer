using UnityEngine;

public interface IAvatarModifier
{
    void ApplyModifier(GameObject avatar);
    void RemoveModifier(GameObject avatar);
}