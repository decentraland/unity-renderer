using UnityEngine;

[System.Serializable]
public abstract class TriggerArea
{
    public abstract Collider AddCollider(GameObject avatarModifierArea);

}

[System.Serializable]
public class BoxTriggerArea : TriggerArea
{
    public Vector3 box;

    public override Collider AddCollider(GameObject avatarModifierArea)
    {
        BoxCollider boxCollider = avatarModifierArea.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.size = box;
        return boxCollider;
    }
}
