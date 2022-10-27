
using DCL.ECSComponents;
using DCL.Helpers;
using NUnit.Framework;

namespace Tests
{
    public class TextureShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;

        [SetUp]
        public void SetUp()
        {
            testUtils = new ECS7TestUtilsScenesAndEntities();
            scene = testUtils.CreateScene("temptation");
        }
        
        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void ReturnCorrectTextureUrl()
        {
            string testTexPath = TestAssetsUtils.GetPath() + "/Images/avatar.png";
            TextureUnion texture = new TextureUnion()
            {
                Texture = new DCL.ECSComponents.Texture()
                {
                    Src = testTexPath
                }
            };
            
            Assert.IsTrue(testTexPath.ToLower() == texture.GetTextureUrl(scene));
        }
        
        [Test]
        public void ReturnCorrectAvatarTextureUrl()
        {
            string avatarTextureAPIBaseUrl = "avatarTextureAPI.test.url/";
            string userId = "156321";
            
            KernelConfigModel kernelConfigModel = new KernelConfigModel()
            {
                avatarTextureAPIBaseUrl = avatarTextureAPIBaseUrl
            };
            KernelConfig.i.Set(kernelConfigModel);
            
            TextureUnion avatarTexture = new TextureUnion()
            {
                AvatarTexture = new AvatarTexture()
                {
                    UserId = userId
                }
            };
         
            Assert.IsTrue((avatarTextureAPIBaseUrl + userId) == avatarTexture.GetTextureUrl(scene));
        }
        
        [Test]
        public void ReturnCorrectWrapMode()
        {
            TextureUnion texture = new TextureUnion()
            {
                Texture = new DCL.ECSComponents.Texture()
                {
                    WrapMode = TextureWrapMode.TwmRepeat
                }
            };
            
            Assert.IsTrue(UnityEngine.TextureWrapMode.Repeat == texture.GetWrapMode());
            
            TextureUnion avatarTexture = new TextureUnion()
            {
                AvatarTexture = new AvatarTexture()
                {
                    WrapMode = TextureWrapMode.TwmMirrorOnce
                }
            };
            
            Assert.IsTrue(UnityEngine.TextureWrapMode.MirrorOnce == avatarTexture.GetWrapMode());
        }
        
        [Test]
        public void ReturnCorrectFilterMode()
        {
            TextureUnion texture = new TextureUnion()
            {
                Texture = new DCL.ECSComponents.Texture()
                {
                    FilterMode = TextureFilterMode.TfmTrilinear
                }
            };
            
            Assert.IsTrue(UnityEngine.FilterMode.Trilinear == texture.GetFilterMode());
            
            TextureUnion avatarTexture = new TextureUnion()
            {
                AvatarTexture = new AvatarTexture()
                {
                    FilterMode = TextureFilterMode.TfmPoint
                }
            };
            
            Assert.IsTrue(UnityEngine.FilterMode.Point == avatarTexture.GetFilterMode());
        }
    }
}