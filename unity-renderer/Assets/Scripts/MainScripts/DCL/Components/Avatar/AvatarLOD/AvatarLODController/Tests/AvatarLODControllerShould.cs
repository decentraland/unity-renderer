using System;
using System.Collections;
using System.Collections.Generic;
using AvatarSystem;
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
        private IAvatar renderer;
        private IAvatarOnPointerDownCollider onPointerDownCollider;
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        [SetUp]
        public void SetUp()
        {
            renderer = Substitute.For<IAvatar>();
            onPointerDownCollider = Substitute.For<IAvatarOnPointerDownCollider>();
            player = new Player { id = "player", avatar = renderer, onPointerDownCollider = onPointerDownCollider };
            controller = new DCL.AvatarLODController(player);
        }

        [Test]
        public void BeCreatedProperly()
        {
            Assert.AreEqual(player, controller.player);
            player.avatar.Received().SetLODLevel(0);
        }

        [Test]
        public void SetAvatarStateProperly()
        {
            controller.SetLOD0();

            player.avatar.Received().SetLODLevel(0);
        }

        [Test]
        public void SetFullAvatarProperly_NullRenderer()
        {
            player.avatar = null;
            Assert.DoesNotThrow(() => controller.SetLOD0());
        }

        [Test]
        public void SetSimpleAvatarStateProperly()
        {
            controller.SetLOD1();

            player.avatar.Received().SetLODLevel(1);
        }

        [Test]
        public void SetSimpleAvatarStateProperly_NullRenderer()
        {
            player.avatar = null;
            Assert.DoesNotThrow(() => controller.SetLOD1());
        }

        [Test]
        public void SetImpostorStateStateProperly()
        {
            controller.SetLOD2();

            player.avatar.Received().SetLODLevel(2);
        }

        [Test]
        public void AffectAvatarColliderBasedOnVisibility()
        {
            controller.SetInvisible();
            controller.player.onPointerDownCollider.Received().SetColliderEnabled(false);

            controller.SetLOD0();
            controller.player.onPointerDownCollider.Received().SetColliderEnabled(true);
        }

        [Test]
        public void SetImpostorStateStateProperly_NullRenderer()
        {
            player.avatar = null;
            Assert.DoesNotThrow(() => controller.SetLOD2());
        }
    }
}