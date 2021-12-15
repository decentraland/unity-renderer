using System;

namespace DCL
{
    public class AssetPromise_PrefetchGLTF : AssetPromise_GLTF
    {
        public AssetPromise_PrefetchGLTF(ContentProvider provider, string url, string hash) : base(provider, url, hash) { settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE; }

        protected override void OnBeforeLoadOrReuse() { asset.container.name = "GLTF: " + fileName; }

        void ParseGLTFWebRequestedFile(ref string requestedFileName) { provider.TryGetContentsUrl(assetDirectoryPath + requestedFileName, out requestedFileName); }

        protected override void OnReuse(Action OnSuccess) { asset.Show(false, OnSuccess); }

        protected override bool AddToLibrary()
        {
            library.Add(asset);
            return true;
        }

        internal override void Load()
        {
            if (!library.Contains(GetId()))
            {
                base.Load();
            }
            else
            {
                CallAndClearEvents(false, new Exception("The asset already exists in the library"));
            }
        }

        protected override void OnCancelLoading() { base.OnCancelLoading(); }

        protected override Asset_GLTF GetAsset(object id) { return library.Get(id); }
    }
}