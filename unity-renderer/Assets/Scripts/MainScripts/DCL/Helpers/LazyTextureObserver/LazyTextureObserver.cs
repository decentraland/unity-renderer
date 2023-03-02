using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Helpers
{
    public interface ILazyTextureObserver
    {
        /// <summary>
        /// Adds a listener for this LazyTextureObserver.
        ///
        /// The listener will be called when anyone reloads the texture by calling any of the refresh methods.
        /// Also, calling this method will ensure the texture starts loading lazily if it was not loaded yet.
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(Action<Texture2D> listener);

        /// <summary>
        /// Remove the given listener. If no listeners want this texture anymore, it will be unloaded.
        /// 
        /// Note that as this method uses the AssetPromiseKeeper_Texture service,
        /// if any other systems use the same texture it will not be unloaded.
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(Action<Texture2D> listener);

        /// <summary>
        /// Set the texture to be loaded from the given URI when any listener is subscribed.
        /// If we already have listeners, the loading will start immediately.
        ///
        /// The uri method has precedence over the texture method.
        /// </summary>
        /// <param name="uri"></param>
        void RefreshWithUri(string uri);

        /// <summary>
        /// Sets the listened texture as the passed texture object.
        ///
        /// If we already have listeners, they will be called immediately with the new texture.
        /// New listeners will receive the new texture.
        ///
        /// The uri method has precedence over the texture method.
        /// </summary>
        /// <param name="texture"></param>
        void RefreshWithTexture(Texture2D texture);
    }

    /// <summary>
    /// The LazyTextureObserver class is a reactive texture loader wrapper.
    /// <br/> <br/>
    /// This class is useful for cases where a texture needs to be used, but it has to be lazy loaded,
    /// and can be changed by an external party.
    /// <br/> <br/>
    /// <ul>
    /// <li>Calling <b>RefreshWithUri</b> and <b>RefreshWithTexture</b> will only load the texture if someone is registered to use it.</li><br/>
    /// <li>If <b>AddListener</b> is called and the texture is not loaded, it will start being loaded.</li><br/>
    /// <li>If <b>RemoveListener</b> is called and no more listeners need this image, the image will be unloaded.</li><br/>
    /// </ul>
    /// All existing listeners will be keep informed of <b>RefreshWithX</b> calls to update the texture if it changes.
    /// </summary>
    public class LazyTextureObserver : ILazyTextureObserver
    {
        enum State
        {
            NONE,
            IN_PROGRESS,
            LOADED
        }

        private State state = State.NONE;
        private event Action<Texture2D> OnLoaded;
        private event Action<Exception> OnFail;

        private HashSet<Action<Texture2D>> subscriptions = new HashSet<Action<Texture2D>>();

        private Texture2D texture;
        private string uri;
        private ITextureLoader textureLoader;

        public LazyTextureObserver () : this(new TextureLoader()) { }

        internal LazyTextureObserver (ITextureLoader textureLoader)
        {
            this.textureLoader = textureLoader;

            textureLoader.OnSuccess += x =>
            {
                state = State.LOADED;
                OnLoaded?.Invoke(x);
            };

            textureLoader.OnFail += error =>
            {
                state = State.NONE;
                uri = null;
                OnFail?.Invoke(error);
            };
        }

        public void AddListener(Action<Texture2D> listener)
        {
            Assert.IsNotNull(listener, "Listener can't be null!");

            if (!subscriptions.Contains(listener))
            {
                subscriptions.Add(listener);
                this.OnLoaded += listener;
                this.OnFail += e => listener.Invoke(null);
            }

            // First, check if we did set a texture directly and return it if so.
            if ( texture != null )
            {
                listener.Invoke(texture);
                return;
            }

            // If not, we try to load the texture if not already loaded.
            if ( state == State.NONE )
            {
                TryToLoadAndDispatch();
                return;
            }

            // From now on we assume that the promise exists.
            // --
            // It must be in progress or available by now. If its available we just return it for this listener only.
            // If not, the texture promise flow will call all the listeners when it finishes.
            if ( state == State.LOADED )
            {
                listener.Invoke(textureLoader.GetTexture());
            }
        }

        public void RemoveListener(Action<Texture2D> listener)
        {
            Assert.IsNotNull(listener, "Listener can't be null!");

            // Not using assert here because this case is more probable, I want to fail silently in this case.
            if (!subscriptions.Contains(listener))
                return;

            this.OnLoaded -= listener;
            subscriptions.Remove(listener);

            if ( subscriptions.Count == 0 )
            {
                textureLoader.Unload();
                state = State.NONE;
            }
        }

        public void RefreshWithUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return;

            this.uri = uri;
            this.texture = null;

            if ( subscriptions.Count > 0 )
                TryToLoadAndDispatch();
        }

        public void RefreshWithTexture(Texture2D texture)
        {
            if (texture == null || texture == this.texture)
                return;

            textureLoader.Unload();
            state = State.NONE;

            this.uri = null;
            this.texture = texture;

            if ( subscriptions.Count > 0 )
                TryToLoadAndDispatch();
        }

        private void TryToLoadAndDispatch()
        {
            if (DispatchTextureByInjectedReference())
                return;

            DispatchLoadedTexture();
        }

        private bool DispatchTextureByInjectedReference()
        {
            if (texture == null)
                return false;

            state = State.LOADED;
            OnLoaded?.Invoke(texture);
            return true;
        }

        private bool DispatchLoadedTexture()
        {
            if (string.IsNullOrEmpty(uri))
                return false;

            state = State.IN_PROGRESS;
            textureLoader.Load(uri);
            return true;
        }

        public void Dispose()
        {
            state = State.NONE;
            textureLoader.Unload();
            subscriptions.Clear();
            OnLoaded = null;
        }

        public static LazyTextureObserver CreateWithUri(string uri)
        {
            var result = new LazyTextureObserver();
            result.RefreshWithUri(uri);
            return result;
        }

        public static LazyTextureObserver CreateWithTexture(Texture2D texture)
        {
            var result = new LazyTextureObserver();
            result.RefreshWithTexture(texture);
            return result;
        }
    }
}