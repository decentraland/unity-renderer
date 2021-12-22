using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using UnityEngine;

public class LazyTextureObserverShould
{
    public class TextureLoader_Mock : ITextureLoader
    {
        public event Action<Texture2D> OnSuccess;
        public event Action<Exception> OnFail;

        private Texture2D texture;

        public void Load(string uri)
        {
            if ( uri == "black" )
                texture = Texture2D.blackTexture;
            else if ( uri == "white")
                texture = Texture2D.whiteTexture;
            else
            {
                OnFail?.Invoke(new Exception("Uri is not white or black"));
                return;
            }

            OnSuccess?.Invoke(texture);
        }

        public void Unload() { texture = null; }

        public Texture2D GetTexture() { return texture; }
    }

    [Test]
    public void CallListenersWhenNewUriIsSet()
    {
        // Arrange
        var textureLoader = new TextureLoader_Mock();

        bool successCalled = false;
        bool otherSuccessCalled = false;

        textureLoader.OnSuccess += (x) => successCalled = true;
        var lazyTextureObserver = new LazyTextureObserver(textureLoader);

        // Act
        lazyTextureObserver.RefreshWithUri("white");

        // Assert
        Assert.That( successCalled, Is.False, "Texture load shouldn't happen yet!. It should only happen with active listeners!" );

        // Act
        lazyTextureObserver.AddListener( (x) => { otherSuccessCalled = true; } );

        // Assert
        Assert.That( otherSuccessCalled, Is.True, "Texture load is not being performed with listeners!" );
        Assert.That( successCalled, Is.True, "Texture load is not being performed with listeners!" );

        // TearDown
        lazyTextureObserver.Dispose();
    }

    [Test]
    public void CallListenersWhenNewTextureIsSet()
    {
        // Arrange
        var textureLoader = new TextureLoader_Mock();
        bool successCalledWithProperTexture = false;

        Texture2D mockTexture = Texture2D.grayTexture;

        var lazyTextureObserver = new LazyTextureObserver(textureLoader);

        // Act
        lazyTextureObserver.RefreshWithTexture(mockTexture);
        lazyTextureObserver.AddListener( (x) =>
        {
            if (x == mockTexture)
                successCalledWithProperTexture = true;
        } );

        // Assert
        Assert.That( successCalledWithProperTexture, Is.True, "Texture load is not being performed with listeners!" );

        // TearDown
        lazyTextureObserver.Dispose();
    }

    [Test]
    public void UnloadOldTexturesWhenNewUrisAreSet()
    {
        // Arrange
        var textureLoader = Substitute.ForPartsOf<TextureLoader_Mock>();
        var lazyTextureObserver = new LazyTextureObserver(textureLoader);

        lazyTextureObserver.AddListener( (x) => { } );
        lazyTextureObserver.RefreshWithUri("white");

        Assert.That( textureLoader.GetTexture() == Texture2D.whiteTexture, Is.True );
        textureLoader.DidNotReceive().Unload();

        // Act
        lazyTextureObserver.RefreshWithUri("black");

        // Assert
        Assert.That( textureLoader.GetTexture() == Texture2D.blackTexture, Is.True );
        textureLoader.Received().Unload();

        // TearDown
        lazyTextureObserver.Dispose();
    }

    [Test]
    public void CallListenersWhenTheyAreAddedAfterTextureIsLoaded()
    {
        // Arrange
        var textureLoader = new TextureLoader_Mock();

        bool onSuccessCalled = false;
        bool callbackCalled = false;

        textureLoader.OnSuccess += (x) => onSuccessCalled = true;
        var lazyTextureObserver = new LazyTextureObserver(textureLoader);

        lazyTextureObserver.RefreshWithUri("white");
        lazyTextureObserver.AddListener( (x) => { } );

        Assert.That( onSuccessCalled, Is.True );

        onSuccessCalled = false;

        // Act
        lazyTextureObserver.AddListener( (x) => { callbackCalled = true; } );

        // Assert
        Assert.That( onSuccessCalled, Is.False, "The success event shouldn't be called twice each time we add a new listener for the same asset!" );
        Assert.That( callbackCalled, Is.True );

        // TearDown
        lazyTextureObserver.Dispose();
    }

