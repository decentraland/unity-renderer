using UnityEngine;

namespace DCL.Social.Friends
{
    public partial class WebInterfaceFriendsApiBridge
    {
        [ContextMenu("Force initialization")]
        public void ForceInitialization()
        {
            InitializeFriends(JsonUtility.ToJson(new FriendshipInitializationMessage()));
        }
    }
}