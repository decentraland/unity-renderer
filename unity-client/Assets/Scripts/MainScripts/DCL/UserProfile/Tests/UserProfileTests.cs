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

            userProfile.UpdateProperties(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel(),
                snapshots = CreateSnapshotsWithFaceUrl("avatarPicURL")
            });

            Assert.AreEqual("name", userProfile.model.name);
            Assert.AreEqual("mail", userProfile.model.email);
            Assert.AreEqual("avatarPicURL", userProfile.model.snapshots.face);
        }

        private static UserProfileModel.Snapshots CreateSnapshotsWithFaceUrl(string faceUrl)
        {
            return new UserProfileModel.Snapshots()
            {
                face = faceUrl
            };
        }

        [Test]
        public void UserProfile_UpdateData_FromEmpty()
        {
            var userProfile = ScriptableObject.CreateInstance<UserProfile>();

            userProfile.UpdateProperties(new UserProfileModel()
            {
                name = "name2",
                email = "mail2",
                avatar = new AvatarModel(),
                snapshots = CreateSnapshotsWithFaceUrl("avatarPicURL2")
            });

            Assert.AreEqual("name2", userProfile.model.name);
            Assert.AreEqual("mail2", userProfile.model.email);
            Assert.AreEqual("avatarPicURL2", userProfile.model.snapshots.face);
        }

        [Test]
        public void UserProfile_UpdateData_FromFilled()
        {
            var userProfile = ScriptableObject.CreateInstance<UserProfile>();
            userProfile.UpdateProperties(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel(),
                snapshots = CreateSnapshotsWithFaceUrl("avatarPicURL")
            });

            userProfile.UpdateProperties(new UserProfileModel()
            {
                name = "name2",
                email = "mail2",
                avatar = new AvatarModel(),
                snapshots = CreateSnapshotsWithFaceUrl("avatarPicURL2")
            });

            Assert.AreEqual("name2", userProfile.model.name);
            Assert.AreEqual("mail2", userProfile.model.email);
            Assert.AreEqual("avatarPicURL2", userProfile.model.snapshots.face);
        }
    }
}
