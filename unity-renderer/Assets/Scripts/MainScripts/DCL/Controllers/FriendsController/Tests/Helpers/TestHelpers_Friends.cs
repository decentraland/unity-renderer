using System.Collections;
using UnityEngine;

public static class TestHelpers_Friends
{
    public static IEnumerator FakeAddFriend(UserProfileController userProfileController, FriendsController_Mock controller, IFriendsHUDComponentView view, string id, FriendshipAction action = FriendshipAction.APPROVED)
    {
        UserProfileModel model = new UserProfileModel()
        {
            userId = id,
            name = id,
        };

        userProfileController.AddUserProfileToCatalog(model);
        controller.RaiseUpdateFriendship(id, action);
        yield return new WaitUntil(view.IsFriendListCreationReady);
    }

    public static FriendEntry GetEntry(IFriendsHUDComponentView view, string id) { return view.GetEntry(id) as FriendEntry; }
}