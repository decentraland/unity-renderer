using System.Collections;
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
                userName = "name",
                mail = "mail",
                avatarPicURL = "avatarPicURL"
            });

            Assert.AreEqual("name", userProfile.model.userName);
            Assert.AreEqual("mail", userProfile.model.mail);
            Assert.AreEqual("avatarPicURL", userProfile.model.avatarPicURL);
        }

        [UnityTest]
        public IEnumerator UserProfile_UpdateData_FromEmpty()
        {
            yield return InitScene();
            var userProfile = ScriptableObject.CreateInstance<UserProfile>();

            userProfile.UpdateProperties(new UserProfileModel()
            {
                userName = "name2",
                mail = "mail2",
                avatarPicURL = "avatarPicURL2"
            });

            Assert.AreEqual("name2", userProfile.model.userName);
            Assert.AreEqual("mail2", userProfile.model.mail);
            Assert.AreEqual("avatarPicURL2", userProfile.model.avatarPicURL);
        }

        [UnityTest]
        public IEnumerator UserProfile_UpdateData_FromFilled()
        {
            yield return InitScene();
            var userProfile = ScriptableObject.CreateInstance<UserProfile>();
            userProfile.UpdateProperties(new UserProfileModel() {userName = "name", mail = "mail", avatarPicURL = "avatarPicURL"});

            userProfile.UpdateProperties(new UserProfileModel()
            {
                userName = "name2",
                mail = "mail2",
                avatarPicURL = "avatarPicURL2"
            });

            Assert.AreEqual("name2", userProfile.model.userName);
            Assert.AreEqual("mail2", userProfile.model.mail);
            Assert.AreEqual("avatarPicURL2", userProfile.model.avatarPicURL);
        }
    }
}
