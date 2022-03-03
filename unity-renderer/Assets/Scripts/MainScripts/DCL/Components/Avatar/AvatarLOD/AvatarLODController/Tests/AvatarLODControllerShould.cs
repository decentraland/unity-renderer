using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.AvatarLODController
{
    public class AvatarLODControllerShould
    {
        private DCL.AvatarLODController controller;
        private Player player;
        private IAvatarRenderer renderer;
        private IAvatarOnPointerDownCollider onPointerDownCollider;
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        [SetUp]
        public void SetUp()
        {
            renderer = Substitute.For<IAvatarRenderer>();
            renderer.isReady.Returns(true);
            onPointerDownCollider = Substitute.For<IAvatarOnPointerDownCollider>();
            player = new Player { id = "player", renderer = renderer, onPointerDownCollider = onPointerDownCollider };
            controller = new DCL.AvatarLODController(player);
        }

        [Test]
        public void BeCreatedProperly()
        {
            Assert.IsNull(controller.lastRequestedState);
            Assert.AreEqual(player, controller.player);
            Assert.AreEqual(1, controller.avatarFade);
            Assert.AreEqual(0, controller.impostorFade);
            Assert.IsTrue(controller.SSAOEnabled);
            Assert.IsTrue(controller.facialFeaturesEnabled);
            player.renderer.Received().SetAvatarFade(1);
            player.renderer.Received().SetImpostorFade(0);
        }

        [Test]
        public void SetAvatarStateProperly()
        {
            controller.avatarFade = 0;
            controller.impostorFade = 1;
            controller.SSAOEnabled = false;
            controller.facialFeaturesEnabled = false;
            controller.SetFullAvatar();

            renderer.Received().SetSSAOEnabled(true);
            renderer.Received().SetFacialFeaturesVisible(true);
            Assert.NotNull(controller.currentTransition);
            Assert.NotNull(controller.lastRequestedState);
        }

        [Test]
        public void SetFullAvatarProperly_NullRenderer()
        {
            player.renderer = null;
            Assert.DoesNotThrow(() => controller.SetFullAvatar());
        }

        [Test]
        public void SetFullAvatarProperly_IgnoresIfTargetsAreSet()
        {
            controller.avatarFade = 1;
            controller.impostorFade = 0;
            controller.currentTransition = null;
            controller.SetFullAvatar();

            Assert.AreEqual(DCL.AvatarLODController.State.FullAvatar , controller.lastRequestedState);
            Assert.IsNull(controller.currentTransition);
        }

        [Test]
        public void SetSimpleAvatarStateProperly()
        {
            controller.impostorFade = 1;
            controller.SSAOEnabled = true;
            controller.facialFeaturesEnabled = true;
            controller.SetSimpleAvatar();

            renderer.Received().SetSSAOEnabled(false);
            renderer.Received().SetFacialFeaturesVisible(false);
            Assert.AreEqual(DCL.AvatarLODController.State.SimpleAvatar , controller.lastRequestedState);
            Assert.NotNull(controller.currentTransition);
        }

        [Test]
        public void SetSimpleAvatarStateProperly_NullRenderer()
        {
            player.renderer = null;
            Assert.DoesNotThrow(() => controller.SetSimpleAvatar());
        }

        [Test]
        public void SetSimpleAvatarStateProperly_IgnoresIfTargetsAreSet()
        {
            controller.avatarFade = 1;
            controller.impostorFade = 0;
            controller.currentTransition = null;
            controller.SetSimpleAvatar();

            Assert.AreEqual(DCL.AvatarLODController.State.SimpleAvatar , controller.lastRequestedState);
            Assert.Null(controller.currentTransition);
        }

        [Test]
        public void SetImpostorStateStateProperly()
        {
            controller.impostorFade = 0;
            controller.SSAOEnabled = true;
            controller.facialFeaturesEnabled = true;
            controller.SetImpostor();

            renderer.Received().SetSSAOEnabled(false);
            renderer.Received().SetFacialFeaturesVisible(false);
            Assert.NotNull(controller.currentTransition);
            Assert.AreEqual(DCL.AvatarLODController.State.Impostor , controller.lastRequestedState);
        }
        
        [UnityTest]
        public IEnumerator AffectAvatarColliderBasedOnVisibility()
        {
            controller.SetInvisible();

            Assert.NotNull(controller.currentTransition);
            yield return controller.currentTransition;

            Assert.AreEqual(0, controller.avatarFade);
            controller.player.onPointerDownCollider.Received().SetColliderEnabled(false);

            controller.SetFullAvatar();

            Assert.NotNull(controller.currentTransition);
            yield return controller.currentTransition;

            Assert.AreEqual(1, controller.avatarFade);
            controller.player.onPointerDownCollider.Received().SetColliderEnabled(true);
        }

        [Test]
        public void SetImpostorStateStateProperly_NullRenderer()
        {
            player.renderer = null;
            Assert.DoesNotThrow(() => controller.SetImpostor());
        }

        [Test]
        public void SetImpostorStateStateProperly_IgnoresIfTargetsAreSet()
        {
            controller.avatarFade = 0;
            controller.impostorFade = 1;
            controller.currentTransition = null;
            controller.SetImpostor();

            Assert.AreEqual(DCL.AvatarLODController.State.Impostor , controller.lastRequestedState);
            Assert.Null(controller.currentTransition);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(6)]
        public void SetAnimationThrottling(int framesBetweenUpdates)
        {
            controller.SetThrottling(framesBetweenUpdates);
            renderer.Received().SetThrottling(framesBetweenUpdates);
        }

        [UnityTest]
        public IEnumerator TransitionProperly()
        {
            controller.avatarFade = 0;
            controller.impostorFade = 1;
            yield return controller.Transition(1, 0, 0);
            Assert.AreEqual(1, controller.avatarFade);
            Assert.AreEqual(0, controller.impostorFade);
        }
    }
}