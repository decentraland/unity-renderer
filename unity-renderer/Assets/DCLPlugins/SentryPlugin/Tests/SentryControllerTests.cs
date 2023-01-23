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
    public class SentryControllerTests
    {
        [Test]
        public void Player_Update_Updates_Sentry_Tags()
        {
            // Arrange
            var sentryHubMock = Substitute.For<IHub>();
            Scope capturedScope = new Scope(new SentryUnityOptions());
            sentryHubMock.ConfigureScope(Arg.Do<Action<Scope>>(x => { x(capturedScope); }));

            DataStore_Player playerDataStore = new DataStore_Player();
            DataStore_Realm realmDataStore = new DataStore_Realm();
            new SentryController(playerDataStore, realmDataStore, sentryHubMock);

            // Act
            playerDataStore.playerGridPosition.Set(new Vector2Int(10, 10), true);

            // Assert
            Assert.AreEqual("10,10", capturedScope.Tags.GetValueOrDefault("Current Position"));
            Assert.AreEqual("0,0",capturedScope.Tags.GetValueOrDefault("Previous Position"));
        }

        [Test]
        public void Teleporting_Updates_Sentry_Extra()
        {
            // Arrange
            var sentryHubMock = Substitute.For<IHub>();
            Scope capturedScope = new Scope(new SentryUnityOptions());
            sentryHubMock.ConfigureScope(Arg.Do<Action<Scope>>(x => { x(capturedScope); }));

            DataStore_Player playerDataStore = new DataStore_Player();
            DataStore_Realm realmDataStore = new DataStore_Realm();
            new SentryController(playerDataStore, realmDataStore, sentryHubMock);

            // Act
            playerDataStore.lastTeleportPosition.Set(new Vector3(25,25), true);

            // Assert
            Assert.AreEqual("25,25", capturedScope.Extra.GetValueOrDefault("Current Teleport Position"));
            Assert.AreEqual("0,0", capturedScope.Extra.GetValueOrDefault("Last Teleport Position"));
        }

        [Test]
        public void Other_Players_Joining_Updates_Sentry_Extra()
        {
            // Arrange
            var sentryHubMock = Substitute.For<IHub>();
            Scope capturedScope = new Scope(new SentryUnityOptions());
            sentryHubMock.ConfigureScope(Arg.Do<Action<Scope>>(x => { x(capturedScope); }));

            DataStore_Player playerDataStore = new DataStore_Player();
            DataStore_Realm realmDataStore = new DataStore_Realm();
            new SentryController(playerDataStore, realmDataStore, sentryHubMock);

            // Act
            playerDataStore.otherPlayers.Add("Player 1", new Player() );
            playerDataStore.otherPlayers.Add("Player 2", new Player() );
            playerDataStore.otherPlayers.Add("Player 3", new Player() );

            // Assert
            Assert.AreEqual("3", capturedScope.Extra.GetValueOrDefault("Total Other Players"));
        }

        [Test]
        public void Other_Players_Leaving_Updates_Sentry_Extra()
        {
            // Arrange
            var sentryHubMock = Substitute.For<IHub>();
            Scope capturedScope = new Scope(new SentryUnityOptions());
            sentryHubMock.ConfigureScope(Arg.Do<Action<Scope>>(x => { x(capturedScope); }));

            DataStore_Player playerDataStore = new DataStore_Player();
            DataStore_Realm realmDataStore = new DataStore_Realm();
            new SentryController(playerDataStore, realmDataStore, sentryHubMock);

            // Act
            playerDataStore.otherPlayers.Add("Player 1", new Player() );
            playerDataStore.otherPlayers.Add("Player 2", new Player() );
            playerDataStore.otherPlayers.Remove("Player 2" );

            // Assert
            Assert.AreEqual("1", capturedScope.Extra.GetValueOrDefault("Total Other Players"));
        }

        [Test]
        public void Realm_Name_Changing_Updates_Sentry_Extra()
        {
            // Arrange
            var sentryHubMock = Substitute.For<IHub>();
            Scope capturedScope = new Scope(new SentryUnityOptions());
            sentryHubMock.ConfigureScope(Arg.Do<Action<Scope>>(x => { x(capturedScope); }));

            DataStore_Player playerDataStore = new DataStore_Player();
            DataStore_Realm realmDataStore = new DataStore_Realm();
            new SentryController(playerDataStore, realmDataStore, sentryHubMock);

            // Act
            realmDataStore.realmName.Set("Old Realm Name");
            realmDataStore.realmName.Set("New Realm Name");

            // Assert
            Assert.AreEqual("New Realm Name", capturedScope.Tags.GetValueOrDefault("Current Realm"));
            Assert.AreEqual("Old Realm Name", capturedScope.Tags.GetValueOrDefault("Previous Realm"));
        }


    }
}
