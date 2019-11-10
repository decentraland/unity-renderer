using System.Collections;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UserProfileTests : TestsBase
    {
        [UnityTest]
        public IEnumerator UserProfile_Creation()
        {
            yield return InitScene();

            var userProfile = ScriptableObject.CreateInstance<UserProfile>();

            Assert.NotNull(userProfile);
        }

        [UnityTest]
        public IEnumerator UserProfile_OwnProfile_Retrieval()
        {
            yield return InitScene();

            var ownUserProfile = UserProfile.GetOwnUserProfile();

            Assert.NotNull(ownUserProfile);
            Assert.AreEqual(UserProfile.ownUserProfile, ownUserProfile);
            Assert.True(AssetDatabase.GetAssetPath(ownUserProfile).Contains("ScriptableObjects/OwnUserProfile"));
        }

        [UnityTest]
        public IEnumerator UserProfile_Model_UpdateProperties()
        {
            yield return InitScene();
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

        [UnityTest]
        public IEnumerator UserProfile_UpdateData_FromEmpty()
        {
            yield return InitScene();
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

        [UnityTest]
        public IEnumerator UserProfile_UpdateData_FromFilled()
        {
            yield return InitScene();
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