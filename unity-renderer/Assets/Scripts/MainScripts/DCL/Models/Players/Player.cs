using AvatarSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Player
{
    public string id;
    public string name;

    public Vector3 worldPosition
    {
        get => worldPositionProp.Value;
        set => worldPositionProp.Value = value;
    }

    public Vector3 forwardDirection;
    public bool isTalking;
    public IAvatar avatar;
    public IAvatarOnPointerDownCollider onPointerDownCollider;
    public IPlayerName playerName;
    public IAvatarAnchorPoints anchorPoints;
    public Collider collider;

    private readonly AsyncReactiveProperty<Vector3> worldPositionProp = new (Vector3.zero);

    public IReadOnlyAsyncReactiveProperty<Vector3> WorldPositionProp => worldPositionProp;
}
