using System.Collections;
using UnityEngine;

public static class TestHelpers_Friends
{
    public static IEnumerator FakeAddFriend(FriendsController_Mock controller, FriendsHUDView hudView, string id, FriendshipAction action = FriendshipAction.APPROVED)
    {
        UserProfileModel model = new UserProfileModel()
        {
            userId = id,
            name = id,
        };

        UserProfileController.i.AddUserProfileToCatalog(model);
        controller.RaiseUpdateFriendship(id, action);
        yield return new WaitUntil(() => hudView.friendsList.creationQueue.Count == 0);
    }

    public static FriendEntry GetEntry(FriendsHUDView hudView, string id)
    {
        return hudView.friendsList.GetEntry(id) as FriendEntry;
    }
}
