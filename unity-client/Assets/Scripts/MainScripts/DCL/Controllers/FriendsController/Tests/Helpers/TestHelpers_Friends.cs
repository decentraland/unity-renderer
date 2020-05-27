public static class TestHelpers_Friends
{
    public static FriendEntry FakeAddFriend(FriendsController_Mock controller, FriendsHUDView hudView, string id, FriendshipAction action = FriendshipAction.APPROVED)
    {
        UserProfileModel model = new UserProfileModel()
        {
            userId = id,
            name = id,
        };

        UserProfileController.i.AddUserProfileToCatalog(model);
        controller.RaiseUpdateFriendship(id, action);
        return hudView.friendsList.GetEntry(id) as FriendEntry;
    }
}
