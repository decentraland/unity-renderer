using DCL;
using UnityEngine;

public class Player
{
    public string id;
    public string name;
    public Vector3 worldPosition;
    public Vector3 forwardDirection;
    public bool isTalking;
    public IAvatarRenderer renderer;
    public IAvatarOnPointerDownCollider onPointerDownCollider;
    public IPlayerName playerName;
}