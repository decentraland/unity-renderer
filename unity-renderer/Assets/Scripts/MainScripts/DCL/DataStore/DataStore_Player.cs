using UnityEngine;

namespace DCL
{
    public class DataStore_Player
    {
        // Current player position
        public readonly BaseVariable<Vector2Int> playerGridPosition = new BaseVariable<Vector2Int>(Vector2Int.zero);
        public readonly BaseVariable<Vector3> playerUnityPosition = new BaseVariable<Vector3>(Vector3.zero);
        public readonly BaseVariable<Vector3> playerWorldPosition = new BaseVariable<Vector3>(Vector3.zero);
        
        // NOTE: set when character is teleported (DCLCharacterController - Teleport)
        public readonly BaseVariable<Vector3> lastTeleportPosition = new BaseVariable<Vector3>(Vector3.zero);
        public readonly BaseDictionary<string, Player> otherPlayers = new BaseDictionary<string, Player>();
        public readonly BaseVariable<Player> ownPlayer = new BaseVariable<Player>();
        public readonly BaseVariable<bool> canPlayerMove = new BaseVariable<bool>(true);
    }
}