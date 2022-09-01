using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using GPUSkinning;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Avatar = AvatarSystem.Avatar;
using Object = UnityEngine.Object;
namespace Test.AvatarSystem
{
    public class BaseAvatarShould
    {

        private BaseAvatar baseAvatar;
        private IBaseAvatarRevealer baseAvatarRevealer;
        private ILOD lod;

        private GameObject container;
        private GameObject armatureContainer;

        [SetUp]
        public void SetUp()
        {
            lod = Substitute.For<ILOD>();
            baseAvatarRevealer = Substitute.For<IBaseAvatarRevealer>();
            container = new GameObject();
            armatureContainer = new GameObject();
            baseAvatar = new BaseAvatar(container.transform, armatureContainer, lod);
            baseAvatar.avatarRevealer = baseAvatarRevealer;
        }

        [Test]
        public void ReturnArmatureContainer()
        {
            Assert.AreEqual(baseAvatar.GetArmatureContainer(), armatureContainer);
        }

        [UnityTest]
        public IEnumerator StartRevealerFadeout() => UniTask.ToCoroutine(async () =>
        {
            MeshRenderer testMesh = new MeshRenderer();
            CancellationToken cancellationToken = CancellationToken.None;
            
            await baseAvatar.FadeOut(testMesh, false, cancellationToken);
            
            baseAvatarRevealer.Received().AddTarget(testMesh);
            baseAvatarRevealer.Received().StartAvatarRevealAnimation(false, Arg.Any<CancellationToken>());
        });

    }
}
