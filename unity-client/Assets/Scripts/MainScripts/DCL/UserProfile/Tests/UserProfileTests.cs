using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class UserProfileTests : TestsBase
    {
        [Test]
        public void UserProfile_Creation()
        {
            var userProfile = ScriptableObject.CreateInstance<UserProfile>();

            Assert.NotNull(userProfile);
        }

        [Test]
        public void UserProfile_OwnProfile_Retrieval()
        {
            var ownUserProfile = UserProfile.GetOwnUserProfile();

            Assert.NotNull(ownUserProfile);
            Assert.AreEqual(UserProfile.ownUserProfile, ownUserProfile);
            Assert.True(AssetDatabase.GetAssetPath(ownUserProfile).Contains("ScriptableObjects/OwnUserProfile"));
        }

        [Test]
        public void UserProfile_Model_UpdateProperties()
        {
            var userProfile = ScriptableObject.CreateInstance<UserProfile>();

            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel(),
                snapshots = CreateSnapshotsWithFaceUrl("avatarPicURL")
            }, false);

            Assert.AreEqual("name", userProfile.model.name);
            Assert.AreEqual("mail", userProfile.model.email);
            Assert.AreEqual("avatarPicURL", userProfile.model.snapshots.face256);
        }

        private static UserProfileModel.Snapshots CreateSnapshotsWithFaceUrl(string faceUrl)
        {
            return new UserProfileModel.Snapshots()
            {
                face = faceUrl,
                face128 = faceUrl,
                face256 = faceUrl
            };
        }

        [Test]
        public void UserProfile_UpdateData_FromEmpty()
        {
            var userProfile = ScriptableObject.CreateInstance<UserProfile>();

            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name2",
                email = "mail2",
                avatar = new AvatarModel(),
                snapshots = CreateSnapshotsWithFaceUrl("avatarPicURL2")
            }, false);

            Assert.AreEqual("name2", userProfile.model.name);
            Assert.AreEqual("mail2", userProfile.model.email);
            Assert.AreEqual("avatarPicURL2", userProfile.model.snapshots.face256);
        }

        [Test]
        public void UserProfile_UpdateData_FromFilled()
        {
            var userProfile = ScriptableObject.CreateInstance<UserProfile>();
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel(),
                snapshots = CreateSnapshotsWithFaceUrl("avatarPicURL")
            }, false);

            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name2",
                email = "mail2",
                avatar = new AvatarModel(),
                snapshots = CreateSnapshotsWithFaceUrl("avatarPicURL2")
            }, false);

            Assert.AreEqual("name2", userProfile.model.name);
            Assert.AreEqual("mail2", userProfile.model.email);
            Assert.AreEqual("avatarPicURL2", userProfile.model.snapshots.face256);
        }

        [Test]
        [TestCase(1)]
        [TestCase(5)]
        public void UserProfile_GetItemAmount(int amount)
        {
            var userProfile = ScriptableObject.CreateInstance<UserProfile>();
            var model = new UserProfileModel();
            model.inventory = new string[amount];
            for (int i = 0; i < amount; i++)
            {
                model.inventory[i] = "nft";
            }
            userProfile.UpdateData(model);

            Assert.AreEqual(amount, userProfile.GetItemAmount("nft"));
        }
    }
}
