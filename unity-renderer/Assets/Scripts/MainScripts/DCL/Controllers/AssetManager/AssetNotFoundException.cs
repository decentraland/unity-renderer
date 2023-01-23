using System;

namespace MainScripts.DCL.Controllers.AssetManager
{
    public class AssetNotFoundException : Exception
    {
        private readonly AssetSource source;
        private readonly string assetId;

        public AssetNotFoundException(AssetSource source, string assetId)
        {
            this.source = source;
            this.assetId = assetId;
        }

        public override string Message => $"Asset \"{assetId}\" not found in the source \"{source}\"";
    }
}
