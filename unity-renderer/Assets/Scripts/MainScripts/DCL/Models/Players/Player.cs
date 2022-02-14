using AvatarSystem;
using UnityEngine;

public class Player
{
    public string id;
    public string name;
    public Vector3 worldPosition;
    public Vector3 forwardDirection;
    public bool isTalking;
    public IAvatar avatar;
    public IAvatarOnPointerDownCollider onPointerDownCollider;
    public IPlayerName playerName;
    public IAvatarAnchorPoints anchorPoints;
    public Collider collider;
}