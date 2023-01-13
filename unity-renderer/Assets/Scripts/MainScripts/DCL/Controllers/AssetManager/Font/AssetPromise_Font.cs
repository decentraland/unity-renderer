using Cysharp.Threading.Tasks;
using DCL.Providers;
using MainScripts.DCL.Controllers.AssetManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_Font : AssetPromise_WithUrl<Asset_Font>
    {
        private const string RESOURCE_FONT_FOLDER = "Fonts & Materials";
        private const string DEFAULT_SANS_SERIF_HEAVY = "Inter-Heavy SDF";
        private const string DEFAULT_SANS_SERIF_BOLD = "Inter-Bold SDF";
        private const string DEFAULT_SANS_SERIF_SEMIBOLD = "Inter-SemiBold SDF";
        private const string DEFAULT_SANS_SERIF = "Inter-Regular SDF";

        private readonly Dictionary<string, string> fontsMapping = new Dictionary<string, string>()
        {
            { "builtin:SF-UI-Text-Regular SDF", DEFAULT_SANS_SERIF },
            { "builtin:SF-UI-Text-Heavy SDF", DEFAULT_SANS_SERIF_HEAVY },
            { "builtin:SF-UI-Text-Semibold SDF", DEFAULT_SANS_SERIF_SEMIBOLD },
            { "builtin:LiberationSans SDF", "LiberationSans SDF" },
            { "SansSerif", DEFAULT_SANS_SERIF },
            { "SansSerif_Heavy", DEFAULT_SANS_SERIF_HEAVY },
            { "SansSerif_Bold", DEFAULT_SANS_SERIF_BOLD },
            { "SansSerif_SemiBold", DEFAULT_SANS_SERIF_SEMIBOLD },
        };

        private string src;
        private Coroutine fontCoroutine;

        private Service<IAssetBundleResolver> assetBundleResolver;
        private CancellationTokenSource cancellationTokenSource;
        private readonly AssetSource permittedSources;

        private readonly bool fetchingEcsFonts;

        public AssetPromise_Font(string src, string baseURL = "", string hash = "", bool fetchingECSFonts = true , AssetSource permittedSources = AssetSource.WEB) : base(baseURL, hash)
        {
            this.permittedSources = permittedSources;
            this.src = src;
            this.fetchingEcsFonts = fetchingECSFonts;
        }

        protected override void OnAfterLoadOrReuse() { }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnCancelLoading()
        {
            CoroutineStarter.Stop(fontCoroutine);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            CoroutineStarter.Stop(fontCoroutine);
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            if (fetchingEcsFonts)
            {
                if (fontsMapping.TryGetValue(src, out string fontResourceName))
                    fontCoroutine = CoroutineStarter.Start(GetFontFromResources(OnSuccess, OnFail, fontResourceName));
                else
                    OnFail?.Invoke(new Exception("Font doesn't correspond with any know font"));
            }
            else
            {
                if (cancellationTokenSource != null)
                    return;

                cancellationTokenSource = new CancellationTokenSource();
                LoadFonts(OnSuccess, OnFail, cancellationTokenSource.Token).Forget();
            }

        }

        private async UniTask LoadFonts(Action onSuccess, Action<Exception> onFail, CancellationToken cancellationToken)
        {
            try
            {
                var assetBundle = await assetBundleResolver.Ref.GetAssetBundleAsync(permittedSources, contentUrl, hash, cancellationToken);

                var fallbackFontAssets = TMP_Settings.fallbackFontAssets;

                if (fallbackFontAssets == null)
                {
                    fallbackFontAssets = new List<TMP_FontAsset>();
                }

                var fonts = assetBundle.LoadAllAssets<TMP_FontAsset>();
                fallbackFontAssets.AddRange(fonts);

                TMP_Settings.fallbackFontAssets = fallbackFontAssets;

                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                onFail?.Invoke(e);
            }
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
            {
                return false;
            }

            asset = library.Get(asset.id);
            return true;
        }

        public override object GetId()
        {
            return src;
        }

        IEnumerator GetFontFromResources(Action OnSuccess, Action<Exception> OnFail, string fontResourceName)
        {
            ResourceRequest request = Resources.LoadAsync($"{RESOURCE_FONT_FOLDER}/{fontResourceName}",
                typeof(TMP_FontAsset));

            yield return request;

            if (request.asset != null)
            {
                asset.font = request.asset as TMP_FontAsset;
                OnSuccess?.Invoke();
            }
            else
            {
                string message = $"couldn't fetch font from resources {fontResourceName}";
                Debug.Log(message);
                OnFail?.Invoke(new Exception(message));
            }
        }
    }
}