    [Test]
    public void CallListenersWhenTheyAreAddedAfterTextureIs_MultipleSubscriptions()
    {
        // Arrange
        var textureLoader = new TextureLoader_Mock();

        bool onSuccessCalled = false;

        textureLoader.OnSuccess += (x) => onSuccessCalled = true;
        var lazyTextureObserver = new LazyTextureObserver(textureLoader);

        lazyTextureObserver.RefreshWithUri("white");
        lazyTextureObserver.AddListener( (x) => { } );

        Assert.That( onSuccessCalled, Is.True );

        onSuccessCalled = false;

        int newListenerCalledCount  = 0;

        // Act
        void listener(Texture2D x) { newListenerCalledCount++; }
        lazyTextureObserver.AddListener( listener );
        lazyTextureObserver.AddListener( listener );
        lazyTextureObserver.AddListener( listener );

        // Assert
        Assert.That( onSuccessCalled, Is.False, "The success event shouldn't be called twice each time we add a new listener for the same asset!" );
        Assert.AreEqual(3, newListenerCalledCount);

        // TearDown
        lazyTextureObserver.Dispose();
    }

    [Test]
    public void CallListenersWhenTextureIsChangedMultipleTimes()
    {
        // Arrange
        var textureLoader = new TextureLoader_Mock();
        var lazyTextureObserver = new LazyTextureObserver(textureLoader);

        Texture2D lastTexture = null;
        int callCount = 0;

        lazyTextureObserver.AddListener( (x) =>
        {
            lastTexture = x;
            callCount++;
        } );

        lazyTextureObserver.RefreshWithUri("white");
        lazyTextureObserver.RefreshWithTexture(Texture2D.redTexture);
        lazyTextureObserver.RefreshWithTexture(Texture2D.grayTexture);
        lazyTextureObserver.RefreshWithUri("black");

        Assert.That( callCount, Is.EqualTo(4) );
        Assert.That( lastTexture, Is.EqualTo(Texture2D.blackTexture) );

        // TearDown
        lazyTextureObserver.Dispose();
    }

    [Test]
    public void UnloadTextureWhenAllListenersAreRemoved()
    {
        // Arrange
        var textureLoader = Substitute.For<ITextureLoader>();
        var lazyTextureObserver = new LazyTextureObserver(textureLoader);

        System.Action<Texture2D> listener1 = (x) => { };
        System.Action<Texture2D> listener2 = (x) => { };
        System.Action<Texture2D> listener3 = (x) => { };

        // Act
        lazyTextureObserver.RefreshWithUri("white");
        lazyTextureObserver.AddListener( listener1 );
        lazyTextureObserver.AddListener( listener2 );
        lazyTextureObserver.AddListener( listener3 );
        textureLoader.Received().Load(Arg.Is("white"));

        lazyTextureObserver.RemoveListener( listener1 );
        textureLoader.DidNotReceive().Unload();
        lazyTextureObserver.RemoveListener( listener2 );
        textureLoader.DidNotReceive().Unload();
        lazyTextureObserver.RemoveListener( listener3 );

        // Assert
        textureLoader.Received().Unload();

        // TearDown
        lazyTextureObserver.Dispose();
    }

    [Test]
    public void ForgetTextureWhenDisposed()
    {
        // Arrange
        var textureLoader = Substitute.For<ITextureLoader>();
        var lazyTextureObserver = new LazyTextureObserver(textureLoader);

        // Act
        lazyTextureObserver.RefreshWithUri("white");
        lazyTextureObserver.AddListener( (x) => { } );
        lazyTextureObserver.AddListener( (x) => { } );
        lazyTextureObserver.AddListener( (x) => { } );
        lazyTextureObserver.Dispose();

        // Assert
        textureLoader.Received().Unload();

        // TearDown
        lazyTextureObserver.Dispose();
    }

    [Test]
    public void BehaveCorrectlyWhenLoadingFails()
    {
        // Arrange
        var textureLoader = new TextureLoader_Mock();
        var lazyTextureObserver = new LazyTextureObserver(textureLoader);

        bool successCalled = false;
        bool failCalled = false;

        textureLoader.OnSuccess += (x) => successCalled = true;
        textureLoader.OnFail += error => failCalled = true;

        // Act
        lazyTextureObserver.RefreshWithUri("failed url");
        lazyTextureObserver.AddListener( (x) => { } );
        lazyTextureObserver.Dispose();

        // Assert
        Assert.That( successCalled, Is.False );
        Assert.That( failCalled, Is.True );

        // TearDown
        lazyTextureObserver.Dispose();
    }

    [Test]
    public void BehaveCorrectlyWhenInvalidArgumentsArePassed()
    {
        // Arrange
        var textureLoader = new TextureLoader_Mock();
        var lazyTextureObserver = new LazyTextureObserver(textureLoader);

        // Act
        lazyTextureObserver.RefreshWithUri(null);
        lazyTextureObserver.RefreshWithTexture(null);

        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => lazyTextureObserver.AddListener(null));
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => lazyTextureObserver.RemoveListener(null));

        // TearDown
        lazyTextureObserver.Dispose();
    }
}