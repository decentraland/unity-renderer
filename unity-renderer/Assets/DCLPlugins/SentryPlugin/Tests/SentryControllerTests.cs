using DCL;
using NSubstitute;
using NUnit.Framework;
using Sentry;
using Sentry.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCLPlugins.SentryPlugin.Tests
{
    [Category("ToFix")]
    public class SentryControllerTests
    {
        private DataStore_Player playerDataStore;
        private DataStore_Realm realmDataStore;
        private Scope capturedScope;

        [SetUp]
        public void SetUp()
        {
            // Arrange
            var sentryHubMock = Substitute.For<IHub>();
            capturedScope = new Scope(new SentryUnityOptions());
            sentryHubMock.ConfigureScope(Arg.Do<Action<Scope>>(x => { x(capturedScope); }));

            this.playerDataStore = new DataStore_Player();
            this.realmDataStore = new DataStore_Realm();
            new SentryController(playerDataStore, realmDataStore, sentryHubMock);
        }

        [Test]
        public void Player_Update_Updates_Sentry_Tags()
        {
            // Act
            playerDataStore.playerGridPosition.Set(new Vector2Int(10, 10), true);

            // Assert
            Assert.AreEqual("10,10", capturedScope.Tags.GetValueOrDefault("Current Position"));
            Assert.AreEqual("0,0", capturedScope.Tags.GetValueOrDefault("Previous Position"));
        }

        [Test]
        public void Teleporting_Updates_Sentry_Contexts()
        {
            // Act
            playerDataStore.lastTeleportPosition.Set(new Vector3(25, 25), true);

            // Assert
            Assert.AreEqual("25,25", capturedScope.Contexts.GetValueOrDefault("Current Teleport Position"));
            Assert.AreEqual("0,0", capturedScope.Contexts.GetValueOrDefault("Last Teleport Position"));
        }

        [Test]
        public void Other_Players_Joining_Updates_Sentry_Contexts()
        {
            // Act
            playerDataStore.otherPlayers.Add("Player 1", new Player());
            playerDataStore.otherPlayers.Add("Player 2", new Player());
            playerDataStore.otherPlayers.Add("Player 3", new Player());

            // Assert
            Assert.AreEqual("3", capturedScope.Contexts.GetValueOrDefault("Total Other Players"));
        }

        [Test]
        public void Other_Players_Leaving_Updates_Sentry_Contexts()
        {
            // Act
            playerDataStore.otherPlayers.Add("Player 1", new Player());
            playerDataStore.otherPlayers.Add("Player 2", new Player());
            playerDataStore.otherPlayers.Remove("Player 2");

            // Assert
            Assert.AreEqual("1", capturedScope.Contexts.GetValueOrDefault("Total Other Players"));
        }

        [Test]
        public void Realm_Name_Changing_Updates_Sentry_Contexts()
        {
            // Act
            realmDataStore.realmName.Set("Old Realm Name");
            realmDataStore.realmName.Set("New Realm Name");

            // Assert
            Assert.AreEqual("New Realm Name", capturedScope.Tags.GetValueOrDefault("Current Realm"));
            Assert.AreEqual("Old Realm Name", capturedScope.Tags.GetValueOrDefault("Previous Realm"));
        }
    }
}
