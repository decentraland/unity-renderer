using System.Collections;
using System.Collections.Generic;
using DCL.Configuration;
using UnityEngine;

public class MissingAsset : MonoBehaviour
{
    public BoxCollider boxCollider;

    public void Configure(BIWEntity entity)
    {
        boxCollider.gameObject.name = entity.rootEntity.entityId.ToString();
        boxCollider.gameObject.layer = BIWSettings.COLLIDER_SELECTION_LAYER_INDEX;
    }
}