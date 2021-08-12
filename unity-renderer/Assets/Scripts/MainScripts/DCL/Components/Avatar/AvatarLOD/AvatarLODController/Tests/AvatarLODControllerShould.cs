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
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        [SetUp]
        public void SetUp()
        {
            renderer = Substitute.For<IAvatarRenderer>();
            renderer.isReady.Returns(true);
            player = new Player { id = "player", renderer = renderer };
            controller = new DCL.AvatarLODController(player);
        }

        [Test]
        public void BeCreatedProperly()
        {
            Assert.AreEqual(player, controller.player);
            Assert.AreEqual(1, controller.avatarFade);
            Assert.AreEqual(1, controller.targetAvatarFade);
            Assert.AreEqual(0, controller.impostorFade);
            Assert.AreEqual(0, controller.targetImpostorFade);
            Assert.IsTrue(controller.SSAOEnabled);
            Assert.IsTrue(controller.facialFeaturesEnabled);
            player.renderer.Received().SetAvatarFade(1);
            player.renderer.Received().SetImpostorFade(0);
        }

        [Test]
        public void SetAvatarStateProperly()
        {
            controller.targetAvatarFade = 0;
            controller.impostorFade = 1;
            controller.SSAOEnabled = false;
            controller.facialFeaturesEnabled = false;
            controller.SetAvatarState();

            renderer.Received().SetSSAOEnabled(true);
            renderer.Received().SetFacialFeaturesVisible(true);
            Assert.AreEqual(1, controller.targetAvatarFade);
            Assert.AreEqual(0, controller.targetImpostorFade);
            Assert.NotNull(controller.currentTransition);
        }

        [Test]
        public void SetAvatarStateProperly_NullRenderer()
        {
            player.renderer = null;
            Assert.DoesNotThrow(() => controller.SetAvatarState());
        }

        [Test]
        public void SetAvatarStateProperly_IgnoresIfTargetsAreSet()
        {
            controller.targetAvatarFade = 1;
            controller.avatarFade = 1;
            controller.targetImpostorFade = 0;
            controller.impostorFade = 0;
            controller.currentTransition = null;
            controller.SetAvatarState();

            Assert.IsNull(controller.currentTransition);
        }

        [Test]
        public void SetSimpleAvatarStateProperly()
        {
            controller.targetAvatarFade = 0;
            controller.impostorFade = 1;
            controller.SSAOEnabled = true;
            controller.facialFeaturesEnabled = true;
            controller.SetSimpleAvatar();

            renderer.Received().SetSSAOEnabled(false);
            renderer.Received().SetFacialFeaturesVisible(false);
            Assert.AreEqual(1, controller.targetAvatarFade);
            Assert.AreEqual(0, controller.targetImpostorFade);
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
            controller.targetAvatarFade = 1;
            controller.avatarFade = 1;
            controller.targetImpostorFade = 0;
            controller.impostorFade = 0;
            controller.currentTransition = null;
            controller.SetSimpleAvatar();

            Assert.Null(controller.currentTransition);
        }

        [Test]
        public void SetImpostorStateStateProperly()
        {
            controller.targetAvatarFade = 1;
            controller.impostorFade = 0;
            controller.SSAOEnabled = true;
            controller.facialFeaturesEnabled = true;
            controller.SetImpostorState();

            renderer.Received().SetSSAOEnabled(false);
            renderer.Received().SetFacialFeaturesVisible(false);
            Assert.AreEqual(0, controller.targetAvatarFade);
            Assert.AreEqual(1, controller.targetImpostorFade);
            Assert.NotNull(controller.currentTransition);
        }

        [Test]
        public void SetImpostorStateStateProperly_NullRenderer()
        {
            player.renderer = null;
            Assert.DoesNotThrow(() => controller.SetImpostorState());
        }

        [Test]
        public void SetImpostorStateStateProperly_IgnoresIfTargetsAreSet()
        {
            controller.targetAvatarFade = 0;
            controller.avatarFade = 0;
            controller.targetImpostorFade = 1;
            controller.impostorFade = 1;
            controller.currentTransition = null;
            controller.SetImpostorState();

            Assert.Null(controller.currentTransition);
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